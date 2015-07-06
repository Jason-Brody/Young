using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Young.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Property,AllowMultiple=false)]
    public class FromTable:Attribute
    {
        public string NickName { get; set; }
    }
}
