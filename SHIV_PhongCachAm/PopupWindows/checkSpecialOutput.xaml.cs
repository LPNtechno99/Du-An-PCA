using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for checkSpecialOutput.xaml
    /// </summary>
    public partial class checkSpecialOutput : Window
    {
        public delegate void ConfirmSpecialOutput(string OutputInfo, string LeaderConfirm);
        public event ConfirmSpecialOutput EventConfirmButton;
        private ObservableCollection<specialOutputInfo> OutputInfoCollect;

        public string ConfirmPersonName { get; private set; }

        public checkSpecialOutput()
        {
            InitializeComponent();
            DataInitial();
        }

        private void DataInitial()
        {
            OutputInfoCollect = new ObservableCollection<specialOutputInfo>
            {
                new specialOutputInfo(){Id = 1, Info = "Sumitomo Heavy Vietnam - Special Output Infomation A"},
                new specialOutputInfo(){Id = 2, Info = "Sumitomo Heavy Vietnam - Special Output Infomation B"},
                new specialOutputInfo(){Id = 3, Info = "Sumitomo Heavy Vietnam - Special Output Infomation C"},
                new specialOutputInfo(){Id = 4, Info = "Sumitomo Heavy Vietnam - Special Output Infomation D"}
            };
            lblInfo001.DataContext = OutputInfoCollect;
            lblInfo002.DataContext = OutputInfoCollect;
            lblInfo003.DataContext = OutputInfoCollect;
        }

        /// <summary>
        /// Nhấn nút xác nhận - Mở hộp thoại nhập Password ẩn + tên người xác nhận
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string collectTempS = "";
            string leaderConfirm = "";
            // Cập nhật Password
            ConfirmPassword();
            leaderConfirm = ConfirmPersonName;
            // Cập nhật thông số lựa chọn
            if (lblInfo001.SelectedValue != null) collectTempS += (lblInfo001.SelectedValue as specialOutputInfo).Info;
            if ((lblInfo002.SelectedValue != null) && (lblInfo001.SelectedValue != lblInfo002.SelectedValue)) collectTempS += " - " + (lblInfo002.SelectedValue as specialOutputInfo).Info;
            if ((lblInfo003.SelectedValue != null) && (lblInfo001.SelectedValue != lblInfo003.SelectedValue) && (lblInfo002.SelectedValue != lblInfo003.SelectedValue)) collectTempS += " - " + (lblInfo003.SelectedValue as specialOutputInfo).Info;
            // Kiểm tra nếu đang được Follow thì gửi Event ra ngoài
            if (EventConfirmButton != null)
            {
                EventConfirmButton(collectTempS, leaderConfirm);
            }
            MessageBox.Show("Lỗi xuất đặc biệt : " + collectTempS + " --- Confirm by : " + leaderConfirm);
            this.Close();
        }

        private void ConfirmPassword()
        {
            passwordInput tempPassInput = new passwordInput();
            tempPassInput.eventPasswordConfirm += ProcessPassword;
            ConfirmPersonName = "";
            tempPassInput.ShowDialog();
        }

        private void ProcessPassword(string username)
        {
            ConfirmPersonName = username;
        }
    }
}
