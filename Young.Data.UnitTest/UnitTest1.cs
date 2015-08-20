using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Young.Data.DBConnection;
using System.Data.SqlClient;
using System.Data;
using Young.Data.Extension;
using System.Text.RegularExpressions;

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
            ds.Tables[0].TableName = "Users";

            //ds.Tables[0].ExportToExcel(@"C:\1.xlsx", "");

            ExcelHelper.Current.Create(@"C:\1.xlsx").Write(ds.Tables[0]);
            ExcelHelper.Current.Close();

            Assert.IsTrue(ds.Tables.Count > 0);
            ds = da.GetData(new SqlDataAdapter(), "select top 10 * from Users", CommandType.Text);
            Assert.IsTrue(ds.Tables.Count > 0);
        }
        
        [TestMethod]
        public void TestMethod2()
        {
            var table = ExcelHelper.Current.Open(@"C:\test.xlsx").Read("SC_4001");
            //var table1 = ExcelHelper.Current.Open(@"D:\06005.xlsx").Read("FA_Recon_BWToListCube",new Range(2,2),new Range(4,4));
            //var table = ExcelHelper.Current.Open(@"D:\06005.xlsx").Read("FA_Recon_BWToListCube", null,null);
            

        }

        [TestMethod]
        public void SplitTest()
        {

            string input = "AB22";
                string[] numbers = Regex.Split(input, @"\d+");
                Console.WriteLine(numbers[0]);
            
            
        }
    }
}
