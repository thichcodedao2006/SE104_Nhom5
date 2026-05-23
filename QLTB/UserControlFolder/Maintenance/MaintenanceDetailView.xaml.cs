using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QLTB.UserControlFolder.Maintenance
{
    public partial class MaintenanceDetailView : UserControl
    {
        public MaintenanceDetailView()
        {
            InitializeComponent(); 

            this.MouseDown += (s, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    Window.GetWindow(this)?.DragMove();
                }
            };
        }
    }
}