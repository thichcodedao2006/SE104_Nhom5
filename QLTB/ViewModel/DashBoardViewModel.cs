using Microsoft.EntityFrameworkCore;
using QLTB.Data;
using QLTB.Helpers;
using QLTB.Models;
using QLTB.UserControlFolder.IncidentReport;
using QLTB.UserControlFolder.Maintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class ThongTinBaoTri
    {
        public string DeviceName {  get; set; }
        public string AssetCode { get; set; }

        public string JobType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string EmployeeName { get; set; }

        public string Status { get; set; }

        public int IdPhongCuaThietBi { get; set; }


    }
    public class DashBoardViewModel : BaseViewModel
    {
        #region Property 
        private string totalDevice;
        private string deviceNeedRepair;
        private string deviceOverRepair;
        private DateTime fromDate;
        private DateTime toDate;
        private int selectedID;
        private int selectedNVId;
        private string selectedState;
        private bool canShow;
        

        // SỬA: Đổi selectedID thành kiểu nullable (int?) để dễ quản lý trạng thái null/"Tất cả"
        private List<PhongBan> listPhongBan;
        private List<NhanVien> listNhanVien;
        private List<ThongTinBaoTri> listBaoTri;

        // SỬA: Bổ sung OnPropertyChanged cho ListPhongBan để ComboBox nhận được data
        public List<PhongBan> ListPhongBan
        {
            get => listPhongBan;
            set { listPhongBan = value; OnPropertyChanged(nameof(ListPhongBan)); }
        }

        public string TotalDevice { get => totalDevice; set { totalDevice = value; OnPropertyChanged(nameof(TotalDevice)); } }
        public string DeviceNeedRepair { get => deviceNeedRepair; set { deviceNeedRepair = value; OnPropertyChanged(nameof(DeviceNeedRepair)); } }
        public string DeviceOverRepair { get => deviceOverRepair; set { deviceOverRepair = value; OnPropertyChanged(nameof(DeviceOverRepair)); } }
        public DateTime FromDate { get => fromDate; set { fromDate = value; OnPropertyChanged(nameof(FromDate)); } }
        public DateTime ToDate { get => toDate; set { toDate = value; OnPropertyChanged(nameof(ToDate)); } }

        public int SelectedID
        {
            get => selectedID;
            set
            {
                selectedID = value;
                OnPropertyChanged(nameof(SelectedID)); 
            }
        }

        public int SelectedNVId { get => selectedNVId; set
            {
                selectedNVId = value;
                OnPropertyChanged(nameof(SelectedNVId));
            }
                }

        public List<NhanVien> ListNhanVien { get => listNhanVien; set
            {
                listNhanVien = value;
                OnPropertyChanged(nameof(ListNhanVien));
            }
                }

        public string SelectedState { get => selectedState; set
            {
                selectedState = value;
                OnPropertyChanged(nameof(SelectedState));
            }
                }

        public List<ThongTinBaoTri> ListBaoTri { get => listBaoTri; set
            {

                listBaoTri = value; OnPropertyChanged(nameof (ListBaoTri));
            }
                }

        public bool CanShow { get => canShow; set
            {
                canShow = value;
                OnPropertyChanged(nameof(CanShow));
            }
                }
        #endregion

        // CONSTRUCTOR
        public DashBoardViewModel(TaiKhoan t)
        {
            CanShow = t.LoaiTaiKhoan == 2 ? false : true;
            Reset();
            // SỬA CỐT LÕI: Không gọi trực tiếp các hàm async tại đây nữa, tránh xung đột luồng
            _ = LoadAllDataAsync();
            LoadCommand();

        }

        #region Command 
        public ICommand ResetCommand { get; set; }
        public ICommand FilterCommand { get; set; }

        public ICommand OpenBaoTriFormCommand {  get; set; }

        public ICommand OpenIncidentFormCommand { get; set; }

        #endregion

        #region Function

        // Hàm trung gian quản lý tuần tự các tác vụ bất đồng bộ
        private async Task LoadAllDataAsync()
        {
            try
            {
                await Initialize(); // Đợi lấy xong danh sách phòng ban...
                await Reload();     // Rồi mới tính toán số lượng thiết bị
            }
            catch (Exception ex)
            {
                // Log lỗi nếu có (ví dụ lỗi kết nối DB)
                System.Diagnostics.Debug.WriteLine($"Lỗi nạp dữ liệu: {ex.Message}");
            }
        }

        public async Task Initialize()
        {
            // Khởi tạo List Phòng 
            // 1. Lấy danh sách từ DB
            var dbList = await DataProvider.Instance.DB.PhongBans.ToListAsync();

            // 2. Tạo một List mới để tránh can thiệp trực tiếp vào Tracker của EF Core
            var customList = new List<PhongBan>();


            customList.Add(new PhongBan { Idphong = -1, TenPhong = "Tất cả" });
            customList.AddRange(dbList);

            // 3. Gán vào property (Trigger sẽ báo cho UI cập nhật ComboBox)
            ListPhongBan = customList;

            SelectedID = -1;

            // Khởi tạo List nhân viên 
            var dbListnv = await DataProvider.Instance.DB.NhanViens.ToListAsync();

            var NVList = new List<NhanVien>();

            NVList.Add(new NhanVien { IdnhanVien = -1, HoTen = "Tất cả" });
            NVList.AddRange(dbListnv);

            ListNhanVien = NVList;

            SelectedNVId = -1;
            SelectedState = "Tất cả";

        }

        public async Task Reload()
        {
            int totaldev = await DataProvider.Instance.DB.ChiTietThietBis.CountAsync();
            TotalDevice = totaldev.ToString();

            int deviceNeedRepair = 0;
            int deviceOverRepair = 0;

            var listThietBi = await DataProvider.Instance.DB.ChiTietThietBis
                                                        .Where(x => x.TinhTrang == "Tốt")
                                                        .ToListAsync();

            var listGocThietBi = await DataProvider.Instance.DB.ThietBis.ToListAsync();
            var listBaoTri = await DataProvider.Instance.DB.BaoTris.ToListAsync();

            DateTime today = DateTime.Today;

            foreach (ChiTietThietBi tb in listThietBi)
            {
                var Tb = listGocThietBi.FirstOrDefault(x => x.IdthietBi == tb.IdthietBi);
                var bt = listBaoTri.FirstOrDefault(x => x.IdthietBi == tb.IdthietBi && x.SoSeri == tb.SoSeri);

                if (Tb == null) continue;

                DateTime? baseDate = bt != null ? bt.NgayBaoTri : Tb.NgayNhapThietBi;

                if (baseDate.HasValue && Tb.BaoHanhDinhKy.HasValue)
                {
                    double value = (double)Tb.BaoHanhDinhKy.Value;
                    int valueInt = Tb.BaoHanhDinhKy.Value;

                    DateTime nextRepairDay = Tb.DonViThoiGian switch
                    {
                        0 => baseDate.Value.AddMinutes(value),
                        1 => baseDate.Value.AddHours(value),
                        2 => baseDate.Value.AddDays(value),
                        3 => baseDate.Value.AddMonths(valueInt),
                        4 => baseDate.Value.AddYears(valueInt),
                        _ => baseDate.Value
                    };

                    if (nextRepairDay.Date == today)
                    {
                        deviceNeedRepair++;
                    }
                    else if (nextRepairDay.Date < today)
                    {
                        deviceOverRepair++;
                    }
                }
            }

            DeviceNeedRepair = deviceNeedRepair.ToString();
            DeviceOverRepair = deviceOverRepair.ToString();
        }

        private void LoadCommand()
        {
            FilterCommand = new RelayCommand<object>
            (
                (p) => true,  async (p) =>  await Filter()

            );
            ResetCommand = new RelayCommand<object>
                (
                    (p) => true, (p) => Reset()
                    );
            OpenBaoTriFormCommand = new RelayCommand<object>
                (
                    (p) => true, (p) => OpenBaoTriForm()
                );
            OpenIncidentFormCommand = new RelayCommand<object>
                (
                    (p) => true, (p) => OpenIncidentForm()
                );
        }

        private async Task Filter()
        {
            try
            {
                // 1. Khởi tạo câu truy vấn gốc từ bảng BaoTris (chưa chạy xuống DB)
                var query = DataProvider.Instance.DB.BaoTris.AsQueryable();

                query = query.Where(x => x.NgayBaoTri.HasValue
                              && x.NgayBaoTri.Value.Date >= FromDate.Date
                              && x.NgayBaoTri.Value.Date <= ToDate.Date);

                // 3. Lọc theo TRẠNG THÁI (Nếu chọn một trạng thái cụ thể khác "Tất cả")
                if (!string.IsNullOrEmpty(SelectedState) && SelectedState != "Tất cả")
                {
                    query = query.Where(x => x.TinhTrangBaoTri == SelectedState);
                }

                // 4. Lọc theo NHÂN VIÊN (Nếu chọn một nhân viên cụ thể khác "Tất cả")
                if (SelectedNVId != -1)
                {
                    query = query.Where(x => x.IdnhanVien == SelectedNVId);
                }

                // 5. THỰC THI GIAI ĐOẠN 1: Tải danh sách bảo trì đã lọc sơ bộ về bộ nhớ RAM
                var listBaoTriSơBộ = await query.ToListAsync();

                // 6. NẠP SẴN CÁC BẢNG LIÊN QUAN LÊN RAM (Bổ sung thêm bảng DichVus)
                var listChiTietTB = await DataProvider.Instance.DB.ChiTietThietBis.ToListAsync();
                var listThietBiGoc = await DataProvider.Instance.DB.ThietBis.ToListAsync();
                var listDichVu = await DataProvider.Instance.DB.DichVuBaoTris.ToListAsync(); // <-- THÊM DÒNG NÀY

                // 7. SỬ DỤNG LINQ TRÊN RAM ĐỂ JOIN 4 BẢNG VÀ XỬ LÝ LỆCH KIỂU
                var result = (from bt in listBaoTriSơBộ

                                  // Khớp kép ChiTietThietBi (Giải quyết lỗi int? và string? bằng cách ép kiểu an toàn)
                              join cttb in listChiTietTB
                              on new
                              {
                                  MaTB = bt.IdthietBi.HasValue ? bt.IdthietBi.Value : 0,
                                  Seri = bt.SoSeri ?? string.Empty
                              }
                              equals new
                              {
                                  MaTB = cttb.IdthietBi,
                                  Seri = cttb.SoSeri
                              }

                              // Join sang ThietBi gốc để lấy tên thiết bị
                              join tbGoc in listThietBiGoc on bt.IdthietBi equals tbGoc.IdthietBi

                              // BỔ SUNG: Join sang bảng DichVu để lấy tên dịch vụ bảo trì
                              join dv in listDichVu on bt.IddichVu equals dv.IddichVu // (Kiểm tra lại chữ hoa/thường của IdDichVu theo DB của bạn)

                              select new ThongTinBaoTri
                              {
                                  DeviceName = tbGoc.TenThietBi,
                                  AssetCode = bt.SoSeri,

                                  // Đã lấy được tên dịch vụ gán vào JobType sạch sẽ!
                                  JobType = dv.TenDichVu, // Thay bằng tên cột chứa tên dịch vụ thực tế dưới DB của bạn

                                  StartDate = bt.NgayBaoTri.Value,
                                  EndDate = CalculateEndDate(bt.NgayBaoTri, dv.Value, dv.Unit),
                                  Status = bt.TinhTrangBaoTri,
                                  IdPhongCuaThietBi = cttb.IdphongBan.Value,
                                  EmployeeName = ListNhanVien.FirstOrDefault(nv => nv.IdnhanVien == bt.IdnhanVien)?.HoTen
                              }).ToList();

                // 8. LỌC THEO PHÒNG BAN (Sau khi đã có IdPhong từ bảng ChiTietThietBi ở bước Join)
                if (SelectedID != -1)
                {
                    result = result.Where(x => x.IdPhongCuaThietBi == SelectedID).ToList();
                }

                // 9. CẬP NHẬT LÊN DATAGRID GIAO DIỆN
                ListBaoTri = result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lọc danh sách bảo trì: {ex.Message}");
            }
        }
        private void Reset()
        {
            ListBaoTri = new List<ThongTinBaoTri>();
            SelectedID = -1;
            SelectedNVId = -1;
            SelectedState = "Tất cả";
            FromDate = DateTime.Today;
            ToDate = DateTime.Today;
        }

        private DateTime CalculateEndDate(DateTime? ngayBaoTri, int? value, int? unit)
        {
            // Nếu không có ngày bảo trì hoặc không cấu hình thời gian dịch vụ, mặc định trả về ngày hôm nay
            if (!ngayBaoTri.HasValue) return DateTime.Today;

            double val = value.HasValue ? (double)value.Value : 0;
            int valInt = value.HasValue ? value.Value : 0;

            // Dựa vào trường đơn vị (unit) để cộng thêm thời gian tương ứng
            // Hãy kiểm tra lại quy ước số (0,1,2,3,4) dưới Database của bạn xem khớp chưa nhé
            return unit switch
            {
                0 => ngayBaoTri.Value.AddMinutes(val), // Phút
                1 => ngayBaoTri.Value.AddHours(val),   // Giờ
                2 => ngayBaoTri.Value.AddDays(val),    // Ngày
                3 => ngayBaoTri.Value.AddMonths(valInt),// Tháng
                4 => ngayBaoTri.Value.AddYears(valInt), // Năm
                _ => ngayBaoTri.Value // Mặc định giữ nguyên nếu lỗi unit
            };
        }

        private void OpenBaoTriForm()
        {
            MaintenancePlanFormView plan = new MaintenancePlanFormView();
            PopUpService.ShowPopUp(plan);
        }

        private void OpenIncidentForm()
        {
            IncidentReportFormView incident = new IncidentReportFormView();
            PopUpService.ShowPopUp(incident);
        }
        #endregion
    }
}