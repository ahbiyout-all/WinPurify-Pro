using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Navigation;
using WinPurifyPro.Services;
using WinPurifyPro.ViewModels;

namespace WinPurifyPro
{
    public partial class MainWindow : Window
    {
        private uint _wmRestoreMsg = 0;

        public MainWindow()
        {
            InitializeComponent();

            SourceInitialized += (s, e) =>
            {
                try
                {
                    var helper = new WindowInteropHelper(this);
                    var source = HwndSource.FromHwnd(helper.Handle);
                    _wmRestoreMsg = SingleInstanceService.RegisterWindowMessage(SingleInstanceService.RestoreMessageName);
                    source?.AddHook(WndProc);
                }
                catch { }
            };

            Loaded += (s, e) =>
            {
                // Ensure window is strictly visible and brought to front
                EnsureVisibleAndForeground();

                if (DataContext is MainViewModel vm)
                {
                    SystemTrayService.Instance.Initialize(this, vm);
                }

                // Post-load watchdog to guarantee visibility after any background init tasks complete
                Dispatcher.InvokeAsync(async () =>
                {
                    await Task.Delay(200);
                    if (!IsVisible || Visibility != Visibility.Visible || WindowState == WindowState.Minimized)
                    {
                        EnsureVisibleAndForeground();
                    }
                }, System.Windows.Threading.DispatcherPriority.Loaded);
            };
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

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (_wmRestoreMsg != 0 && msg == _wmRestoreMsg)
            {
                Dispatcher.Invoke(() =>
                {
                    EnsureVisibleAndForeground();
                });
                handled = true;
            }
            return IntPtr.Zero;
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
