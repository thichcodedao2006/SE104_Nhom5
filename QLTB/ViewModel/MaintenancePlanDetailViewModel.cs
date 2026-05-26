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
    public class MaintenanceDeviceDetail
    {
        public string TenThietBi { get; set; }
        public string SoSeri { get; set; }
        public string TinhTrang { get; set; }
        public string PhongBan { get; set; }
        public string DichVu { get; set; }
        public string TienDo { get; set; }
        public string KetQua { get; set; }
    }

    public class MaintenancePlanDetailViewModel : BaseViewModel
    {
        public MaintenancePlanItem Plan { get; set; }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private string _type;
        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        private string _priority;
        public string Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPropertyChanged(nameof(Priority));
            }
        }

        private string _assignedTo;
        public string AssignedTo
        {
            get => _assignedTo;
            set
            {
                _assignedTo = value;
                OnPropertyChanged(nameof(AssignedTo));
            }
        }

        private string _nextDue;
        public string NextDue
        {
            get => _nextDue;
            set
            {
                _nextDue = value;
                OnPropertyChanged(nameof(NextDue));
            }
        }

        private decimal _estimatedCost;
        public decimal EstimatedCost
        {
            get => _estimatedCost;
            set
            {
                _estimatedCost = value;
                OnPropertyChanged(nameof(EstimatedCost));
            }
        }

        private ObservableCollection<MaintenanceDeviceDetail> _devices;
        public ObservableCollection<MaintenanceDeviceDetail> Devices
        {
            get => _devices;
            set
            {
                _devices = value;
                OnPropertyChanged(nameof(Devices));
            }
        }

        public ICommand CloseCommand { get; set; }

        public MaintenancePlanDetailViewModel(MaintenancePlanItem item)
        {
            Plan = item;

            // Khởi tạo properties từ Plan
            Title = Plan?.Title ?? "N/A";
            Type = Plan?.Type ?? "N/A";
            Priority = Plan?.Priority ?? "N/A";
            AssignedTo = Plan?.AssignedTo ?? "N/A";
            NextDue = Plan?.NextDue ?? "N/A";
            EstimatedCost = Plan?.EstimatedCost ?? 0;

            Devices = new ObservableCollection<MaintenanceDeviceDetail>();

            CloseCommand = new RelayCommand<object>(
            p => true,
            p =>
            {
                if (p is DependencyObject d)
                {
                    Window.GetWindow(d)?.Close();
                }
            });

            _ = LoadDevices();
        }

        private async Task LoadDevices()
        {
            try
            {
                // Kiểm tra Plan có null không
                if (Plan == null)
                {
                    MessageBox.Show("Lỗi: Không có thông tin kế hoạch bảo trì.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using var context = new QuanLyVatTuContext();

                var details = await context.ChiTietBaoTris
                    .Include(x => x.ChiTietThietBi)
                        .ThenInclude(x => x.IdthietBiNavigation)
                    .Include(x => x.ChiTietThietBi)
                        .ThenInclude(x => x.IdphongBanNavigation)
                    .Include(x => x.IddichVuNavigation)
                    .Where(x => x.IdbaoTri == Plan.IdBaoTri)
                    .ToListAsync();

                Devices = new ObservableCollection<MaintenanceDeviceDetail>(
                    details.Select(x => new MaintenanceDeviceDetail
                    {
                        TenThietBi = x.ChiTietThietBi?.IdthietBiNavigation?.TenThietBi ?? "N/A",
                        SoSeri = x.SoSeri ?? "N/A",
                        TinhTrang = x.ChiTietThietBi?.TinhTrang ?? "N/A",
                        PhongBan = x.ChiTietThietBi?.IdphongBanNavigation?.TenPhong ?? "N/A",
                        DichVu = x.IddichVuNavigation?.TenDichVu ?? "N/A",
                        TienDo = x.TienDo ?? "Chưa bắt đầu",
                        KetQua = x.KetQua ?? "Chưa có"
                    })
                );

                OnPropertyChanged(nameof(Devices));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu chi tiết:\n{ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}