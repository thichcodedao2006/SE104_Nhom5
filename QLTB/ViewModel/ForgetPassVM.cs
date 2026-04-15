using QLTB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QLTB.ViewModel
{
    public class ForgetPassVM:BaseViewModel
    {
        #region Declare
        private object currentUC;
        private ForgetPass1VM vm1;
        private ForgetPass2VM vm2;
        private ForgetPass3VM vm3;
        private ForgetPass share_data;
        #endregion

        #region Property
        public object CurrentUC
        {
            get => currentUC;
            set
            {
                currentUC = value;
                OnPropertyChanged();
            }
        }

        public ForgetPass Share_data { get => share_data; set
            {
                share_data = value;
                OnPropertyChanged();
            }
                }
        #endregion

        public ForgetPassVM()
        {
            Share_data = new ForgetPass();
            vm1 = new ForgetPass1VM(Share_data);
            CurrentUC = vm1;
            vm1.DoneWithUsername += (data) =>
            {
                vm2 = new ForgetPass2VM(data);
                CurrentUC = vm2;
                vm2.BackDone += (data) =>
                {
                    CurrentUC = vm1;
                };
                vm2.NextDone += (data) =>
                {
                    vm3 = new ForgetPass3VM(data);
                    CurrentUC = vm3;
                };
            };
        }

        #region Command

        
        #endregion

        #region Load
        
        #endregion
    }
}
