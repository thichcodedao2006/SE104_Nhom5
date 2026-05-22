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
namespace QLTB.ViewModel
{
    // Device Model Class
    public class Device
    {
        public string Name { get; set; }
        public string Model { get; set; }
        public string Serial { get; set; }
        public string Manufacturer { get; set; }
        public string Department { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public string WarrantyDate { get; set; }
    }

    public class DeviceViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Device> Devices { get; set; }
        public ObservableCollection<Device> FilteredDevices { get; set; }

        private Device _selectedDevice;
        public Device SelectedDevice
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
        public int TotalDevices => Devices?.Count ?? 0;
        public int ActiveDevices => Devices?.Count(d => d.Status == "Hoạt động") ?? 0;
        public int MaintenanceDevices => Devices?.Count(d => d.Status == "Bảo trì") ?? 0;
        public int InactiveDevices => Devices?.Count(d => d.Status == "Ngừng hoạt động") ?? 0;

        public ICommand AddDeviceCommand { get; set; }
        public ICommand EditDeviceCommand { get; set; }
        public ICommand DeleteDeviceCommand { get; set; }
        public ICommand ExportCommand { get; set; } // Lệnh Xuất file
        public ICommand ImportCommand { get; set; } // Lệnh Nhập file
        public ICommand SwitchToListViewCommand { get; set; }
        public ICommand SwitchToGridViewCommand { get; set; }

