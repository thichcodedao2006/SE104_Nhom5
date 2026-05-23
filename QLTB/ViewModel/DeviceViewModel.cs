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
    public class DeviceViewModel : INotifyPropertyChanged
    {
        private readonly QuanLyVatTuContext _context;

        private ObservableCollection<ThietBi> _devices;
        public ObservableCollection<ThietBi> Devices
        {
            get => _devices;
            set { _devices = value; OnPropertyChanged(nameof(Devices)); }
        }

        private ObservableCollection<ThietBi> _filteredDevices;
        public ObservableCollection<ThietBi> FilteredDevices
        {
            get => _filteredDevices;
            set { _filteredDevices = value; OnPropertyChanged(nameof(FilteredDevices)); }
        }

        private ThietBi _selectedDevice;
        public ThietBi SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged(nameof(SelectedDevice));
            }
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
            set
            {
                _isGridView = value;
                OnPropertyChanged(nameof(IsGridView));
            }
        }

        // Thống kê dựa trên các trường thực tế của bảng ThietBi
        public int TotalDevices => Devices?.Count ?? 0;
        public int ActiveDevices => Devices?.Count(d => d.LoaiThietBi == "Hoạt động") ?? 0;
        public int MaintenanceDevices => Devices?.Count(d => d.LoaiThietBi == "Bảo trì") ?? 0;
        public int InactiveDevices => Devices?.Count(d => d.LoaiThietBi == "Ngừng hoạt động") ?? 0;

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

            // Gọi hàm lấy dữ liệu từ database thực tế
            LoadDataFromDatabase();

            SwitchToListViewCommand = new RelayCommand(o => IsGridView = false);
            SwitchToGridViewCommand = new RelayCommand(o => IsGridView = true);

            // ==========================================
            // LỆNH: THÊM THIẾT BỊ MỚI
            // ==========================================
            AddDeviceCommand = new RelayCommand(o =>
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
                    var newDevice = new ThietBi
                    {
                        TenThietBi = formViewModel.Name,
                        LoaiThietBi = formViewModel.Status,
                        DonViSanXuat = formViewModel.Manufacturer,
                        NgayNhapThietBi = DateTime.Now
                    };

                    _context.ThietBis.Add(newDevice); // Thêm vào DbSet trong bối cảnh dữ liệu
                    _context.SaveChanges();          // Lưu xuống Somee mssql Server

                    LoadDataFromDatabase(); // Cập nhật lại UI và thống kê
                    MessageBox.Show("Thêm thiết bị mới vào cơ sở dữ liệu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });

            // ==========================================
            // LỆNH: CHỈNH SỬA THIẾT BỊ
            // ==========================================
            EditDeviceCommand = new RelayCommand(o =>
            {
                if (SelectedDevice == null)
                {
                    MessageBox.Show("Vui lòng chọn một thiết bị để sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Đã sửa: formViewModel nhận SelectedDevice kiểu ThietBi chuẩn xác
                var formViewModel = new DeviceFormViewModel(SelectedDevice);
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
                    // Lấy bản ghi thực tế trong Db dựa trên Khóa chính IDThietBi
                    var dbDevice = _context.ThietBis.FirstOrDefault(d => d.IdthietBi == SelectedDevice.IdthietBi);
                    if (dbDevice != null)
                    {
                        dbDevice.TenThietBi = formViewModel.Name;
                        dbDevice.LoaiThietBi = formViewModel.Status;
                        dbDevice.DonViSanXuat = formViewModel.Manufacturer;

                        _context.SaveChanges(); // Cập nhật thay đổi lên cơ sở dữ liệu

                        LoadDataFromDatabase();
                        MessageBox.Show("Cập nhật thông tin thiết bị thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            });

            // ==========================================
            // LỆNH: XÓA THIẾT BỊ
            // ==========================================
            DeleteDeviceCommand = new RelayCommand(o =>
            {
                if (SelectedDevice == null)
                {
                    MessageBox.Show("Vui lòng chọn một thiết bị để xóa!", "Thông báo");
                    return;
                }

                var result = MessageBox.Show($"Bạn có chắc muốn xóa thiết bị {SelectedDevice.TenThietBi} khỏi hệ thống?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var dbDevice = _context.ThietBis.FirstOrDefault(d => d.IdthietBi == SelectedDevice.IdthietBi);
                    if (dbDevice != null)
                    {
                        _context.ThietBis.Remove(dbDevice); // Lệnh xóa đối tượng
                        _context.SaveChanges();          // Thực thi DELETE lệnh SQL

                        LoadDataFromDatabase();
                    }
                }
            });

            ExportCommand = new RelayCommand(o => ExportToCsv());
            ImportCommand = new RelayCommand(o => ImportFromCsv());
        }

        private void LoadData()
        {
            if (Devices != null)
            {
                FilterDevices();
                RefreshStats();
            }
        }

        // Hàm đọc dữ liệu thực tế kết nối trực tiếp từ Server SQL Somee
        private void LoadDataFromDatabase()
        {
            try
            {
                var list = _context.ThietBis.ToList();

                Devices = new ObservableCollection<ThietBi>(list);
                FilterDevices();
                RefreshStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối cơ sở dữ liệu Somee: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Tìm kiếm an toàn chống dính lỗi hệ thống NullReferenceException nếu chuỗi trong database trống
        private void FilterDevices()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                FilteredDevices = new ObservableCollection<ThietBi>(Devices ?? new ObservableCollection<ThietBi>());
            }
            else
            {
                FilteredDevices = new ObservableCollection<ThietBi>(
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

        private void ImportFromCsv()
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
                    _context.SaveChanges();

                    LoadDataFromDatabase();
                    MessageBox.Show("Nhập dữ liệu từ file và lưu vào database thành công!");
                }
                catch (Exception ex) { MessageBox.Show("Lỗi nhập file: " + ex.Message); }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}