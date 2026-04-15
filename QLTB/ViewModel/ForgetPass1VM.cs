using QLTB.Model;
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
        private ForgetPass data;
        public ICommand ConfirmCommand {  get; set; }

        public Action<ForgetPass> DoneWithUsername { get; set; }
        public ForgetPass Data { get => data; set => data = value; }

        public ForgetPass1VM(ForgetPass data)
        {
            Data = data;
            ConfirmCommand = new RelayCommand<string>(o => true, o =>
            {
                // kiểm tra Database 
                DoneWithUsername.Invoke(Data);
            });
        }
    }
}
