using System.Windows;
using MoreAutomation.Domain.Entities;

namespace MoreAutomation.UI.Shell
{
    public partial class ConfigureProxyWindow : Window
    {
        private readonly Account _account;
        public bool Saved { get; private set; }

        public ConfigureProxyWindow(Account account)
        {
            InitializeComponent();
            _account = account;
            PortBox.Text = account.ProxyPort.ToString();
            ModeBox.SelectedIndex = 0;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Saved = false;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(PortBox.Text?.Trim(), out int port))
            {
                MessageBox.Show("请输入有效端口");
                return;
            }

            _account.ProxyPort = port;
            Saved = true;
            Close();
        }
    }
}
