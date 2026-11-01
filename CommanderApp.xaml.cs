using System;
using System.Windows;
using System.Windows.Threading;

namespace WinPurifyPro
{
    public partial class CommanderApp : Application
    {
        public CommanderApp()
        {
            // 최상위 도메인 치명적 예외 핸들러 등록
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                string msg = $"[WinPurify Commander 시작 오류]\n\n{ex?.Message}\n\n상세 정보:\n{ex?.StackTrace}";
                if (ex?.InnerException != null)
                {
                    msg += $"\n\n[내부 예외]\n{ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
                }
                MessageBox.Show(
                    msg,
                    "WinPurify Pro Central Commander 치명적 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            };

            // UI 스레드 미처리 예외 핸들러 등록
            DispatcherUnhandledException += (s, args) =>
            {
                string msg = $"[WinPurify Commander 런타임 예외]\n\n{args.Exception.Message}\n\n상세 정보:\n{args.Exception.StackTrace}";
                if (args.Exception.InnerException != null)
                {
                    msg += $"\n\n[내부 예외]\n{args.Exception.InnerException.Message}\n{args.Exception.InnerException.StackTrace}";
                }
                MessageBox.Show(
                    msg,
                    "WinPurify Pro Central Commander 예외",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                args.Handled = true;
            };
        }

        // Central Commander 인스턴스 고유 식별자
        public string InstanceId => WinPurifyPro.Services.AppEnvironment.ProcessInstanceId;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Native Engine background warmup
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    WinPurifyPro.Services.NativeEngineService.IsNativeCoreAvailable();
                }
                catch { }
            });

            // Central Commander 방화벽 예외 규칙 자동 등록 (버전 명시형 및 범용 규칙)
            WinPurifyPro.Services.FirewallService.EnsureCommanderFirewallRulesAsync();

            try
            {
                var mainWindow = new CommanderMainWindow();
                MainWindow = mainWindow;
                mainWindow.Visibility = Visibility.Visible;
                if (mainWindow.WindowState == WindowState.Minimized)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }
                mainWindow.Show();
                mainWindow.Activate();
                mainWindow.Focus();
                mainWindow.Topmost = true;
                mainWindow.Topmost = false;

                // 백그라운드 엔진 로딩 완료 후 커맨더 창 화면 표시 상태(Visible) 재확인 워치독
                mainWindow.Dispatcher.InvokeAsync(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(250);
                    if (!mainWindow.IsVisible || mainWindow.Visibility != Visibility.Visible || mainWindow.WindowState == WindowState.Minimized)
                    {
                        mainWindow.Visibility = Visibility.Visible;
                        mainWindow.WindowState = WindowState.Normal;
                        mainWindow.Show();
                        mainWindow.Activate();
                        mainWindow.Focus();
                    }
                }, System.Windows.Threading.DispatcherPriority.Loaded);
            }
            catch (Exception ex)
            {
                string msg = $"[WinPurify Commander 창 생성 실패]\n\n{ex.Message}\n\n상세 정보:\n{ex.StackTrace}";
                if (ex.InnerException != null)
                {
                    msg += $"\n\n[내부 예외]\n{ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
                }
                MessageBox.Show(
                    msg,
                    "WinPurify Pro Central Commander 초기화 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(-1);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
