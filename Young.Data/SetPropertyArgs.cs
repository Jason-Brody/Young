using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Young.Data.Attributes;

namespace Young.Data
{
    public class SetPropertyArgs : EventArgs
    {
        public PropertyInfo Property { get; set; }

        public object Value { get; set; }

        public ColumnBindingAttribute Attribute { get; set; }

        public SetPropertyArgs(PropertyInfo Prop, object value, ColumnBindingAttribute Attribute)
        {
            this.Property = Prop;
            this.Value = value;
            this.Attribute = Attribute;
        }

        public SetPropertyArgs() { }
    }
}
