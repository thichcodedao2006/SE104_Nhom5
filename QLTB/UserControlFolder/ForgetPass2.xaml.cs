using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QLTB.UserControlFolder
{
    /// <summary>
    /// Interaction logic for ForgetPass2.xaml
    /// </summary>
    public partial class ForgetPass2 : UserControl
    {
        public ForgetPass2()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb != null && tb.Text.Length == tb.MaxLength)
            {
                // Cho hệ thống trễ lại 1 nhịp siêu nhỏ (mắt thường không thấy) 
                // để ViewModel kịp mở khóa ô tiếp theo
                tb.Dispatcher.BeginInvoke(new Action(() =>
                {
                    tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }
    }
}
