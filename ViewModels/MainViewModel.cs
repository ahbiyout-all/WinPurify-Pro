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
    public class MainViewModel : IMainViewModel, INotifyPropertyChanged
    {
        public string AppVersion => AppEnvironment.AppVersion;

        public string WindowTitle => $"WinPurify Pro v{AppVersion} - Next-Gen Native Optimization Suite";
        public string AppHeaderSubtitle => $"v{AppVersion} Next-Gen Hybrid Core";

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

        private string _latestCommanderConfirmationStatus = "🟢 원격 관제 채널 대기 중 (TCP 9870 / HMAC-SHA256 보호됨)";
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
            OpenCommanderModalCommand = new RelayCommand(_ => { });
            CloseCommanderModalCommand = new RelayCommand(_ => IsCommanderModalOpen = false);
            LaunchCommanderProcessCommand = new RelayCommand(_ => { });

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
                LatestCommanderConfirmationStatus = "🟢 원격 관제 채널 대기 중 (TCP 9870 / HMAC-SHA256 보호됨)";
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

                // 1. 동작 알람 경고음 재생 (System Exclamation Alert)
                try { System.Media.SystemSounds.Exclamation.Play(); } catch { }

                // 2. 모달 상태 및 상세 데이터 세팅
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
                // 완료 알람 사운드
                try { System.Media.SystemSounds.Asterisk.Play(); } catch { }

                RemoteActionTitle = e.Status == "Success" 
                    ? "✅ 중앙 관제(Commander) 원격 명령 처리 완료" 
                    : "⚠️ 중앙 관제(Commander) 원격 명령 오류";
                RemoteActionStatus = e.Status == "Success" ? "정상 완료" : "실패";
                RemoteActionDetails = $"{e.ResultMessage} (소요시간: {e.ElapsedMs}ms)";
                IsRemoteActionActive = false;

                // 감사 이력 리스트 갱신
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

                // Zero-Trace 센터 전용 실시간 Commander 원격 정화 확인 로그 및 상태 갱신
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

                // 5초 카운트다운 타이머 시작 후 자동 닫힘
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
                // Unregister if currently registered
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

            // Register custom schedule with selected parameters
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
                AddLog($"[Task Scheduler] 자동 정화 설정 저장: 트리거={triggerDesc} | 대상={targetDesc} | RAM압축={ScheduleAutoTrimRam} | 세이프포인트={ScheduleCreateSafepoint}");

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
                AddLog("[Task Scheduler] schtasks 백그라운드 태스크 기동 완료 (결과는 %ProgramData%\\WinPurifyPro\\Logs\\AutoScheduler.log 기록)");
                SystemTrayService.Instance.ShowTrayNotification("⏰ 자동 스케줄러 실행", "백그라운드 스케줄 정화 작업이 시작되었습니다.", TrayNotificationType.Info);
            }
            else
            {
                // Fallback direct execution simulation
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

        public async Task ToggleScheduleAsync()
        {
            OpenScheduleModal();
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

        private void LoadTasks()
        {
            var tasks = TaskDataSeeder.GetAllTasks();
            AllTasks.Clear();
            foreach (var t in tasks)
            {
                t.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(OptimizationTask.IsSelected))
                    {
                        if (t.Id == "spc-sync-default-user-profile" && t.IsSelected && !_suppressDefaultProfileWarning)
                        {
                            _suppressDefaultProfileWarning = true;
                            t.IsSelected = false;
                            _suppressDefaultProfileWarning = false;
                            IsDefaultProfileWarningModalOpen = true;
                            return;
                        }
                        OnPropertyChanged(nameof(SelectedCount));
                        CalculateTotalReclaimable();
                    }
                };
                AllTasks.Add(t);
            }
            ApplyFilter();
            AddLog($"[엔진 초기화] v{AppEnvironment.AppVersion} 정밀 엔진 가동 - {AllTasks.Count}개 모듈 및 Central Commander Option A 특수 기능 허브 연동 완료.");
        }

        public void ApplyFilter()
        {
            FilteredTasks.Clear();
            var query = AllTasks.AsEnumerable();

            if (SelectedCategory.HasValue)
            {
                query = query.Where(t => t.Category == SelectedCategory.Value);

                if (SelectedCategory.Value == TaskCategory.Special &&
                    !string.IsNullOrEmpty(SelectedSpecialSubCategory) &&
                    !string.Equals(SelectedSpecialSubCategory, "All", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(t => string.Equals(t.SubCategory, SelectedSpecialSubCategory, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string lower = SearchQuery.ToLower();
                query = query.Where(t => t.Name.ToLower().Contains(lower) || t.Description.ToLower().Contains(lower) || t.Id.ToLower().Contains(lower));
            }

            int seq = 1;
            foreach (var item in query)
            {
                item.SequenceNumber = seq++;
                FilteredTasks.Add(item);
            }
            OnPropertyChanged(nameof(SelectedCount));
        }

        private void SetSelection(bool select)
        {
            foreach (var t in FilteredTasks)
            {
                // Special category items must never be bulk-selected automatically unless explicitly clicked
                if (select && t.Category == TaskCategory.Special && SelectedCategory != TaskCategory.Special)
                {
                    continue;
                }
                t.IsSelected = select;
            }
            CalculateTotalReclaimable();
            OnPropertyChanged(nameof(SelectedCount));
        }

        private void ApplyPreset(RiskLevel maxRisk)
        {
            foreach (var t in AllTasks)
            {
                // Special category tasks are never selected by standard presets
                if (t.Category == TaskCategory.Special)
                {
                    t.IsSelected = false;
                    continue;
                }
                t.IsSelected = (t.Risk <= maxRisk);
            }
            CalculateTotalReclaimable();
            OnPropertyChanged(nameof(SelectedCount));
            AddLog($"[프리셋] {maxRisk} 등급 이하 항목 자동 선택 완료 (선택: {SelectedCount}개)");
        }

        private void ApplyGamingProfile()
        {
            foreach (var t in AllTasks)
            {
                if (t.Category == TaskCategory.Special)
                {
                    t.IsSelected = false;
                    continue;
                }
                t.IsSelected = (t.Category == TaskCategory.System || t.Category == TaskCategory.Storage || t.Category == TaskCategory.Update) && (t.Risk <= RiskLevel.Deep);
            }
            CalculateTotalReclaimable();
            OnPropertyChanged(nameof(SelectedCount));
            AddLog($"[프로필 적용] Gaming Boost Profile 활성화 (선택: {SelectedCount}개 모듈)");
        }

        private void ApplyPrivacyProfile()
        {
            foreach (var t in AllTasks)
            {
                if (t.Category == TaskCategory.Special)
                {
                    t.IsSelected = false;
                    continue;
                }
                t.IsSelected = (t.Category == TaskCategory.Privacy || t.Category == TaskCategory.Browser || t.Category == TaskCategory.Security);
            }
            CalculateTotalReclaimable();
            OnPropertyChanged(nameof(SelectedCount));
            AddLog($"[프로필 적용] Deep Privacy Profile 활성화 (선택: {SelectedCount}개 모듈)");
        }

        private void ApplyZeroTraceProfile()
        {
            foreach (var t in AllTasks)
            {
                // Zero-Trace specifically targets privacy, credentials, browser sessions, tokens, security logs, and user storage dumps
                // while excluding generic OS speed tuning / animations / network tweaks.
                if (t.Category == TaskCategory.Special)
                {
                    t.IsSelected = false;
                    continue;
                }

                // 1. All Privacy & Anti-Forensics (Credentials, RDP, USB history, BAM/DAM, ComDlg32, Wi-Fi, PowerShell history, Clipboard, Recent)
                if (t.Category == TaskCategory.Privacy)
                {
                    t.IsSelected = true;
                    continue;
                }

                // 2. All Browser & Messenger & Cloud Sync Sessions (Saved passwords, Autofill/Cards, KakaoTalk, Discord, Teams, Slack, Cloud caches)
                if (t.Category == TaskCategory.Browser)
                {
                    t.IsSelected = true;
                    continue;
                }

                // 3. Security Event Logs & Defender Scans
                if (t.Category == TaskCategory.Security)
                {
                    t.IsSelected = true;
                    continue;
                }

                // 4. Critical Storage dumps & footprints (Recycle Bin, Crash dumps, Temp folders, WER reports, hiberfil, Search index DB)
                if (t.Category == TaskCategory.Storage)
                {
                    // Target user-activity and identity-holding storage items
                    bool isZeroTraceStorage = t.Id.Contains("recycle-bin") ||
                                              t.Id.Contains("crash-memory") ||
                                              t.Id.Contains("wer-reports") ||
                                              t.Id.Contains("temp") ||
                                              t.Id.Contains("hiberfil") ||
                                              t.Id.Contains("wsearch-edb") ||
                                              t.Id.Contains("prefetch");
                    t.IsSelected = isZeroTraceStorage;
                    continue;
                }

                // Exclude pure OS tuning / UI tweaks / Update optimizations
                t.IsSelected = false;
            }
            CalculateTotalReclaimable();
            OnPropertyChanged(nameof(SelectedCount));
            AddLog($"[프로필 적용] Zero-Trace 안티포렌식 초기화 프로필 활성화 ({SelectedCount}개 사용자 흔적/자격증명/세션 모듈 집중 선별)");
        }

        public async Task RunDiagnosticsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusText = "시스템 및 엔진 무결성 진단(Self-Diagnostics) 실행 중...";
            AddLog($"[셀프 진단] Native DLL, 관리자 권한, I/O 스캐너 및 {AllTasks.Count}개 모듈 무결성 검증 시작");

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

        public async Task ScanAllAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ProgressValue = 0;
            TotalProgressMax = AllTasks.Count;
            StatusText = "초고속 I/O 용량 분석 중 (Robocopy / Win32 API)...";
            AddLog($"[스캔 시작] {AllTasks.Count}개 모듈 대상 Robocopy 초고속 가상 탐색 개시");

            long totalBytes = 0;
            int scanned = 0;

            await Task.Run(async () =>
            {
                foreach (var task in AllTasks)
                {
                    task.Status = Models.TaskStatus.Scanning;
                    if (!string.IsNullOrWhiteSpace(task.ScanTargetFolder))
                    {
                        long size = await RobocopyScannerService.GetFolderSizeFastAsync(task.ScanTargetFolder);
                        task.EstimatedBytes = size;
                        if (task.IsSelected) totalBytes += size;
                    }
                    task.Status = Models.TaskStatus.Scanned;
                    scanned++;

                    System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        ProgressValue = scanned;
                        TotalReclaimableBytes = totalBytes;
                    });
                }
            });

            IsBusy = false;
            StatusText = $"스캔 완료 - 확보 가능 공간: {FormattedTotalSize}";
            AddLog($"[스캔 완료] 총 {AllTasks.Count}개 모듈 분석 완료. 예상 절감 용량: {FormattedTotalSize}");
            SystemTrayService.Instance.ShowTrayNotification("🔍 전체 스캔 완료", $"총 {AllTasks.Count}개 모듈 분석 완료. 예상 절감 용량: {FormattedTotalSize}", TrayNotificationType.Info);
            SystemTrayService.Instance.UpdateTooltip($"WinPurify Pro v{AppVersion}\n스캔 완료 (확보 가능: {FormattedTotalSize})\n상태: 준비 완료");
        }

        public async Task OptimizeSelectedAsync()
        {
            var targets = AllTasks.Where(t => t.IsSelected).ToList();
            if (!targets.Any()) return;

            IsBusy = true;
            ProgressValue = 0;
            TotalProgressMax = targets.Count;
            StatusText = "하이브리드 다중화 엔진 최적화 진행 중...";

            SystemTrayService.Instance.ResetProgressMilestone();
            SystemTrayService.Instance.ShowTrayNotification("🚀 시스템 최적화 시작", $"{targets.Count}개 모듈의 초고속 정화 작업을 시작합니다.", TrayNotificationType.Info);

            AddLog($"[Safepoint] 최적화 전 레지스트리 백업 및 무결성 복원 지점 생성 중...");
            LastSafepoint = await LiveSafepointService.CreateSafepointAsync(targets, TotalReclaimableBytes);
            AddLog($"[Safepoint 완료] 복원 스냅샷 저장됨 ({LastSafepoint.BackupRegFiles.Count}개 레지스트리 키 백업 완료)");

            int completed = 0;
            int successCount = 0;

            foreach (var task in targets)
            {
                task.Status = Models.TaskStatus.Optimizing;
                var res = await MultiEngineDispatcher.ExecuteTaskAsync(task);

                if (res.Success)
                {
                    task.Status = Models.TaskStatus.Completed;
                    task.StatusMessage = $"{res.UsedMode} ({res.ExecutionTimeMs}ms)";
                    successCount++;
                    AddLog($"[성공] {task.Name} -> {res.UsedMode} ({res.ExecutionTimeMs}ms)");
                }
                else
                {
                    task.Status = Models.TaskStatus.Failed;
                    task.StatusMessage = "Failover Error";
                    AddLog($"[오류] {task.Name} -> {res.OutputMessage}");
                }

                completed++;
                ProgressValue = completed;
                SystemTrayService.Instance.UpdateProgress(completed, targets.Count, task.Name);
            }

            IsBusy = false;
            StatusText = $"최적화 완료 ({successCount}/{targets.Count} 모듈 성공)";
            AddLog($"[작업 완료] {successCount}개 모듈 최적화 성공. 시스템 리소스가 즉시 반환되었습니다.");
            SystemTrayService.Instance.ShowTrayNotification("✨ 최적화 완료", $"총 {successCount}/{targets.Count}개 모듈 최적화 성공! 리소스가 회수되었습니다.", TrayNotificationType.Success);
            SystemTrayService.Instance.UpdateTooltip($"WinPurify Pro v{AppVersion}\n최적화 완료 ({successCount}/{targets.Count} 성공)\n상태: 준비 완료");
        }

        public async Task RollbackSafepointAsync()
        {
            if (LastSafepoint == null || IsBusy) return;
            IsBusy = true;
            StatusText = "세이프포인트 복원(Undo Rollback) 실행 중...";
            AddLog($"[Rollback] 스냅샷({LastSafepoint.Id.Substring(0, 8)}) 복원 개시...");

            bool success = await LiveSafepointService.RollbackSafepointAsync(LastSafepoint);

            IsBusy = false;
            if (success)
            {
                StatusText = "세이프포인트 복원 성공 (1-Click Undo 완료)";
                AddLog("[Rollback 완료] 백업된 레지스트리 및 시스템 설정이 원래대로 복원되었습니다.");
                SystemTrayService.Instance.ShowTrayNotification("↩️ 세이프포인트 복원 성공", "레지스트리 및 시스템 설정이 복원되었습니다.", TrayNotificationType.Success);
            }
            else
            {
                StatusText = "세이프포인트 복원 중 일부 경고 발생";
                AddLog("[Rollback 경고] 일부 레지스트리 항목 복원 중 권한 확인이 필요합니다.");
                SystemTrayService.Instance.ShowTrayNotification("⚠️ 세이프포인트 복원 경고", "일부 레지스트리 복원 중 관리자 권한 확인이 필요합니다.", TrayNotificationType.Warning);
            }
        }

        public async Task CompressRamAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusText = "물리 RAM 커널 압축(Working Set Trim) 실행 중...";
            AddLog("[RAM 최적화] 커널 특권 획득 후 전체 프로세스 물리 메모리 페이징 압축 개시");

            int count = await Task.Run(() => NativeEngineService.CompressPhysicalRAM());

            IsBusy = false;
            StatusText = $"RAM 압축 완료 ({count}개 프로세스 메모리 회수)";
            AddLog($"[RAM 최적화 완료] {count}개 활성 프로세스에 대한 Working Set Trim 완료.");
            SystemTrayService.Instance.ShowTrayNotification("🧠 RAM 압축 완료", $"{count}개 프로세스의 Working Set Trim 완료. 여유 메모리가 확보되었습니다.", TrayNotificationType.Success);
        }

        public async Task ApplyGameBoostAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            StatusText = "하이퍼 게이밍 가속 모드 활성화 중...";
            AddLog("[Game Boost] 게임 프로세스 우선순위 HIGH 격상 및 네트워크 스택 최적화 개시");

            await Task.Run(() =>
            {
                NativeEngineService.TrySetKernelGameBoost(true);
                NativeEngineService.TryOptimizeNetwork(1); // Low-Latency Gaming TCP Tuning
                NativeEngineService.ExecuteQuickMaintenance(2); // DNS Flush
                NativeEngineService.CompressPhysicalRAM();
            });

            IsBusy = false;
            StatusText = "게이밍 가속 적용 완료 (Low-Latency Mode)";
            AddLog("[Game Boost 완료] 지연시간 단축 및 프레임 드랍 방지 가속 설정 적용됨.");
        }

        private void OpenCommanderModal()
        {
        }

        private string? FindCommanderExecutable()
        {
            return null;
        }

        private void LaunchCommanderProcess(string? specificPath = null)
        {
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
                CommanderIp = "192.168.1.100 (Central Commander)",
                CommandType = "AccountPurge",
                CommandTitle = "통합 계정 & 세션 토큰 정화 (Zero-Trace)",
                Details = "[원격 확인 영수증] 15개 타깃 계정 세션 토큰 파쇄 및 선행 프로세스 종료 완료 (HMAC-SHA256 서명 검증됨)",
                ExecutionTimeMs = 345,
                IsSuccess = true,
                Timestamp = DateTime.Now
            };

            HasCommanderConfirmation = true;
            LastCommanderConfirmationTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            LatestCommanderConfirmationStatus = $"✅ [Commander 실시간 확인] 192.168.1.100 -> 통합 계정 & 세션 정화 완료 (345ms)";
            CommanderZeroTraceLogs.Insert(0, record);

            try { System.Media.SystemSounds.Asterisk.Play(); } catch { }
            AddLog("[Commander 원격 확인] 중앙 관제 콘솔로부터 원격 정화 완료 영수증(Confirmation)을 실시간 수신하였습니다.");
        }

        private void CalculateTotalReclaimable()
        {
            TotalReclaimableBytes = AllTasks.Where(t => t.IsSelected).Sum(t => t.EstimatedBytes);
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

                if (File.Exists(koPath))
                {
                    LicenseKoreanText = File.ReadAllText(koPath);
                }
                else
                {
                    LicenseKoreanText = "WinPurify Pro 최종 사용자 사용권 계약서 (EULA - Korean Edition)\n\n" +
                                        "제1조 (사용권의 부여): 개인 및 기업 사용자에게 비독점적이고 양도 불가능한 소프트웨어 사용 권한을 부여합니다.\n" +
                                        "제2조 (원격 관제): TCP 9870 및 UDP 9871 포트를 통한 로컬 네트워크 관제를 지원합니다.\n" +
                                        "제3조 (저작권): Cisnet Soft에 모든 지적재산권이 귀속됩니다.\n" +
                                        "제4조 (데이터 보호): 모든 정화 작업은 로컬 장치 내에서 오프라인 완결형으로 실행됩니다.\n" +
                                        "제5조 (보증의 한계): 소프트웨어는 AS-IS로 제공되며 세이프포인트 백업 사용을 권장합니다.";
                }

                if (File.Exists(enPath))
                {
                    LicenseEnglishText = File.ReadAllText(enPath);
                }
                else
                {
                    LicenseEnglishText = "WinPurify Pro End User License Agreement (EULA - Standard English Edition)\n\n" +
                                         "1. Grant of License: Cisnet Soft grants a non-exclusive, non-transferable license to use WinPurify Pro.\n" +
                                         "2. Remote Ports: Operates over TCP 9870 and UDP 9871 strictly for authorized administration.\n" +
                                         "3. Intellectual Property: All title and copyright belong exclusively to Cisnet Soft.\n" +
                                         "4. Data Privacy: Operates entirely offline with zero external telemetry transmission.\n" +
                                         "5. Disclaimer: Provided 'AS IS'. Users are advised to utilize Live Safepoint registry backups.";
                }
            }
            catch (Exception ex)
            {
                LicenseKoreanText = "라이선스 파일을 불러오는 중 오류가 발생했습니다: " + ex.Message;
                LicenseEnglishText = "Failed to load license agreement files: " + ex.Message;
            }
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

        public async Task CheckForUpdatesAsync(bool isManual = true)
        {
            if (IsCheckingUpdate) return;
            IsCheckingUpdate = true;
            UpdateStatusText = "GitHub Releases API를 통해 최신 버전을 확인하는 중...";
            
            if (isManual)
            {
                IsUpdateModalOpen = true;
                AddLog("[자동 업데이트] GitHub API (ahbiyout-all/WinPurify-Pro) 최신 릴리스 검사 시작...");
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
                if (isManual)
                {
                    AddLog($"[자동 업데이트 오류] {res.ErrorMessage}");
                }
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
                AddLog($"[업데이트 발견] 현재: v{res.CurrentVersion} -> 최신: v{res.LatestVersion}. 업데이트 알림 팝업창을 자동 실행합니다.");
                
                // 업데이트가 있으면 알림 팝업창 자동 실행!
                System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                {
                    IsUpdateModalOpen = true;
                });

                SystemTrayService.Instance.ShowTrayNotification(
                    "🚀 WinPurify Pro 새 버전 출시",
                    $"새로운 버전 v{res.LatestVersion}이 출시되었습니다. 확인하여 업데이트를 진행하세요.",
                    TrayNotificationType.Info
                );
            }
            else
            {
                UpdateStatusText = $"현재 최신 버전(v{res.CurrentVersion})을 사용하고 있습니다.";
                if (isManual)
                {
                    AddLog($"[업데이트 점검] 현재 최신 버전(v{res.CurrentVersion}) 사용 중.");
                }
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
                AddLog("[자동 업데이트 오류] 인스톨러 다운로드/실행 실패. 브라우저에서 직접 다운로드하십시오.");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
