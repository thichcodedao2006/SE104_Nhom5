using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using QLTB.Helpers;
using QLTB.Models;

namespace QLTB.ViewModel
{
    public static class DataSyncService
    {
        public static event Action DataChanged;
        public static void NotifyDataChanged() => DataChanged?.Invoke();
    }

    public class MaintenanceDisplayItem
    {
        public int IdBaoTri { get; set; }
        public int IdThietBi { get; set; }
        public string TenThietBi { get; set; }
        public string SoSeri { get; set; }
        public string TenDichVu { get; set; }
        public double GiaDichVu { get; set; }
        public string GiaDichVuFormatted => GiaDichVu.ToString("N0") + " đ";
        public string TenNhanVien { get; set; }
        public DateTime? NgayBaoTri { get; set; }
        public string NgayBaoTriText => NgayBaoTri?.ToString("dd/MM/yyyy") ?? "Chưa có";
        public string DoUuTien { get; set; }
        public string TinhTrangBaoTri { get; set; }
        public string GhiChu { get; set; }
    }

    public class MaintenanceTaskViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<MaintenanceDisplayItem> _maintenances;
        public ObservableCollection<MaintenanceDisplayItem> Maintenances
        {
            get => _maintenances;
            set { _maintenances = value; OnPropertyChanged(nameof(Maintenances)); }
        }

        private ObservableCollection<MaintenanceDisplayItem> _filteredMaintenances;
        public ObservableCollection<MaintenanceDisplayItem> FilteredMaintenances
        {
            get => _filteredMaintenances;
            set { _filteredMaintenances = value; OnPropertyChanged(nameof(FilteredMaintenances)); }
        }

        private MaintenanceDisplayItem _selectedMaintenance;
        public MaintenanceDisplayItem SelectedMaintenance
        {
            get => _selectedMaintenance;
            set { _selectedMaintenance = value; OnPropertyChanged(nameof(SelectedMaintenance)); }
        }

