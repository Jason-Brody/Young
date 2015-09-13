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
        public MemberInfo Member{ get; set; }

        public object Value { get; set; }

        public ColumnBindingAttribute Attribute { get; set; }

        public SetPropertyArgs(MemberInfo Member, object value, ColumnBindingAttribute Attribute)
        {
            this.Member = Member;
            this.Value = value;
            this.Attribute = Attribute;
        }

        public SetPropertyArgs() { }
    }
}
