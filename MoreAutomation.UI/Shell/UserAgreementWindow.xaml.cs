using System.Windows;
using MoreAutomation.Infrastructure.Config;
using MoreAutomation.Contracts.Configuration;

namespace MoreAutomation.UI.Shell
{
    public partial class UserAgreementWindow : Window
    {
        public bool Accepted { get; private set; }

        public UserAgreementWindow()
        {
            InitializeComponent();
            AgreeCheck.Checked += (s, e) => BtnAccept.IsEnabled = true;
            AgreeCheck.Unchecked += (s, e) => BtnAccept.IsEnabled = false;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Accepted = false;
            Close();
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            Accepted = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 如果用户未同意就直接关闭窗口（如点击X按钮），则阻止关闭并设置为未接受
            if (!Accepted)
            {
                Accepted = false;
            }
        }
    }
}
