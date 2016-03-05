using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Young.Excel.Interop.Attributes;
using Ex = Microsoft.Office.Interop.Excel;

namespace Young.Excel.Interop.Extensions
{
    public static class Export
    {
        public static Ex.Application exApp = null;
        public static Ex.Workbook exWb = null;

        /// <summary>
        ///  If set fileName, the excel will save and quit
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Data"></param>
        /// <param name="sheetName"></param>
        /// <param name="otherAction"></param>
        /// <param name="fileName"></param>
        public static void ExportToExcel<T>(this IEnumerable<T> Data, string sheetName, Action<object> otherAction, string fileName = null) where T : class
        {
            List<PropertyInfo> props = typeof(T).GetProperties().Where(p => (p.PropertyType == typeof(string) || p.PropertyType.IsPrimitive) && p.GetCustomAttribute<IgnoreAttribute>() == null).ToList();
            if (exApp == null)
                exApp = new Ex.Application();

            exApp.Visible = true;

            Ex.Workbooks wbs = exApp.Workbooks;

            if (exWb == null)
            {
                exWb = wbs.Add();
            }

            var sheet = exWb.Worksheets.Add() as Ex.Worksheet;

            sheet.Name = sheetName;

            Ex.Range range = sheet.Range[sheet.Cells[1, 1], sheet.Cells[1 + Data.Count(), props.Count]];



            object[,] datas = new object[Data.Count() + 1, props.Count];

            List<Tuple<string, int>> formualList = new List<Tuple<string, int>>();


            for (int i = 0; i < props.Count; i++)
            {
                var attr = props[i].GetCustomAttribute<DisplayAttribute>();
                if (attr != null)
                    datas[0, i] = attr.Name;
                else
                    datas[0, i] = props[i].Name;

                var formulaAttr = props[i].GetCustomAttribute<ExcelFormulaAttribute>();
                if (formulaAttr != null)
                {
                    Tuple<string, int> item = new Tuple<string, int>(formulaAttr.Formula, i + 1);
                    formualList.Add(item);
                }
            }

            for (int i = 0; i < Data.Count(); i++)
            {
                for (int j = 0; j < props.Count; j++)
                {
                    var val = props[j].GetValue(Data.ElementAt(i));
                    datas[i + 1, j] = val;
                }
            }



            range.Value = datas;


            for (int i = 0; i < props.Count; i++)
            {
                range = sheet.Cells[1, i + 1];

                var attr = props[i].GetCustomAttribute<ExcelHeaderStyleAttribute>();
                if (attr != null)
                {
                    range.Interior.Color = attr.BackgroundColor;
                    range.EntireColumn.NumberFormat = attr.NumberFormat;
                    range.ColumnWidth = attr.Width;
                    range.Font.Bold = attr.IsFontBold;
                    range.Font.Size = attr.FontSize;
                    range.WrapText = attr.IsTextWrap;
                    range.HorizontalAlignment = attr.HAlign;
                    range.VerticalAlignment = attr.VAlign;
                }
            }



            if (formualList.Count > 0)
            {
                foreach (var item in formualList)
                {
                    range = sheet.Cells[2, item.Item2];
                    range.Formula = item.Item1;
                    range.AutoFill(sheet.Range[sheet.Cells[2, item.Item2], sheet.Cells[Data.Count() + 1, item.Item2]], Ex.XlAutoFillType.xlFillDefault);
                }

            }

            if (otherAction != null)
                otherAction(sheet);

            range = sheet.UsedRange;

            range.Borders[Ex.XlBordersIndex.xlEdgeLeft].LineStyle = Ex.XlLineStyle.xlContinuous;
            range.Borders[Ex.XlBordersIndex.xlEdgeTop].LineStyle = Ex.XlLineStyle.xlContinuous;
            range.Borders[Ex.XlBordersIndex.xlEdgeRight].LineStyle = Ex.XlLineStyle.xlContinuous;
            range.Borders[Ex.XlBordersIndex.xlEdgeBottom].LineStyle = Ex.XlLineStyle.xlContinuous;
            range.Borders.Color = ConsoleColor.Black;


            range = sheet.Cells[1, 1] as Ex.Range;
            range.Select();


            if (!string.IsNullOrEmpty(fileName))
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
                exWb.SaveAs(fileName);
                exWb.Close();
                exApp.Quit();

                Marshal.ReleaseComObject(range);
                range = null;
                Marshal.ReleaseComObject(sheet);
                sheet = null;
                Marshal.ReleaseComObject(exWb);
                exWb = null;
                Marshal.ReleaseComObject(wbs);
                wbs = null;
                Marshal.ReleaseComObject(exApp);
                exApp = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }






        }

