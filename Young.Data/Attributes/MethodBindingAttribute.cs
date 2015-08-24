using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Young.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MethodBindingAttribute : OrderAttribute
    {
        public string[] ParameterNames { get; set; }
        public MethodBindingAttribute() { }


    }
}
