using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
namespace QLTB.Helpers
{
    public class EmailService
    {
        private static string SenderEmail = KeyData.CompanyEmail;
        private static string AppPassword = "cjwc yzup yygz zail";

        public static async Task SendEmail(string ReceiveEmail, string Content)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Hệ Thống Thông Báo", SenderEmail));
            message.To.Add(new MailboxAddress("", ReceiveEmail));
            message.Subject = "Thông Báo Hệ Thống"; // Tiêu đề xuất hiện ở hòm thư đến

            // 2. Thiết kế HTML Template với CSS tông màu Xanh Dương (Blue Theme)
            string htmlTemplate = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <style>
                body {{
                    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                    background-color: #f4f7f6;
                    color: #333333;
                    margin: 0;
                    padding: 20px;
                }}
                .email-container {{
                    max-width: 600px;
                    margin: 0 auto;
                    background-color: #ffffff;
                    border-radius: 8px;
                    overflow: hidden;
                    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
                    border-top: 5px solid #0056b3;
                }}
                .email-header {{
                    background-color: #0056b3;
                    padding: 25px;
                    text-align: center;
                    font-size: 22px;
                    font-weight: bold;
                    color: #ffffff;
                    letter-spacing: 1px;
                }}
                .email-body {{
                    padding: 30px;
                    line-height: 1.6;
                    font-size: 15px;
                    color: #444444;
                }}
                .email-footer {{
                    text-align: center;
                    padding: 20px;
                    font-size: 12px;
                    color: #777777;
                    background-color: #f8f9fa;
                    border-top: 1px solid #eeeeee;
                }}
                /* Class hỗ trợ nếu bạn muốn chèn nút bấm trong Content */
                .blue-button {{
                    display: inline-block;
                    padding: 10px 20px;
                    background-color: #0056b3;
                    color: #ffffff !important;
                    text-decoration: none;
                    font-weight: bold;
                    border-radius: 4px;
                    margin: 15px 0;
                }}
            </style>
        </head>
        <body>
            <div class='email-container'>
                <div class='email-header'>
                    THÔNG BÁO
                </div>
                <div class='email-body'>
                    {Content}
                </div>
                <div class='email-footer'>
                    &copy; {DateTime.Now.Year} Hệ thống quản lý tự động.<br>
                    Vui lòng không trả lời trực tiếp email này.
                </div>
            </div>
        </body>
        </html>";

            // 3. Đưa HTML vào nội dung Mail
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = htmlTemplate;
            message.Body = bodyBuilder.ToMessageBody();

            // 4. Tiến hành kết nối và gửi qua MailKit
            using (var client = new SmtpClient())
            {
                try
                {
                    // Bỏ qua kiểm tra chứng chỉ SSL nếu chạy thử nghiệm local
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    // Kết nối tới SMTP Server của Gmail
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                    // Xác thực bằng Email gửi và Mật khẩu ứng dụng (App Password)
                    await client.AuthenticateAsync(SenderEmail, AppPassword);

                    // Gửi mail bất đồng bộ
                    await client.SendAsync(message);
                    Console.WriteLine("Gửi email thành công!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Có lỗi xảy ra khi gửi email: {ex.Message}");
                }
                finally
                {
                    // Ngắt kết nối an toàn sau khi xong việc
                    await client.DisconnectAsync(true);
                }
            }
        }

    public static async Task SendOtpEmailAsync(string ReceiveEmail, string otpCode)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Hệ Thống Xác Thực", SenderEmail));
            message.To.Add(new MailboxAddress("", ReceiveEmail));
            message.Subject = "Mã OTP Xác Thực Tài Khoản";

            // Nhúng chuỗi HTML/CSS vào biến (Inject biến otpCode vào đúng vị trí)
            string htmlTemplate = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <style>
            body {{ font-family: 'Segoe UI', Tahoma, sans-serif; background-color: #f4f6f9; margin: 0; padding: 30px; }}
            .email-container {{ max-width: 500px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 15px rgba(0, 86, 179, 0.08); border-top: 6px solid #0056b3; }}
            .email-header {{ background: linear-gradient(135deg, #0056b3, #007bff); padding: 30px 20px; text-align: center; color: #ffffff; }}
            .email-header h2 {{ margin: 0; font-size: 22px; letter-spacing: 1px; text-transform: uppercase; font-weight: 600; }}
            .email-body {{ padding: 35px 30px; line-height: 1.6; font-size: 15px; color: #333333; }}
            .otp-box {{ background-color: #f0f4f8; border: 1px dashed #0056b3; border-radius: 8px; padding: 15px; text-align: center; margin: 25px 0; }}
            .otp-code {{ font-size: 36px; font-weight: bold; color: #0056b3; letter-spacing: 8px; font-family: 'Courier New', Courier, monospace; }}
            .warning-note {{ color: #c92a2a; font-weight: 500; font-size: 13.5px; background-color: #fff5f5; padding: 12px 15px; border-left: 4px solid #ffc9c9; border-radius: 4px; }}
            .email-footer {{ text-align: center; padding: 20px; font-size: 12px; color: #888888; background-color: #fafbfc; border-top: 1px solid #eaedf1; }}
        </style>
    </head>
    <body>
        <div class='email-container'>
            <div class='email-header'>
                <h2>Xác Thực Tài Khoản</h2>
            </div>
            <div class='email-body'>
                <p>Đây là mã OTP để xác thực tài khoản.</p>
                
                <div class='otp-box'>
                    <div class='otp-code'>{otpCode}</div>
                </div>
                
                <div class='warning-note'>
                    ⚠️ <strong>Lưu ý:</strong> Mã OTP sẽ tự hủy sau 5p.
                </div>
            </div>
            <div class='email-footer'>
                &copy; {DateTime.Now.Year} Hệ Thống Quản Lý.<br>
                Email tự động, vui lòng không phản hồi.
            </div>
        </div>
    </body>
    </html>";

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlTemplate };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(SenderEmail, AppPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
