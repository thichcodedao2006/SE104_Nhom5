using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.EntityFrameworkCore;
using QLTB.Helpers;
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

namespace QLTB.ViewModel
{
    public class StatisticViewModel : INotifyPropertyChanged
    {
        #region Summary Cards

        private int _totalDevices;
        public int TotalDevices
        {
            get => _totalDevices;
            set { _totalDevices = value; OnPropertyChanged(nameof(TotalDevices)); }
        }

        private int _activeDevices;
        public int ActiveDevices
        {
            get => _activeDevices;
            set { _activeDevices = value; OnPropertyChanged(nameof(ActiveDevices)); }
        }

        private int _maintenanceThisMonthCount;
        public int MaintenanceThisMonthCount
        {
            get => _maintenanceThisMonthCount;
            set { _maintenanceThisMonthCount = value; OnPropertyChanged(nameof(MaintenanceThisMonthCount)); }
        }

        private int _unresolvedIncidents;
        public int UnresolvedIncidents
        {
            get => _unresolvedIncidents;
            set { _unresolvedIncidents = value; OnPropertyChanged(nameof(UnresolvedIncidents)); }
        }

        // FIX: thêm tổng chi phí năm
        private double _totalCostThisYear;
        public double TotalCostThisYear
        {
            get => _totalCostThisYear;
            set { _totalCostThisYear = value; OnPropertyChanged(nameof(TotalCostThisYear)); }
        }
        public string TotalCostFormatted => TotalCostThisYear.ToString("N0") + " đ";

        #endregion

        #region Charts

        private ISeries[] _maintenanceSeries;
        public ISeries[] MaintenanceSeries
        {
            get => _maintenanceSeries;
            set { _maintenanceSeries = value; OnPropertyChanged(nameof(MaintenanceSeries)); }
        }

        private Axis[] _maintenanceXAxes;
        public Axis[] MaintenanceXAxes
        {
            get => _maintenanceXAxes;
            set { _maintenanceXAxes = value; OnPropertyChanged(nameof(MaintenanceXAxes)); }
        }

        private ISeries[] _costSeries;
        public ISeries[] CostSeries
        {
            get => _costSeries;
            set { _costSeries = value; OnPropertyChanged(nameof(CostSeries)); }
        }

        private Axis[] _costXAxes;
        public Axis[] CostXAxes
        {
            get => _costXAxes;
            set { _costXAxes = value; OnPropertyChanged(nameof(CostXAxes)); }
        }

        private Axis[] _costYAxes;
        public Axis[] CostYAxes
        {
            get => _costYAxes;
            set { _costYAxes = value; OnPropertyChanged(nameof(CostYAxes)); }
        }

        private ISeries[] _deviceStatusSeries;
        public ISeries[] DeviceStatusSeries
        {
            get => _deviceStatusSeries;
            set { _deviceStatusSeries = value; OnPropertyChanged(nameof(DeviceStatusSeries)); }
        }

        #endregion

        #region Top Incident Devices

        public class TopIncidentDevice
        {
            public string TenThietBi { get; set; }
            public string SoSeri { get; set; }
            public string LoaiThietBi { get; set; }
            public int SoSuCo { get; set; }
            public string LanGanNhat { get; set; }
            public string TrangThai { get; set; }
        }

        private ObservableCollection<TopIncidentDevice> _topIncidentDevices;
        public ObservableCollection<TopIncidentDevice> TopIncidentDevices
        {
            get => _topIncidentDevices;
            set { _topIncidentDevices = value; OnPropertyChanged(nameof(TopIncidentDevices)); }
        }

        #endregion

        #region Year Filter

