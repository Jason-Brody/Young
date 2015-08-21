using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Young.Data.Attributes;

namespace Young.Data
{
    public class DataDriven
    {
        private static List<string> _privateTables;
        static DataDriven()
        {
            Cycle = CycleType.Default;
        }

        public DataDriven(bool Shared)
        {
            this.Shared = Shared;
        }

        private DataBindingAttribute dba;


        public static DataSet Data { get; set; }

        public static int CurrentId { get; set; }

        public static Dictionary<Type, int> TypeCounts = new Dictionary<Type, int>();

        private Dictionary<string, DataRow[]> _selectedRow;

        public static List<string> NonSharedTables
        {
            get { return _privateTables; }
            set
            {
                _privateTables = value;
                addColumnTableMapping();
            }
        }

        public static CycleType Cycle { get; set; }

        private static Dictionary<string, string> _tableMapping;

        public bool Shared { get; set; }

        private static void addColumnTableMapping()
        {
            _tableMapping = new Dictionary<string, string>();
            if (_privateTables == null)
                _privateTables = new List<string>();
            else
                _privateTables.ForEach(s => s = s.ToLower());

            if (Data != null)
            {
                foreach (DataTable table in Data.Tables)
                {
                    if (!_privateTables.Contains(table.TableName.ToLower()))
                    {
                        foreach (DataColumn dc in table.Columns)
                        {
                            if (!_tableMapping.ContainsKey(dc.ColumnName.ToLower()))
                                _tableMapping.Add(dc.ColumnName.ToLower(), table.TableName);
                        }
                    }
                }
            }
        }

        private void shareDataBinding(List<Tuple<MemberInfo, OrderAttribute>> mos)
        {
            foreach (var mo in mos)
            {
                if (mo.Item1 is PropertyInfo)
                {

                    setProperty(mo.Item1 as PropertyInfo, mo.Item2 as OrderAttribute,
                        () =>
                        {
                            DataTable dt = null;
                            string tableName = "";
                            if (mo.Item2 is ColumnBindingAttribute)
                            {
                                var colAt = mo.Item2 as ColumnBindingAttribute;
                                colAt.ColName = string.IsNullOrEmpty(colAt.ColName) ? mo.Item1.Name : colAt.ColName;
                                if (_tableMapping.ContainsKey(colAt.ColName.ToLower()))
                                {
                                    tableName = _tableMapping[colAt.ColName.ToLower()];
                                }
                            }
                            else if (mo.Item2 is MultiColumnBindingAttribute)
                            {
                                var mulColAt = mo.Item2 as MultiColumnBindingAttribute;
                                if (_tableMapping.ContainsKey(mulColAt.ColNames.First().ToLower()))
                                {
                                    tableName = _tableMapping[mulColAt.ColNames.First().ToLower()];
                                }
                            }
                            if (tableName != "" && Data.Tables.Contains(tableName))
                            {
                                dt = Data.Tables[tableName];
                            }
                            return dt;
                        });
                }
                else if (mo.Item1 is MethodInfo)
                {
                    invokeMethod(mo.Item1 as MethodInfo);
                }
            }



        }

        public void DataBinding()
        {
            Type me = this.GetType();
            dba = me.GetCustomAttributes(typeof(DataBindingAttribute), true).FirstOrDefault() as DataBindingAttribute;

            if (dba != null)
            {
                dba.TableName = string.IsNullOrEmpty(dba.TableName) ? me.Name : dba.TableName;



                var atMiPairs = from m in me.GetMembers()
                                where (m.MemberType == MemberTypes.Property
                                || m.MemberType == MemberTypes.Method)
                                && m.GetCustomAttributes(typeof(OrderAttribute), true).FirstOrDefault() != null
                                orderby (m.GetCustomAttributes(typeof(OrderAttribute), true).First() as OrderAttribute).Order
                                select
                                new Tuple<MemberInfo, OrderAttribute>(m, m.GetCustomAttributes(typeof(OrderAttribute), true).FirstOrDefault() as OrderAttribute);


                if (Shared)
                {
                    shareDataBinding(atMiPairs.ToList());
                }
                else
                {
                    nonShareDataBinding(atMiPairs.ToList());
                }
            }
        }

        private void setProperty(PropertyInfo prop, OrderAttribute attribute, Func<DataTable> GetDataTable)
        {
            var propertyType = prop.PropertyType;
            if (propertyType != typeof(string) && propertyType.IsSubclassOf(typeof(DataDriven)))
            {
                if (propertyType.GetConstructor(Type.EmptyTypes) != null)
                {
                    dynamic newInstance = Activator.CreateInstance(propertyType);
                    prop.SetValue(this, newInstance, null);
                    newInstance.DataBinding();
                }
            }
            else
            {
                DataTable dt = GetDataTable();
                if (dt != null && dt.Columns.Cast<DataColumn>().Where(c => c.ColumnName.ToLower() == dba.IdColumnName.ToLower()).FirstOrDefault() != null)
                {
                    DataRow[] rows = dt.Select(dba.IdColumnName + "=" + CurrentId);

                    if (attribute is ColumnBindingAttribute)
                    {
                        ColumnBindingAttribute colAt = attribute as ColumnBindingAttribute;
                        colAt.ColName = string.IsNullOrEmpty(colAt.ColName) ? prop.Name : colAt.ColName;

                        if (dt.Columns.Contains(colAt.ColName))
                        {
                            DataRow dr = rows.FirstOrDefault();

                            if (dr != null && dr[colAt.ColName].ToString() != "")
                            {
                                if (colAt.Directory == DataDirectory.Input)
                                {
                                    var value = Convert.ChangeType(dr[colAt.ColName], propertyType);
                                    prop.SetValue(this, value, null);
                                }
                                if (colAt.Directory == DataDirectory.Output)
                                {
                                    dr[colAt.ColName] = prop.GetValue(this, null);
                                }
                            }
                        }
                    }
                    else if (attribute is MultiColumnBindingAttribute)
                    {
                        MultiColumnBindingAttribute mulColAt = attribute as MultiColumnBindingAttribute;
                        bool isAllColContains = true;
                        foreach (var col in mulColAt.ColNames)
                        {
                            if (!dt.Columns.Contains(col))
                            {
                                isAllColContains = false;
                                break;
                            }
                        }
                        if (isAllColContains)
                        {
                            if (rows != null)
                            {
                                if (propertyType == typeof(DataRow[]))
                                    prop.SetValue(this, rows, null);
                            }
                        }
                    }
                }
            }

        }

        private void invokeMethod(MethodInfo method)
        {
            if (method.GetParameters().Count() == 0)
            {
                dynamic returnObj = method.Invoke(this, null);
                if (returnObj != null && returnObj.GetType().IsSubclassOf(typeof(DataDriven)))
                {
                    returnObj.DataBinding();
                }

            }
        }

        private void nonShareDataBinding(List<Tuple<MemberInfo, OrderAttribute>> mos)
        {
            foreach (var mo in mos)
            {
                if (mo.Item1 is PropertyInfo)
                {
                    setProperty(mo.Item1 as PropertyInfo, mo.Item2 as OrderAttribute
                        , () =>
                        {
                            DataTable dt = null;
                            if (Data.Tables.Contains(dba.TableName))
                            {
                                dt = Data.Tables[dba.TableName];
                            }
                            return dt;
                        });
                }
                else if (mo.Item1 is MethodInfo)
                {
                    invokeMethod(mo.Item1 as MethodInfo);
                }

            }
        }


    }


}
