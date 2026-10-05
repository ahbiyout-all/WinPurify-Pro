using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// Windows SCM(Service Control Manager)과 통신하는 네이티브 윈도우 서비스 호스트
    /// 인스톨러로 데스크탑에 설치 시 시스템 서비스로 상시 구동되어 원격 커맨더 명령을 수신합니다.
    /// </summary>
    public static class WindowsServiceHost
    {
        #region Win32 SCM Native Interop

        [StructLayout(LayoutKind.Sequential)]
        private struct SERVICE_TABLE_ENTRY
        {
            public IntPtr lpServiceName;
            public ServiceMainDelegate lpServiceProc;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SERVICE_STATUS
        {
            public int dwServiceType;
            public int dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        }

        private const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
        private const int SERVICE_STOPPED = 0x00000001;
        private const int SERVICE_START_PENDING = 0x00000002;
        private const int SERVICE_STOP_PENDING = 0x00000003;
        private const int SERVICE_RUNNING = 0x00000004;

        private const int SERVICE_ACCEPT_STOP = 0x00000001;
        private const int SERVICE_ACCEPT_SHUTDOWN = 0x00000004;

        private const int SERVICE_CONTROL_STOP = 0x00000001;
        private const int SERVICE_CONTROL_SHUTDOWN = 0x00000005;

        private delegate void ServiceMainDelegate(int argc, IntPtr argv);
        private delegate int ServiceControlHandlerExDelegate(int control, int eventType, IntPtr eventData, IntPtr context);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool StartServiceCtrlDispatcher([In] SERVICE_TABLE_ENTRY[] lpServiceTable);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr RegisterServiceCtrlHandlerEx(
            string lpServiceName,
            ServiceControlHandlerExDelegate lpHandlerProc,
            IntPtr lpContext);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr hServiceStatus, ref SERVICE_STATUS lpServiceStatus);

        #endregion

        private static IntPtr _statusHandle = IntPtr.Zero;
        private static SERVICE_STATUS _serviceStatus;
        private static ManualResetEventSlim? _stopEvent;
        private static ServiceMainDelegate? _serviceMainDelegate;
        private static ServiceControlHandlerExDelegate? _controlHandlerDelegate;

        /// <summary>
        /// Windows Service로서 서비스 루프 진입
        /// </summary>
        public static void RunAsService()
        {
            _stopEvent = new ManualResetEventSlim(false);
            _serviceMainDelegate = ServiceMain;

            var serviceTable = new SERVICE_TABLE_ENTRY[2];
            serviceTable[0].lpServiceName = Marshal.StringToHGlobalUni(AppEnvironment.ServiceName);
            serviceTable[0].lpServiceProc = _serviceMainDelegate;
            serviceTable[1].lpServiceName = IntPtr.Zero;
            serviceTable[1].lpServiceProc = null!;

            bool dispatcherStarted = false;
            try
            {
                // SCM에 서비스 디스패처 등록
                dispatcherStarted = StartServiceCtrlDispatcher(serviceTable);
            }
            catch
            {
                dispatcherStarted = false;
            }
            finally
            {
                if (serviceTable[0].lpServiceName != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(serviceTable[0].lpServiceName);
                }
            }

            // SCM 디스패처가 실패한 경우 (예: 관리자 콘솔에서 직접 --service 실행한 경우)
            if (!dispatcherStarted)
            {
                RunAsInteractiveDaemon();
            }
        }

        private static void ServiceMain(int argc, IntPtr argv)
        {
            try
            {
                // Windows Service는 기본 작업 디렉토리가 C:\Windows\System32이므로 앱 설치 디렉토리로 전환
                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            }
            catch { }

            _controlHandlerDelegate = ServiceControlHandler;
            _statusHandle = RegisterServiceCtrlHandlerEx(AppEnvironment.ServiceName, _controlHandlerDelegate, IntPtr.Zero);

            if (_statusHandle == IntPtr.Zero)
            {
                return;
            }

            // 1. SERVICE_START_PENDING 보고
            UpdateStatus(SERVICE_START_PENDING, 5000);

            try
            {
                // 2. RemoteAgentService 에이전트 시작 (TCP 9870 / UDP 9871)
                RemoteAgentService.Instance.StartAgent(
                    RemoteAgentService.DefaultHttpPort,
                    RemoteAgentService.DefaultDiscoveryPort);

                // 3. SERVICE_RUNNING 보고
                UpdateStatus(SERVICE_RUNNING, 0, SERVICE_ACCEPT_STOP | SERVICE_ACCEPT_SHUTDOWN);
            }
            catch (Exception ex)
            {
                LogServiceError("에이전트 서비스 시작 실패: " + ex.Message);
                UpdateStatus(SERVICE_STOPPED, 0);
                return;
            }

            // 4. 서비스 중지 신호 대기
            _stopEvent?.Wait();

            // 5. 종료 처리
            UpdateStatus(SERVICE_STOP_PENDING, 3000);
            try
            {
                RemoteAgentService.Instance.StopAgent();
            }
            catch { }

            UpdateStatus(SERVICE_STOPPED, 0);
        }

        private static int ServiceControlHandler(int control, int eventType, IntPtr eventData, IntPtr context)
        {
            switch (control)
            {
                case SERVICE_CONTROL_STOP:
                case SERVICE_CONTROL_SHUTDOWN:
                    UpdateStatus(SERVICE_STOP_PENDING, 3000);
                    _stopEvent?.Set();
                    return 0; // NO_ERROR
                default:
                    return 0;
            }
        }

        private static void UpdateStatus(int currentState, int waitHint, int controlsAccepted = 0)
        {
            if (_statusHandle == IntPtr.Zero) return;

            _serviceStatus.dwServiceType = SERVICE_WIN32_OWN_PROCESS;
            _serviceStatus.dwCurrentState = currentState;
            _serviceStatus.dwControlsAccepted = controlsAccepted;
            _serviceStatus.dwWin32ExitCode = 0;
            _serviceStatus.dwServiceSpecificExitCode = 0;
            _serviceStatus.dwCheckPoint = 0;
            _serviceStatus.dwWaitHint = waitHint;

            SetServiceStatus(_statusHandle, ref _serviceStatus);
        }

        /// <summary>
        /// SCM 없이 대화형 데몬 모드로 실행할 때의 폴백 루프
        /// </summary>
        private static void RunAsInteractiveDaemon()
        {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                RemoteAgentService.Instance.StartAgent();
                // 종료 시까지 대기
                while (!cts.IsCancellationRequested)
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                LogServiceError("대화형 데몬 실행 오류: " + ex.Message);
            }
            finally
            {
                RemoteAgentService.Instance.StopAgent();
            }
        }

        private static void LogServiceError(string message)
        {
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                Directory.CreateDirectory(logDir);
                string logFile = Path.Combine(logDir, "agent_service.log");
                File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
            catch { }
        }

        /// <summary>
        /// 서비스 등록 여부 확인
        /// </summary>
        public static bool IsServiceInstalled()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{AppEnvironment.ServiceName}");
                return key != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 백그라운드 비동기로 서비스 등록 및 상태 점검 (미등록 시 자동 등록 시도)
        /// </summary>
        public static Task EnsureServiceRegisteredAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    if (!IsServiceInstalled())
                    {
                        string currentExe = Process.GetCurrentProcess().MainModule?.FileName 
                                            ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WinPurifyPro.exe");
                        if (File.Exists(currentExe))
                        {
                            InstallService(currentExe);
                        }
                    }
                }
                catch { }
            });
        }

        /// <summary>
        /// sc.exe를 통한 윈도우 서비스 설치 및 시작
        /// </summary>
        public static bool InstallService(string? exePath = null)
        {
            try
            {
                exePath ??= Process.GetCurrentProcess().MainModule?.FileName 
                            ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WinPurifyPro.exe");

                if (!File.Exists(exePath)) return false;

                // 기존 잔여 서비스 삭제
                UninstallService();
                Thread.Sleep(300);

                string binPath = $"\\\"{exePath}\\\" --service";
                string createArgs = $"create {AppEnvironment.ServiceName} binPath= \"{binPath}\" start= auto DisplayName= \"{AppEnvironment.ServiceDisplayName}\"";
                var psi = new ProcessStartInfo("sc.exe", createArgs)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var p = Process.Start(psi);
                p?.WaitForExit(5000);

                // 설명 설정
                string descArgs = $"description {AppEnvironment.ServiceName} \"{AppEnvironment.ServiceDescription}\"";
                var pDesc = Process.Start(new ProcessStartInfo("sc.exe", descArgs) { UseShellExecute = false, CreateNoWindow = true });
                pDesc?.WaitForExit(3000);

                // 서비스 시작
                var pStart = Process.Start(new ProcessStartInfo("sc.exe", $"start {AppEnvironment.ServiceName}") { UseShellExecute = false, CreateNoWindow = true });
                pStart?.WaitForExit(5000);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// sc.exe를 통한 윈도우 서비스 삭제
        /// </summary>
        public static bool UninstallService()
        {
            try
            {
                var pStop = Process.Start(new ProcessStartInfo("sc.exe", $"stop {AppEnvironment.ServiceName}") { UseShellExecute = false, CreateNoWindow = true });
                pStop?.WaitForExit(5000);

                var pDel = Process.Start(new ProcessStartInfo("sc.exe", $"delete {AppEnvironment.ServiceName}") { UseShellExecute = false, CreateNoWindow = true });
                pDel?.WaitForExit(5000);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
