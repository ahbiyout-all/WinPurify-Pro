using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WinPurifyPro.Models;
using WinPurifyPro.Services;

namespace WinPurifyPro.ViewModels
{
    public partial class MainViewModel : IMainViewModel, INotifyPropertyChanged
    {
        public string AppVersion => AppEnvironment.AppVersion;
        public string ProcessInstanceId => AppEnvironment.ProcessInstanceId;

        public string WindowTitle => $"WinPurify Pro v{AppVersion} - Clean Handover & Fresh Environment Suite";
        public string AppHeaderSubtitle => $"v{AppVersion} 이전 사용자·앱 세션 완전 초기화 ({ProcessInstanceId})";

        public ObservableCollection<OptimizationTask> AllTasks { get; set; } = new();
        public ObservableCollection<OptimizationTask> FilteredTasks { get; set; } = new();
        public ObservableCollection<string> LogConsole { get; set; } = new();
        public ObservableCollection<DiagnosticCheckItem> DiagnosticResults { get; set; } = new();

        private DispatcherTimer? _metricsTimer;
        private bool _isMetricsExecuting = false;

        // Hardware Metrics
        private double _cpuUsage;
        public double CpuUsage
        {
            get => _cpuUsage;
            set { if (_cpuUsage != value) { _cpuUsage = value; OnPropertyChanged(); } }
        }

        private double _ramUsagePercent;
        public double RamUsagePercent
        {
            get => _ramUsagePercent;
            set { if (_ramUsagePercent != value) { _ramUsagePercent = value; OnPropertyChanged(); } }
        }

        private string _ramUsageText = "0 / 0 GB";
        public string RamUsageText
        {
            get => _ramUsageText;
            set { if (_ramUsageText != value) { _ramUsageText = value; OnPropertyChanged(); } }
        }

        private string _diskFreeText = "0 GB Free";
        public string DiskFreeText
        {
            get => _diskFreeText;
            set { if (_diskFreeText != value) { _diskFreeText = value; OnPropertyChanged(); } }
        }

        private double _diskUsagePercent = 35.0;
        public double DiskUsagePercent
        {
            get => _diskUsagePercent;
            set { if (_diskUsagePercent != value) { _diskUsagePercent = value; OnPropertyChanged(); } }
        }

        private bool _isScheduled;
        public bool IsScheduled
        {
            get => _isScheduled;
            set { if (_isScheduled != value) { _isScheduled = value; OnPropertyChanged(); } }
        }

        // Schedule Configuration Properties (User Customization)
        private bool _isScheduleModalOpen;
        public bool IsScheduleModalOpen
        {
            get => _isScheduleModalOpen;
            set { if (_isScheduleModalOpen != value) { _isScheduleModalOpen = value; OnPropertyChanged(); } }
        }

        // System Tray & Notification Center Properties
        private bool _isTraySettingsModalOpen;
        public bool IsTraySettingsModalOpen
        {
            get => _isTraySettingsModalOpen;
            set { if (_isTraySettingsModalOpen != value) { _isTraySettingsModalOpen = value; OnPropertyChanged(); } }
        }

        public bool MinimizeToTray
        {
            get => SystemTrayService.Instance.MinimizeToTray;
            set
            {
                if (SystemTrayService.Instance.MinimizeToTray != value)
                {
                    SystemTrayService.Instance.MinimizeToTray = value;
                    OnPropertyChanged();
                    AddLog(value ? "[트레이 설정] 창 최소화 시 시스템 트레이로 축소 활성화" : "[트레이 설정] 창 최소화 시 트레이 축소 비활성화");
                }
            }
        }

        public bool CloseToTray
        {
            get => SystemTrayService.Instance.CloseToTray;
            set
            {
                if (SystemTrayService.Instance.CloseToTray != value)
                {
                    SystemTrayService.Instance.CloseToTray = value;
                    OnPropertyChanged();
                    AddLog(value ? "[트레이 설정] 창 닫기(X) 시 백그라운드 트레이 상주 활성화" : "[트레이 설정] 창 닫기 시 즉시 종료 활성화");
                }
            }
        }

        public bool TrayNotificationsEnabled
        {
            get => SystemTrayService.Instance.NotificationsEnabled;
            set
            {
                if (SystemTrayService.Instance.NotificationsEnabled != value)
                {
                    SystemTrayService.Instance.NotificationsEnabled = value;
                    OnPropertyChanged();
                    AddLog(value ? "[트레이 설정] 시스템 트레이 토스트/풍선 알림 활성화" : "[트레이 설정] 시스템 트레이 알림 비활성화");
                }
            }
        }