        // FIX: thêm bộ lọc theo năm
        private int _selectedYear;
        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                _selectedYear = value;
                OnPropertyChanged(nameof(SelectedYear));
                _ = LoadData();
            }
        }

        private ObservableCollection<int> _availableYears;
        public ObservableCollection<int> AvailableYears
        {
            get => _availableYears;
            set { _availableYears = value; OnPropertyChanged(nameof(AvailableYears)); }
        }

        #endregion

        #region Loading state

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }

        #endregion

        public ICommand ExportReportCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        public StatisticViewModel()
        {
            int currentYear = DateTime.Now.Year;
            AvailableYears = new ObservableCollection<int>();
            for (int y = currentYear; y >= currentYear - 4; y--)
                AvailableYears.Add(y);

            _selectedYear = currentYear;

            ExportReportCommand = new RelayCommand<object>(p => true, p => ExportReport());
            RefreshCommand = new RelayCommand(async o => await LoadData());

            _ = LoadData();
        }

        private async Task LoadData()
        {
            IsLoading = true;

            try
            {
                // FIX: load toàn bộ data cần thiết rồi đóng context
                // Trước đây context được truyền vào sub-method sau khi đã thoát using → disposed context
                List<BaoTri> baoTris;
                List<ChiTietBaoTri> chiTietBaoTris;
                List<ChiTietThietBi> chiTietThietBis;
                List<BaoCaoSuaChua> baoCaos;

                using (var context = new QuanLyVatTuContext())
                {
                    baoTris = await context.BaoTris.ToListAsync();
                    chiTietBaoTris = await context.ChiTietBaoTris
                        .Include(x => x.IdbaoTriNavigation)
                        .Include(x => x.IddichVuNavigation)
                        .Include(x => x.ChiTietThietBi)
                            .ThenInclude(x => x.IdthietBiNavigation)
                        .ToListAsync();
                    chiTietThietBis = await context.ChiTietThietBis
                        .Include(x => x.IdthietBiNavigation)
                        .ToListAsync();
                    baoCaos = await context.BaoCaoSuaChuas
                        .Include(x => x.ChiTietThietBi)
                            .ThenInclude(x => x.IdthietBiNavigation)
                        .ToListAsync();
                }

                // Summary
                TotalDevices = chiTietThietBis.Count;
                ActiveDevices = chiTietThietBis.Count(x => x.TinhTrang == "Tốt");
                MaintenanceThisMonthCount = baoTris.Count(x =>
                    x.NgayBaoTri.HasValue &&
                    x.NgayBaoTri.Value.Month == DateTime.Now.Month &&
                    x.NgayBaoTri.Value.Year == DateTime.Now.Year);
                UnresolvedIncidents = baoCaos.Count(x => x.TrangThai != "Đã giải quyết");

                // FIX: tất cả chart đều filter theo SelectedYear
                var baoTrisOfYear = baoTris
                    .Where(x => x.NgayBaoTri.HasValue && x.NgayBaoTri.Value.Year == SelectedYear)
                    .ToList();

                var chiTietOfYear = chiTietBaoTris
                    .Where(x => x.IdbaoTriNavigation?.NgayBaoTri.HasValue == true
                             && x.IdbaoTriNavigation.NgayBaoTri.Value.Year == SelectedYear)
                    .ToList();

                // Tổng chi phí năm
                TotalCostThisYear = chiTietOfYear.Sum(x => x.IddichVuNavigation?.GiaDichVu ?? 0);
                OnPropertyChanged(nameof(TotalCostFormatted));

                // Charts — truyền data đã load, không truyền context
                LoadMaintenanceChart(baoTrisOfYear);
                LoadCostChart(chiTietOfYear);
                LoadDeviceStatusChart(chiTietThietBis);
                LoadTopIncidentDevices(baoCaos);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu thống kê:\n{ex.Message}", "Lỗi hệ thống",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // FIX: nhận List<BaoTri> thay vì context — context đã Dispose rồi không dùng được
        private void LoadMaintenanceChart(List<BaoTri> baoTris)
        {
            int[] values = new int[12];

            var data = baoTris
                .Where(x => x.NgayBaoTri != null)
                .GroupBy(x => x.NgayBaoTri.Value.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() });

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
                    Labels = new[] { "T1","T2","T3","T4","T5","T6","T7","T8","T9","T10","T11","T12" }
                }
            };
        }

        private void LoadCostChart(List<ChiTietBaoTri> chiTietBaoTris)
        {
            double[] values = new double[12];

            var data = chiTietBaoTris
                .Where(x => x.IdbaoTriNavigation?.NgayBaoTri != null)
                .GroupBy(x => x.IdbaoTriNavigation.NgayBaoTri.Value.Month)
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
                    Name = "Chi phí (đ)",
                    Values = values,
                    Fill = null,
                    GeometrySize = 10
                }
            };

            CostXAxes = new Axis[]
            {
                new Axis
                {
                    Labels = new[] { "T1","T2","T3","T4","T5","T6","T7","T8","T9","T10","T11","T12" }
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

        private void LoadDeviceStatusChart(List<ChiTietThietBi> chiTietThietBis)
        {
            int good = chiTietThietBis.Count(x => x.TinhTrang == "Tốt");
            int error = chiTietThietBis.Count(x => x.TinhTrang == "Lỗi");
            int maintenance = chiTietThietBis.Count(x => x.TinhTrang == "Đang bảo trì");

            DeviceStatusSeries = new ISeries[]
            {
                new PieSeries<int> { Name = "Tốt", Values = new[] { good } },
                new PieSeries<int> { Name = "Lỗi", Values = new[] { error } },
                new PieSeries<int> { Name = "Đang bảo trì", Values = new[] { maintenance } }
            };
        }

        private void LoadTopIncidentDevices(List<BaoCaoSuaChua> baoCaos)
        {
            var data = baoCaos
                .GroupBy(x => new { x.IdthietBi, x.SoSeri })
                .Select(g =>
                {
                    var latest = g.OrderByDescending(x => x.NgayBaoCao).FirstOrDefault();
                    var thietBi = latest?.ChiTietThietBi?.IdthietBiNavigation;

                    return new TopIncidentDevice
                    {
                        TenThietBi = thietBi?.TenThietBi ?? "Thiết bị đã xóa",
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
                FileName = $"BaoCaoThongKe_{SelectedYear}",
                DefaultExt = ".csv",
                Filter = "CSV file (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() != true) return;

            var sb = new StringBuilder();

            sb.AppendLine("BÁO CÁO THỐNG KÊ HỆ THỐNG QUẢN LÝ THIẾT BỊ");
            sb.AppendLine($"Năm thống kê,{SelectedYear}");
            sb.AppendLine($"Ngày xuất,{DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine();

            sb.AppendLine("TỔNG QUAN");
            sb.AppendLine($"Tổng thiết bị (serial),{TotalDevices}");
            sb.AppendLine($"Đang hoạt động tốt,{ActiveDevices}");
            sb.AppendLine($"Bảo trì trong tháng này,{MaintenanceThisMonthCount}");
            sb.AppendLine($"Sự cố chưa xử lý,{UnresolvedIncidents}");
            sb.AppendLine($"Tổng chi phí năm {SelectedYear},{TotalCostThisYear:F0} đ");
            sb.AppendLine();

            sb.AppendLine($"CÔNG VIỆC BẢO TRÌ THEO THÁNG - NĂM {SelectedYear}");
            sb.AppendLine("Tháng,Số công việc");
            if (MaintenanceSeries != null && MaintenanceSeries.Length > 0)
            {
                var values = MaintenanceSeries[0].Values.Cast<int>().ToList();
                for (int i = 0; i < values.Count; i++)
                    sb.AppendLine($"Tháng {i + 1},{values[i]}");
            }

            sb.AppendLine();
            sb.AppendLine($"CHI PHÍ BẢO TRÌ THEO THÁNG - NĂM {SelectedYear}");
            sb.AppendLine("Tháng,Chi phí (đ)");
            if (CostSeries != null && CostSeries.Length > 0)
            {
                var values = CostSeries[0].Values.Cast<double>().ToList();
                for (int i = 0; i < values.Count; i++)
                    sb.AppendLine($"Tháng {i + 1},{values[i]:F0}");
            }

            sb.AppendLine();
            sb.AppendLine("TOP 5 THIẾT BỊ SỰ CỐ NHIỀU NHẤT");
            sb.AppendLine("Tên thiết bị,Số Serial,Loại,Số sự cố,Lần gần nhất,Trạng thái");
            if (TopIncidentDevices != null)
            {
                foreach (var item in TopIncidentDevices)
                    sb.AppendLine($"{item.TenThietBi},{item.SoSeri},{item.LoaiThietBi},{item.SoSuCo},{item.LanGanNhat},{item.TrangThai}");
            }

            try
            {
                File.WriteAllText(dialog.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Xuất báo cáo thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xuất file:\n{ex.Message}", "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}