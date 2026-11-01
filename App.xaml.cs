using System;
using System.Threading;
using System.Windows;
using WinPurifyPro.Services;

namespace WinPurifyPro
{
    public partial class App : Application
    {
        private static Mutex? _appMutex;

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
            if (AppEnvironment.IsCommanderMode)
            {
                // Central Commander 전용 글로벌 뮤텍스 획득 (커맨더 자체 중복 실행 방지)
                try
                {
                    _appMutex = new Mutex(true, SingleInstanceService.CommanderMutexName, out bool createdCommander);
                    if (!createdCommander)
                    {
                        // 커맨더가 이미 실행 중이면 기존 커맨더 창을 활성화 시도
                        bool activated = SingleInstanceService.ActivateCommanderWindow();
                        if (activated)
                        {
                            Shutdown(0);
                            return;
                        }
                        // 기존 창이 없거나 활성화 실패한 경우 계속 진행하여 새 창 표시
                    }
                }
                catch (AbandonedMutexException)
                {
                    // 이전 프로세스 비정상 종료 시 소유권 획득 후 계속 진행
                }
                catch { }

                base.OnStartup(e);
                try
                {
                    var commanderWindow = new CommanderMainWindow();
                    MainWindow = commanderWindow;
                    if (commanderWindow.WindowState == WindowState.Minimized)
                    {
                        commanderWindow.WindowState = WindowState.Normal;
                    }
                    commanderWindow.Show();
                    commanderWindow.Activate();
                    commanderWindow.Focus();
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

            // 3. 일반 데스크탑 GUI 애플리케이션 시작 (커맨더와 동시 실행 완벽 지원)
            // WinPurify Pro 단일 인스턴스 글로벌 뮤텍스 획득 (메인 앱 자체 중복 실행 방지)
            try
            {
                _appMutex = new Mutex(true, SingleInstanceService.AppMutexName, out bool createdNew);
                if (!createdNew)
                {
                    // 메인 앱이 이미 실행 중인 경우 기존 메인 창을 화면 전면으로 활성화
                    bool activated = SingleInstanceService.ActivateAppWindow();
                    if (activated)
                    {
                        // 기존 GUI 창이 성공적으로 활성화된 경우에만 현재 중복 인스턴스 종료
                        Shutdown(0);
                        return;
                    }
                    // 백그라운드 서비스나 고스트 프로세스로 인해 기존 창 활성화가 실패한 경우,
                    // 현재 GUI 창을 정상 생성하여 화면에 표시하도록 계속 진행!
                }
            }
            catch (AbandonedMutexException)
            {
                // 이전 프로세스 비정상 종료 시 소유권 획득 후 계속 진행
            }
            catch
            {
                // 권한 등으로 인해 Global 뮤텍스 생성 실패 시 로컬 뮤텍스 폴백
                try
                {
                    _appMutex = new Mutex(true, "WinPurifyPro_App_Mutex_Local_6904B914", out bool localCreated);
                    if (!localCreated)
                    {
                        bool activated = SingleInstanceService.ActivateAppWindow();
                        if (activated)
                        {
                            Shutdown(0);
                            return;
                        }
                    }
                }
                catch { }
            }

            base.OnStartup(e);

            // Windows 방화벽 예외 규칙 자동 등록 (버전 명시형 및 범용 규칙)
            FirewallService.EnsureAppFirewallRulesAsync();

            // 원격 에이전트 윈도우 서비스(WinPurifyAgentService) 등록 상태 자가 점검 및 복구
            WindowsServiceHost.EnsureServiceRegisteredAsync();

            // 메인 윈도우 생성 및 화면 전면 표시 보장
            try
            {
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                if (mainWindow.WindowState == WindowState.Minimized)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }
                mainWindow.Show();
                mainWindow.Activate();
                mainWindow.Focus();
                mainWindow.Topmost = true;
                mainWindow.Topmost = false;
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
            try
            {
                _appMutex?.ReleaseMutex();
                _appMutex?.Dispose();
            }
            catch { }

            base.OnExit(e);
        }
    }
}
