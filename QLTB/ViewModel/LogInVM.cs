using Microsoft.EntityFrameworkCore;
using QLTB.Data;
using QLTB.HashingData;
using QLTB.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class LogInVM
    {
        #region Property
        private string username;
        private string password;
        private bool IsCheckingLogIn = false;

        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        #endregion

        #region Command
        public ICommand SignInCommand { get; set; }
        public ICommand SignUpCommand { get; set; }
        public ICommand ForgetPassCommand {  get; set; }    

        public ICommand PasswordChangedCommand { get; set; }
        #endregion

        public LogInVM()
        {
            PasswordChangedCommand = new RelayCommand<PasswordBox>
            (
                (p) => true , 
                (p) =>
                {
                    Password = p.Password;
                }
                );
            SignInCommand = new RelayCommand<Window>
                (
                    (p) => CanLogIn(p), async (p) => await LogIn(p)

                );
            SignUpCommand = new RelayCommand<Window>
                (
                    (p) => true, (p) => OpenSignUp(p));
            ForgetPassCommand = new RelayCommand<Window>
                (
                    (p) => true, (p) => OpenForgetPass(p)
                );
        }

        #region ExecuteCommand
        private void OpenSignUp(Window w)
        {
            w.Hide();
            fmDangKy dk = new fmDangKy();
            dk.ShowDialog();
            w.Show();
        }

        private void OpenForgetPass(Window w)
        {
            w.Hide();
            fmQuenMatKhau mk = new fmQuenMatKhau();
            mk.ShowDialog();
            w.Show();
        }
            
        private bool CanLogIn(object p)
        {
            return !string.IsNullOrEmpty(Username) &&  Username.Length >0 && !string.IsNullOrEmpty(Password) &&  Password.Length > 0;   
        }

        private async Task LogIn(Window p)
        {
            if (IsCheckingLogIn) return;

            try // tránh lỗi văng 
            {
                if (!CheckValidPass())
                {
                    MessageBox.Show("Mật khẩu phải có độ dài ít nhất là 8 kí tự.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                IsCheckingLogIn = true;
                string hashpass = Security.HashPasswordSHA256(Password);
                var tk = await DataProvider.Instance.DB.TaiKhoans.FirstOrDefaultAsync(
                    x => x.TenTaiKhoan == Username && x.MatKhau == hashpass 

                    );
                if (tk != null)
                {
                    if (tk is TaiKhoan t)
                    {
                        if (t.DuocXacThuc == 1)
                        {

                            // Mở MainWindow và đóng form đăng nhập
                            MainWindow mainWindow = new MainWindow(t);
                            mainWindow.Show();

                            p.Close();
                        } else
                        {
                            MessageBox.Show("Tài khoản chưa được xác thực.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Sai tên tài khoản hoặc mật khẩu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // ĐÂY LÀ NƠI BẮT LỖI GÂY VĂNG APP
                // Nó sẽ hiển thị chính xác lý do tại sao EF Core không chạy được sau khi Scaffold
                string errorMsg = $"Lỗi kết nối cơ sở dữ liệu: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMsg += $"\nChi tiết: {ex.InnerException.Message}";
                }
                MessageBox.Show(errorMsg, "Lỗi Nghiêm Trọng", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsCheckingLogIn = false;
            }
            
           
        }

        private bool CheckValidPass()
        {
            return !(Password.Length < 8);
        }
        #endregion
    }
}
