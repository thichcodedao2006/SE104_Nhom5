using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLTB.Helpers
{
    public class CloudinaryService
    {
        private static Cloudinary cloudinary;

        static CloudinaryService()
        {
            // Thay thế bằng thông tin thực tế của bạn
            Account account = new Account(
                "dqehmxzq6",
                "985482741258345",
                "2KHq0iH9ewOZDW8e2k6Oecj2RVM"
            );
            cloudinary = new Cloudinary(account);
        }

        // Hàm này nhận đường dẫn file trên máy tính và trả về Link ảnh trên Cloud
        public static async Task<string> UploadImageAsync(string filePath, string folderName, string imageId)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(filePath),
                Folder = folderName,
                PublicId = imageId,         // Gắn tên cố định cho ảnh
                Overwrite = true,           // Mặc định luôn ghi đè
                Invalidate = true,          // Bắt buộc: Xóa cache mạng để WPF load ảnh mới lên ngay lập tức
                UseFilename = false,        // Bỏ qua tên file gốc (tên file gốc trên máy tính là gì không quan trọng)
                UniqueFilename = false      // Tắt tự sinh đuôi ngẫu nhiên
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams);

            // Trả về Link ảnh
            return uploadResult.SecureUrl.ToString();
        }

        public static string GetImageUrl(string folderName, string imageId)
        {
            // Sử dụng API của Cloudinary để tự động sinh ra link ảnh chuẩn
            // Secure(true) để lấy link HTTPS (Bảo mật, WPF sẽ load mượt hơn)
            string publicId = $"{folderName}/{imageId}";
            return cloudinary.Api.UrlImgUp.Secure(true).BuildUrl(publicId);
        }
    }
}
