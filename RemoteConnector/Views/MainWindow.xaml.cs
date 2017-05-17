using RemoteConnector.Properties;
using System.Windows;

namespace RemoteConnector.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Left = Settings.Default.Left;
            Top = Settings.Default.Top;
            Width = Settings.Default.Width;
            Height = Settings.Default.Height;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Settings.Default.Left = Left;
            Settings.Default.Top = Top;
            Settings.Default.Width = Width;
            Settings.Default.Height = Height;
            Settings.Default.Save();
        }

        private void SettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var w = new SettingsWindow { Owner = this };
            w.PuTTYPathBox.Path = Settings.Default.PuTTYPath;
            w.PuTTYSessionBox.Text = Settings.Default.PuTTYSession;
            w.WinSCPPathBox.Path = Settings.Default.WinSCPPath;

            if (w.ShowDialog() == true)
            {
                Settings.Default.PuTTYPath = w.PuTTYPathBox.Path;
                Settings.Default.PuTTYSession = w.PuTTYSessionBox.Text;
                Settings.Default.WinSCPPath = w.WinSCPPathBox.Path;
                Settings.Default.Save();
            }
        }

        private void ItemEditButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
