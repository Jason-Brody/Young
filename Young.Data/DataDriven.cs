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

        private Type me;

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

        private void classBinding()
        {

        }

        private void propBinding(MemberInfo mi, OrderAttribute order)
        {
            var property = mi as PropertyInfo;
            var propertyType = property.PropertyType;
            if (propertyType != typeof(string) && propertyType.IsSubclassOf(typeof(DataDriven)))
            {
                if (propertyType.GetConstructor(Type.EmptyTypes) != null)
                {
                    dynamic newInstance = Activator.CreateInstance(propertyType);
                    property.SetValue(this, newInstance, null);
                    newInstance.DataBinding();
                }
            }
            else
            {
                DataRow[] tempData = null;
                if (order is ColumnBindingAttribute)
                {
                    ColumnBindingAttribute colAt = order as ColumnBindingAttribute;
                    colAt.ColName = string.IsNullOrEmpty(colAt.ColName) ? mi.Name : colAt.ColName;
                    string tableName = "";
                    if (Shared)
                    {
                        if (_tableMapping.ContainsKey(colAt.ColName.ToLower()))
                        {
                            tableName = _tableMapping[colAt.ColName.ToLower()];
                        }
                    }
                    else
                    {
                        tableName = dba.TableName;
                    }

                    if (_selectedRow.ContainsKey(tableName))
                    {
                        tempData = _selectedRow[tableName];
                    }
                    else
                    {
                        if (Data.Tables.Contains(tableName))
                        {
                            tempData = Data.Tables[tableName].Select(dba.IdColumnName + "=" + CurrentId);
                            _selectedRow.Add(tableName, tempData);
                        }
                    }

                    if (tempData != null)
                    {
                        DataRow dr = tempData.FirstOrDefault();
                        if (_tableMapping.ContainsKey(colAt.ColName.ToLower()) && dr[colAt.ColName].ToString() != "")
                        {
                            if (colAt.Directory == DataDirectory.Input)
                            {
                                var value = Convert.ChangeType(dr[colAt.ColName], propertyType);
                                property.SetValue(this, value, null);
                            }
                            if (colAt.Directory == DataDirectory.Output)
                            {
                                dr[colAt.ColName] = property.GetValue(this, null);
                            }
                        }
                    }
                }
                else if (order is MultiColumnBindingAttribute)
                {
                    //MultiColumnBindingAttribute mulColAt = order as MultiColumnBindingAttribute;
                    //bool isAllColContains = true;

                    //foreach (var col in mulColAt.ColNames)
                    //{
                    //    if (!dt.Columns.Contains(col))
                    //    {
                    //        isAllColContains = false;
                    //        break;
                    //    }
                    //}
                    //if (isAllColContains)
                    //{
                    //    DataRow[] drs = dt.Select(tableAt.IdColumnName + "=" + CurrentId);
                    //    if (drs != null)
                    //    {
                    //        if (property.PropertyType == typeof(DataRow[]))
                    //            property.SetValue(this, drs, null);
                    //        else
                    //        {
                    //            var method = Delegate.CreateDelegate(typeof(ConvertMethod), this, mulColAt.MethodName) as ConvertMethod;
                    //            property.SetValue(this, method(drs), null);
                    //        }
                    //    }
                    //}
                }
            }
        }

        private void methodBinding(MemberInfo mi)
        {
            var method = mi as MethodInfo;
            if (method.GetParameters().Count() == 0)
            {
                dynamic returnObj = method.Invoke(this, null);
                if (returnObj != null && returnObj.GetType().IsSubclassOf(typeof(DataDriven)))
                {
                    returnObj.DataBinding();
                }

            }
        }

        public void DataBinding()
        {
            if (Data != null && Data.Tables.Count > 0)
            {
                me = this.GetType();
                DataTable dt = null;

                dba = me.GetCustomAttributes(typeof(DataBindingAttribute), true).FirstOrDefault() as DataBindingAttribute;

                if (dba != null)
                {
                    dba.TableName = string.IsNullOrEmpty(dba.TableName) ? me.Name : dba.TableName;

                    if (TypeCounts.ContainsKey(me))
                    {
                        TypeCounts[me] += 1;
                    }
                    else
                    {
                        TypeCounts.Add(me, 0);
                    }

                    if (!Shared && Data.Tables.Contains(dba.TableName))
                    {
                        dt = Data.Tables[dba.TableName];
                    }


                    var atMiPairs = from m in me.GetMembers()
                                    where (m.MemberType == MemberTypes.Property
                                    || m.MemberType == MemberTypes.Method)
                                    && m.GetCustomAttributes(typeof(OrderAttribute), true).FirstOrDefault() != null
                                    orderby (m.GetCustomAttributes(typeof(OrderAttribute), true).First() as OrderAttribute).Order
                                    select new { Order = m.GetCustomAttributes(typeof(OrderAttribute), true).FirstOrDefault(), MemberInfo = m };

                    _selectedRow = new Dictionary<string, DataRow[]>();

                    foreach (var pair in atMiPairs)
                    {
                        if (pair.MemberInfo is PropertyInfo)
                        {
                            propBinding(pair.MemberInfo as PropertyInfo, pair.Order as OrderAttribute);
                        }
                        else if (pair.MemberInfo is MethodInfo)
                        {
                            methodBinding(pair.MemberInfo);
                        }
                    }
                }
            }

        }


         public void DataBindingV2()
        {
            if (Data != null && Data.Tables.Count > 0)
            {
                Type me = this.GetType();
                DataTable dt = null;

                var tableAt = me.GetCustomAttributes(typeof(DataBindingAttribute), true).FirstOrDefault() as DataBindingAttribute;

                if (tableAt != null)
                {
                    tableAt.TableName = string.IsNullOrEmpty(tableAt.TableName) ? me.Name : tableAt.TableName;


                    if (Data.Tables.Contains(tableAt.TableName))
                    {
                        if (TypeCounts.ContainsKey(me))
                        {
                           TypeCounts[me] += 1;
                        }
                        else
                        {
                            TypeCounts.Add(me, 0);
                        }
                        dt =Data.Tables[tableAt.TableName];
                    }


                    var atMiPairs = from m in me.GetMembers()
                                    where (m.MemberType == MemberTypes.Property
                                    || m.MemberType == MemberTypes.Method)
                                    && m.GetCustomAttributes(typeof(OrderAttribute), true).FirstOrDefault() != null
                                    orderby (m.GetCustomAttributes(typeof(OrderAttribute), true).First() as OrderAttribute).Order
                                    select new { Order = m.GetCustomAttributes(typeof(OrderAttribute), true).FirstOrDefault(), MemberInfo = m };

                    foreach (var pair in atMiPairs)
                    {
                        if (pair.MemberInfo is PropertyInfo)
                        {
                            var property = pair.MemberInfo as PropertyInfo;
                            var propertyType = property.PropertyType;
                            if (propertyType.IsClass && propertyType != typeof(string) && propertyType.IsSubclassOf(typeof(DataDriven)))
                            {
                                if (propertyType.GetConstructor(Type.EmptyTypes) != null)
                                {
                                    dynamic newInstance = Activator.CreateInstance(propertyType);
                                    property.SetValue(this, newInstance, null);
                                    newInstance.DataBinding();
                                }
                            }
                            else if (dt != null)
                            {
                                if (pair.Order is ColumnBindingAttribute)
                                {
                                    ColumnBindingAttribute colAt = pair.Order as ColumnBindingAttribute;
                                    colAt.ColName = string.IsNullOrEmpty(colAt.ColName) ? pair.MemberInfo.Name : colAt.ColName;

                                    if (dt != null && dt.Columns.Contains(colAt.ColName))
                                    {
                                        DataRow dr = null;
                                        if (Cycle == CycleType.Default)
                                            dr = dt.Select(tableAt.IdColumnName + "=" + CurrentId).FirstOrDefault();
                                        else
                                            dr = dt.Select(tableAt.IdColumnName + "=" + CurrentId)[TypeCounts[me]];

                                        if (dr != null && dr[colAt.ColName].ToString() != "")
                                        {
                                            if (colAt.Directory == DataDirectory.Input)
                                            {
                                                var value = Convert.ChangeType(dr[colAt.ColName], propertyType);
                                                property.SetValue(this, value, null);
                                            }
                                            if (colAt.Directory == DataDirectory.Output)
                                            {
                                                dr[colAt.ColName] = property.GetValue(this, null);
                                            }
                                        }
                                    }
                                }
                                else if (pair.Order is MultiColumnBindingAttribute)
                                {
                                    MultiColumnBindingAttribute mulColAt = pair.Order as MultiColumnBindingAttribute;
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


                                        DataRow[] drs = dt.Select(tableAt.IdColumnName + "=" + CurrentId);
                                        if (drs != null)
                                        {
                                            if (property.PropertyType == typeof(DataRow[]))
                                                property.SetValue(this, drs, null);
                                            else
                                            {
                                                var method = Delegate.CreateDelegate(typeof(ConvertMethod), this, mulColAt.MethodName) as ConvertMethod;
                                                property.SetValue(this, method(drs), null);
                                            }

                                        }
                                    }
                                }
                            }

                        }
                        else if (pair.MemberInfo is MethodInfo)
                        {
                            var method = pair.MemberInfo as MethodInfo;
                            if (method.GetParameters().Count() == 0)
                            {
                                dynamic returnObj = method.Invoke(this, null);
                                if (returnObj != null && returnObj.GetType().IsSubclassOf(typeof(DataDriven)))
                                {
                                    returnObj.DataBinding();
                                }

                            }
                        }

                    }
                }
            }

        }
    }


}
