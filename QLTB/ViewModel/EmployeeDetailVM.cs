using Microsoft.EntityFrameworkCore;
using QLTB.Data;
using QLTB.Helpers;
using QLTB.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class AccountType
    {
        public int LoaiTaiKhoan { get; set; }
        public string TenLoaiTaiKhoan { get; set; }
    }

    public class EmployeeDetailVM : BaseViewModel
    {
        private string maNV;
        private string hoTen;
        private string sdt;
        private string email;
        private int selectedBoPhan;
        private int selectedChucDanh;
        private string chuyenMon;
        private int selectedLoaiTaiKhoan;
        private string avatarPath;
        private Employee data;
        private TaiKhoan account;
        private TaiKhoan userAccount;

        private bool HaveClickValidate = false;

        private List<BoPhan> listBoPhan;
        private List<ChucDanh> listChucDanh;
        private List<AccountType> listLoaiTaiKhoan;

        public ICommand CloseForm {  get; set; }
        public ICommand SaveInfoCommand { get; set; }

        public ICommand XacThucTaiKhoanCommand { get; set; }

        public string MaNV { get => maNV; set
            {
                maNV = value;
                OnPropertyChanged(nameof(MaNV));    
            }
                }
        public string HoTen { get => hoTen; set
            {
               hoTen = value;
                OnPropertyChanged(nameof(HoTen));
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
                email = value;
                OnPropertyChanged(nameof(Email));
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
        public string ChuyenMon { get => chuyenMon; set
            {
                chuyenMon = value;
                OnPropertyChanged(nameof(ChuyenMon));
            }
                }
        public int SelectedLoaiTaiKhoan { get => selectedLoaiTaiKhoan; set
            {
                selectedLoaiTaiKhoan = value;
                OnPropertyChanged(nameof (SelectedLoaiTaiKhoan));   
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
                OnPropertyChanged (nameof(ListChucDanh));
            }
                }

        public string AvatarPath { get => avatarPath; set
            {
                avatarPath = value;
                OnPropertyChanged(nameof(AvatarPath));
            }
                }

        public List<AccountType> ListLoaiTaiKhoan { get => listLoaiTaiKhoan; set
            {
                listLoaiTaiKhoan = value;
                OnPropertyChanged(nameof(ListLoaiTaiKhoan));
            }
                }

        public Employee Data { get => data; set
            {
                data = value;
                OnPropertyChanged(nameof(Data));
            }
                }

        public TaiKhoan Account { get => account; set
            {
                account = value;
                OnPropertyChanged(nameof(Account));
            }
                }

        public TaiKhoan UserAccount { get => userAccount; set
            {
                userAccount = value;
                OnPropertyChanged(nameof(UserAccount));
            }
                }

        public EmployeeDetailVM(Employee e, TaiKhoan t)
        {
            UserAccount = t;
            _ = LoadData(e);
            LoadCommand();
        }


        private void LoadCommand()
        {
            CloseForm = new RelayCommand<UserControl>
                (
                p => true, p => Close(p)
                );
            SaveInfoCommand = new RelayCommand<UserControl>
                (
                p => true,  async p => await SaveInfo(p)
                );
            XacThucTaiKhoanCommand = new RelayCommand<object>
            (
                 p => true, async p => await ValidateAccount()
            );
        }


        private async Task ValidateAccount()
        {
            try
            {
                if (HaveClickValidate) return;
                if (Account == null) return;

                HaveClickValidate = true;

                // 1. Thay đổi trạng thái ngay trên đối tượng đang hiển thị ở UI để nút đổi màu/chữ
                if (Account.DuocXacThuc == 0)
                {
                    Account.DuocXacThuc = 1;
                }
                else
                {
                    Account.DuocXacThuc = 0;
                }

                // 2. Ép UI cập nhật lại giao diện ngay lập tức
                OnPropertyChanged(nameof(Account));

                // 3. Sử dụng Context riêng để tìm và cập nhật chính xác dòng đó dưới DB
                using (var _context = new QuanLyVatTuContext())
                {
                    // Tìm thực thể sạch trực tiếp từ DB dựa vào Email hoặc Id của tài khoản
                    var taiKhoanDb = await _context.TaiKhoans.FirstOrDefaultAsync(tk => tk.Email == Account.Email);

                    if (taiKhoanDb != null)
                    {
                        // Cập nhật giá trị mới vào dòng tìm được dưới DB
                        taiKhoanDb.DuocXacThuc = Account.DuocXacThuc;

                        // Lưu thay đổi xuống SQL Server
                        await _context.SaveChangesAsync();

                        MessageBox.Show("Thay đổi xác thực tài khoản thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy tài khoản này trong cơ sở dữ liệu.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    HaveClickValidate = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}\n{ex.InnerException?.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Close(UserControl p)
        {
            PopUpService.ClosePopUp(p);
        }

        private async Task SaveInfo(UserControl p)
        {
            try
            {
                
                if (Data == null) return;
                using (var _context = new QuanLyVatTuContext())
                {
                    var nhanVienToUpdate = await _context.NhanViens.FirstOrDefaultAsync(nv => nv.IdnhanVien == Data.Id);

                    if (nhanVienToUpdate != null)
                    {
                        // Cập nhật IdBoPhan và IdChucDanh từ các biến đã Binding trên UI
                        nhanVienToUpdate.IdboPhan = SelectedBoPhan;
                        nhanVienToUpdate.IdchucDanh = SelectedChucDanh;

                    }

                    // ================= 2. CẬP NHẬT BẢNG TAIKHOAN =================
                    // Tìm tài khoản theo Email
                    var taiKhoanToUpdate = await _context.TaiKhoans.FirstOrDefaultAsync(tk => tk.Email == Data.Email);

                    if (taiKhoanToUpdate != null)
                    {
                        // Cập nhật LoaiTaiKhoan từ biến SelectedLoaiTaiKhoan của ComboBox
                        taiKhoanToUpdate.LoaiTaiKhoan = SelectedLoaiTaiKhoan;
                    }

                    // ================= 3. LƯU VÀO DATABASE =================
                    // Lưu tất cả các thay đổi (tracking bởi EF Core) xuống cơ sở dữ liệu
                    await _context.SaveChangesAsync();
                }
                MessageBox.Show("Lưu thông tin nhân viên thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information );
                PopUpService.ClosePopUp(p);
                
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo nếu có lỗi (chẳng hạn mất kết nối DB)
                System.Windows.MessageBox.Show($"Có lỗi xảy ra khi lưu thông tin: {ex.Message}", "Lỗi",
                                                System.Windows.MessageBoxButton.OK,
                                                System.Windows.MessageBoxImage.Error);
            }
        }
        private async Task LoadData(Employee e)
        {
            try
            {
                ListLoaiTaiKhoan = new List<AccountType>()
                {
                    new AccountType(){ LoaiTaiKhoan = 0, TenLoaiTaiKhoan = "Admin"  },
                    new AccountType(){ LoaiTaiKhoan = 1, TenLoaiTaiKhoan = "Quản lý"},
                    new AccountType(){LoaiTaiKhoan  = 2, TenLoaiTaiKhoan = "Nhân viên kĩ thuật"}
                };

                await LoadChucDanh();
                await LoadBoPhan();
                Data = e;

                await LoadTaiKhoanNhanVien();
            }
            catch
            {

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

        private async Task LoadBoPhan()
        {
            using (var context = new QuanLyVatTuContext())
            {
                var list = await context.BoPhans.ToListAsync();
                ListBoPhan = list;
            }
        }

        private async Task LoadTaiKhoanNhanVien()
        {
            using (var _context = new QuanLyVatTuContext())
            {
                var nhanVienInfo = await _context.NhanViens.FirstOrDefaultAsync(nv => nv.IdnhanVien == Data.Id);
                if (nhanVienInfo != null)
                {
                    // Tùy thuộc vào cách bạn Binding lên View, hãy gán vào thuộc tính tương ứng.
                    // Ví dụ bạn có 2 biến SelectedIdBoPhan và SelectedIdChucDanh:
                    SelectedBoPhan = nhanVienInfo.IdboPhan.Value;
                    SelectedChucDanh = nhanVienInfo.IdchucDanh.Value;
                    ChuyenMon = nhanVienInfo.ChuyenMon;
                }

                // B. Lấy thông tin TaiKhoan dựa vào Email của Employee (e.Email)
                var taiKhoanInfo = await _context.TaiKhoans.FirstOrDefaultAsync(tk => tk.Email == Data.Email);
                if (taiKhoanInfo != null)
                {
                    // Gán giá trị vào biến SelectedLoaiTaiKhoan (đã Binding ở XAML trong câu hỏi trước)
                    SelectedLoaiTaiKhoan = taiKhoanInfo.LoaiTaiKhoan.Value;
                    Account = taiKhoanInfo;
                }
            }
        }
    }
}
