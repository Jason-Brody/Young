using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Young.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Property,AllowMultiple=false)]
    public class BizDataAttribute:Attribute
    {
        public string Name { get; set; }
    }
}
