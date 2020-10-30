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
    /// Interaction logic for checkAmsacWD.xaml
    /// </summary>
    public partial class checkRotary : Window
    {
        public delegate void IsCheckedDelegate(string value);
        public event IsCheckedDelegate isCheckedEvent;
        private string inputDirection;
        public checkRotary()
        {
            InitializeComponent();
        }

        public checkRotary(string input):this()
        {
            inputDirection = input;
            lblTitle.Content = inputDirection;
        }

        private void btnOK_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isCheckedEvent != null) isCheckedEvent("OK");
            this.Close();
        }

        private void btnNG_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isCheckedEvent != null) isCheckedEvent("NG");
            this.Close();
        }

        private void FirstMethod(object sender, ExecutedRoutedEventArgs e)
        {
            if (isCheckedEvent != null) isCheckedEvent("OK");
            this.Close();
        }

        private void SecondMethod(object sender, ExecutedRoutedEventArgs e)
        {
            if (isCheckedEvent != null) isCheckedEvent("NG");
            this.Close();
        }
    }
}
