using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using WinPurifyPro.Services;
using WinPurifyPro.ViewModels;

namespace WinPurifyPro
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                if (DataContext is MainViewModel vm)
                {
                    SystemTrayService.Instance.Initialize(this, vm);
                }
            };
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
                e.Handled = true;
            }
            catch { }
        }
    }
}
