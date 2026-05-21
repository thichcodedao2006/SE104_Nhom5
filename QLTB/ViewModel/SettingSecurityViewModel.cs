using Microsoft.VisualBasic;
using QLTB.Data;
using QLTB.HashingData;
using QLTB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Converters;

namespace QLTB.ViewModel
{
    public class SettingSecurityViewModel: BaseViewModel
    {
        private TaiKhoan userAccount;
        public ICommand CurrentPasswordChangedCommand { get; set; }
        public ICommand NewPasswordChangedCommand { get; set; }

        public ICommand ReNewPasswordChangedCommand { get; set; }

        public ICommand UpdatePassCommand { get; set; }
        public TaiKhoan UserAccount { get => userAccount; set
            {
                userAccount = value;
                OnPropertyChanged(nameof(UserAccount));
            }
                }

        private string currentPass;
        private string newPass;
        private string renewPass;
        public SettingSecurityViewModel(TaiKhoan t)
        {
            UserAccount = t;
            LoadCommand();
        }

        private void LoadCommand()
        {
            CurrentPasswordChangedCommand = new RelayCommand<PasswordBox>
                (
                 p => true, p => currentPass = p.Password
                );
            NewPasswordChangedCommand = new RelayCommand<PasswordBox>
                (
                 p => true, p => newPass = p.Password
                 );
            ReNewPasswordChangedCommand= new RelayCommand<PasswordBox>
                (
                 p => true, p => renewPass = p.Password
                );
            UpdatePassCommand = new RelayCommand<object>
                (
                p => CheckCondition(), async p => await UpdatePass()
                );
        }

        private bool CheckCondition()
        {
            if (currentPass != null && currentPass.Length > 0 && newPass != null  && newPass.Length > 0 && renewPass != null && renewPass.Length > 0) return true;
            return false;
        }

        private async Task UpdatePass()
        {
            if (!CheckPassCondition())
            {
                return;
            }
            string hasPass = Security.HashPasswordSHA256(newPass);
            UserAccount.MatKhau = hasPass;
            DataProvider.Instance.DB.TaiKhoans.Update(UserAccount);
            await DataProvider.Instance.DB.SaveChangesAsync();
            MessageBox.Show("Cập nhật mật khẩu thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CheckPassCondition()
        {
            string hasPass = Security.HashPasswordSHA256(currentPass);
            if (hasPass != UserAccount.MatKhau)
            {
                MessageBox.Show("Mật khẩu hiện tại không khớp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (newPass.Length< 8 || renewPass.Length < 8)
            {
                MessageBox.Show("Mật khẩu phải có độ dài ít nharat 8 kí tự.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (newPass != renewPass)
            {
                MessageBox.Show("Mật khẩu mới được nhập lại không khớp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            return true;
            
        }


    }
}
