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
    // Model class for Maintenance Plan
    public class MaintenancePlanItem
    {
        public string Title { get; set; }
        public string Equipment { get; set; }
        public string Priority { get; set; } // High Priority, Medium Priority, Low Priority
        public string Status { get; set; } // Active, Inactive, Completed
        public string Type { get; set; } // Preventive, Corrective
        public string NextDue { get; set; }
        public string Schedule { get; set; } // Monthly - 1st, Quarterly, Weekly, etc.
        public string AssignedTo { get; set; }
        public decimal EstimatedCost { get; set; }
    }

    public class MaintenancePlanViewModel : BaseViewModel
    {
        public ObservableCollection<MaintenancePlanItem> Plans { get; set; }

        // Statistics
        private int _totalPlans;
        public int TotalPlans
        {
            get => _totalPlans;
            set
            {
                _totalPlans = value;
                OnPropertyChanged(nameof(TotalPlans));
            }
        }

        private int _activePlans;
        public int ActivePlans
        {
            get => _activePlans;
            set
            {
                _activePlans = value;
                OnPropertyChanged(nameof(ActivePlans));
            }
        }

        private int _dueThisMonth;
        public int DueThisMonth
        {
            get => _dueThisMonth;
            set
            {
                _dueThisMonth = value;
                OnPropertyChanged(nameof(DueThisMonth));
            }
        }

        private decimal _estimatedMonthlyCost;
        public decimal EstimatedMonthlyCost
        {
            get => _estimatedMonthlyCost;
            set
            {
                _estimatedMonthlyCost = value;
                OnPropertyChanged(nameof(EstimatedMonthlyCost));
            }
        }

        // View mode
        private bool _isListView;
        public bool IsListView
        {
            get => _isListView;
            set
            {
                _isListView = value;
                OnPropertyChanged(nameof(IsListView));
            }
        }

        public ICommand CreatePlanCommand { get; set; }
        public ICommand ViewDetailsCommand { get; set; }
        public ICommand SwitchToListViewCommand { get; set; }
        public ICommand SwitchToCalendarViewCommand { get; set; }

        public MaintenancePlanViewModel()
        {
            IsListView = true;

            // Sample data
            Plans = new ObservableCollection<MaintenancePlanItem>
            {
                new MaintenancePlanItem
                {
                    Title = "Monthly CNC Inspection",
                    Equipment = "CNC Machine A",
                    Priority = "Cao",
                    Status = "Hoạt động",
                    Type = "Preventive",
                    NextDue = "2026-06-01",
                    Schedule = "Monthly - 1st",
                    AssignedTo = "John Doe",
                    EstimatedCost = 500
                },
                new MaintenancePlanItem
                {
                    Title = "Quarterly Hydraulic Service",
                    Equipment = "Hydraulic Press B",
                    Priority = "Trung bình",
                    Status = "Active",
                    Type = "Preventive",
                    NextDue = "2026-07-15",
                    Schedule = "Quarterly",
                    AssignedTo = "Jane Smith",
                    EstimatedCost = 1200
                },
                new MaintenancePlanItem
                {
                    Title = "Weekly Safety Check",
                    Equipment = "Conveyor System C",
                    Priority = "Cao",
                    Status = "Active",
                    Type = "Preventive",
                    NextDue = "2026-05-19",
                    Schedule = "Weekly",
                    AssignedTo = "Mike Johnson",
                    EstimatedCost = 150
                }
            };

            // Calculate statistics
            UpdateStatistics();

            // Commands
            CreatePlanCommand = new RelayCommand(o =>
            {
                // Open create plan form
            });

            ViewDetailsCommand = new RelayCommand(o =>
            {
                // Open plan details
            });

            SwitchToListViewCommand = new RelayCommand(o =>
            {
                IsListView = true;
            });

            SwitchToCalendarViewCommand = new RelayCommand(o =>
            {
                IsListView = false;
            });
        }

        private void UpdateStatistics()
        {
            TotalPlans = Plans.Count;
            ActivePlans = Plans.Count(p => p.Status == "Active");
            DueThisMonth = Plans.Count(p => DateTime.Parse(p.NextDue).Month == DateTime.Now.Month);
            EstimatedMonthlyCost = Plans.Where(p => p.Status == "Active").Sum(p => p.EstimatedCost);
        }
    }
}
