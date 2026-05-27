using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using OpenTK.Graphics.OpenGL;
using QLTB.Data;
using QLTB.Helpers;
using QLTB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;



namespace QLTB.ViewModel
{
    public class IncidentReportFormVM : BaseViewModel
    {
        private List<ThietBi> devices;
        private int selectedDevice;
        private string seriNumber;
        private string reportedName;
        private string phoneNumber;
        private string problemDescription;
        private string currentPath;
        private UserControl uc;
        public ICommand CloseForm {  get; set; }

        public ICommand SelectPhotosCommand { get; set; }
        public ICommand ConfirmReportCommand { get; set; }
        public List<ThietBi> Devices { get => devices; set
            {
                devices = value;
                OnPropertyChanged(nameof(Devices));
            }
                }

        public int SelectedDevice { get => selectedDevice; set
            {
                selectedDevice = value;
                OnPropertyChanged(nameof(SelectedDevice));
            }
                }

        public string SeriNumber { get => seriNumber; set
            {
                seriNumber = value;
                OnPropertyChanged(nameof(SeriNumber));
            }
                }

        public string ReportedName { get => reportedName; set
            {
                reportedName = value;
                OnPropertyChanged(nameof(ReportedName));
            }
                }

        public string PhoneNumber { get => phoneNumber; set
            {
                phoneNumber = value;
                OnPropertyChanged(nameof(PhoneNumber));
            }
                }
        public string ProblemDescription { get => problemDescription; set
            {
                problemDescription = value;
                OnPropertyChanged(nameof(ProblemDescription));
            }
                }

        public string CurrentPath { get => currentPath; set
            {
                currentPath = value;
                OnPropertyChanged(nameof(CurrentPath));
            }
                }

        public IncidentReportFormVM()
        {
            
            LoadCommand();
            _ = LoadAllData();
        }
        private void LoadCommand()
        {
            CloseForm = new RelayCommand<UserControl>
                (
                p => true, p => Close(p)
                );
            SelectPhotosCommand = new RelayCommand<object>
                (
                    p => true, p => OpenPhoto()
                );
            ConfirmReportCommand = new RelayCommand<UserControl>
                (
                    p=> CheckInputCondition() , async p=> await SaveReport(p)
                );
        }

        private async Task SaveReport(UserControl p)
        {
            var ct = await DataProvider.Instance.DB.ChiTietThietBis.AnyAsync(x => x.IdthietBi == SelectedDevice && x.SoSeri == SeriNumber);

            if (ct != null)
            {
                // ==========================================
                // PHẦN THÊM MỚI: KIỂM TRA TRẠNG THÁI BÁO CÁO CŨ
                // ==========================================
                // Lưu ý: Bạn hãy đổi chữ "TrangThai" thành đúng tên cột lưu trạng thái trong DB của bạn (ví dụ: TinhTrang, Status...)
                bool isAlreadyReported = await DataProvider.Instance.DB.BaoCaoSuaChuas.AnyAsync(x =>
                    x.IdthietBi == SelectedDevice &&
                    x.SoSeri == SeriNumber &&
                    (x.TrangThai == "Vừa cập nhật" || x.TrangThai == "Đang xử lý"));

                if (isAlreadyReported)
                {
                    MessageBox.Show("Thiết bị này đang có báo cáo sự cố chờ xử lý. Không thể tạo thêm báo cáo mới!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Dừng chạy hàm, không lưu thêm xuống DB
                }
                // ==========================================

                BaoCaoSuaChua bc = new BaoCaoSuaChua()
                {
                    IdthietBi = SelectedDevice,
                    SoSeri = SeriNumber,
                    Sdt = PhoneNumber,
                    TenNguoiBaoCao = ReportedName,
                    GhiChu = ProblemDescription,
                    NgayBaoCao = DateTime.Today,
                    MucDoNghiemTrong = "Chưa xác định",
                };

                await DataProvider.Instance.DB.BaoCaoSuaChuas.AddAsync(bc);
                await DataProvider.Instance.DB.SaveChangesAsync();

                if (CurrentPath != null)
                {
                    await CloudinaryService.UploadImageAsync(CurrentPath, KeyData.ReportFolder, KeyData.BaoCaoTag + bc.IdbaoCao);
                }

                MessageBox.Show("Báo cáo sự cố thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                PopUpService.ClosePopUp(p);
            }
            else
            {
                MessageBox.Show("Không tồn tại thiết bị.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CheckInputCondition()
        {
            return SelectedDevice != null && SeriNumber != null && SeriNumber.Length > 0 && ReportedName != null
                && ReportedName.Length > 0 && PhoneNumber != null && PhoneNumber.Length > 0 && ProblemDescription != null
                && ProblemDescription.Length > 0;
        }
        private void OpenPhoto()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Thiết lập bộ lọc chỉ cho phép chọn đuôi .png, .jpg và .jpeg
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            // Đặt tiêu đề cho cửa sổ chọn file
            openFileDialog.Title = "Chọn hình ảnh sự cố";

            // (Tùy chọn) Cho phép chọn nhiều ảnh hay không. Ở đây đang để false (chỉ chọn 1)
            openFileDialog.Multiselect = false;

            // Hiển thị hộp thoại và kiểm tra xem người dùng đã chọn file hay chưa
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                // Lưu đường dẫn của tệp đã chọn vào biến CurrentPath
                CurrentPath = openFileDialog.FileName;
            }
        }
        private void Close(UserControl p)
        {
            PopUpService.ClosePopUp(p);
        }

        private async Task LoadDevice()
        {
            using (var context = new QuanLyVatTuContext())
            {
                var list = await context.ThietBis.ToListAsync();
                Devices = list;
                SelectedDevice = 1;
            }
        }

        private async Task LoadAllData()
        {
            try
            {
                await LoadDevice();
            }
            catch
            {

            }
        }
    }
}
