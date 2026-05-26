using QLTB.Helpers;
using QLTB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class SettingNotificationViewModel : BaseViewModel
    {
        #region Properties

        private bool _notifyOverdueDevice;
        public bool NotifyOverdueDevice
        {
            get => _notifyOverdueDevice;
            set { _notifyOverdueDevice = value; OnPropertyChanged(nameof(NotifyOverdueDevice)); }
        }

        private bool _notifyTodayMaintenance;
        public bool NotifyTodayMaintenance
        {
            get => _notifyTodayMaintenance;
            set { _notifyTodayMaintenance = value; OnPropertyChanged(nameof(NotifyTodayMaintenance)); }
        }

        private bool _notifyNewIncident;
        public bool NotifyNewIncident
        {
            get => _notifyNewIncident;
            set { _notifyNewIncident = value; OnPropertyChanged(nameof(NotifyNewIncident)); }
        }

        private bool _notifyByEmail;
        public bool NotifyByEmail
        {
            get => _notifyByEmail;
            set { _notifyByEmail = value; OnPropertyChanged(nameof(NotifyByEmail)); }
        }

        // Thống kê thông báo tổng quan
        private int _overdueCount;
        public int OverdueCount
        {
            get => _overdueCount;
            set { _overdueCount = value; OnPropertyChanged(nameof(OverdueCount)); }
        }

        private int _todayMaintenanceCount;
        public int TodayMaintenanceCount
        {
            get => _todayMaintenanceCount;
            set { _todayMaintenanceCount = value; OnPropertyChanged(nameof(TodayMaintenanceCount)); }
        }

        private int _unresolvedIncidentCount;
        public int UnresolvedIncidentCount
        {
            get => _unresolvedIncidentCount;
            set { _unresolvedIncidentCount = value; OnPropertyChanged(nameof(UnresolvedIncidentCount)); }
        }

        private bool _isSending;
        public bool IsSending
        {
            get => _isSending;
            set { _isSending = value; OnPropertyChanged(nameof(IsSending)); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        #endregion

        #region Commands

        public ICommand SendTestNotificationCommand { get; set; }
        public ICommand RefreshStatsCommand { get; set; }
        public ICommand SendOverdueAlertCommand { get; set; }

        #endregion

        public SettingNotificationViewModel()
        {
            // Giá trị mặc định - bật hết các loại thông báo
            NotifyOverdueDevice = true;
            NotifyTodayMaintenance = true;
            NotifyNewIncident = true;
            NotifyByEmail = true;

            LoadCommands();
            _ = LoadStatsAsync();
        }

        private void LoadCommands()
        {
            RefreshStatsCommand = new RelayCommand(async o =>
            {
                await LoadStatsAsync();
                StatusMessage = "Đã làm mới thống kê.";
            });

            SendTestNotificationCommand = new RelayCommand(async o =>
            {
                await SendTestEmailAsync();
            });

            SendOverdueAlertCommand = new RelayCommand(async o =>
            {
                if (OverdueCount == 0)
                {
                    MessageBox.Show("Không có thiết bị nào quá hạn bảo trì.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                await SendOverdueAlertEmailAsync();
            });
        }

        private async Task LoadStatsAsync()
        {
            try
            {
                using var context = new QuanLyVatTuContext();

                // Đếm thiết bị quá hạn bảo trì
                var listThietBi = await context.ChiTietThietBis
                    .Where(x => x.TinhTrang == "Tốt")
                    .ToListAsync();

                var listGocThietBi = await context.ThietBis.ToListAsync();
                var listBaoTri = await context.BaoTris.ToListAsync();

                int overdueCount = 0;
                int todayCount = 0;
                DateTime today = DateTime.Today;

                foreach (var tb in listThietBi)
                {
                    var tbGoc = listGocThietBi.FirstOrDefault(x => x.IdthietBi == tb.IdthietBi);
                    var bt = listBaoTri
                        .Where(x => x.ChiTietBaoTris.Any(ct => ct.IdthietBi == tb.IdthietBi && ct.SoSeri == tb.SoSeri))
                        .OrderByDescending(x => x.NgayBaoTri)
                        .FirstOrDefault();

                    if (tbGoc == null) continue;

                    DateTime? baseDate = bt?.NgayBaoTri ?? tbGoc.NgayNhapThietBi;

                    if (baseDate.HasValue && tbGoc.BaoHanhDinhKy.HasValue)
                    {
                        double val = (double)tbGoc.BaoHanhDinhKy.Value;
                        int valInt = tbGoc.BaoHanhDinhKy.Value;

                        DateTime nextDue = tbGoc.DonViThoiGian switch
                        {
                            0 => baseDate.Value.AddMinutes(val),
                            1 => baseDate.Value.AddHours(val),
                            2 => baseDate.Value.AddDays(val),
                            3 => baseDate.Value.AddMonths(valInt),
                            4 => baseDate.Value.AddYears(valInt),
                            _ => baseDate.Value
                        };

                        if (nextDue.Date == today) todayCount++;
                        else if (nextDue.Date < today) overdueCount++;
                    }
                }

                OverdueCount = overdueCount;
                TodayMaintenanceCount = todayCount;

                UnresolvedIncidentCount = await context.BaoCaoSuaChuas
                    .CountAsync(x => x.TrangThai != "Đã giải quyết");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải thống kê thông báo: {ex.Message}");
            }
        }

        private async Task SendTestEmailAsync()
        {
            if (IsSending) return;
            IsSending = true;
            StatusMessage = "Đang gửi email kiểm tra...";

            try
            {
                string content = $@"
                    <p>Đây là email kiểm tra từ hệ thống Quản Lý Thiết Bị.</p>
                    <p>Các cài đặt thông báo hiện tại:</p>
                    <ul>
                        <li>Thiết bị quá hạn bảo trì: <strong>{(NotifyOverdueDevice ? "Bật" : "Tắt")}</strong></li>
                        <li>Bảo trì hôm nay: <strong>{(NotifyTodayMaintenance ? "Bật" : "Tắt")}</strong></li>
                        <li>Sự cố mới: <strong>{(NotifyNewIncident ? "Bật" : "Tắt")}</strong></li>
                    </ul>
                    <p>Thời điểm kiểm tra: <strong>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</strong></p>";

                await EmailService.SendEmail(KeyData.AdminEmail, content);

                StatusMessage = $"Đã gửi email kiểm tra thành công đến {KeyData.AdminEmail}";
                MessageBox.Show($"Email kiểm tra đã được gửi đến {KeyData.AdminEmail}",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Gửi thất bại: {ex.Message}";
                MessageBox.Show($"Không thể gửi email:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSending = false;
            }
        }

        private async Task SendOverdueAlertEmailAsync()
        {
            if (IsSending) return;
            IsSending = true;
            StatusMessage = "Đang gửi cảnh báo quá hạn...";

            try
            {
                string content = $@"
                    <p>Hệ thống phát hiện có <strong style='color:red;'>{OverdueCount} thiết bị</strong> đã quá hạn bảo trì định kỳ.</p>
                    <p>Ngoài ra còn có <strong>{TodayMaintenanceCount} thiết bị</strong> cần bảo trì trong ngày hôm nay.</p>
                    <p>Vui lòng truy cập hệ thống để kiểm tra và lên kế hoạch bảo trì kịp thời.</p>
                    <br/>
                    <p style='color:#888;'>Thời điểm gửi: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>";

                await EmailService.SendEmail(KeyData.AdminEmail, content);

                StatusMessage = "Đã gửi cảnh báo quá hạn thành công.";
                MessageBox.Show("Cảnh báo quá hạn bảo trì đã được gửi đến admin!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Gửi thất bại: {ex.Message}";
                MessageBox.Show($"Không thể gửi email cảnh báo:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSending = false;
            }
        }
    }
}