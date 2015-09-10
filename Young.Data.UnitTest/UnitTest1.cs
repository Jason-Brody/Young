using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Young.Data.DBConnection;
using System.Data.SqlClient;
using System.Data;
using Young.Data.Extension;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Young.Data.Attributes;
using SAPAutomation;
using SAPFEWSELib;
using System.Reflection;
using SAPGuiAutomationLib;
using System.IO;


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
            var table = ExcelHelper.Current.Open(@"C:\test.xlsx").Read("Sheet1",new Range(1,2),new Range(10,4));
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

        [TestMethod]
        public void GetCharValueTest()
        {
            string testStr = "AAA";
            double value = 0;



            var arrayList = testStr.ToList();
            while(arrayList.Count > 0)
            {
                char first = arrayList.First();
                arrayList.Remove(first);
                value += (Convert.ToInt16(first) - 64) * Math.Pow(26,arrayList.Count);
               
            }
        }

        [TestMethod]
        public void BindingTest()
        {
            DataEngine dc = new DataEngine();
            dc.SetData(ExcelHelper.Current.Open(@"D:\test1.xlsx").ReadAll());
            dc.CurrentId = 1;
            TestC t = new TestC();
            string Msg = "";
            dc.OnSettingProperty += (o, e) =>
            {
                var str = string.Format("Set Property:{0} values {1}\n", e.Property.Name, e.Value);
                Msg += str;
            };

            dc.DataBinding(t);
            //DataDriven.Data = ExcelHelper.Current.Open(@"D:\test1.xlsx").ReadAll();
            //DataDriven.CurrentId = 1;
            //DataDriven.GlobalBindingModeType.RecusionMode = RecusionType.Recusion;
            //DataDriven.NonSharedTables = new List<string>();
            //TestC t = new TestC();


            //t.DataBinding();
            var msg = TestC.Msg;
        }

        public delegate T GetResult<T>(T a, T b);

        [TestMethod]
        public void DelegateTest()
        {
            MethodInfo mi = typeof(TestDelegate).GetMethod(nameof(TestDelegate.Add));

            //Delegate.CreateDelegate()
            var method = Delegate.CreateDelegate(typeof(GetResult<string>), mi);
            //string a = method.Method.Invoke()
        }

        [TestMethod]
        public void SAPCompTest()
        {
            //SAPAutomationHelper.Current.SetSAPApiAssembly();
            //var asm = SAPAutomationHelper.Current.SAPGuiApiAssembly;
           // var tps = GetTypeByMethod(asm, "PressContextButton");
            
            SAPTestHelper.Current.SetSession();
            var menu = SAPTestHelper.Current.MainWindow.FindByName<GuiContainerShell>("shellcont").FindByName<GuiGridView>("shell").CurrentContextMenu;
            string temp;
            foreach(GuiContextMenu s in menu.Children)
            {
                
                temp = s.Text;
            }
            var guiToolBar = SAPTestHelper.Current.MainWindow.FindByName<GuiGOSShell>("shellcont[1]").FindByName<GuiToolbarControl>("shell");
            
            SAPTestHelper.Current.MainWindow.ShowMessageBox("Warning", "Hello", 0, 0);
            string script = "";
            using (StreamReader sr = new StreamReader(new FileStream(@"C:\Users\Young\AppData\Roaming\SAP\SAP GUI\Scripts\Script4.vbs",FileMode.Open)))
            {
                script = sr.ReadToEnd();
            }
            SAPTestHelper.Current.SAPGuiSession.RunScriptControl(script, 1);

            //var guiToolBar = SAPTestHelper.Current.MainWindow.FindByName<GuiGOSShell>("shellcont[1]").FindByName<GuiToolbarControl>("shell");

            var test = SAPTestHelper.Current.GetElementById("wnd[0]/titl/shellcont[1]/shell");
            var dp = test.Type;

            var control = SAPTestHelper.Current.MainWindow.FindByName<GuiContainerShell>("shellcont[1]").FindByName<GuiTextedit>("shell");

            var tree = SAPTestHelper.Current.MainWindow.FindByName<GuiContainerShell>("shellcont[0]").FindByName<GuiTree>("shell");
            var node = tree.GetNextNodeKey(tree.TopNode);
            tree.SelectNode(node);
            var comp = SAPTestHelper.Current.MainWindow.FindByName<GuiComboBox>("VBAK-AUGRU");
            string b= "";
            string v = "";
            foreach(GuiComboBoxEntry keyV in comp.GetEntries())
            {
                b += string.Format("Key:{0} -> Pos:{1} -> Val:{2}\n", keyV.Key, keyV.Pos, keyV.Value);
                if(keyV.Key=="C02")
                {
                    v = keyV.Value;
                }
            }
        }

        

        [TestMethod]
        public void RegexTest()
        {
            var str = "Document 38643036 posted";
            var number = Regex.Replace(str,@"\D+","");
        }


        public static IEnumerable<Type> GetTypeByMethod(Assembly asm,string MethodName)
        {
            foreach(var t in asm.GetTypes())
            {
                foreach(var m in t.GetMethods().Where(mi=>mi.IsSpecialName == false))
                {
                    if(m.Name.IndexOf(MethodName)>=0)
                    {
                        yield return t;
                    }
                }
            }
        }

       
    }

    public class TestNinja
    {
        public void Test()
        {
           
           
        }

    }


    public class TestBase:DataDriven
    {
        public static string Msg = "";
        public TestBase(bool isShare)
        {
            OnSettingProperty += (o, e) =>
            {
                var str = string.Format("Set Property:{0} values {1}\n", e.Property.Name, e.Value);
                Msg += str;
            };
        }


    }

   

    

    public static class TestDelegate
    {
        public static string Add(string a,string b)
        {
            return a;
        }
        
    }


    [DataBinding]
    public class TestA:TestBase
    {
        public TestA():base(true)
        {

        }

        [ColumnBinding]
        public string A { get; set; }

        [ColumnBinding]
        public string B { get; set; }

        [ColumnBinding]
        public string C { get; set; }
    }

    [DataBinding("1")]
    public class TestC:TestBase
    {
        public static object Create(object obj)
        {
            return new TestA();
        }

        public TestC():base(false)
        {

        }

        [ColumnBinding(new string[] { },nameof(Create),typeof(TestC),"GroupId",Order = -1)]
        public TestA TestA { get; set; }

        [ColumnBinding]
        public string Test { get; set; }

        [ColumnBinding]
        public string A { get; set; }

        [ColumnBinding]
        public string B { get; set; }

        [ColumnBinding]
        public string C { get; set; }

        [ColumnBinding]
        public string D { get; set; }

        [ColumnBinding]
        public string Name { get; set; }
    }
}
