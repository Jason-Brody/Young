using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Young.Data.Attributes
{
    public delegate object ColumnBindingConvert(object value);

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnBindingAttribute:OrderAttribute
    {
        public Type Target { get; set; }

        public string MethodName { get; set; }

        public DataDirectory Directory { get; set; }

        public string GroupIdColumnName { get; set; }

        public string[] ColNames { get; set; }

        public ColumnBindingAttribute(string[] Cols,string MethodName,Type Target,string GroupIdColumnName)
        {
            this.ColNames = Cols;
            this.MethodName = MethodName;
            this.Target = Target;
            this.GroupIdColumnName = GroupIdColumnName;
            Directory = DataDirectory.Input;
        }

        public ColumnBindingAttribute(string Col):this(new string[] { Col},null,null,"GroupId")
        {

        }

        public ColumnBindingAttribute(string[] Cols):this(Cols,null,null,"GroupId")
        {

        }

        public ColumnBindingAttribute():this(null,null,null,"GroupId")
        {

        }
    }
}
