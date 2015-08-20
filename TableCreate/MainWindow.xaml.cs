using SAPAutomation.Framework;
using SAPAutomation.Framework.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using winform = System.Windows.Forms;
namespace TableCreate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string fileName = "";
            winform.OpenFileDialog ofd = new winform.OpenFileDialog();
            if(ofd.ShowDialog()==winform.DialogResult.OK)
            {
                tb_DllPath.Text = ofd.FileName;
                fileName = ofd.FileName;

                Assembly asm = loadDll(fileName);

                List<Type> items = new List<Type>();

                foreach(var tp in asm.GetTypes().Where(t=>t.IsSubclassOf(typeof(SAPGuiScreen))))
                {
                    var tbAt = tp.GetCustomAttributes(typeof(TableBindingAttribute),true).FirstOrDefault();
                    if(tbAt != null)
                    {
                        items.Add(tp);
                    }
                   
                }


            }
        }

        private Assembly loadDll(string fileName)
        {
            return Assembly.LoadFile(fileName);
        }
    }

   
}