        private string _selectedStatusFilter = "Tất cả trạng thái";
        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set { _selectedStatusFilter = value; OnPropertyChanged(nameof(SelectedStatusFilter)); FilterMaintenances(); }
        }

        private string _selectedPriorityFilter = "Tất cả mức độ";
        public string SelectedPriorityFilter
        {
            get => _selectedPriorityFilter;
            set { _selectedPriorityFilter = value; OnPropertyChanged(nameof(SelectedPriorityFilter)); FilterMaintenances(); }
        }

        private string _selectedTechnicianFilter = "Tất cả kỹ thuật viên";
        public string SelectedTechnicianFilter
        {
            get => _selectedTechnicianFilter;
            set { _selectedTechnicianFilter = value; OnPropertyChanged(nameof(SelectedTechnicianFilter)); FilterMaintenances(); }
        }

        private ObservableCollection<string> _technicianList;
        public ObservableCollection<string> TechnicianList
        {
            get => _technicianList;
            set { _technicianList = value; OnPropertyChanged(nameof(TechnicianList)); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; FilterMaintenances(); OnPropertyChanged(nameof(SearchText)); }
        }

        public int TotalCount => Maintenances?.Count ?? 0;
        public int ProcessingCount => Maintenances?.Count(m => m.TinhTrangBaoTri == "Đang xử lý") ?? 0;
        public int CompletedCount => Maintenances?.Count(m => m.TinhTrangBaoTri == "Hoàn thành") ?? 0;
        public int OverdueCount => Maintenances?.Count(m => m.TinhTrangBaoTri == "Quá hạn") ?? 0;

        public ICommand DeleteMaintenanceCommand { get; set; }
        public ICommand CompleteTaskCommand { get; set; }
        public ICommand TriggerFilterCommand { get; set; }
        public ICommand EditTaskCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        public MaintenanceTaskViewModel()
        {
            TechnicianList = new ObservableCollection<string> { "Tất cả kỹ thuật viên" };
            _ = LoadMaintenancesAsync();

            TriggerFilterCommand = new RelayCommand(o => FilterMaintenances());
            RefreshCommand = new RelayCommand(async o => await LoadMaintenancesAsync());

            DeleteMaintenanceCommand = new RelayCommand(async o =>
            {
                if (SelectedMaintenance == null) return;

                var result = MessageBox.Show(
                    "Bạn có chắc chắn muốn xóa toàn bộ phiếu bảo trì này?",
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;

                try
                {
                    using var db = new QuanLyVatTuContext();

                    // Xóa tất cả ChiTietBaoTri thuộc phiếu
                    var childDetails = await db.ChiTietBaoTris
                        .Where(ct => ct.IdbaoTri == SelectedMaintenance.IdBaoTri)
                        .ToListAsync();
                    db.ChiTietBaoTris.RemoveRange(childDetails);

                    // Lấy phiếu bảo trì để biết nhân viên
                    var dbMaintenance = await db.BaoTris
                        .FirstOrDefaultAsync(b => b.IdbaoTri == SelectedMaintenance.IdBaoTri);

                    if (dbMaintenance != null)
                    {
                        int? staffId = dbMaintenance.IdnhanVien;
                        db.BaoTris.Remove(dbMaintenance);
                        await db.SaveChangesAsync();

                        if (staffId.HasValue)
                            await UpdateStaffStatusAsync(db, staffId.Value);
                    }
                    else
                    {
                        await db.SaveChangesAsync();
                    }

                    await LoadMaintenancesAsync();
                    DataSyncService.NotifyDataChanged();

                    MessageBox.Show("Xóa phiếu bảo trì thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa:\n{ex.Message}", "Lỗi hệ thống",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            // FIX: thay ExecuteSqlRawAsync bằng EF Core thuần
            CompleteTaskCommand = new RelayCommand(async o =>
            {
                if (o is not MaintenanceDisplayItem item) return;

                try
                {
                    using var db = new QuanLyVatTuContext();

                    // 1. Tìm ChiTietBaoTri cụ thể theo (IDBaoTri, IDThietBi, SoSeri)
                    var chiTiet = await db.ChiTietBaoTris
                        .FirstOrDefaultAsync(ct =>
                            ct.IdbaoTri == item.IdBaoTri &&
                            ct.IdthietBi == item.IdThietBi &&
                            ct.SoSeri == item.SoSeri);

                    if (chiTiet == null)
                    {
                        MessageBox.Show("Không tìm thấy hạng mục bảo trì!", "Lỗi",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // 2. Đánh dấu hoàn thành hạng mục này
                    chiTiet.TienDo = "Hoàn thành";
                    chiTiet.KetQua = $"Hoàn thành lúc {DateTime.Now:dd/MM/yyyy HH:mm}";
                    db.Entry(chiTiet).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    // 3. Kiểm tra toàn bộ hạng mục của phiếu đã xong chưa
                    var allDetailsInTicket = await db.ChiTietBaoTris
                        .Where(ct => ct.IdbaoTri == item.IdBaoTri)
                        .ToListAsync();

                    bool isTicketFinished = allDetailsInTicket.All(ct => ct.TienDo == "Hoàn thành");

                    if (isTicketFinished)
                    {
                        // 4. Nếu tất cả xong → cập nhật phiếu BaoTri
                        var dbMaster = await db.BaoTris
                            .FirstOrDefaultAsync(b => b.IdbaoTri == item.IdBaoTri);

                        if (dbMaster != null)
                        {
                            dbMaster.TinhTrangBaoTri = "Hoàn thành";
                            db.Entry(dbMaster).State = EntityState.Modified;
                            await db.SaveChangesAsync();

                            // 5. Cập nhật trạng thái các thiết bị liên quan
                            foreach (var ct in allDetailsInTicket)
                            {
                                var tbDetail = await db.ChiTietThietBis
                                    .FirstOrDefaultAsync(tb => tb.IdthietBi == ct.IdthietBi && tb.SoSeri == ct.SoSeri);
                                if (tbDetail != null && tbDetail.TinhTrang == "Đang bảo trì")
                                {
                                    tbDetail.TinhTrang = "Tốt";
                                    db.Entry(tbDetail).State = EntityState.Modified;
                                }
                            }
                            await db.SaveChangesAsync();

                            // 6. Trả nhân viên về "Đang rảnh"
                            if (dbMaster.IdnhanVien.HasValue)
                                await UpdateStaffStatusAsync(db, dbMaster.IdnhanVien.Value);
                        }
                    }

                    await LoadMaintenancesAsync();
                    DataSyncService.NotifyDataChanged();

                    MessageBox.Show(
                        isTicketFinished
                            ? "Phiếu bảo trì đã hoàn thành! Trạng thái thiết bị và nhân viên đã được cập nhật."
                            : "Đã cập nhật hoàn thành hạng mục thiết bị!",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi cập nhật:\n{ex.Message}", "Lỗi hệ thống",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            EditTaskCommand = new RelayCommand(async o =>
            {
                if (o is not MaintenanceDisplayItem item) return;

                var editVM = new MaintenanceEditViewModel(item);
                var editView = new QLTB.UserControlFolder.Maintenance.MaintenanceEditFormView
                {
                    DataContext = editVM
                };

                Window window = new Window
                {
                    Content = editView,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true
                };
                window.ShowDialog();

                if (!editVM.IsSaved) return;

                try
                {
                    using var db = new QuanLyVatTuContext();

                    // FIX: thay ExecuteSqlRawAsync bằng EF
                    var chiTiet = await db.ChiTietBaoTris
                        .FirstOrDefaultAsync(ct =>
                            ct.IdbaoTri == item.IdBaoTri &&
                            ct.IdthietBi == item.IdThietBi &&
                            ct.SoSeri == item.SoSeri);

                    if (chiTiet != null)
                    {
                        string newStatus = editVM.TinhTrangBaoTri?.ToString()
                            .Replace("System.Windows.Controls.ComboBoxItem: ", "");
                        chiTiet.TienDo = newStatus;
                        db.Entry(chiTiet).State = EntityState.Modified;
                    }

                    var dbMaster = await db.BaoTris
                        .FirstOrDefaultAsync(b => b.IdbaoTri == item.IdBaoTri);

                    if (dbMaster != null)
                    {
                        int? oldStaffId = dbMaster.IdnhanVien;

                        dbMaster.NgayBaoTri = editVM.NgayBaoTri;
                        dbMaster.DoUuTien = editVM.DoUuTien;
                        dbMaster.IdnhanVien = editVM.SelectedStaffId;
                        db.Entry(dbMaster).State = EntityState.Modified;

                        await db.SaveChangesAsync();

                        // Cập nhật trạng thái cả nhân viên cũ lẫn mới
                        if (oldStaffId.HasValue && oldStaffId != editVM.SelectedStaffId)
                            await UpdateStaffStatusAsync(db, oldStaffId.Value);
                        if (editVM.SelectedStaffId.HasValue)
                            await UpdateStaffStatusAsync(db, editVM.SelectedStaffId.Value);
                    }
                    else
                    {
                        await db.SaveChangesAsync();
                    }

                    await LoadMaintenancesAsync();
                    DataSyncService.NotifyDataChanged();

                    MessageBox.Show("Cập nhật thông tin hạng mục thành công!", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi hệ thống khi lưu:\n{ex.Message}", "Lỗi SQL",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        // Logic cập nhật trạng thái nhân viên dựa trên công việc còn lại
        private async Task UpdateStaffStatusAsync(QuanLyVatTuContext db, int staffId)
        {
            bool hasActiveTasks = await db.BaoTris
                .AnyAsync(b => b.IdnhanVien == staffId
                            && b.TinhTrangBaoTri != null
                            && b.TinhTrangBaoTri.Trim().ToLower() != "hoàn thành");

            var staff = await db.NhanViens
                .FirstOrDefaultAsync(n => n.IdnhanVien == staffId);

            if (staff == null) return;

            string newStatus = hasActiveTasks ? "Đang bận" : "Đang rảnh";
            if (staff.TinhTrang == newStatus) return;

            staff.TinhTrang = newStatus;
            db.Entry(staff).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        private async Task LoadMaintenancesAsync()
        {
            try
            {
                using var db = new QuanLyVatTuContext();

                var rawList = await db.ChiTietBaoTris
                    .Select(ct => new MaintenanceDisplayItem
                    {
                        IdBaoTri = ct.IdbaoTri,
                        IdThietBi = ct.IdthietBi,
                        TenThietBi = ct.ChiTietThietBi != null && ct.ChiTietThietBi.IdthietBiNavigation != null
                            ? ct.ChiTietThietBi.IdthietBiNavigation.TenThietBi
                            : "Thiết bị đã xóa",
                        SoSeri = ct.SoSeri,
                        TenDichVu = ct.IddichVuNavigation != null
                            ? ct.IddichVuNavigation.TenDichVu
                            : "Không rõ",
                        GiaDichVu = ct.IddichVuNavigation != null
                            ? (double)ct.IddichVuNavigation.GiaDichVu
                            : 0,
                        TenNhanVien = ct.IdbaoTriNavigation != null && ct.IdbaoTriNavigation.IdnhanVienNavigation != null
                            ? ct.IdbaoTriNavigation.IdnhanVienNavigation.HoTen
                            : "Chưa phân công",
                        NgayBaoTri = ct.IdbaoTriNavigation != null
                            ? ct.IdbaoTriNavigation.NgayBaoTri
                            : null,
                        DoUuTien = ct.IdbaoTriNavigation != null
                            ? ct.IdbaoTriNavigation.DoUuTien
                            : "Thấp",
                        TinhTrangBaoTri = ct.TienDo ?? (ct.IdbaoTriNavigation != null
                            ? ct.IdbaoTriNavigation.TinhTrangBaoTri
                            : "Đang xử lý"),
                        GhiChu = ct.GhiChuThietBi
                    })
                    .ToListAsync();

                Maintenances = new ObservableCollection<MaintenanceDisplayItem>(rawList);

                var staffNames = await db.NhanViens
                    .Select(nv => nv.HoTen)
                    .Distinct()
                    .ToListAsync();

                TechnicianList = new ObservableCollection<string>(
                    staffNames.Prepend("Tất cả kỹ thuật viên")
                );

                FilterMaintenances();
                RefreshStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải danh sách bảo trì:\n{ex.Message}", "Lỗi Dữ Liệu",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterMaintenances()
        {
            if (Maintenances == null) return;

            var result = Maintenances.AsEnumerable();

            if (!string.IsNullOrEmpty(SearchText))
                result = result.Where(m =>
                    (m.TenThietBi != null && m.TenThietBi.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (m.SoSeri != null && m.SoSeri.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (m.TenNhanVien != null && m.TenNhanVien.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));

            if (SelectedStatusFilter != "Tất cả trạng thái")
                result = result.Where(m => m.TinhTrangBaoTri == SelectedStatusFilter);

            if (SelectedPriorityFilter != "Tất cả mức độ")
                result = result.Where(m => m.DoUuTien == SelectedPriorityFilter);

            if (SelectedTechnicianFilter != "Tất cả kỹ thuật viên")
                result = result.Where(m => m.TenNhanVien == SelectedTechnicianFilter);

            FilteredMaintenances = new ObservableCollection<MaintenanceDisplayItem>(result);
        }

        private void RefreshStats()
        {
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(ProcessingCount));
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(OverdueCount));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}