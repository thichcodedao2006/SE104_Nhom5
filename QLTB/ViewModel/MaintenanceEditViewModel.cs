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

            // ĐÃ SỬA: Chỉ lấy những nhân viên CHƯA nghỉ việc lên hệ thống ComboBox
            StaffList = new ObservableCollection<NhanVien>(_context.NhanViens.Where(nv => nv.TinhTrang != "Đã nghỉ việc").ToList());
            ServiceList = new ObservableCollection<DichVuBaoTri>(_context.DichVuBaoTris.ToList());

            // Tự động tìm kiếm ID tương ứng để hiển thị trúng đích lên ComboBox
            int? originalStaffId = null;
            var currentStaff = StaffList.FirstOrDefault(s => s.HoTen == item.TenNhanVien);
            if (currentStaff != null)
            {
                SelectedStaffId = currentStaff.IdnhanVien;
                originalStaffId = currentStaff.IdnhanVien;
            }

            var currentService = ServiceList.FirstOrDefault(s => s.TenDichVu == item.TenDichVu);
            if (currentService != null) SelectedServiceId = currentService.IddichVu;

            SaveCommand = new RelayCommand(o => {
                // KIỂM TRA PHÂN CÔNG BẬN/RẢNH TUYỆT ĐỐI
                if (SelectedStaffId.HasValue && SelectedStaffId != originalStaffId)
                {
                    var newStaff = _context.NhanViens.FirstOrDefault(nv => nv.IdnhanVien == SelectedStaffId.Value);

                    // Quét trực tiếp xem nhân viên này có lịch bảo trì nào đang dang dở không
                    bool hasActiveTask = _context.BaoTris.Any(b => b.IdnhanVien == SelectedStaffId.Value
                                                                && (b.TinhTrangBaoTri == "Đang xử lý" || b.TinhTrangBaoTri == "Quá hạn"));

                    // ĐÃ SỬA: Đối chiếu chuẩn theo từ khóa "Đang bận"
                    if (hasActiveTask || (newStaff != null && newStaff.TinhTrang == "Đang bận"))
                    {
                        MessageBox.Show($"Kỹ thuật viên [{newStaff?.HoTen}] hiện đang bận xử lý một ca bảo trì khác. Vui lòng chọn người khác hoặc đợi họ hoàn thành!", "Nhân sự đang bận", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return; // Chặn không cho đóng form hay lưu dữ liệu
                    }
                }

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