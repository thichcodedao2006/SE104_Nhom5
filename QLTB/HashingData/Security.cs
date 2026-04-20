using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QLTB.HashingData
{
    public static class Security
    {
        // Hàm này nhận vào mật khẩu thường và trả về mã băm SHA256
        public static string HashPasswordSHA256(string rawPassword)
        {
            if (string.IsNullOrEmpty(rawPassword)) return "";

            using (SHA256 sha256 = SHA256.Create())
            {
                // Chuyển chuỗi thành mảng byte
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawPassword));

                // Chuyển mảng byte ngược lại thành chuỗi Hex (ký tự a-f, 0-9)
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
