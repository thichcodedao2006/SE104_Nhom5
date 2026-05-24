using QLTB.Helpers;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QLTB.Models;

namespace QLTB.ViewModel
{
    public class DeviceFormViewModel : BaseViewModel
    {
        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _serial;
        public string Serial { get => _serial; set { _serial = value; OnPropertyChanged(); } }

        private string _manufacturer;
        public string Manufacturer { get => _manufacturer; set { _manufacturer = value; OnPropertyChanged(); } }

        private string _department;
        public string Department { get => _department; set { _department = value; OnPropertyChanged(); } }

        private string _location;
        public string Location { get => _location; set { _location = value; OnPropertyChanged(); } }

        private string _status = "Đang hoạt động";
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        private string _warrantyDate = DateTime.Now.AddYears(2).ToString("dd/MM/yyyy");
        public string WarrantyDate { get => _warrantyDate; set { _warrantyDate = value; OnPropertyChanged(); } }

        private string _imagePath;
        public string ImagePath { get => _imagePath; set { _imagePath = value; OnPropertyChanged(nameof(ImagePath)); } }

        public bool IsSaved { get; private set; } = false;

        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand SelectImageCommand { get; set; }

        public DeviceFormViewModel()
        {
            InitCommands();
        }

        public DeviceFormViewModel(ThietBi existingDevice)
        {
            if (existingDevice != null)
            {
                Name = existingDevice.TenThietBi;
                Manufacturer = existingDevice.DonViSanXuat;
                WarrantyDate = existingDevice.NgayNhapThietBi?.ToString("dd/MM/yyyy") ?? DateTime.Now.ToString("dd/MM/yyyy");

                if (existingDevice.LoaiThietBi != null && (existingDevice.LoaiThietBi.Contains(":\\") || existingDevice.LoaiThietBi.Contains(":/")))
                {
                    ImagePath = existingDevice.LoaiThietBi;
                }

                var firstDetail = existingDevice.ChiTietThietBis?.FirstOrDefault();
                if (firstDetail != null)
                {
                    Serial = firstDetail.SoSeri;

                    if (string.IsNullOrEmpty(firstDetail.TinhTrang) || firstDetail.TinhTrang == "Tốt")
                    {
                        Status = "Đang hoạt động";
                    }
                    else
                    {
                        Status = firstDetail.TinhTrang;
                    }

                    if (firstDetail.IdphongBanNavigation != null)
                    {
                        Department = firstDetail.IdphongBanNavigation.TenPhong ?? string.Empty;
                        Location = firstDetail.IdphongBanNavigation.ViTri != null ? "Tầng " + firstDetail.IdphongBanNavigation.ViTri : string.Empty;
                    }
                    else
                    {
                        Department = string.Empty;
                        Location = string.Empty;
                    }
                }
                else
                {
                    Serial = string.Empty;
                    Status = "Đang hoạt động";
                    Department = string.Empty;
                    Location = string.Empty;
                }
            }

            InitCommands();
        }

        private void InitCommands()
        {
            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ Tên thiết bị!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                IsSaved = true;

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
                    ImagePath = openFileDialog.FileName;
                    MessageBox.Show("Đã chọn ảnh thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });

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