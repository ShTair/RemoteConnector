using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace RemoteConnector.Views
{
    /// <summary>
    /// CommonOpenFileControl.xaml の相互作用ロジック
    /// </summary>
    public partial class CommonOpenFileControl : UserControl
    {
        public CommonOpenFileControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(CommonOpenFileControl), new FrameworkPropertyMetadata { BindsTwoWayByDefault = true });

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public bool IsFolderPicker { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var w = new CommonOpenFileDialog { IsFolderPicker = IsFolderPicker };

            try
            {
                w.DefaultFileName = System.IO.Path.GetFileName(Path);
                w.DefaultDirectory = System.IO.Path.GetDirectoryName(Path);
            }
            catch { }

            if (w.ShowDialog() != CommonFileDialogResult.Ok) return;

            Path = w.FileName;
        }
    }
}
