using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// 시스템 전원 및 재부팅 제어 전용 서비스 (4-Tier Failover System Reboot Engine)
    /// - Tier 1: Win32 Native API (SeShutdownPrivilege 권한 토큰 획득 + ExitWindowsEx)
    /// - Tier 2: Windows System32 shutdown.exe (/r /f /t 3 강제 종료 플래그)
    /// - Tier 3: PowerShell CIM/WMI (Restart-Computer -Force)
    /// - Tier 4: cmd.exe 셸 직접 강제 호출
    /// </summary>
    public static class SystemPowerService
    {
        #region Win32 P/Invoke Declarations

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool LookupPrivilegeValue(string? lpSystemName, string lpName, ref long lpLuid);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public long Luid;
            public int Attributes;
        }

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(
            IntPtr TokenHandle,
            bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState,
            int BufferLength,
            IntPtr PreviousState,
            IntPtr ReturnLength);

        [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern bool ExitWindowsEx(int uFlags, int dwReason);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern bool CloseHandle(IntPtr handle);

        private const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        private const int TOKEN_QUERY = 0x00000008;
        private const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
        private const int SE_PRIVILEGE_ENABLED = 0x00000002;

        // ExitWindowsEx flags
        private const int EWX_LOGOFF = 0x00000000;
        private const int EWX_SHUTDOWN = 0x00000001;
        private const int EWX_REBOOT = 0x00000002;
        private const int EWX_FORCE = 0x00000004;
        private const int EWX_POWEROFF = 0x00000008;
        private const int EWX_FORCEIFHUNG = 0x00000010;

        // Reason codes
        private const int SHTDN_REASON_MAJOR_OPERATINGSYSTEM = 0x00020000;
        private const int SHTDN_REASON_MINOR_MAINTENANCE = 0x00000001;
        private const int SHTDN_REASON_FLAG_PLANNED = 0x40000000;

        #endregion

        /// <summary>
        /// 현재 프로세스 토큰에 SeShutdownPrivilege 권한을 활성화합니다.
        /// </summary>
        public static bool EnableShutdownPrivilege()
        {
            try
            {
                IntPtr hToken = IntPtr.Zero;
                if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref hToken))
                {
                    return false;
                }

                try
                {
                    TOKEN_PRIVILEGES tp = new TOKEN_PRIVILEGES
                    {
                        PrivilegeCount = 1,
                        Attributes = SE_PRIVILEGE_ENABLED,
                        Luid = 0
                    };

                    if (!LookupPrivilegeValue(null, SE_SHUTDOWN_NAME, ref tp.Luid))
                    {
                        return false;
                    }

                    return AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
                }
                finally
                {
                    CloseHandle(hToken);
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 시스템 재부팅을 비동기 안전 지연(delaySeconds 초 후) 및 강제 플래그(/f)로 실행합니다.
        /// 4단계 다중 계층 Failover를 통해 어떠한 환경에서도 재부팅이 확실히 수행되도록 보장합니다.
        /// </summary>
        public static void ScheduleForceReboot(int delaySeconds = 3, Action<string>? logger = null)
        {
            Task.Run(async () =>
            {
                logger?.Invoke($"[SystemPower] {delaySeconds}초 후 시스템 강제 재부팅(/r /f) 예약 시작...");
                if (delaySeconds > 0)
                {
                    await Task.Delay(delaySeconds * 1000);
                }

                bool executed = false;

                // Tier 1: Windows System32 shutdown.exe (/r /f /t 0 강제 종료)
                try
                {
                    string systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
                    string shutdownPath = Path.Combine(systemDir, "shutdown.exe");

                    if (!File.Exists(shutdownPath))
                    {
                        shutdownPath = "shutdown.exe";
                    }

                    var psi = new ProcessStartInfo
                    {
                        FileName = shutdownPath,
                        Arguments = "/r /f /t 0 /c \"WinPurify Pro Central Commander: 원격 유지보수 시스템 강제 재부팅\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var p = Process.Start(psi);
                    if (p != null)
                    {
                        p.WaitForExit(3000);
                        if (p.ExitCode == 0)
                        {
                            logger?.Invoke("[SystemPower] Tier 1: shutdown.exe /r /f /t 0 실행 성공");
                            executed = true;
                        }
                        else
                        {
                            logger?.Invoke($"[SystemPower 경고] shutdown.exe 반환 코드 {p.ExitCode} - Tier 2로 전환합니다.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger?.Invoke($"[SystemPower 경고] Tier 1 shutdown.exe 호출 실패: {ex.Message}");
                }

                // Tier 2: Win32 Native API (SeShutdownPrivilege 토큰 획득 + ExitWindowsEx 강제 재부팅)
                if (!executed)
                {
                    try
                    {
                        bool privOk = EnableShutdownPrivilege();
                        logger?.Invoke($"[SystemPower] SeShutdownPrivilege 권한 활성화: {(privOk ? "성공" : "실패")}");

                        int flags = EWX_REBOOT | EWX_FORCE | EWX_FORCEIFHUNG;
                        int reason = SHTDN_REASON_MAJOR_OPERATINGSYSTEM | SHTDN_REASON_MINOR_MAINTENANCE | SHTDN_REASON_FLAG_PLANNED;

                        bool win32Ok = ExitWindowsEx(flags, reason);
                        if (win32Ok)
                        {
                            logger?.Invoke("[SystemPower] Tier 2: Win32 ExitWindowsEx(EWX_REBOOT | EWX_FORCE) 호출 성공");
                            executed = true;
                        }
                        else
                        {
                            int err = Marshal.GetLastWin32Error();
                            logger?.Invoke($"[SystemPower 경고] ExitWindowsEx 실패 (Win32Error: {err}) - Tier 3로 전환합니다.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.Invoke($"[SystemPower 경고] Tier 2 Win32 API 호출 실패: {ex.Message}");
                    }
                }

                // Tier 3: PowerShell CIM/WMI Restart-Computer -Force
                if (!executed)
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = "-NoProfile -NonInteractive -WindowStyle Hidden -Command \"(Get-CimInstance Win32_OperatingSystem).Win32Shutdown(6); Restart-Computer -Force\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using var p = Process.Start(psi);
                        p?.WaitForExit(4000);
                        logger?.Invoke("[SystemPower] Tier 3: PowerShell CIM Restart-Computer -Force 실행 완료");
                        executed = true;
                    }
                    catch (Exception ex)
                    {
                        logger?.Invoke($"[SystemPower 경고] Tier 3 PowerShell 호출 실패: {ex.Message}");
                    }
                }

                // Tier 4: cmd.exe 셸 권한 상승(runas) 백업 호출
                if (!executed)
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = "/c shutdown.exe /r /f /t 0",
                            UseShellExecute = true,
                            Verb = "runas",
                            WindowStyle = ProcessWindowStyle.Hidden
                        };

                        Process.Start(psi);
                        logger?.Invoke("[SystemPower] Tier 4: cmd.exe (runas) shutdown 실행 완료");
                    }
                    catch (Exception ex)
                    {
                        logger?.Invoke($"[SystemPower 오류] 모든 재부팅 Tier 실행 실패: {ex.Message}");
                    }
                }
            });
        }

        /// <summary>
        /// 시스템 안전 종료를 비동기 지연(delaySeconds 초 후) 및 강제 플래그(/f)로 실행합니다.
        /// 4단계 다중 계층 Failover를 통해 어떠한 환경에서도 시스템 종료가 확실히 수행되도록 보장합니다.
        /// </summary>
        public static void ScheduleForceShutdown(int delaySeconds = 3, Action<string>? logger = null)
        {
            Task.Run(async () =>
            {
                logger?.Invoke($"[SystemPower] {delaySeconds}초 후 시스템 강제 종료(/s /f) 예약 시작...");
                if (delaySeconds > 0)
                {
                    await Task.Delay(delaySeconds * 1000);
                }

                bool executed = false;

                // Tier 1: Windows System32 shutdown.exe (/s /f /t 0 강제 전원 끄기)
                try
                {
                    string systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
                    string shutdownPath = Path.Combine(systemDir, "shutdown.exe");

                    if (!File.Exists(shutdownPath))
                    {
                        shutdownPath = "shutdown.exe";
                    }

                    var psi = new ProcessStartInfo
                    {
                        FileName = shutdownPath,
                        Arguments = "/s /f /t 0 /c \"WinPurify Pro Central Commander: 원격 시스템 강제 종료\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var p = Process.Start(psi);
                    if (p != null)
                    {
                        p.WaitForExit(3000);
                        if (p.ExitCode == 0)
                        {
                            logger?.Invoke("[SystemPower] Tier 1: shutdown.exe /s /f /t 0 실행 성공");
                            executed = true;
                        }
                        else
                        {
                            logger?.Invoke($"[SystemPower 경고] shutdown.exe 반환 코드 {p.ExitCode} - Tier 2로 전환합니다.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger?.Invoke($"[SystemPower 경고] Tier 1 shutdown.exe 호출 실패: {ex.Message}");
                }

                // Tier 2: Win32 Native API (SeShutdownPrivilege 토큰 획득 + ExitWindowsEx 강제 시스템 종료)
                if (!executed)
                {
                    try
                    {
                        bool privOk = EnableShutdownPrivilege();
                        logger?.Invoke($"[SystemPower] SeShutdownPrivilege 권한 활성화: {(privOk ? "성공" : "실패")}");

                        int flags = EWX_SHUTDOWN | EWX_POWEROFF | EWX_FORCE | EWX_FORCEIFHUNG;
                        int reason = SHTDN_REASON_MAJOR_OPERATINGSYSTEM | SHTDN_REASON_MINOR_MAINTENANCE | SHTDN_REASON_FLAG_PLANNED;

                        bool win32Ok = ExitWindowsEx(flags, reason);
                        if (win32Ok)
                        {
                            logger?.Invoke("[SystemPower] Tier 2: Win32 ExitWindowsEx(EWX_SHUTDOWN | EWX_POWEROFF) 호출 성공");
                            executed = true;
                        }
                        else
                        {
                            int err = Marshal.GetLastWin32Error();
                            logger?.Invoke($"[SystemPower 경고] ExitWindowsEx 실패 (Win32Error: {err}) - Tier 3로 전환합니다.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.Invoke($"[SystemPower 경고] Tier 2 Win32 API 호출 실패: {ex.Message}");
                    }
                }

                // Tier 3: PowerShell CIM/WMI Stop-Computer -Force
                if (!executed)
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "powershell.exe",
                            Arguments = "-NoProfile -NonInteractive -WindowStyle Hidden -Command \"(Get-CimInstance Win32_OperatingSystem).Win32Shutdown(12); Stop-Computer -Force\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using var p = Process.Start(psi);
                        p?.WaitForExit(4000);
                        logger?.Invoke("[SystemPower] Tier 3: PowerShell CIM Stop-Computer -Force 실행 완료");
                        executed = true;
                    }
                    catch (Exception ex)
                    {
                        logger?.Invoke($"[SystemPower 경고] Tier 3 PowerShell 호출 실패: {ex.Message}");
                    }
                }

                // Tier 4: cmd.exe 셸 권한 상승(runas) 백업 호출
                if (!executed)
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = "/c shutdown.exe /s /f /t 0",
                            UseShellExecute = true,
                            Verb = "runas",
                            WindowStyle = ProcessWindowStyle.Hidden
                        };

                        Process.Start(psi);
                        logger?.Invoke("[SystemPower] Tier 4: cmd.exe (runas) shutdown /s 실행 완료");
                    }
                    catch (Exception ex)
                    {
                        logger?.Invoke($"[SystemPower 오류] 모든 시스템 종료 Tier 실행 실패: {ex.Message}");
                    }
                }
            });
        }
    }
}
