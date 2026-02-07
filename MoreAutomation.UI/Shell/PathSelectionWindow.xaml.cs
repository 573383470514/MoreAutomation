using System.Windows;
using Microsoft.Win32;

namespace MoreAutomation.UI.Shell
{
    public partial class PathSelectionWindow : Window
    {
        public string? SelectedPath { get; private set; }

        public PathSelectionWindow(string? initial)
        {
            InitializeComponent();
            PathBox.Text = initial ?? string.Empty;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                PathBox.Text = dlg.FileName;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            SelectedPath = null;
            Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            SelectedPath = PathBox.Text?.Trim();
            Close();
        }
    }
}
