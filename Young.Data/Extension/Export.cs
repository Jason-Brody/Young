using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Young.Data.Attributes;

namespace Young.Data.Extension
{
    public static class Export
    {
        public static void ExportToFile<T>(this IEnumerable<T> Data, string fileName, string splitChar) where T : class
        {
            List<PropertyInfo> props = typeof(T).GetProperties().Where(p => (p.PropertyType == typeof(string) || p.PropertyType.IsValueType) && p.DeclaringType.IsPublic).ToList();

            using (StreamWriter sw = new StreamWriter(fileName, false))
            {
                string line = "";
                foreach (var prop in props)
                {
                    var attr = prop.GetCustomAttribute<AliasAttribute>();
                    if (attr != null)
                        line += attr.Name + splitChar;
                    else
                        line += prop.Name + splitChar;
                }
                line = line.Substring(0, line.Length - 1);
                sw.WriteLine(line);
                foreach (var item in Data)
                {
                    line = "";
                    foreach (var p in props)
                    {
                        var val = p.GetValue(item);
                        if (val == null)
                            val = "";

                        line += val.ToString() + splitChar;
                    }
                    line = line.Substring(0, line.Length - 1);
                    sw.WriteLine(line);
                }

            }
        }
    }
}
