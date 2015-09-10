using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Young.Data.Attributes;

namespace Young.Data
{
    public class DataEngine
    {
        public event OnPropertyChangeHander OnSettingProperty;

        public event OnPropertyChangeHander OnGettingProperty;

        public Dictionary<string, DataBindingConfig> ClassConfigs;

        private Stack<Tuple<object,Type,bool,DataBindingAttribute>> _objStatus;

        private DataSet _ds;

        public DataSet Data
        {
            get { return _ds; }
        }

        public static BindingMode GlobalBindingMode { get; set; }

        private BindingMode _bindingMode { get; set; }

        private Queue<object> _objs;

        public Queue<object> Objects { get { return _objs; } }

        public int CurrentId { get; set; }

        private DataBindingAttribute _tableAttr;

        private bool _isFromConfig;

        private Type me;

        public bool IsUsingSampleData { get; set; }

        protected Dictionary<Type, int> TypeCounts = new Dictionary<Type, int>();

        public DataEngine()
        {
            _columnTableMappings = new List<Dictionary<string, string>>();
            _objStatus = new Stack<Tuple<object, Type, bool, DataBindingAttribute>>();
            _objs = new Queue<object>();
        }

        private List<Dictionary<string, string>> _columnTableMappings;

        public void SetData(DataSet Datas)
        {
            this._ds = Datas;
            _columnTableMappings = new List<Dictionary<string, string>>();
            setColumnMappings();
        }

        private void setColumnMappings()
        {
            foreach (DataTable dt in _ds.Tables)
            {
                Dictionary<string, string> tableColumnMapping = new Dictionary<string, string>();
                foreach (DataColumn dc in dt.Columns)
                {
                    tableColumnMapping.Add(dc.ColumnName.ToLower(), dt.TableName);
                }
                _columnTableMappings.Add(tableColumnMapping);
            }
        }

        public T Create<T>() where T : class, new()
        {
            me = typeof(T);
            if (isMatchBinding(typeof(T)))
            {
                T obj = new T();
                dataBinding(obj);
                return obj;
            }
            return null;
        }

        public void Update(object t)
        {
            me = t.GetType();
            if (isMatchBinding(t.GetType()))
                dataBinding(t);
        }

        public object Create(Type t)
        {
            me = t;
            if (isMatchBinding(t))
            {
                object obj = Activator.CreateInstance(t);
                dataBinding(obj);
                return obj;
            }
            return null;

        }

        public object Create(string TypeName)
        {
            Type t = Type.GetType(TypeName);
            return Create(t);
        }

        private void dataBinding(object instance)
        {
            _tempInstance = instance;

            _objs.Enqueue(instance);
           

            if (TypeCounts.ContainsKey(me))
            {
                TypeCounts[me] += 1;
            }
            else
            {
                TypeCounts.Add(me, 0);
            }

            if (_isFromConfig)
            {
                bindingFromConfigs(ClassConfigs[me.FullName]);
            }
            else
            {
                bindingFromCode();
            }
        }

        

        private object _tempInstance;

        private bool isMatchBinding(Type t)
        {
            if (ClassConfigs != null && ClassConfigs.ContainsKey(me.FullName))
            {
                _isFromConfig = true;
                return true;
            }
            _tableAttr = me.GetCustomAttribute<DataBindingAttribute>(true);
            if (_tableAttr != null)
            {
                setBindingMode(_tableAttr);
                _isFromConfig = false;
                return true;
            }

            return false;
        }

        private void setBindingMode(DataBindingAttribute tableAttr)
        {
            if(GlobalBindingMode == null)
            {
                _bindingMode = new BindingMode() {
                    DataMode = tableAttr.DataMode,
                    LoopMode = tableAttr.LoopMode,
                    RecusionMode = tableAttr.RecusionMode,
                    SettingMode = tableAttr.SettingMode
                };
            }
            else
            {
                _bindingMode = GlobalBindingMode;
            }
        }

        private void bindingFromCode()
        {
            _tableAttr.TableName = string.IsNullOrEmpty(_tableAttr.TableName) ? me.Name : _tableAttr.TableName;
            List<Tuple<MemberInfo, OrderAttribute>> reflectionMembers = null;

            IEnumerable<MemberInfo> members = null;

            switch (_bindingMode.SettingMode)
            {
                case SettingType.PropertyOnly:
                    members = me.GetProperties();
                    break;
                case SettingType.MethodOnly:
                    members = me.GetMethods();
                    break;
                default:
                    members = me.GetMembers();
                    break;
            }

            reflectionMembers = members
                .Where(m => m.GetCustomAttributes(typeof(OrderAttribute), true).FirstOrDefault() != null)
                .Select(m =>
                {
                    return new Tuple<MemberInfo, OrderAttribute>(m, m.GetCustomAttributes(typeof(OrderAttribute), true).FirstOrDefault() as OrderAttribute);
                }).OrderBy(m => m.Item2.Order).ToList();

            if (IsUsingSampleData)
            {
                sampleDataBinding(reflectionMembers);
            }
            else
            {
                tableDataBinding(reflectionMembers);
            }

        }

