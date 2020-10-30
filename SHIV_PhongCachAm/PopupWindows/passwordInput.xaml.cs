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
    /// Interaction logic for passwordInput.xaml
    /// </summary>
    public partial class passwordInput : Window
    {
        public delegate void ConfirmPassword(string username);
        public event ConfirmPassword eventPasswordConfirm;
        public passwordInput()
        {
            InitializeComponent();
        }

        private void BtnConfirmPass_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CheckPassword())
            {
                if (eventPasswordConfirm != null) eventPasswordConfirm(txtUserName.Text);
                this.Close();
            }
            else MessageBox.Show("Sai mật khẩu!!!");
        }

        // Hàm kiểm tra Password
        private bool CheckPassword()
        {
            if (txtPassword.Password == "0000") return true;
            if ((txtUserName.Text.ToLower() == "manager") && (txtPassword.Password == "1456")) return true;
            return false;
        }
    }
}
