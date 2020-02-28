using RemoteConnector.Properties;
using RemoteConnector.ViewModels;
using System.Windows;

namespace RemoteConnector.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();

            Left = Settings.Default.Left;
            Top = Settings.Default.Top;
            Width = Settings.Default.Width;
            Height = Settings.Default.Height;

            DataContext = new MainViewModel();
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
            w.WinSCPPathBox.Path = Settings.Default.WinSCPPath;
            w.IconBaseBox.Text = Settings.Default.IconBaseUrl;

            if (w.ShowDialog() == true)
            {
                Settings.Default.PuTTYPath = w.PuTTYPathBox.Path;
                Settings.Default.WinSCPPath = w.WinSCPPathBox.Path;
                Settings.Default.IconBaseUrl = w.IconBaseBox.Text;
                Settings.Default.Save();
            }
        }

        private void RecheckMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _ = ViewModel.Refresh();
        }

        private void ItemEditButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
