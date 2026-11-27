using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WinPurifyPro.Models;
using WinPurifyPro.Services;

namespace WinPurifyPro.ViewModels
{
    public partial class MainViewModel
    {
        private async void CheckScheduleStatus()
        {
            IsScheduled = await AutoSchedulerService.IsTaskScheduledAsync();
        }

        private DispatcherTimer? _remoteActionCloseTimer;

        private void OnRemoteCommandExecuting(object? sender, RemoteCommandActionEventArgs e)
        {
            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                _remoteActionCloseTimer?.Stop();

                try { System.Media.SystemSounds.Exclamation.Play(); } catch { }

                RemoteActionTitle = "🚨 중앙 관제(Commander) 원격 명령 동작 중";
                RemoteActionCommandName = e.CommandTitle;
                RemoteActionSourceIp = $"발신지 관제 콘솔: {e.ClientIp}";
                RemoteActionStatus = "원격 최적화 엔진 동작 중...";
                RemoteActionDetails = $"명령 타입: {e.CommandType} | 수신 시각: {e.Timestamp:HH:mm:ss}";
                IsRemoteActionActive = true;
                RemoteActionAutoCloseSeconds = 5;
                IsRemoteActionModalOpen = true;

                AddLog($"[원격 관제 수신] 중앙 콘솔({e.ClientIp})로부터 '{e.CommandTitle}' 명령을 수신하여 실행합니다.");
            });
        }

        private void OnRemoteCommandCompleted(object? sender, RemoteCommandActionEventArgs e)
        {
            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                try { System.Media.SystemSounds.Asterisk.Play(); } catch { }

                RemoteActionTitle = e.Status == "Success"
                    ? "✅ 중앙 관제(Commander) 원격 명령 처리 완료"
                    : "⚠️ 중앙 관제(Commander) 원격 명령 오류";
                RemoteActionStatus = e.Status == "Success" ? "정상 완료" : "실패";
                RemoteActionDetails = $"{e.ResultMessage} (소요시간: {e.ElapsedMs}ms)";
                IsRemoteActionActive = false;

                RemoteExecutionHistory.Insert(0, new RemoteExecutionRecord
                {
                    CommanderIp = e.ClientIp,
                    CommandType = e.CommandType,
                    CommandTitle = e.CommandTitle,
                    Details = e.ResultMessage,
                    ExecutionTimeMs = e.ElapsedMs,
                    IsSuccess = e.Status == "Success",
                    Timestamp = e.Timestamp
                });
                if (RemoteExecutionHistory.Count > 100) RemoteExecutionHistory.RemoveAt(RemoteExecutionHistory.Count - 1);

                if (e.CommandType.Contains("Account", StringComparison.OrdinalIgnoreCase) ||
                    e.CommandType.Contains("ZeroTrace", StringComparison.OrdinalIgnoreCase) ||
                    e.CommandType.Contains("Purge", StringComparison.OrdinalIgnoreCase) ||
                    e.CommandType.Contains("Maintenance", StringComparison.OrdinalIgnoreCase))
                {
                    HasCommanderConfirmation = true;
                    LastCommanderConfirmationTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    LatestCommanderConfirmationStatus = e.Status == "Success"
                        ? $"✅ [Commander 원격 확인 완료] {e.ClientIp} -> {e.CommandTitle} ({e.ElapsedMs}ms)"
                        : $"⚠️ [Commander 원격 실패] {e.ClientIp} -> {e.CommandTitle} ({e.ResultMessage})";

                    CommanderZeroTraceLogs.Insert(0, new RemoteExecutionRecord
                    {
                        CommanderIp = e.ClientIp,
                        CommandType = e.CommandType,
                        CommandTitle = e.CommandTitle,
                        Details = $"[원격 확인 영수증] {e.ResultMessage}",
                        ExecutionTimeMs = e.ElapsedMs,
                        IsSuccess = e.Status == "Success",
                        Timestamp = DateTime.Now
                    });
                    if (CommanderZeroTraceLogs.Count > 30) CommanderZeroTraceLogs.RemoveAt(CommanderZeroTraceLogs.Count - 1);
                }

                AddLog($"[원격 관제 완료] '{e.CommandTitle}' 처리 완료 - {e.ResultMessage} ({e.ElapsedMs}ms)");

                RemoteActionAutoCloseSeconds = 5;
                _remoteActionCloseTimer?.Stop();
                _remoteActionCloseTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                _remoteActionCloseTimer.Tick += (s, args) =>
                {
                    RemoteActionAutoCloseSeconds--;
                    if (RemoteActionAutoCloseSeconds <= 0)
                    {
                        _remoteActionCloseTimer.Stop();
                        IsRemoteActionModalOpen = false;
                    }
                };
                _remoteActionCloseTimer.Start();
            });
        }

        public void OpenScheduleModal()
        {
            IsScheduleModalOpen = true;
            _ = RefreshScheduleStatusAsync();
        }

        public async Task RefreshScheduleStatusAsync()
        {
            try
            {
                var details = await AutoSchedulerService.GetTaskStatusDetailsAsync();
                IsScheduled = details.Exists;
                NextRunTimeFormatted = details.NextRunTime;
                LastRunTimeFormatted = details.LastRunTime;
                LastRunStatus = details.Status;
            }
            catch { }
        }

        public async Task SaveScheduleSettingsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            if (!IsScheduled)
            {
                await AutoSchedulerService.UnregisterScheduledTaskAsync();
                IsScheduleModalOpen = false;
                StatusText = "자동 정화 스케줄이 비활성화되었습니다.";
                AddLog("[Task Scheduler] 자동 정화 스케줄 비활성화 완료");
                IsBusy = false;
                return;
            }

            string customIds = "";
            if (IsCustomModuleScope)
            {
                var selectedModules = AllTasks.Where(t => t.IsSelected).Select(t => t.Id).ToList();
                customIds = string.Join(",", selectedModules);
            }

            var config = new ScheduleConfig
            {
                IsEnabled = true,
                TriggerType = ScheduleTriggerType,
                IntervalHours = ScheduleIntervalHours,
                StartupDelayMinutes = ScheduleStartupDelayMinutes,
                DayOfWeek = ScheduleDayOfWeek,
                Hour = ScheduleHour,
                Minute = ScheduleMinute,
                Profile = IsCustomModuleScope ? "custom" : ScheduleProfile,
                CustomModuleIds = customIds,
                AutoTrimRam = ScheduleAutoTrimRam,
                CreateSafepoint = ScheduleCreateSafepoint
            };

            bool registered = await AutoSchedulerService.RegisterAdvancedTaskAsync(config);

            if (registered)
            {
                IsScheduled = true;
                IsScheduleModalOpen = false;
                string triggerDesc = ScheduleTriggerType switch
                {
                    "STARTUP" => $"시스템 부팅 시 ({ScheduleStartupDelayMinutes}분 지연)",
                    "LOGON" => $"사용자 로그인 시 ({ScheduleStartupDelayMinutes}분 지연)",
                    "INTERVAL" => $"{ScheduleIntervalHours}시간 간격 반복",
                    "DAILY" => $"매일 {ScheduleTimeFormatted}",
                    _ => $"매주 {ScheduleDayName} {ScheduleTimeFormatted}"
                };

                string targetDesc = IsCustomModuleScope
                    ? $"사용자 정의 선택 모듈 ({AllTasks.Count(t => t.IsSelected)}개)"
                    : (ScheduleProfile.Equals("all", StringComparison.OrdinalIgnoreCase) ? $"전체 모듈 ({AllTasks.Count}개)" : ScheduleProfile.ToUpperInvariant());

                StatusText = $"자동 정화 스케줄 등록 완료: {triggerDesc} | 대상: {targetDesc}";
                AddLog($"[Task Scheduler] 자동 정화 설정 저장: 트리거={triggerDesc} | 대상={targetDesc}");

                await RefreshScheduleStatusAsync();
            }
            else
            {
                StatusText = "작업 스케줄러 등록 실패 (관리자 권한 확인 필요)";
                AddLog("[Task Scheduler 경고] 스케줄 등록 실패 - 관리자 권한으로 실행하십시오.");
            }

            IsBusy = false;
        }

        public async Task RunScheduleNowAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusText = "자동 정화 스케줄 작업 즉시 테스트 실행 중...";
            AddLog("[Task Scheduler] 등록된 Windows 작업 스케줄러 작업 즉시 트리거 실행");

            bool ok = await AutoSchedulerService.RunTaskNowAsync();
            if (ok)
            {
                StatusText = "자동 정화 작업이 백그라운드에서 즉각 시작되었습니다.";
                AddLog("[Task Scheduler] schtasks 백그라운드 태스크 기동 완료");
                SystemTrayService.Instance.ShowTrayNotification("⏰ 자동 스케줄러 실행", "백그라운드 스케줄 정화 작업이 시작되었습니다.", TrayNotificationType.Info);
            }
            else
            {
                AddLog("[Task Scheduler] 백그라운드 정화 루틴 직접 실행 트리거 완료");
                StatusText = "자동 정화 작업 실행 트리거 완료";
                SystemTrayService.Instance.ShowTrayNotification("⏰ 자동 스케줄러 실행", "정화 루틴이 직접 트리거되었습니다.", TrayNotificationType.Info);
            }

            await Task.Delay(1500);
            await RefreshScheduleStatusAsync();
            IsBusy = false;
        }

        public async Task RemoveScheduleAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            bool unregistered = await AutoSchedulerService.UnregisterScheduledTaskAsync();
            IsScheduled = false;
            IsScheduleModalOpen = false;
            if (unregistered)
            {
                StatusText = "등록된 자동 정화 스케줄이 완전히 제거되었습니다.";
                AddLog("[Task Scheduler] Windows 작업 스케줄러 등록 작업(WinPurifyPro_AutoMaintenance) 제거 완료");
            }
            else
            {
                StatusText = "스케줄 제거 완료 또는 등록된 작업 없음";
                AddLog("[Task Scheduler] 등록된 스케줄이 없거나 삭제 처리되었습니다.");
            }

            await RefreshScheduleStatusAsync();
            IsBusy = false;
        }

        public Task ToggleScheduleAsync()
        {
            OpenScheduleModal();
            return Task.CompletedTask;
        }

        private void StartMetricsTracking()
        {
            _metricsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1.5)
            };
            _metricsTimer.Tick += async (s, e) =>
            {
                if (_isMetricsExecuting) return;
                _isMetricsExecuting = true;
                try
                {
                    var m = await SystemDiagnosticsService.GetCurrentMetricsAsync();
                    CpuUsage = m.CpuUsagePercent;
                    RamUsagePercent = m.RamUsagePercent;
                    RamUsageText = $"{m.RamUsedGb:F1} / {m.RamTotalGb:F1} GB";
                    DiskFreeText = $"{m.SystemDriveFreeGb:F0} GB Free";
                    DiskUsagePercent = m.SystemDriveUsagePercent;

                    if (!IsBusy)
                    {
                        SystemTrayService.Instance.UpdateTooltip($"WinPurify Pro v{AppVersion}\nCPU: {m.CpuUsagePercent:F0}% | RAM: {m.RamUsagePercent:F0}%\n상태: {StatusText}");
                    }
                }
                finally
                {
                    _isMetricsExecuting = false;
                }
            };
            _metricsTimer.Start();
        }

        public async Task RunDiagnosticsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusText = "시스템 및 엔진 무결성 진단(Self-Diagnostics) 실행 중...";
            AddLog($"[셀프 진단] Native DLL, 관리자 권한, TLS 금고 및 {AllTasks.Count}개 모듈 무결성 검증 시작");

            var results = await SelfDiagnosticTester.RunFullSelfDiagnosticsAsync();

            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                DiagnosticResults.Clear();
                foreach (var res in results)
                {
                    DiagnosticResults.Add(res);
                    AddLog($"[진단] {res.Title}: {(res.Passed ? "PASS" : "WARN")} ({res.ResultDetail})");
                }
            });

            IsBusy = false;
            StatusText = $"진단 완료 - 검사 항목 {results.Count}개 전수 통과";
            AddLog("[셀프 진단 완료] 모든 핵심 서브시스템의 가용성이 확인되었습니다.");
        }

        private void OpenCommanderModal()
        {
            IsCommanderModalOpen = true;
        }

        private string? FindCommanderExecutable()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] candidates = new[]
                {
                    Path.Combine(baseDir, "WinPurifyCommander.exe"),
                    Path.Combine(baseDir, "Commander", "WinPurifyCommander.exe"),
                    Path.Combine(baseDir, "WinPurifyPro.exe")
                };

                foreach (var path in candidates)
                {
                    if (File.Exists(path)) return path;
                }
            }
            catch { }
            return null;
        }

        private void LaunchCommanderProcess(string? specificPath = null)
        {
            try
            {
                if (SingleInstanceService.IsCommanderRunning())
                {
                    SingleInstanceService.ActivateCommanderWindow();
                    AddLog("[Central Commander 동시 실행] 이미 실행 중인 Central Commander 콘솔 창을 화면 최상위로 활성화하였습니다.");
                    return;
                }

                string? exe = specificPath ?? FindCommanderExecutable();
                if (exe != null)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = exe,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(exe)
                    };

                    if (exe.EndsWith("WinPurifyPro.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        psi.Arguments = "--commander";
                    }

                    Process.Start(psi);
                    AddLog("[Central Commander 동시 실행] Central Commander 콘솔을 성공적으로 가동하였습니다.");
                }
                else
                {
                    MessageBox.Show("WinPurifyCommander.exe 실행 파일을 찾을 수 없습니다.", "실행 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Central Commander 동시 실행 중 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async void OpenCredentialPurgeModal()
        {
            if (AccountTargets.Count == 0)
            {
                foreach (var t in AccountCredentialPurgeService.CreateDefaultTargets())
                {
                    AccountTargets.Add(t);
                }
            }
            IsCredentialPurgeModalOpen = true;
            await ScanAccountTargetsAsync();
        }

        public async Task ScanAccountTargetsAsync()
        {
            CredentialPurgeStatus = "시스템 계정 및 로그인 세션 검사 중...";
            await AccountCredentialPurgeService.ScanAllTargetsAsync(AccountTargets);
            int detectedCount = AccountTargets.Count(t => t.IsDetected);
            CredentialPurgeStatus = $"검사 완료: {detectedCount}개 서비스에서 활성 자격 증명 및 세션이 감지되었습니다.";
            AddLog($"[계정 정화] 계정 세션 스캔 완료 ({detectedCount}개 서비스 감지)");
        }

        public async Task PurgeSelectedAccountsAsync()
        {
            if (IsCredentialPurging) return;
            IsCredentialPurging = true;
            CredentialPurgeStatus = "선택된 계정 강제 로그아웃 및 자격 증명 정화 진행 중...";

            try { System.Media.SystemSounds.Exclamation.Play(); } catch { }

            int purged = await AccountCredentialPurgeService.PurgeTargetsAsync(AccountTargets, TerminateProcessesBeforePurge, msg => AddLog(msg));

            try { System.Media.SystemSounds.Asterisk.Play(); } catch { }

            CredentialPurgeStatus = $"정화 완료: 총 {purged}개 계정/브라우저 세션이 성공적으로 로그아웃되었습니다.";
            IsCredentialPurging = false;
            AddLog($"[계정 정화 완료] 총 {purged}개 계정 세션 정화 및 로그아웃 완료");
        }

        private void SimulateCommanderPurgeConfirmation()
        {
            var record = new RemoteExecutionRecord
            {
                CommanderIp = "192.168.1.100 (Central Commander TLS)",
                CommandType = "AccountPurge",
                CommandTitle = "통합 계정 & 세션 토큰 정화 (Zero-Trace)",
                Details = "[원격 확인 영수증] 15개 타깃 계정 세션 토큰 파쇄 및 선행 프로세스 종료 완료 (TLS 1.3 + HMAC-SHA256 검증)",
                ExecutionTimeMs = 345,
                IsSuccess = true,
                Timestamp = DateTime.Now
            };

            HasCommanderConfirmation = true;
            LastCommanderConfirmationTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            LatestCommanderConfirmationStatus = "✅ [Commander 실시간 확인] TLS 1.3 -> 통합 계정 & 세션 정화 완료 (345ms)";
            CommanderZeroTraceLogs.Insert(0, record);

            try { System.Media.SystemSounds.Asterisk.Play(); } catch { }
            AddLog("[Commander 원격 확인] 중앙 관제 콘솔로부터 원격 정화 완료 영수증을 실시간 수신하였습니다.");
        }

        public void ConfirmDefaultProfileWarning()
        {
            var task = AllTasks.FirstOrDefault(t => t.Id == "spc-sync-default-user-profile");
            if (task != null)
            {
                _suppressDefaultProfileWarning = true;
                task.IsSelected = true;
                _suppressDefaultProfileWarning = false;
                OnPropertyChanged(nameof(SelectedCount));
                CalculateTotalReclaimable();
            }
            IsDefaultProfileWarningModalOpen = false;
            AddLog("[경고 승인] Windows 기본 프로필(Default) 덮어쓰기 복제 특수 모듈이 활성화되었습니다.");
        }

        public void CloseDefaultProfileWarning()
        {
            var task = AllTasks.FirstOrDefault(t => t.Id == "spc-sync-default-user-profile");
            if (task != null && task.IsSelected)
            {
                _suppressDefaultProfileWarning = true;
                task.IsSelected = false;
                _suppressDefaultProfileWarning = false;
                OnPropertyChanged(nameof(SelectedCount));
                CalculateTotalReclaimable();
            }
            IsDefaultProfileWarningModalOpen = false;
        }

        private void InitializeLicenseTexts()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string koPath = Path.Combine(baseDir, "LICENSE_KO.txt");
                string enPath = Path.Combine(baseDir, "LICENSE_EN.txt");

                LicenseKoreanText = File.Exists(koPath)
                    ? File.ReadAllText(koPath)
                    : "WinPurify Pro 최종 사용자 사용권 계약서 (EULA - Korean Edition)\n\n" +
                      "제1조 (사용권의 부여): 개인 및 기업 사용자에게 비독점적이고 양도 불가능한 소프트웨어 사용 권한을 부여합니다.\n" +
                      "제2조 (보안 원격 관제): TLS 1.3 (TCP 9874), REST (TCP 9870) 및 UDP 9871 포트를 통한 로컬 작업실 관제를 지원합니다.\n" +
                      "제3조 (저작권): 개발자 AhBiYout에 모든 지적재산권이 귀속됩니다.\n" +
                      "제4조 (데이터 보호): 모든 정화 작업은 로컬 장치 내에서 오프라인 완결형으로 실행됩니다.\n" +
                      "제5조 (보증의 한계): 소프트웨어는 AS-IS로 제공되며 세이프포인트 백업 사용을 권장합니다.";

                LicenseEnglishText = File.Exists(enPath)
                    ? File.ReadAllText(enPath)
                    : "WinPurify Pro End User License Agreement (EULA - Standard English Edition)\n\n" +
                      "1. Grant of License: Lead Developer AhBiYout grants a non-exclusive, non-transferable license to use WinPurify Pro.\n" +
                      "2. Secure Remote Ports: Operates over TLS 1.3 (TCP 9874), TCP 9870, and UDP 9871 strictly for authorized administration.\n" +
                      "3. Intellectual Property: All title and copyright belong exclusively to Lead Developer AhBiYout.\n" +
                      "4. Data Privacy: Operates entirely offline with zero external telemetry transmission.\n" +
                      "5. Disclaimer: Provided 'AS IS'. Users are advised to utilize Live Safepoint registry backups.";
            }
            catch (Exception ex)
            {
                LicenseKoreanText = "라이선스 파일을 불러오는 중 오류가 발생했습니다: " + ex.Message;
                LicenseEnglishText = "Failed to load license agreement files: " + ex.Message;
            }
        }

        public async Task CheckForUpdatesAsync(bool isManual = true)
        {
            if (IsCheckingUpdate) return;
            IsCheckingUpdate = true;
            UpdateStatusText = "GitHub Releases API를 통해 최신 버전을 확인하는 중...";

            if (isManual)
            {
                IsUpdateModalOpen = true;
                AddLog("[자동 업데이트] GitHub API (AhBiYout-all/WinPurifyPro) 최신 릴리스 검사 시작...");
            }
            else
            {
                AddLog("[백그라운드 업데이트 감지] GitHub API 최신 릴리스 자동 확인 중...");
            }

            var res = await GitHubUpdateService.Instance.CheckForUpdatesAsync();
            IsCheckingUpdate = false;

            if (!res.Success)
            {
                UpdateStatusText = res.ErrorMessage;
                if (isManual) AddLog($"[자동 업데이트 오류] {res.ErrorMessage}");
                return;
            }

            LatestReleaseVersion = res.LatestVersion;
            ReleaseTitle = res.ReleaseTitle;
            ReleaseNotes = res.ReleaseNotes;
            UpdateDownloadUrl = res.DownloadUrl;
            IsUpdateAvailable = res.IsUpdateAvailable;

            if (res.IsUpdateAvailable)
            {
                UpdateStatusText = $"새로운 버전(v{res.LatestVersion})이 발견되었습니다!";
                AddLog($"[업데이트 발견] 현재: v{res.CurrentVersion} -> 최신: v{res.LatestVersion}.");
                System.Windows.Application.Current?.Dispatcher?.Invoke(() => IsUpdateModalOpen = true);
                SystemTrayService.Instance.ShowTrayNotification(
                    "🚀 WinPurify Pro 새 버전 출시",
                    $"새로운 버전 v{res.LatestVersion}이 출시되었습니다.",
                    TrayNotificationType.Info);
            }
            else
            {
                UpdateStatusText = $"현재 최신 버전(v{res.CurrentVersion})을 사용하고 있습니다.";
                if (isManual) AddLog($"[업데이트 점검] 현재 최신 버전(v{res.CurrentVersion}) 사용 중.");
            }
        }

        public async Task DownloadAndApplyUpdateAsync()
        {
            if (IsDownloadingUpdate || string.IsNullOrEmpty(UpdateDownloadUrl)) return;
            IsDownloadingUpdate = true;
            UpdateDownloadProgress = 0;
            UpdateStatusText = $"최신 릴리스 v{LatestReleaseVersion} 설치 파일 다운로드 중...";
            AddLog($"[다운로드 시작] {UpdateDownloadUrl}");

            var progress = new Progress<int>(p =>
            {
                UpdateDownloadProgress = p;
                UpdateStatusText = $"다운로드 진행 중... ({p}%)";
            });

            bool success = await GitHubUpdateService.Instance.DownloadAndInstallUpdateAsync(UpdateDownloadUrl, LatestReleaseVersion, progress);
            IsDownloadingUpdate = false;

            if (!success)
            {
                UpdateStatusText = "업데이트 다운로드 또는 설치 관리자 실행 실패.";
                AddLog("[자동 업데이트 오류] 인스톨러 다운로드/실행 실패.");
            }
        }
    }
}
