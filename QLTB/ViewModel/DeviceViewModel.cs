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
        public int? SoLuong { get; set; }
        public double? Gia { get; set; }
        public DateTime? NgayNhapThietBi { get; set; }
        public string Serial { get; set; }
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
            set
            {
                _searchText = value;
                FilterDevices();
                OnPropertyChanged(nameof(SearchText));
            }
        }

        private bool _isGridView;
        public bool IsGridView
        {
            get => _isGridView;
            set { _isGridView = value; OnPropertyChanged(nameof(IsGridView)); }
        }

        public int TotalDevices => Devices?.Count ?? 0;
        public int ActiveDevices => Devices?.Count(d => d.Status == "Đang hoạt động") ?? 0;
        public int MaintenanceDevices => Devices?.Count(d => d.Status == "Đang bảo trì") ?? 0;
        public int InactiveDevices => Devices?.Count(d => d.Status == "Ngừng hoạt động") ?? 0;

        public ICommand AddDeviceCommand { get; set; }
        public ICommand EditDeviceCommand { get; set; }
        public ICommand DeleteDeviceCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand ImportCommand { get; set; }
        public ICommand SwitchToListViewCommand { get; set; }
        public ICommand SwitchToGridViewCommand { get; set; }

        public DeviceViewModel()
        {
            _context = new QuanLyVatTuContext();

            _ = LoadDataFromDatabaseAsync();

            SwitchToListViewCommand = new RelayCommand(o => IsGridView = false);
            SwitchToGridViewCommand = new RelayCommand(o => IsGridView = true);

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
                    var existingDevice = await _context.ThietBis
                        .FirstOrDefaultAsync(t => t.TenThietBi.ToLower() == formViewModel.Name.ToLower()
                                               && t.DonViSanXuat.ToLower() == formViewModel.Manufacturer.ToLower());

                    int targetDeviceId;

                    if (existingDevice != null)
                    {
                        targetDeviceId = existingDevice.IdthietBi;
                    }
                    else
                    {
                        var newDevice = new ThietBi
                        {
                            TenThietBi = formViewModel.Name,
                            LoaiThietBi = "Thiết bị điện tử",
                            DonViSanXuat = formViewModel.Manufacturer,
                            NgayNhapThietBi = DateTime.Now
                        };

                        _context.ThietBis.Add(newDevice);
                        await _context.SaveChangesAsync();
                        targetDeviceId = newDevice.IdthietBi;
                    }

                    int? departmentId = null;
                    if (!string.IsNullOrWhiteSpace(formViewModel.Department))
                    {
                        var pb = await _context.PhongBans.FirstOrDefaultAsync(p => p.TenPhong == formViewModel.Department);
                        if (pb != null)
                        {
                            departmentId = pb.Idphong;
                        }
                    }

                    var newDetail = new ChiTietThietBi
                    {
                        IdthietBi = targetDeviceId,
                        SoSeri = formViewModel.Serial,
                        TinhTrang = formViewModel.Status == "Đang hoạt động" ? "Tốt" : formViewModel.Status,
                        IdphongBan = departmentId
                    };

                    _context.ChiTietThietBis.Add(newDetail);
                    await _context.SaveChangesAsync();

                    await LoadDataFromDatabaseAsync();
                    MessageBox.Show("Thêm thiết bị và cập nhật số lượng kho thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });

            EditDeviceCommand = new RelayCommand(async o =>
            {
                if (SelectedDevice == null)
                {
                    MessageBox.Show("Vui lòng chọn một thiết bị để sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var dbDevice = await _context.ThietBis
                                             .Include(d => d.ChiTietThietBis)
                                                .ThenInclude(ct => ct.IdphongBanNavigation)
                                             .FirstOrDefaultAsync(d => d.IdthietBi == SelectedDevice.IdthietBi);
                if (dbDevice == null) return;

                var formViewModel = new DeviceFormViewModel(dbDevice);
                var deviceForm = new QLTB.UserControlFolder.Device.DeviceFormView { DataContext = formViewModel };

                Window window = new Window
                {
                    Title = "Chỉnh sửa thiết bị",
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
                    dbDevice.TenThietBi = formViewModel.Name;
                    dbDevice.DonViSanXuat = formViewModel.Manufacturer;

                    var detail = dbDevice.ChiTietThietBis.FirstOrDefault();
                    if (detail != null)
                    {
                        detail.SoSeri = formViewModel.Serial;
                        detail.TinhTrang = formViewModel.Status == "Đang hoạt động" ? "Tốt" : formViewModel.Status;

                        if (!string.IsNullOrWhiteSpace(formViewModel.Department))
                        {
                            var pb = await _context.PhongBans.FirstOrDefaultAsync(p => p.TenPhong == formViewModel.Department);
                            if (pb != null)
                            {
                                detail.IdphongBan = pb.Idphong;
                            }
                        }
                        else
                        {
                            detail.IdphongBan = null;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await LoadDataFromDatabaseAsync();
                    MessageBox.Show("Cập nhật thông tin thiết bị thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });

            // Lệnh xóa đã được tối ưu hóa để loại bỏ ràng buộc khóa ngoại trước khi xóa cha
            DeleteDeviceCommand = new RelayCommand(async o =>
            {
                if (SelectedDevice == null)
                {
                    MessageBox.Show("Vui lòng chọn một thiết bị để xóa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Bạn có chắc muốn xóa thiết bị {SelectedDevice.TenThietBi} khỏi hệ thống? Tất cả các số Serial chi tiết liên quan cũng sẽ bị xóa bỏ hoàn toàn.", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var dbDevice = await _context.ThietBis
                                                 .Include(d => d.ChiTietThietBis)
                                                 .FirstOrDefaultAsync(d => d.IdthietBi == SelectedDevice.IdthietBi);
                    if (dbDevice != null)
                    {
                        if (dbDevice.ChiTietThietBis != null && dbDevice.ChiTietThietBis.Any())
                        {
                            _context.ChiTietThietBis.RemoveRange(dbDevice.ChiTietThietBis);
                        }

                        _context.ThietBis.Remove(dbDevice);
                        await _context.SaveChangesAsync();

                        await LoadDataFromDatabaseAsync();
                        MessageBox.Show("Xóa thiết bị khỏi hệ thống thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            });

            ExportCommand = new RelayCommand(o => ExportToCsv());
            ImportCommand = new RelayCommand(async o => await ImportFromCsvAsync());
        }

        private async Task LoadDataFromDatabaseAsync()
        {
            try
            {
                var rawList = await _context.ThietBis
                                             .Include(d => d.ChiTietThietBis)
                                             .ToListAsync();

                var mappedList = rawList.Select(d => {
                    var firstDetail = d.ChiTietThietBis.FirstOrDefault();
                    string statusText = "Đang hoạt động";
                    if (firstDetail != null && !string.IsNullOrEmpty(firstDetail.TinhTrang) && firstDetail.TinhTrang != "Tốt")
                    {
                        statusText = firstDetail.TinhTrang;
                    }

                    return new DeviceDisplayItem
                    {
                        IdthietBi = d.IdthietBi,
                        TenThietBi = d.TenThietBi,
                        LoaiThietBi = d.LoaiThietBi,
                        DonViSanXuat = d.DonViSanXuat,
                        SoLuong = d.ChiTietThietBis.Count,
                        Gia = d.Gia,
                        NgayNhapThietBi = d.NgayNhapThietBi,
                        Serial = firstDetail?.SoSeri ?? "Không có",
                        Status = statusText
                    };
                }).ToList();

                Devices = new ObservableCollection<DeviceDisplayItem>(mappedList);
                FilterDevices();
                RefreshStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối cơ sở dữ liệu Somee: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                    || (d.LoaiThietBi != null && d.LoaiThietBi.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                                    || (d.DonViSanXuat != null && d.DonViSanXuat.Contains(SearchText, StringComparison.OrdinalIgnoreCase))));
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

        private void ExportToCsv()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "CSV Files|*.csv", FileName = "DanhSachThietBi.csv" };
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Ten,Loai,DonViSanXuat,NgayNhap");

                    foreach (var d in FilteredDevices)
                    {
                        sb.AppendLine($"{d.TenThietBi},{d.LoaiThietBi},{d.DonViSanXuat},{d.NgayNhapThietBi}");
                    }
                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file CSV thành công!");
                }
                catch (Exception ex) { MessageBox.Show("Lỗi xuất file: " + ex.Message); }
            }
        }

        private async Task ImportFromCsvAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "CSV Files|*.csv" };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var lines = File.ReadAllLines(openFileDialog.FileName);
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var cols = lines[i].Split(',');
                        if (cols.Length >= 3)
                        {
                            _context.ThietBis.Add(new ThietBi
                            {
                                TenThietBi = cols[0],
                                LoaiThietBi = cols[1],
                                DonViSanXuat = cols[2],
                                NgayNhapThietBi = DateTime.Now
                            });
                        }
                    }
                    await _context.SaveChangesAsync();
                    await LoadDataFromDatabaseAsync();
                    MessageBox.Show("Nhập dữ liệu từ file và lưu vào database thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) { MessageBox.Show("Lỗi nhập file: " + ex.Message); }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}