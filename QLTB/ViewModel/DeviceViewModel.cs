using QLTB.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public int TotalDevices => Devices.Count;
        public int ActiveDevices => Devices.Count(d => d.Status == "Hoạt động");
        public int MaintenanceDevices => Devices.Count(d => d.Status == "Bảo trì");
        public int InactiveDevices => Devices.Count(d => d.Status == "Ngừng hoạt động");

        public ICommand AddDeviceCommand { get; set; }
        public ICommand EditDeviceCommand { get; set; }
        public ICommand DeleteDeviceCommand { get; set; }

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
            AddDeviceCommand = new RelayCommand(o => { /* implement add */ });
            EditDeviceCommand = new RelayCommand(o => { /* implement edit */ });
            DeleteDeviceCommand = new RelayCommand(o => { /* implement delete */ });
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
