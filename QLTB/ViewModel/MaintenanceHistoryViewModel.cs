using QLTB.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using QLTB.Models;

namespace QLTB.ViewModel
{
    public class MaintenanceHistoryViewModel : BaseViewModel
    {
        private readonly QuanLyVatTuContext _context;
        private List<ChiTietBaoTri> _allDbRecords;

        private ObservableCollection<ChiTietBaoTri> _historyRecords;
        public ObservableCollection<ChiTietBaoTri> HistoryRecords
        {
            get => _historyRecords;
            set { _historyRecords = value; OnPropertyChanged(nameof(HistoryRecords)); }
        }

        private int _totalRecords;
        public int TotalRecords
        {
            get => _totalRecords;
            set { _totalRecords = value; OnPropertyChanged(nameof(TotalRecords)); }
        }

        private int _thisMonth;
        public int ThisMonth
        {
            get => _thisMonth;
            set { _thisMonth = value; OnPropertyChanged(nameof(ThisMonth)); }
        }

        private double _totalCost;
        public double TotalCost
        {
            get => _totalCost;
            set { _totalCost = value; OnPropertyChanged(nameof(TotalCost)); }
        }

        private double _avgCost;
        public double AvgCost
        {
            get => _avgCost;
            set { _avgCost = value; OnPropertyChanged(nameof(AvgCost)); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                FilterHistory();
                OnPropertyChanged(nameof(SearchText));
            }
        }

        private string _selectedTimeFilter = "Tất cả thời gian";
        public string SelectedTimeFilter
        {
            get => _selectedTimeFilter;
            set
            {
                _selectedTimeFilter = value;
                OnPropertyChanged(nameof(SelectedTimeFilter));
                FilterHistory();
            }
        }

        public ICommand ViewDetailsCommand { get; set; }
        public ICommand ExportToExcelCommand { get; set; }

        public MaintenanceHistoryViewModel()
        {
            _context = new QuanLyVatTuContext();
            _allDbRecords = new List<ChiTietBaoTri>();
            HistoryRecords = new ObservableCollection<ChiTietBaoTri>();

            _ = LoadHistoryFromDatabaseAsync();

            ViewDetailsCommand = new RelayCommand(o =>
            {
                if (o is ChiTietBaoTri record)
                {
                    var baoTri = record.IdbaoTriNavigation;

                    if (baoTri == null)
                    {
                        MessageBox.Show("Không tìm thấy phiếu bảo trì.");
                        return;
                    }

                    var detailViewModel = new MaintenanceDetailViewModel(baoTri);
                    var detailForm = new QLTB.UserControlFolder.Maintenance.MaintenanceDetailView
                    {
                        DataContext = detailViewModel
                    };

                    Window window = new Window
                    {
                        Title = "Chi tiết lịch sử bảo trì",
                        Content = detailForm,
                        SizeToContent = SizeToContent.WidthAndHeight,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        ResizeMode = ResizeMode.NoResize,
                        WindowStyle = WindowStyle.None,
                        AllowsTransparency = true
                    };

                    window.ShowDialog();
                }
            });

            ExportToExcelCommand = new RelayCommand(o => ExportToCsv());
        }

