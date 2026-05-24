using Microsoft.EntityFrameworkCore;
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


        private List<BoPhan> listBoPhan;
        private List<ChucDanh> listChucDanh;
        private List<AccountType> listLoaiTaiKhoan;

        public ICommand CloseForm {  get; set; }
        public ICommand SaveInfoCommand { get; set; }

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

        public EmployeeDetailVM(Employee e)
        {
            _ = LoadData(e);
            LoadCommand();
        }


        private void LoadCommand()
        {
            CloseForm = new RelayCommand<UserControl>
                (
                p => true, p => PopUpService.ClosePopUp(p)
                );
            SaveInfoCommand = new RelayCommand<UserControl>
                (
                p => true,  async p => await SaveInfo(p)
                );
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
                }
            }
        }
    }
}
