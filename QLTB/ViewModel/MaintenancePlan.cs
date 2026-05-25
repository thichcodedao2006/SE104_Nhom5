using Microsoft.EntityFrameworkCore;
using QLTB.Helpers;
using QLTB.Models;
using QLTB.UserControlFolder.Maintenance;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class MaintenancePlanItem
    {
        public int IdBaoTri { get; set; }
        public string Title { get; set; }
        public string Equipment { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string NextDue { get; set; }
        public string Schedule { get; set; }
        public string AssignedTo { get; set; }
        public decimal EstimatedCost { get; set; }
    }

    public class MaintenancePlanViewModel : BaseViewModel
    {
        public ObservableCollection<MaintenancePlanItem> Plans { get; set; }

        public int TotalPlans { get; set; }
        public int ActivePlans { get; set; }
        public int DueThisMonth { get; set; }
        public decimal EstimatedMonthlyCost { get; set; }

        public ICommand CreatePlanCommand { get; set; }
        public ICommand ViewDetailsCommand { get; set; }

        public MaintenancePlanViewModel()
        {
            Plans = new ObservableCollection<MaintenancePlanItem>();

            _ = LoadData();

            CreatePlanCommand = new RelayCommand<object>
            (
                p => true,
                p => OpenCreatePlan()
            );

            ViewDetailsCommand = new RelayCommand(o =>
            {
                try
                {
                    if (o is MaintenancePlanItem item)
                    {
                        PopUpService.ShowPopUp(new MaintenancePlanDetailView(item));
                    }
                    else
                    {
                        MessageBox.Show("CommandParameter không phải MaintenancePlanItem");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi mở popup chi tiết:\n" + ex.Message);
                }
            });
        }

        private async Task LoadData()
        {
            using var context = new QuanLyVatTuContext();

            var data = await context.BaoTris
                .Include(x => x.ChiTietThietBi)
                    .ThenInclude(x => x.IdthietBiNavigation)
                .Include(x => x.IddichVuNavigation)
                .Include(x => x.IdnhanVienNavigation)
                .ToListAsync();

            Plans = new ObservableCollection<MaintenancePlanItem>(
                data.Select(x => new MaintenancePlanItem
                {
                    IdBaoTri = x.IdbaoTri,

                    Title = x.IddichVuNavigation?.TenDichVu ?? "Kế hoạch bảo trì",

                    Equipment = x.ChiTietThietBi?.IdthietBiNavigation?.TenThietBi ?? "Không rõ",

                    Priority = x.DoUuTien ?? "Trung bình",

                    Status = x.TinhTrangBaoTri ?? "Đang xử lý",

                    Type = "Bảo trì",

                    NextDue = x.NgayBaoTri?.ToString("yyyy-MM-dd") ?? "",

                    Schedule = x.IddichVuNavigation != null
                        ? $"{x.IddichVuNavigation.Value} {ConvertUnit(x.IddichVuNavigation.Unit)}"
                        : "Không rõ",

                    AssignedTo = x.IdnhanVienNavigation?.HoTen ?? "Chưa phân công",

                    EstimatedCost = Convert.ToDecimal(x.IddichVuNavigation?.GiaDichVu ?? 0)
                })
            );

            UpdateStatistics();

            OnPropertyChanged(nameof(Plans));
        }

        private void UpdateStatistics()
        {
            TotalPlans = Plans.Count;

            ActivePlans = Plans.Count(p =>
                p.Status == "Đang xử lý" ||
                p.Status == "Hoạt động");

            DueThisMonth = Plans.Count(p =>
                DateTime.TryParse(p.NextDue, out DateTime date)
                && date.Month == DateTime.Now.Month
                && date.Year == DateTime.Now.Year);

            EstimatedMonthlyCost = Plans
                .Where(p => DateTime.TryParse(p.NextDue, out DateTime date)
                    && date.Month == DateTime.Now.Month
                    && date.Year == DateTime.Now.Year)
                .Sum(p => p.EstimatedCost);

            OnPropertyChanged(nameof(TotalPlans));
            OnPropertyChanged(nameof(ActivePlans));
            OnPropertyChanged(nameof(DueThisMonth));
            OnPropertyChanged(nameof(EstimatedMonthlyCost));
        }
        private void OpenCreatePlan()
        {
            PopUpService.ShowPopUp(new MaintenancePlanFormView());
        }
        private string ConvertUnit(int? unit)
        {
            return unit switch
            {
                0 => "phút",
                1 => "giờ",
                2 => "ngày",
                3 => "tháng",
                4 => "năm",
                _ => ""
            };
        }
    }
}