using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using QLTB.Data;
using QLTB.Helpers;
using QLTB.Models;
namespace QLTB.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        #region Declare
        private object _currentViewModel;

        private string _currentAvatar;
        private TaiKhoan userAccount;
        private NhanVien userDetail;

        public object CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }


        private DashBoardViewModel _dashboardVM;
        private MaterialViewModel _materialVM;
        private DeviceViewModel _deviceVM;
        private EmployeeViewModel _employeeVM;
        private MaintenancePlanViewModel _maintenancePlanVM;
        private MaintenanceTaskViewModel _maintenanceTaskVM;
        private MaintenanceHistoryViewModel _maintenanceHistoryVM;
        private IncidentReportViewModel _incidentReportVm;
        private SettingViewModel _settingVM;
        #endregion

        public ICommand OpenDashboardCommand { get; set; }
        public ICommand OpenMaterialCommand { get; set; }
        public ICommand OpenDeviceCommand { get; set; }
        public ICommand OpenEmployeeCommand { get; set; }
        public ICommand OpenMaintenancePlanCommand { get; set; }
        public ICommand OpenMaintenanceTaskCommand { get; set; }
        public ICommand OpenMaintenanceHistoryCommand { get; set; }
        public ICommand OpenIncidentReportCommand { get; set; }
        public ICommand OpenSettingCommand { get; set; }
        public ICommand OpenStatisticCommand { get; set; }

        public ICommand SignOutCommand { get; set; }
        public string CurrentAvatar { get => _currentAvatar; set
            {
                _currentAvatar = value;
                OnPropertyChanged(nameof(CurrentAvatar));
            }
                }

        public TaiKhoan UserAccount { get => userAccount; set
            {
                userAccount = value;
                OnPropertyChanged(nameof(UserAccount));
            }
                }
        public NhanVien UserDetail { get => userDetail; set
            {
                userDetail = value;
                OnPropertyChanged(nameof(UserDetail));  
            }
                }



        public MainViewModel(TaiKhoan t)
        {
            EventSystem.AvatarChange += ImageChange;

            SetUpUser(t);

            _dashboardVM = new DashBoardViewModel(t);
            CurrentViewModel = _dashboardVM;

            OpenDashboardCommand = new RelayCommand(o =>
            {
                if (_dashboardVM == null)
                {
                    _dashboardVM = new DashBoardViewModel(t);
                }
                CurrentViewModel = _dashboardVM;
                _dashboardVM.Reload();

            });

            OpenMaterialCommand = new RelayCommand(o =>
            {
                if (_materialVM == null)
                {
                    _materialVM = new MaterialViewModel();
                }
                CurrentViewModel = _materialVM;
            });

            OpenDeviceCommand = new RelayCommand(o =>
            {
                if (_deviceVM == null)
                {
                    _deviceVM = new DeviceViewModel();
                }
                CurrentViewModel = _deviceVM;
            });

            OpenEmployeeCommand = new RelayCommand(o =>
            {
                if (_employeeVM == null)
                {
                    _employeeVM = new EmployeeViewModel();
                }
                CurrentViewModel = _employeeVM;
                _employeeVM.Reload();
            });

            OpenMaintenancePlanCommand = new RelayCommand(o =>
            {
                if (_maintenancePlanVM == null)
                {
                    _maintenancePlanVM = new MaintenancePlanViewModel();
                }
                CurrentViewModel = _maintenancePlanVM;
            });

            OpenMaintenanceTaskCommand = new RelayCommand(o =>
            {
                if (_maintenanceTaskVM == null)
                {
                    _maintenanceTaskVM = new MaintenanceTaskViewModel();
                }
                CurrentViewModel = _maintenanceTaskVM;
            });

            OpenMaintenanceHistoryCommand = new RelayCommand(o =>
            {
                if (_maintenanceHistoryVM == null)
                {
                    _maintenanceHistoryVM = new MaintenanceHistoryViewModel();
                }
                CurrentViewModel = _maintenanceHistoryVM;
            });

            OpenIncidentReportCommand = new RelayCommand(o =>
            {
                if (_incidentReportVm == null)
                {
                    _incidentReportVm = new IncidentReportViewModel();
                }
                CurrentViewModel = _incidentReportVm;
                _incidentReportVm.Reload();
            });

            OpenSettingCommand = new RelayCommand(o =>
            {
                if (_settingVM == null)
                {
                    _settingVM = new SettingViewModel(t, UserDetail);
                }
                CurrentViewModel = _settingVM;
            });

            OpenStatisticCommand = new RelayCommand(o =>
            {
                CurrentViewModel = new StatisticViewModel();
            });
            SignOutCommand = new RelayCommand<Window>
                (
                    (p) => true, (p) => LogOut(p)
                );

        }

        public void Dispose()
        {
            EventSystem.AvatarChange -= ImageChange;
        }

        private void SetUpUser(TaiKhoan t)
        {
            UserAccount = t;
            var nv = DataProvider.Instance.DB.NhanViens.FirstOrDefault(x => x.Email == t.Email);
            if (nv != null)
            {
                UserDetail = nv;
            }
            ChangeUserAvatar();
        }

        private void LogOut(Window p)
        {
            
            fmDangNhap dn = new fmDangNhap();
            dn.Show();
            if (p != null)
            {
                p.Close();
            }
        }

        private void ChangeUserAvatar()
        {
            CurrentAvatar = CloudinaryService.GetImageUrl(KeyData.AvatarFolder, KeyData.NhanVienTag + UserDetail.IdnhanVien); // thay đổi Avatar;
        }

        private void ImageChange(string newLink)
        {
            CurrentAvatar = newLink;
        }
        
    }
}
