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
using System.Windows.Shapes;
using QLTB.ViewModel;

namespace QLTB
{
    /// <summary>
    /// Interaction logic for fmQuenMatKhau.xaml
    /// </summary>
    public partial class fmQuenMatKhau : Window
    {
        public fmQuenMatKhau()
        {
            InitializeComponent();
            this.DataContext = new ForgetPassVM();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
