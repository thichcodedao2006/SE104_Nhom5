using QLTB.Helpers;
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

        public ICommand ShowProfileCommand { get; }
        public ICommand ShowSecurityCommand { get; }

        public ICommand ShowNotificationCommand { get; }

        public ICommand ShowSystemCommand { get; }

        public SettingViewModel()
        {
            CurrentView = new SettingProfileViewModel();

            ShowProfileCommand = new RelayCommand<object>(
                 (p) => true,
                 (p) =>
                 {
                     CurrentView = new SettingProfileViewModel();
                 });
            
            ShowSecurityCommand = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    CurrentView = new SettingSecurityViewModel();
                });
            
            ShowNotificationCommand = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    CurrentView = new SettingNotificationViewModel();
                });

            ShowSystemCommand = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    CurrentView = new SettingSystemViewModel();
                });
        }
    }
}
