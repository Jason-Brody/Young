using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Young.Data
{
    public class ExcelHelper
    {
        private static object _lockObj = new object();

        private List<string> _expectSheets;

        private SpreadsheetDocument _doc;

        private SharedStringTablePart _shareStringPart;

        private WorkbookPart _wbPart;

        private string _excelFilePath;

        private static ExcelHelper _instance;
        public static ExcelHelper Current
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObj)
                    {
                        if (_instance == null)
                            _instance = new ExcelHelper();
                    }
                }
                return _instance;
            }
        }
        private ExcelHelper() {  }

        public ExcelHelper Open(string ExcelFile,bool IsEditable=false)
        {
            _expectSheets = new List<string>();
            OpenSettings os = new OpenSettings();
            this._excelFilePath = ExcelFile;
            _doc = SpreadsheetDocument.Open(_excelFilePath, IsEditable);
            _wbPart = _doc.WorkbookPart;
            _shareStringPart = _wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
            return _instance;
        }

        public void Close()
        {
            if (_doc != null)
            {
                _doc.Close();
                _expectSheets = null;
                _wbPart = null;
                _shareStringPart = null;
            }
                
        }

        public ExcelHelper Create(string ExcelFile)
        {
            return _instance;
        }

        public ExcelHelper WithoutSheets(params string[] SheetNames)
        {
            foreach (var name in SheetNames)
                _expectSheets.Add(name.ToLower());
            return _instance;
        }

        public DataSet ReadAll()
        {
            DataSet ds = new DataSet();
            var sheets = _wbPart.Workbook.Descendants<Sheet>();
            foreach(var sheet in sheets)
            {
                if(!_expectSheets.Contains(sheet.Name.Value.ToLower()))
                {
                    DataTable dt = read(sheet);
                    ds.Tables.Add(dt);
                }
                    
            }
            return ds;
        }

        public DataTable Read(string sheetName)
        {         
            return Read(sheetName,null,null);
        }


        /// <summary>
        /// The method will read part of sheet depend on range start and end
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="HeaderRow"></param>
        /// <returns></returns>
        public DataTable Read(string sheetName,Range start,Range end,int HeaderRow = 0)
        {
            Sheet sheet = _wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault();
            if (sheet == null)
            {
                throw new ArgumentNullException(sheetName);
            }

            return read(sheet, start, end,HeaderRow);
        }

        private void createColumns(DataTable dt,Row row)
        {
            var cells = row.Descendants<Cell>();
            foreach (var cell in cells)
            {
                string value = getCellValue(cell);
                DataColumn dc = new DataColumn(value);
                dt.Columns.Add(dc);
            }
        }

        private DataTable read(Sheet sheet, Range start, Range end,int tableHeaderRow = 0)
        {
            DataTable dt = new DataTable(sheet.Name);
            WorksheetPart wsPart = _wbPart.GetPartById(sheet.Id) as WorksheetPart;
            var rows = wsPart.Worksheet.Descendants<Row>();
            var columnRow = rows.ElementAt(tableHeaderRow);
            createColumns(dt, columnRow);
            int columnCount = dt.Columns.Count;
            int rowCount = rows.Count();

            //set initial for both start and end if they are null
            if (start == null)
                start = new Range() { Row = tableHeaderRow + 2, Column = 1};
            if (end == null)
                end = new Range() { Row = rowCount, Column = columnCount};


            if (end.Row > rowCount)
                end.Row = rowCount;
            if (end.Column > columnCount)
                end.Column = columnCount;


            start.Row = start.Row < 2 ? 1 : start.Row - 1;
            start.Column = start.Column < 1 ? 0 : start.Column - 1;




            //remove first column
            if(start.Column>0)
            {
                for(int i = 0;i<start.Column;i++)
                {
                    dt.Columns.RemoveAt(0);
                }
            }
            //remove last column
            if(end.Column < columnCount)
            {
                for(int i=0;i<columnCount - end.Column;i++)
                {
                    dt.Columns.RemoveAt(dt.Columns.Count-1);
                }
            }

            if(start.Row < end.Row && start.Column < end.Column)
            {
                for(int i = start.Row;i<end.Row;i++)
                {
                    DataRow dr = dt.NewRow();
                    var row = rows.ElementAt<Row>(i);
                    var cells = row.Descendants<Cell>();
                    int cellCount = cells.Count();
                    for(int j = start.Column;j<end.Column;j++)
                    {
                        string value = "";
                        if (cellCount > j)
                            value = getCellValue(cells.ElementAt<Cell>(j));
                        dr[j-start.Column] = value;
                    }
                    dt.Rows.Add(dr);
                }
            }

            return dt;

        }

        private DataTable read(Sheet sheet)
        {
            
            WorksheetPart wsPart = _wbPart.GetPartById(sheet.Id) as WorksheetPart;
            DataTable dt = new DataTable(sheet.Name);

            bool isTableCreate = false;
            int columnCount = 0;
            foreach(var row in wsPart.Worksheet.Descendants<Row>())
            {
                if(!isTableCreate)
                {
                    var cells = row.Descendants<Cell>();
                    columnCount = cells.Count();
                    foreach (var cell in cells)
                    {
                        string value = getCellValue(cell);
                        DataColumn dc = new DataColumn(value);
                        dt.Columns.Add(dc);
                    }
                    isTableCreate = true;
                    
                }
                else
                {
                    DataRow dr = dt.NewRow();
                    var cells = row.Descendants<Cell>();
                    int count = cells.Count();
                    for (int i = 0; i < columnCount;i++ )
                    {
                        string value = "";
                        if (count > i)
                            value = getCellValue(cells.ElementAt<Cell>(i));
                        dr[i] = value;
                    }
                    dt.Rows.Add(dr);
                        
                }
            }
            
            return dt;
            
        }

        private DataTable createTable(List<List<string>> datas,string tableName)
        {
            var header = datas.First();
            DataTable dt = new DataTable(tableName);
            foreach(var str in header)
            {
                DataColumn dc = new DataColumn(str);
                dt.Columns.Add(dc);
            }
            for (int i = 1; i < datas.Count; i++)
            {
                DataRow dr = dt.NewRow();
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    dr[j] = datas[i][j];
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public void CleanSheet(string sheetName)
        {

        }

        private static string getColumnName(int columnIndex)
        {
            int dividend = columnIndex;
            string columnName = String.Empty;
            int modifier;

            while (dividend > 0)
            {
                modifier = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modifier).ToString() + columnName;
                dividend = (int)((dividend - modifier) / 26);
            }

            return columnName;
        }

        private string getCellValue(Cell cell)
        {
            string value = cell.InnerText;
            if(cell.DataType != null)
            {
                switch(cell.DataType.Value)
                {
                    case CellValues.SharedString:
                        
                        if(_shareStringPart != null)
                        {
                            value = _shareStringPart.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
                        }
                        break;
                    case CellValues.Boolean:
                        switch(value)
                        {
                            case "0":
                                value = "FALSE";
                                break;
                            default:
                                value = "TRUE";
                                break;
                        }
                        break;
                }
            }
            return value;
        }
    }
}
