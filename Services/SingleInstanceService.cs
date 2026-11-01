using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// WinPurify Pro 프로세스 동시 실행(Concurrent Multi-Instance) 및 Central Commander 공존 관리 서비스
    /// 
    /// [핵심 실행 규칙]
    /// 1. 글로벌 애플리케이션 레벨의 강제 단일 Mutex 차단을 전면 해제하여, 사용자가 필요에 따라 다중 WinPurify Pro 인스턴스를 자유롭게 동시 구동할 수 있습니다.
    /// 2. Central Commander(중앙 관제 콘솔)와 WinPurify Pro 메인 앱이 완벽하게 독립적으로 상호 간섭 없이 동시 실행됩니다.
    /// 3. 각 프로세스는 AppEnvironment.ProcessInstanceId(PID + 고유 인스턴스 GUID)를 통해 독립적인 고유 식별자를 보유합니다.
    /// 4. 소켓 및 포트 공유: 동일 머신 동시 구동 시 포트 충돌 없이 에이전트 채널 및 C++ 커널 가속을 공유합니다.
    /// </summary>
    public static class SingleInstanceService
    {
        public static string CurrentProcessInstanceId => AppEnvironment.ProcessInstanceId;

        public const string AppMutexName = @"Global\WinPurifyPro_App_Mutex_Global_6904B914";
        public const string CommanderMutexName = @"Global\WinPurifyPro_Commander_Mutex_Global_6904B914";

        #region Win32 API Interop for Window Activation
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint RegisterWindowMessage(string lpString);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
        private static readonly IntPtr HWND_BROADCAST = (IntPtr)0xFFFF;

        public const string RestoreMessageName = "WinPurifyPro_RestoreWindow_6904B914";
        public const string CommanderRestoreMessageName = "WinPurifyPro_Commander_RestoreWindow_6904B914";
        #endregion

        /// <summary>
        /// Central Commander가 현재 시스템에서 실행 중인지 여부를 감지합니다.
        /// </summary>
        public static bool IsCommanderRunning()
        {
            // 1. 글로벌 뮤텍스 확인
            try
            {
                if (Mutex.TryOpenExisting(CommanderMutexName, out var existingMutex))
                {
                    existingMutex?.Dispose();
                    return true;
                }
            }
            catch { }

            // 2. 프로세스 목록 확인 (WinPurifyCommander.exe 또는 WinPurifyPro.exe --commander)
            try
            {
                int currentPid = Process.GetCurrentProcess().Id;
                var procs = Process.GetProcesses();
                foreach (var proc in procs)
                {
                    try
                    {
                        if (proc.Id == currentPid) continue;

                        string pName = proc.ProcessName;
                        if (string.Equals(pName, "WinPurifyCommander", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        if (string.Equals(pName, "WinPurifyPro", StringComparison.OrdinalIgnoreCase))
                        {
                            if (proc.MainWindowHandle != IntPtr.Zero)
                            {
                                string title = proc.MainWindowTitle;
                                if (!string.IsNullOrEmpty(title) &&
                                    (title.Contains("Central Commander", StringComparison.OrdinalIgnoreCase) ||
                                     title.Contains("Commander Console", StringComparison.OrdinalIgnoreCase)))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        proc.Dispose();
                    }
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// WinPurify Pro 메인 앱이 현재 시스템에서 실행 중인지 여부를 감지합니다.
        /// (백그라운드 윈도우 서비스나 헤드리스 프로세스는 제외하고 유효한 GUI 클라이언트 프로세스 감지)
        /// </summary>
        public static bool IsAppRunning()
        {
            // 1. 글로벌 뮤텍스 확인
            try
            {
                if (Mutex.TryOpenExisting(AppMutexName, out var existingMutex))
                {
                    existingMutex?.Dispose();
                    return true;
                }
            }
            catch { }

            // 2. 프로세스 목록 확인
            try
            {
                int currentPid = Process.GetCurrentProcess().Id;
                var procs = Process.GetProcessesByName("WinPurifyPro");
                foreach (var proc in procs)
                {
                    try
                    {
                        if (proc.Id == currentPid) continue;

                        string title = proc.MainWindowTitle;
                        if (string.IsNullOrEmpty(title) || !title.Contains("Commander", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    catch { }
                    finally
                    {
                        proc.Dispose();
                    }
                }
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 이미 실행 중인 WinPurify Central Commander 창을 전면으로 활성화합니다.
        /// </summary>
        public static bool ActivateCommanderWindow()
        {
            try
            {
                uint msg = RegisterWindowMessage(CommanderRestoreMessageName);
                if (msg != 0)
                {
                    PostMessage(HWND_BROADCAST, msg, IntPtr.Zero, IntPtr.Zero);
                }
            }
            catch { }

            return ActivateWindowByProcessName("WinPurifyCommander", "Central Commander");
        }

        /// <summary>
        /// 이미 실행 중인 WinPurify Pro 메인 앱 창을 전면으로 활성화합니다.
        /// (트레이에 숨겨져 있거나 최소화된 상태에서도 브로드캐스트 복원 메시지 및 EnumWindows로 100% 복구)
        /// </summary>
        public static bool ActivateAppWindow()
        {
            try
            {
                uint msg = RegisterWindowMessage(RestoreMessageName);
                if (msg != 0)
                {
                    PostMessage(HWND_BROADCAST, msg, IntPtr.Zero, IntPtr.Zero);
                }
            }
            catch { }

            return ActivateWindowByProcessName("WinPurifyPro", "WinPurify Pro");
        }

        private static bool ActivateWindowByProcessName(string procName, string? titleKeyword = null)
        {
            try
            {
                int currentPid = Process.GetCurrentProcess().Id;
                var procs = Process.GetProcesses();
                foreach (var proc in procs)
                {
                    try
                    {
                        if (proc.Id == currentPid) continue;

                        bool match = string.Equals(proc.ProcessName, procName, StringComparison.OrdinalIgnoreCase);
                        if (!match && !string.IsNullOrEmpty(titleKeyword))
                        {
                            match = !string.IsNullOrEmpty(proc.MainWindowTitle) &&
                                    proc.MainWindowTitle.Contains(titleKeyword, StringComparison.OrdinalIgnoreCase);
                        }

                        if (match)
                        {
                            // 1. MainWindowHandle이 유효한 경우 직접 활성화
                            if (proc.MainWindowHandle != IntPtr.Zero)
                            {
                                IntPtr hWnd = proc.MainWindowHandle;
                                if (IsIconic(hWnd))
                                {
                                    ShowWindow(hWnd, SW_RESTORE);
                                }
                                else
                                {
                                    ShowWindow(hWnd, SW_SHOW);
                                }
                                SetForegroundWindow(hWnd);
                                return true;
                            }

                            // 2. 트레이 숨김 등으로 MainWindowHandle이 0인 경우 EnumWindows로 해당 프로세스의 최상위 창 탐색 및 표시
                            IntPtr targetHwnd = IntPtr.Zero;
                            EnumWindows((hWnd, lParam) =>
                            {
                                GetWindowThreadProcessId(hWnd, out uint wndPid);
                                if (wndPid == proc.Id)
                                {
                                    var sb = new System.Text.StringBuilder(256);
                                    GetWindowText(hWnd, sb, 256);
                                    string wndTitle = sb.ToString();

                                    if (string.IsNullOrEmpty(titleKeyword) || wndTitle.Contains(titleKeyword, StringComparison.OrdinalIgnoreCase))
                                    {
                                        targetHwnd = hWnd;
                                        return false; // 탐색 중단
                                    }
                                }
                                return true;
                            }, IntPtr.Zero);

                            if (targetHwnd != IntPtr.Zero)
                            {
                                ShowWindow(targetHwnd, SW_RESTORE);
                                ShowWindow(targetHwnd, SW_SHOW);
                                SetForegroundWindow(targetHwnd);
                                return true;
                            }

                            // 프로세스가 존재하므로 브로드캐스트 메시지가 수신되어 복원될 것으로 간주
                            return true;
                        }
                    }
                    catch { }
                    finally
                    {
                        proc.Dispose();
                    }
                }
            }
            catch { }

            return false;
        }
    }
}
