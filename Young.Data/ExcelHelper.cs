using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            _doc = SpreadsheetDocument.Create(ExcelFile, SpreadsheetDocumentType.Workbook);
            _wbPart = _doc.AddWorkbookPart();
            _wbPart.Workbook = new Workbook();
            _doc.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
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

        public DataSet Read(params string[] sheetNames)
        {
            DataSet ds = new DataSet();
            var sheets = _wbPart.Workbook.Descendants<Sheet>();
            foreach (var sheet in sheets)
            {
                if (sheetNames.Contains(sheet.Name.Value.ToLower()))
                {
                    DataTable dt = read(sheet);
                    ds.Tables.Add(dt);
                }

            }
            return ds;
        }

        public DataTable Read(string sheetName)
        {
            Sheet sheet = _wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault();
            if (sheet == null)
            {
                throw new ArgumentNullException(sheetName);
            }
            return read(sheet);
        }


        /// <summary>
        /// The method will read part of sheet depend on range start and end
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="HeaderRow"></param>
        /// <returns></returns>
        public DataTable Read(string sheetName,Range start,Range end,int HeaderRow = 1)
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
            _colNameMapping = new Dictionary<string, string>();
            var cells = row.Descendants<Cell>();

            foreach (var cell in cells)
            {
                string value = getCellValue(cell);
                string colHeader = Regex.Split(cell.CellReference.Value, @"\d+").First();
                _colNameMapping.Add(colHeader, value);
                DataColumn dc = new DataColumn(colHeader);
                dt.Columns.Add(dc);
            }
        }

        private void addRow(DataTable dt,Row row)
        {
            DataRow dr = dt.NewRow();

            foreach (var cell in row.Descendants<Cell>())
            {
                string rowHeader = Regex.Split(cell.CellReference.Value, @"\d+").First();
                if (_colNameMapping.ContainsKey(rowHeader))
                {
                    dr[rowHeader] = getCellValue(cell);
                }

            }

            dt.Rows.Add(dr);
        }

        private void renameColumn(DataTable dt)
        {
            foreach (DataColumn dc in dt.Columns)
            {
                dc.ColumnName = _colNameMapping[dc.ColumnName];
            }
        }

        //private DataTable read(Sheet sheet, Range start, Range end,int tableHeaderRow = 0)
        //{
        //    DataTable dt = new DataTable(sheet.Name);
        //    WorksheetPart wsPart = _wbPart.GetPartById(sheet.Id) as WorksheetPart;
        //    var rows = wsPart.Worksheet.Descendants<Row>();
        //    var columnRow = rows.ElementAt(tableHeaderRow);

        //    createColumns(dt, columnRow);


        //    int columnCount = dt.Columns.Count;
        //    int rowCount = rows.Count();

        //    //set initial for both start and end if they are null
        //    if (start == null)
        //        start = new Range() { Row = tableHeaderRow + 2, Column = 1};
        //    if (end == null)
        //        end = new Range() { Row = rowCount, Column = columnCount};


        //    if (end.Row > rowCount)
        //        end.Row = rowCount;
        //    if (end.Column > columnCount)
        //        end.Column = columnCount;


        //    start.Row = start.Row < 2 ? 1 : start.Row - 1;
        //    start.Column = start.Column < 1 ? 0 : start.Column - 1;




        //    //remove first column
        //    if(start.Column>0)
        //    {
        //        for(int i = 0;i<start.Column;i++)
        //        {
        //            dt.Columns.RemoveAt(0);
        //        }
        //    }
        //    //remove last column
        //    if(end.Column < columnCount)
        //    {
        //        for(int i=0;i<columnCount - end.Column;i++)
        //        {
        //            dt.Columns.RemoveAt(dt.Columns.Count-1);
        //        }
        //    }

        //    if(start.Row < end.Row && start.Column < end.Column)
        //    {
        //        for(int i = start.Row;i<end.Row;i++)
        //        {
        //            DataRow dr = dt.NewRow();
        //            var row = rows.ElementAt<Row>(i);
        //            var cells = row.Descendants<Cell>();
        //            int cellCount = cells.Count();
        //            for(int j = start.Column;j<end.Column;j++)
        //            {
        //                string value = "";
        //                if (cellCount > j)
        //                    value = getCellValue(cells.ElementAt<Cell>(j));
        //                dr[j-start.Column] = value;
        //            } 
        //            dt.Rows.Add(dr);
        //        }
        //    }

        //    return dt;

        //}


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

        private string getColumnName(int columnIndex)
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

        private int getColumnValue(string columnName)
        {
            double value = 0;
            var arrayList = columnName.ToList();
            while (arrayList.Count > 0)
            {
                char first = arrayList.First();
                arrayList.Remove(first);
                value += (Convert.ToInt16(first) - 64) * Math.Pow(26, arrayList.Count);
            }
            return Convert.ToInt16(value);
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

        public void Write(DataTable dt)
        {
            var sheets = _wbPart.Workbook.Descendants<Sheet>();
            int sheetCount = sheets.Count();
            var sheet = sheets.Where(c => c.Name.Value.ToLower() == dt.TableName.ToLower()).FirstOrDefault();
            WorksheetPart wsPart = null;
            if(sheet==null)
            {
                wsPart = _wbPart.AddNewPart<WorksheetPart>();
                wsPart.Worksheet = new Worksheet(new SheetData());
                sheet = new Sheet()
                {
                    Id = _wbPart.GetIdOfPart(wsPart),
                    Name = dt.TableName,
                    SheetId = (uint)(sheetCount + 1)
                };
                _wbPart.Workbook.GetFirstChild<Sheets>().Append(sheet);
            }
            else
            {
                wsPart = _wbPart.GetPartById(sheet.Id) as WorksheetPart;
                wsPart.Worksheet.GetFirstChild<SheetData>().RemoveAllChildren();
            }

            SheetData sheetData = wsPart.Worksheet.GetFirstChild<SheetData>();

            Row headRow = new Row();
            headRow.RowIndex = 1;


            for (int i = 0; i < dt.Columns.Count; i++)
            {
                Cell c = new Cell();
                c.DataType = new EnumValue<CellValues>(CellValues.String);
                c.CellReference = getColumnName(i + 1) + "1";
                c.CellValue = new CellValue(dt.Columns[i].ColumnName);
                headRow.AppendChild(c);
            }
            sheetData.AppendChild(headRow);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Row r = new Row();
                r.RowIndex = (UInt32)i + 2;
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    Cell c = new Cell();
                    c.DataType = new EnumValue<CellValues>(getCellType(dt.Columns[j].DataType));
                    c.CellReference = getColumnName(j + 1) + r.RowIndex.ToString();

                    DataRow dr = dt.Rows[i];

                    if(dr.IsNull(j))
                    {
                        c.CellValue = new CellValue("");
                    }
                    else if (c.DataType.Value == CellValues.Boolean)
                    {
                        string value = bool.Parse(dt.Rows[i][j].ToString()) ? "1" : "0";
                        c.CellValue = new CellValue(value);
                    }
                    else
                    {
                        c.CellValue = new CellValue(dt.Rows[i][j].ToString());
                    }

                    r.Append(c);
                }
                sheetData.Append(r);
            }

            _wbPart.Workbook.Save();
        }

        private  CellValues getCellType(Type dataType)
        {
            if (dataType == typeof(string))
            {
                return CellValues.String;
            }
            else if (dataType == typeof(DateTime))
            {
                return CellValues.String;
            }
            else if (dataType == typeof(Boolean))
            {
                return CellValues.Boolean;
            }
            else
            {
                return CellValues.Number;
            }
        }

        private Dictionary<string, string> _colNameMapping;

        private DataTable read(Sheet sheet)
        {

            WorksheetPart wsPart = _wbPart.GetPartById(sheet.Id) as WorksheetPart;
            DataTable dt = new DataTable(sheet.Name);
            bool isTableCreate = false;
            foreach (var row in wsPart.Worksheet.Descendants<Row>())
            {
                if (!isTableCreate)
                {
                    createColumns(dt, row);
                    isTableCreate = true;
                }
                else
                {
                    addRow(dt, row);
                }
            }

            renameColumn(dt);

            return dt;

        }

        private DataTable read(Sheet sheet, Range start, Range end, int tableHeaderRow = 1)
        {
            DataTable dt = new DataTable(sheet.Name);
            WorksheetPart wsPart = _wbPart.GetPartById(sheet.Id) as WorksheetPart;

            var headerRow = wsPart.Worksheet.Descendants<Row>().Where(r => r.RowIndex == tableHeaderRow).FirstOrDefault();
            if(headerRow != null)
            {
                createColumns(dt, headerRow);
            }

            foreach(var cell in headerRow.Descendants<Cell>())
            {
                
                string colN = Regex.Split(cell.CellReference.Value, @"\d+").First();
                int colV = getColumnValue(colN);

                if(colV <start.Column|| colV>end.Column)
                {
                    if(_colNameMapping.ContainsKey(colN))
                    {
                        _colNameMapping.Remove(colN);
                        dt.Columns.Remove(colN);
                    }
                }
            }

            
            

            if (start.Row == tableHeaderRow)
            {
                start.Row += 1;
            }
                

            var bodyRows = wsPart.Worksheet.Descendants<Row>().Where(r => r.RowIndex >= start.Row && r.RowIndex <= end.Row);

            foreach(var row in bodyRows)
            {
                addRow(dt, row);
            }

            renameColumn(dt);


            return dt;

        }
    }
}