        private async Task LoadHistoryFromDatabaseAsync()
        {
            try
            {
                _allDbRecords = await _context.ChiTietBaoTris
                    .Include(ct => ct.IdbaoTriNavigation)
                        .ThenInclude(bt => bt.IdnhanVienNavigation)
                    .Include(ct => ct.IddichVuNavigation)
                    .Include(ct => ct.ChiTietThietBi)
                        .ThenInclude(tb => tb.IdthietBiNavigation)
                    .Where(ct => ct.IdbaoTriNavigation.TinhTrangBaoTri == "Hoàn thành")
                    .OrderByDescending(ct => ct.IdbaoTriNavigation.NgayBaoTri)
                    .ToListAsync();

                FilterHistory();
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải lịch sử bảo trì: {ex.Message}", "Lỗi hệ thống", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterHistory()
        {
            if (_allDbRecords == null) return;

            var query = _allDbRecords.AsEnumerable();

            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(ct =>
                    (ct.ChiTietThietBi?.IdthietBiNavigation?.TenThietBi != null &&
                     ct.ChiTietThietBi.IdthietBiNavigation.TenThietBi.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||

                    (ct.IddichVuNavigation?.TenDichVu != null &&
                     ct.IddichVuNavigation.TenDichVu.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||

                    (ct.IdbaoTriNavigation?.IdnhanVienNavigation?.HoTen != null &&
                     ct.IdbaoTriNavigation.IdnhanVienNavigation.HoTen.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||

                    (ct.SoSeri != null &&
                     ct.SoSeri.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                );
            }

            DateTime now = DateTime.Now;

            if (SelectedTimeFilter == "Tháng này")
            {
                query = query.Where(ct =>
                    ct.IdbaoTriNavigation.NgayBaoTri.HasValue &&
                    ct.IdbaoTriNavigation.NgayBaoTri.Value.Month == now.Month &&
                    ct.IdbaoTriNavigation.NgayBaoTri.Value.Year == now.Year);
            }
            else if (SelectedTimeFilter == "Tháng trước")
            {
                var firstDayOfThisMonth = new DateTime(now.Year, now.Month, 1);
                var firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);

                query = query.Where(ct =>
                    ct.IdbaoTriNavigation.NgayBaoTri.HasValue &&
                    ct.IdbaoTriNavigation.NgayBaoTri.Value >= firstDayOfLastMonth &&
                    ct.IdbaoTriNavigation.NgayBaoTri.Value < firstDayOfThisMonth);
            }
            else if (SelectedTimeFilter == "3 Tháng trước")
            {
                var threeMonthsAgo = now.AddMonths(-3);

                query = query.Where(ct =>
                    ct.IdbaoTriNavigation.NgayBaoTri.HasValue &&
                    ct.IdbaoTriNavigation.NgayBaoTri.Value >= threeMonthsAgo);
            }

            HistoryRecords = new ObservableCollection<ChiTietBaoTri>(query);
        }

        private void UpdateStatistics()
        {
            TotalRecords = _allDbRecords.Count;
            DateTime now = DateTime.Now;

            ThisMonth = _allDbRecords.Count(ct =>
                ct.IdbaoTriNavigation.NgayBaoTri.HasValue &&
                ct.IdbaoTriNavigation.NgayBaoTri.Value.Month == now.Month &&
                ct.IdbaoTriNavigation.NgayBaoTri.Value.Year == now.Year);

            TotalCost = _allDbRecords.Sum(ct => ct.IddichVuNavigation?.GiaDichVu ?? 0);
            AvgCost = TotalRecords > 0 ? TotalCost / TotalRecords : 0;
        }

        private void ExportToCsv()
        {
            if (HistoryRecords == null || !HistoryRecords.Any())
            {
                MessageBox.Show("Không có dữ liệu để xuất file!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"LichSuBaoTri_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();

                    sb.AppendLine("Thiết bị,Số Seri,Loại hình dịch vụ,Ngày hoàn thành,Kỹ thuật viên,Chi phí (đ)");

                    foreach (var item in HistoryRecords)
                    {
                        string device = item.ChiTietThietBi?.IdthietBiNavigation?.TenThietBi ?? "N/A";
                        string serial = item.SoSeri ?? "N/A";
                        string service = item.IddichVuNavigation?.TenDichVu ?? "N/A";
                        string date = item.IdbaoTriNavigation?.NgayBaoTri.HasValue == true
                            ? item.IdbaoTriNavigation.NgayBaoTri.Value.ToString("dd/MM/yyyy")
                            : "N/A";
                        string tech = item.IdbaoTriNavigation?.IdnhanVienNavigation?.HoTen ?? "N/A";
                        string cost = (item.IddichVuNavigation?.GiaDichVu ?? 0).ToString("F0");

                        device = device.Contains(",") ? $"\"{device}\"" : device;
                        service = service.Contains(",") ? $"\"{service}\"" : service;
                        tech = tech.Contains(",") ? $"\"{tech}\"" : tech;

                        sb.AppendLine($"{device},{serial},{service},{date},{tech},{cost}");
                    }

                    System.IO.File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("Xuất file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xuất file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}