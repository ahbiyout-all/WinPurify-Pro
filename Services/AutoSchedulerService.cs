using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinPurifyPro;
using WinPurifyPro.Models;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// 스케줄러 설정 데이터 모델
    /// </summary>
    public class ScheduleConfig
    {
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 실행 트리거: "STARTUP" (부팅 시), "LOGON" (로그온 시), "INTERVAL" (지정 주기 간격), "DAILY" (매일 정시), "WEEKLY" (매주 지정 요일)
        /// </summary>
        public string TriggerType { get; set; } = "WEEKLY";

        /// <summary>
        /// 주기 간격 값 (단위: 시간, 예: 1, 2, 4, 6, 12, 24시간)
        /// </summary>
        public int IntervalHours { get; set; } = 4;

        /// <summary>
        /// 주기 간격 값 (단위: 분, 예: 30, 60, 120분)
        /// </summary>
        public int IntervalMinutes { get; set; } = 60;

        /// <summary>
        /// 주기 단위: "HOURS" or "MINUTES"
        /// </summary>
        public string IntervalUnit { get; set; } = "HOURS";

        /// <summary>
        /// 시스템 부팅 후 실행 지연 시간(분) (부팅 안정화 대기, 예: 1분)
        /// </summary>
        public int StartupDelayMinutes { get; set; } = 1;

        /// <summary>
        /// 매주 실행 요일 (0: SUN, 1: MON, ..., 6: SAT)
        /// </summary>
        public int DayOfWeek { get; set; } = 0;

        /// <summary>
        /// 실행 시각 (시: 0~23)
        /// </summary>
        public int Hour { get; set; } = 3;

        /// <summary>
        /// 실행 시각 (분: 0~59)
        /// </summary>
        public int Minute { get; set; } = 0;

        /// <summary>
        /// 대상 프로필: "all", "safe", "deep", "gaming", "privacy", "custom"
        /// </summary>
        public string Profile { get; set; } = "safe";

        /// <summary>
        /// 사용자 정의 개별 모듈 ID 목록 (콤마 구분)
        /// </summary>
        public string CustomModuleIds { get; set; } = string.Empty;

        /// <summary>
        /// 정화 전 RAM 즉시 압축 (Working Set Trim) 수행 여부
        /// </summary>
        public bool AutoTrimRam { get; set; } = true;

        /// <summary>
        /// 정화 전 실시간 세이프포인트(레지스트리 스냅샷) 자동 생성 여부
        /// </summary>
        public bool CreateSafepoint { get; set; } = true;

        /// <summary>
        /// AC 전원 연결 시에만 실행 (배터리 사용 시 절약)
        /// </summary>
        public bool RunOnlyOnAC { get; set; } = false;

        /// <summary>
        /// 절전 모드 해제 후 실행 (Wake to run)
        /// </summary>
        public bool WakeToRun { get; set; } = false;
    }

    /// <summary>
    /// Windows 작업 스케줄러(schtasks.exe) 최고 관리자 권한 연동 및 무음/백그라운드 자동 정화 실행 서비스
    /// </summary>
    public static class AutoSchedulerService
    {
        public const string TaskName = "WinPurifyPro_AutoMaintenance";

        private static string GetLogDirectory()
        {
            string logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "WinPurifyPro",
                "Logs"
            );
            if (!Directory.Exists(logDir))
            {
                try { Directory.CreateDirectory(logDir); } catch { }
            }
            return logDir;
        }

        private static string GetExecutablePath()
        {
            try
            {
                string? path = Environment.ProcessPath;
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    return path;
                }
                path = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    return path;
                }
            }
            catch { }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WinPurifyPro.exe");
        }

        /// <summary>
        /// 고급 스케줄러 등록 (부팅 시, 주기별 간격, 매일, 매주 및 개별 모듈 지정 지원)
        /// </summary>
        public static async Task<bool> RegisterAdvancedTaskAsync(ScheduleConfig config)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string exePath = GetExecutablePath();
                    if (string.IsNullOrWhiteSpace(exePath)) return false;

                    string options = "--silent --schedule-run";
                    if (!string.IsNullOrWhiteSpace(config.Profile))
                    {
                        options += $" --profile {config.Profile}";
                    }
                    if (!string.IsNullOrWhiteSpace(config.CustomModuleIds))
                    {
                        options += $" --modules \"{config.CustomModuleIds}\"";
                    }
                    if (config.AutoTrimRam)
                    {
                        options += " --trim-ram";
                    }
                    if (config.CreateSafepoint)
                    {
                        options += " --safepoint";
                    }

                    string timeStr = $"{config.Hour:D2}:{config.Minute:D2}";
                    string args;

                    string trigger = config.TriggerType.ToUpperInvariant();
                    switch (trigger)
                    {
                        case "STARTUP":
                        case "BOOT":
                        case "ONSTART":
                            // 시스템 부팅 시 실행 (/SC ONSTART)
                            args = $"/Create /TN \"{TaskName}\" /TR \"\\\"{exePath}\\\" {options}\" /SC ONSTART /RL HIGHEST /F";
                            if (config.StartupDelayMinutes > 0)
                            {
                                string delayStr = $"000{Math.Min(config.StartupDelayMinutes, 9):D1}:00";
                                args += $" /DELAY {delayStr}";
                            }
                            break;

                        case "LOGON":
                        case "ONLOGON":
                            // 사용자 로그온 시 실행 (/SC ONLOGON)
                            args = $"/Create /TN \"{TaskName}\" /TR \"\\\"{exePath}\\\" {options}\" /SC ONLOGON /RL HIGHEST /F";
                            if (config.StartupDelayMinutes > 0)
                            {
                                string delayStr = $"000{Math.Min(config.StartupDelayMinutes, 9):D1}:00";
                                args += $" /DELAY {delayStr}";
                            }
                            break;

                        case "INTERVAL":
                        case "INTERVAL_HOURS":
                            // 특정 시간 간격 실행 (/SC HOURLY /MO N)
                            int moHours = Math.Clamp(config.IntervalHours, 1, 24);
                            args = $"/Create /TN \"{TaskName}\" /TR \"\\\"{exePath}\\\" {options}\" /SC HOURLY /MO {moHours} /ST {timeStr} /RL HIGHEST /F";
                            break;

                        case "INTERVAL_MINUTES":
                            // 특정 분 간격 실행 (/SC MINUTE /MO N)
                            int moMins = Math.Clamp(config.IntervalMinutes, 1, 1440);
                            args = $"/Create /TN \"{TaskName}\" /TR \"\\\"{exePath}\\\" {options}\" /SC MINUTE /MO {moMins} /ST {timeStr} /RL HIGHEST /F";
                            break;

                        case "DAILY":
                            // 매일 정시 실행
                            args = $"/Create /TN \"{TaskName}\" /TR \"\\\"{exePath}\\\" {options}\" /SC DAILY /ST {timeStr} /RL HIGHEST /F";
                            break;

                        case "WEEKLY":
                        default:
                            // 매주 특정 요일 실행
                            string dayStr = config.DayOfWeek switch
                            {
                                1 => "MON",
                                2 => "TUE",
                                3 => "WED",
                                4 => "THU",
                                5 => "FRI",
                                6 => "SAT",
                                _ => "SUN"
                            };
                            args = $"/Create /TN \"{TaskName}\" /TR \"\\\"{exePath}\\\" {options}\" /SC WEEKLY /D {dayStr} /ST {timeStr} /RL HIGHEST /F";
                            break;
                    }

                    var psi = new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = args,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var proc = Process.Start(psi);
                    if (proc == null) return false;

                    bool exited = proc.WaitForExit(5000);
                    if (!exited)
                    {
                        try { proc.Kill(true); } catch { }
                        return false;
                    }

                    return proc.ExitCode == 0;
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// 기존 호출 호환용 메서드
        /// </summary>
        public static async Task<bool> RegisterTaskAsync(string frequency, int dayOfWeek, int hour, int minute, string profile = "safe", bool autoTrimRam = true)
        {
            var config = new ScheduleConfig
            {
                TriggerType = frequency,
                DayOfWeek = dayOfWeek,
                Hour = hour,
                Minute = minute,
                Profile = profile,
                AutoTrimRam = autoTrimRam,
                CreateSafepoint = true
            };
            return await RegisterAdvancedTaskAsync(config);
        }

        public static async Task<bool> RegisterWeeklyTaskAsync(int dayOfWeek, int hour, int minute, string profile = "safe", bool autoTrimRam = true)
        {
            return await RegisterTaskAsync("WEEKLY", dayOfWeek, hour, minute, profile, autoTrimRam);
        }

        /// <summary>
        /// 작업 스케줄러 등록 해제
        /// </summary>
        public static async Task<bool> UnregisterScheduledTaskAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = $"/Delete /TN \"{TaskName}\" /F",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var proc = Process.Start(psi);
                    if (proc == null) return false;

                    bool exited = proc.WaitForExit(3000);
                    if (!exited)
                    {
                        try { proc.Kill(true); } catch { }
                        return false;
                    }

                    return proc.ExitCode == 0;
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// 작업 스케줄러 등록 여부 확인
        /// </summary>
        public static async Task<bool> IsTaskScheduledAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = $"/Query /TN \"{TaskName}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var proc = Process.Start(psi);
                    if (proc == null) return false;

                    bool exited = proc.WaitForExit(2000);
                    if (!exited)
                    {
                        try { proc.Kill(true); } catch { }
                        return false;
                    }

                    return proc.ExitCode == 0;
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// 등록된 작업 스케줄의 상세 상태(다음 실행 일시, 마지막 실행 결과 등) 조회
        /// </summary>
        public static async Task<(bool Exists, string NextRunTime, string LastRunTime, string Status)> GetTaskStatusDetailsAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = $"/Query /TN \"{TaskName}\" /FO LIST /V",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    using var proc = Process.Start(psi);
                    if (proc == null) return (false, "N/A", "N/A", "Not Found");

                    string output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(3000);

                    if (proc.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
                    {
                        return (false, "N/A", "N/A", "미등록");
                    }

                    string nextRun = "N/A";
                    string lastRun = "N/A";
                    string status = "준비 완료";

                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("Next Run Time:", StringComparison.OrdinalIgnoreCase) ||
                            line.StartsWith("다음 실행 시간:", StringComparison.OrdinalIgnoreCase))
                        {
                            nextRun = line.Substring(line.IndexOf(':') + 1).Trim();
                        }
                        else if (line.StartsWith("Last Run Time:", StringComparison.OrdinalIgnoreCase) ||
                                 line.StartsWith("마지막 실행 시간:", StringComparison.OrdinalIgnoreCase))
                        {
                            lastRun = line.Substring(line.IndexOf(':') + 1).Trim();
                        }
                        else if (line.StartsWith("Status:", StringComparison.OrdinalIgnoreCase) ||
                                 line.StartsWith("상태:", StringComparison.OrdinalIgnoreCase))
                        {
                            status = line.Substring(line.IndexOf(':') + 1).Trim();
                        }
                    }

                    return (true, nextRun, lastRun, status);
                }
                catch
                {
                    return (false, "N/A", "N/A", "조회 실패");
                }
            });
        }

        /// <summary>
        /// 스케줄된 작업을 지금 즉시 실행 테스트
        /// </summary>
        public static async Task<bool> RunTaskNowAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "schtasks.exe",
                        Arguments = $"/Run /TN \"{TaskName}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var proc = Process.Start(psi);
                    if (proc == null) return false;

                    bool exited = proc.WaitForExit(3000);
                    if (!exited)
                    {
                        try { proc.Kill(true); } catch { }
                        return false;
                    }

                    return proc.ExitCode == 0;
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// 무음/헤드리스 모드로 실행 시, 지정된 프로필 또는 개별 모듈들을 백그라운드에서 순차 실행하고 로그를 기록
        /// </summary>
        public static async Task<int> ExecuteHeadlessMaintenanceAsync(string[] args)
        {
            string logPath = Path.Combine(GetLogDirectory(), "AutoScheduler.log");
            var sb = new StringBuilder();
            sb.AppendLine($"================================================================================");
            sb.AppendLine($"[WinPurify Pro AutoScheduler] Execution Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"[Arguments] {string.Join(" ", args)}");

            try
            {
                // 파라미터 파싱
                string profile = "safe";
                string customModulesStr = "";
                bool trimRam = false;
                bool safepoint = true;

                for (int i = 0; i < args.Length; i++)
                {
                    string a = args[i];
                    if (string.Equals(a, "--profile", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    {
                        profile = args[++i].ToLowerInvariant();
                    }
                    else if (string.Equals(a, "--modules", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                    {
                        customModulesStr = args[++i];
                    }
                    else if (string.Equals(a, "--trim-ram", StringComparison.OrdinalIgnoreCase))
                    {
                        trimRam = true;
                    }
                    else if (string.Equals(a, "--safepoint", StringComparison.OrdinalIgnoreCase))
                    {
                        safepoint = true;
                    }
                    else if (string.Equals(a, "--no-safepoint", StringComparison.OrdinalIgnoreCase))
                    {
                        safepoint = false;
                    }
                }

                // 1. RAM Working Set 압축
                if (trimRam)
                {
                    try
                    {
                        int trimRes = NativeEngineService.ExecuteQuickMaintenance(4); // RamTrim
                        sb.AppendLine($"[RAM Trim] Working Set Memory Compacted (Result: {trimRes})");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"[RAM Trim Warning] {ex.Message}");
                    }
                }

                // 2. 세이프포인트 백업
                if (safepoint)
                {
                    try
                    {
                        var snapRes = await LiveSafepointService.CreateSnapshotDetailedAsync("AutoScheduler Scheduled Maintenance");
                        sb.AppendLine($"[Safepoint] Snapshot Created: {snapRes.Success} (Elapsed: {snapRes.ElapsedMs}ms)");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"[Safepoint Warning] {ex.Message}");
                    }
                }

                // 3. 대상 모듈 로드 및 필터링
                var allTasks = TaskDataSeeder.GetAllTasks();
                var targetTasks = new List<OptimizationTask>();

                var customModuleIds = new HashSet<string>(
                    customModulesStr.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries),
                    StringComparer.OrdinalIgnoreCase
                );

                if (customModuleIds.Count > 0)
                {
                    targetTasks = allTasks.Where(t => customModuleIds.Contains(t.Id)).ToList();
                    sb.AppendLine($"[Target Scope] Custom Modules Mode ({targetTasks.Count} modules)");
                }
                else
                {
                    switch (profile)
                    {
                        case "all":
                            targetTasks = allTasks;
                            break;
                        case "deep":
                            targetTasks = allTasks.Where(t => t.Risk == RiskLevel.Safe || t.Risk == RiskLevel.Deep).ToList();
                            break;
                        case "gaming":
                            targetTasks = allTasks.Where(t => 
                                t.Category == TaskCategory.System || 
                                t.Category == TaskCategory.Privacy ||
                                t.Category == TaskCategory.Registry ||
                                t.Id.Contains("game", StringComparison.OrdinalIgnoreCase) ||
                                t.Id.Contains("boost", StringComparison.OrdinalIgnoreCase)
                            ).ToList();
                            break;
                        case "privacy":
                            targetTasks = allTasks.Where(t => t.Category == TaskCategory.Privacy || t.Category == TaskCategory.Browser).ToList();
                            break;
                        case "safe":
                        default:
                            targetTasks = allTasks.Where(t => t.Risk == RiskLevel.Safe).ToList();
                            break;
                    }
                    sb.AppendLine($"[Target Scope] Profile '{profile.ToUpperInvariant()}' Mode ({targetTasks.Count} modules)");
                }

                int successCount = 0;
                int failureCount = 0;

                // 4. 모듈 순차 실행
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(15));
                foreach (var task in targetTasks)
                {
                    try
                    {
                        var res = await MultiEngineDispatcher.ExecuteTaskAsync(task, cts.Token);
                        if (res.Success)
                        {
                            successCount++;
                            sb.AppendLine($"  [OK] {task.Id} - {task.Name} ({res.UsedMode}, {res.ExecutionTimeMs}ms)");
                        }
                        else
                        {
                            failureCount++;
                            sb.AppendLine($"  [FAIL] {task.Id} - {task.Name}: {res.OutputMessage}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        sb.AppendLine($"  [ERR] {task.Id} - {task.Name}: {ex.Message}");
                    }
                }

                sb.AppendLine($"[Summary] Completed: {successCount} succeeded, {failureCount} failed of {targetTasks.Count} total.");
                sb.AppendLine($"[WinPurify Pro AutoScheduler] Execution Finished at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine();

                try
                {
                    File.AppendAllText(logPath, sb.ToString(), Encoding.UTF8);
                }
                catch { }

                return failureCount == 0 ? 0 : 1;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[FATAL ERROR] {ex.Message}\n{ex.StackTrace}");
                try
                {
                    File.AppendAllText(logPath, sb.ToString(), Encoding.UTF8);
                }
                catch { }
                return -1;
            }
        }
    }
}
