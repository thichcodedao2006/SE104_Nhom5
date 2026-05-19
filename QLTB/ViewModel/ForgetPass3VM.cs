using Microsoft.EntityFrameworkCore;
using Microsoft.Xaml.Behaviors.Media;
using QLTB.Data;
using QLTB.Model;
using QLTB.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class ForgetPass3VM : BaseViewModel
    {
        private ForgetPass data;
        private string password;
        private string retypepass;
        public ICommand ConfirmPassCommand { get; set; }

        public ICommand PasswordChangedCommand { get; set; }

        public ICommand RetypePasswordChangedCommand { get; set; }

        public Action CloseWindow { get; set; }
        
        public ForgetPass3VM(ForgetPass data)
        {
            Data = data;


            ConfirmPassCommand = new RelayCommand<object>(
                (p) => ValidInput() , async (p) =>
                {
                    await UpdatePass();
                    
                }
                );

            PasswordChangedCommand = new RelayCommand<PasswordBox>
                (
                (p) => true,
                (p) => Password = p.Password
                );
            RetypePasswordChangedCommand = new  RelayCommand<PasswordBox>
                (
                (p) => true,
                (p) => Retypepass = p.Password
                );
        }

        public ForgetPass Data { get => data; set => data = value; }
        public string Password
        {
            get => password; set
            {
                password = value;
                OnPropertyChanged();
            }
        }

        public string Retypepass
        {
            get => retypepass; set
            { 
                retypepass = value;
                OnPropertyChanged();
            }
        }

        private bool ValidInput()
        {
            return (Password == Retypepass && Retypepass.Length > 0 && Password.Length > 0);
        }

        private async Task UpdatePass()
        {
            var tk = await DataProvider.Instance.DB.TaiKhoans.FirstOrDefaultAsync(x => x.TenTaiKhoan == Data.Username);
            if (tk != null)
            {
                tk.MatKhau = Password;
                await DataProvider.Instance.DB.SaveChangesAsync();
                MessageBox.Show("Cập nhật mật khẩu thành công", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow?.Invoke();
            }
        }
    }
}
