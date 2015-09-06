using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Young.Data
{
    public class DataCenter
    {
        private DataSet _ds;

        public DataCenter()
        {
            _columnTableMappings = new List<Dictionary<string, string>>();
        }

        private List<Dictionary<string, string>> _columnTableMappings;

        public void SetData(DataSet Datas)
        {
            this._ds = Datas;
            _columnTableMappings = new List<Dictionary<string, string>>();
        }

        private void setColumnMappings()
        {
            foreach(DataTable dt in _ds.Tables)
            {
                Dictionary<string, string> tableColumnMapping = new Dictionary<string, string>();
                foreach(DataColumn dc in dt.Columns)
                {
                    tableColumnMapping.Add(dc.ColumnName.ToLower(),dt.TableName);
                }
                _columnTableMappings.Add(tableColumnMapping);
            }
        }

        public void BindToDataSet(Type t)
        {

        }

        public void BindToDataSet<T>()
        {

        }

        public void BindToTable<T>(DataTable dt)
        {

        }

        public void BindToTable(DataTable dt, Type t)
        {

        }

        public void BindToTable<T>(string tableName)
        {

        }

        public void BindToTable(string tableName,Type t)
        {

        }

        private void bindToDataSet(object instance)
        {

        }

        private void bindToTable(object instance,DataTable dt)
        {

        }
    }
}