        public DeviceViewModel()
        {
            

            // Khởi tạo dữ liệu mẫu
            Devices = new ObservableCollection<Device>
            {
                new Device 
                { 
                    Name = "Máy tính để bàn", 
                    Model = "Dell OptiPlex 7090", 
                    Serial = "SN-2024-001", 
                    Manufacturer = "Dell Inc.", 
                    Department = "Phòng IT", 
                    Location = "Tầng 3 - Phòng 301", 
                    Status = "Hoạt động", 
                    WarrantyDate = "15/08/2027" 
                },
                new Device 
                { 
                    Name = "Laptop", 
                    Model = "HP EliteBook 840 G8", 
                    Serial = "SN-2024-002", 
                    Manufacturer = "HP Inc.", 
                    Department = "Phòng Kế toán", 
                    Location = "Tầng 2 - Phòng 205", 
                    Status = "Hoạt động", 
                    WarrantyDate = "20/09/2027" 
                },
                new Device 
                { 
                    Name = "Máy in laser", 
                    Model = "Canon LBP6030", 
                    Serial = "SN-2024-003", 
                    Manufacturer = "Canon", 
                    Department = "Phòng Hành chính", 
                    Location = "Tầng 1 - Phòng 102", 
                    Status = "Hoạt động", 
                    WarrantyDate = "10/06/2026" 
                },
                new Device 
                { 
                    Name = "Máy chủ", 
                    Model = "Dell PowerEdge R740", 
                    Serial = "SN-2023-015", 
                    Manufacturer = "Dell Inc.", 
                    Department = "Phòng IT", 
                    Location = "Tầng 4 - Server Room", 
                    Status = "Hoạt động", 
                    WarrantyDate = "30/12/2028" 
                },
                new Device 
                { 
                    Name = "Máy chiếu", 
                    Model = "Epson EB-X06", 
                    Serial = "SN-2024-004", 
                    Manufacturer = "Epson", 
                    Department = "Phòng Đào tạo", 
                    Location = "Tầng 2 - Phòng họp A", 
                    Status = "Bảo trì", 
                    WarrantyDate = "05/07/2026" 
                },
                new Device 
                { 
                    Name = "Máy scan", 
                    Model = "Fujitsu ScanSnap iX1600", 
                    Serial = "SN-2024-005", 
                    Manufacturer = "Fujitsu", 
                    Department = "Phòng Hành chính", 
                    Location = "Tầng 1 - Phòng 103", 
                    Status = "Hoạt động", 
                    WarrantyDate = "18/04/2027" 
                },
                new Device 
                { 
                    Name = "Switch mạng", 
                    Model = "Cisco Catalyst 2960", 
                    Serial = "SN-2023-020", 
                    Manufacturer = "Cisco Systems", 
                    Department = "Phòng IT", 
                    Location = "Tầng 3 - Tủ rack", 
                    Status = "Hoạt động", 
                    WarrantyDate = "22/11/2026" 
                },
                new Device 
                { 
                    Name = "UPS", 
                    Model = "APC Smart-UPS 1500VA", 
                    Serial = "SN-2023-018", 
                    Manufacturer = "APC by Schneider", 
                    Department = "Phòng IT", 
                    Location = "Tầng 4 - Server Room", 
                    Status = "Hoạt động", 
                    WarrantyDate = "15/10/2026" 
                },
                new Device 
                { 
                    Name = "Máy photocopy", 
                    Model = "Ricoh MP 2555", 
                    Serial = "SN-2022-012", 
                    Manufacturer = "Ricoh", 
                    Department = "Phòng Hành chính", 
                    Location = "Tầng 1 - Khu vực chung", 
                    Status = "Hoạt động", 
                    WarrantyDate = "28/02/2026" 
                },
                new Device 
                { 
                    Name = "Điện thoại IP", 
                    Model = "Cisco IP Phone 7841", 
                    Serial = "SN-2024-006", 
                    Manufacturer = "Cisco Systems", 
                    Department = "Phòng Kinh doanh", 
                    Location = "Tầng 2 - Phòng 210", 
                    Status = "Hoạt động", 
                    WarrantyDate = "12/05/2027" 
                },
                new Device 
                { 
                    Name = "Màn hình LCD", 
                    Model = "LG 27UK850-W", 
                    Serial = "SN-2024-007", 
                    Manufacturer = "LG Electronics", 
                    Department = "Phòng Thiết kế", 
                    Location = "Tầng 3 - Phòng 305", 
                    Status = "Hoạt động", 
                    WarrantyDate = "08/06/2027" 
                },
                new Device 
                { 
                    Name = "Router WiFi", 
                    Model = "TP-Link Archer AX6000", 
                    Serial = "SN-2024-008", 
                    Manufacturer = "TP-Link", 
                    Department = "Phòng IT", 
                    Location = "Tầng 2 - Hành lang", 
                    Status = "Hoạt động", 
                    WarrantyDate = "25/03/2027" 
                },
                new Device 
                { 
                    Name = "Máy tính All-in-One", 
                    Model = "iMac 24\" M1", 
                    Serial = "SN-2024-009", 
                    Manufacturer = "Apple Inc.", 
                    Department = "Phòng Marketing", 
                    Location = "Tầng 2 - Phòng 208", 
                    Status = "Hoạt động", 
                    WarrantyDate = "30/07/2027" 
                },
                new Device 
                { 
                    Name = "Ổ cứng NAS", 
                    Model = "Synology DS920+", 
                    Serial = "SN-2023-025", 
                    Manufacturer = "Synology", 
                    Department = "Phòng IT", 
                    Location = "Tầng 4 - Server Room", 
                    Status = "Hoạt động", 
                    WarrantyDate = "14/12/2026" 
                },
                new Device 
                { 
                    Name = "Webcam", 
                    Model = "Logitech Brio 4K", 
                    Serial = "SN-2024-010", 
                    Manufacturer = "Logitech", 
                    Department = "Phòng Đào tạo", 
                    Location = "Tầng 2 - Phòng họp B", 
                    Status = "Ngừng hoạt động", 
                    WarrantyDate = "19/01/2027" 
                }
            };

            FilteredDevices = new ObservableCollection<Device>(Devices);

            // Khởi tạo commands
            AddDeviceCommand = new RelayCommand(o => {
                var formViewModel = new DeviceFormViewModel();
                var deviceForm = new QLTB.UserControlFolder.Device.DeviceFormView();
                deviceForm.DataContext = formViewModel;
                System.Windows.Window window = new System.Windows.Window
                {
                    Title = "Thêm thiết bị mới",
                    Content = deviceForm,
                    SizeToContent = System.Windows.SizeToContent.WidthAndHeight,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                    ResizeMode = System.Windows.ResizeMode.NoResize,
                    WindowStyle = WindowStyle.None
                };

                
                window.ShowDialog();
                if (formViewModel.IsSaved)
                {
                    Devices.Add(new Device
                    {
                        Name = formViewModel.Name,
                        Model = formViewModel.Model,
                        Serial = formViewModel.Serial,
                        Manufacturer = formViewModel.Manufacturer,
                        Department = formViewModel.Department,
                        Location = formViewModel.Location,
                        Status = formViewModel.Status,
                        WarrantyDate = formViewModel.WarrantyDate
                    });
                    LoadData();
                    MessageBox.Show("Thêm thiết bị mới thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });

            EditDeviceCommand = new RelayCommand(o =>
            {
                if (SelectedDevice == null)
                {
                    MessageBox.Show("Vui lòng chọn một thiết bị để sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                var formViewModel = new DeviceFormViewModel(SelectedDevice);
                var deviceForm = new QLTB.UserControlFolder.Device.DeviceFormView();
                deviceForm.DataContext = formViewModel;

                System.Windows.Window window = new System.Windows.Window
                {
                    Title = "Chỉnh sửa thiết bị",
                    Content = deviceForm,
                    SizeToContent = System.Windows.SizeToContent.WidthAndHeight,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                    ResizeMode = System.Windows.ResizeMode.NoResize
                };

                window.ShowDialog();

                if (formViewModel.IsSaved)
                {
                    SelectedDevice.Name = formViewModel.Name;
                    SelectedDevice.Model = formViewModel.Model;
                    SelectedDevice.Serial = formViewModel.Serial;
                    SelectedDevice.Manufacturer = formViewModel.Manufacturer;
                    SelectedDevice.Department = formViewModel.Department;
                    SelectedDevice.Location = formViewModel.Location;
                    SelectedDevice.Status = formViewModel.Status;
                    SelectedDevice.WarrantyDate = formViewModel.WarrantyDate;

                    LoadData(); 
                    MessageBox.Show("Cập nhật thông tin thiết bị thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });
            DeleteDeviceCommand = new RelayCommand(o => {
                if (SelectedDevice != null)
                {
                    var result = MessageBox.Show($"Bạn có chắc muốn xóa {SelectedDevice.Name}?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        Devices.Remove(SelectedDevice);
                        FilterDevices();
                        RefreshStats();
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn một thiết bị để xóa!", "Thông báo");
                }
            });
            ExportCommand = new RelayCommand(o => ExportToCsv());
            ImportCommand = new RelayCommand(o => ImportFromCsv());
            SwitchToListViewCommand = new RelayCommand(o => IsGridView = false);
            SwitchToGridViewCommand = new RelayCommand(o => IsGridView = true);
        }
        private void LoadData()
        {
            if (Devices != null)
            {
                FilterDevices();
                RefreshStats();
            }
        }
        private void FilterDevices()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                FilteredDevices = new ObservableCollection<Device>(Devices);
            }
            else
            {
                FilteredDevices = new ObservableCollection<Device>(
                    Devices.Where(d => d.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                                    || d.Model.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                                    || d.Serial.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                                    || d.Department.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
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
                    sb.AppendLine("Ten,Model,Serial,Manufacturer,Department,Location,Status,WarrantyDate"); // Header

                    foreach (var d in FilteredDevices)
                    {
                        sb.AppendLine($"{d.Name},{d.Model},{d.Serial},{d.Manufacturer},{d.Department},{d.Location},{d.Status},{d.WarrantyDate}");
                    }
                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file CSV thành công!");
                }
                catch (Exception ex) { MessageBox.Show("Lỗi xuất file: " + ex.Message); }
            }
        }

        // HÀM NHẬP CSV
        private void ImportFromCsv()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "CSV Files|*.csv" };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var lines = File.ReadAllLines(openFileDialog.FileName);
                    // Bỏ qua dòng header (i = 1)
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var cols = lines[i].Split(',');
                        if (cols.Length >= 8)
                        {
                            Devices.Add(new Device
                            {
                                Name = cols[0],
                                Model = cols[1],
                                Serial = cols[2],
                                Manufacturer = cols[3],
                                Department = cols[4],
                                Location = cols[5],
                                Status = cols[6],
                                WarrantyDate = cols[7]
                            });
                        }
                    }
                    FilterDevices();
                    RefreshStats();
                    MessageBox.Show("Nhập file thành công!");
                }
                catch (Exception ex) { MessageBox.Show("Lỗi nhập file: " + ex.Message); }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
