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
            try
            {
                Window ownerWindow = Application.Current.MainWindow;

                Window dialogWindow = new Window
                {
                    Title = "Chi tiết",
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Owner = ownerWindow,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    Content = vm,
                    // Đảm bảo Window kế thừa Resources từ Application
                    Resources = Application.Current.Resources
                };

                dialogWindow.Loaded += (sender, args) =>
                {
                    try
                    {
                        if (dialogWindow.Owner != null)
                        {
                            dialogWindow.Left = dialogWindow.Owner.Left + (dialogWindow.Owner.ActualWidth - dialogWindow.ActualWidth) / 2;
                            dialogWindow.Top = dialogWindow.Owner.Top + (dialogWindow.Owner.ActualHeight - dialogWindow.ActualHeight) / 2;
                        }
                        else
                        {
                            dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        }
                    }
                    catch
                    {
                        dialogWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    }
                };

                dialogWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi trong PopUpService:\n{ex.Message}", 
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
