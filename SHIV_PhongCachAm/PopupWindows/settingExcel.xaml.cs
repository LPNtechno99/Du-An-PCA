using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SHIV_PhongCachAm.PopupWindows
{
    /// <summary>
    /// Interaction logic for settingExcel.xaml
    /// </summary>
    public partial class settingExcel : Window
    {
        public delegate void outputExcelLink(string bCode);
        public event outputExcelLink ExcelLinkChange;

        private string tempOutput;
        public settingExcel()
        {
            InitializeComponent();
        }

        private void btnBrowerExcel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog tempOpenfile = new Microsoft.Win32.OpenFileDialog();
            tempOpenfile.DefaultExt = ".xlsm";
            Nullable<bool> result = tempOpenfile.ShowDialog();

            if (result == true)
            {
                string filename = tempOpenfile.FileName;
                txtExcelLink.Text = filename;
                if (ExcelLinkChange != null) ExcelLinkChange(filename);
                this.Close();
            }
        }

        private void btnSave_MouseDown(object sender, MouseButtonEventArgs e)
        {
            tempOutput = txtExcelLink.Text;
        }

        private void formClosing_(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void btnSave_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("123");
        }
    }
}
