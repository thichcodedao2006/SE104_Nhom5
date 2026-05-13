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
    // Model class for Incident Report
    public class IncidentReport
    {
        public string DeviceName { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; } // Nghiêm trọng, Cao, Trung bình, Thấp
        public string Status { get; set; } // Mới mở, Đang xử lý, Đã giải quyết
        public string ReportedBy { get; set; }
        public string ReportedAt { get; set; }
        public string AssignedTo { get; set; }
        public bool HasAssignee { get; set; }
    }

    public class IncidentReportViewModel : BaseViewModel
    {
        public ObservableCollection<IncidentReport> Incidents { get; set; }

        // Statistics
        private int _totalIncidents;
        public int TotalIncidents
        {
            get => _totalIncidents;
            set
            {
                _totalIncidents = value;
                OnPropertyChanged(nameof(TotalIncidents));
            }
        }

        private int _openIncidents;
        public int OpenIncidents
        {
            get => _openIncidents;
            set
            {
                _openIncidents = value;
                OnPropertyChanged(nameof(OpenIncidents));
            }
        }

        private int _inProgressIncidents;
        public int InProgressIncidents
        {
            get => _inProgressIncidents;
            set
            {
                _inProgressIncidents = value;
                OnPropertyChanged(nameof(InProgressIncidents));
            }
        }

        private int _resolvedIncidents;
        public int ResolvedIncidents
        {
            get => _resolvedIncidents;
            set
            {
                _resolvedIncidents = value;
                OnPropertyChanged(nameof(ResolvedIncidents));
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

        public ICommand ReportIncidentCommand { get; set; }
        public ICommand ViewDetailsCommand { get; set; }

        public IncidentReportViewModel()
        {
            // Sample data
            Incidents = new ObservableCollection<IncidentReport>
            {
                new IncidentReport
                {
                    DeviceName = "Máy nén khí",
                    Description = "Tiếng ồn bất thường và áp suất dao động được phát hiện",
                    Priority = "Cao",
                    Status = "Đang xử lý",
                    ReportedBy = "Nhân viên Mike",
                    ReportedAt = "2026-05-10 14:30",
                    AssignedTo = "John Doe",
                    HasAssignee = true
                },
                new IncidentReport
                {
                    DeviceName = "Máy CNC B",
                    Description = "Quá nhiệt trong quá trình vận hành, tự động tắt máy được kích hoạt",
                    Priority = "Nghiêm trọng",
                    Status = "Mới mở",
                    ReportedBy = "Giám sát viên Tom",
                    ReportedAt = "2026-05-11 09:15",
                    AssignedTo = "",
                    HasAssignee = false
                },
                new IncidentReport
                {
                    DeviceName = "Hệ thống làm mát",
                    Description = "Nhiệt độ vượt quá giới hạn hoạt động bình thường",
                    Priority = "Cao",
                    Status = "Đã giải quyết",
                    ReportedBy = "Kỹ thuật viên Sarah",
                    ReportedAt = "2026-05-08 11:20",
                    AssignedTo = "Alice Brown",
                    HasAssignee = true
                }
            };

            // Calculate statistics
            UpdateStatistics();

            // Commands
            ReportIncidentCommand = new RelayCommand(o =>
            {
                // Open report incident form
            });

            ViewDetailsCommand = new RelayCommand(o =>
            {
                if (o is IncidentReport incident)
                {
                    // Open incident details
                }
            });
        }

        private void UpdateStatistics()
        {
            TotalIncidents = Incidents.Count;
            OpenIncidents = Incidents.Count(i => i.Status == "Mới mở");
            InProgressIncidents = Incidents.Count(i => i.Status == "Đang xử lý");
            ResolvedIncidents = Incidents.Count(i => i.Status == "Đã giải quyết");
        }
    }
}
