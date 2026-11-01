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

        private static System.Threading.Mutex? _commanderMutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Central Commander 전용 글로벌 뮤텍스 획득 (커맨더 자체 중복 실행 방지, WinPurify Pro와 동시 실행 완벽 지원)
            try
            {
                _commanderMutex = new System.Threading.Mutex(true, WinPurifyPro.Services.SingleInstanceService.CommanderMutexName, out bool createdNew);
                if (!createdNew)
                {
                    // 커맨더가 이미 실행 중이면 기존 커맨더 창을 활성화하고 종료
                    WinPurifyPro.Services.SingleInstanceService.ActivateCommanderWindow();
                    Shutdown(0);
                    return;
                }
            }
            catch { }

            base.OnStartup(e);

            // Central Commander 방화벽 예외 규칙 자동 등록 (버전 명시형 및 범용 규칙)
            WinPurifyPro.Services.FirewallService.EnsureCommanderFirewallRulesAsync();

            try
            {
                var mainWindow = new CommanderMainWindow();
                MainWindow = mainWindow;
                mainWindow.Show();
                mainWindow.Activate();
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
            try
            {
                _commanderMutex?.ReleaseMutex();
                _commanderMutex?.Dispose();
            }
            catch { }

            base.OnExit(e);
        }
    }
}
