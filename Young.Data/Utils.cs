using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Young.Data
{
    public static class Utils
    {
        public static DataTable ReadStringToTable(string filePath, Func<string, string, List<string>> LineFunc)
        {
            string tempString = "";
            DataTable table = null;
            string headerRow = "";
            using (StreamReader sr = new StreamReader(filePath))
            {
                while (!sr.EndOfStream)
                {
                    tempString = sr.ReadLine();
                    var vals = LineFunc(tempString, headerRow);
                    if (vals != null && vals.Count() > 0)
                    {
                        if (table == null)
                        {
                            table = new DataTable();
                            Dictionary<string, int> cols = new Dictionary<string, int>();
                            headerRow = tempString;
                            for (int i = 0; i < vals.Count(); i++)
                            {
                                if (vals[i] == "")
                                {
                                    vals[i] = "Header_Temp_" + i.ToString();
                                }
                                while (cols.ContainsKey(vals[i]))
                                {
                                    vals[i] = vals[i] + "1";
                                }
                                cols.Add(vals[i], 0);

                                table.Columns.Add(vals[i]);
                            }
                        }
                        else
                        {
                            DataRow dr = table.NewRow();
                            for (int i = 0; i < vals.Count(); i++)
                            {
                                dr[i] = vals[i];
                            }
                            table.Rows.Add(dr);
                        }
                    }
                }
            }
            return table;
        }
    }
}
