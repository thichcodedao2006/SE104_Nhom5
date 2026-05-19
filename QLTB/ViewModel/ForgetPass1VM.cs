using Microsoft.EntityFrameworkCore;
using QLTB.Data;
using QLTB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using QLTB.Models;
using QLTB.Helpers;

namespace QLTB.ViewModel
{
    public class ForgetPass1VM : BaseViewModel
    {
        private ForgetPass data;
        private bool IsCheckUsername = false;
        public ICommand ConfirmCommand {  get; set; }

        public Action<ForgetPass> DoneWithUsername { get; set; }
        public ForgetPass Data { get => data; set => data = value; }

        public ForgetPass1VM(ForgetPass data)
        {
            Data = data;
            ConfirmCommand = new RelayCommand<string>(o => true, async o =>
            {
                // kiểm tra Database 
                try
                {
                    if (!IsCheckUsername)
                    {
                        IsCheckUsername = true;
                        await CheckData();
                    }
                    
                }
                finally
                {
                    IsCheckUsername = false;
                }
            });
        }

        private async Task CheckData()
        {
            var tk = await DataProvider.Instance.DB.TaiKhoans.FirstOrDefaultAsync(x => x.TenTaiKhoan == Data.Username);
            if (tk == null)
            {
                // khong ton tai tai khoan
                MessageBox.Show("Không tồn tại tài khoản.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            MessageBox.Show("Tài khoản hợp lệ. Xin hãy chờ email được gửi.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            // Nếu tồn tại thì gen ra mã OTP rồi add vào bảng 
            int otp = RandomData.RandomNumber(1, 1000000);
            // gửi Email 
            TaiKhoan t = (TaiKhoan)tk;
            await EmailService.SendOtpEmailAsync(t.Email, otp.ToString());
            var NewForget = new FogetPass
            {
                Username = Data.Username,
                Otp = otp.ToString(),
            };
            DataProvider.Instance.DB.FogetPasses.Add(NewForget);
            DataProvider.Instance.DB.SaveChanges();
            DoneWithUsername.Invoke(Data);
        }
    }
}
