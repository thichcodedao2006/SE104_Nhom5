using Microsoft.EntityFrameworkCore;
using QLTB.Helpers;
using QLTB.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    // Đây là display model dùng để hiển thị trên DataGrid
    public class MaterialDisplayItem
    {
        public int IdThietBi { get; set; }
        public string TenThietBi { get; set; }
        public string LoaiThietBi { get; set; }
        public string DonViSanXuat { get; set; }
        public int? SoLuong { get; set; }
        public double? Gia { get; set; }
        public string GiaFormatted => Gia.HasValue ? Gia.Value.ToString("N0") + " đ" : "0 đ";
        public int? BaoHanhDinhKy { get; set; }
        public string DonViThoiGian { get; set; }
        public string BaoHanhText => BaoHanhDinhKy.HasValue
            ? $"{BaoHanhDinhKy} {DonViThoiGian}"
            : "Không có";
        public DateTime? NgayNhap { get; set; }
        public string NgayNhapText => NgayNhap?.ToString("dd/MM/yyyy") ?? "N/A";
        public int SoSerialDangDung { get; set; }
        public string TrangThai => SoLuong.HasValue && SoLuong > 0 ? "Còn hàng" : "Hết hàng";
    }

    public class MaterialViewModel : BaseViewModel
    {
        #region Collections & Display

        private ObservableCollection<MaterialDisplayItem> _allMaterials;
        private ObservableCollection<MaterialDisplayItem> _materials;
        public ObservableCollection<MaterialDisplayItem> Materials
        {
            get => _materials;
            set { _materials = value; OnPropertyChanged(nameof(Materials)); }
        }

        private MaterialDisplayItem _selectedMaterial;
        public MaterialDisplayItem SelectedMaterial
        {
            get => _selectedMaterial;
            set { _selectedMaterial = value; OnPropertyChanged(nameof(SelectedMaterial)); }
        }

        #endregion

        #region Search & Filter

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ApplyFilter();
            }
        }

        private string _selectedLoai = "Tất cả";
        public string SelectedLoai
        {
            get => _selectedLoai;
            set
            {
                _selectedLoai = value;
                OnPropertyChanged(nameof(SelectedLoai));
                ApplyFilter();
            }
        }

        private ObservableCollection<string> _loaiList;
        public ObservableCollection<string> LoaiList
        {
            get => _loaiList;
            set { _loaiList = value; OnPropertyChanged(nameof(LoaiList)); }
        }

        #endregion

        #region Statistics

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set { _totalCount = value; OnPropertyChanged(nameof(TotalCount)); }
        }

        private int _activeCount;
        public int ActiveCount
        {
            get => _activeCount;
            set { _activeCount = value; OnPropertyChanged(nameof(ActiveCount)); }
        }

        private double _totalValue;
        public double TotalValue
        {
            get => _totalValue;
            set { _totalValue = value; OnPropertyChanged(nameof(TotalValue)); }
        }

        public string TotalValueFormatted => TotalValue.ToString("N0") + " đ";

        #endregion

        #region Form properties (Add/Edit)

        private bool _isFormOpen;
        public bool IsFormOpen
        {
            get => _isFormOpen;
            set { _isFormOpen = value; OnPropertyChanged(nameof(IsFormOpen)); }
        }

        private bool _isEditMode;
        public bool IsEditMode
        {
            get => _isEditMode;
            set { _isEditMode = value; OnPropertyChanged(nameof(IsEditMode)); }
        }

        private string _formTitle;
        public string FormTitle
        {
            get => _formTitle;
            set { _formTitle = value; OnPropertyChanged(nameof(FormTitle)); }
        }

        private int _editingId;

        private string _tenThietBi;
        public string TenThietBi
        {
            get => _tenThietBi;
            set { _tenThietBi = value; OnPropertyChanged(nameof(TenThietBi)); }
        }

        private string _loaiThietBi;
        public string LoaiThietBi
        {
            get => _loaiThietBi;
            set { _loaiThietBi = value; OnPropertyChanged(nameof(LoaiThietBi)); }
        }

        private string _donViSanXuat;
        public string DonViSanXuat
        {
            get => _donViSanXuat;
            set { _donViSanXuat = value; OnPropertyChanged(nameof(DonViSanXuat)); }
        }

        private string _soLuong;
        public string SoLuong
        {
            get => _soLuong;
            set { _soLuong = value; OnPropertyChanged(nameof(SoLuong)); }
        }

        private string _gia;
        public string Gia
        {
            get => _gia;
            set { _gia = value; OnPropertyChanged(nameof(Gia)); }
        }

        private string _baoHanhDinhKy;
        public string BaoHanhDinhKy
        {
            get => _baoHanhDinhKy;
            set { _baoHanhDinhKy = value; OnPropertyChanged(nameof(BaoHanhDinhKy)); }
        }

        private string _selectedDonViThoiGian = "ngày";
        public string SelectedDonViThoiGian
        {
            get => _selectedDonViThoiGian;
            set { _selectedDonViThoiGian = value; OnPropertyChanged(nameof(SelectedDonViThoiGian)); }
        }

        public ObservableCollection<string> DonViThoiGianList { get; set; } = new ObservableCollection<string>
        {
            "phút", "giờ", "ngày", "tháng", "năm"
        };

        #endregion

        #region Commands

        public ICommand OpenAddFormCommand { get; set; }
        public ICommand OpenEditFormCommand { get; set; }
        public ICommand CloseFormCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        #endregion

        public MaterialViewModel()
        {
            _allMaterials = new ObservableCollection<MaterialDisplayItem>();
            Materials = new ObservableCollection<MaterialDisplayItem>();
            LoaiList = new ObservableCollection<string> { "Tất cả" };

            LoadCommands();
            _ = LoadDataAsync();
        }

        private void LoadCommands()
        {
            OpenAddFormCommand = new RelayCommand(o =>
            {
                IsEditMode = false;
                FormTitle = "Thêm thiết bị / vật tư mới";
                ClearForm();
                IsFormOpen = true;
            });

            OpenEditFormCommand = new RelayCommand(o =>
            {
                if (SelectedMaterial == null)
                {
                    MessageBox.Show("Vui lòng chọn một dòng để chỉnh sửa.", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                IsEditMode = true;
                FormTitle = "Chỉnh sửa thiết bị / vật tư";
                FillFormFromSelected();
                IsFormOpen = true;
            });

            CloseFormCommand = new RelayCommand(o =>
            {
                IsFormOpen = false;
                ClearForm();
            });

            SaveCommand = new RelayCommand(async o =>
            {
                if (!ValidateForm()) return;
                if (IsEditMode)
                    await UpdateThietBiAsync();
                else
                    await AddThietBiAsync();
            });

            DeleteCommand = new RelayCommand(async o =>
            {
                if (SelectedMaterial == null)
                {
                    MessageBox.Show("Vui lòng chọn một dòng để xóa.", "Thông báo",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa thiết bị \"{SelectedMaterial.TenThietBi}\"?\n" +
                    "Toàn bộ serial và lịch sử liên quan cũng sẽ bị xóa.",
                    "Xác nhận xóa",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                    await DeleteThietBiAsync();
            });

            RefreshCommand = new RelayCommand(async o => await LoadDataAsync());
        }

        private async Task LoadDataAsync()
        {
            try
            {
                using var context = new QuanLyVatTuContext();

                var data = await context.ThietBis
                    .Include(tb => tb.ChiTietThietBis)
                    .ToListAsync();

                _allMaterials = new ObservableCollection<MaterialDisplayItem>(
                    data.Select(tb => new MaterialDisplayItem
                    {
                        IdThietBi = tb.IdthietBi,
                        TenThietBi = tb.TenThietBi ?? "N/A",
                        LoaiThietBi = tb.LoaiThietBi ?? "N/A",
                        DonViSanXuat = tb.DonViSanXuat ?? "N/A",
                        SoLuong = tb.SoLuong,
                        Gia = tb.Gia,
                        BaoHanhDinhKy = tb.BaoHanhDinhKy,
                        DonViThoiGian = ConvertUnit(tb.DonViThoiGian),
                        NgayNhap = tb.NgayNhapThietBi,
                        SoSerialDangDung = tb.ChiTietThietBis.Count
                    })
                );

                // Cập nhật danh sách loại để filter
                var loaiValues = data
                    .Where(tb => !string.IsNullOrEmpty(tb.LoaiThietBi))
                    .Select(tb => tb.LoaiThietBi)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                LoaiList = new ObservableCollection<string>(loaiValues.Prepend("Tất cả"));
                OnPropertyChanged(nameof(LoaiList));

                ApplyFilter();
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi hệ thống",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            if (_allMaterials == null) return;

            var query = _allMaterials.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(m =>
                    (m.TenThietBi != null && m.TenThietBi.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (m.LoaiThietBi != null && m.LoaiThietBi.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (m.DonViSanXuat != null && m.DonViSanXuat.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                );
            }

            if (!string.IsNullOrEmpty(SelectedLoai) && SelectedLoai != "Tất cả")
            {
                query = query.Where(m => m.LoaiThietBi == SelectedLoai);
            }

            Materials = new ObservableCollection<MaterialDisplayItem>(query);
            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            TotalCount = _allMaterials?.Count ?? 0;
            ActiveCount = _allMaterials?.Count(m => m.SoLuong > 0) ?? 0;
            TotalValue = _allMaterials?.Sum(m => (m.Gia ?? 0) * (m.SoLuong ?? 0)) ?? 0;
            OnPropertyChanged(nameof(TotalValueFormatted));
        }

        private async Task AddThietBiAsync()
        {
            try
            {
                using var context = new QuanLyVatTuContext();

                var newItem = new ThietBi
                {
                    TenThietBi = TenThietBi.Trim(),
                    LoaiThietBi = LoaiThietBi?.Trim(),
                    DonViSanXuat = DonViSanXuat?.Trim(),
                    SoLuong = int.TryParse(SoLuong, out int sl) ? sl : 0,
                    Gia = double.TryParse(Gia, out double g) ? g : 0,
                    BaoHanhDinhKy = int.TryParse(BaoHanhDinhKy, out int bh) ? bh : null,
                    DonViThoiGian = ConvertUnitToInt(SelectedDonViThoiGian),
                    NgayNhapThietBi = DateTime.Now
                };

                context.ThietBis.Add(newItem);
                await context.SaveChangesAsync();

                MessageBox.Show("Thêm thiết bị thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                IsFormOpen = false;
                ClearForm();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateThietBiAsync()
        {
            try
            {
                using var context = new QuanLyVatTuContext();

                var item = await context.ThietBis.FirstOrDefaultAsync(tb => tb.IdthietBi == _editingId);
                if (item == null)
                {
                    MessageBox.Show("Không tìm thấy thiết bị để cập nhật.", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                item.TenThietBi = TenThietBi.Trim();
                item.LoaiThietBi = LoaiThietBi?.Trim();
                item.DonViSanXuat = DonViSanXuat?.Trim();
                item.SoLuong = int.TryParse(SoLuong, out int sl) ? sl : 0;
                item.Gia = double.TryParse(Gia, out double g) ? g : 0;
                item.BaoHanhDinhKy = int.TryParse(BaoHanhDinhKy, out int bh) ? bh : null;
                item.DonViThoiGian = ConvertUnitToInt(SelectedDonViThoiGian);

                context.Entry(item).State = EntityState.Modified;
                await context.SaveChangesAsync();

                MessageBox.Show("Cập nhật thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                IsFormOpen = false;
                ClearForm();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteThietBiAsync()
        {
            try
            {
                using var context = new QuanLyVatTuContext();

                // Xóa ChiTietBaoTri liên quan trước
                var chiTietBaoTris = await context.ChiTietBaoTris
                    .Where(ct => ct.IdthietBi == SelectedMaterial.IdThietBi)
                    .ToListAsync();
                context.ChiTietBaoTris.RemoveRange(chiTietBaoTris);

                // Xóa BaoCaoSuaChua liên quan
                var baoCaos = await context.BaoCaoSuaChuas
                    .Where(bc => bc.IdthietBi == SelectedMaterial.IdThietBi)
                    .ToListAsync();
                context.BaoCaoSuaChuas.RemoveRange(baoCaos);

                // Xóa ChiTietThietBi
                var chiTiets = await context.ChiTietThietBis
                    .Where(ct => ct.IdthietBi == SelectedMaterial.IdThietBi)
                    .ToListAsync();
                context.ChiTietThietBis.RemoveRange(chiTiets);

                // Xóa ThietBi chính
                var thietBi = await context.ThietBis
                    .FirstOrDefaultAsync(tb => tb.IdthietBi == SelectedMaterial.IdThietBi);
                if (thietBi != null)
                    context.ThietBis.Remove(thietBi);

                await context.SaveChangesAsync();

                MessageBox.Show("Xóa thiết bị thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa: {ex.Message}\n" +
                    "Có thể thiết bị này đang được tham chiếu ở nơi khác.",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(TenThietBi))
            {
                MessageBox.Show("Tên thiết bị không được để trống.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(SoLuong) && !int.TryParse(SoLuong, out _))
            {
                MessageBox.Show("Số lượng phải là số nguyên.", "Dữ liệu không hợp lệ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Gia) && !double.TryParse(Gia, out _))
            {
                MessageBox.Show("Giá phải là số.", "Dữ liệu không hợp lệ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearForm()
        {
            _editingId = 0;
            TenThietBi = string.Empty;
            LoaiThietBi = string.Empty;
            DonViSanXuat = string.Empty;
            SoLuong = string.Empty;
            Gia = string.Empty;
            BaoHanhDinhKy = string.Empty;
            SelectedDonViThoiGian = "ngày";
        }

        private void FillFormFromSelected()
        {
            if (SelectedMaterial == null) return;
            _editingId = SelectedMaterial.IdThietBi;
            TenThietBi = SelectedMaterial.TenThietBi;
            LoaiThietBi = SelectedMaterial.LoaiThietBi;
            DonViSanXuat = SelectedMaterial.DonViSanXuat;
            SoLuong = SelectedMaterial.SoLuong?.ToString() ?? string.Empty;
            Gia = SelectedMaterial.Gia?.ToString() ?? string.Empty;
            BaoHanhDinhKy = SelectedMaterial.BaoHanhDinhKy?.ToString() ?? string.Empty;
            SelectedDonViThoiGian = SelectedMaterial.DonViThoiGian ?? "ngày";
        }

        private string ConvertUnit(int? unit) => unit switch
        {
            0 => "phút",
            1 => "giờ",
            2 => "ngày",
            3 => "tháng",
            4 => "năm",
            _ => "ngày"
        };

        private int ConvertUnitToInt(string unit) => unit switch
        {
            "phút" => 0,
            "giờ" => 1,
            "ngày" => 2,
            "tháng" => 3,
            "năm" => 4,
            _ => 2
        };
    }
}