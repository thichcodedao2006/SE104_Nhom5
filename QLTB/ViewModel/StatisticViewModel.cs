using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;
using QLTB.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static QLTB.ViewModel.StatisticViewModel;

namespace QLTB.ViewModel
{
    public class StatisticViewModel : INotifyPropertyChanged
    {
        public int TotalDevices { get; set; }
        public int ActiveDevices { get; set; }
        public int MaintenanceThisMonthCount { get; set; }
        public int UnresolvedIncidents { get; set; }

        public ISeries[] MaintenanceSeries { get; set; }
        public Axis[] MaintenanceXAxes { get; set; }

        public ISeries[] CostSeries { get; set; }
        public Axis[] CostXAxes { get; set; }
        public Axis[] CostYAxes { get; set; }
        
        public class TopIncidentDevice
        {
            public string TenThietBi { get; set; }
            public string SoSeri { get; set; }
            public string LoaiThietBi { get; set; }
            public int SoSuCo { get; set; }
            public string LanGanNhat { get; set; }
            public string TrangThai { get; set; }
        }


        public ISeries[] DeviceStatusSeries { get; set; }

        public ICommand ExportReportCommand { get; set; }

        public ObservableCollection<TopIncidentDevice> TopIncidentDevices { get; set; }

        public StatisticViewModel()
        {

            ExportReportCommand = new RelayCommand<object>(
                    p => true,
                    p => ExportReport());

            _ = LoadData();
        }

        private async Task LoadData()
        {
            using var context = new QuanLyVatTuContext();

            TotalDevices = await context.ChiTietThietBis.CountAsync();

            ActiveDevices = await context.ChiTietThietBis
                .CountAsync(x => x.TinhTrang == "Tốt");

            MaintenanceThisMonthCount = await context.BaoTris
                .CountAsync(x => x.NgayBaoTri.HasValue && x.NgayBaoTri.Value.Month == DateTime.Now.Month && x.NgayBaoTri.Value.Year == DateTime.Now.Year);

            UnresolvedIncidents = await context.BaoCaoSuaChuas
                .CountAsync(x => x.TrangThai != "Đã giải quyết");

            LoadMaintenanceChart(context);
            LoadCostChart(context);
            LoadDeviceStatusChart(context);
            LoadTopIncidentDevices(context);

            OnPropertyChanged(nameof(TopIncidentDevices));
            OnPropertyChanged(nameof(TotalDevices));
            OnPropertyChanged(nameof(ActiveDevices));
            OnPropertyChanged(nameof(MaintenanceThisMonthCount));
            OnPropertyChanged(nameof(UnresolvedIncidents));
            OnPropertyChanged(nameof(MaintenanceSeries));
            OnPropertyChanged(nameof(MaintenanceXAxes));
            OnPropertyChanged(nameof(CostSeries));
            OnPropertyChanged(nameof(CostXAxes));
            OnPropertyChanged(nameof(CostYAxes));
            OnPropertyChanged(nameof(DeviceStatusSeries));
        }

        private void LoadMaintenanceChart(QuanLyVatTuContext context)
        {
            int[] values = new int[12];

            var data = context.BaoTris
                .Where(x => x.NgayBaoTri != null)
                .AsEnumerable()
                .GroupBy(x => x.NgayBaoTri.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count()
                });

            foreach (var item in data)
                values[item.Month - 1] = item.Count;

            MaintenanceSeries = new ISeries[]
            {
                new ColumnSeries<int>
                {
                    Name = "Số công việc",
                    Values = values
                }
            };

            MaintenanceXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = new[]
                    {
                        "T1","T2","T3","T4","T5","T6",
                        "T7","T8","T9","T10","T11","T12"
                    }
                }
            };
        }

        private void LoadCostChart(QuanLyVatTuContext context)
        {
            double[] values = new double[12];

            var data = context.BaoTris
                .Include(x => x.IddichVuNavigation)
                .Where(x => x.NgayBaoTri != null)
                .AsEnumerable()
                .GroupBy(x => x.NgayBaoTri.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Cost = g.Sum(x => x.IddichVuNavigation?.GiaDichVu ?? 0)
                });

            foreach (var item in data)
                values[item.Month - 1] = item.Cost;

            CostSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Chi phí",
                    Values = values,
                    Fill = null,
                    GeometrySize = 10
                }
            };

            CostXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = new[]
                    {
                        "T1","T2","T3","T4","T5","T6",
                        "T7","T8","T9","T10","T11","T12"
                    }
                }
            };

            CostYAxes = new Axis[]
            {
                new Axis
                {
                    MinLimit = 0,
                    Labeler = value => value.ToString("N0") + "đ"
                }
            };
        }

        private void LoadDeviceStatusChart(QuanLyVatTuContext context)
        {
            int good = context.ChiTietThietBis.Count(x => x.TinhTrang == "Tốt");
            int error = context.ChiTietThietBis.Count(x => x.TinhTrang == "Lỗi");
            int maintenance = context.ChiTietThietBis.Count(x => x.TinhTrang == "Đang bảo trì");

            DeviceStatusSeries = new ISeries[]
            {
                new PieSeries<int> { Name = "Tốt", Values = new[] { good } },
                new PieSeries<int> { Name = "Lỗi", Values = new[] { error } },
                new PieSeries<int> { Name = "Đang bảo trì", Values = new[] { maintenance } }
            };
        }

        private void LoadTopIncidentDevices(QuanLyVatTuContext context)
        {
            var data = context.BaoCaoSuaChuas
                .Include(x => x.ChiTietThietBi)
                .ThenInclude(x => x.IdthietBiNavigation)
                .AsEnumerable()
                .GroupBy(x => new
                {
                    x.IdthietBi,
                    x.SoSeri
                })
                .Select(g =>
                {
                    var latest = g.OrderByDescending(x => x.NgayBaoCao).FirstOrDefault();
                    var thietBi = latest?.ChiTietThietBi?.IdthietBiNavigation;

                    return new TopIncidentDevice
                    {
                        TenThietBi = thietBi?.TenThietBi ?? "Không rõ",
                        SoSeri = latest?.SoSeri ?? "",
                        LoaiThietBi = thietBi?.LoaiThietBi ?? "Không rõ",
                        SoSuCo = g.Count(),
                        LanGanNhat = latest?.NgayBaoCao?.ToString("dd/MM/yyyy") ?? "",
                        TrangThai = latest?.TrangThai ?? ""
                    };
                })
                .OrderByDescending(x => x.SoSuCo)
                .Take(5)
                .ToList();

            TopIncidentDevices = new ObservableCollection<TopIncidentDevice>(data);
        }
        private void ExportReport()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "BaoCaoThongKe",
                DefaultExt = ".csv",
                Filter = "CSV file (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                var sb = new StringBuilder();

                sb.AppendLine("BÁO CÁO THỐNG KÊ");
                sb.AppendLine($"Ngày xuất,{DateTime.Now:dd/MM/yyyy HH:mm}");
                sb.AppendLine();

                sb.AppendLine("Tổng quan");
                sb.AppendLine($"Tổng thiết bị,{TotalDevices}");
                sb.AppendLine($"Đang hoạt động,{ActiveDevices}");
                sb.AppendLine($"Bảo trì trong tháng,{MaintenanceThisMonthCount}");
                sb.AppendLine($"Sự cố chưa xử lý,{UnresolvedIncidents}");
                sb.AppendLine();

                sb.AppendLine("Công việc bảo trì theo tháng");
                sb.AppendLine("Tháng,Số công việc");

                if (MaintenanceSeries != null)
                {
                    var values = MaintenanceSeries[0].Values.Cast<int>().ToList();

                    for (int i = 0; i < values.Count; i++)
                    {
                        sb.AppendLine($"Tháng {i + 1},{values[i]}");
                    }
                }

                sb.AppendLine();
                sb.AppendLine("Chi phí bảo trì theo tháng");
                sb.AppendLine("Tháng,Chi phí");

                if (CostSeries != null)
                {
                    var values = CostSeries[0].Values.Cast<double>().ToList();

                    for (int i = 0; i < values.Count; i++)
                    {
                        sb.AppendLine($"Tháng {i + 1},{values[i]}");
                    }
                }

                File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);

                MessageBox.Show("Xuất báo cáo thành công!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}