using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Young.Data.Attributes
{
    public class MethodBindingAttribute : OrderAttribute
    {
        public string[] ParameterNames { get; set; }
        public MethodBindingAttribute() { }


    }
}
