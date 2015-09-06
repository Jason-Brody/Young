using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Young.Data.Attributes;

namespace Young.Data
{
    public class DataBindingConfig
    {
        public string Name { get; set; }

        public AttributeTargets Target { get; set; }

        public OrderAttribute Attribute { get; set; }

        public string AttributeTypeName { get; set; }
    }
}
