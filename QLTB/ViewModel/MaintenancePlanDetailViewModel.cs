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
                        TenThietBi = x.ChiTietThietBi?.IdthietBiNavigation?.TenThietBi ?? "",
                        SoSeri = x.SoSeri,
                        TinhTrang = x.ChiTietThietBi?.TinhTrang ?? "",
                        PhongBan = x.ChiTietThietBi?.IdphongBanNavigation?.TenPhong ?? "",
                        DichVu = x.IddichVuNavigation?.TenDichVu ?? "",
                        TienDo = x.TienDo ?? "",
                        KetQua = x.KetQua ?? ""
                    })
                );

                OnPropertyChanged(nameof(Devices));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi mở chi tiết:\n" + ex.Message);
            }
        }
    }
}