        public static void ExportToExcel<T>(this IEnumerable<T> Data, string fileName, string sheetName, Action<object> otherAction) where T : class
        {
            List<PropertyInfo> props = typeof(T).GetProperties().Where(p => (p.PropertyType == typeof(string) || p.PropertyType.IsPrimitive)).ToList();
            Ex.Application app = new Ex.Application();
            app.Visible = true;

            var wbs = app.Workbooks;
            var wb = wbs.Add();

            var sheet = wb.ActiveSheet as Ex.Worksheet;

            sheet.Name = sheetName;

            Ex.Range range = sheet.Range[sheet.Cells[1, 1], sheet.Cells[1 + Data.Count(), props.Count]];

            try
            {

                object[,] datas = new object[Data.Count() + 1, props.Count];

                List<Tuple<string, int>> formualList = new List<Tuple<string, int>>();


                for (int i = 0; i < props.Count; i++)
                {
                    var attr = props[i].GetCustomAttribute<DisplayAttribute>();
                    if (attr != null)
                        datas[0, i] = attr.Name;
                    else
                        datas[0, i] = props[i].Name;

                    var formulaAttr = props[i].GetCustomAttribute<ExcelFormulaAttribute>();
                    if (formulaAttr != null)
                    {
                        Tuple<string, int> item = new Tuple<string, int>(formulaAttr.Formula, i + 1);
                        formualList.Add(item);
                    }
                }

                for (int i = 0; i < Data.Count(); i++)
                {
                    for (int j = 0; j < props.Count; j++)
                    {
                        var val = props[j].GetValue(Data.ElementAt(i));
                        datas[i + 1, j] = val;
                    }
                }



                range.Value = datas;


                for (int i = 0; i < props.Count; i++)
                {
                    range = sheet.Cells[1, i + 1];

                    var attr = props[i].GetCustomAttribute<ExcelHeaderStyleAttribute>();
                    if (attr != null)
                    {
                        range.Interior.Color = attr.BackgroundColor;
                        range.EntireColumn.NumberFormat = attr.NumberFormat;
                        range.ColumnWidth = attr.Width;
                        range.Font.Bold = attr.IsFontBold;
                        range.Font.Size = attr.FontSize;
                        range.WrapText = attr.IsTextWrap;
                        range.HorizontalAlignment = attr.HAlign;
                        range.VerticalAlignment = attr.VAlign;
                    }
                }



                if (formualList.Count > 0)
                {
                    foreach (var item in formualList)
                    {
                        range = sheet.Cells[2, item.Item2];
                        range.Formula = item.Item1;
                        range.AutoFill(sheet.Range[sheet.Cells[2, item.Item2], sheet.Cells[Data.Count() + 1, item.Item2]], Ex.XlAutoFillType.xlFillDefault);
                    }

                }

                if (otherAction != null)
                    otherAction(sheet);

                range = sheet.UsedRange;

                range.Borders[Ex.XlBordersIndex.xlEdgeLeft].LineStyle = Ex.XlLineStyle.xlContinuous;
                range.Borders[Ex.XlBordersIndex.xlEdgeTop].LineStyle = Ex.XlLineStyle.xlContinuous;
                range.Borders[Ex.XlBordersIndex.xlEdgeRight].LineStyle = Ex.XlLineStyle.xlContinuous;
                range.Borders[Ex.XlBordersIndex.xlEdgeBottom].LineStyle = Ex.XlLineStyle.xlContinuous;
                range.Borders.Color = ConsoleColor.Black;


                range = sheet.Cells[1, 1] as Ex.Range;
                range.Select();


                if (File.Exists(fileName))
                    File.Delete(fileName);
                wb.SaveAs(fileName);


                wb.Close();
                app.Quit();

            }

            catch (Exception ex)
            {
                throw (ex);
            }

            finally
            {
                Marshal.ReleaseComObject(range);
                range = null;
                Marshal.ReleaseComObject(sheet);
                sheet = null;
                Marshal.ReleaseComObject(wb);
                wb = null;
                Marshal.ReleaseComObject(wbs);
                wbs = null;
                Marshal.ReleaseComObject(app);
                app = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }
    }
}
