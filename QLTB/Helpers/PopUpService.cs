using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QLTB.Helpers
{
    public class PopUpService
    {
        public static void ShowPopUp(object vm)
        {
            Window ownerWindow = Application.Current.MainWindow;

            // 2. Tạo một Window trống để làm vỏ bọc chứa UserControl
            Window dialogWindow = new Window
            {
                Title = "Chi tiết", // Bạn có thể tùy biến title dựa vào ViewModel nếu muốn
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Owner = ownerWindow, // Nằm đè lên MainWindow
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                // Ép thẳng ViewModel vào Content và DataContext
                Content = vm,
                DataContext = vm
            };

            dialogWindow.Loaded += (sender, args) =>
            {
                if (dialogWindow.Owner != null)
                {
                    // Công thức: Tọa độ = Tọa độ cha + (Kích thước cha - Kích thước con) / 2
                    dialogWindow.Left = dialogWindow.Owner.Left + (dialogWindow.Owner.ActualWidth - dialogWindow.ActualWidth) / 2;
                    dialogWindow.Top = dialogWindow.Owner.Top + (dialogWindow.Owner.ActualHeight - dialogWindow.ActualHeight) / 2;
                }
                else
                {
                    // Nếu không tìm thấy cha, cho ra giữa màn hình máy tính luôn
                    dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            };

            // 3. Hiển thị lên
            dialogWindow.ShowDialog();
        }

        public static void ClosePopUp(object vm)
        {
            if (Application.Current == null) return;

            // Duyệt qua toàn bộ các Window đang được mở trong phần mềm
            foreach (Window window in Application.Current.Windows)
            {
                // Nếu tìm thấy Window nào đang chứa cái ViewModel này
                if (window.DataContext == vm)
                {
                    window.Close(); // Đóng cửa sổ đó lại
                    break;          // Thoát khỏi vòng lặp vì đã tìm thấy và xử lý xong
                }
            }
        }
    }
}
