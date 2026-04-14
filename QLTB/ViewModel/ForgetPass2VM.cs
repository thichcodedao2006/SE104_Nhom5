using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QLTB.ViewModel
{
    public class OtpChar : BaseViewModel
    {
        private string value = "";
        private bool isEnable = false;
        public string Value { get => value; set
            {
                this.value = value;
                OnPropertyChanged();
            } }

        public bool IsEnable { get => isEnable; set
            {
                isEnable = value;
                OnPropertyChanged();
            } }
    }
    public class ForgetPass2VM : BaseViewModel
    {
        public ObservableCollection<OtpChar> OtpList { get; set; }  

        public ForgetPass2VM()
        {
            // 1. Tạo 6 ô (mặc định đang bị khóa hết)
            OtpList = new ObservableCollection<OtpChar>
        {
            new OtpChar(), new OtpChar(), new OtpChar(),
            new OtpChar(), new OtpChar(), new OtpChar()
        };

            // 2. Mở khóa riêng cho ô ĐẦU TIÊN
            OtpList[0].IsEnable = true;

            // 3. Viết logic: Cứ ô này gõ xong thì mở khóa ô tiếp theo
            for (int i = 0; i < OtpList.Count; i++)
            {
                int currentIndex = i; // Tạo biến tạm để tránh lỗi closure trong vòng lặp

                // Lắng nghe sự thay đổi của từng ô
                OtpList[i].PropertyChanged += (s, e) =>
                {
                    // Nếu thuộc tính Value bị thay đổi
                    if (e.PropertyName == nameof(OtpChar.Value))
                    {
                        // Nếu ô hiện tại có chữ, và chưa phải là ô cuối cùng (vị trí số 5)
                        if (!string.IsNullOrEmpty(OtpList[currentIndex].Value) && currentIndex < OtpList.Count - 1)
                        {
                            // Lập tức mở khóa ô kế tiếp!
                            OtpList[currentIndex + 1].IsEnable = true;
                            
                        }
                    }
                };
            }
        }



    }
}
