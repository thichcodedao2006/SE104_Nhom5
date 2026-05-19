using Microsoft.EntityFrameworkCore;
using QLTB.Models;
using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace QLTB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private DispatcherTimer _cleanupTimer;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Khởi tạo Timer
            _cleanupTimer = new DispatcherTimer();

            // 2. Cài đặt thời gian lặp lại là 5 phút
            _cleanupTimer.Interval = TimeSpan.FromMinutes(5);

            // 3. Gắn sự kiện: Mỗi khi hết 5 phút thì gọi hàm CleanupExpiredOtps
            _cleanupTimer.Tick += async (sender, args) => await CleanupExpiredOtps();

            // 4. Bấm nút "Start" cho đồng hồ chạy
            _cleanupTimer.Start();
        }

        private async Task CleanupExpiredOtps()
        {
            try
            {
                // ⚠️ LƯU Ý QUAN TRỌNG: Phải tạo một DbContext MỚI hoàn toàn ở đây.
                // Không dùng chung DataProvider.Instance.DB vì nếu bộ đếm giờ chạy trùng 
                // lúc người dùng đang thao tác chức năng khác sẽ gây lỗi "đa luồng" của EF Core.
                using (var context = new QuanLyVatTuContext())
                {
                    // Lấy ra các mã OTP đã quá hạn hoặc đã được sử dụng
                    var garbageOtps = await context.FogetPasses
                        .Where(x => x.ExpiredTime < DateTime.Now)
                        .ToListAsync();

                    if (garbageOtps.Any())
                    {
                        // Xóa khỏi DB và lưu lại
                        context.FogetPasses.RemoveRange(garbageOtps);
                        await context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Chỗ này chạy ngầm nên nếu có lỗi (ví dụ rớt mạng) thì cứ im lặng bỏ qua, 
                // hoặc bạn có thể ghi log ra file txt để theo dõi.
                Console.WriteLine("Lỗi khi dọn rác OTP: " + ex.Message);
            }
        }
    }

}
