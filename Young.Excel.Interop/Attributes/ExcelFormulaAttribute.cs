using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Young.Excel.Interop.Attributes
{
    /// <summary>
    ///  Set The Formula of Cell
    ///  if formual like column D = column C + column B
    ///  then set Formual = "=C2+B2"
    ///  why choose 2 because the first row should be header
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ExcelFormulaAttribute : Attribute
    {
        public string Formula { get; set; }

        public ExcelFormulaAttribute(string Formula)
        {
            this.Formula = Formula;
        }
    }
}
