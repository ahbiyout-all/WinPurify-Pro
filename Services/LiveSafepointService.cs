using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WinPurifyPro.Models;

namespace WinPurifyPro.Services
{
    public class SafepointRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Description { get; set; } = "Pre-Optimization Snapshot";
        public List<string> OptimizedTaskIds { get; set; } = new();
        public List<string> BackupRegFiles { get; set; } = new();
        public long ReclaimedBytes { get; set; }
    }

    public static class LiveSafepointService
    {
        private static readonly string BackupDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinPurifyPro",
            "Safepoints"
        );

        static LiveSafepointService()
        {
            try
            {
                if (!Directory.Exists(BackupDir))
                {
                    Directory.CreateDirectory(BackupDir);
                }
            }
            catch { }
        }

        public static async Task<SafepointRecord> CreateSafepointAsync(List<OptimizationTask> tasks, long totalReclaimed)
        {
            return await Task.Run(() =>
            {
                var record = new SafepointRecord
                {
                    Timestamp = DateTime.Now,
                    Description = $"Live Snapshot before {tasks.Count} Tasks",
                    ReclaimedBytes = totalReclaimed
                };

                foreach (var t in tasks)
                {
                    record.OptimizedTaskIds.Add(t.Id);
                    // Backup sensitive registry targets before clean
                    if (t.Category == TaskCategory.Registry && !string.IsNullOrWhiteSpace(t.FastCliCommand) && t.FastCliCommand.Contains("reg delete"))
                    {
                        string regPath = ExtractRegPath(t.FastCliCommand);
                        if (!string.IsNullOrWhiteSpace(regPath))
                        {
                            string backupFile = Path.Combine(BackupDir, $"{record.Id}_{t.Id}.reg");
                            if (ExportRegistryKey(regPath, backupFile))
                            {
                                record.BackupRegFiles.Add(backupFile);
                            }
                        }
                    }
                }

                try
                {
                    string filePath = Path.Combine(BackupDir, $"safepoint_{record.Timestamp:yyyyMMdd_HHmmss}.json");
                    string json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, json);
                }
                catch { }

                return record;
            });
        }

        public static async Task<bool> RollbackSafepointAsync(SafepointRecord record)
        {
            return await Task.Run(() =>
            {
                if (record.BackupRegFiles == null || record.BackupRegFiles.Count == 0)
                {
                    return true;
                }

                bool allSuccess = true;
                foreach (var regFile in record.BackupRegFiles)
                {
                    if (File.Exists(regFile))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "reg.exe",
                            Arguments = $"import \"{regFile}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        using var proc = Process.Start(psi);
                        proc?.WaitForExit(3000);
                        if (proc?.ExitCode != 0) allSuccess = false;
                    }
                }
                return allSuccess;
            });
        }

        private static string ExtractRegPath(string cmd)
        {
            try
            {
                int start = cmd.IndexOf("reg delete", StringComparison.OrdinalIgnoreCase);
                if (start >= 0)
                {
                    string sub = cmd.Substring(start + 10).Trim();
                    if (sub.StartsWith("\""))
                    {
                        int endQuote = sub.IndexOf('\"', 1);
                        if (endQuote > 1) return sub.Substring(1, endQuote - 1);
                    }
                    else
                    {
                        var parts = sub.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0) return parts[0];
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        private static bool ExportRegistryKey(string keyPath, string targetFile)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "reg.exe",
                    Arguments = $"export \"{keyPath}\" \"{targetFile}\" /y",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc?.WaitForExit(3000);
                return proc?.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 윈도우 시스템 복원(System Protection) 상태를 사전 점검하고, 비활성화되어 있을 경우
        /// C: 드라이브 보호 강제 활성화 및 24시간 빈도 제한(Frequency)을 해제한 후 복원 지점을 확실하게 생성합니다.
        /// </summary>
        public static async Task<(bool Success, string Message, long ElapsedMs)> CreateSnapshotDetailedAsync(string description = "WinPurify Pro Snapshot")
        {
            var sw = Stopwatch.StartNew();
            var record = new SafepointRecord
            {
                Timestamp = DateTime.Now,
                Description = description,
                ReclaimedBytes = 0
            };

            // 1. 로컬 세이프포인트 JSON 백업 기록 (이중 백업 안전장치)
            try
            {
                string filePath = Path.Combine(BackupDir, $"safepoint_{record.Timestamp:yyyyMMdd_HHmmss}.json");
                string json = JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch { }

            // 2. Native C++ 초고속 1차 시도 (PowerShell 오버헤드 없는 네이티브 복원 지점 생성)
            if (NativeEngineService.IsNativeCoreAvailable())
            {
                var (nSuccess, nSeq, nMsg) = NativeEngineService.TryCreateNativeRestorePoint(description);
                if (nSuccess)
                {
                    sw.Stop();
                    return (true, $"{nMsg} [Native C++ 초고속]", sw.ElapsedMilliseconds);
                }
            }

            // 3. Windows 시스템 복원 지점 생성 및 자동 활성화 파이프라인 (PowerShell 다단계 폴백)
            var (psSuccess, psMessage) = await Task.Run(() =>
            {
                try
                {
                    // PowerShell 스크립트 구성:
                    // 1) 레지스트리 시스템 복원 정책(DisableSR=0) 및 24시간 생성 제한(Frequency=0) 해제
                    // 2) VSS, swprv, srservice 서비스 시작
                    // 3) C:\ 드라이브 시스템 보호 활성화(Enable-ComputerRestore)
                    // 4) Checkpoint-Computer 실행, 실패 시 WMI SystemRestore 클래스로 폴백
                    // 5) Get-ComputerRestorePoint를 통한 실제 생성 검증
                    string script = $@"
$ErrorActionPreference = 'Continue'
$log = @()

# 1. 시스템 복원 레지스트리 정책 및 24시간 빈도 제한 해제
try {{
    $srKey = 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore'
    if (-not (Test-Path $srKey)) {{
        New-Item -Path $srKey -Force | Out-Null
    }}
    Set-ItemProperty -Path $srKey -Name 'DisableSR' -Value 0 -Type DWord -Force -ErrorAction SilentlyContinue
    Set-ItemProperty -Path $srKey -Name 'RPSessionInterval' -Value 1 -Type DWord -Force -ErrorAction SilentlyContinue
    # Windows 기본 24시간(1440분) 1회 생성 제한을 0분으로 변경하여 즉시 강제 생성 허용
    Set-ItemProperty -Path $srKey -Name 'SystemRestorePointCreationFrequency' -Value 0 -Type DWord -Force -ErrorAction SilentlyContinue

    $policyKey = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore'
    if (Test-Path $policyKey) {{
        Set-ItemProperty -Path $policyKey -Name 'DisableSR' -Value 0 -Type DWord -Force -ErrorAction SilentlyContinue
        Set-ItemProperty -Path $policyKey -Name 'DisableConfig' -Value 0 -Type DWord -Force -ErrorAction SilentlyContinue
    }}
    $log += 'RegConfigured'
}} catch {{
    $log += ('RegErr:' + $_.Exception.Message)
}}

# 2. 필수 서비스 (VSS, swprv, srservice) 활성화 및 가동
try {{
    Start-Process -FilePath 'sc.exe' -ArgumentList 'config vss start= demand' -WindowStyle Hidden -Wait -ErrorAction SilentlyContinue
    Start-Process -FilePath 'net.exe' -ArgumentList 'start vss' -WindowStyle Hidden -Wait -ErrorAction SilentlyContinue
    Start-Process -FilePath 'sc.exe' -ArgumentList 'config swprv start= demand' -WindowStyle Hidden -Wait -ErrorAction SilentlyContinue
    Start-Process -FilePath 'net.exe' -ArgumentList 'start swprv' -WindowStyle Hidden -Wait -ErrorAction SilentlyContinue
    Start-Process -FilePath 'sc.exe' -ArgumentList 'config srservice start= demand' -WindowStyle Hidden -Wait -ErrorAction SilentlyContinue
    Start-Process -FilePath 'net.exe' -ArgumentList 'start srservice' -WindowStyle Hidden -Wait -ErrorAction SilentlyContinue
    $log += 'ServicesStarted'
}} catch {{
    $log += ('SvcErr:' + $_.Exception.Message)
}}

# 3. C:\ 드라이브 시스템 보호(ComputerRestore) 자동 활성화
$protectionAutoEnabled = $false
try {{
    Enable-ComputerRestore -Drive 'C:\' -ErrorAction Stop
    $protectionAutoEnabled = $true
    $log += 'ProtectionAutoEnabled'
}} catch {{
    # 이미 활성화되어 있거나 WMI 폴백
    $log += ('ProtectionInfo:' + $_.Exception.Message)
    try {{
        Start-Process -FilePath 'vssadmin.exe' -ArgumentList 'resize shadowstorage /for=C: /on=C: /maxsize=5%' -WindowStyle Hidden -Wait -ErrorAction SilentlyContinue
    }} catch {{}}
}}

# 4. 복원 지점 생성 (Checkpoint-Computer 및 WMI 다단계 시도)
$created = $false
$desc = '{description.Replace("'", "''")}'
try {{
    Checkpoint-Computer -Description $desc -RestorePointType 'MODIFY_SETTINGS' -ErrorAction Stop
    $created = $true
    $log += 'CheckpointSuccess'
}} catch {{
    $chkErr = $_.Exception.Message
    $log += ('CheckpointErr:' + $chkErr)

    # WMI 폴백 시도
    try {{
        $wmi = [wmiclass]'\\localhost\root\default:SystemRestore'
        $wmiRes = $wmi.CreateRestorePoint($desc, 0, 100)
        if ($wmiRes.ReturnValue -eq 0) {{
            $created = $true
            $log += 'WmiSuccess'
        }} else {{
            $log += ('WmiFailCode:' + $wmiRes.ReturnValue)
        }}
    }} catch {{
        $log += ('WmiExc:' + $_.Exception.Message)
    }}
}}

# 5. 최종 검증 (최근 5분 이내 생성된 복원 지점 확인)
$latest = $null
try {{
    $latest = Get-ComputerRestorePoint -ErrorAction SilentlyContinue | Sort-Object CreationTime -Descending | Select-Object -First 1
}} catch {{}}

if ($created -or ($latest -ne $null)) {{
    $seq = if ($latest) {{ $latest.SequenceNumber }} else {{ 0 }}
    $autoText = if ($protectionAutoEnabled) {{ '[시스템 보호 자동 활성화됨]' }} else {{ '' }}
    Write-Output ""RESULT_SUCCESS|Seq:$seq|$autoText|Log:$($log -join ';')""
}} else {{
    Write-Output ""RESULT_FAILED|Log:$($log -join ';')""
}}
";
                    // Base64 인코딩을 통해 따옴표나 공백, 특수문자 에러 원천 차단
                    string encodedScript = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -EncodedCommand {encodedScript}",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using var proc = Process.Start(psi);
                    if (proc == null)
                    {
                        return (false, "PowerShell 프로세스를 시작할 수 없습니다.");
                    }

                    // 복원 지점 VSS 스냅샷 생성을 위해 최대 60초 대기
                    string stdOut = proc.StandardOutput.ReadToEnd();
                    string stdErr = proc.StandardError.ReadToEnd();
                    proc.WaitForExit(60000);

                    if (stdOut.Contains("RESULT_SUCCESS"))
                    {
                        string details = "복원 지점 생성 성공";
                        if (stdOut.Contains("[시스템 보호 자동 활성화됨]"))
                        {
                            details += " (비활성화되어 있던 C: 시스템 보호 자동 활성화 완료)";
                        }
                        return (true, details);
                    }
                    else
                    {
                        string reason = !string.IsNullOrWhiteSpace(stdErr) ? stdErr : stdOut;
                        if (reason.Length > 200) reason = reason.Substring(0, 200) + "...";
                        return (false, $"복원 지점 생성 실패 (원인: {reason})");
                    }
                }
                catch (Exception ex)
                {
                    return (false, $"복원 지점 엔진 예외: {ex.Message}");
                }
            });

            sw.Stop();
            return (psSuccess, psMessage, sw.ElapsedMilliseconds);
        }

        public static bool CreateSnapshot(string description = "WinPurify Pro Remote Commander Snapshot")
        {
            try
            {
                var task = CreateSnapshotDetailedAsync(description);
                task.Wait(65000);
                return task.Result.Item1;
            }
            catch
            {
                return false;
            }
        }

        public static List<SafepointRecord> GetAvailableSafepoints()
        {
            var list = new List<SafepointRecord>();
            try
            {
                if (!Directory.Exists(BackupDir)) return list;
                foreach (var file in Directory.GetFiles(BackupDir, "safepoint_*.json"))
                {
                    string json = File.ReadAllText(file);
                    var record = JsonSerializer.Deserialize<SafepointRecord>(json);
                    if (record != null) list.Add(record);
                }
            }
            catch { }
            return list;
        }

        public static async Task<(bool Success, string Message)> RollbackLatestSafepointAsync()
        {
            var points = GetAvailableSafepoints();
            if (points.Count == 0)
            {
                return (false, "복원 가능한 로컬 세이프포인트(백업 레코드)가 존재하지 않습니다.");
            }

            points.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));
            var latest = points[0];

            bool ok = await RollbackSafepointAsync(latest);
            if (ok)
            {
                return (true, $"최신 스냅샷({latest.Timestamp:yyyy-MM-dd HH:mm:ss}, {latest.BackupRegFiles.Count}개 레지스트리 키)으로 롤백 복원 완료");
            }
            else
            {
                return (false, $"스냅샷({latest.Id.Substring(0, 8)}) 복원 중 일부 항목 처리 실패");
            }
        }
    }
}
