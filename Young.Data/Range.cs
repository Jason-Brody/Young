using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Young.Data
{
    public class Range
    {
        public int Row { get; set; }

        public int Column { get; set; }

        public Range() { }

        public Range(int Row,int Column)
        {
            this.Row = Row;
            this.Column = Column;
        }
    }
}
