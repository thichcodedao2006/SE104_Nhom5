using QLTB.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class ForgetPass2VM : BaseViewModel
    {
        private ForgetPass data;

        public ICommand BackCommand {  get; set; }
        public ICommand NextCommand { get; set; }
        public ForgetPass Data { get => data; set => data = value; }

        public Action<ForgetPass> BackDone {  get; set; }
        public Action<ForgetPass> NextDone { get; set; }

        public ForgetPass2VM(ForgetPass data)
        {
            Data = data;
            BackCommand = new RelayCommand<object>(o => true, o =>
            {
                //logic 
                BackDone.Invoke(Data);
            }
            );
            NextCommand = new RelayCommand<object>(o => true, o =>
            {
                //logic
                NextDone.Invoke(Data);
            }
            );
        }

    }
}
