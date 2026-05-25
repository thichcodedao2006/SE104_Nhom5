using System;
using System.Collections.Generic;
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
        public string TenNhanVien { get; set; }
        public DateTime? NgayBaoTri { get; set; }
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

        public MaintenanceTaskViewModel()
        {
            TechnicianList = new ObservableCollection<string> { "Tất cả kỹ thuật viên" };
            _ = LoadMaintenancesAsync();

            TriggerFilterCommand = new RelayCommand(o => FilterMaintenances());

            DeleteMaintenanceCommand = new RelayCommand(async o =>
            {
                if (SelectedMaintenance == null) return;
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa bản ghi bảo trì này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    using (var db = new QuanLyVatTuContext())
                    {
                        var dbMaintenance = await db.BaoTris.FirstOrDefaultAsync(b => b.IdbaoTri == SelectedMaintenance.IdBaoTri);
                        if (dbMaintenance != null)
                        {
                            int? staffId = dbMaintenance.IdnhanVien;
                            db.BaoTris.Remove(dbMaintenance);
                            await db.SaveChangesAsync();

                            if (staffId.HasValue) await UpdateStaffStatusAsync(db, staffId.Value);
                        }
                    }
                    await LoadMaintenancesAsync();
                    DataSyncService.NotifyDataChanged();
                }
            });

            CompleteTaskCommand = new RelayCommand(async o =>
            {
                if (o is MaintenanceDisplayItem item)
                {
                    using (var db = new QuanLyVatTuContext())
                    {
                        var dbItem = await db.BaoTris.FirstOrDefaultAsync(b => b.IdbaoTri == item.IdBaoTri);
                        if (dbItem != null)
                        {
                            dbItem.TinhTrangBaoTri = "Hoàn thành";
                            db.Entry(dbItem).State = EntityState.Modified;
                            await db.SaveChangesAsync();

                            if (dbItem.IdnhanVien.HasValue) await UpdateStaffStatusAsync(db, dbItem.IdnhanVien.Value);
                        }
                    }
                    await LoadMaintenancesAsync();
                    DataSyncService.NotifyDataChanged();
                    MessageBox.Show("Đã hoàn thành công việc!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });

            EditTaskCommand = new RelayCommand(async o =>
            {
                if (o is MaintenanceDisplayItem item)
                {
                    var editVM = new MaintenanceEditViewModel(item);
                    var editView = new QLTB.UserControlFolder.Maintenance.MaintenanceEditFormView { DataContext = editVM };
                    Window window = new Window { Content = editView, SizeToContent = SizeToContent.WidthAndHeight, WindowStartupLocation = WindowStartupLocation.CenterScreen, WindowStyle = WindowStyle.None, AllowsTransparency = true };
                    window.ShowDialog();

                    if (editVM.IsSaved)
                    {
                        try
                        {
                            using (var db = new QuanLyVatTuContext())
                            {
                                var dbItem = await db.BaoTris.FirstOrDefaultAsync(b => b.IdbaoTri == item.IdBaoTri);
                                if (dbItem != null)
                                {
                                    int? oldStaffId = dbItem.IdnhanVien;
                                    dbItem.NgayBaoTri = editVM.NgayBaoTri;
                                    dbItem.DoUuTien = editVM.DoUuTien;
                                    dbItem.TinhTrangBaoTri = editVM.TinhTrangBaoTri?.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
                                    dbItem.IdnhanVien = editVM.SelectedStaffId;
                                    db.Entry(dbItem).State = EntityState.Modified;
                                    await db.SaveChangesAsync();

                                    if (oldStaffId.HasValue) await UpdateStaffStatusAsync(db, oldStaffId.Value);
                                    if (editVM.SelectedStaffId.HasValue) await UpdateStaffStatusAsync(db, editVM.SelectedStaffId.Value);
                                }
                            }
                            await LoadMaintenancesAsync();
                            DataSyncService.NotifyDataChanged();
                            MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                    }
                }
            });
        }

        private async Task UpdateStaffStatusAsync(QuanLyVatTuContext db, int staffId)
        {
            bool hasActiveTasks = await db.BaoTris
                .AnyAsync(b => b.IdnhanVien == staffId
                            && b.TinhTrangBaoTri != null
                            && b.TinhTrangBaoTri.Trim().ToLower() != "hoàn thành");

            var staff = await db.NhanViens.FirstOrDefaultAsync(n => n.IdnhanVien == staffId);
            if (staff != null)
            {
                string newStatus = hasActiveTasks ? "Đang bận" : "Đang rảnh";

                if (staff.TinhTrang != newStatus)
                {
                    staff.TinhTrang = newStatus;
                    db.Entry(staff).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
            }
        }

        private async Task LoadMaintenancesAsync()
        {
            try
            {
                using (var db = new QuanLyVatTuContext())
                {
                    var rawList = await db.BaoTris.Include(b => b.IddichVuNavigation).Include(b => b.IdnhanVienNavigation).Include(b => b.ChiTietThietBi).ThenInclude(ct => ct.IdthietBiNavigation).ToListAsync();
                    Maintenances = new ObservableCollection<MaintenanceDisplayItem>(rawList.Select(b => new MaintenanceDisplayItem { IdBaoTri = b.IdbaoTri, TenThietBi = b.ChiTietThietBi?.IdthietBiNavigation?.TenThietBi ?? "Thiết bị đã xóa", SoSeri = b.SoSeri, TenDichVu = b.IddichVuNavigation?.TenDichVu ?? "Không rõ", TenNhanVien = b.IdnhanVienNavigation?.HoTen ?? "Chưa phân công", NgayBaoTri = b.NgayBaoTri, DoUuTien = b.DoUuTien ?? "Thấp", TinhTrangBaoTri = b.TinhTrangBaoTri ?? "Đang xử lý" }));
                    var staffNames = await db.NhanViens.Select(nv => nv.HoTen).Distinct().ToListAsync();
                    TechnicianList = new ObservableCollection<string>(staffNames.Prepend("Tất cả kỹ thuật viên"));
                }
                FilterMaintenances();
                RefreshStats();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải: " + ex.Message); }
        }

        private void FilterMaintenances()
        {
            if (Maintenances == null) return;
            var result = Maintenances.AsEnumerable();
            if (!string.IsNullOrEmpty(SearchText)) result = result.Where(m => (m.TenThietBi != null && m.TenThietBi.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) || (m.SoSeri != null && m.SoSeri.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            if (SelectedStatusFilter != "Tất cả trạng thái") result = result.Where(m => m.TinhTrangBaoTri == SelectedStatusFilter);
            if (SelectedPriorityFilter != "Tất cả mức độ") result = result.Where(m => m.DoUuTien == SelectedPriorityFilter);
            if (SelectedTechnicianFilter != "Tất cả kỹ thuật viên") result = result.Where(m => m.TenNhanVien == SelectedTechnicianFilter);
            FilteredMaintenances = new ObservableCollection<MaintenanceDisplayItem>(result);
        }

        private void RefreshStats()
        {
            OnPropertyChanged(nameof(TotalCount)); OnPropertyChanged(nameof(ProcessingCount)); OnPropertyChanged(nameof(CompletedCount)); OnPropertyChanged(nameof(OverdueCount));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}