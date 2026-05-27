using Microsoft.Win32;
using QLTB.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using QLTB.Models;

namespace QLTB.ViewModel
{
    public class DeviceDisplayItem
    {
        public int IdthietBi { get; set; }
        public string TenThietBi { get; set; }
        public string LoaiThietBi { get; set; }
        public string DonViSanXuat { get; set; }
        public double? Gia { get; set; }
        public DateTime? NgayNhapThietBi { get; set; }
        public string Serial { get; set; }
        public string PhongBan { get; set; }
        public string Status { get; set; }
    }

    public class DeviceViewModel : INotifyPropertyChanged
    {
        private readonly QuanLyVatTuContext _context;

        private ObservableCollection<DeviceDisplayItem> _devices;
        public ObservableCollection<DeviceDisplayItem> Devices
        {
            get => _devices;
            set { _devices = value; OnPropertyChanged(nameof(Devices)); }
        }

        private ObservableCollection<DeviceDisplayItem> _filteredDevices;
        public ObservableCollection<DeviceDisplayItem> FilteredDevices
        {
            get => _filteredDevices;
            set { _filteredDevices = value; OnPropertyChanged(nameof(FilteredDevices)); }
        }

        private DeviceDisplayItem _selectedDevice;
        public DeviceDisplayItem SelectedDevice
        {
            get => _selectedDevice;
            set { _selectedDevice = value; OnPropertyChanged(nameof(SelectedDevice)); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; FilterDevices(); OnPropertyChanged(nameof(SearchText)); }
        }

        private bool _isGridView;
        public bool IsGridView
        {
            get => _isGridView;
            set { _isGridView = value; OnPropertyChanged(nameof(IsGridView)); }
        }

        public int TotalDevices => Devices?.Count ?? 0;
        public int ActiveDevices => Devices?.Count(d => d.Status == "Đang hoạt động" || d.Status == "Tốt") ?? 0;
        public int MaintenanceDevices => Devices?.Count(d => d.Status == "Đang bảo trì") ?? 0;
        public int InactiveDevices => Devices?.Count(d => d.Status == "Ngừng hoạt động") ?? 0;

        public ICommand AddDeviceCommand { get; set; }
        public ICommand EditDeviceCommand { get; set; }
        public ICommand DeleteDeviceCommand { get; set; }
        public ICommand DeleteByNameCommand { get; set; }
        public ICommand SwitchToListViewCommand { get; set; }
        public ICommand SwitchToGridViewCommand { get; set; }
        public ICommand ExportCommand { get; set; }

