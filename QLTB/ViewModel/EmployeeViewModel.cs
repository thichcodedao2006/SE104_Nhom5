using Microsoft.EntityFrameworkCore;
using QLTB.Data;
using QLTB.Helpers;
using QLTB.Models;
using QLTB.UserControlFolder.Employee;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace QLTB.ViewModel
{
    public class Employee
    {
        // Bạn nên thêm Id vào để sau này làm chức năng Sửa/Xóa cho dễ nhé
        public int Id { get; set; }
        public string HoTen { get; set; }
        public string ChucDanh { get; set; }
        public string Email { get; set; }
        public string Sdt { get; set; }
        public string BoPhan { get; set; }
        public string TinhTrang { get; set; }
        public Brush StatusColor => TinhTrang == "Đang rảnh" ? Brushes.Green : (TinhTrang == "Đang bận" ? Brushes.Red : Brushes.Gray);
    }

    public class EmployeeViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Employee> Employees { get; set; }

        private ObservableCollection<Employee> _filteredEmployees;
        public ObservableCollection<Employee> FilteredEmployees
        {
            get => _filteredEmployees;
            set
            {
                _filteredEmployees = value;
                OnPropertyChanged(nameof(FilteredEmployees));
            }
        }

        private string _searchText;
        private string _filterText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                FilterEmployees();
                OnPropertyChanged(nameof(SearchText));
            }
        }

        private int totalEmployees;
        private int activeEmployees;
        private int departmentsCount;
        private int rolesCount;

        private ObservableCollection<NhanVien> ListNhanVien;
        private ObservableCollection<BoPhan> ListBoPhan;
        private ObservableCollection<ChucDanh> ListChucDanh;
        private ObservableCollection<TaiKhoan> ListTaiKhoan;

        public ICommand AddEmployeeCommand { get; set; }
        public ICommand EditEmployeeCommand { get; set; }
        public ICommand DeleteEmployeeCommand { get; set; }

        public int TotalEmployees
        {
            get => totalEmployees;
            set
            {
                totalEmployees = value;
                OnPropertyChanged(nameof(TotalEmployees));
            }
        }

        public int ActiveEmployees
        {
            get => activeEmployees;
            set
            {
                activeEmployees = value;
                OnPropertyChanged(nameof(ActiveEmployees));
            }
        }

        public int DepartmentsCount
        {
            get => departmentsCount;
            set
            {
                departmentsCount = value;
                OnPropertyChanged(nameof(DepartmentsCount));
            }
        }

        public int RolesCount
        {
            get => rolesCount;
            set
            {
                rolesCount = value;
                OnPropertyChanged(nameof(RolesCount));
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                // Khi người dùng đổi tiêu chí lọc trong ComboBox, tự động lọc lại
                FilterEmployees();
                OnPropertyChanged(nameof(FilterText));
            }
        }

        public EmployeeViewModel()
        {
            FilterText = "Tên nhân viên";
            _ = LoadAllData();


            LoadCommand();
        }

        // === HÀM ÁNH XẠ (MAPPING) DỮ LIỆU TỪ DB LÊN UI ===
        private Employee MapToEmployeeUI(NhanVien nv)
        {
            // Đi tìm tên bộ phận và chức danh tương ứng
            var bp = ListBoPhan?.FirstOrDefault(b => b.Id == nv.IdboPhan);
            var cd = ListChucDanh?.FirstOrDefault(c => c.Id == nv.IdchucDanh);

            return new Employee
            {
                Id = nv.IdnhanVien,
                HoTen = nv.HoTen ?? "Chưa cập nhật",
                Email = nv.Email,
                Sdt = nv.Sdt,
                TinhTrang = nv.TinhTrang ?? "Không rõ",
                BoPhan = bp != null ? bp.TenBoPhan : "Chưa có",
                ChucDanh = cd != null ? cd.TenChucDanh : "Chưa có"
            };
        }

        private void FilterEmployees()
        {
            if (ListNhanVien == null) return;

            // 1. Tạo biến hứng kết quả lọc tạm thời
            IEnumerable<NhanVien> query = ListNhanVien;

            // 2. Lọc dữ liệu nếu có chữ trong ô tìm kiếm
            if (!string.IsNullOrEmpty(SearchText))
            {
                string keyword = SearchText.ToLower();
                string criteria = FilterText ?? "Tên nhân viên";

                switch (criteria)
                {
                    case "Xác thực của tài khoản":
                        {
                            // 1. Phân tích ý định của người dùng từ chuỗi keyword
                            int? targetValue = null; // null nghĩa là gõ tào lao, chưa đoán được ý

                            // Nhóm từ khóa ám chỉ "ĐÃ XÁC THỰC" (map về 1)
                            if (keyword == "1" || keyword == "true" ||
                                keyword.Contains("đã") || keyword.Contains("rồi") || keyword.Contains("có"))
                            {
                                targetValue = 1;
                            }
                            // Nhóm từ khóa ám chỉ "CHƯA XÁC THỰC" (map về 0)
                            else if (keyword == "0" || keyword == "false" ||
                                     keyword.Contains("chưa") || keyword.Contains("không"))
                            {
                                targetValue = 0;
                            }

                            // 2. Tiến hành lọc dựa trên giá trị đã phiên dịch
                            if (targetValue.HasValue)
                            {
                                query = query.Where(e => ListTaiKhoan != null &&
                                                         ListTaiKhoan.Any(tk => tk.Email == e.Email && tk.DuocXacThuc == targetValue.Value));
                            }
                            else
                            {
                                // Nếu người dùng gõ những chữ không liên quan (ví dụ: "con mèo"), 
                                // thì không trả về kết quả nào cả để tránh sai lệch dữ liệu.
                                query = query.Where(e => false);
                            }
                            break;
                        }
                    case "Tình trạng":
                        query = query.Where(e => e.TinhTrang != null && e.TinhTrang.ToLower().Contains(keyword));
                        break;
                    case "Bộ phận":
                        query = query.Where(e => ListBoPhan != null && ListBoPhan.FirstOrDefault(bp => bp.Id == e.IdboPhan)?.TenBoPhan?.ToLower().Contains(keyword) == true);
                        break;
                    case "Chức vụ":
                        query = query.Where(e => ListChucDanh != null && ListChucDanh.FirstOrDefault(cd => cd.Id == e.IdchucDanh)?.TenChucDanh?.ToLower().Contains(keyword) == true);
                        break;
                    case "Tên nhân viên":
                    default:
                        query = query.Where(e => e.HoTen != null && e.HoTen.ToLower().Contains(keyword));
                        break;
                }
            }

            // 3. Biến hình List<NhanVien> thành List<Employee> và gán lên UI
            var mappedResult = query.Select(nv => MapToEmployeeUI(nv));
            FilteredEmployees = new ObservableCollection<Employee>(mappedResult);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public async Task Reload()
        {
            using (var context = new QuanLyVatTuContext())
            {
                var allNhanViens = await context.NhanViens.ToListAsync();

                if (allNhanViens != null)
                {
                    ListNhanVien = new ObservableCollection<NhanVien>(allNhanViens);
                }

                TotalEmployees = allNhanViens.Count;
                ActiveEmployees = allNhanViens.Count(x => x.TinhTrang == "Đang rảnh");
                DepartmentsCount = allNhanViens.Select(nv => nv.IdboPhan).Distinct().Count();
                RolesCount = allNhanViens.Select(nv => nv.IdchucDanh).Distinct().Count();
            }
        }

        private async Task InitializeData()
        {
            using (var context = new QuanLyVatTuContext())
            {
                var boPhans = await context.BoPhans.ToListAsync();
                ListBoPhan = new ObservableCollection<BoPhan>(boPhans);

                var chucDanhs = await context.ChucDanhs.ToListAsync();
                ListChucDanh = new ObservableCollection<ChucDanh>(chucDanhs);

                var taikhoans = await context.TaiKhoans.ToListAsync();
                ListTaiKhoan = new ObservableCollection<TaiKhoan>(taikhoans);   
            }
        }

        private void LoadCommand()
        {
            AddEmployeeCommand = new RelayCommand<object>
            (
                  p => true, p => OpenAddNV()
                );


        }

        private void OpenAddNV()
        {
            EmployeeFormView emp = new EmployeeFormView();
            PopUpService.ShowPopUp(emp);
        }
        private async Task LoadAllData()
        {
            try
            {
                await Reload();
                await InitializeData();

                if (ListNhanVien != null)
                {
                    // Chuyển đổi dữ liệu lần đầu tiên lúc vừa load xong
                    var mappedList = ListNhanVien.Select(nv => MapToEmployeeUI(nv));
                    FilteredEmployees = new ObservableCollection<Employee>(mappedList);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Thủ phạm làm số bằng 0 đây: \n" + ex.Message);
            }
        }
    }
}