        private void bindingFromConfigs(DataBindingConfig config)
        {
            _tableAttr = config.DataBinding;
            setBindingMode(_tableAttr);
            _tableAttr.TableName = string.IsNullOrEmpty(_tableAttr.TableName) ? me.Name : _tableAttr.TableName;

            List<Tuple<MemberInfo, OrderAttribute>> reflectionMembers = config.GetInfoes();

            if (IsUsingSampleData)
            {
                sampleDataBinding(reflectionMembers);
            }
            else
            {
                tableDataBinding(reflectionMembers);
            }


        }

        private void sampleDataBinding(List<Tuple<MemberInfo, OrderAttribute>> mos)
        {
            foreach (var mo in mos)
            {
                if (mo.Item1 is PropertyInfo)
                {
                    if (_bindingMode.LoopMode == LoopType.Loop)
                    {
                        setProperty(mo.Item1 as PropertyInfo, mo.Item2 as ColumnBindingAttribute, TypeCounts[me]);
                    }
                    else
                    {
                        setProperty(mo.Item1 as PropertyInfo, mo.Item2 as ColumnBindingAttribute, 0);
                    }
                }
                else if (mo.Item1 is MethodInfo)
                {
                    invokeMethod(mo.Item1 as MethodInfo);
                }
            }
        }

        private void tableDataBinding(List<Tuple<MemberInfo, OrderAttribute>> mos)
        {
            foreach (var mo in mos)
            {
                if (mo.Item1 is PropertyInfo)
                {
                    PropertyInfo prop = mo.Item1 as PropertyInfo;
                    ColumnBindingAttribute colAttr = mo.Item2 as ColumnBindingAttribute;
                    resetColumnName(mo.Item1, colAttr);
                    bool isSimpleProp = isSimpleField(prop);

                    DataRow[] data = null;


                    if (_bindingMode.DataMode == DataType.FromShareTable)
                    {
                        data = getSharedData(colAttr, isSimpleProp);
                    }
                    else
                    {
                        data = getPrivateData(colAttr, isSimpleProp);
                    }

                    int index = _bindingMode.LoopMode == LoopType.Loop ? TypeCounts[me] : 0;


                    if (isSimpleProp)
                    {
                        if (colAttr.Directory == DataDirectory.Input)
                            setSingleProperty(prop, colAttr, data, index);
                        else
                            getSingleProperty(prop, colAttr, data, index);
                    }
                    else
                    {
                        if (colAttr.Directory == DataDirectory.Input)
                            setComplexProperty(prop, colAttr, data, index);
                    }
                }
                else if (mo.Item1 is MethodInfo)
                {
                    invokeMethod(mo.Item1 as MethodInfo);
                }
            }

        }

        private DataRow[] getSharedData(ColumnBindingAttribute attribute, bool isSimpleCondition)
        {
            DataTable dt = null;
            string tableName = "";

            string columnName = attribute.ColNames.Last();

            foreach (var dic in _columnTableMappings)
            {
                if (dic.ContainsKey(columnName.ToLower()))
                {
                    tableName = dic[columnName.ToLower()];
                }
            }
            if (tableName != "" && _ds.Tables.Contains(tableName))
            {
                dt = _ds.Tables[tableName];
            }
            if (dt != null)
            {
                string filter = null;
                filter = getTableFilter(attribute, isSimpleCondition);
                if (filter != null && isColumnsInclude(dt, attribute))
                {
                    return dt.Select(filter);
                }
            }
            return null;
        }

        private DataRow[] getPrivateData(ColumnBindingAttribute attribute, bool isSimpleCondition)
        {
            DataTable dt = null;
            if (_ds.Tables.Contains(_tableAttr.TableName))
            {
                dt = _ds.Tables[_tableAttr.TableName];
            }

            if (dt != null)
            {
                string filter = null;
                filter = getTableFilter(attribute, isSimpleCondition);
                if (filter != null && isColumnsInclude(dt, attribute))
                {
                    return dt.Select(filter);
                }
            }

            return null;
        }

        private bool isSimpleField(PropertyInfo prop)
        {
            if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
                return true;
            return false;
        }

        private void resetColumnName(MemberInfo prop, ColumnBindingAttribute attr)
        {
            if (attr.ColNames == null || attr.ColNames.Count() == 0)
            {
                attr.ColNames = new string[] { prop.Name };
            }

        }

        private void getSingleProperty(PropertyInfo prop, ColumnBindingAttribute attribute, DataRow[] rows, int index)
        {
            string colName = attribute.ColNames.First();
            DataRow dr = rows[index];

            dr[colName] = prop.GetValue(_tempInstance, null);
            if(OnGettingProperty != null)
                OnGettingProperty(_tempInstance, new SetPropertyArgs(prop, dr[colName], attribute));
        }

