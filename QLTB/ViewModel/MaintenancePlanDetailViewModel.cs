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
    }

    public class MaintenancePlanDetailViewModel : BaseViewModel
    {
        public MaintenancePlanItem Plan { get; set; }
        public int IdBaoTri { get; set; }

        public string Title => Plan.Title;
        public string Type => Plan.Type;
        public string Priority => Plan.Priority;
        public string AssignedTo => Plan.AssignedTo;
        public string NextDue => Plan.NextDue;
        public decimal EstimatedCost => Plan.EstimatedCost;

        public ObservableCollection<MaintenanceDeviceDetail> Devices { get; set; }

        public ICommand CloseCommand { get; set; }

        public MaintenancePlanDetailViewModel(MaintenancePlanItem item)
        {
            Plan = item;

            Devices = new ObservableCollection<MaintenanceDeviceDetail>();

            CloseCommand = new RelayCommand<object>(
                p => true,
                p => PopUpService.ClosePopUp(this));

            _ = LoadDevices();
        }

        private async Task LoadDevices()
        {
            try
            {
                using var context = new QuanLyVatTuContext();

                var item = await context.BaoTris
                    .Include(x => x.ChiTietThietBi)
                        .ThenInclude(x => x.IdthietBiNavigation)
                    .Include(x => x.ChiTietThietBi)
                        .ThenInclude(x => x.IdphongBanNavigation)
                    .FirstOrDefaultAsync(x => x.IdbaoTri == Plan.IdBaoTri);

                if (item == null)
                {
                    MessageBox.Show("Không tìm thấy kế hoạch bảo trì.");
                    return;
                }

                Devices = new ObservableCollection<MaintenanceDeviceDetail>
        {
            new MaintenanceDeviceDetail
            {
                TenThietBi = item.ChiTietThietBi?.IdthietBiNavigation?.TenThietBi ?? "",
                SoSeri = item.SoSeri,
                TinhTrang = item.ChiTietThietBi?.TinhTrang ?? "",
                PhongBan = item.ChiTietThietBi?.IdphongBanNavigation?.TenPhong ?? ""
            }
        };

                OnPropertyChanged(nameof(Devices));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi mở chi tiết:\n" + ex.Message);
            }
        }
    }
}