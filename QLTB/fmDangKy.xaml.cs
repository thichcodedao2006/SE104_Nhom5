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
    /// Interaction logic for fmDangKy.xaml
    /// </summary>
    public partial class fmDangKy : Window
    {
        public fmDangKy()
        {
            InitializeComponent();
            this.DataContext = new SignUpVM();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
