using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Young.Data.DBConnection;
using System.Data.SqlClient;
using System.Data;

namespace Young.Data.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            SqlConnectionStringBuilder ssb = new SqlConnectionStringBuilder();
            ssb.DataSource = "localhost";
            ssb.InitialCatalog = "SAPTestCenter";
            ssb.IntegratedSecurity = true;
            SqlConnection sqlCn = new SqlConnection(ssb.ConnectionString);
            DBAccess da = new DBAccess(sqlCn,new SqlCommand());
            DataSet ds = da.GetData(new SqlDataAdapter(), "select top 10 * from Users", CommandType.Text);
            Assert.IsTrue(ds.Tables.Count > 0);
            ds = da.GetData(new SqlDataAdapter(), "select top 10 * from Users", CommandType.Text);
            Assert.IsTrue(ds.Tables.Count > 0);
        }
    }
}
