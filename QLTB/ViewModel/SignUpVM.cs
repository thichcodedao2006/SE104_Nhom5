using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net.Mail;
using System.Linq.Expressions;
using QLTB.Data;
using Microsoft.EntityFrameworkCore;
using QLTB.Models;
using QLTB.HashingData;
using QLTB.Helpers;

namespace QLTB.ViewModel
{
    public class SignUpVM
    {
        #region Property 
        private string username;
        private string realname;
        private string password;
        private string retypepass;
        private string phone;
        private string email;
        private bool IsSignUp = false;

        public string Username { get => username; set => username = value; }
        public string Realname { get => realname; set => realname = value; }
        public string Password { get => password; set => password = value; }
        public string Retypepass { get => retypepass; set => retypepass = value; }
        public string Phone { get => phone; set => phone = value; }
        public string Email { get => email; set => email = value; }
        #endregion

        #region Command 
        public ICommand PasswordChangedCommand { get; set; }

        public ICommand RetypePassChangedCommand { get; set; }

        public ICommand SignUpCommand { get; set; }
        #endregion

        public SignUpVM()
        {
            PasswordChangedCommand = new RelayCommand<PasswordBox>
                (
                    (p) => true, (p) => Password = p.Password
                );
            RetypePassChangedCommand = new RelayCommand<PasswordBox>
                (
                    (p) => true, (p) => Retypepass = p.Password
                );
            SignUpCommand = new RelayCommand<object>
                (
                    (p) => CanSignUp(p), async (p) =>  await SignUp(p)
                );
        }

        private bool CanSignUp(object t)
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(Retypepass)
                && !string.IsNullOrEmpty(Phone) && !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Realname)
                && Username.Length > 0 && Password.Length > 0 && Retypepass.Length > 0 && Realname.Length > 0
                && Phone.Length > 0 && Email.Length > 0;
        }

        private async Task SignUp(object t)
        {
            if (IsSignUp) return;
            try
            {
                IsSignUp = true;
                if (!await CheckInputCondition())
                {
                    return;
                }
                // đủ các điều kiện 
                string HashPass = Security.HashPasswordSHA256(Password);
                var NewAccount = new TaiKhoan()
                {
                    TenTaiKhoan = Username,
                    LoaiTaiKhoan = 2,
                    MatKhau = HashPass,
                    DuocXacThuc = 0,
                    Email = Email
                };
                DataProvider.Instance.DB.TaiKhoans.Add(NewAccount); // thêm vào Database TaiKhoan 
                var NewEmployee = new NhanVien()
                {
                    HoTen = Realname,
                    Sdt = Phone,
                    Email = Email,
                    ChuyenMon = "Sửa chữa",
                    TinhTrang = "Đang rảnh"
                };
                DataProvider.Instance.DB.NhanViens.Add(NewEmployee);    
                await DataProvider.Instance.DB.SaveChangesAsync();
                // gửi Email
                await EmailService.SendEmail(Email, "Cảm ơn bạn đã đăng kí tài khoản tại app. Xin hãy chờ sự phê duyệt từ người quản lý.");
                MessageBox.Show("Đăng kí thành công. Kiểm tra email đăng kí.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                await EmailService.SendEmail(KeyData.AdminEmail, "Có người đăng kí tài khoản mới. Hãy vào ứng dụng để kiểm tra.");

            }
            finally
            {
                IsSignUp = false;
            }
        }

        private async Task<bool> CheckInputCondition()
        {
            if (!ValidPass())
            {
                MessageBox.Show("Mật khẩu phải có độ dài ít nhất 8 kí tự.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (!ValidRetype())
            {
                MessageBox.Show("Mật khẩu được xác nhận không khớp.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (! await ValidName())
            {
                MessageBox.Show("Tên nhân viên đăng kí tài khoản đã tồn tại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (!ValidPhone())
            {
                MessageBox.Show("Số điện thoại có độ dài không hợp lệ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (! await ValidEmail())
            {
                MessageBox.Show("Email có định dạng không hợp lệ hoặc đã tồn tại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (!await ValidUsername())
            {
                MessageBox.Show("Tên đăng nhập đã tồn tại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            return true;
        }

        private async Task<bool> ValidUsername()
        {
            var tk = await DataProvider.Instance.DB.TaiKhoans.FirstOrDefaultAsync(x => x.TenTaiKhoan == Username);
            return tk == null;
        }

        private async  Task<bool> ValidEmail()
        {
            
            try
            {
                var address = new MailAddress(Email); // có thể null -> NullReference

                if (address.Address == Email)
                {
                    var tk = await DataProvider.Instance.DB.TaiKhoans.FirstOrDefaultAsync(x => x.Email == Email);
                    return tk == null; // không được tồn tại trùng Email
                } else
                {
                    return false;
                }
            }
            catch
            { return false; }

        }
        private bool ValidPass()
        {
            return Password.Length >= 8;
        }

        private bool ValidRetype()
        {
            return Password == Retypepass;
        }

        private bool ValidPhone()
        {
            return Phone.Length>= 9 && Phone.Length<=10;
        }

        private async Task<bool> ValidName()
        {
            var tk = await DataProvider.Instance.DB.NhanViens.FirstOrDefaultAsync(x => x.HoTen == Realname);
            return tk == null; // mỗi nhân viên chỉ có 1 tài khoản 
        }




    }
}
