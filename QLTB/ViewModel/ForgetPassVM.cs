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

        public Action CloseThisWindow { get; set; }
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
            // Khởi tạo data ban đầu
            Share_data = new ForgetPass();

            // Bắt đầu load màn hình 1
            LoadStep1();
        }

        private void LoadStep1()
        {
            // Xóa trắng dữ liệu cũ nếu có
            Share_data = new ForgetPass();

            // Khởi tạo MỚI hoàn toàn vm1
            var vm1 = new ForgetPass1VM(Share_data);

            // Đăng ký sự kiện Next
            vm1.DoneWithUsername += (data) => LoadStep2(data);

            // Gán UC
            CurrentUC = vm1;
        }

        private void LoadStep2(ForgetPass data)
        {
            var vm2 = new ForgetPass2VM(data);

            // Đăng ký sự kiện Back và Next
            vm2.BackDone += (d) => LoadStep1(); // Quay lại thì load lại từ đầu
            vm2.NextDone += (d) => LoadStep3(d);

            CurrentUC = vm2;
        }

        private void LoadStep3(ForgetPass data)
        {
            var vm3 = new ForgetPass3VM(data);
            CurrentUC = vm3;
            vm3.CloseWindow += CloseWindow;
        }


        #region Command
        private void CloseWindow()
        {
            CloseThisWindow?.Invoke();
        }

        #endregion

        #region Load

        #endregion
    }
}
