using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Young.Excel.Interop.Attributes
{
    [AttributeUsage(AttributeTargets.Property,AllowMultiple =false)]
    public class IgnoreAttribute:Attribute
    {
        public IgnoreAttribute() { }
    }
}
