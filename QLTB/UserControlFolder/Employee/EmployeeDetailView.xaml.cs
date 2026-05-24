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
using QLTB.ViewModel;

namespace QLTB.UserControlFolder.Employee
{
    /// <summary>
    /// Interaction logic for EmployeeDetailView.xaml
    /// </summary>
    public partial class EmployeeDetailView : UserControl
    {
        public EmployeeDetailView(ViewModel.Employee e)
        {
            InitializeComponent();
            this.DataContext = new EmployeeDetailVM(e);
        }
    }
}
