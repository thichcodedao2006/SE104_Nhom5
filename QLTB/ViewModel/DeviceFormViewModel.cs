using QLTB.Helpers;
using System;
using System.Windows;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class DeviceFormViewModel : BaseViewModel
    {
        // Các thuộc tính để Bind vào TextBox trên giao diện Form
        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _model;
        public string Model { get => _model; set { _model = value; OnPropertyChanged(); } }

        private string _serial;
        public string Serial { get => _serial; set { _serial = value; OnPropertyChanged(); } }

        private string _manufacturer;
        public string Manufacturer { get => _manufacturer; set { _manufacturer = value; OnPropertyChanged(); } }

        private string _department;
        public string Department { get => _department; set { _department = value; OnPropertyChanged(); } }

        private string _location;
        public string Location { get => _location; set { _location = value; OnPropertyChanged(); } }

        private string _status = "Hoạt động"; // Mặc định là Hoạt động
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        private string _warrantyDate = DateTime.Now.AddYears(2).ToString("dd/MM/yyyy"); // Mặc định 2 năm bảo hành
        public string WarrantyDate { get => _warrantyDate; set { _warrantyDate = value; OnPropertyChanged(); } }
        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        // Biến lưu trạng thái người dùng nhấn Lưu hay Hủy
        public bool IsSaved { get; private set; } = false;

        // Các lệnh (Commands)
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand SelectImageCommand { get; set; }

        // Hàm khởi tạo dùng khi THÊM MỚI
        public DeviceFormViewModel()
        {
            InitCommands();
        }

        // Hàm khởi tạo dùng khi SỬA (Nạp lại dữ liệu cũ)
        public DeviceFormViewModel(Device existingDevice)
        {
            Name = existingDevice.Name;
            Model = existingDevice.Model;
            Serial = existingDevice.Serial;
            Manufacturer = existingDevice.Manufacturer;
            Department = existingDevice.Department;
            Location = existingDevice.Location;
            Status = existingDevice.Status;
            WarrantyDate = existingDevice.WarrantyDate;

            InitCommands();
        }

        private void InitCommands()
        {
            // Logic khi nhấn nút Lưu
            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Serial))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ Tên thiết bị và Số Serial!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                IsSaved = true;

                // Tìm Window đang chứa Form này và đóng nó lại
                if (o is Window currentWindow)
                {
                    currentWindow.Close();
                }
            });
            SelectImageCommand = new RelayCommand(o =>
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*",
                    Title = "Chọn hình ảnh thiết bị"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    ImagePath = openFileDialog.FileName; // Lưu đường dẫn file ảnh đã chọn
                    MessageBox.Show("Đã chọn ảnh thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });
            // Logic khi nhấn nút Hủy
            CancelCommand = new RelayCommand(o =>
            {
                IsSaved = false;
                if (o is Window currentWindow)
                {
                    currentWindow.Close();
                }
            });
        }
    }
}