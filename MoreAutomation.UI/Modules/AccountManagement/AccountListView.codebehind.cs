using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MoreAutomation.UI.Modules.AccountManagement
{
    public partial class AccountListView : UserControl
    {
        public AccountListView()
        {
            InitializeComponent();
        }

        private void DataGridRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row != null)
            {
                row.IsSelected = true;
            }
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row != null && row.Item is Domain.Entities.Account acc)
            {
                if (DataContext is AccountViewModel vm)
                {
                    vm.LoginAccountCommand.Execute(acc);
                }
            }
        }

        private void ConfigureProxy_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is AccountViewModel vm && sender is MenuItem mi)
            {
                if (mi.DataContext is Domain.Entities.Account acc)
                {
                    vm.ConfigureProxyCommand.Execute(acc);
                }
            }
        }

        private void NewPwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AccountViewModel vm && sender is PasswordBox pb)
            {
                vm.NewPassword = pb.Password;
            }
        }
    }
}
