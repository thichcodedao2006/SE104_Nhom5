using QLTB.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private List<BaoTri> _allDbRecords;

        private ObservableCollection<BaoTri> _historyRecords;
        public ObservableCollection<BaoTri> HistoryRecords
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

        public ICommand ViewDetailsCommand { get; set; }

        public MaintenanceHistoryViewModel()
        {
            _context = new QuanLyVatTuContext();
            _allDbRecords = new List<BaoTri>();
            HistoryRecords = new ObservableCollection<BaoTri>();

            LoadHistoryFromDatabase();

            ViewDetailsCommand = new RelayCommand(o =>
            {
                if (o is BaoTri record)
                {
                    var detailViewModel = new MaintenanceDetailViewModel(record);

                    var detailForm = new QLTB.UserControlFolder.Maintenance.MaintenanceDetailView { DataContext = detailViewModel };

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
        }

        private void LoadHistoryFromDatabase()
        {
            try
            {
                // ĐÃ SỬA: Đổi điều kiện thành "Hoàn thành" để khớp chính xác dữ liệu trong SSMS của bạn
                _allDbRecords = _context.BaoTris
                    .Include(b => b.IddichVuNavigation)
                    .Include(b => b.IdnhanVienNavigation)
                    .Include(b => b.ChiTietThietBi)
                        .ThenInclude(ct => ct.IdthietBiNavigation)
                    .Where(b => b.TinhTrangBaoTri == "Hoàn thành")
                    .OrderByDescending(b => b.NgayBaoTri)
                    .ToList();

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
            if (string.IsNullOrEmpty(SearchText))
            {
                HistoryRecords = new ObservableCollection<BaoTri>(_allDbRecords);
            }
            else
            {
                HistoryRecords = new ObservableCollection<BaoTri>(
                    _allDbRecords.Where(b =>
                        (b.ChiTietThietBi?.IdthietBiNavigation?.TenThietBi != null && b.ChiTietThietBi.IdthietBiNavigation.TenThietBi.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        (b.IddichVuNavigation?.TenDichVu != null && b.IddichVuNavigation.TenDichVu.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        (b.IdnhanVienNavigation?.HoTen != null && b.IdnhanVienNavigation.HoTen.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        (b.SoSeri != null && b.SoSeri.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    ));
            }
        }

        private void UpdateStatistics()
        {
            TotalRecords = _allDbRecords.Count;

            ThisMonth = _allDbRecords.Count(b => b.NgayBaoTri.HasValue &&
                                                b.NgayBaoTri.Value.Month == 5 &&
                                                b.NgayBaoTri.Value.Year == 2026);

            TotalCost = _allDbRecords.Sum(b => b.IddichVuNavigation?.GiaDichVu ?? 0);
            AvgCost = TotalRecords > 0 ? TotalCost / TotalRecords : 0;
        }
    }
}