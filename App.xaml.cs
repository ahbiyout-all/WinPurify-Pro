using System;
using System.Threading;
using System.Windows;
using WinPurifyPro.Services;

namespace WinPurifyPro
{
    public partial class App : Application
    {
        // 프로세스 인스턴스 고유 식별자 (AppEnvironment.ProcessInstanceId)
        public string InstanceId => AppEnvironment.ProcessInstanceId;

        public App()
        {
            // 최상위 도메인 치명적 예외 핸들러 등록 및 파일 로깅
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                string msg = $"[WinPurify Pro 시작 오류]\n\n{ex?.Message}\n\n상세 정보:\n{ex?.StackTrace}";
                if (ex?.InnerException != null)
                {
                    msg += $"\n\n[내부 예외]\n{ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
                }
                try
                {
                    LoggerService.Instance.LogException("AppDomain.UnhandledException", ex ?? new Exception("Unknown"));
                }
                catch { }

                MessageBox.Show(
                    msg,
                    "WinPurify Pro 치명적 시작 오류",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            };

            // UI 스레드 미처리 예외 핸들러 등록
            DispatcherUnhandledException += (s, args) =>
            {
                string msg = $"[WinPurify Pro 런타임 예외]\n\n{args.Exception.Message}\n\n상세 정보:\n{args.Exception.StackTrace}";
                if (args.Exception.InnerException != null)
                {
                    msg += $"\n\n[내부 예외]\n{args.Exception.InnerException.Message}\n{args.Exception.InnerException.StackTrace}";
                }
                try
                {
                    LoggerService.Instance.LogException("DispatcherUnhandledException", args.Exception);
                }
                catch { }

                MessageBox.Show(
                    msg,
                    "WinPurify Pro 런타임 예외",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                args.Handled = true;
            };
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            // 1. Windows Service 구동 모드 (--service 인수) 처리
            if (AppEnvironment.IsServiceMode)
            {
                // GUI 창을 띄우지 않고 네이티브 윈도우 서비스(WinPurifyAgentService) 백그라운드 수신 루프 실행
                WindowsServiceHost.RunAsService();
                Shutdown();
                return;
            }

            // 1-1. 서비스 수동 설치/삭제 CLI 인수 처리 (--install-service / --uninstall-service)
            string[] cmdArgs = Environment.GetCommandLineArgs();
            foreach (var arg in cmdArgs)
            {
                if (string.Equals(arg, "--install-service", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "-install-service", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "/install-service", StringComparison.OrdinalIgnoreCase))
                {
                    bool ok = WindowsServiceHost.InstallService();
                    Shutdown(ok ? 0 : 1);
                    return;
                }

                if (string.Equals(arg, "--uninstall-service", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "-uninstall-service", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "/uninstall-service", StringComparison.OrdinalIgnoreCase))
                {
                    bool ok = WindowsServiceHost.UninstallService();
                    Shutdown(ok ? 0 : 1);
                    return;
                }
            }

            // 1-2. 무음 자동 스케줄러 실행 CLI 인수 처리 (--schedule-run, --silent)
            bool isScheduleRun = false;
            foreach (var arg in cmdArgs)
            {
                if (string.Equals(arg, "--schedule-run", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "--silent", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "/silent", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arg, "--automated-run", StringComparison.OrdinalIgnoreCase))
                {
                    isScheduleRun = true;
                    break;
                }
            }

            if (isScheduleRun)
            {
                // GUI 창 표시 없이 백그라운드에서 지정된 스케줄 최적화 모듈 일괄 실행 후 종료
                int exitCode = AutoSchedulerService.ExecuteHeadlessMaintenanceAsync(cmdArgs).GetAwaiter().GetResult();
                Shutdown(exitCode);
                return;
            }

            // 2. 중앙 관제 콘솔 전용 구동 모드 (--commander, -commander, /commander, --fleet 인수) 처리
            // (글로벌 Mutex 제거: 독립 프로세스 고유 식별자 기반 동시 실행 전면 지원)
            if (AppEnvironment.IsCommanderMode)
            {
                base.OnStartup(e);

                // C++ 네이티브 엔진(PurifyEngineCore.dll) 및 커널 백그라운드 프로세스 비동기 예열
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        NativeEngineService.IsNativeCoreAvailable();
                    }
                    catch { }
                });

                // Central Commander 방화벽 예외 규칙 자동 등록
                FirewallService.EnsureCommanderFirewallRulesAsync();

                try
                {
                    var commanderWindow = new CommanderMainWindow();
                    MainWindow = commanderWindow;
                    commanderWindow.Visibility = Visibility.Visible;
                    if (commanderWindow.WindowState == WindowState.Minimized)
                    {
                        commanderWindow.WindowState = WindowState.Normal;
                    }
                    commanderWindow.Show();
                    commanderWindow.Activate();
                    commanderWindow.Focus();
                    commanderWindow.Topmost = true;
                    commanderWindow.Topmost = false;

                    // 백그라운드 엔진 로딩 완료 후 커맨더 창 화면 표시 상태(Visible) 재확인 워치독
                    commanderWindow.Dispatcher.InvokeAsync(async () =>
                    {
                        await System.Threading.Tasks.Task.Delay(250);
                        if (!commanderWindow.IsVisible || commanderWindow.Visibility != Visibility.Visible || commanderWindow.WindowState == WindowState.Minimized)
                        {
                            commanderWindow.Visibility = Visibility.Visible;
                            commanderWindow.WindowState = WindowState.Normal;
                            commanderWindow.Show();
                            commanderWindow.Activate();
                            commanderWindow.Focus();
                        }
                    }, System.Windows.Threading.DispatcherPriority.Loaded);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"[Central Commander 실행 오류]\n\n{ex.Message}\n\n상세 정보:\n{ex.StackTrace}",
                        "WinPurify Pro Central Commander 오류",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Shutdown(-1);
                }
                return;
            }

            // 3. 일반 데스크탑 GUI 애플리케이션 시작
            // (글로벌 Mutex 제거: 독립 프로세스 고유 식별자 기반 다중 인스턴스 및 커맨더 동시 실행 전면 허용)
            base.OnStartup(e);

            // C++ 네이티브 엔진(PurifyEngineCore.dll) 및 커널 백그라운드 프로세스 비동기 예열 (UI 스레드 지연 0초)
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    NativeEngineService.IsNativeCoreAvailable();
                }
                catch { }
            });

            // Windows 방화벽 예외 규칙 자동 등록 (버전 명시형 및 범용 규칙)
            FirewallService.EnsureAppFirewallRulesAsync();

            // 원격 에이전트 윈도우 서비스(WinPurifyAgentService) 등록 상태 자가 점검 및 복구
            WindowsServiceHost.EnsureServiceRegisteredAsync();

            // 메인 윈도우 생성 및 화면 전면 표시(Visible) 절대 보장
            try
            {
                var mainWindow = new MainWindow();
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

                // 백그라운드 엔진 로딩 완료 후 메인 창 화면 표시 상태(Visible) 재확인 워치독
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
                MessageBox.Show(
                    $"[WinPurify Pro 메인 창 초기화 오류]\n\n{ex.Message}\n\n상세 정보:\n{ex.StackTrace}" +
                    (ex.InnerException != null ? $"\n\n[내부 예외]\n{ex.InnerException.Message}" : ""),
                    "WinPurify Pro 시작 오류",
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
