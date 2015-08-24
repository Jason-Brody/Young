using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Young.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Property,AllowMultiple=true)]
    public class ComplexSampleDataAttribute:DataGroupAttribute
    {
        public string[] Content { get; set; }

        public SampleDataType DataType { get; set; }
    }

    public enum SampleDataType
    {
        Header = 0,
        Body = 1
    }
}
