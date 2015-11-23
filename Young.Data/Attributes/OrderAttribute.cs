using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Young.Data.Attributes
{
    public abstract class OrderAttribute : Attribute
    {
        public int Order { get; set; }

        public OrderAttribute() { }

        public OrderAttribute(int Order)
        {
            this.Order = Order;
        }
    }
}
