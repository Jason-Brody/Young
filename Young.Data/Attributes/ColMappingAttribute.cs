using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Young.Data.Attributes
{


    [AttributeUsage(AttributeTargets.Property,AllowMultiple =true)]
    public class ColMappingAttribute:Attribute
    {
        public ColMappingAttribute(string Name)
        {
            this.Name = Name;
        }
       
        public string Name { get; set; }

 

       

        
    }
}
