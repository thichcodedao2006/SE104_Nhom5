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
    public class SerialOption : BaseViewModel
    {
        public string SoSeri { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }

    public class MaintenancePlanFormViewModel : BaseViewModel
    {
        public ObservableCollection<DichVuBaoTri> DichVuList { get; set; }
        public ObservableCollection<ThietBi> ThietBiList { get; set; }
        public ObservableCollection<NhanVien> NhanVienList { get; set; }
        public ObservableCollection<SerialOption> SerialOptions { get; set; }

        public ObservableCollection<string> DoUuTien { get; set; }

        private SerialOption _selectedSerial;
        public SerialOption SelectedSerial
        {
            get => _selectedSerial;
            set
            {
                _selectedSerial = value;
                OnPropertyChanged(nameof(SelectedSerial));
            }
        }

        private DichVuBaoTri _selectedDichVu;
        public DichVuBaoTri SelectedDichVu
        {
            get => _selectedDichVu;
            set
            {
                _selectedDichVu = value;
                OnPropertyChanged(nameof(SelectedDichVu));

                GiaDichVu = value?.GiaDichVu?.ToString() ?? "0";
                OnPropertyChanged(nameof(GiaDichVu));
            }
        }

        private ThietBi _selectedThietBi;
        public ThietBi SelectedThietBi
        {
            get => _selectedThietBi;
            set
            {
                _selectedThietBi = value;
                OnPropertyChanged(nameof(SelectedThietBi));
                _ = LoadSerialByDevice();
            }
        }

        public NhanVien SelectedNhanVien { get; set; }
        public string SelectedPriority { get; set; }

        public string GiaDichVu { get; set; }
        public DateTime? NgayBaoTri { get; set; } = DateTime.Now;
        public string Notes { get; set; }

        public ICommand CreatePlanCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public MaintenancePlanFormViewModel()
        {
            DichVuList = new ObservableCollection<DichVuBaoTri>();
            ThietBiList = new ObservableCollection<ThietBi>();
            NhanVienList = new ObservableCollection<NhanVien>();
            SerialOptions = new ObservableCollection<SerialOption>();

            DoUuTien = new ObservableCollection<string>
            {
                "Cao",
                "Trung bình",
                "Thấp"
            };

            CreatePlanCommand = new RelayCommand<object>(
                p => true,
                async p => await CreatePlan(p));

            CancelCommand = new RelayCommand<object>(
                p => true,
                p => CloseForm(p));

            _ = LoadData();
        }

        private async Task LoadData()
        {
            using var context = new QuanLyVatTuContext();

            DichVuList = new ObservableCollection<DichVuBaoTri>(
                await context.DichVuBaoTris.ToListAsync());

            ThietBiList = new ObservableCollection<ThietBi>(
                await context.ThietBis.ToListAsync());

            NhanVienList = new ObservableCollection<NhanVien>(
                await context.NhanViens
                    .Where(x => x.TinhTrang == "Đang rảnh")
                    .ToListAsync());

            OnPropertyChanged(nameof(DichVuList));
            OnPropertyChanged(nameof(ThietBiList));
            OnPropertyChanged(nameof(NhanVienList));
        }

        private async Task LoadSerialByDevice()
        {
            SerialOptions.Clear();
            SelectedSerial = null;

            if (SelectedThietBi == null) return;

            using var context = new QuanLyVatTuContext();

            var serials = await context.ChiTietThietBis
                .Where(x => x.IdthietBi == SelectedThietBi.IdthietBi)
                .Select(x => x.SoSeri)
                .ToListAsync();

            foreach (var s in serials)
            {
                SerialOptions.Add(new SerialOption { SoSeri = s });
            }

            OnPropertyChanged(nameof(SerialOptions));
        }

        private async Task CreatePlan(object p)
        {
            try
            {
                if (SelectedDichVu == null || SelectedThietBi == null || SelectedNhanVien == null || SelectedSerial == null)
                {
                    MessageBox.Show("Vui lòng chọn đầy đủ thông tin.");
                    return;
                }

                using var context = new QuanLyVatTuContext();

                var baoTri = new BaoTri
                {
                    IdthietBi = SelectedThietBi.IdthietBi,
                    SoSeri = SelectedSerial.SoSeri,
                    IddichVu = SelectedDichVu.IddichVu,
                    IdnhanVien = SelectedNhanVien.IdnhanVien,
                    NgayBaoTri = NgayBaoTri ?? DateTime.Now,
                    DoUuTien = SelectedPriority,
                    GhiChu = Notes,
                    TinhTrangBaoTri = "Đang xử lý"
                };

                context.BaoTris.Add(baoTri);

                await context.SaveChangesAsync();

                MessageBox.Show("Tạo kế hoạch bảo trì thành công.");
                PopUpService.ClosePopUp(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo kế hoạch:\n" + ex.Message);
            }
        }
        private void CloseForm(object p)
        {
            if (p is DependencyObject d)
            {
                Window.GetWindow(d)?.Close();
            }
        }
    }
}