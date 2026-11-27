using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WinPurifyPro.Models;
using WinPurifyPro.Services;

namespace WinPurifyPro.ViewModels
{
    public partial class MainViewModel
    {
        private string _activeWorkspace = "Handover"; // "Handover" | "Modules"
        public string ActiveWorkspace
        {
            get => _activeWorkspace;
            set
            {
                if (_activeWorkspace != value)
                {
                    _activeWorkspace = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsHandoverWorkspace));
                    OnPropertyChanged(nameof(IsModulesWorkspace));
                    OnPropertyChanged(nameof(ActiveModuleCategoryHighlight));
                }
            }
        }

        public bool IsHandoverWorkspace => string.Equals(_activeWorkspace, "Handover", StringComparison.OrdinalIgnoreCase);
        public bool IsModulesWorkspace => string.Equals(_activeWorkspace, "Modules", StringComparison.OrdinalIgnoreCase);

        public ObservableCollection<DetectedUserProfile> HandoverProfiles { get; } = new();
        public ObservableCollection<DetectedSessionArtifact> HandoverArtifacts { get; } = new();
        public ObservableCollection<DetectedSessionArtifact> FilteredHandoverArtifacts { get; } = new();
        public ObservableCollection<HandoverExclusionRule> HandoverExclusions { get; } = new();
        public ObservableCollection<string> HandoverLiveLogs { get; } = new();

        private HandoverScanPhase _handoverPhase = HandoverScanPhase.Idle;
        public HandoverScanPhase HandoverPhase
        {
            get => _handoverPhase;
            set
            {
                if (_handoverPhase != value)
                {
                    _handoverPhase = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsHandoverScanning));
                    OnPropertyChanged(nameof(IsHandoverPurging));
                    OnPropertyChanged(nameof(IsHandoverBusy));
                }
            }
        }

        public bool IsHandoverScanning => _handoverPhase == HandoverScanPhase.Scanning;
        public bool IsHandoverPurging => _handoverPhase == HandoverScanPhase.Purging;
        public bool IsHandoverBusy => IsHandoverScanning || IsHandoverPurging;

        private HandoverPresetMode _handoverPreset = HandoverPresetMode.PreviousUsersAndSessions;
        public HandoverPresetMode HandoverPreset
        {
            get => _handoverPreset;
            set
            {
                if (_handoverPreset != value)
                {
                    _handoverPreset = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _handoverDomainFilter = "All";
        public string HandoverDomainFilter
        {
            get => _handoverDomainFilter;
            set
            {
                if (_handoverDomainFilter != value)
                {
                    _handoverDomainFilter = value;
                    OnPropertyChanged();
                    FilterHandoverArtifacts();
                }
            }
        }

        private string _handoverSearchQuery = string.Empty;
        public string HandoverSearchQuery
        {
            get => _handoverSearchQuery;
            set
            {
                if (_handoverSearchQuery != value)
                {
                    _handoverSearchQuery = value;
                    OnPropertyChanged();
                    FilterHandoverArtifacts();
                }
            }
        }

        private string _handoverStatusBanner = "앱 기동 중: 이전 사용자 프로필 및 로그인 세션을 자동 탐지하고 있습니다...";
        public string HandoverStatusBanner
        {
            get => _handoverStatusBanner;
            set { if (_handoverStatusBanner != value) { _handoverStatusBanner = value; OnPropertyChanged(); } }
        }

        private int _handoverProgressPercent;
        public int HandoverProgressPercent
        {
            get => _handoverProgressPercent;
            set { if (_handoverProgressPercent != value) { _handoverProgressPercent = value; OnPropertyChanged(); } }
        }

        private string _handoverCurrentStepText = "대기 중";
        public string HandoverCurrentStepText
        {
            get => _handoverCurrentStepText;
            set { if (_handoverCurrentStepText != value) { _handoverCurrentStepText = value; OnPropertyChanged(); } }
        }

        private bool _isHandoverConfirmModalOpen;
        public bool IsHandoverConfirmModalOpen
        {
            get => _isHandoverConfirmModalOpen;
            set { if (_isHandoverConfirmModalOpen != value) { _isHandoverConfirmModalOpen = value; OnPropertyChanged(); } }
        }

        private bool _handoverCreateSafepoint = true;
        public bool HandoverCreateSafepoint
        {
            get => _handoverCreateSafepoint;
            set { if (_handoverCreateSafepoint != value) { _handoverCreateSafepoint = value; OnPropertyChanged(); } }
        }

        private bool _handoverTerminateProcesses = true;
        public bool HandoverTerminateProcesses
        {
            get => _handoverTerminateProcesses;
            set { if (_handoverTerminateProcesses != value) { _handoverTerminateProcesses = value; OnPropertyChanged(); } }
        }

        private bool _handoverSchedulePendingReboot = true;
        public bool HandoverSchedulePendingReboot
        {
            get => _handoverSchedulePendingReboot;
            set { if (_handoverSchedulePendingReboot != value) { _handoverSchedulePendingReboot = value; OnPropertyChanged(); } }
        }

        private bool _hasHandoverVerification;
        public bool HasHandoverVerification
        {
            get => _hasHandoverVerification;
            set { if (_hasHandoverVerification != value) { _hasHandoverVerification = value; OnPropertyChanged(); } }
        }

        private bool _isHandoverCleanVerified = true;
        public bool IsHandoverCleanVerified
        {
            get => _isHandoverCleanVerified;
            set
            {
                if (_isHandoverCleanVerified != value)
                {
                    _isHandoverCleanVerified = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HandoverVerificationTitle));
                }
            }
        }

        private int _handoverPendingRebootCount;
        public int HandoverPendingRebootCount
        {
            get => _handoverPendingRebootCount;
            set
            {
                if (_handoverPendingRebootCount != value)
                {
                    _handoverPendingRebootCount = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasPendingRebootItems));
                    OnPropertyChanged(nameof(HandoverVerificationTitle));
                }
            }
        }

        public bool HasPendingRebootItems => _handoverPendingRebootCount > 0;

        public string HandoverVerificationTitle => IsHandoverCleanVerified
            ? "✅ 클린 인수인계 검증 완료: "
            : (HasPendingRebootItems
                ? "🕒 재부팅 자동 소거 예약 완료: "
                : "⚠️ 일부 잔여 항목 확인 필요: ");

        private string _handoverVerificationMessage = string.Empty;
        public string HandoverVerificationMessage
        {
            get => _handoverVerificationMessage;
            set { if (_handoverVerificationMessage != value) { _handoverVerificationMessage = value; OnPropertyChanged(); } }
        }

        public HandoverVerificationSummary? LastHandoverSummary { get; private set; }

        private DetectedSessionArtifact? _selectedInspectArtifact;
        public DetectedSessionArtifact? SelectedInspectArtifact
        {
            get => _selectedInspectArtifact;
            set
            {
                if (_selectedInspectArtifact != value)
                {
                    _selectedInspectArtifact = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsHandoverDetailOpen));
                    OnPropertyChanged(nameof(SelectedInspectArtifactPathsText));
                }
            }
        }

        public bool IsHandoverDetailOpen => _selectedInspectArtifact != null;

        public string SelectedInspectArtifactPathsText => _selectedInspectArtifact == null
            ? string.Empty
            : (_selectedInspectArtifact.MatchedPaths.Count > 0
                ? string.Join(Environment.NewLine, _selectedInspectArtifact.MatchedPaths)
                : _selectedInspectArtifact.TargetPathsSummary);

        private bool _isHandoverExclusionModalOpen;
        public bool IsHandoverExclusionModalOpen
        {
            get => _isHandoverExclusionModalOpen;
            set { if (_isHandoverExclusionModalOpen != value) { _isHandoverExclusionModalOpen = value; OnPropertyChanged(); } }
        }

        private string _newExclusionPathInput = string.Empty;
        public string NewExclusionPathInput
        {
            get => _newExclusionPathInput;
            set { if (_newExclusionPathInput != value) { _newExclusionPathInput = value; OnPropertyChanged(); } }
        }

        private string _newExclusionLabelInput = string.Empty;
        public string NewExclusionLabelInput
        {
            get => _newExclusionLabelInput;
            set { if (_newExclusionLabelInput != value) { _newExclusionLabelInput = value; OnPropertyChanged(); } }
        }

        public int SelectedHandoverProfilesCount => HandoverProfiles.Count(p => p.IsSelected);
        public int DetectedPreviousProfilesCount => HandoverProfiles.Count(p => !p.IsSystemProtected && !p.IsCurrentSession);
        public int SelectedHandoverArtifactsCount => HandoverArtifacts.Count(a => a.IsSelected);
        public int DetectedSessionArtifactsCount => HandoverArtifacts.Count(a => a.DetectedCount > 0 && !a.IsExcludedByWhitelist);
        public string HandoverEstimatedSpaceText
        {
            get
            {
                long total = HandoverProfiles.Where(p => p.IsSelected).Sum(p => p.EstimatedBytes)
                           + HandoverArtifacts.Where(a => a.IsSelected).Sum(a => a.EstimatedBytes);
                return DetectedUserProfile.FormatBytes(total);
            }
        }

        public ICommand? SwitchWorkspaceCommand { get; private set; }
        public ICommand? ScanHandoverCommand { get; private set; }
        public ICommand? ApplyHandoverPresetCommand { get; private set; }
        public ICommand? SetHandoverDomainFilterCommand { get; private set; }
        public ICommand? SelectAllFilteredArtifactsCommand { get; private set; }
        public ICommand? DeselectAllFilteredArtifactsCommand { get; private set; }
        public ICommand? InspectHandoverArtifactCommand { get; private set; }
        public ICommand? CloseHandoverDetailCommand { get; private set; }
        public ICommand? OpenHandoverExclusionModalCommand { get; private set; }
        public ICommand? CloseHandoverExclusionModalCommand { get; private set; }
        public ICommand? AddArtifactExclusionCommand { get; private set; }
        public ICommand? AddPathExclusionCommand { get; private set; }
        public ICommand? RemoveExclusionRuleCommand { get; private set; }
        public ICommand? OpenHandoverConfirmModalCommand { get; private set; }
        public ICommand? CloseHandoverConfirmModalCommand { get; private set; }
        public ICommand? ExecuteHandoverPurgeCommand { get; private set; }
        public ICommand? ExportHandoverCertificateCommand { get; private set; }
        public ICommand? RebootNowForPendingPurgeCommand { get; private set; }

        private void InitializeHandoverWorkspace()
        {
            ReloadExclusionCollection();

            SwitchWorkspaceCommand = new RelayCommand(param =>
            {
                string target = param?.ToString() ?? "Handover";
                ActiveWorkspace = target;
            });

            ScanHandoverCommand = new RelayCommand(async _ => await ScanHandoverEnvironmentAsync(), _ => !IsHandoverBusy);

            ApplyHandoverPresetCommand = new RelayCommand(param =>
            {
                string modeStr = param?.ToString() ?? "PreviousUsersAndSessions";
                if (Enum.TryParse<HandoverPresetMode>(modeStr, true, out var mode))
                {
                    HandoverPreset = mode;
                    FreshHandoverService.ApplyHandoverPreset(HandoverProfiles, HandoverArtifacts, mode);
                    UpdateHandoverCounters();
                    AddLog($"[인수인계 프리셋] '{mode}' 정책이 적용되었습니다.");
                }
            });

            SetHandoverDomainFilterCommand = new RelayCommand(param =>
            {
                HandoverDomainFilter = param?.ToString() ?? "All";
            });

            SelectAllFilteredArtifactsCommand = new RelayCommand(_ =>
            {
                foreach (var art in FilteredHandoverArtifacts)
                {
                    if (art.IsSelectable) art.IsSelected = true;
                }
                UpdateHandoverCounters();
            }, _ => !IsHandoverBusy);

            DeselectAllFilteredArtifactsCommand = new RelayCommand(_ =>
            {
                foreach (var art in FilteredHandoverArtifacts)
                {
                    art.IsSelected = false;
                }
                UpdateHandoverCounters();
            }, _ => !IsHandoverBusy);

            InspectHandoverArtifactCommand = new RelayCommand(param =>
            {
                if (param is DetectedSessionArtifact art)
                {
                    SelectedInspectArtifact = art;
                }
            });

            CloseHandoverDetailCommand = new RelayCommand(_ => SelectedInspectArtifact = null);

            OpenHandoverExclusionModalCommand = new RelayCommand(_ =>
            {
                ReloadExclusionCollection();
                IsHandoverExclusionModalOpen = true;
            });

            CloseHandoverExclusionModalCommand = new RelayCommand(_ => IsHandoverExclusionModalOpen = false);

            AddArtifactExclusionCommand = new RelayCommand(async param =>
            {
                if (param is DetectedSessionArtifact art)
                {
                    HandoverPolicyService.AddExclusion("ArtifactId", art.ArtifactId, art.Title);
                    ReloadExclusionCollection();
                    SelectedInspectArtifact = null;
                    AddLog($"[보존 예외 등록] '{art.Title}' 항목이 영구 보존 목록에 추가되었습니다.");
                    await ScanHandoverEnvironmentAsync();
                }
            });

            AddPathExclusionCommand = new RelayCommand(async _ =>
            {
                if (string.IsNullOrWhiteSpace(NewExclusionPathInput)) return;
                string label = string.IsNullOrWhiteSpace(NewExclusionLabelInput) ? NewExclusionPathInput.Trim() : NewExclusionLabelInput.Trim();
                HandoverPolicyService.AddExclusion("PathPrefix", NewExclusionPathInput.Trim(), label);
                NewExclusionPathInput = string.Empty;
                NewExclusionLabelInput = string.Empty;
                ReloadExclusionCollection();
                AddLog($"[보존 예외 등록] 경로 예외 '{label}'이(가) 추가되었습니다.");
                await ScanHandoverEnvironmentAsync();
            });

            RemoveExclusionRuleCommand = new RelayCommand(async param =>
            {
                if (param is string id && !string.IsNullOrWhiteSpace(id))
                {
                    HandoverPolicyService.RemoveExclusion(id);
                    ReloadExclusionCollection();
                    AddLog("[보존 예외 해제] 선택한 예외 규칙을 제거했습니다.");
                    await ScanHandoverEnvironmentAsync();
                }
            });

            OpenHandoverConfirmModalCommand = new RelayCommand(_ =>
            {
                IsHandoverConfirmModalOpen = true;
            }, _ => !IsHandoverBusy && (SelectedHandoverProfilesCount > 0 || SelectedHandoverArtifactsCount > 0));

            CloseHandoverConfirmModalCommand = new RelayCommand(_ =>
            {
                IsHandoverConfirmModalOpen = false;
            });

            ExecuteHandoverPurgeCommand = new RelayCommand(async _ => await ExecuteHandoverPurgeFromUiAsync(), _ => !IsHandoverBusy);

            ExportHandoverCertificateCommand = new RelayCommand(_ => ExportHandoverCertificateDialog(), _ => LastHandoverSummary != null);

            RebootNowForPendingPurgeCommand = new RelayCommand(_ =>
            {
                var confirm = MessageBox.Show(
                    "잠겨 있던 잔여 파일 정리를 완료하기 위해 지금 시스템을 안전하게 재부팅하시겠습니까?\n(진행 중인 다른 작업은 먼저 저장해 주세요.)",
                    "WinPurify Pro - 안전 재부팅 확인",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes)
                {
                    AddLog("[인수인계 마무리] PendingFileRenameOperations 완료를 위해 3초 후 안전 재부팅을 시작합니다.");
                    SystemPowerService.ScheduleForceReboot(3, msg => AddLog(msg));
                }
            });

            // 앱 실행 즉시 비파괴 자동 탐지 시작 (디자이너 환경 제외)
            if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                _ = Task.Run(async () =>
                {
                    await Task.Delay(350);
                    await ScanHandoverEnvironmentAsync();
                });
            }
        }

        private void ReloadExclusionCollection()
        {
            var rules = HandoverPolicyService.LoadExclusions();
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                HandoverExclusions.Clear();
                foreach (var r in rules) HandoverExclusions.Add(r);
            });
        }

        public async Task ScanHandoverEnvironmentAsync()
        {
            if (IsHandoverBusy) return;

            Application.Current?.Dispatcher?.Invoke(() =>
            {
                HandoverPhase = HandoverScanPhase.Scanning;
                HasHandoverVerification = false;
                HandoverPendingRebootCount = 0;
                HandoverStatusBanner = "이전 Windows 사용자 프로필·앱 데이터·브라우저·개발 도구 로그인 세션을 스캔하는 중...";
                StatusText = "클린 인수인계 환경 자동 스캔 중...";
            });

            try
            {
                var profiles = await FreshHandoverService.ScanUserProfilesAsync(msg => AddLog(msg));
                var artifacts = await FreshHandoverService.ScanSessionArtifactsAsync(profiles, msg => AddLog(msg));
                FreshHandoverService.ApplyHandoverPreset(profiles, artifacts, HandoverPreset);

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    HandoverProfiles.Clear();
                    foreach (var p in profiles)
                    {
                        p.PropertyChanged += (_, _) => UpdateHandoverCounters();
                        HandoverProfiles.Add(p);
                    }

                    HandoverArtifacts.Clear();
                    foreach (var a in artifacts)
                    {
                        a.PropertyChanged += (_, _) => UpdateHandoverCounters();
                        HandoverArtifacts.Add(a);
                    }

                    FilterHandoverArtifacts();
                    UpdateHandoverCounters();

                    HandoverPhase = HandoverScanPhase.Ready;
                    HandoverStatusBanner = $"스캔 완료: 이전 사용자 {DetectedPreviousProfilesCount}개 · 활성 세션 영역 {DetectedSessionArtifactsCount}건 탐지됨 (총 {HandoverEstimatedSpaceText})";
                    StatusText = HandoverStatusBanner;
                    AddLog($"[인수인계 스캔 완료] {HandoverStatusBanner}");
                });
            }
            catch (Exception ex)
            {
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    HandoverPhase = HandoverScanPhase.Ready;
                    HandoverStatusBanner = $"스캔 중 예외 발생: {ex.Message}";
                    AddLog($"[인수인계 스캔 오류] {ex.Message}");
                });
            }
        }

        private async Task ExecuteHandoverPurgeFromUiAsync()
        {
            if (IsHandoverBusy) return;
            IsHandoverConfirmModalOpen = false;
            SelectedInspectArtifact = null;

            HandoverPhase = HandoverScanPhase.Purging;
            HandoverProgressPercent = 5;
            HandoverCurrentStepText = "Step 1/4: 초기화 파이프라인 준비 중...";
            HandoverLiveLogs.Clear();
            HandoverStatusBanner = "잠금 프로세스 종료 및 이전 사용자 데이터·로그인 세션 완전 소거 실행 중...";
            StatusText = "새 사용자 클린 환경 초기화 실행 중...";

            var progress = new Progress<HandoverProgressReport>(rep =>
            {
                HandoverProgressPercent = rep.Percent;
                HandoverCurrentStepText = $"Step {rep.StepIndex}/4 · {rep.StepTitle} ({rep.CurrentItemLabel})";
                if (!string.IsNullOrWhiteSpace(rep.LogLine))
                {
                    HandoverLiveLogs.Insert(0, rep.LogLine);
                    while (HandoverLiveLogs.Count > 6) HandoverLiveLogs.RemoveAt(HandoverLiveLogs.Count - 1);
                }
            });

            try
            {
                var summary = await FreshHandoverService.ExecuteHandoverPurgeAsync(
                    HandoverProfiles.ToList(),
                    HandoverArtifacts.ToList(),
                    terminateProcessesBeforePurge: HandoverTerminateProcesses,
                    createSafepoint: HandoverCreateSafepoint,
                    logger: msg => AddLog(msg),
                    schedulePendingReboot: HandoverSchedulePendingReboot,
                    presetUsed: HandoverPreset.ToString(),
                    progress: progress
                );

                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    if (HandoverCreateSafepoint)
                    {
                        LastSafepoint = LiveSafepointService.GetLatestSafepoint();
                    }

                    LastHandoverSummary = summary;
                    UpdateHandoverCounters();
                    FilterHandoverArtifacts();
                    HandoverPendingRebootCount = summary.PendingRebootCount;
                    IsHandoverCleanVerified = summary.IsCleanHandoverVerified;
                    HasHandoverVerification = true;
                    HandoverVerificationMessage = summary.SummaryMessage;
                    HandoverPhase = HandoverScanPhase.Verified;
                    HandoverStatusBanner = summary.SummaryMessage;
                    StatusText = summary.IsCleanHandoverVerified
                        ? "새 사용자 환경 초기화 및 검증 완료"
                        : (summary.HasPendingReboot
                            ? "초기화 및 재부팅 자동 소거 예약 완료"
                            : "인수인계 정화 완료 (일부 잔여 항목 확인 필요)");
                    CommandManager.InvalidateRequerySuggested();

                    SystemTrayService.Instance.ShowTrayNotification(
                        summary.IsCleanHandoverVerified ? "✨ 새 사용자 환경 초기화 완료" : (summary.HasPendingReboot ? "🕒 재부팅 자동 삭제 예약 완료" : "⚠️ 인수인계 정화 일부 확인 필요"),
                        summary.SummaryMessage,
                        summary.IsCleanHandoverVerified ? TrayNotificationType.Success : TrayNotificationType.Warning);
                });
            }
            catch (Exception ex)
            {
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    HandoverPhase = HandoverScanPhase.Ready;
                    HandoverStatusBanner = $"초기화 중 오류: {ex.Message}";
                    AddLog($"[인수인계 초기화 오류] {ex.Message}");
                });
            }
        }

        private void ExportHandoverCertificateDialog()
        {
            if (LastHandoverSummary == null) return;
            try
            {
                var dlg = new SaveFileDialog
                {
                    Title = "클린 환경 인수인계 검증 인증서 저장",
                    FileName = $"WinPurify_Handover_Report_{DateTime.Now:yyyyMMdd_HHmmss}",
                    DefaultExt = ".html",
                    Filter = "HTML 인증서 문서 (*.html)|*.html|JSON 감사 데이터 (*.json)|*.json"
                };

                if (dlg.ShowDialog() == true)
                {
                    bool isJson = string.Equals(Path.GetExtension(dlg.FileName), ".json", StringComparison.OrdinalIgnoreCase);
                    string content = isJson
                        ? HandoverPolicyService.BuildVerificationCertificateJson(LastHandoverSummary)
                        : HandoverPolicyService.BuildVerificationCertificateHtml(LastHandoverSummary);
                    File.WriteAllText(dlg.FileName, content, Encoding.UTF8);
                    AddLog($"[인수인계 리포트 저장] 검증 인증서가 저장되었습니다: {dlg.FileName}");
                }
            }
            catch (Exception ex)
            {
                AddLog($"[인수인계 리포트 오류] 저장 실패: {ex.Message}");
            }
        }

        private void FilterHandoverArtifacts()
        {
            FilteredHandoverArtifacts.Clear();
            string q = HandoverSearchQuery?.Trim().ToLowerInvariant() ?? string.Empty;
            string domain = HandoverDomainFilter ?? "All";

            foreach (var a in HandoverArtifacts)
            {
                if (!string.Equals(domain, "All", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(a.DomainCategory.ToString(), domain, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(q) ||
                    a.Title.ToLowerInvariant().Contains(q) ||
                    a.CategoryLabel.ToLowerInvariant().Contains(q) ||
                    a.TargetPathsSummary.ToLowerInvariant().Contains(q))
                {
                    FilteredHandoverArtifacts.Add(a);
                }
            }
        }

        private void UpdateHandoverCounters()
        {
            OnPropertyChanged(nameof(SelectedHandoverProfilesCount));
            OnPropertyChanged(nameof(DetectedPreviousProfilesCount));
            OnPropertyChanged(nameof(SelectedHandoverArtifactsCount));
            OnPropertyChanged(nameof(DetectedSessionArtifactsCount));
            OnPropertyChanged(nameof(HandoverEstimatedSpaceText));
            Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
            {
                CommandManager.InvalidateRequerySuggested();
            }));
        }
    }
}
