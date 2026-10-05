using System;
using System.Threading;
using System.Windows;
using WinPurifyPro.Services;

namespace WinPurifyPro
{
    public partial class App : Application
    {
        private static Mutex? _appMutex;

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
                base.OnStartup(e);
                try
                {
                    var commanderWindow = new CommanderMainWindow();
                    MainWindow = commanderWindow;
                    commanderWindow.Show();
                    commanderWindow.Activate();
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
            const string mutexName = @"Global\WinPurifyPro_App_Mutex_Global_6904B914";
            try
            {
                _appMutex = new Mutex(true, mutexName, out bool createdNew);
                if (!createdNew)
                {
                    // 이미 실행 중인 경우 다른 인스턴스가 존재함
                    // Inno Setup의 AppMutex가 이를 감지하여 안전하게 설치/업데이트/언인스톨 처리 가능
                }
            }
            catch
            {
                // 권한 등으로 인해 Global 뮤텍스 생성 실패 시 로컬 뮤텍스 폴백
                try
                {
                    _appMutex = new Mutex(true, "WinPurifyPro_App_Mutex_Global_6904B914");
                }
                catch { }
            }

            base.OnStartup(e);

            // Windows 방화벽 예외 규칙 자동 등록 (버전 명시형 및 범용 규칙)
            FirewallService.EnsureAppFirewallRulesAsync();

            // 원격 에이전트 윈도우 서비스(WinPurifyAgentService) 등록 상태 자가 점검 및 복구
            WindowsServiceHost.EnsureServiceRegisteredAsync();

            // 메인 윈도우 표시
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
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
