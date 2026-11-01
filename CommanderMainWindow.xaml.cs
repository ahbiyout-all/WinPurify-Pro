using System.Windows;
using System.Windows.Input;
using WinPurifyPro.ViewModels;

namespace WinPurifyPro
{
    public partial class CommanderMainWindow : Window
    {
        private uint _wmCommanderRestoreMsg = 0;

        public CommanderMainWindow()
        {
            try
            {
                InitializeComponent();

                SourceInitialized += (s, e) =>
                {
                    try
                    {
                        var helper = new System.Windows.Interop.WindowInteropHelper(this);
                        var source = System.Windows.Interop.HwndSource.FromHwnd(helper.Handle);
                        _wmCommanderRestoreMsg = WinPurifyPro.Services.SingleInstanceService.RegisterWindowMessage(WinPurifyPro.Services.SingleInstanceService.CommanderRestoreMessageName);
                        source?.AddHook(WndProc);
                    }
                    catch { }
                };

                Loaded += (s, e) =>
                {
                    EnsureVisibleAndForeground();
                };
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

        public void EnsureVisibleAndForeground()
        {
            try
            {
                Visibility = Visibility.Visible;
                if (WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }
                Show();
                Activate();
                Focus();
                Topmost = true;
                Topmost = false;
            }
            catch { }
        }

        private System.IntPtr WndProc(System.IntPtr hwnd, int msg, System.IntPtr wParam, System.IntPtr lParam, ref bool handled)
        {
            if (_wmCommanderRestoreMsg != 0 && msg == _wmCommanderRestoreMsg)
            {
                Dispatcher.Invoke(() =>
                {
                    EnsureVisibleAndForeground();
                });
                handled = true;
            }
            return System.IntPtr.Zero;
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
