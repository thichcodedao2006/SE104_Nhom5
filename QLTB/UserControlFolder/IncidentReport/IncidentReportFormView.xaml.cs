
using System.Windows;
using System.Windows.Controls;

using QLTB.ViewModel;

namespace QLTB.UserControlFolder.IncidentReport
{
    /// <summary>
    /// Interaction logic for IncidentReportFormView.xaml
    /// </summary>
    public partial class IncidentReportFormView : UserControl
    {
        public IncidentReportFormView()
        {
            InitializeComponent();
            this.DataContext = new IncidentReportFormVM();
        }
    }
}
