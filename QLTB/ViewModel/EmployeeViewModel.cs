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
using System.Windows.Threading;

namespace QLTB.ViewModel
{
    public class Employee : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string HoTen { get; set; }
        public string ChucDanh { get; set; }
        public string Email { get; set; }
        public string Sdt { get; set; }
        public string BoPhan { get; set; }

        private string _tinhTrang;
        public string TinhTrang
        {
            get => _tinhTrang;
            set
            {
                if (_tinhTrang != value)
                {
                    _tinhTrang = value;
                    OnPropertyChanged(nameof(TinhTrang));
                    OnPropertyChanged(nameof(StatusColor));
                }
            }
        }

        public string CurrentAvatar { get; set; }
        public Brush StatusColor => TinhTrang == "Đang rảnh" ? Brushes.Green : (TinhTrang == "Đang bận" ? Brushes.Red : Brushes.Gray);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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
            set { totalEmployees = value; OnPropertyChanged(nameof(TotalEmployees)); }
        }

        public int ActiveEmployees
        {
            get => activeEmployees;
            set { activeEmployees = value; OnPropertyChanged(nameof(ActiveEmployees)); }
        }

        public int DepartmentsCount
        {
            get => departmentsCount;
            set { departmentsCount = value; OnPropertyChanged(nameof(DepartmentsCount)); }
        }

        public int RolesCount
        {
            get => rolesCount;
            set { rolesCount = value; OnPropertyChanged(nameof(RolesCount)); }
        }

        public string FilterText
        {
            get => _filterText;
            set { _filterText = value; FilterEmployees(); OnPropertyChanged(nameof(FilterText)); }
        }

        public EmployeeViewModel()
        {
            FilterText = "Tên nhân viên";
            _ = LoadAllData();
            LoadCommand();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += async (s, e) => await Reload();
            timer.Start();
        }

        private Employee MapToEmployeeUI(NhanVien nv)
        {
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
                ChucDanh = cd != null ? cd.TenChucDanh : "Chưa có",
                CurrentAvatar = CloudinaryService.GetImageUrl(KeyData.AvatarFolder, KeyData.NhanVienTag + nv.IdnhanVien)
            };
        }

        private void FilterEmployees()
        {
            if (ListNhanVien == null) return;
            IEnumerable<NhanVien> query = ListNhanVien;

            if (!string.IsNullOrEmpty(SearchText))
            {
                string keyword = SearchText.ToLower();
                string criteria = FilterText ?? "Tên nhân viên";

                switch (criteria)
                {
                    case "Xác thực của tài khoản":
                        int? targetValue = null;
                        if (keyword == "1" || keyword == "true" || keyword.Contains("đã") || keyword.Contains("rồi")) targetValue = 1;
                        else if (keyword == "0" || keyword == "false" || keyword.Contains("chưa") || keyword.Contains("không")) targetValue = 0;

                        if (targetValue.HasValue)
                            query = query.Where(e => ListTaiKhoan != null && ListTaiKhoan.Any(tk => tk.Email == e.Email && tk.DuocXacThuc == targetValue.Value));
                        else
                            query = query.Where(e => false);
                        break;
                    case "Tình trạng":
                        query = query.Where(e => e.TinhTrang != null && e.TinhTrang.ToLower().Contains(keyword));
                        break;
                    case "Bộ phận":
                        query = query.Where(e => ListBoPhan != null && ListBoPhan.FirstOrDefault(bp => bp.Id == e.IdboPhan)?.TenBoPhan?.ToLower().Contains(keyword) == true);
                        break;
                    case "Chức vụ":
                        query = query.Where(e => ListChucDanh != null && ListChucDanh.FirstOrDefault(cd => cd.Id == e.IdchucDanh)?.TenChucDanh?.ToLower().Contains(keyword) == true);
                        break;
                    default:
                        query = query.Where(e => e.HoTen != null && e.HoTen.ToLower().Contains(keyword));
                        break;
                }
            }

            var mappedResult = query.Select(nv => MapToEmployeeUI(nv));
            FilteredEmployees = new ObservableCollection<Employee>(mappedResult);
        }

        public async Task Reload()
        {
            using (var context = new QuanLyVatTuContext())
            {
                var allNhanViens = await context.NhanViens.ToListAsync();
                if (allNhanViens == null) return;

                ListNhanVien = new ObservableCollection<NhanVien>(allNhanViens);
                FilterEmployees();

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
                ListBoPhan = new ObservableCollection<BoPhan>(await context.BoPhans.ToListAsync());
                ListChucDanh = new ObservableCollection<ChucDanh>(await context.ChucDanhs.ToListAsync());
                ListTaiKhoan = new ObservableCollection<TaiKhoan>(await context.TaiKhoans.ToListAsync());
            }
        }

        private void LoadCommand()
        {
            AddEmployeeCommand = new RelayCommand<object>(p => true, p => OpenAddNV());
            DeleteEmployeeCommand = new RelayCommand(async p =>
            {
                if (p is Employee e)
                {
                    if (MessageBox.Show("Xóa nhân viên này?", "Thông báo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (e.TinhTrang == "Đang bận") { MessageBox.Show("Nhân viên đang bận."); return; }
                        var nv = await DataProvider.Instance.DB.NhanViens.FirstOrDefaultAsync(x => x.IdnhanVien == e.Id);
                        var tk = await DataProvider.Instance.DB.TaiKhoans.FirstOrDefaultAsync(x => x.Email == e.Email);
                        if (nv != null) DataProvider.Instance.DB.Remove(nv);
                        if (tk != null) DataProvider.Instance.DB.Remove(tk);
                        await DataProvider.Instance.DB.SaveChangesAsync();
                        await Reload();
                    }
                }
            });
            EditEmployeeCommand = new RelayCommand(p => { if (p is Employee e) PopUpService.ShowPopUp(new EmployeeDetailView(e)); });
        }

        private void OpenAddNV() => PopUpService.ShowPopUp(new EmployeeFormView());

        private async Task LoadAllData()
        {
            try { await InitializeData(); await Reload(); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}