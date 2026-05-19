using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;
using QLTB.Data;
using QLTB.Helpers;
using QLTB.Model;
using QLTB.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class ForgetPass2VM : BaseViewModel
    {
        private ForgetPass data;

        public ICommand BackCommand {  get; set; }
        public ICommand NextCommand { get; set; }
        public ICommand ResendOTP {  get; set; }    
        public ForgetPass Data { get => data; set => data = value; }

        public Action<ForgetPass> BackDone {  get; set; }
        public Action<ForgetPass> NextDone { get; set; }


        public ForgetPass2VM(ForgetPass data)
        {
            Data = data;
            BackCommand = new RelayCommand<object>(o => true, o =>
            {
                //logic 
                BackDone.Invoke(Data);
            }
            );
            NextCommand = new RelayCommand<object>(o => true, async o =>
            {
                //logic
                await CheckOTP();
            }
            );
            ResendOTP = new RelayCommand<object>(o => true, async o =>
            {
                await Resend();
            }
            );
        }

        private async Task CheckOTP()
        {

            string otpInput = "";
            foreach (OtpChar o in Data.OtpList)
            {
                otpInput += o.Value;
            }
            var fp = await DataProvider.Instance.DB.FogetPasses.FirstOrDefaultAsync(x => x.Username == Data.Username && x.Otp == otpInput);
            if (fp != null) // nhập đúng 
            {
                NextDone.Invoke(Data);
            } else
            {
                MessageBox.Show("Mã otp không hợp lệ.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task Resend()
        {
            var tk = await DataProvider.Instance.DB.TaiKhoans.FirstOrDefaultAsync(x => x.TenTaiKhoan == Data.Username);
            if (tk != null && tk is TaiKhoan t)
            {
                int otp = RandomData.RandomNumber(1, 1000000);
                await EmailService.SendOtpEmailAsync(t.Email, otp.ToString());
                MessageBox.Show("Xin hãy kiểm tra email.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
           
        }

    }
}
