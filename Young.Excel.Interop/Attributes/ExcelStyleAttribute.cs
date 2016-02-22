using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

namespace Young.Excel.Interop.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ExcelHeaderStyleAttribute : Attribute
    {
        public ExcelHeaderStyleAttribute() : this("General", 16777215, 10.00) { }

        public ExcelHeaderStyleAttribute(string NumberFormat) : this(NumberFormat, 16777215, 10.00) { }

        public ExcelHeaderStyleAttribute(int BackgroundColor) : this("General", BackgroundColor, 10.00) { }

        public ExcelHeaderStyleAttribute(double Width) : this("General", 16777215, Width) { }

        public ExcelHeaderStyleAttribute(int BackgroundColor, double Width) : this("General", BackgroundColor, Width) { }

        public ExcelHeaderStyleAttribute(string NumberFormat, int BackgroundColor, double Width)
        {
            this.NumberFormat = NumberFormat;
            this.BackgroundColor = BackgroundColor;
            this.Width = Width;
        }

        public bool IsTextWrap { get; set; } = true;

        public string NumberFormat { get; set; }

        public int BackgroundColor { get; set; }

        public double Width { get; set; }

        public XlHAlign HAlign { get; set; } = XlHAlign.xlHAlignCenter;

        public XlVAlign VAlign { get; set; } = XlVAlign.xlVAlignBottom;

        public bool IsFontBold { get; set; } = true;

        public int FontSize { get; set; } = 10;


    }
}
