using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Young.Data.Attributes
{
    public class DataGroupAttribute:Attribute
    {
        public int Group { get; set; }
    }
}
