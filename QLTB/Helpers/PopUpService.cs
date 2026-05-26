using System;
using System.Windows;
using System.Windows.Controls;

namespace QLTB.Helpers
{
    public class PopUpService
    {
        // Hàm hiển thị Popup: Chấp nhận cả UIElement (UserControl, Window) hoặc ViewModel
        public static void ShowPopUp(object content)
        {
            try
            {
                Window ownerWindow = Application.Current.MainWindow;

                // Nếu content truyền vào đã là một Window, chỉ cần thiết lập Owner và hiển thị luôn
                if (content is Window existingWindow)
                {
                    existingWindow.Owner = ownerWindow;
                    if (existingWindow.WindowStartupLocation == WindowStartupLocation.CenterOwner && ownerWindow != null)
                    {
                        // Giữ nguyên logic tính toán vị trí thủ công của bạn nếu None WindowStyle làm mất CenterOwner
                        ConfigureWindowPosition(existingWindow, ownerWindow);
                    }
                    existingWindow.ShowDialog();
                    return;
                }

                // Nếu là UserControl hoặc ViewModel, bọc nó lại trong một Window mới
                Window dialogWindow = new Window
                {
                    Title = "Chi tiết",
                    SizeToContent = SizeToContent.WidthAndHeight,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Owner = ownerWindow,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    Content = content, // Gán trực tiếp vào Content (WPF tự hiểu DataTemplate nếu là VM, hoặc render nếu là UC)
                    Resources = Application.Current.Resources
                };

                // Nếu truyền vào là một UserControl, gán DataContext của Window bằng chính DataContext của UC đó
                if (content is FrameworkElement element)
                {
                    dialogWindow.DataContext = element.DataContext;
                }
                else
                {
                    // Nếu truyền vào là một ViewModel, gán DataContext bằng chính nó
                    dialogWindow.DataContext = content;
                }

                dialogWindow.Loaded += (sender, args) =>
                {
                    ConfigureWindowPosition(dialogWindow, ownerWindow);
                };

                dialogWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi trong PopUpService:\n{ex.Message}",
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm đóng Popup: Chấp nhận truyền vào Window, UserControl, hoặc thậm chí là ViewModel cũ
        public static void ClosePopUp(object target)
        {
            if (Application.Current == null || target == null) return;

            // Trường hợp 1: Nếu target truyền vào là chính cái Window
            if (target is Window targetWindow)
            {
                targetWindow.Close();
                return;
            }

            // Trường hợp 2: Nếu target truyền vào là một UserControl (hoặc bất kỳ Control nào thuộc View)
            if (target is DependencyObject depObj)
            {
                Window parentWindow = Window.GetWindow(depObj);
                if (parentWindow != null)
                {
                    parentWindow.Close();
                    return;
                }
            }

            // Trường hợp 3: Dự phòng (Fallback) nếu lỡ truyền ViewModel từ chỗ khác vào
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == target || window.Content == target)
                {
                    window.Close();
                    break;
                }
            }
        }

        // Hàm phụ trợ tính toán vị trí hiển thị giữa màn hình chính
        private static void ConfigureWindowPosition(Window dialog, Window owner)
        {
            try
            {
                if (owner != null)
                {
                    dialog.Left = owner.Left + (owner.ActualWidth - dialog.ActualWidth) / 2;
                    dialog.Top = owner.Top + (owner.ActualHeight - dialog.ActualHeight) / 2;
                }
                else
                {
                    dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            catch
            {
                dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
    }
}