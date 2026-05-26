using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Xaml.Behaviors.Media;
using QLTB.Data;
using QLTB.Helpers;
using QLTB.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class IncidentDetailVM : BaseViewModel
    {
        private IncidentReportData data;
        private string currentPath;
        private ObservableCollection<DichVuBaoTri> listDichVu;
        private ObservableCollection<NhanVien> listNhanVien;
        private ObservableCollection<NhanVien> filterNameList;
        private ObservableCollection<BaoTri> listBaoTri;

        private bool canEdit;

        private int selectedServiceId;
        private string servicePrice;
        private string selectedSpecial;
        private int selectedStaff;

        public int SelectedServiceId
        {
            get => selectedServiceId; set
            {
                selectedServiceId = value;
                OnPropertyChanged(nameof(SelectedServiceId));
                UpdatePrice();
            }
        }

        public string ServicePrice
        {
            get => servicePrice; set
            {
                servicePrice = value;
                OnPropertyChanged(nameof(ServicePrice));
            }
        }

        public IncidentDetailVM(IncidentReportData data)
        {
            _ = LoadData(data);
            LoadCommand();
        }

        public IncidentReportData Data { get => data; set
            {
                data = value;
                OnPropertyChanged(nameof(Data));    
            }
                }

        public ICommand CloseForm {  get; set; }

        public ICommand DeleteReportCommand { get; set; }

        public ICommand SaveReportCommand { get; set; }
        public string CurrentPath { get => currentPath; set
            {
                currentPath = value;
                OnPropertyChanged(nameof(CurrentPath));
            }
                }

        public ObservableCollection<DichVuBaoTri> ListDichVu { get => listDichVu; set
            {
                listDichVu = value;
                OnPropertyChanged(nameof(ListDichVu));
            }
                }

        public ObservableCollection<NhanVien> ListNhanVien { get => listNhanVien; set
            {
                listNhanVien = value;
                OnPropertyChanged(nameof(ListNhanVien));
            }
                }

        public string SelectedSpecial { get => selectedSpecial; set
            {
                selectedSpecial = value;
                FilterName();
                OnPropertyChanged(nameof(SelectedSpecial));
            }
                }

        public ObservableCollection<NhanVien> FilterNameList { get => filterNameList; set
            {
                filterNameList = value;
                OnPropertyChanged(nameof(FilterNameList));
            }
                }

        public int SelectedStaff { get => selectedStaff; set
            {
                selectedStaff = value;
                OnPropertyChanged(nameof(SelectedStaff));
            }
                }

        public bool CanEdit { get => canEdit; set
            {
                canEdit = value;
                OnPropertyChanged(nameof(CanEdit));
            }
                }

        public ObservableCollection<BaoTri> ListBaoTri { get => listBaoTri; set => listBaoTri = value; }

        private void LoadCommand()
        {
            CloseForm = new RelayCommand<UserControl>
                (
                    p => true, p => Cloes(p)
                );
            DeleteReportCommand = new RelayCommand<UserControl>

                (
                    p => true,async p => await DeleteReport(p)
                );
            SaveReportCommand = new RelayCommand<UserControl>
                (
                    p => CheckCondition(), async p => await SaveReport(p)
                );
        }

        private bool CheckCondition()
        {
            return SelectedStaff > 0 && SelectedServiceId > 0;
        }
        private void Cloes(UserControl p)
        {
            PopUpService.ClosePopUp(p);
        }

        private async Task DeleteReport(UserControl p)
        {
            MessageBoxResult res = MessageBox.Show("Bạn có chắc muốn xóa báo cáo này?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    var record = await DataProvider.Instance.DB.BaoCaoSuaChuas.FirstOrDefaultAsync(x => x.IdbaoCao == Data.IdReport);
                    if (record != null)
                    {
                        // 1. Kiểm tra xem có IdBaoTri hợp lệ không
                        if (record.IdBaoTri != null && record.IdBaoTri != -1)
                        {
                            // 2. Tìm phiếu Bảo trì tương ứng
                            var baoTriRecord = await DataProvider.Instance.DB.BaoTris.FirstOrDefaultAsync(b => b.IdbaoTri == record.IdBaoTri);
                            if (baoTriRecord != null)
                            {
                                // THÊM MỚI: TRẢ LẠI TRẠNG THÁI "ĐANG RẢNH" CHO NHÂN VIÊN
                                if (baoTriRecord.IdnhanVien != null)
                                {
                                    // Tìm nhân viên trong DB và cập nhật
                                    var nhanVienDb = await DataProvider.Instance.DB.NhanViens.FirstOrDefaultAsync(nv => nv.IdnhanVien == baoTriRecord.IdnhanVien);
                                    if (nhanVienDb != null)
                                    {
                                        nhanVienDb.TinhTrang = "Đang rảnh"; // Thay TinhTrang bằng tên cột thực tế của bạn
                                    }

                                    // Cập nhật lại UI nếu ListNhanVien đang tồn tại
                                    if (ListNhanVien != null)
                                    {
                                        var nhanVienUi = ListNhanVien.FirstOrDefault(nv => nv.IdnhanVien == baoTriRecord.IdnhanVien);
                                        if (nhanVienUi != null)
                                        {
                                            nhanVienUi.TinhTrang = "Đang rảnh";
                                        }
                                    }
                                }

                                // Đưa lệnh xóa Bảo trì vào hàng đợi
                                DataProvider.Instance.DB.BaoTris.Remove(baoTriRecord);
                            }
                        }

                        // 3. Đưa lệnh xóa Báo cáo vào hàng đợi
                        DataProvider.Instance.DB.BaoCaoSuaChuas.Remove(record);

                        // 4. Lưu tất cả thay đổi xuống DB cùng một lúc (Gồm: Sửa NhanVien, Xóa BaoTri, Xóa BaoCao)
                        await DataProvider.Instance.DB.SaveChangesAsync();

                        MessageBox.Show("Xóa thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        PopUpService.ClosePopUp(p);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Có lỗi xảy ra khi xóa dữ liệu. Có thể do dữ liệu này đang được ràng buộc ở nơi khác.\n\nChi tiết lỗi: {ex.Message}", "Lỗi xóa dữ liệu", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task SaveReport(UserControl p)
        {
            MessageBoxResult res = MessageBox.Show(
                "Xác nhận lưu báo cáo. Các thông tin chỉnh sửa sẽ được thực hiện ở Danh sách bảo trì",
                "Thông báo",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (res != MessageBoxResult.Yes) return;

            try
            {
                using (var context = new QuanLyVatTuContext())
                {
                    // 1. TẠO PHIẾU BẢO TRÌ CHUNG
                    BaoTri newBaoTri = new BaoTri()
                    {
                        IdnhanVien = SelectedStaff,
                        NgayBaoTri = DateTime.Now,
                        GhiChu = Data.Description,
                        TinhTrangBaoTri = "Đang xử lý",
                        DoUuTien = Data.Priority switch
                        {
                            "Chưa xác định" => "Thấp",
                            "Thấp" => "Thấp",
                            "Trung bình" => "Trung bình",
                            _ => "Cao"
                        }
                    };

                    context.BaoTris.Add(newBaoTri);
                    await context.SaveChangesAsync();

                    // 2. TẠO CHI TIẾT BẢO TRÌ CHO THIẾT BỊ BỊ SỰ CỐ
                    ChiTietBaoTri chiTiet = new ChiTietBaoTri()
                    {
                        IdbaoTri = newBaoTri.IdbaoTri,
                        IdthietBi = Data.IdDevice,
                        SoSeri = Data.SeriNumber,
                        IddichVu = SelectedServiceId,
                        GhiChuThietBi = Data.Description,
                        TienDo = "Đang xử lý",
                        KetQua = null
                    };

                    context.ChiTietBaoTris.Add(chiTiet);

                    // 3. CẬP NHẬT LẠI PHIẾU BÁO CÁO SỰ CỐ
                    var baoCao = await context.BaoCaoSuaChuas
                        .FirstOrDefaultAsync(x => x.IdbaoCao == Data.IdReport);

                    if (baoCao != null)
                    {
                        baoCao.TrangThai = "Đang xử lý";
                        baoCao.IdBaoTri = newBaoTri.IdbaoTri;

                        Data.Status = "Đang xử lý";
                        Data.IdBaoTri = newBaoTri.IdbaoTri;
                    }

                    // 4. CẬP NHẬT TRẠNG THÁI THIẾT BỊ
                    var chiTietThietBi = await context.ChiTietThietBis
                        .FirstOrDefaultAsync(x => x.IdthietBi == Data.IdDevice
                                               && x.SoSeri == Data.SeriNumber);

                    if (chiTietThietBi != null)
                    {
                        chiTietThietBi.TinhTrang = "Đang bảo trì";
                    }

                    // 5. CẬP NHẬT TRẠNG THÁI NHÂN VIÊN
                    if (SelectedStaff != null)
                    {
                        var nhanVienDb = await context.NhanViens
                            .FirstOrDefaultAsync(nv => nv.IdnhanVien == SelectedStaff);

                        if (nhanVienDb != null)
                        {
                            nhanVienDb.TinhTrang = "Đang bận";
                        }

                        if (ListNhanVien != null)
                        {
                            var nhanVienUi = ListNhanVien
                                .FirstOrDefault(nv => nv.IdnhanVien == SelectedStaff);

                            if (nhanVienUi != null)
                            {
                                nhanVienUi.TinhTrang = "Đang bận";
                            }
                        }
                    }

                    await context.SaveChangesAsync();

                    MessageBox.Show(
                        "Đã tạo phiếu bảo trì và phân công công việc thành công!",
                        "Thành công",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    PopUpService.ClosePopUp(p);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Có lỗi xảy ra trong quá trình lưu:\n{ex.Message}",
                    "Lỗi hệ thống",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task LoadDichVu()
        {
            try
            {
                using (var context = new QuanLyVatTuContext())
                {
                    var listdv = await context.DichVuBaoTris.ToListAsync();
                    ListDichVu = new ObservableCollection<DichVuBaoTri>(listdv);
                    
                }
            }
            catch

            {

            }
        }

        private async Task LoadNhanVien()
        {
            try
            {
                using (var context = new QuanLyVatTuContext())
                {
                    var listnv = await context.NhanViens.ToListAsync(); // chỉ lọc ra những người đang rảnh
                    ListNhanVien = new ObservableCollection<NhanVien>(listnv);
                    FilterNameList = new ObservableCollection<NhanVien>(ListNhanVien.Where(x => x.TinhTrang == "Đang rảnh"));
                    
                }
            }
            catch
            {

            }
        }
        private async Task LoadData(IncidentReportData data)
        {
            try
            {
                CanEdit = true;
                Data = data;

                CurrentPath = CloudinaryService.GetImageUrl(
                    KeyData.ReportFolder,
                    KeyData.BaoCaoTag + Data.IdReport
                );

                await LoadDichVu();
                await LoadNhanVien();

                if (Data.IdBaoTri != -1)
                {
                    CanEdit = false;

                    using (var context = new QuanLyVatTuContext())
                    {
                        var baoTri = await context.BaoTris
                            .Include(x => x.ChiTietBaoTris)
                            .FirstOrDefaultAsync(x => x.IdbaoTri == Data.IdBaoTri);

                        if (baoTri != null)
                        {
                            var chiTiet = baoTri.ChiTietBaoTris.FirstOrDefault();

                            if (chiTiet != null && chiTiet.IddichVu.HasValue)
                            {
                                SelectedServiceId = chiTiet.IddichVu.Value;
                            }

                            if (ListNhanVien != null && baoTri.IdnhanVien.HasValue)
                            {
                                var staff = ListNhanVien
                                    .FirstOrDefault(x => x.IdnhanVien == baoTri.IdnhanVien.Value);

                                if (staff != null)
                                {
                                    SelectedSpecial = staff.ChuyenMon;
                                }
                            }

                            if (baoTri.IdnhanVien.HasValue)
                            {
                                SelectedStaff = baoTri.IdnhanVien.Value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tại LoadData: {ex.Message}");
                MessageBox.Show("Lỗi tải dữ liệu chi tiết sự cố:\n" + ex.Message);
            }
        }

        private void FilterName()
        {
            if (string.IsNullOrEmpty(SelectedSpecial))
            {
                return;
            }
            SelectedStaff = 0;
            var filterlist = ListNhanVien.Where(x => x.ChuyenMon == SelectedSpecial && x.TinhTrang =="Đang rảnh");
            FilterNameList = new ObservableCollection<NhanVien>(filterlist);
        }
        private void UpdatePrice()
        {
            // Kiểm tra an toàn xem list đã có dữ liệu chưa
            if (listDichVu != null && SelectedServiceId > 0)
            {
                // Dùng FirstOrDefault để tìm Dịch vụ có ID khớp với SelectedServiceId
                var selectedService = listDichVu.FirstOrDefault(x => x.IddichVu == SelectedServiceId);

                if (selectedService != null)
                {
                    // Lấy giá ra và format thành kiểu tiền tệ (ví dụ: 150.000 VNĐ)
                    ServicePrice = string.Format("{0:N0} VNĐ", selectedService.GiaDichVu);
                }
                else
                {
                    ServicePrice = "0 VNĐ"; // Không tìm thấy thì cho về 0
                }
            }
            else
            {
                ServicePrice = "0 VNĐ"; // Nếu chưa chọn gì thì cũng cho về 0
            }
        }
    }
}
