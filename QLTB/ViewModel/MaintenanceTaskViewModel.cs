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
    // Model class for Maintenance Task
    public class MaintenanceTaskItem
    {
        public string Device { get; set; }
        public string Type { get; set; }
        public string DueDate { get; set; }
        public string AssignedTo { get; set; }
        public string Priority { get; set; } // High, Medium, Low
        public string Status { get; set; } // Overdue, Pending, In Progress, Completed
    }

    public class MaintenanceTaskViewModel : BaseViewModel
    {
        public ObservableCollection<MaintenanceTaskItem> Tasks { get; set; }

        // Statistics
        private int _totalTasks;
        public int TotalTasks
        {
            get => _totalTasks;
            set
            {
                _totalTasks = value;
                OnPropertyChanged(nameof(TotalTasks));
            }
        }

        private int _pendingTasks;
        public int PendingTasks
        {
            get => _pendingTasks;
            set
            {
                _pendingTasks = value;
                OnPropertyChanged(nameof(PendingTasks));
            }
        }

        private int _overdueTasks;
        public int OverdueTasks
        {
            get => _overdueTasks;
            set
            {
                _overdueTasks = value;
                OnPropertyChanged(nameof(OverdueTasks));
            }
        }

        private int _completedTasks;
        public int CompletedTasks
        {
            get => _completedTasks;
            set
            {
                _completedTasks = value;
                OnPropertyChanged(nameof(CompletedTasks));
            }
        }

        public ICommand CompleteTaskCommand { get; set; }

        public MaintenanceTaskViewModel()
        {
            // Sample data
            Tasks = new ObservableCollection<MaintenanceTaskItem>
            {
                new MaintenanceTaskItem
                {
                    Device = "CNC Machine A",
                    Type = "Preventive Maintenance",
                    DueDate = "2026-05-12",
                    AssignedTo = "John Doe",
                    Priority = "Cao",
                    Status = "Quá hạn"
                },
                new MaintenanceTaskItem
                {
                    Device = "Hydraulic Press B",
                    Type = "Inspection",
                    DueDate = "2026-05-16",
                    AssignedTo = "Jane Smith",
                    Priority = "Trung bình",
                    Status = "Đang chờ"
                },
                new MaintenanceTaskItem
                {
                    Device = "Conveyor System",
                    Type = "Repair",
                    DueDate = "2026-05-13",
                    AssignedTo = "Bob Johnson",
                    Priority = "Cao",
                    Status = "Đang tiến hành"
                },
                new MaintenanceTaskItem
                {
                    Device = "Welding Robot",
                    Type = "Preventive Maintenance",
                    DueDate = "2026-05-18",
                    AssignedTo = "Alice Brown",
                    Priority = "Thấp",
                    Status = "Đang chờ"
                },
                new MaintenanceTaskItem
                {
                    Device = "Air Compressor",
                    Type = "Emergency Repair",
                    DueDate = "2026-05-11",
                    AssignedTo = "John Doe",
                    Priority = "Cao",
                    Status = "Hoàn thành"
                }
            };

            // Calculate statistics
            UpdateStatistics();

            // Commands
            CompleteTaskCommand = new RelayCommand(o =>
            {
                if (o is MaintenanceTaskItem task)
                {
                    task.Status = "Hoàn thành";
                    UpdateStatistics();
                }
            });
        }

        private void UpdateStatistics()
        {
            TotalTasks = Tasks.Count;
            PendingTasks = Tasks.Count(t => t.Status == "Đang chờ");
            OverdueTasks = Tasks.Count(t => t.Status == "Quá hạn");
            CompletedTasks = Tasks.Count(t => t.Status == "Hoàn thành");
        }
    }
}
