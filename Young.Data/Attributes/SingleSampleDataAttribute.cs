using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Young.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SingleSampleDataAttribute:DataGroupAttribute
    {
        public object Value { get; set; }

        public SingleSampleDataAttribute() { }

        public SingleSampleDataAttribute(object Value)
        {
            this.Value = Value;
        }
    }
}
