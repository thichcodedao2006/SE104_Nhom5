using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLTB.ViewModel
{
    public class ForgetPassVM:BaseViewModel
    {
        private object currentUC;

        public object CurrentUC { get => currentUC;
            set
            { 
                currentUC = value;
                OnPropertyChanged();
            } }

        public ForgetPassVM()
        {
            currentUC = new ForgetPass1VM(ConfirmUsername); // Gán giá trị ViewModel tương ứng hiển thị UC tương ứng.
        }

        private void ConfirmUsername(string Username)
        {
            // kiểm tra Database 
            CurrentUC = new ForgetPass2VM();
        }
    }
}
