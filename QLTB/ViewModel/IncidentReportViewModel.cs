using Microsoft.EntityFrameworkCore;
using QLTB.Helpers;
using QLTB.Models;
using QLTB.UserControlFolder.IncidentReport;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    // Model class for Incident Report
    public class IncidentReportData
    {
        public int IdReport {  get; set; }
        public int IdDevice { get; set; }

        public string SeriNumber { get; set; }
        public string DeviceName { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; } // Nghiêm trọng, Cao, Trung bình, Thấp, Chưa xác định
        public string Status { get; set; } // Vừa cập nhật, Đang xử lý, Đã giải quyết
        public string ReportedBy { get; set; }
        public string ReportedAt { get; set; }

        public int IdBaoTri {  get; set; }
    }

    public class IncidentReportViewModel : BaseViewModel
    {
        private ObservableCollection<BaoCaoSuaChua> listBaoCao;

        private ObservableCollection<ThietBi> listThietBi;
        private ObservableCollection<IncidentReportData> incidents;
        private ObservableCollection<IncidentReportData> baseList;

        private string selectedState;
        private string selectedWarning;

        // Statistics
        private int _totalIncidents;
        public int TotalIncidents
        {
            get => _totalIncidents;
            set
            {
                _totalIncidents = value;
                OnPropertyChanged(nameof(TotalIncidents));
            }
        }

        private int _openIncidents;
        public int OpenIncidents
        {
            get => _openIncidents;
            set
            {
                _openIncidents = value;
                OnPropertyChanged(nameof(OpenIncidents));
            }
        }

        private int _inProgressIncidents;
        public int InProgressIncidents
        {
            get => _inProgressIncidents;
            set
            {
                _inProgressIncidents = value;
                OnPropertyChanged(nameof(InProgressIncidents));
            }
        }

        private int _resolvedIncidents;
        public int ResolvedIncidents
        {
            get => _resolvedIncidents;
            set
            {
                _resolvedIncidents = value;
                OnPropertyChanged(nameof(ResolvedIncidents));
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                FilterReport();
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public ICommand ReportIncidentCommand { get; set; }
        public ICommand ViewDetailsCommand { get; set; }
        public string SelectedState { get => selectedState; set
            {
                selectedState = value;
                FilterReport();
                OnPropertyChanged(nameof(SelectedState));
            }
                }
        public string SelectedWarning { get => selectedWarning; set
            {
                selectedWarning = value;
                FilterReport();
                OnPropertyChanged(nameof(SelectedWarning));
            }
                }

        public ObservableCollection<IncidentReportData> Incidents { get => incidents; set
            {
                incidents = value;
                OnPropertyChanged(nameof(Incidents));
            }
                }

        
        public IncidentReportViewModel()
        {
     

            _ = Reload();

            LoadCommand();

            
        }

        private void LoadCommand()
        {
            ViewDetailsCommand = new RelayCommand(o =>
            {
                if (o is IncidentReportData incident)
                {
                    IncidentDetailView detail = new IncidentDetailView(incident);
                    PopUpService.ShowPopUp(detail);
                }
            });
        }

        public async Task Reload()
        {
            try
            {
                SelectedState = "Tất cả trạng thái";
                SelectedWarning = "Tất cả mức độ";
                using (var context = new QuanLyVatTuContext())
                {
                    var list = await context.BaoCaoSuaChuas.ToListAsync();
                    listBaoCao = new ObservableCollection<BaoCaoSuaChua>(list);

                    TotalIncidents = list.Count;
                    OpenIncidents = list.Count(x => x.TrangThai == "Vừa cập nhật");
                    InProgressIncidents = list.Count(x => x.TrangThai == "Đang xử lý");
                    ResolvedIncidents = list.Count(x => x.TrangThai == "Đã giải quyết");

                    var listtb = await context.ThietBis.ToListAsync();
                    listThietBi = new ObservableCollection<ThietBi>(listtb);

                    // ========================================================
                    // THỰC HIỆN KẾT NỐI (JOIN) VÀ MAP DỮ LIỆU SANG INCIDENT REPORT
                    // ========================================================
                    var query = from bc in listBaoCao
                                    // Kết nối với listThietBi. Dùng DefaultIfEmpty() (Left Join) 
                                    // để đề phòng trường hợp thiết bị đã bị xóa nhưng báo cáo vẫn còn
                                join tb in listThietBi on bc.IdthietBi equals tb.IdthietBi into tbGroup
                                from tb in tbGroup.DefaultIfEmpty()
                                select new IncidentReportData
                                {
                                    IdReport = bc.IdbaoCao,

                                    IdDevice = tb.IdthietBi,

                                    SeriNumber = bc.SoSeri,
                                    // Lấy tên thiết bị. Nếu không tìm thấy thì để "Không xác định"
                                    DeviceName = tb.TenThietBi,

                                    // CÁC TRƯỜNG DƯỚI ĐÂY BẠN TỰ SỬA LẠI TÊN PROPERTY CHO ĐÚNG NHÉ
                                    Description = bc.GhiChu,          // ??? Thay bằng trường mô tả lỗi của bạn
                                    Priority = bc.MucDoNghiemTrong,        // ??? Thay bằng trường ưu tiên của bạn
                                    Status = bc.TrangThai,                 // ??? Thay bằng trường trạng thái
                                    ReportedBy = bc.TenNguoiBaoCao,        // ??? Thay bằng người báo cáo

                                    // Chuyển kiểu DateTime sang string. Có thể format tùy ý (vd: "dd/MM/yyyy HH:mm")
                                    ReportedAt = bc.NgayBaoCao.Value.ToString("dd/MM/yyyy HH:mm"),

                                    IdBaoTri = bc.IdBaoTri.Value, // mặc định là -1 
                                };

                    // Gán dữ liệu đã xử lý xong vào Incidents để đẩy lên UI
                    baseList = new ObservableCollection<IncidentReportData>(query);
                    Incidents = new ObservableCollection<IncidentReportData>(baseList);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi nạp dữ liệu: {ex.Message}");
            }
        }

        private void FilterReport()
        {
            try
            {
                // Kiểm tra an toàn: nếu baseList chưa có dữ liệu thì không làm gì cả
                if (baseList == null || !baseList.Any())
                    return;

                // Bắt đầu với toàn bộ danh sách gốc
                var query = baseList.AsEnumerable();

                // 1. Lọc theo chuỗi tìm kiếm (SearchText)
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    string keyword = SearchText.ToLower().Trim();
                    // Lọc theo Tên thiết bị, Tên người báo cáo hoặc Mô tả (Bạn có thể thêm bớt tùy ý)
                    query = query.Where(x =>
                        (x.DeviceName != null && x.DeviceName.ToLower().Contains(keyword)) ||
                        (x.ReportedBy != null && x.ReportedBy.ToLower().Contains(keyword)) 
                    );
                }

                // 2. Lọc theo Trạng thái (SelectedState)
                if (!string.IsNullOrEmpty(SelectedState) && SelectedState != "Tất cả trạng thái")
                {
                    query = query.Where(x => x.Status == SelectedState);
                }

                // 3. Lọc theo Mức độ cảnh báo/Ưu tiên (SelectedWarning)
                if (!string.IsNullOrEmpty(SelectedWarning) && SelectedWarning != "Tất cả mức độ")
                {
                    query = query.Where(x => x.Priority == SelectedWarning);
                }

                // 4. Gán kết quả đã lọc vào Incidents để giao diện tự cập nhật
                Incidents = new ObservableCollection<IncidentReportData>(query.ToList());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lọc danh sách: {ex.Message}");
            }
        }
    }
}
