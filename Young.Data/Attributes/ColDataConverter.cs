using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Young.Data.Attributes
{

    public interface IDataConverter
    {
        object Convert(object data);
    }

    [AttributeUsage(AttributeTargets.Property,AllowMultiple =false)]
    public abstract class DataConverterMethodAttribute:Attribute
    {
        public abstract IDataConverter GetConverter();
       
    }
}
