using System.Windows;
using System.Windows.Input;
using WinPurifyPro.ViewModels;

namespace WinPurifyPro
{
    public partial class CommanderMainWindow : Window
    {
        public CommanderMainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (System.Exception ex)
            {
                string msg = $"[CommanderMainWindow XAML 초기화 실패]\n\n{ex.Message}\n\n상세 정보:\n{ex.StackTrace}";
                if (ex.InnerException != null)
                {
                    msg += $"\n\n[내부 예외]\n{ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
                }
                MessageBox.Show(msg, "WinPurify Commander UI 로드 오류", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddCustomIp_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is CommanderViewModel vm)
            {
                string ip = TxtCustomIp.Text;
                vm.AddCustomNode(ip);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                e.Handled = true;
            }
            catch { }
        }
    }
}
