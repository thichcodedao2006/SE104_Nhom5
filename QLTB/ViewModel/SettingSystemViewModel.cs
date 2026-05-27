using QLTB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class SettingSystemViewModel: BaseViewModel
    {
        public ICommand LogOutCommand { get; set; }

        public SettingSystemViewModel()
        {
            LoadCommand();
        }

        private void LoadCommand()
        {
            LogOutCommand = new RelayCommand<UserControl>
                (
                    p => true, p => LogOut(p)
                );
        }

        private void LogOut(UserControl uc)
        {
            try
            {
                // Xóa sạch toàn bộ Cache đang lưu vết các thực thể trong DbContext (nếu có)
                // Điều này đảm bảo phiên đăng nhập sau hoàn toàn sạch sẽ dữ liệu
                DataProvider.Instance.DB.ChangeTracker.Clear();
            }
            catch { /* Tránh crash nếu tầng DB chưa khởi tạo xong */ }
            Window p = Window.GetWindow ( uc );
            if ( p != null )
            {
                fmDangNhap dn = new fmDangNhap();
                dn.Show ();
                p.Close();
            }
        }
    }
}
