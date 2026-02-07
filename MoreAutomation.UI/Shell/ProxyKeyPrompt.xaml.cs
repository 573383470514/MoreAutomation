using System.Windows;

namespace MoreAutomation.UI.Shell
{
    public partial class ProxyKeyPrompt : Window
    {
        public bool Accepted { get; private set; }
        public string Key => KeyBox.Password ?? string.Empty;

        public ProxyKeyPrompt()
        {
            InitializeComponent();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Accepted = false;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Accepted = true;
            Close();
        }
    }
}
