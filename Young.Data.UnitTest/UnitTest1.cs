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
            
            DataEngine de = new DataEngine();

            de.IsUsingSampleData = true;
            var sample = de.Create<SampleDataTest>();
            Assert.AreEqual(sample.Age, 11);
            var sample1 = de.Create<SampleDataB>();
            Assert.AreEqual(sample1.Age, 11);
        }
    }

    [DataBinding]
    public class SampleDataTest
    {
        [ColumnBinding]
        [SingleSampleData("Test")]
        public string Name { get; set; }

        [ColumnBinding(Directory = DataDirectory.Output)]
        [SingleSampleData("11")]
        public int Age { get; set; }
    }

    [DataBinding]
    public class SampleDataB
    {
        [ColumnBinding]
        public int Age { get; set; }
    }

  
}
