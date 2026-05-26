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

            var listChiTietBaoTri = await DataProvider.Instance.DB.ChiTietBaoTris
                .Include(x => x.IdbaoTriNavigation)
                .ToListAsync();

            DateTime today = DateTime.Today;

            foreach (ChiTietThietBi tb in listThietBi)
            {
                var Tb = listGocThietBi.FirstOrDefault(x => x.IdthietBi == tb.IdthietBi);

                if (Tb == null) continue;

                var lastBaoTri = listChiTietBaoTri
                    .Where(x => x.IdthietBi == tb.IdthietBi && x.SoSeri == tb.SoSeri)
                    .OrderByDescending(x => x.IdbaoTriNavigation.NgayBaoTri)
                    .FirstOrDefault();

                DateTime? baseDate = lastBaoTri?.IdbaoTriNavigation?.NgayBaoTri
                                     ?? Tb.NgayNhapThietBi;

                if (baseDate.HasValue && Tb.BaoHanhDinhKy.HasValue)
                {
                    double value = Tb.BaoHanhDinhKy.Value;
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
                var query = DataProvider.Instance.DB.BaoTris.AsQueryable();

                query = query.Where(x => x.NgayBaoTri.HasValue
                                      && x.NgayBaoTri.Value.Date >= FromDate.Date
                                      && x.NgayBaoTri.Value.Date <= ToDate.Date);

                if (!string.IsNullOrEmpty(SelectedState) && SelectedState != "Tất cả")
                {
                    query = query.Where(x => x.TinhTrangBaoTri == SelectedState);
                }

                if (SelectedNVId != -1)
                {
                    query = query.Where(x => x.IdnhanVien == SelectedNVId);
                }

                var listBaoTri = await query
                    .Include(x => x.IdnhanVienNavigation)
                    .Include(x => x.ChiTietBaoTris)
                        .ThenInclude(ct => ct.ChiTietThietBi)
                            .ThenInclude(cttb => cttb.IdthietBiNavigation)
                    .Include(x => x.ChiTietBaoTris)
                        .ThenInclude(ct => ct.ChiTietThietBi)
                            .ThenInclude(cttb => cttb.IdphongBanNavigation)
                    .Include(x => x.ChiTietBaoTris)
                        .ThenInclude(ct => ct.IddichVuNavigation)
                    .ToListAsync();

                var result = listBaoTri
                    .SelectMany(bt => bt.ChiTietBaoTris.Select(ct => new ThongTinBaoTri
                    {
                        DeviceName = ct.ChiTietThietBi?.IdthietBiNavigation?.TenThietBi ?? "",
                        AssetCode = ct.SoSeri,
                        JobType = ct.IddichVuNavigation?.TenDichVu ?? "",
                        StartDate = bt.NgayBaoTri ?? DateTime.Today,
                        EndDate = CalculateEndDate(
                            bt.NgayBaoTri,
                            ct.IddichVuNavigation?.Value,
                            ct.IddichVuNavigation?.Unit
                        ),
                        Status = bt.TinhTrangBaoTri ?? "",
                        IdPhongCuaThietBi = ct.ChiTietThietBi?.IdphongBan ?? -1,
                        EmployeeName = bt.IdnhanVienNavigation?.HoTen ?? ""
                    }))
                    .ToList();

                if (SelectedID != -1)
                {
                    result = result
                        .Where(x => x.IdPhongCuaThietBi == SelectedID)
                        .ToList();
                }

                ListBaoTri = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lọc danh sách bảo trì:\n" + ex.Message);
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