using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// WinPurify Pro 메인 앱과 Central Commander 콘솔 간의 독립 실행 및 동시 실행(Coexistence & Independent Instance) 제어 서비스
    /// 
    /// [핵심 공존 규칙]
    /// 1. 한 PC에서 WinPurify Pro 메인 앱과 Central Commander 콘솔의 '동시 실행(Coexistence)'을 전면 지원합니다.
    /// 2. 동일 앱 중복 실행 방지:
    ///    - WinPurify Pro 메인 앱은 오직 자기 자신의 중복 실행만 방지 (이미 실행 중이면 기존 창 활성화)
    ///    - Central Commander 콘솔은 오직 자기 자신의 중복 실행만 방지 (이미 실행 중이면 기존 창 활성화)
    /// 3. 상호 비간섭: 메인 앱과 커맨더는 서로의 실행을 차단하지 않으며 각자의 창을 독립적으로 유지합니다.
    /// 4. 소켓 및 포트 공유: 동일 PC 동시 구동 시 포트 충돌 없이 에이전트 채널 및 C++ 커널 가속을 공유합니다.
    /// </summary>
    public static class SingleInstanceService
    {
        public const string AppMutexName = @"Global\WinPurifyPro_App_Mutex_Global_6904B914";
        public const string CommanderMutexName = @"Global\WinPurifyPro_Commander_Mutex_Global_6904B914";

        #region Win32 API Interop for Window Activation
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
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
                            string title = proc.MainWindowTitle;
                            if (!string.IsNullOrEmpty(title) &&
                                (title.Contains("Central Commander", StringComparison.OrdinalIgnoreCase) ||
                                 title.Contains("Commander Console", StringComparison.OrdinalIgnoreCase)))
                            {
                                return true;
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
                        // 커맨더 창이 아닌 일반 메인 윈도우인지 확인
                        if (string.IsNullOrEmpty(title) ||
                            !title.Contains("Commander", StringComparison.OrdinalIgnoreCase))
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
            return ActivateWindowByProcessName("WinPurifyCommander", "Central Commander");
        }

        /// <summary>
        /// 이미 실행 중인 WinPurify Pro 메인 앱 창을 전면으로 활성화합니다.
        /// </summary>
        public static bool ActivateAppWindow()
        {
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

                        if (match && proc.MainWindowHandle != IntPtr.Zero)
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
