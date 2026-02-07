using System.Windows.Controls;

namespace MoreAutomation.UI.Modules.DailyTasks
{
    public partial class DailyTasksView : UserControl
    {
        public DailyTasksView()
        {
            InitializeComponent();
            DataContext = new DailyTasksViewModel();
        }
    }
}
