using QLTB.Helpers;
using QLTB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class SettingViewModel : BaseViewModel
    {
        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }
        private SettingProfileViewModel _profile;
        private SettingSecurityViewModel _security;
        private SettingNotificationViewModel _notification;
        private SettingSystemViewModel _system;

        public ICommand ShowProfileCommand { get; }
        public ICommand ShowSecurityCommand { get; }

        public ICommand ShowNotificationCommand { get; }

        public ICommand ShowSystemCommand { get; }

        public SettingViewModel(TaiKhoan t, NhanVien n)
        {
            _profile = new SettingProfileViewModel(t, n);
            CurrentView = _profile;

            ShowProfileCommand = new RelayCommand<object>(
                 (p) => true,
                 (p) =>
                 {
                     if (_profile == null)
                     {
                         _profile = new SettingProfileViewModel(t, n);
                     }
                     CurrentView = _profile;
                 });
            
            ShowSecurityCommand = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    if (_security == null)
                    {
                        _security = new SettingSecurityViewModel(t);
                    }
                    CurrentView = _security;
                });
            
            ShowNotificationCommand = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    if (_notification == null)
                    {
                        _notification = new SettingNotificationViewModel();
                    }
                    CurrentView = _notification;
                });

            ShowSystemCommand = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    if (_system  == null)
                    {
                        _system = new SettingSystemViewModel();
                    }
                    CurrentView = _system;
                });
        }
    }
}
