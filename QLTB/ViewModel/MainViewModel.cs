using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using QLTB.Helpers;
namespace QLTB.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private object _currentViewModel;

        public object CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public ICommand OpenDashboardCommand { get; set; }
        public ICommand OpenMaterialCommand { get; set; }
        public ICommand OpenDeviceCommand { get; set; }
        public ICommand OpenEmployeeCommand { get; set; }
        public ICommand OpenMaintenancePlanCommand { get; set; }
        public ICommand OpenMaintenanceTaskCommand { get; set; }
        public ICommand OpenMaintenanceHistoryCommand { get; set; }
        public ICommand OpenIncidentReportCommand { get; set; }
        public ICommand OpenSettingCommand { get; set; }
        public MainViewModel()
        {
           CurrentViewModel = new DashBoardViewModel();

            OpenDashboardCommand = new RelayCommand(o =>
            {
                CurrentViewModel = new DashBoardViewModel();
            });

            OpenMaterialCommand = new RelayCommand(o =>
            {
                CurrentViewModel = new MaterialViewModel();
            });

            OpenDeviceCommand = new RelayCommand(o =>
            {
                CurrentViewModel = new DeviceViewModel();
            });

            OpenEmployeeCommand = new RelayCommand(o =>
            {
                CurrentViewModel = new EmployeeViewModel();
            });

            OpenMaintenancePlanCommand = new RelayCommand(o =>
            {
                CurrentViewModel = new MaintenancePlanViewModel();
            });

            OpenMaintenanceTaskCommand = new RelayCommand(o =>
            {
                CurrentViewModel = new MaintenanceTaskViewModel();
            });

            OpenMaintenanceHistoryCommand = new RelayCommand(o =>
            {
                CurrentViewModel = new MaintenanceHistoryViewModel();
            });

            OpenIncidentReportCommand = new RelayCommand(o =>
            {
                CurrentViewModel = new IncidentReportViewModel();
            });

            OpenSettingCommand = new RelayCommand(o =>
            {
                CurrentViewModel = new SettingViewModel();
            });
        }
    }
}