        public DeviceViewModel()
        {
            _context = new QuanLyVatTuContext();
            _ = LoadDataFromDatabaseAsync();

            SwitchToListViewCommand = new RelayCommand(o => IsGridView = false);
            SwitchToGridViewCommand = new RelayCommand(o => IsGridView = true);

            ExportCommand = new RelayCommand(o =>
            {
                if (Devices == null || !Devices.Any())
                {
                    MessageBox.Show("Không có dữ liệu thiết bị nào để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV file (*.csv)|*.csv",
                    FileName = $"DanhSachThietBi_{DateTime.Now:ddMMyyyy}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                        {
                            sw.Write("\xFEFF");
                            sw.WriteLine("Tên Thiết Bị,Loại Thiết Bị,Nhà Sản Xuất,Số Serial,Phòng Ban,Trạng Thái");

                            var listToExport = FilteredDevices != null && FilteredDevices.Any() ? FilteredDevices : Devices;
                            foreach (var item in listToExport)
                            {
                                string ten = $"\"{item.TenThietBi}\"";
                                string loai = $"\"{item.LoaiThietBi}\"";
                                string nsx = $"\"{item.DonViSanXuat}\"";
                                string seri = $"\"{item.Serial}\"";
                                string phong = $"\"{item.PhongBan}\"";
                                string trangThai = $"\"{item.Status}\"";

                                sw.WriteLine($"{ten},{loai},{nsx},{seri},{phong},{trangThai}");
                            }
                        }
                        MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });

            AddDeviceCommand = new RelayCommand(async o =>
            {
                var formViewModel = new DeviceFormViewModel();
                var deviceForm = new QLTB.UserControlFolder.Device.DeviceFormView { DataContext = formViewModel };

                Window window = new Window
                {
                    Title = "Thêm thiết bị mới",
                    Content = deviceForm,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true
                };

                window.ShowDialog();

                if (formViewModel.IsSaved)
                {
                    await LoadDataFromDatabaseAsync();
                }
            });

            DeleteByNameCommand = new RelayCommand(async o =>
            {
                var currentItem = o as DeviceDisplayItem ?? SelectedDevice;
                if (currentItem == null)
                {
                    MessageBox.Show("Vui lòng chọn một thiết bị bất kỳ trong danh sách để làm mẫu xóa theo tên!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"CẢNH BÁO: Bạn có chắc chắn muốn xóa TOÀN BỘ các thiết bị chi tiết có tên là [{currentItem.TenThietBi}]?\nHành động này sẽ xóa sạch số Serial và Lịch sử bảo trì liên quan!", "Xác nhận xóa hàng loạt", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var chiTietBaoTris = await _context.ChiTietBaoTris.Where(ct => ct.IdthietBi == currentItem.IdthietBi).ToListAsync();
                        if (chiTietBaoTris.Any()) _context.ChiTietBaoTris.RemoveRange(chiTietBaoTris);

                        var baoCaoSuaChuas = await _context.BaoCaoSuaChuas.Where(bc => bc.IdthietBi == currentItem.IdthietBi).ToListAsync();
                        if (baoCaoSuaChuas.Any()) _context.BaoCaoSuaChuas.RemoveRange(baoCaoSuaChuas);

                        var detailsToDelete = await _context.ChiTietThietBis.Where(ct => ct.IdthietBi == currentItem.IdthietBi).ToListAsync();
                        if (detailsToDelete.Any()) _context.ChiTietThietBis.RemoveRange(detailsToDelete);

                        var parentDevice = await _context.ThietBis.FirstOrDefaultAsync(t => t.IdthietBi == currentItem.IdthietBi);
                        if (parentDevice != null) _context.ThietBis.Remove(parentDevice);

                        await _context.SaveChangesAsync();
                        await LoadDataFromDatabaseAsync();
                        MessageBox.Show($"Đã xóa sạch dòng sản phẩm [{currentItem.TenThietBi}] khỏi hệ thống!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi hệ thống khi thực hiện xóa hàng loạt: " + ex.Message, "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });

            EditDeviceCommand = new RelayCommand(async o =>
            {
                var currentItem = o as DeviceDisplayItem ?? SelectedDevice;
                if (currentItem == null)
                {
                    MessageBox.Show("Vui lòng chọn một thiết bị cụ thể từ danh sách để chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var dbDetail = await _context.ChiTietThietBis
                                             .Include(ct => ct.IdthietBiNavigation)
                                             .Include(ct => ct.IdphongBanNavigation)
                                             .FirstOrDefaultAsync(ct => ct.IdthietBi == currentItem.IdthietBi && ct.SoSeri == currentItem.Serial);
                if (dbDetail == null) return;

                var formViewModel = new DeviceFormViewModel(currentItem.IdthietBi)
                {
                    Name = dbDetail.IdthietBiNavigation?.TenThietBi,
                    Manufacturer = dbDetail.IdthietBiNavigation?.DonViSanXuat,
                    SelectedCategory = dbDetail.IdthietBiNavigation?.LoaiThietBi,
                    Serial = dbDetail.SoSeri,
                    SelectedPhongBanId = dbDetail.IdphongBan,
                    Status = dbDetail.TinhTrang == "Tốt" ? "Đang hoạt động" : dbDetail.TinhTrang
                };

                formViewModel.SaveCommand = new RelayCommand(async param =>
                {
                    if (string.IsNullOrWhiteSpace(formViewModel.Name) || string.IsNullOrWhiteSpace(formViewModel.Serial))
                    {
                        MessageBox.Show("Tên thiết bị và Số Serial không được phép để trống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (!formViewModel.Serial.Trim().Equals(dbDetail.SoSeri, StringComparison.OrdinalIgnoreCase))
                    {
                        bool isSerialExist = await _context.ChiTietThietBis.AnyAsync(ct => ct.SoSeri.ToLower() == formViewModel.Serial.Trim().ToLower());
                        if (isSerialExist)
                        {
                            MessageBox.Show($"Số Serial [{formViewModel.Serial}] đã tồn tại ở một thiết bị khác!", "Xung đột Serial", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    try
                    {
                        if (dbDetail.IdthietBiNavigation != null)
                        {
                            dbDetail.IdthietBiNavigation.TenThietBi = formViewModel.Name.Trim();
                            dbDetail.IdthietBiNavigation.DonViSanXuat = formViewModel.Manufacturer.Trim();
                            dbDetail.IdthietBiNavigation.LoaiThietBi = string.IsNullOrWhiteSpace(formViewModel.SelectedCategory) ? "Thiết bị điện tử" : formViewModel.SelectedCategory.Trim();
                        }

                        dbDetail.SoSeri = formViewModel.Serial.Trim();
                        dbDetail.TinhTrang = formViewModel.Status == "Đang hoạt động" ? "Tốt" : formViewModel.Status;
                        dbDetail.IdphongBan = formViewModel.SelectedPhongBanId;

                        await _context.SaveChangesAsync();
                        formViewModel.GetType().GetProperty("IsSaved")?.SetValue(formViewModel, true);

                        if (param is Window w) w.Close();
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi cập nhật dữ liệu: " + ex.Message); }
                });

                var deviceForm = new QLTB.UserControlFolder.Device.DeviceFormView { DataContext = formViewModel };
                Window window = new Window
                {
                    Title = "Chỉnh sửa thông tin thiết bị",
                    Content = deviceForm,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true
                };

                window.ShowDialog();

                var isSavedObj = formViewModel.GetType().GetProperty("IsSaved")?.GetValue(formViewModel);
                if (isSavedObj is bool isSaved && isSaved)
                {
                    await LoadDataFromDatabaseAsync();
                }
            });

            DeleteDeviceCommand = new RelayCommand(async o =>
            {
                var currentItem = o as DeviceDisplayItem ?? SelectedDevice;
                if (currentItem == null)
                {
                    MessageBox.Show("Vui lòng chọn một thiết bị cụ thể từ danh sách để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa thiết bị [{currentItem.TenThietBi}] có số Serial: {currentItem.Serial} khỏi hệ thống?", "Xác nhận xóa đơn lẻ", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var chiTietBaoTris = await _context.ChiTietBaoTris.Where(ct => ct.IdthietBi == currentItem.IdthietBi && ct.SoSeri == currentItem.Serial).ToListAsync();
                        if (chiTietBaoTris.Any()) _context.ChiTietBaoTris.RemoveRange(chiTietBaoTris);

                        var baoCaoSuaChuas = await _context.BaoCaoSuaChuas.Where(bc => bc.IdthietBi == currentItem.IdthietBi && bc.SoSeri == currentItem.Serial).ToListAsync();
                        if (baoCaoSuaChuas.Any()) _context.BaoCaoSuaChuas.RemoveRange(baoCaoSuaChuas);

                        var dbDetail = await _context.ChiTietThietBis.FirstOrDefaultAsync(ct => ct.IdthietBi == currentItem.IdthietBi && ct.SoSeri == currentItem.Serial);
                        if (dbDetail != null)
                        {
                            _context.ChiTietThietBis.Remove(dbDetail);
                        }

                        await _context.SaveChangesAsync();
                        await LoadDataFromDatabaseAsync();
                        MessageBox.Show("Đã xóa thiết bị ra khỏi hệ thống thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi hệ thống khi xóa: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            });
        }

        private async Task LoadDataFromDatabaseAsync()
        {
            try
            {
                var flatList = await _context.ChiTietThietBis
                                             .Include(ct => ct.IdthietBiNavigation)
                                             .Include(ct => ct.IdphongBanNavigation)
                                             .ToListAsync();

                var mappedList = flatList.Select(ct => new DeviceDisplayItem
                {
                    IdthietBi = ct.IdthietBi,
                    TenThietBi = ct.IdthietBiNavigation?.TenThietBi ?? "Không rõ tên",
                    LoaiThietBi = ct.IdthietBiNavigation?.LoaiThietBi ?? "Thiết bị",
                    DonViSanXuat = ct.IdthietBiNavigation?.DonViSanXuat ?? "Không rõ nhà SX",
                    Gia = ct.IdthietBiNavigation?.Gia,
                    NgayNhapThietBi = ct.IdthietBiNavigation?.NgayNhapThietBi,
                    Serial = ct.SoSeri,
                    PhongBan = ct.IdphongBanNavigation?.TenPhong ?? "Chưa phân bổ",
                    Status = ct.TinhTrang == "Tốt" ? "Đang hoạt động" : ct.TinhTrang
                }).ToList();

                Devices = new ObservableCollection<DeviceDisplayItem>(mappedList);
                FilterDevices();
                RefreshStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối CSDL: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterDevices()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                FilteredDevices = new ObservableCollection<DeviceDisplayItem>(Devices ?? new ObservableCollection<DeviceDisplayItem>());
            }
            else
            {
                FilteredDevices = new ObservableCollection<DeviceDisplayItem>(
                    Devices.Where(d => (d.TenThietBi != null && d.TenThietBi.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                                    || (d.Serial != null && d.Serial.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                                    || (d.PhongBan != null && d.PhongBan.Contains(SearchText, StringComparison.OrdinalIgnoreCase))));
            }
            OnPropertyChanged(nameof(FilteredDevices));
        }

        private void RefreshStats()
        {
            OnPropertyChanged(nameof(TotalDevices));
            OnPropertyChanged(nameof(ActiveDevices));
            OnPropertyChanged(nameof(MaintenanceDevices));
            OnPropertyChanged(nameof(InactiveDevices));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}