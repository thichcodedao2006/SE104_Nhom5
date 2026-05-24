using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using QLTB.Helpers;
using QLTB.Models;

namespace QLTB.ViewModel
{
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
        private readonly QuanLyVatTuContext _context;

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
            _context = new QuanLyVatTuContext();
            TechnicianList = new ObservableCollection<string> { "Tất cả kỹ thuật viên" };
            _ = LoadMaintenancesAsync();

            TriggerFilterCommand = new RelayCommand(o => FilterMaintenances());

            DeleteMaintenanceCommand = new RelayCommand(async o =>
            {
                if (SelectedMaintenance == null)
                {
                    MessageBox.Show("Vui lòng chọn một bản ghi để xóa!");
                    return;
                }
                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa bản ghi bảo trì này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var dbMaintenance = await _context.BaoTris.FirstOrDefaultAsync(b => b.IdbaoTri == SelectedMaintenance.IdBaoTri);
                    if (dbMaintenance != null)
                    {
                        _context.BaoTris.Remove(dbMaintenance);
                        await _context.SaveChangesAsync();
                        await LoadMaintenancesAsync();
                    }
                }
            });

            CompleteTaskCommand = new RelayCommand(async o =>
            {
                if (o is MaintenanceDisplayItem item)
                {
                    var dbItem = await _context.BaoTris.FirstOrDefaultAsync(b => b.IdbaoTri == item.IdBaoTri);
                    if (dbItem != null)
                    {
                        dbItem.TinhTrangBaoTri = "Hoàn thành";
                        await _context.SaveChangesAsync();
                        await LoadMaintenancesAsync();
                    }
                }
            });

            // Hiện thực hóa logic mở Popup Form chỉnh sửa và cập nhật dữ liệu bất đồng bộ
            EditTaskCommand = new RelayCommand(async o =>
            {
                if (o is MaintenanceDisplayItem item)
                {
                    var editVM = new MaintenanceEditViewModel(item);
                    var editView = new QLTB.UserControlFolder.Maintenance.MaintenanceEditFormView { DataContext = editVM };

                    Window window = new Window
                    {
                        Content = editView,
                        SizeToContent = SizeToContent.WidthAndHeight,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        WindowStyle = WindowStyle.None,
                        AllowsTransparency = true
                    };

                    window.ShowDialog();

                    if (editVM.IsSaved)
                    {
                        var dbItem = await _context.BaoTris.FirstOrDefaultAsync(b => b.IdbaoTri == item.IdBaoTri);
                        if (dbItem != null)
                        {
                            dbItem.NgayBaoTri = editVM.NgayBaoTri;
                            dbItem.DoUuTien = editVM.DoUuTien;
                            dbItem.TinhTrangBaoTri = editVM.TinhTrangBaoTri;
                            dbItem.IdnhanVien = editVM.SelectedStaffId;

                            await _context.SaveChangesAsync();
                            await LoadMaintenancesAsync();
                            MessageBox.Show("Cập nhật công việc bảo trì thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            });
        }

        private async Task LoadMaintenancesAsync()
        {
            try
            {
                var rawList = await _context.BaoTris
                                            .Include(b => b.IddichVuNavigation)
                                            .Include(b => b.IdnhanVienNavigation)
                                            .Include(b => b.ChiTietThietBi)
                                                .ThenInclude(ct => ct.IdthietBiNavigation)
                                            .ToListAsync();

                var mappedList = rawList.Select(b => new MaintenanceDisplayItem
                {
                    IdBaoTri = b.IdbaoTri,
                    IdThietBi = b.IdthietBi ?? 0,
                    TenThietBi = b.ChiTietThietBi?.IdthietBiNavigation?.TenThietBi ?? "Thiết bị đã xóa",
                    SoSeri = b.SoSeri,
                    TenDichVu = b.IddichVuNavigation?.TenDichVu ?? "Không rõ dịch vụ",
                    GiaDichVu = b.IddichVuNavigation?.GiaDichVu ?? 0,
                    TenNhanVien = b.IdnhanVienNavigation?.HoTen ?? "Chưa phân công",
                    NgayBaoTri = b.NgayBaoTri,
                    DoUuTien = b.DoUuTien ?? "Thấp",
                    TinhTrangBaoTri = b.TinhTrangBaoTri ?? "Đang xử lý",
                    GhiChu = b.GhiChu
                }).ToList();

                Maintenances = new ObservableCollection<MaintenanceDisplayItem>(mappedList);

                var staffNames = await _context.NhanViens.Select(nv => nv.HoTen).Distinct().ToListAsync();
                if (!staffNames.Contains("Phạm Đan Trường"))
                {
                    staffNames.Add("Phạm Đan Trường");
                }

                TechnicianList = new ObservableCollection<string> { "Tất cả kỹ thuật viên" };
                foreach (var name in staffNames)
                {
                    TechnicianList.Add(name);
                }

                FilterMaintenances();
                RefreshStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách: " + ex.Message);
            }
        }

        private void FilterMaintenances()
        {
            if (Maintenances == null) return;

            var result = Maintenances.AsEnumerable();

            if (!string.IsNullOrEmpty(SearchText))
            {
                result = result.Where(m => (m.TenThietBi != null && m.TenThietBi.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                                        || (m.SoSeri != null && m.SoSeri.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrEmpty(SelectedStatusFilter) && SelectedStatusFilter != "Tất cả trạng thái")
            {
                result = result.Where(m => m.TinhTrangBaoTri == SelectedStatusFilter);
            }

            if (!string.IsNullOrEmpty(SelectedPriorityFilter) && SelectedPriorityFilter != "Tất cả mức độ")
            {
                result = result.Where(m => m.DoUuTien == SelectedPriorityFilter);
            }

            if (!string.IsNullOrEmpty(SelectedTechnicianFilter) && SelectedTechnicianFilter != "Tất cả kỹ thuật viên")
            {
                result = result.Where(m => m.TenNhanVien == SelectedTechnicianFilter);
            }

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