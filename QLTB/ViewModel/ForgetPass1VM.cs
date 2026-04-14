using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace QLTB.ViewModel
{
    public class ForgetPass1VM : BaseViewModel
    {
        private string username;

        private ICommand confirmCommand;

        public ICommand ConfirmCommand { get => confirmCommand; set => confirmCommand = value; }
        public string Username { get => username; set => username = value; }

        public ForgetPass1VM(Action<string> Confirm )
        {
            ConfirmCommand = new RelayCommand<string>(/*o => !string.IsNullOrEmpty(Username),*/o => true, o => Confirm.Invoke(Username));
        }

    }
}
