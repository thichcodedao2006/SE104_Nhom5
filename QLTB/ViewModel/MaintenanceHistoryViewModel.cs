using QLTB.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    // Model class for Maintenance History Record
    public class MaintenanceHistoryRecord
    {
        public string Device { get; set; }
        public string Type { get; set; }
        public string CompletedDate { get; set; }
        public string Technician { get; set; }
        public int Cost { get; set; }
    }

    public class MaintenanceHistoryViewModel : BaseViewModel
    {
        public ObservableCollection<MaintenanceHistoryRecord> HistoryRecords { get; set; }

        // Statistics
        private int _totalRecords;
        public int TotalRecords
        {
            get => _totalRecords;
            set
            {
                _totalRecords = value;
                OnPropertyChanged(nameof(TotalRecords));
            }
        }

        private int _thisMonth;
        public int ThisMonth
        {
            get => _thisMonth;
            set
            {
                _thisMonth = value;
                OnPropertyChanged(nameof(ThisMonth));
            }
        }

        private int _totalCost;
        public int TotalCost
        {
            get => _totalCost;
            set
            {
                _totalCost = value;
                OnPropertyChanged(nameof(TotalCost));
            }
        }

        private int _avgCost;
        public int AvgCost
        {
            get => _avgCost;
            set
            {
                _avgCost = value;
                OnPropertyChanged(nameof(AvgCost));
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public ICommand ViewDetailsCommand { get; set; }

        public MaintenanceHistoryViewModel()
        {
            // Sample data
            HistoryRecords = new ObservableCollection<MaintenanceHistoryRecord>
            {
                new MaintenanceHistoryRecord
                {
                    Device = "CNC Machine A",
                    Type = "Preventive Maintenance",
                    CompletedDate = "2026-04-15",
                    Technician = "John Doe",
                    Cost = 450
                },
                new MaintenanceHistoryRecord
                {
                    Device = "Hydraulic Press B",
                    Type = "Repair",
                    CompletedDate = "2026-04-10",
                    Technician = "Jane Smith",
                    Cost = 1200
                },
                new MaintenanceHistoryRecord
                {
                    Device = "Conveyor System",
                    Type = "Inspection",
                    CompletedDate = "2026-04-05",
                    Technician = "Bob Johnson",
                    Cost = 150
                },
                new MaintenanceHistoryRecord
                {
                    Device = "Welding Robot",
                    Type = "Preventive Maintenance",
                    CompletedDate = "2026-03-28",
                    Technician = "Alice Brown",
                    Cost = 680
                },
                new MaintenanceHistoryRecord
                {
                    Device = "Air Compressor",
                    Type = "Emergency Repair",
                    CompletedDate = "2026-03-20",
                    Technician = "John Doe",
                    Cost = 850
                }
            };

            // Calculate statistics
            UpdateStatistics();

            // Commands
            ViewDetailsCommand = new RelayCommand(o =>
            {
                if (o is MaintenanceHistoryRecord record)
                {
                    // Open details view
                }
            });
        }

        private void UpdateStatistics()
        {
            TotalRecords = HistoryRecords.Count;
            
            // Count records from this month (May 2026)
            ThisMonth = HistoryRecords.Count(r => 
            {
                var date = DateTime.Parse(r.CompletedDate);
                return date.Month == 5 && date.Year == 2026;
            });
            
            TotalCost = HistoryRecords.Sum(r => r.Cost);
            AvgCost = TotalRecords > 0 ? TotalCost / TotalRecords : 0;
        }
    }
}
