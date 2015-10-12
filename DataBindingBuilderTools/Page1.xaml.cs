using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Young.Data;
using Young.Data.Attributes;
using WinForm = System.Windows.Forms;

namespace DataBindingBuilderTools
{
    /// <summary>
    /// Page1.xaml 的交互逻辑
    /// </summary>
    public partial class Page1 : Page
    {
        ObservableCollection<Assembly> asms = new ObservableCollection<Assembly>();

        ObservableCollection<Type> selectedTypes = new ObservableCollection<Type>();

        public Page1()
        {
            InitializeComponent();
            lv_Assembly.DataContext = asms;
            lv_SelectedComponents.DataContext = selectedTypes;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            MessageBox.Show(string.Format("Can't find assembly {0}",args.Name));
            return getAssembly();
            
        }

        private void mi_Add_Click(object sender, RoutedEventArgs e)
        {
            var tp = lv_Components.SelectedItem as Type;
            if (tp != null && selectedTypes.Contains(tp) == false)
                selectedTypes.Add(tp);
        }

        private void btn_Create_Click(object sender, RoutedEventArgs e)
        {
            DataSet ds = new DataSet();
            DataTable shareTable = new DataTable("Shared");
            ds.Tables.Add(shareTable);
            shareTable.Columns.Add("Id");
            DataRow dr = shareTable.Rows.Add("1");
            foreach (var t in selectedTypes)
            {
                DataBindingAttribute dba = t.GetCustomAttribute<DataBindingAttribute>();
                string tableName = dba.TableName == null ? t.Name : dba.TableName;
                foreach (var prop in t.GetProperties().Where(p => p.GetCustomAttribute(typeof(ColumnBindingAttribute), true) != null))
                {
                    ColumnBindingAttribute attr = prop.GetCustomAttribute(typeof(ColumnBindingAttribute), true) as ColumnBindingAttribute;
                    if (attr.ColNames == null || attr.ColNames.Count() < 2)
                    {
                        string colName = attr.ColNames == null ? prop.Name : attr.ColNames.First();
                        if (!shareTable.Columns.Contains(colName))
                        {
                            shareTable.Columns.Add(colName);
                            SingleSampleDataAttribute sampleAttr = prop.GetCustomAttribute(typeof(SingleSampleDataAttribute), true) as SingleSampleDataAttribute;
                            if (sampleAttr != null)
                            {
                                shareTable.Rows[0][colName] = sampleAttr.Value;
                            }
                        }
                    }
                    else
                    {
                        DataTable newTable = null;
                        if (!ds.Tables.Contains(tableName))
                        {
                            newTable = new DataTable(tableName);
                            ds.Tables.Add(newTable);
                        }
                        if (newTable != null)
                        {
                            foreach (var s in attr.ColNames)
                            {
                                newTable.Columns.Add(s);
                            }
                        }
                    }
                }
                ExcelHelper.Current.Create(@"C:\Demo\1.xlsx");
                ExcelHelper.Current.WriteAll(ds);
                
                ExcelHelper.Current.Close();
            }
        }

        private Assembly getAssembly()
        {
            using (WinForm.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog())
            {
                if (ofd.ShowDialog() == WinForm.DialogResult.OK)
                {
                    try
                    {
                        Assembly asm = Assembly.LoadFile(ofd.FileName);
                        return asm;
                        
                    }
                    catch {
                        return null;
                    }
                }
                return null;
            }
        }


        private void btn_Add_Click(object sender, RoutedEventArgs e)
        {
            var asm = getAssembly();
            if(asm!=null)
            {
                asms.Add(asm);
            }
            
             
        }

        private void mi_Remove_Click(object sender, RoutedEventArgs e)
        {
            var tp = lv_SelectedComponents.SelectedItem as Type;
            if (tp != null)
                selectedTypes.Remove(tp);
        }
    }
}
