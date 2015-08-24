using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Young.Data.Attributes
{
    public delegate object ConvertMethod(DataRow[] drs);


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MultiColumnBindingAttribute : OrderAttribute
    {
        public string MethodName { get; set; }

        public string[] ColNames { get; set; }

        public string GroupIdColumnName { get; set; }

        public MultiColumnBindingAttribute(string GroupIdColumnName, string[] Cols, string MethodName)
        {
            this.ColNames = Cols;
            this.MethodName = MethodName;
            this.Directory = DataDirectory.Input;
            this.GroupIdColumnName = GroupIdColumnName;
        }

        public MultiColumnBindingAttribute(string[] Cols,string MethodName):this("GroupId",Cols,MethodName)
        {

        }

        public DataDirectory Directory { get; set; }


    }


   
}
