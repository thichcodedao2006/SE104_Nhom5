using QLTB.ViewModel;
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

namespace QLTB.UserControlFolder.Maintenance
{
    /// <summary>
    /// Interaction logic for MaintenancePlanDetailView.xaml
    /// </summary>
    public partial class MaintenancePlanDetailView : UserControl
    {
        public MaintenancePlanDetailView(MaintenancePlanItem item)
        {
            InitializeComponent();
            DataContext = new MaintenancePlanDetailViewModel(item);
        }
    }
}