        private void setSingleProperty(PropertyInfo prop, ColumnBindingAttribute attribute, DataRow[] rows, int index)
        {
            object value = null;
            if (rows != null && rows.Count() > 0)
            {
                string colName = attribute.ColNames.First();

                //May Throw Error
                DataRow dr = rows[index];

                if (dr[colName] != null && dr[colName].ToString() != "")
                {
                    value = getConvertValue(attribute, dr[colName], prop.PropertyType);
                }
            }
            setValue(prop, value, attribute);
        }

        private void setComplexProperty(PropertyInfo prop, ColumnBindingAttribute attribute, DataRow[] rows, int index)
        {
            object value = null;
            value = getConvertValue(attribute, rows, prop.PropertyType);
            setValue(prop, value, attribute);
        }

        private void setValue(PropertyInfo prop, object value, ColumnBindingAttribute attribute)
        {
            if (value != null)
            {
                prop.SetValue(_tempInstance, value, null);
                if (OnSettingProperty != null)
                    OnSettingProperty(_tempInstance, new SetPropertyArgs(prop, value, attribute));
                runRecusion(value);
            }
        }

        

        private void setProperty(PropertyInfo prop, ColumnBindingAttribute attribute, int index)
        {
            var propertyType = prop.PropertyType;
            object value = null;

            var singleData = prop.GetCustomAttributes(typeof(SingleSampleDataAttribute), true).Cast<SingleSampleDataAttribute>().Where(d => d.Group == index).FirstOrDefault();
            if (singleData != null)
            {
                value = getConvertValue(attribute, singleData.Value, propertyType);
            }
            else
            {
                var datas = prop.GetCustomAttributes(typeof(ComplexSampleDataAttribute), true).Cast<ComplexSampleDataAttribute>();
                if (datas.Count() > 0)
                {
                    var header = datas.Where(c => c.DataType == SampleDataType.Header && c.Group == index).FirstOrDefault();
                    if (header != null)
                    {
                        DataTable dt = new DataTable();
                        foreach (var s in header.Content)
                        {
                            dt.Columns.Add(new DataColumn(s));
                        }
                        var body = datas.Where(c => c.DataType == SampleDataType.Body && c.Group == index);
                        foreach (var r in body)
                        {
                            var row = dt.NewRow();
                            for (int i = 0; i < r.Content.Count(); i++)
                            {
                                row[i] = r.Content[i];
                            }
                            dt.Rows.Add(row);
                        }
                        var rows = dt.Select();
                        value = getConvertValue(attribute, rows, propertyType);
                    }
                }
            }

            setValue(prop, value, attribute);
        }

        private void invokeMethod(MethodInfo method)
        {
            if (method.GetParameters().Count() == 0)
            {
                dynamic returnObj = method.Invoke(_tempInstance, null);
                if (method.ReturnType != typeof(void))
                {
                    runRecusion(returnObj);
                }

            }
        }

        private string getTableFilter(ColumnBindingAttribute attribute, bool isSimplecondition)
        {
            string filter = null;
            if (attribute != null)
            {
                filter = _tableAttr.IdColumnName + "=" + CurrentId;
                if (!isSimplecondition)
                {
                    if (_bindingMode.LoopMode == LoopType.Loop)
                    {
                        filter += (" and " + attribute.GroupIdColumnName + "=" + TypeCounts[me].ToString());
                    }
                }


            }
            return filter;
        }

        private bool isColumnsInclude(DataTable dt, ColumnBindingAttribute attribute)
        {
            bool isAllContain = true;

            foreach (var s in attribute.ColNames)
            {
                if (dt.Columns.Cast<DataColumn>().Where(c => c.ColumnName.ToLower().Contains(s.ToLower())).FirstOrDefault() == null)
                {
                    isAllContain = false;
                    break;
                }
            }
            return isAllContain;
        }

        private object getConvertValue(ColumnBindingAttribute colAttr, object SourceValue, Type targetType)
        {
            object value = null;
            if (colAttr.MethodName != null && colAttr.Target != null)
            {
                var customDelegate = Delegate.CreateDelegate(typeof(ColumnBindingConvert), colAttr.Target, colAttr.MethodName) as ColumnBindingConvert;
                value = customDelegate.Invoke(SourceValue);
            }
            else
            {
                value = Convert.ChangeType(SourceValue, targetType);
            }
            return value;
        }

        private void runRecusion(object returnObj)
        {
            if (_bindingMode.RecusionMode == RecusionType.Recusion)
            {

                if (returnObj != null)
                {
                    Type returnType = returnObj.GetType();
                    if (returnType.IsClass && returnType != typeof(string))
                    {
                        Tuple<object, Type, bool, DataBindingAttribute> status 
                            = new Tuple<object, Type, bool, DataBindingAttribute>(
                                _tempInstance,me,_isFromConfig,_tableAttr);
                        _objStatus.Push(status);
                        Update(returnObj);
                        status = _objStatus.Pop();
                        _tempInstance = status.Item1;
                        me = status.Item2;
                        _isFromConfig = status.Item3;
                        _tableAttr = status.Item4;
                    }
                        
                }
            }
        }
    }
}
