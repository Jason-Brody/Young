using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Young.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DataBindingAttribute:Attribute
    {
        public string TableName { get; set; }

        public DataBindingAttribute(string TableName, string IdColumnName)
        {
            this.TableName = TableName;
            this.IdColumnName = IdColumnName;
        }

        public DataBindingAttribute() { this.IdColumnName = "Id"; }

        public DataBindingAttribute(string TableName)
        {
            this.TableName = TableName;
            this.IdColumnName = "Id";
        }

        public string IdColumnName { get; set; }
    }
}
