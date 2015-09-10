using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Young.Data.Attributes;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

namespace Young.Data
{
    public class DataBindingConfig
    {

        public string TypeName { get; set; }

        public DataBindingAttribute DataBinding { get; set; }

        public List<BindingConfigDetails> DetailInfoes { get; set; }

        public void Save(string configFile)
        {
            Type[] subTypes = { typeof(ColumnBindingAttribute), typeof(MethodBindingAttribute) };
            XmlSerializer xs = new XmlSerializer(typeof(DataBindingConfig), subTypes);
            using (FileStream fs = new FileStream(configFile, FileMode.Create))
            {
                xs.Serialize(fs, this);
            }
        }

        public static DataBindingConfig ReadFromConfigFile(string configFile)
        {
            Type[] subTypes = { typeof(ColumnBindingAttribute), typeof(MethodBindingAttribute) };
            XmlSerializer xs = new XmlSerializer(typeof(DataBindingConfig),subTypes);
            DataBindingConfig config = null;
            using (FileStream fs = new FileStream(configFile, FileMode.Open))
            {

                config = xs.Deserialize(fs) as DataBindingConfig;
            }
            return config;
        }

        public List<Tuple<MemberInfo, OrderAttribute>> GetInfoes()
        {
            List<Tuple<MemberInfo, OrderAttribute>> myInfoes = new List<Tuple<MemberInfo, OrderAttribute>>();
            Type t = Type.GetType(TypeName);
            if (t == null)
                throw new NullReferenceException(string.Format("Can't find type {0}", TypeName));
            foreach (var detail in DetailInfoes)
            {
                MemberInfo m = null;
                if (detail.Target == BindingType.Property)
                {
                    m = t.GetProperty(detail.MemberName);
                    if (m == null)
                        throw new NullReferenceException(string.Format("Can't find Property {0} in type {1}", detail.MemberName, TypeName));
                }
                else
                {
                    m = t.GetMethod(detail.MemberName);
                    if(m == null)
                        throw new NullReferenceException(string.Format("Can't find Memthod {0} in type {1}", detail.MemberName, TypeName));
                }
                Tuple<MemberInfo, OrderAttribute> info = new Tuple<MemberInfo, OrderAttribute>(m, detail.Attribute);
                myInfoes.Add(info);
            }
            return myInfoes;
        }

       
    }

    public class BindingConfigDetails
    {
        public string MemberName { get; set; }

        public BindingType Target { get; set; }

        public OrderAttribute Attribute { get; set; }

        public string AttributeTypeName { get; set; }
    }

    public enum BindingType
    {
        Method = 0,
        Property = 1
    }
}
