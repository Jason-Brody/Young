using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Young.Data
{
    //public sealed class DataBindingFactory
    //{

    //    public static T CreateInstance<T>(Type t,object[] parameters)
    //    {
    //        return default(T);
    //    }

        

    //    public void DataBinding()
    //    {

    //        if (BindingModeType == null)
    //        {
    //            _bindingType = GlobalBindingModeType;
    //        }
    //        else
    //        {
    //            _bindingType = BindingModeType;
    //        }

    //        dba = me.GetCustomAttributes(typeof(DataBindingAttribute), true).FirstOrDefault() as DataBindingAttribute;

    //        if (dba != null)
    //        {
    //            dba.TableName = string.IsNullOrEmpty(dba.TableName) ? me.Name : dba.TableName;

    //            if (TypeCounts.ContainsKey(me))
    //            {
    //                TypeCounts[me] += 1;
    //            }
    //            else
    //            {
    //                TypeCounts.Add(me, 0);
    //            }


    //            var atMiPairs = from m in me.GetMembers()
    //                            where (m.MemberType == MemberTypes.Property
    //                            || m.MemberType == MemberTypes.Method)
    //                            && m.GetCustomAttributes(typeof(OrderAttribute), true).FirstOrDefault() != null
    //                            orderby (m.GetCustomAttributes(typeof(OrderAttribute), true).First() as OrderAttribute).Order
    //                            select
    //                            new Tuple<MemberInfo, OrderAttribute>(m, m.GetCustomAttributes(typeof(OrderAttribute), true).FirstOrDefault() as OrderAttribute);


    //            if (_bindingType.DataMode == DataType.FromShareTable && _bindingType.DataMode != DataType.FromPrivateTable)
    //            {
    //                shareDataBinding(atMiPairs.ToList());
    //            }
    //            else
    //            {
    //                nonShareDataBinding(atMiPairs.ToList());
    //            }
    //        }
    //    }
    //    private void shareDataBinding(List<Tuple<MemberInfo, OrderAttribute>> mos)
    //    {
    //        foreach (var mo in mos)
    //        {
    //            if (mo.Item1 is PropertyInfo && (_bindingType.SettingMode == SettingType.PropertyOnly || _bindingType.SettingMode == SettingType.MethodAndProperty))
    //            {
    //                if (_bindingType.IsUsingSampleData)
    //                {
    //                    if (_bindingType.LoopMode == LoopType.Loop)
    //                    {
    //                        setProperty(mo.Item1 as PropertyInfo, mo.Item2 as OrderAttribute, TypeCounts[me]);
    //                    }
    //                    else
    //                    {
    //                        setProperty(mo.Item1 as PropertyInfo, mo.Item2 as OrderAttribute, 0);
    //                    }
    //                }
    //                else
    //                {
    //                    Func<DataTable> getTableFun = new Func<DataTable>(() =>
    //                    {
    //                        DataTable dt = null;
    //                        string tableName = "";
    //                        if (mo.Item2 is ColumnBindingAttribute)
    //                        {
    //                            var colAt = mo.Item2 as ColumnBindingAttribute;
    //                            colAt.ColName = string.IsNullOrEmpty(colAt.ColName) ? mo.Item1.Name : colAt.ColName;
    //                            if (_tableMapping.ContainsKey(colAt.ColName.ToLower()))
    //                            {
    //                                tableName = _tableMapping[colAt.ColName.ToLower()];
    //                            }
    //                        }
    //                        else if (mo.Item2 is MultiColumnBindingAttribute)
    //                        {
    //                            var mulColAt = mo.Item2 as MultiColumnBindingAttribute;
    //                            if (_tableMapping.ContainsKey(mulColAt.ColNames.First().ToLower()))
    //                            {
    //                                tableName = _tableMapping[mulColAt.ColNames.First().ToLower()];
    //                            }
    //                        }
    //                        if (tableName != "" && Data.Tables.Contains(tableName))
    //                        {
    //                            dt = Data.Tables[tableName];
    //                        }
    //                        return dt;
    //                    });

    //                    if (_bindingType.LoopMode == LoopType.Loop)
    //                    {
    //                        setProperty(mo.Item1 as PropertyInfo, mo.Item2 as OrderAttribute, getTableFun, TypeCounts[me]);
    //                    }
    //                    else
    //                    {
    //                        setProperty(mo.Item1 as PropertyInfo, mo.Item2 as OrderAttribute, getTableFun, 0);
    //                    }

    //                }

    //            }
    //            else if (mo.Item1 is MethodInfo && (_bindingType.SettingMode == SettingType.MethodOnly || _bindingType.SettingMode == SettingType.MethodAndProperty))
    //            {
    //                invokeMethod(mo.Item1 as MethodInfo);
    //            }
    //        }



    //    }

    //    private void nonShareDataBinding(List<Tuple<MemberInfo, OrderAttribute>> mos)
    //    {
    //        foreach (var mo in mos)
    //        {
    //            if (mo.Item1 is PropertyInfo && (_bindingType.SettingMode == SettingType.PropertyOnly || _bindingType.SettingMode == SettingType.MethodAndProperty))
    //            {
    //                if (_bindingType.IsUsingSampleData)
    //                {
    //                    if (_bindingType.LoopMode == LoopType.Loop)
    //                    {
    //                        setProperty(mo.Item1 as PropertyInfo, mo.Item2 as OrderAttribute, TypeCounts[me]);
    //                    }
    //                    else
    //                    {
    //                        setProperty(mo.Item1 as PropertyInfo, mo.Item2 as OrderAttribute, 0);
    //                    }
    //                }
    //                else
    //                {
    //                    Func<DataTable> getTableMethod = new Func<DataTable>(() =>
    //                    {
    //                        DataTable dt = null;
    //                        if (Data.Tables.Contains(dba.TableName))
    //                        {
    //                            dt = Data.Tables[dba.TableName];
    //                        }
    //                        return dt;
    //                    });

    //                    if (_bindingType.LoopMode == LoopType.Loop)
    //                    {
    //                        setProperty(mo.Item1 as PropertyInfo, mo.Item2 as OrderAttribute, getTableMethod, TypeCounts[me]);
    //                    }
    //                    else
    //                    {
    //                        setProperty(mo.Item1 as PropertyInfo, mo.Item2 as OrderAttribute, getTableMethod, 0);
    //                    }


    //                }

    //            }
    //            else if (mo.Item1 is MethodInfo && (_bindingType.SettingMode == SettingType.MethodOnly || _bindingType.SettingMode == SettingType.MethodAndProperty))
    //            {
    //                invokeMethod(mo.Item1 as MethodInfo);
    //            }

    //        }
    //    }
    //    private void setProperty(PropertyInfo prop, OrderAttribute attribute, int index)
    //    {
    //        var propertyType = prop.PropertyType;
    //        if (propertyType != typeof(string) && propertyType.IsSubclassOf(typeof(DataDriven)))
    //        {
    //            if (propertyType.GetConstructor(Type.EmptyTypes) != null)
    //            {
    //                dynamic newInstance = Activator.CreateInstance(propertyType);
    //                prop.SetValue(this, newInstance, null);
    //                if (OnSettingProperty != null)
    //                    OnSettingProperty(this, new SetPropertyArgs(prop, newInstance, attribute));


    //                if (_bindingType.RecusionMode != RecusionType.NoRecursion && _bindingType.RecusionMode == RecusionType.Recusion)
    //                    newInstance.DataBinding();
    //            }
    //        }
    //        else
    //        {
    //            if (attribute is ColumnBindingAttribute)
    //            {
    //                var sampleData = prop.GetCustomAttributes(typeof(SingleSampleDataAttribute), true).Cast<SingleSampleDataAttribute>().Where(d => d.Group == index).FirstOrDefault();
    //                if (sampleData != null)
    //                {
    //                    var value = Convert.ChangeType(sampleData.Value, propertyType);
    //                    prop.SetValue(this, value, null);
    //                    if (OnSettingProperty != null)
    //                        OnSettingProperty(this, new SetPropertyArgs(prop, value, attribute));
    //                }

    //            }
    //            else if (attribute is MultiColumnBindingAttribute)
    //            {
    //                var datas = prop.GetCustomAttributes(typeof(ComplexSampleDataAttribute), true).Cast<ComplexSampleDataAttribute>();
    //                if (datas.Count() > 0)
    //                {
    //                    var header = datas.Where(c => c.DataType == SampleDataType.Header && c.Group == index).FirstOrDefault();
    //                    if (header != null)
    //                    {
    //                        DataTable dt = new DataTable();
    //                        foreach (var s in header.Content)
    //                        {
    //                            dt.Columns.Add(new DataColumn(s));
    //                        }
    //                        var body = datas.Where(c => c.DataType == SampleDataType.Body && c.Group == index);
    //                        foreach (var r in body)
    //                        {
    //                            var row = dt.NewRow();
    //                            for (int i = 0; i < r.Content.Count(); i++)
    //                            {
    //                                row[i] = r.Content[i];
    //                            }
    //                            dt.Rows.Add(row);
    //                        }
    //                        var rows = dt.Select();
    //                        prop.SetValue(this, rows, null);
    //                        if (OnSettingProperty != null)
    //                            OnSettingProperty(this, new SetPropertyArgs(prop, rows, attribute));
    //                    }
    //                }

    //            }

    //        }
    //    }

    //    private void setProperty(PropertyInfo prop, OrderAttribute attribute, Func<DataTable> GetDataTable, int index)
    //    {
    //        var propertyType = prop.PropertyType;
    //        if (propertyType != typeof(string) && propertyType.IsSubclassOf(typeof(DataDriven)))
    //        {
    //            if (propertyType.GetConstructor(Type.EmptyTypes) != null)
    //            {
    //                dynamic newInstance = Activator.CreateInstance(propertyType);
    //                prop.SetValue(this, newInstance, null);
    //                if (OnSettingProperty != null)
    //                    OnSettingProperty(this, new SetPropertyArgs(prop, newInstance, attribute));
    //                if (_bindingType.RecusionMode != RecusionType.NoRecursion && _bindingType.RecusionMode == RecusionType.Recusion)
    //                    newInstance.DataBinding();
    //            }
    //        }
    //        else
    //        {
    //            DataTable dt = GetDataTable();
    //            if (dt != null && dt.Columns.Cast<DataColumn>().Where(c => c.ColumnName.ToLower() == dba.IdColumnName.ToLower()).FirstOrDefault() != null)
    //            {
    //                if (attribute is ColumnBindingAttribute)
    //                {
    //                    DataRow[] rows = dt.Select(dba.IdColumnName + "=" + CurrentId);
    //                    ColumnBindingAttribute colAt = attribute as ColumnBindingAttribute;
    //                    colAt.ColName = string.IsNullOrEmpty(colAt.ColName) ? prop.Name : colAt.ColName;

    //                    if (dt.Columns.Contains(colAt.ColName))
    //                    {
    //                        DataRow dr = rows[index];

    //                        if (dr != null && dr[colAt.ColName].ToString() != "")
    //                        {
    //                            if (colAt.Directory == DataDirectory.Input)
    //                            {
    //                                var value = Convert.ChangeType(dr[colAt.ColName], propertyType);
    //                                prop.SetValue(this, value, null);
    //                                if (OnSettingProperty != null)
    //                                    OnSettingProperty(this, new SetPropertyArgs(prop, value, attribute));
    //                            }
    //                            if (colAt.Directory == DataDirectory.Output)
    //                            {
    //                                dr[colAt.ColName] = prop.GetValue(this, null);
    //                            }
    //                        }
    //                    }
    //                }
    //                else if (attribute is MultiColumnBindingAttribute)
    //                {

    //                    MultiColumnBindingAttribute mulColAt = attribute as MultiColumnBindingAttribute;
    //                    string filter = dba.IdColumnName + "=" + CurrentId + " and " + mulColAt.GroupIdColumnName + index;
    //                    DataRow[] rows = dt.Select(filter);
    //                    bool isAllColContains = true;
    //                    foreach (var col in mulColAt.ColNames)
    //                    {
    //                        if (!dt.Columns.Contains(col))
    //                        {
    //                            isAllColContains = false;
    //                            break;
    //                        }
    //                    }
    //                    if (isAllColContains)
    //                    {
    //                        if (rows != null)
    //                        {
    //                            if (propertyType == typeof(DataRow[]))
    //                            {
    //                                prop.SetValue(this, rows, null);
    //                                if (OnSettingProperty != null)
    //                                    OnSettingProperty(this, new SetPropertyArgs(prop, rows, attribute));
    //                            }

    //                        }
    //                    }
    //                }
    //            }
    //        }

    //    }

    //    private void invokeMethod(MethodInfo method)
    //    {
    //        if (method.GetParameters().Count() == 0)
    //        {
    //            dynamic returnObj = method.Invoke(this, null);
    //            if (_bindingType.RecusionMode == RecusionType.Recusion && _bindingType.RecusionMode != RecusionType.NoRecursion)
    //            {
    //                if (returnObj != null && returnObj.GetType().IsSubclassOf(typeof(DataDriven)))
    //                {
    //                    returnObj.DataBinding();
    //                }
    //            }


    //        }
    //    }
    //}
}
