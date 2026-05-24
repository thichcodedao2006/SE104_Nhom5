using Microsoft.EntityFrameworkCore;
using QLTB.Data;
using QLTB.HashingData;
using QLTB.Helpers;
using QLTB.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class EmployeeFormVM : BaseViewModel
    {
        private string realname;
        private string sdt;
        private string email;
        private string chuyenMon;
        private int selectedBoPhan;
        private int selectedChucDanh;

        private List<BoPhan> listBoPhan;
        private List<ChucDanh> listChucDanh;

        public ICommand CloseCommand { get; set; }
        public ICommand SaveEmployeeCommand { get; set; }

        public string RealName { get => realname; set
            {

                realname = value;
                OnPropertyChanged(nameof(RealName));
            }
            }
        public string Sdt { get => sdt; set
            {
                sdt = value;
                OnPropertyChanged(nameof(Sdt));
            }
                }
        public string Email { get => email; set
            {
                email = value; OnPropertyChanged(nameof(Email));
            }
                }
        public string ChuyenMon { get => chuyenMon; set
            {
                chuyenMon = value;
                OnPropertyChanged(nameof(ChuyenMon));
            }
                }
        public int SelectedBoPhan { get => selectedBoPhan; set
            {
                selectedBoPhan = value;
                OnPropertyChanged(nameof(SelectedBoPhan));
            }
                }
        public int SelectedChucDanh { get => selectedChucDanh; set
            {
                selectedChucDanh = value;
                OnPropertyChanged(nameof(SelectedChucDanh));
            }
                }

        public List<BoPhan> ListBoPhan { get => listBoPhan; set
            {
                listBoPhan = value;
                OnPropertyChanged(nameof(ListBoPhan));
            }
                }
        public List<ChucDanh> ListChucDanh { get => listChucDanh; set
            {
                listChucDanh = value;
                OnPropertyChanged(nameof(ListChucDanh));
            }
                }


        public EmployeeFormVM()
        {
            _ = LoadData();

            LoadCommand();
        }

        private async Task LoadBoPhan()
        {
            using (var context = new QuanLyVatTuContext())
            {
                var list = await context.BoPhans.ToListAsync();
                ListBoPhan = list;
            }
        }

        private async Task LoadChucDanh()
        {
            using (var context = new QuanLyVatTuContext())
            {
                var list = await context.ChucDanhs.ToListAsync();
                ListChucDanh = list;
            }
        }
        
        private void LoadCommand()
        {
            CloseCommand = new RelayCommand<UserControl>
                (
                    p => true, p => PopUpService.ClosePopUp(p)
                );

            SaveEmployeeCommand = new RelayCommand<UserControl>
                (
                    p => CheckCondition(), async p => await SaveEmployee(p)
                );
        }

        private async Task SaveEmployee(UserControl p)
        {
            if (!await ValidEmail())
            {
                MessageBox.Show("Email không hợp lệ hoặc đã được sử dụng.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // 1. TẠO VÀ LƯU NHÂN VIÊN TRƯỚC
                NhanVien nv = new NhanVien()
                {
                    HoTen = RealName,
                    Sdt = Sdt,
                    Email = Email,
                    ChuyenMon = ChuyenMon,
                    IdchucDanh = SelectedChucDanh,
                    IdboPhan = SelectedBoPhan,
                    TinhTrang = "Đang rảnh"
                };
                await DataProvider.Instance.DB.NhanViens.AddAsync(nv);

                // BẮT BUỘC PHẢI SAVE Ở ĐÂY ĐỂ LẤY ID TỪ SQL SERVER
                await DataProvider.Instance.DB.SaveChangesAsync();

                // -> Ngay tại dòng này, nv.IdnhanVien đã có giá trị thực tế (VD: 7) thay vì 0

                // 2. LẤY ID VỪA TẠO ĐỂ TẠO TÀI KHOẢN
                TaiKhoan t = new TaiKhoan()
                {
                    TenTaiKhoan = KeyData.NhanVienTag + nv.IdnhanVien, // Lúc này nó sẽ ra NV7 chuẩn xác
                    MatKhau = Security.HashPasswordSHA256("123456789"),
                    Email = Email,
                    LoaiTaiKhoan = 2,
                    DuocXacThuc = 1
                };
                await DataProvider.Instance.DB.TaiKhoans.AddAsync(t);

                // SAVE LẦN 2 ĐỂ LƯU TÀI KHOẢN
                await DataProvider.Instance.DB.SaveChangesAsync();

                MessageBox.Show("Thêm nhân viên thành công. Tài khoản được tạo mặc định.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                PopUpService.ClosePopUp(p);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi trong quá trình lưu dữ liệu:\n{ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<bool> ValidEmail()
        {

            try
            {
                var address = new MailAddress(Email); // có thể null -> NullReference

                if (address.Address == Email)
                {
                    var tk = await DataProvider.Instance.DB.NhanViens.FirstOrDefaultAsync(x => x.Email == Email);
                    return tk == null; // không được tồn tại trùng Email
                }
                else
                {
                    return false;
                }
            }
            catch
            { return false; }

        }
        private bool CheckCondition()
        {
            return RealName != null && RealName.Length > 0 && Sdt != null && Sdt.Length > 0 && Email != null && Email.Length > 0
                && ChuyenMon != null && ChuyenMon.Length > 0 && SelectedBoPhan > 0 && SelectedChucDanh > 0;
        }
        private async Task LoadData()
        {
            try
            {
                await LoadBoPhan();
                await LoadChucDanh();

            }
            catch
            {

            }
        }
    }
}