        public bool TrayMilestoneNotifications
        {
            get => SystemTrayService.Instance.MilestoneNotificationsEnabled;
            set
            {
                if (SystemTrayService.Instance.MilestoneNotificationsEnabled != value)
                {
                    SystemTrayService.Instance.MilestoneNotificationsEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool TrayLiveProgressTooltip
        {
            get => SystemTrayService.Instance.LiveTooltipProgressEnabled;
            set
            {
                if (SystemTrayService.Instance.LiveTooltipProgressEnabled != value)
                {
                    SystemTrayService.Instance.LiveTooltipProgressEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        // Multi-PC Central Commander Modal
        private bool _isCommanderModalOpen;
        public bool IsCommanderModalOpen
        {
            get => _isCommanderModalOpen;
            set { if (_isCommanderModalOpen != value) { _isCommanderModalOpen = value; OnPropertyChanged(); } }
        }

        // Multi-PC Central Commander Menu Visibility (일반 사용자 오동작 방지를 위해 메인 메뉴에 기본 미노출)
        private bool _isCommanderMenuVisible;
        public bool IsCommanderMenuVisible
        {
            get => _isCommanderMenuVisible;
            set { if (_isCommanderMenuVisible != value) { _isCommanderMenuVisible = value; OnPropertyChanged(); } }
        }

        // Remote Action Modal (Central Commander Incoming Command HUD & Animation)
        private bool _isRemoteActionModalOpen;
        public bool IsRemoteActionModalOpen
        {
            get => _isRemoteActionModalOpen;
            set { if (_isRemoteActionModalOpen != value) { _isRemoteActionModalOpen = value; OnPropertyChanged(); } }
        }

        // License Agreement Modal (Korean & English Dual Support)
        private bool _isLicenseModalOpen;
        public bool IsLicenseModalOpen
        {
            get => _isLicenseModalOpen;
            set { if (_isLicenseModalOpen != value) { _isLicenseModalOpen = value; OnPropertyChanged(); } }
        }

        private string _licenseLanguage = "KO"; // "KO" or "EN"
        public string LicenseLanguage
        {
            get => _licenseLanguage;
            set
            {
                if (_licenseLanguage != value)
                {
                    _licenseLanguage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentLicenseText));
                    OnPropertyChanged(nameof(IsLicenseKorean));
                    OnPropertyChanged(nameof(IsLicenseEnglish));
                }
            }
        }

        public bool IsLicenseKorean => _licenseLanguage == "KO";
        public bool IsLicenseEnglish => _licenseLanguage == "EN";

        public string CurrentLicenseText => _licenseLanguage == "KO" ? LicenseKoreanText : LicenseEnglishText;

        public string LicenseKoreanText { get; private set; } = "";
        public string LicenseEnglishText { get; private set; } = "";

        private string _remoteActionTitle = "🚨 중앙 관제(Commander) 원격 명령 동작 중";
        public string RemoteActionTitle
        {
            get => _remoteActionTitle;
            set { if (_remoteActionTitle != value) { _remoteActionTitle = value; OnPropertyChanged(); } }
        }

        private string _remoteActionCommandName = "";
        public string RemoteActionCommandName
        {
            get => _remoteActionCommandName;
            set { if (_remoteActionCommandName != value) { _remoteActionCommandName = value; OnPropertyChanged(); } }
        }

        private string _remoteActionSourceIp = "";
        public string RemoteActionSourceIp
        {
            get => _remoteActionSourceIp;
            set { if (_remoteActionSourceIp != value) { _remoteActionSourceIp = value; OnPropertyChanged(); } }
        }

        private string _remoteActionStatus = "";
        public string RemoteActionStatus
        {
            get => _remoteActionStatus;
            set { if (_remoteActionStatus != value) { _remoteActionStatus = value; OnPropertyChanged(); } }
        }

        private string _remoteActionDetails = "";
        public string RemoteActionDetails
        {
            get => _remoteActionDetails;
            set { if (_remoteActionDetails != value) { _remoteActionDetails = value; OnPropertyChanged(); } }
        }

        private bool _isRemoteActionActive = false;
        public bool IsRemoteActionActive
        {
            get => _isRemoteActionActive;
            set { if (_isRemoteActionActive != value) { _isRemoteActionActive = value; OnPropertyChanged(); } }
        }

        private int _remoteActionAutoCloseSeconds = 5;
        public int RemoteActionAutoCloseSeconds
        {
            get => _remoteActionAutoCloseSeconds;
            set { if (_remoteActionAutoCloseSeconds != value) { _remoteActionAutoCloseSeconds = value; OnPropertyChanged(); } }
        }

        public ObservableCollection<RemoteExecutionRecord> RemoteExecutionHistory { get; } = new();

        // Account & Credential Purge Modal Properties
        private bool _isCredentialPurgeModalOpen;
        public bool IsCredentialPurgeModalOpen
        {
            get => _isCredentialPurgeModalOpen;
            set { if (_isCredentialPurgeModalOpen != value) { _isCredentialPurgeModalOpen = value; OnPropertyChanged(); } }
        }

        // Default Profile Overwrite Warning Modal Properties
        private bool _isDefaultProfileWarningModalOpen;
        public bool IsDefaultProfileWarningModalOpen
        {
            get => _isDefaultProfileWarningModalOpen;
            set { if (_isDefaultProfileWarningModalOpen != value) { _isDefaultProfileWarningModalOpen = value; OnPropertyChanged(); } }
        }
        private bool _suppressDefaultProfileWarning = false;

        private bool _isCredentialPurging;
        public bool IsCredentialPurging
        {
            get => _isCredentialPurging;
            set { if (_isCredentialPurging != value) { _isCredentialPurging = value; OnPropertyChanged(); } }
        }

        private string _credentialPurgeStatus = "스캔 완료: 선택된 계정을 안전하게 정화할 수 있습니다.";
        public string CredentialPurgeStatus
        {
            get => _credentialPurgeStatus;
            set { if (_credentialPurgeStatus != value) { _credentialPurgeStatus = value; OnPropertyChanged(); } }
        }

        private bool _terminateProcessesBeforePurge = true;
        public bool TerminateProcessesBeforePurge
        {
            get => _terminateProcessesBeforePurge;
            set { if (_terminateProcessesBeforePurge != value) { _terminateProcessesBeforePurge = value; OnPropertyChanged(); } }
        }

        public ObservableCollection<AccountSessionTarget> AccountTargets { get; } = new();
        public ObservableCollection<RemoteExecutionRecord> CommanderZeroTraceLogs { get; } = new();

        private string _latestCommanderConfirmationStatus = "🟢 원격 관제 채널 대기 중 (TLS 9874 / HMAC-SHA256 보호됨)";
        public string LatestCommanderConfirmationStatus
        {
            get => _latestCommanderConfirmationStatus;
            set { if (_latestCommanderConfirmationStatus != value) { _latestCommanderConfirmationStatus = value; OnPropertyChanged(); } }
        }

        private bool _hasCommanderConfirmation = false;
        public bool HasCommanderConfirmation
        {
            get => _hasCommanderConfirmation;
            set { if (_hasCommanderConfirmation != value) { _hasCommanderConfirmation = value; OnPropertyChanged(); } }
        }

        private string _lastCommanderConfirmationTime = "-";
        public string LastCommanderConfirmationTime
        {
            get => _lastCommanderConfirmationTime;
            set { if (_lastCommanderConfirmationTime != value) { _lastCommanderConfirmationTime = value; OnPropertyChanged(); } }
        }

        public ICommand OpenCredentialPurgeModalCommand { get; }
        public ICommand CloseCredentialPurgeModalCommand { get; }
        public ICommand ScanAccountTargetsCommand { get; }
        public ICommand PurgeSelectedAccountsCommand { get; }
        public ICommand SelectAllAccountTargetsCommand { get; }
        public ICommand DeselectAllAccountTargetsCommand { get; }
        public ICommand ApplyAccountScenarioCommand { get; }
        public ICommand SimulateCommanderConfirmationCommand { get; }
        public ICommand ClearCommanderZeroTraceLogsCommand { get; }

        public ICommand CloseRemoteActionModalCommand { get; }
        public ICommand ClearRemoteExecutionHistoryCommand { get; }

        public ICommand ConfirmDefaultProfileWarningCommand { get; }
        public ICommand CloseDefaultProfileWarningCommand { get; }

        public ICommand OpenLicenseModalCommand { get; }
        public ICommand CloseLicenseModalCommand { get; }
        public ICommand SwitchLicenseLanguageCommand { get; }

        // System Tray & Notification Center Commands
        public ICommand OpenTraySettingsModalCommand { get; }
        public ICommand CloseTraySettingsModalCommand { get; }
        public ICommand SendTestTrayNotificationCommand { get; }
        public ICommand MinimizeToTrayCommand { get; }

        // GitHub Releases Live Auto-Update Properties & Commands
        private bool _isUpdateModalOpen;
        public bool IsUpdateModalOpen
        {
            get => _isUpdateModalOpen;
            set { if (_isUpdateModalOpen != value) { _isUpdateModalOpen = value; OnPropertyChanged(); } }
        }

        private bool _isCheckingUpdate;
        public bool IsCheckingUpdate
        {
            get => _isCheckingUpdate;
            set { if (_isCheckingUpdate != value) { _isCheckingUpdate = value; OnPropertyChanged(); } }
        }

        private bool _isUpdateAvailable;
        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set { if (_isUpdateAvailable != value) { _isUpdateAvailable = value; OnPropertyChanged(); } }
        }

        private string _latestReleaseVersion = "";
        public string LatestReleaseVersion
        {
            get => _latestReleaseVersion;
            set { if (_latestReleaseVersion != value) { _latestReleaseVersion = value; OnPropertyChanged(); } }
        }

        private string _releaseTitle = "";
        public string ReleaseTitle
        {
            get => _releaseTitle;
            set { if (_releaseTitle != value) { _releaseTitle = value; OnPropertyChanged(); } }
        }

        private string _releaseNotes = "";
        public string ReleaseNotes
        {
            get => _releaseNotes;
            set { if (_releaseNotes != value) { _releaseNotes = value; OnPropertyChanged(); } }
        }

        private string _updateDownloadUrl = "";
        public string UpdateDownloadUrl
        {
            get => _updateDownloadUrl;
            set { if (_updateDownloadUrl != value) { _updateDownloadUrl = value; OnPropertyChanged(); } }
        }

        private bool _isDownloadingUpdate;
        public bool IsDownloadingUpdate
        {
            get => _isDownloadingUpdate;
            set { if (_isDownloadingUpdate != value) { _isDownloadingUpdate = value; OnPropertyChanged(); } }
        }

        private int _updateDownloadProgress;
        public int UpdateDownloadProgress
        {
            get => _updateDownloadProgress;
            set { if (_updateDownloadProgress != value) { _updateDownloadProgress = value; OnPropertyChanged(); } }
        }

        private string _updateStatusText = "GitHub Releases 최신 버전 확인 대기 중";
        public string UpdateStatusText
        {
            get => _updateStatusText;
            set { if (_updateStatusText != value) { _updateStatusText = value; OnPropertyChanged(); } }
        }

        public ICommand CheckUpdateCommand { get; }
        public ICommand InstallUpdateCommand { get; }
        public ICommand OpenUpdateModalCommand { get; }
        public ICommand CloseUpdateModalCommand { get; }
        public ICommand OpenGitHubRepoCommand { get; }

        // File Logging & Auto-Save Management (작업 log 기록 및 컴퓨터 자동 저장)
        private bool _autoSaveLogs = true;
        public bool AutoSaveLogs
        {
            get => _autoSaveLogs;
            set
            {
                if (_autoSaveLogs != value)
                {
                    _autoSaveLogs = value;
                    LoggerService.Instance.AutoSaveEnabled = value;
                    OnPropertyChanged();
                    AddLog(value ? "[로그 설정] 작업 로그 컴퓨터 디스크 자동 저장이 활성화되었습니다." : "[로그 설정] 작업 로그 컴퓨터 디스크 자동 저장이 비활성화되었습니다.");
                }
            }
        }

        public string LogDirectoryPath => LoggerService.Instance.GetLogDirectory();
        public string CurrentLogFilePath => LoggerService.Instance.GetMainLogFilePath();

        public ICommand OpenLogFolderCommand { get; }
        public ICommand ExportLogsCommand { get; }
        public ICommand ClearLogConsoleCommand { get; }
        public ICommand ToggleAutoSaveLogsCommand { get; }

        // Theme Style Management (다크 / 회색 / 화이트 / 베이지)
        private string _currentTheme = "Dark";
        public string CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsDarkTheme));
                    OnPropertyChanged(nameof(IsGrayTheme));
                    OnPropertyChanged(nameof(IsWhiteTheme));
                    OnPropertyChanged(nameof(IsBeigeTheme));
                    ApplyTheme(value);
                }
            }
        }

        public bool IsDarkTheme => CurrentTheme == "Dark";
        public bool IsGrayTheme => CurrentTheme == "Gray";
        public bool IsWhiteTheme => CurrentTheme == "White";
        public bool IsBeigeTheme => CurrentTheme == "Beige";

        public ICommand ChangeThemeCommand { get; }

        public void ApplyTheme(string theme)
        {
            try
            {
                var app = System.Windows.Application.Current;
                if (app == null) return;

                switch (theme)
                {
                    case "Gray":
                        app.Resources["BgDark"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E232B"));
                        app.Resources["BgCard"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#282F3A"));
                        app.Resources["BgCardItem"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#232933"));
                        app.Resources["BgSidebar"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#232933"));
                        app.Resources["BorderDark"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3D4756"));
                        app.Resources["BorderCardItem"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3D4756"));
                        app.Resources["TextPrimary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"));
                        app.Resources["TextSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CBD5E1"));
                        app.Resources["AccentCyan"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8"));
                        app.Resources["AccentGreen"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4ADE80"));
                        app.Resources["AccentAmber"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBBF24"));
                        break;

                    case "White":
                        app.Resources["BgDark"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC"));
                        app.Resources["BgCard"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                        app.Resources["BgCardItem"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
                        app.Resources["BgSidebar"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"));
                        app.Resources["BorderDark"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CBD5E1"));
                        app.Resources["BorderCardItem"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
                        app.Resources["TextPrimary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"));
                        app.Resources["TextSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
                        app.Resources["AccentCyan"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0284C7"));
                        app.Resources["AccentGreen"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#15803D"));
                        app.Resources["AccentAmber"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B45309"));
                        break;

                    case "Beige":
                        app.Resources["BgDark"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F0E6"));
                        app.Resources["BgCard"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCFBF8"));
                        app.Resources["BgCardItem"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCFBF8"));
                        app.Resources["BgSidebar"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FAF7F0"));
                        app.Resources["BorderDark"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6DFD3"));
                        app.Resources["BorderCardItem"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E6DFD3"));
                        app.Resources["TextPrimary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D241E"));
                        app.Resources["TextSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4A3F37"));
                        app.Resources["AccentCyan"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0369A1"));
                        app.Resources["AccentGreen"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#166534"));
                        app.Resources["AccentAmber"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#92400E"));
                        break;

                    case "Dark":
                    default:
                        app.Resources["BgDark"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0B0F19"));
                        app.Resources["BgCard"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#111827"));
                        app.Resources["BgCardItem"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"));
                        app.Resources["BgSidebar"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"));
                        app.Resources["BorderDark"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
                        app.Resources["BorderCardItem"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
                        app.Resources["TextPrimary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F8FAFC"));
                        app.Resources["TextSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
                        app.Resources["AccentCyan"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8"));
                        app.Resources["AccentGreen"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4ADE80"));
                        app.Resources["AccentAmber"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FBBF24"));
                        break;
                }

                AddLog($"[테마 변경] {theme} 테마 스타일 적용 완료");
            }
            catch { }
        }

        public CommanderViewModel Commander { get; } = new CommanderViewModel();

        public string ScheduleFrequency
        {
            get => _scheduleTriggerType;
            set => ScheduleTriggerType = value;
        }

        private string _scheduleTriggerType = "WEEKLY"; // "STARTUP", "INTERVAL", "DAILY", "WEEKLY"
        public string ScheduleTriggerType
        {
            get => _scheduleTriggerType;
            set
            {
                if (_scheduleTriggerType != value)
                {
                    _scheduleTriggerType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ScheduleFrequency));
                    OnPropertyChanged(nameof(IsStartupSchedule));
                    OnPropertyChanged(nameof(IsIntervalSchedule));
                    OnPropertyChanged(nameof(IsDailySchedule));
                    OnPropertyChanged(nameof(IsWeeklySchedule));
                }
            }
        }

        public bool IsStartupSchedule => _scheduleTriggerType.Equals("STARTUP", StringComparison.OrdinalIgnoreCase);
        public bool IsIntervalSchedule => _scheduleTriggerType.Equals("INTERVAL", StringComparison.OrdinalIgnoreCase);
        public bool IsDailySchedule => _scheduleTriggerType.Equals("DAILY", StringComparison.OrdinalIgnoreCase);
        public bool IsWeeklySchedule => _scheduleTriggerType.Equals("WEEKLY", StringComparison.OrdinalIgnoreCase);

        private int _scheduleIntervalHours = 4;
        public int ScheduleIntervalHours
        {
            get => _scheduleIntervalHours;
            set { if (_scheduleIntervalHours != value) { _scheduleIntervalHours = Math.Clamp(value, 1, 24); OnPropertyChanged(); } }
        }

        private int _scheduleStartupDelayMinutes = 1;
        public int ScheduleStartupDelayMinutes
        {
            get => _scheduleStartupDelayMinutes;
            set { if (_scheduleStartupDelayMinutes != value) { _scheduleStartupDelayMinutes = Math.Clamp(value, 0, 15); OnPropertyChanged(); } }
        }

        private bool _scheduleRunOnLogon = false;
        public bool ScheduleRunOnLogon
        {
            get => _scheduleRunOnLogon;
            set { if (_scheduleRunOnLogon != value) { _scheduleRunOnLogon = value; OnPropertyChanged(); } }
        }

        private bool _scheduleCreateSafepoint = true;
        public bool ScheduleCreateSafepoint
        {
            get => _scheduleCreateSafepoint;
            set { if (_scheduleCreateSafepoint != value) { _scheduleCreateSafepoint = value; OnPropertyChanged(); } }
        }

        private string _scheduleModuleScope = "profile"; // "profile" or "custom"
        public string ScheduleModuleScope
        {
            get => _scheduleModuleScope;
            set
            {
                if (_scheduleModuleScope != value)
                {
                    _scheduleModuleScope = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCustomModuleScope));
                }
            }
        }
        public bool IsCustomModuleScope => _scheduleModuleScope.Equals("custom", StringComparison.OrdinalIgnoreCase);

        private string _nextRunTimeFormatted = "조회 대기";
        public string NextRunTimeFormatted
        {
            get => _nextRunTimeFormatted;
            set { if (_nextRunTimeFormatted != value) { _nextRunTimeFormatted = value; OnPropertyChanged(); } }
        }

        private string _lastRunTimeFormatted = "없음";
        public string LastRunTimeFormatted
        {
            get => _lastRunTimeFormatted;
            set { if (_lastRunTimeFormatted != value) { _lastRunTimeFormatted = value; OnPropertyChanged(); } }
        }

        private string _lastRunStatus = "준비 완료";
        public string LastRunStatus
        {
            get => _lastRunStatus;
            set { if (_lastRunStatus != value) { _lastRunStatus = value; OnPropertyChanged(); } }
        }

        private int _scheduleDayOfWeek = 0; // 0: SUN, 1: MON, ..., 6: SAT
        public int ScheduleDayOfWeek
        {
            get => _scheduleDayOfWeek;
            set { if (_scheduleDayOfWeek != value) { _scheduleDayOfWeek = value; OnPropertyChanged(); OnPropertyChanged(nameof(ScheduleDayName)); } }
        }

        public string ScheduleDayName => ScheduleDayOfWeek switch
        {
            1 => "월요일 (MON)",
            2 => "화요일 (TUE)",
            3 => "수요일 (WED)",
            4 => "목요일 (THU)",
            5 => "금요일 (FRI)",
            6 => "토요일 (SAT)",
            _ => "일요일 (SUN)"
        };

        private int _scheduleHour = 3;
        public int ScheduleHour
        {
            get => _scheduleHour;
            set { if (_scheduleHour != value) { _scheduleHour = Math.Clamp(value, 0, 23); OnPropertyChanged(); OnPropertyChanged(nameof(ScheduleTimeFormatted)); } }
        }

        private int _scheduleMinute = 0;
        public int ScheduleMinute
        {
            get => _scheduleMinute;
            set { if (_scheduleMinute != value) { _scheduleMinute = Math.Clamp(value, 0, 59); OnPropertyChanged(); OnPropertyChanged(nameof(ScheduleTimeFormatted)); } }
        }

        public string ScheduleTimeFormatted => $"{ScheduleHour:D2}:{ScheduleMinute:D2}";

        private string _scheduleProfile = "safe"; // "all", "safe", "deep", "gaming", "privacy", "custom"
        public string ScheduleProfile
        {
            get => _scheduleProfile;
            set { if (_scheduleProfile != value) { _scheduleProfile = value; OnPropertyChanged(); } }
        }

        private bool _scheduleAutoTrimRam = true;
        public bool ScheduleAutoTrimRam
        {
            get => _scheduleAutoTrimRam;
            set { if (_scheduleAutoTrimRam != value) { _scheduleAutoTrimRam = value; OnPropertyChanged(); } }
        }

        private SafepointRecord? _lastSafepoint = null;
        public SafepointRecord? LastSafepoint
        {
            get => _lastSafepoint;
            set
            {
                if (_lastSafepoint != value)
                {
                    _lastSafepoint = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasSafepoint));
                }
            }
        }
        public bool HasSafepoint => LastSafepoint != null;

        public object? ActiveModuleCategoryHighlight => IsModulesWorkspace ? (_selectedCategory ?? (object)"All") : "__NONE__";

        private TaskCategory? _selectedCategory = null;
        public TaskCategory? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    if (_selectedCategory != TaskCategory.Special)
                    {
                        _selectedSpecialSubCategory = "All";
                        OnPropertyChanged(nameof(SelectedSpecialSubCategory));
                    }
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ActiveModuleCategoryHighlight));
                    ApplyFilter();
                }
            }
        }

        private string _selectedSpecialSubCategory = "All";
        public string SelectedSpecialSubCategory
        {
            get => _selectedSpecialSubCategory;
            set
            {
                if (_selectedSpecialSubCategory != value)
                {
                    _selectedSpecialSubCategory = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }

        public int SpecialSubCategoryCountAll => AllTasks.Count(t => t.Category == TaskCategory.Special);
        public int SpecialSubCategoryCountAccount => AllTasks.Count(t => t.Category == TaskCategory.Special && string.Equals(t.SubCategory, "Account", StringComparison.OrdinalIgnoreCase));
        public int SpecialSubCategoryCountFactoryReset => AllTasks.Count(t => t.Category == TaskCategory.Special && string.Equals(t.SubCategory, "FactoryReset", StringComparison.OrdinalIgnoreCase));
        public int SpecialSubCategoryCountProfile => AllTasks.Count(t => t.Category == TaskCategory.Special && string.Equals(t.SubCategory, "Profile", StringComparison.OrdinalIgnoreCase));
        public int SpecialSubCategoryCountForensics => AllTasks.Count(t => t.Category == TaskCategory.Special && string.Equals(t.SubCategory, "Forensics", StringComparison.OrdinalIgnoreCase));

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { if (_isBusy != value) { _isBusy = value; OnPropertyChanged(); } }
        }

        private string _statusText = "준비 완료 (Ready)";
        public string StatusText
        {
            get => _statusText;
            set { if (_statusText != value) { _statusText = value; OnPropertyChanged(); } }
        }

        private int _progressValue;
        public int ProgressValue
        {
            get => _progressValue;
            set { if (_progressValue != value) { _progressValue = value; OnPropertyChanged(); } }
        }

        private int _totalProgressMax = 100;
        public int TotalProgressMax
        {
            get => _totalProgressMax;
            set { if (_totalProgressMax != value) { _totalProgressMax = value; OnPropertyChanged(); } }
        }

        private long _totalReclaimableBytes;
        public long TotalReclaimableBytes
        {
            get => _totalReclaimableBytes;
            set
            {
                if (_totalReclaimableBytes != value)
                {
                    _totalReclaimableBytes = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FormattedTotalSize));
                }
            }
        }

        public string FormattedTotalSize
        {
            get
            {
                if (TotalReclaimableBytes <= 0) return "0 MB";
                if (TotalReclaimableBytes < 1024 * 1024 * 1024)
                    return $"{(TotalReclaimableBytes / (1024.0 * 1024.0)):F1} MB";
                return $"{(TotalReclaimableBytes / (1024.0 * 1024.0 * 1024.0)):F2} GB";
            }
        }

        public int SelectedCount => AllTasks.Count(t => t.IsSelected);
        public int TotalTasksCount => AllTasks.Count;

        // Commands
        public ICommand ScanAllCommand { get; }
        public ICommand OptimizeSelectedCommand { get; }
        public ICommand RollbackSafepointCommand { get; }
        public ICommand CompressRamCommand { get; }
        public ICommand GameBoostCommand { get; }
        public ICommand RunDiagnosticsCommand { get; }
        public ICommand ToggleScheduleCommand { get; }
        public ICommand OpenScheduleModalCommand { get; }
        public ICommand CloseScheduleModalCommand { get; }
        public ICommand SaveScheduleSettingsCommand { get; }
        public ICommand UnregisterScheduleCommand { get; }
        public ICommand RunScheduleNowCommand { get; }
        public ICommand RefreshScheduleStatusCommand { get; }
        public ICommand OpenCommanderModalCommand { get; }
        public ICommand CloseCommanderModalCommand { get; }
        public ICommand LaunchCommanderProcessCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand DeselectAllCommand { get; }
        public ICommand SelectSafePresetCommand { get; }
        public ICommand SelectDeepPresetCommand { get; }
        public ICommand SelectGamingPresetCommand { get; }
        public ICommand SelectPrivacyPresetCommand { get; }
        public ICommand SelectZeroTracePresetCommand { get; }
        public ICommand FilterCategoryCommand { get; }
        public ICommand FilterSpecialSubCategoryCommand { get; }

        public MainViewModel()
        {
            ScanAllCommand = new RelayCommand(async _ => await ScanAllAsync(), _ => !IsBusy);
            OptimizeSelectedCommand = new RelayCommand(async _ => await OptimizeSelectedAsync(), _ => !IsBusy && SelectedCount > 0);
            RollbackSafepointCommand = new RelayCommand(async _ => await RollbackSafepointAsync(), _ => !IsBusy && HasSafepoint);
            CompressRamCommand = new RelayCommand(async _ => await CompressRamAsync(), _ => !IsBusy);
            GameBoostCommand = new RelayCommand(async _ => await ApplyGameBoostAsync(), _ => !IsBusy);
            RunDiagnosticsCommand = new RelayCommand(async _ => await RunDiagnosticsAsync(), _ => !IsBusy);
            ToggleScheduleCommand = new RelayCommand(_ => OpenScheduleModal());
            OpenScheduleModalCommand = new RelayCommand(_ => OpenScheduleModal());
            CloseScheduleModalCommand = new RelayCommand(_ => IsScheduleModalOpen = false);
            SaveScheduleSettingsCommand = new RelayCommand(async _ => await SaveScheduleSettingsAsync(), _ => !IsBusy);
            UnregisterScheduleCommand = new RelayCommand(async _ => await RemoveScheduleAsync(), _ => !IsBusy);
            RunScheduleNowCommand = new RelayCommand(async _ => await RunScheduleNowAsync(), _ => !IsBusy);
            RefreshScheduleStatusCommand = new RelayCommand(async _ => await RefreshScheduleStatusAsync(), _ => !IsBusy);
            OpenCommanderModalCommand = new RelayCommand(_ => IsCommanderModalOpen = true);
            CloseCommanderModalCommand = new RelayCommand(_ => IsCommanderModalOpen = false);
            LaunchCommanderProcessCommand = new RelayCommand(_ => LaunchCommanderProcess());

            // Account & Credential Purge Command Bindings
            OpenCredentialPurgeModalCommand = new RelayCommand(_ => OpenCredentialPurgeModal());
            CloseCredentialPurgeModalCommand = new RelayCommand(_ => IsCredentialPurgeModalOpen = false);
            ScanAccountTargetsCommand = new RelayCommand(async _ => await ScanAccountTargetsAsync(), _ => !IsCredentialPurging);
            PurgeSelectedAccountsCommand = new RelayCommand(async _ => await PurgeSelectedAccountsAsync(), _ => !IsCredentialPurging && AccountTargets.Any(t => t.IsSelected));
            SelectAllAccountTargetsCommand = new RelayCommand(_ => { foreach (var t in AccountTargets) t.IsSelected = true; });
            DeselectAllAccountTargetsCommand = new RelayCommand(_ => { foreach (var t in AccountTargets) t.IsSelected = false; });
            ApplyAccountScenarioCommand = new RelayCommand(param =>
            {
                if (param is string presetId)
                {
                    AccountCredentialPurgeService.ApplyScenarioPreset(AccountTargets, presetId);
                    AddLog($"[계정 정화] 시나리오 프리셋 적용: {presetId}");
                }
            });

            SimulateCommanderConfirmationCommand = new RelayCommand(_ => SimulateCommanderPurgeConfirmation());
            ClearCommanderZeroTraceLogsCommand = new RelayCommand(_ =>
            {
                CommanderZeroTraceLogs.Clear();
                HasCommanderConfirmation = false;
                LatestCommanderConfirmationStatus = "🟢 원격 관제 채널 대기 중 (TLS 9874 / HMAC-SHA256 보호됨)";
            });

            CloseRemoteActionModalCommand = new RelayCommand(_ =>
            {
                _remoteActionCloseTimer?.Stop();
                IsRemoteActionModalOpen = false;
            });

            ClearRemoteExecutionHistoryCommand = new RelayCommand(_ =>
            {
                RemoteExecutionAuditService.Instance.ClearHistory();
                RemoteExecutionHistory.Clear();
                AddLog("[감사] 원격 실행 이력 초기화 완료");
            });

            ConfirmDefaultProfileWarningCommand = new RelayCommand(_ => ConfirmDefaultProfileWarning());
            CloseDefaultProfileWarningCommand = new RelayCommand(_ => CloseDefaultProfileWarning());

            // Dual-Language License Agreement Command Bindings (KO & EN)
            InitializeLicenseTexts();
            OpenLicenseModalCommand = new RelayCommand(_ => IsLicenseModalOpen = true);
            CloseLicenseModalCommand = new RelayCommand(_ => IsLicenseModalOpen = false);
            SwitchLicenseLanguageCommand = new RelayCommand(lang =>
            {
                if (lang is string s && (s == "KO" || s == "EN"))
                {
                    LicenseLanguage = s;
                }
            });

            // System Tray & Notification Center Command Bindings
            OpenTraySettingsModalCommand = new RelayCommand(_ => IsTraySettingsModalOpen = true);
            CloseTraySettingsModalCommand = new RelayCommand(_ => IsTraySettingsModalOpen = false);
            SendTestTrayNotificationCommand = new RelayCommand(_ =>
            {
                SystemTrayService.Instance.ShowTrayNotification(
                    "🔔 WinPurify Pro 트레이 알림 센터",
                    "시스템 트레이 작업 감시 및 알림 기능이 정상 작동 중입니다.",
                    TrayNotificationType.Info
                );
                AddLog("[트레이 알림] 테스트 트레이 알림 발송 완료");
            });
            MinimizeToTrayCommand = new RelayCommand(_ =>
            {
                SystemTrayService.Instance.ToggleMainWindow();
            });

            // 원격 실행 이력 초기 로드
            try
            {
                foreach (var rec in RemoteExecutionAuditService.Instance.GetRecentRecords(50))
                {
                    RemoteExecutionHistory.Add(rec);
                }
            }
            catch { }

            // 중앙 관제 콘솔 원격 명령 실행 이벤트 구독 (알람 및 동작 HUD 연동)
            RemoteAgentService.RemoteCommandExecuting += OnRemoteCommandExecuting;
            RemoteAgentService.RemoteCommandCompleted += OnRemoteCommandCompleted;

            SelectAllCommand = new RelayCommand(_ => SetSelection(true));
            DeselectAllCommand = new RelayCommand(_ => SetSelection(false));
            SelectSafePresetCommand = new RelayCommand(_ => ApplyPreset(RiskLevel.Safe));
            SelectDeepPresetCommand = new RelayCommand(_ => ApplyPreset(RiskLevel.Deep));
            SelectGamingPresetCommand = new RelayCommand(_ => ApplyGamingProfile());
            SelectPrivacyPresetCommand = new RelayCommand(_ => ApplyPrivacyProfile());
            SelectZeroTracePresetCommand = new RelayCommand(_ => ApplyZeroTraceProfile());
            FilterCategoryCommand = new RelayCommand(cat =>
            {
                ActiveWorkspace = "Modules";
                if (cat is TaskCategory c) SelectedCategory = c;
                else SelectedCategory = null;
            });
            FilterSpecialSubCategoryCommand = new RelayCommand(sub =>
            {
                SelectedSpecialSubCategory = sub?.ToString() ?? "All";
            });

            ChangeThemeCommand = new RelayCommand(param =>
            {
                if (param is string t) CurrentTheme = t;
            });

            // Logging & File Auto-Save Commands (작업 log 기록 및 컴퓨터 자동 저장)
            OpenLogFolderCommand = new RelayCommand(_ =>
            {
                LoggerService.Instance.OpenLogFolder();
                AddLog($"[로그 탐색기] 컴퓨터 로그 저장 폴더 열기: {LoggerService.Instance.GetLogDirectory()}");
            });

            ExportLogsCommand = new RelayCommand(_ =>
            {
                try
                {
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string targetFile = Path.Combine(desktop, $"WinPurifyPro_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                    bool success = LoggerService.Instance.ExportLogs(targetFile, LogConsole, "WinPurify Pro Main Client Operation Logs");
                    if (success)
                    {
                        AddLog($"[로그 내보내기] 현재 작업 로그를 바탕화면에 저장 완료: {Path.GetFileName(targetFile)}");
                    }
                    else
                    {
                        AddLog("[로그 내보내기 오류] 로그 파일 저장 실패");
                    }
                }
                catch (Exception ex)
                {
                    AddLog($"[로그 내보내기 오류] {ex.Message}");
                }
            });

            ClearLogConsoleCommand = new RelayCommand(_ =>
            {
                LogConsole.Clear();
                AddLog("[로그 초기화] 화면 상의 작업 로그 목록이 지워졌습니다. (컴퓨터 디스크 로그 파일은 영구 보존됨)");
            });

            ToggleAutoSaveLogsCommand = new RelayCommand(_ =>
            {
                AutoSaveLogs = !AutoSaveLogs;
            });

            // GitHub Releases Live Auto-Update Command Bindings
            OpenUpdateModalCommand = new RelayCommand(_ =>
            {
                IsUpdateModalOpen = true;
                _ = CheckForUpdatesAsync();
            });
            CloseUpdateModalCommand = new RelayCommand(_ => IsUpdateModalOpen = false);
            CheckUpdateCommand = new RelayCommand(async _ => await CheckForUpdatesAsync());
            InstallUpdateCommand = new RelayCommand(async _ => await DownloadAndApplyUpdateAsync());
            OpenGitHubRepoCommand = new RelayCommand(_ =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = GitHubUpdateService.GitHubRepoUrl,
                        UseShellExecute = true
                    });
                }
                catch { }
            });

            LoadTasks();
            InitializeHandoverWorkspace();
            StartMetricsTracking();
            CheckScheduleStatus();

            // 다중 PC 중앙 관제 (Commander): 메뉴는 상시 접근 가능하되 클릭 시 관리자 확인 대화상자로 사용자 오동작 완벽 차단
            IsCommanderMenuVisible = true;

            // 앱 기동 2.5초 후 백그라운드 최신 업데이트 자동 감지 -> 업데이트 발견 시 알림 팝업창 자동 실행
            _ = Task.Run(async () =>
            {
                await Task.Delay(2500);
                await CheckForUpdatesAsync(isManual: false);
            });
        }

        public void AddLog(string msg)
        {
            // 1. 컴퓨터 디스크 파일에 실시간 자동 저장
            if (AutoSaveLogs)
            {
                LoggerService.Instance.LogMain(msg);
            }

            // 2. UI 실시간 콘솔 갱신
            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                LogConsole.Insert(0, $"[{timestamp}] {msg}");
                if (LogConsole.Count > 120) LogConsole.RemoveAt(LogConsole.Count - 1);
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
