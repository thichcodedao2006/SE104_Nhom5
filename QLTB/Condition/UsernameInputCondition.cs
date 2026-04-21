using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace QLTB.Condition
{
    public class UsernameInputCondition : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            // Bắt sự kiện gõ ký tự
            AssociatedObject.PreviewTextInput += AssociatedObject_PreviewTextInput;
            // Bắt sự kiện bấm phím đặc biệt (như Space)
            AssociatedObject.PreviewKeyDown += AssociatedObject_PreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewTextInput -= AssociatedObject_PreviewTextInput;
            AssociatedObject.PreviewKeyDown -= AssociatedObject_PreviewKeyDown;
        }

        private void AssociatedObject_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Chặn dứt điểm dấu cách (Space)
            if (e.Key == Key.Space)
            {
                e.Handled = true; // Nuốt luôn phím, không cho hiện ra
            }
        }

        private void AssociatedObject_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Regex: DANH SÁCH TRẮNG (Whitelist)
            // Chỉ cho phép: a-z, A-Z, 0-9 và các ký tự đặc biệt thông dụng
            // Lưu ý: e.Text là ký tự mà người dùng VỪA GÕ VÀO
            Regex regex = new Regex(@"^[a-zA-Z0-9!@#$%^&*()_+=\-{\}\[\]|\\:;""'<>,.?/]+$");

            // Nếu ký tự vừa gõ KHÔNG KHỚP với danh sách trên (ví dụ copy paste chữ có dấu)
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true; // Chặn lại ngay lập tức
            }
        }
    }
}
