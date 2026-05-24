using QLTB.Helpers;
using QLTB.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

namespace QLTB.ViewModel
{
    public class MaintenanceEditViewModel : BaseViewModel
    {
        private readonly QuanLyVatTuContext _context;

        private string _deviceName;
        public string DeviceName { get => _deviceName; set { _deviceName = value; OnPropertyChanged(); } }

        private DateTime? _ngayBaoTri;
        public DateTime? NgayBaoTri { get => _ngayBaoTri; set { _ngayBaoTri = value; OnPropertyChanged(); } }

        private string _doUuTien;
        public string DoUuTien { get => _doUuTien; set { _doUuTien = value; OnPropertyChanged(); } }

        private string _tinhTrangBaoTri;
        public string TinhTrangBaoTri { get => _tinhTrangBaoTri; set { _tinhTrangBaoTri = value; OnPropertyChanged(); } }

        private int? _selectedStaffId;
        public int? SelectedStaffId { get => _selectedStaffId; set { _selectedStaffId = value; OnPropertyChanged(); } }

        private int? _selectedServiceId;
        public int? SelectedServiceId { get => _selectedServiceId; set { _selectedServiceId = value; OnPropertyChanged(); } }

        private string _selectedSerial;
        public string SelectedSerial { get => _selectedSerial; set { _selectedSerial = value; OnPropertyChanged(); } }

        private ObservableCollection<NhanVien> _staffList;
        public ObservableCollection<NhanVien> StaffList { get => _staffList; set { _staffList = value; OnPropertyChanged(); } }

        // Đã sửa đổi kiểu dữ liệu thành danh mục chuẩn DichVuBaoTri
        private ObservableCollection<DichVuBaoTri> _serviceList;
        public ObservableCollection<DichVuBaoTri> ServiceList { get => _serviceList; set { _serviceList = value; OnPropertyChanged(); } }

        public bool IsSaved { get; private set; } = false;
        public ICommand SaveCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public MaintenanceEditViewModel(MaintenanceDisplayItem item)
        {
            _context = new QuanLyVatTuContext();

            // Gán dữ liệu ban đầu từ dòng được chọn trong DataGrid
            DeviceName = item.TenThietBi;
            NgayBaoTri = item.NgayBaoTri;
            DoUuTien = item.DoUuTien;
            TinhTrangBaoTri = item.TinhTrangBaoTri;
            SelectedSerial = item.SoSeri;

            // Nạp danh sách nguồn dữ liệu chuẩn từ Database lên hệ thống
            StaffList = new ObservableCollection<NhanVien>(_context.NhanViens.ToList());
            ServiceList = new ObservableCollection<DichVuBaoTri>(_context.DichVuBaoTris.ToList());

            // Tự động tìm kiếm ID tương ứng để hiển thị trúng đích lên ComboBox
            var currentStaff = StaffList.FirstOrDefault(s => s.HoTen == item.TenNhanVien);
            if (currentStaff != null) SelectedStaffId = currentStaff.IdnhanVien;

            var currentService = ServiceList.FirstOrDefault(s => s.TenDichVu == item.TenDichVu);
            if (currentService != null) SelectedServiceId = currentService.IddichVu;

            SaveCommand = new RelayCommand(o => {
                IsSaved = true;
                if (o is Window w) w.Close();
            });

            CancelCommand = new RelayCommand(o => {
                IsSaved = false;
                if (o is Window w) w.Close();
            });
        }
    }
}