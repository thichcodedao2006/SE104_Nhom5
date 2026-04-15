using QLTB.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLTB.Model
{
    public class OtpChar : BaseViewModel
    {
        private string value;
        private bool isEnable = false;

        public string Value { get => value; set
            {
                this.value = value;
                OnPropertyChanged();
            }
                }
        public bool IsEnable { get => isEnable; set
            {
                isEnable = value;
                OnPropertyChanged();
            }
                }
    }

    public class ForgetPass : BaseViewModel
    {
        private string username;
        private ObservableCollection<OtpChar> otpList;
        private string password;
        private string retypepass;

        public string Username { get => username; set
            {
                username = value;
                OnPropertyChanged();
            } }
        
        public string Password { get => password; set
            {
                password = value; OnPropertyChanged();
            }
                }
        public string Retypepass { get => retypepass; set
            {
                retypepass = value; OnPropertyChanged();
            }
                }

        public ObservableCollection<OtpChar> OtpList { get => otpList; 
        set
            {
                otpList = value; OnPropertyChanged();
            }
        }


        public ForgetPass()
        {
            LoadTextBox();
        }

        private void LoadTextBox()
        {
            OtpList = new ObservableCollection<OtpChar>
            {
                new OtpChar(), new OtpChar(), new OtpChar(),
                new OtpChar(), new OtpChar(), new OtpChar()
            };

            // mở khóa cho ô đầu 
            OtpList[0].IsEnable = true;
            for (int i = 0; i < OtpList.Count; i++)
            {
                int currentIndex = i;
                OtpList[i].PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(OtpChar.Value))
                    {
                        if (!string.IsNullOrEmpty(OtpList[currentIndex].Value) && currentIndex < OtpList.Count - 1)
                        {
                            OtpList[currentIndex + 1].IsEnable = true;
                        }
                    }
                };
            }
        }
    }
}
