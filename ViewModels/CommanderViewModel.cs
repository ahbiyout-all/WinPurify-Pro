using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Input;
using WinPurifyPro.Models;
using WinPurifyPro.Services;

namespace WinPurifyPro.ViewModels
{
    public class CommanderViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public ObservableCollection<RemoteNodeItem> RemoteNodes { get; set; } = new();
        public ObservableCollection<string> CommanderLogs { get; set; } = new();

        private bool _isScanning;
        public bool IsScanning
        {
            get => _isScanning;
            set { if (_isScanning != value) { _isScanning = value; OnPropertyChanged(); } }
        }

        private bool _isExecutingBatch;
        public bool IsExecutingBatch
        {
            get => _isExecutingBatch;
            set { if (_isExecutingBatch != value) { _isExecutingBatch = value; OnPropertyChanged(); } }
        }

        private string _clusterSecretKey = "WinPurifyClusterKey2026";
        public string ClusterSecretKey
        {
            get => _clusterSecretKey;
            set
            {
                if (_clusterSecretKey != value)
                {
                    _clusterSecretKey = value;
                    RemoteCommanderManager.Instance.ClusterSecretKey = value;
                    RemoteAgentService.Instance.ClusterSecretKey = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isAgentServerRunning;
        public bool IsAgentServerRunning
        {
            get => _isAgentServerRunning;
            set
            {
                if (_isAgentServerRunning != value)
                {
                    _isAgentServerRunning = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AgentButtonText));
                    OnPropertyChanged(nameof(AgentButtonBackground));
                }
            }
        }

        public bool IsPortableMode => AppEnvironment.IsPortable;
        public bool IsInstalledMode => AppEnvironment.IsInstalled;

        public string AgentButtonText
        {
            get
            {
                if (IsPortableMode)
                {
                    return "🔒 포터블: 명령 수신 차단됨";
                }
                return IsAgentServerRunning
                    ? "📡 윈도우 서비스 에이전트: ON (TCP:9870)"
                    : "📡 윈도우 서비스 에이전트: OFF (시작)";
            }
        }

        public string AgentButtonBackground
        {
            get
            {
                if (IsPortableMode) return "#334155";
                return IsAgentServerRunning ? "#059669" : "#475569";
            }
        }
        public string CommanderVersion => $"WinPurify Pro Central Commander Engine v{AppEnvironment.AppVersion}";

        private string _commanderStatus = "원격 관제 준비 완료 (Ready)";
        public string CommanderStatus
        {
            get => _commanderStatus;
            set { if (_commanderStatus != value) { _commanderStatus = value; OnPropertyChanged(); } }
        }

        public int OnlineNodesCount => RemoteNodes.Count(n => n.IsOnline);
        public int SelectedNodesCount => RemoteNodes.Count(n => n.IsSelected && n.IsOnline);
        public int TotalNodesCount => RemoteNodes.Count;

        private string _selectedPreset = "Safe";
        public string SelectedPreset
        {
            get => _selectedPreset;
            set { if (_selectedPreset != value) { _selectedPreset = value; OnPropertyChanged(); } }
        }

        public ObservableCollection<string> PresetList { get; } = new()
        {
            "Safe",
            "Deep",
            "Gaming",
            "Privacy",
            "ZeroTrace"
        };

        // Fleet Auto-Scheduler Properties
        private bool _isScheduleModalOpen;
        public bool IsScheduleModalOpen
        {
            get => _isScheduleModalOpen;
            set { if (_isScheduleModalOpen != value) { _isScheduleModalOpen = value; OnPropertyChanged(); } }
        }

        private string _scheduleFrequency = "WEEKLY";
        public string ScheduleFrequency
        {
            get => _scheduleFrequency;
            set { if (_scheduleFrequency != value) { _scheduleFrequency = value; OnPropertyChanged(); } }
        }

        private int _scheduleDay = 0; // 0=Sun, 1=Mon, 2=Tue, 3=Wed, 4=Thu, 5=Fri, 6=Sat
        public int ScheduleDay
        {
            get => _scheduleDay;
            set { if (_scheduleDay != value) { _scheduleDay = value; OnPropertyChanged(); } }
        }

        private int _scheduleHour = 3;
        public int ScheduleHour
        {
            get => _scheduleHour;
            set { if (_scheduleHour != value) { _scheduleHour = value; OnPropertyChanged(); } }
        }

        private int _scheduleMinute = 0;
        public int ScheduleMinute
        {
            get => _scheduleMinute;
            set { if (_scheduleMinute != value) { _scheduleMinute = value; OnPropertyChanged(); } }
        }

        private string _scheduleProfile = "Safe";
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

        public ObservableCollection<int> HoursList { get; } = new(Enumerable.Range(0, 24));
        public ObservableCollection<int> MinutesList { get; } = new(new[] { 0, 15, 30, 45 });

        // Account & Session Token Purge (Zero-Trace) Properties
        private bool _isAccountPurgeModalOpen;
        public bool IsAccountPurgeModalOpen
        {
            get => _isAccountPurgeModalOpen;
            set { if (_isAccountPurgeModalOpen != value) { _isAccountPurgeModalOpen = value; OnPropertyChanged(); } }
        }

        private string _accountPurgeScenario = "workplace"; // workplace, full, public, developer
        public string AccountPurgeScenario
        {
            get => _accountPurgeScenario;
            set { if (_accountPurgeScenario != value) { _accountPurgeScenario = value; OnPropertyChanged(); } }
        }

        private bool _terminateProcessesBeforePurge = true;
        public bool TerminateProcessesBeforePurge
        {
            get => _terminateProcessesBeforePurge;
            set { if (_terminateProcessesBeforePurge != value) { _terminateProcessesBeforePurge = value; OnPropertyChanged(); } }
        }

        // Special Fleet Operations Properties (Profile Sync, Browser Reset, Forensics Wipe)
        private bool _isSpecialOpsModalOpen;
        public bool IsSpecialOpsModalOpen
        {
            get => _isSpecialOpsModalOpen;
            set { if (_isSpecialOpsModalOpen != value) { _isSpecialOpsModalOpen = value; OnPropertyChanged(); } }
        }

        private string _specialOpCategory = "Profile"; // "Profile", "Browser", "Forensics"
        public string SpecialOpCategory
        {
            get => _specialOpCategory;
            set { if (_specialOpCategory != value) { _specialOpCategory = value; OnPropertyChanged(); } }
        }

        private string _browserResetTarget = "all"; // "all", "chrome", "edge", "whale", "firefox"
        public string BrowserResetTarget
        {
            get => _browserResetTarget;
            set { if (_browserResetTarget != value) { _browserResetTarget = value; OnPropertyChanged(); } }
        }

        private string _forensicsTargetDrive = "C:";
        public string ForensicsTargetDrive
        {
            get => _forensicsTargetDrive;
            set { if (_forensicsTargetDrive != value) { _forensicsTargetDrive = value; OnPropertyChanged(); } }
        }

        private bool _backupDefaultProfile = true;
        public bool BackupDefaultProfile
        {
            get => _backupDefaultProfile;
            set { if (_backupDefaultProfile != value) { _backupDefaultProfile = value; OnPropertyChanged(); } }
        }

        // Commands
        public ICommand DiscoverNodesCommand { get; }
        public ICommand ToggleAgentServerCommand { get; }
        public ICommand BatchPurgeRamCommand { get; }
        public ICommand BatchRestorePointCommand { get; }
        public ICommand BatchGameBoostCommand { get; }
        public ICommand BatchQuickMaintenanceCommand { get; }
        public ICommand BatchApplyPresetCommand { get; }
        public ICommand BatchFlushDnsCommand { get; }
        public ICommand BatchDiagnosticsCommand { get; }
        public ICommand BatchRollbackCommand { get; }
        public ICommand BatchZeroTraceResetCommand { get; }
        public ICommand OpenAccountPurgeModalCommand { get; }
        public ICommand CloseAccountPurgeModalCommand { get; }
        public ICommand BatchAccountPurgeCommand { get; }
        public ICommand OpenSpecialOpsModalCommand { get; }
        public ICommand CloseSpecialOpsModalCommand { get; }
        public ICommand BatchSyncDefaultProfileCommand { get; }
        public ICommand BatchBrowserFactoryResetCommand { get; }
        public ICommand BatchForensicsWipeCommand { get; }
        public ICommand BatchRebootCommand { get; }
        public ICommand BatchShutdownCommand { get; }
        public ICommand OpenScheduleModalCommand { get; }
        public ICommand CloseScheduleModalCommand { get; }
        public ICommand BatchDeployScheduleCommand { get; }
        public ICommand BatchUnregisterScheduleCommand { get; }
        public ICommand RefreshTelemetryCommand { get; }
        public ICommand SelectAllNodesCommand { get; }
        public ICommand DeselectAllNodesCommand { get; }
        public ICommand AddCustomNodeCommand { get; }

        // Per-Node Quick Actions
        public ICommand SingleNodeRamCommand { get; }
        public ICommand SingleNodeRestorePointCommand { get; }
        public ICommand SingleNodeQuickCleanCommand { get; }
        public ICommand SingleNodeGameBoostCommand { get; }
        public ICommand SingleNodeRollbackCommand { get; }
        public ICommand SingleNodeAccountPurgeCommand { get; }
        public ICommand SingleNodeProfileSyncCommand { get; }
        public ICommand SingleNodeBrowserResetCommand { get; }
        public ICommand SingleNodeForensicsWipeCommand { get; }
        public ICommand SingleNodeRebootCommand { get; }
        public ICommand SingleNodeShutdownCommand { get; }
        public ICommand RemoveNodeCommand { get; }

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
                    AddCommanderLog(value ? "[로그 설정] 중앙 관제 작업 로그 컴퓨터 디스크 자동 저장이 활성화되었습니다." : "[로그 설정] 중앙 관제 작업 로그 컴퓨터 디스크 자동 저장이 비활성화되었습니다.");
                }
            }
        }

        public string LogDirectoryPath => LoggerService.Instance.GetLogDirectory();
        public string CommanderLogFilePath => LoggerService.Instance.GetCommanderLogFilePath();

        public ICommand OpenCommanderLogFolderCommand { get; }
        public ICommand ExportCommanderLogsCommand { get; }
        public ICommand ClearCommanderLogsCommand { get; }
        public ICommand ToggleAutoSaveLogsCommand { get; }

        public CommanderViewModel()
        {
            DiscoverNodesCommand = new RelayCommand(async _ => await DiscoverNodesAsync(), _ => !IsScanning);
            ToggleAgentServerCommand = new RelayCommand(_ => ToggleAgentServer());
            BatchPurgeRamCommand = new RelayCommand(async _ => await ExecuteBatchPurgeRamAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            BatchRestorePointCommand = new RelayCommand(async _ => await ExecuteBatchRestorePointAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            BatchGameBoostCommand = new RelayCommand(async _ => await ExecuteBatchGameBoostAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            BatchQuickMaintenanceCommand = new RelayCommand(async _ => await ExecuteBatchQuickMaintenanceAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            BatchApplyPresetCommand = new RelayCommand(async _ => await ExecuteBatchApplyPresetAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            BatchFlushDnsCommand = new RelayCommand(async _ => await ExecuteBatchFlushDnsAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            BatchDiagnosticsCommand = new RelayCommand(async _ => await ExecuteBatchDiagnosticsAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            BatchRollbackCommand = new RelayCommand(async _ => await ExecuteBatchRollbackAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            BatchZeroTraceResetCommand = new RelayCommand(async _ => await ExecuteBatchZeroTraceResetAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            OpenAccountPurgeModalCommand = new RelayCommand(_ => IsAccountPurgeModalOpen = true);
            CloseAccountPurgeModalCommand = new RelayCommand(_ => IsAccountPurgeModalOpen = false);
            BatchAccountPurgeCommand = new RelayCommand(async _ => await ExecuteBatchAccountPurgeAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            
            OpenSpecialOpsModalCommand = new RelayCommand(param =>
            {
                if (param is string cat && !string.IsNullOrEmpty(cat))
                {
                    SpecialOpCategory = cat;
                }
                IsSpecialOpsModalOpen = true;
            });
            CloseSpecialOpsModalCommand = new RelayCommand(_ => IsSpecialOpsModalOpen = false);
            BatchSyncDefaultProfileCommand = new RelayCommand(async _ => await ExecuteBatchSyncDefaultProfileAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            BatchBrowserFactoryResetCommand = new RelayCommand(async _ => await ExecuteBatchBrowserFactoryResetAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            BatchForensicsWipeCommand = new RelayCommand(async _ => await ExecuteBatchForensicsWipeAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);

            BatchRebootCommand = new RelayCommand(async _ => await ExecuteBatchRebootAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            BatchShutdownCommand = new RelayCommand(async _ => await ExecuteBatchShutdownAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            OpenScheduleModalCommand = new RelayCommand(_ => IsScheduleModalOpen = true);
            CloseScheduleModalCommand = new RelayCommand(_ => IsScheduleModalOpen = false);
            BatchDeployScheduleCommand = new RelayCommand(async _ => await ExecuteBatchDeployScheduleAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);
            BatchUnregisterScheduleCommand = new RelayCommand(async _ => await ExecuteBatchUnregisterScheduleAsync(), _ => !IsExecutingBatch && SelectedNodesCount > 0);

            RefreshTelemetryCommand = new RelayCommand(async _ => await RefreshAllTelemetryAsync(), _ => !IsScanning);
            SelectAllNodesCommand = new RelayCommand(_ => SetAllSelection(true));
            DeselectAllNodesCommand = new RelayCommand(_ => SetAllSelection(false));
            AddCustomNodeCommand = new RelayCommand(param => AddCustomNode(param as string));

            // Single Node Quick Actions
            SingleNodeRamCommand = new RelayCommand(async param => await ExecuteSingleNodeAsync(param as RemoteNodeItem, "RamTrim"));
            SingleNodeRestorePointCommand = new RelayCommand(async param => await ExecuteSingleNodeAsync(param as RemoteNodeItem, "CreateRestorePoint"));
            SingleNodeQuickCleanCommand = new RelayCommand(async param => await ExecuteSingleNodeAsync(param as RemoteNodeItem, "QuickMaintenance", quickType: 0));
            SingleNodeGameBoostCommand = new RelayCommand(async param => await ExecuteSingleNodeAsync(param as RemoteNodeItem, "GameBoost"));
            SingleNodeRollbackCommand = new RelayCommand(async param => await ExecuteSingleNodeAsync(param as RemoteNodeItem, "Rollback"));
            SingleNodeAccountPurgeCommand = new RelayCommand(async param => await ExecuteSingleNodeAsync(param as RemoteNodeItem, "AccountPurge"));
            SingleNodeProfileSyncCommand = new RelayCommand(async param => await ExecuteSingleNodeAsync(param as RemoteNodeItem, "ProfileSync"));
            SingleNodeBrowserResetCommand = new RelayCommand(async param => await ExecuteSingleNodeAsync(param as RemoteNodeItem, "BrowserFactoryReset", browserTarget: BrowserResetTarget));
            SingleNodeForensicsWipeCommand = new RelayCommand(async param => await ExecuteSingleNodeAsync(param as RemoteNodeItem, "ForensicsWipe", drive: ForensicsTargetDrive));
            SingleNodeRebootCommand = new RelayCommand(async param => await ExecuteSingleNodeAsync(param as RemoteNodeItem, "Reboot"));
            SingleNodeShutdownCommand = new RelayCommand(async param => await ExecuteSingleNodeAsync(param as RemoteNodeItem, "Shutdown"));
            RemoveNodeCommand = new RelayCommand(param =>
            {
                if (param is RemoteNodeItem n)
                {
                    RemoteNodes.Remove(n);
                    NotifyCounts();
                    AddCommanderLog($"[Node 제거] {n.Name} ({n.IpAddress}) 관제 대시보드에서 제외됨");
                }
            });

            // Logging & File Auto-Save Commands (작업 log 기록 및 컴퓨터 자동 저장)
            OpenCommanderLogFolderCommand = new RelayCommand(_ =>
            {
                LoggerService.Instance.OpenLogFolder();
                AddCommanderLog($"[로그 탐색기] 컴퓨터 관제 로그 저장 폴더 열기: {LoggerService.Instance.GetLogDirectory()}");
            });

            ExportCommanderLogsCommand = new RelayCommand(_ =>
            {
                try
                {
                    string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string targetFile = System.IO.Path.Combine(desktop, $"WinPurifyCommander_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                    bool success = LoggerService.Instance.ExportLogs(targetFile, CommanderLogs, "WinPurify Central Commander Fleet Logs");
                    if (success)
                    {
                        AddCommanderLog($"[로그 내보내기] 관제 작업 로그를 바탕화면에 저장 완료: {System.IO.Path.GetFileName(targetFile)}");
                    }
                    else
                    {
                        AddCommanderLog("[로그 내보내기 오류] 로그 파일 저장 실패");
                    }
                }
                catch (Exception ex)
                {
                    AddCommanderLog($"[로그 내보내기 오류] {ex.Message}");
                }
            });

            ClearCommanderLogsCommand = new RelayCommand(_ =>
            {
                CommanderLogs.Clear();
                AddCommanderLog("[로그 초기화] 화면 상의 관제 로그 목록이 지워졌습니다. (컴퓨터 디스크 로그 파일은 영구 보존됨)");
            });

            ToggleAutoSaveLogsCommand = new RelayCommand(_ =>
            {
                AutoSaveLogs = !AutoSaveLogs;
            });

            // 기본 로컬 에이전트 서비스 로깅 바인딩
            RemoteAgentService.Instance.LogOccurred += msg => AddCommanderLog(msg);
            
            // 포터블 모드가 아닌 정식 설치 환경인 경우에만 로컬 에이전트 서비스 구동
            if (!IsPortableMode)
            {
                try
                {
                    RemoteAgentService.Instance.StartAgent();
                }
                catch { }
            }

            // 기본 상태 초기화
            IsAgentServerRunning = RemoteAgentService.Instance.IsRunning;
            if (IsPortableMode)
            {
                CommanderStatus = "포터블 모드: 원격 커맨더 명령 수신 비활성화됨 (클라이언트 전용)";
            }
            InitialSeed();
        }

        private void InitialSeed()
        {
            try
            {
                var telemetry = RemoteAgentService.Instance.CollectCurrentTelemetry();
                string bestIp = RemoteAgentService.GetBestLocalIPv4Address();

                // 기본 감지 로컬 노드
                RemoteNodes.Add(new RemoteNodeItem
                {
                    Name = $"{Environment.MachineName} (로컬 호스트)",
                    IpAddress = "127.0.0.1",
                    Port = RemoteAgentService.DefaultHttpPort,
                    GroupName = "관리자 콘솔 PC",
                    IsOnline = true,
                    CpuUsage = telemetry.CpuUsagePercent,
                    RamUsage = telemetry.RamUsagePercent,
                    RamText = telemetry.RamUsageText,
                    DiskText = telemetry.DiskFreeText,
                    OptimizationScore = telemetry.OptimizationScore,
                    AppliedRules = telemetry.AppliedRulesCount,
                    TotalRules = telemetry.TotalRulesCount,
                    LastSeen = IsPortableMode ? "포터블 로컬 (명령 수신 비활성화)" : "로컬 실행 중",
                    IsSelected = true
                });

                if (IsPortableMode)
                {
                    AddCommanderLog($"[Commander v{AppEnvironment.AppVersion}] WinPurify Pro 중앙 관제 엔진 초기화 (포터블 송신 모드, Option A 특수 기능 허브 온라인)");
                }
                else
                {
                    AddCommanderLog($"[Commander v{AppEnvironment.AppVersion}] WinPurify Pro 중앙 관제 엔진 초기화 완료 (HMAC-SHA256 보호, Option A 특수 기능 허브 대기, IP: {bestIp})");
                }
            }
            catch (Exception ex)
            {
                // 로컬 호스트 기본 노드 수동 추가
                RemoteNodes.Add(new RemoteNodeItem
                {
                    Name = $"{Environment.MachineName} (로컬 호스트)",
                    IpAddress = "127.0.0.1",
                    Port = RemoteAgentService.DefaultHttpPort,
                    GroupName = "관리자 콘솔 PC",
                    IsOnline = true,
                    LastSeen = "로컬 실행 중",
                    IsSelected = true
                });
                AddCommanderLog($"[Commander 초기화 안내] 로컬 텔레메트리 기본값 적용 ({ex.Message})");
            }
        }

        public void ToggleAgentServer()
        {
            if (IsPortableMode)
            {
                CommanderStatus = "포터블 버전은 보안 정책상 원격 명령을 수신하지 않습니다. (정식 설치 필요)";
                AddCommanderLog("[보안 정책] 포터블 실행 파일은 원격 명령 수신을 지원하지 않습니다. Windows 서비스가 자동 등록되는 정식 설치 버전을 이용하세요.");
                return;
            }

            if (RemoteAgentService.Instance.IsRunning)
            {
                RemoteAgentService.Instance.StopAgent();
                IsAgentServerRunning = false;
                CommanderStatus = "로컬 에이전트 수신 포트(TCP:9870)가 닫혔습니다.";
                AddCommanderLog("[Commander] 로컬 에이전트 수신(TCP:9870) 중지됨");
            }
            else
            {
                RemoteAgentService.Instance.StartAgent();
                IsAgentServerRunning = RemoteAgentService.Instance.IsRunning;
                CommanderStatus = IsAgentServerRunning 
                    ? "로컬 에이전트 수신 활성화됨 (TCP:9870, UDP:9871)" 
                    : "에이전트 시작 실패 (포트 점유 여부 확인 필요)";
                AddCommanderLog(IsAgentServerRunning
                    ? "[Commander] 로컬 에이전트 수신(TCP:9870, UDP:9871) 시작됨"
                    : "[Commander] 로컬 에이전트 수신 시작 실패");
            }
            OnPropertyChanged(nameof(AgentButtonText));
            OnPropertyChanged(nameof(AgentButtonBackground));
        }

        public async Task DiscoverNodesAsync()
        {
            if (IsScanning) return;
            IsScanning = true;
            CommanderStatus = "네트워크 상의 WinPurify Pro 에이전트 노드 자동 탐색 중...";
            AddCommanderLog("[Discovery] UDP 9871 브로드캐스트로 LAN 클러스터 노드 스캔 개시...");

            try
            {
                var found = await RemoteCommanderManager.Instance.DiscoverLocalNodesAsync();
                foreach (var node in found)
                {
                    if (!RemoteNodes.Any(n => n.IpAddress == node.IpAddress && n.Port == node.Port))
                    {
                        RemoteNodes.Add(node);
                        AddCommanderLog($"[Discovery 발견] 새 노드 연결: {node.Name} ({node.IpAddress}:{node.Port})");
                    }
                }

                await RefreshAllTelemetryAsync();
                CommanderStatus = $"노드 탐색 완료 (총 {RemoteNodes.Count}대 등록됨)";
            }
            catch (Exception ex)
            {
                CommanderStatus = $"탐색 오류: {ex.Message}";
            }
            finally
            {
                IsScanning = false;
                NotifyCounts();
            }
        }

        public async Task RefreshAllTelemetryAsync()
        {
            foreach (var node in RemoteNodes.ToList())
            {
                var tel = await RemoteCommanderManager.Instance.FetchTelemetryAsync(node.IpAddress, node.Port);
                if (tel != null)
                {
                    node.IsOnline = true;
                    // 원격 에이전트가 보고한 실제 LAN IPv4가 유효하면 루프백 또는 가상 IP를 실제 물리 IP로 자동 보정
                    if (!string.IsNullOrWhiteSpace(tel.ClientIp) && !tel.ClientIp.StartsWith("127.") && !tel.ClientIp.StartsWith("169.254."))
                    {
                        if (node.IpAddress.StartsWith("127.") || node.IpAddress.StartsWith("::1"))
                        {
                            node.IpAddress = tel.ClientIp;
                        }
                    }

                    node.CpuUsage = tel.CpuUsagePercent;
                    node.RamUsage = tel.RamUsagePercent;
                    node.RamText = tel.RamUsageText;
                    node.DiskText = tel.DiskFreeText;
                    node.OptimizationScore = tel.OptimizationScore;
                    node.AppliedRules = tel.AppliedRulesCount;
                    node.TotalRules = tel.TotalRulesCount;
                    node.LastSeen = "방금 동기화됨";
                }
                else
                {
                    node.LastSeen = "응답 대기";
                }
            }
            NotifyCounts();
        }

        public async Task ExecuteBatchPurgeRamAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (!targets.Any() || IsExecutingBatch) return;

            IsExecutingBatch = true;
            CommanderStatus = $"선택된 {targets.Count}대 PC에 C++ Native RAM 일괄 정화 명령 전송 중...";
            AddCommanderLog($"[Batch Action] {targets.Count}대 노드에 동시 물리 RAM 압축(Trim) 브로드캐스트 전송");

            var results = await RemoteCommanderManager.Instance.BatchPurgeRamAsync(targets);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"일괄 RAM 정화 완료 (성공: {success}/{targets.Count}대)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message} ({res.Response.ExecutionTimeMs}ms)");
            }

            await RefreshAllTelemetryAsync();
            IsExecutingBatch = false;
        }

        public async Task ExecuteBatchRestorePointAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (!targets.Any() || IsExecutingBatch) return;

            IsExecutingBatch = true;
            CommanderStatus = $"선택된 {targets.Count}대 PC에 시스템 복원 지점 생성 및 보호 기능 활성화 명령 수행 중...";
            AddCommanderLog($"[Batch Action] {targets.Count}대 노드에 Windows 시스템 복원 지점 생성 (시스템 보호 비활성화 시 자동 활성화)");

            var results = await RemoteCommanderManager.Instance.BatchCreateRestorePointAsync(targets);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"일괄 복원지점 생성 완료 (성공: {success}/{targets.Count}대)";
            foreach (var res in results)
            {
                string icon = res.Response.Success ? "✅" : "⚠️";
                AddCommanderLog($"  └ {icon} [{res.Node.Name}] {res.Response.Message} ({res.Response.ExecutionTimeMs}ms)");
            }

            IsExecutingBatch = false;
        }

        public async Task ExecuteBatchGameBoostAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (!targets.Any() || IsExecutingBatch) return;

            IsExecutingBatch = true;
            CommanderStatus = $"선택된 {targets.Count}대 PC에 게이밍 부스트 & 지연시간 단축 모드 배포 중...";
            AddCommanderLog($"[Batch Action] {targets.Count}대 노드에 e스포츠 게이밍 가속(프로세스 우선순위 HIGH, RAM 압축) 전송");

            var results = await RemoteCommanderManager.Instance.BatchGameBoostAsync(targets);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"게이밍 부스트 일괄 배포 완료 (성공: {success}/{targets.Count}대)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message}");
            }

            await RefreshAllTelemetryAsync();
            IsExecutingBatch = false;
        }

        public async Task ExecuteBatchQuickMaintenanceAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (!targets.Any() || IsExecutingBatch) return;

            IsExecutingBatch = true;
            CommanderStatus = $"선택된 {targets.Count}대 PC에 C++ 고속 유지보수(휴지통/DNS/Temp) 일괄 실행 중...";
            AddCommanderLog($"[Batch Action] {targets.Count}대 노드에 통합 고속 하우스키핑 루틴 브로드캐스트 전송");

            var results = await RemoteCommanderManager.Instance.BatchQuickMaintenanceAsync(targets, 0);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"고속 유지보수 일괄 완료 (성공: {success}/{targets.Count}대)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message} ({res.Response.ExecutionTimeMs}ms)");
            }

            await RefreshAllTelemetryAsync();
            IsExecutingBatch = false;
        }

        public async Task ExecuteBatchApplyPresetAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (!targets.Any() || IsExecutingBatch) return;

            IsExecutingBatch = true;
            string preset = SelectedPreset ?? "Safe";
            CommanderStatus = $"선택된 {targets.Count}대 PC에 [{preset}] 프리셋 일괄 배포 중...";
            AddCommanderLog($"[Batch Action] {targets.Count}대 노드에 [{preset}] 최적화 프로필 패키지 배포 명령 하달");

            var results = await RemoteCommanderManager.Instance.BatchApplyPresetAsync(targets, preset);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"[{preset}] 프리셋 일괄 배포 완료 (성공: {success}/{targets.Count}대)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message} ({res.Response.ExecutionTimeMs}ms)");
            }

            await RefreshAllTelemetryAsync();
            IsExecutingBatch = false;
        }

        public async Task ExecuteBatchFlushDnsAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (!targets.Any() || IsExecutingBatch) return;

            IsExecutingBatch = true;
            CommanderStatus = $"선택된 {targets.Count}대 PC에 DNS 캐시 플러시 & 네트워크 갱신 중...";
            AddCommanderLog($"[Batch Action] {targets.Count}대 노드에 DNS 캐시 초기화 및 Winsock 소켓 갱신 전송");

            var results = await RemoteCommanderManager.Instance.BatchFlushDnsAsync(targets);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"DNS 플러시 완료 (성공: {success}/{targets.Count}대)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message}");
            }

            IsExecutingBatch = false;
        }

        public async Task ExecuteBatchDiagnosticsAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (!targets.Any() || IsExecutingBatch) return;

            IsExecutingBatch = true;
            CommanderStatus = $"선택된 {targets.Count}대 PC 상태 및 무결성 자가진단 수행 중...";
            AddCommanderLog($"[Batch Action] {targets.Count}대 노드에 헬스체크 및 144개 모듈 무결성 점검 요청");

            var results = await RemoteCommanderManager.Instance.BatchRunDiagnosticsAsync(targets);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"원격 자가진단 완료 (성공: {success}/{targets.Count}대)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message}");
            }

            await RefreshAllTelemetryAsync();
            IsExecutingBatch = false;
        }

        public async Task ExecuteBatchRebootAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (!targets.Any() || IsExecutingBatch) return;

            IsExecutingBatch = true;
            CommanderStatus = $"선택된 {targets.Count}대 PC에 원격 강제 재부팅 명령 전송 중...";
            AddCommanderLog($"[Batch Action 긴급] {targets.Count}대 노드에 4-Tier 시스템 강제 재부팅(/r /f /t 3) 명령 하달");

            var results = await RemoteCommanderManager.Instance.BatchRebootAsync(targets);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"원격 강제 재부팅 명령 하달 완료 (전송: {success}/{targets.Count}대)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message}");
            }

            IsExecutingBatch = false;
        }

        public async Task ExecuteBatchShutdownAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (!targets.Any() || IsExecutingBatch) return;

            IsExecutingBatch = true;
            CommanderStatus = $"선택된 {targets.Count}대 PC에 원격 강제 시스템 종료 명령 전송 중...";
            AddCommanderLog($"[Batch Action 긴급] {targets.Count}대 노드에 4-Tier 시스템 강제 종료(/s /f /t 3) 명령 하달");

            var results = await RemoteCommanderManager.Instance.BatchShutdownAsync(targets);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"원격 강제 시스템 종료 명령 하달 완료 (전송: {success}/{targets.Count}대)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message}");
            }

            IsExecutingBatch = false;
        }

        private async Task ExecuteBatchRollbackAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (targets.Count == 0 || IsExecutingBatch) return;

            IsExecutingBatch = true;
            CommanderStatus = $"선택된 {targets.Count}대 PC 세이프포인트(백업 레지스트리) 원격 롤백 실행 중...";
            AddCommanderLog($"[Fleet Action] {targets.Count}개 노드 대상 최신 세이프포인트 1-Click 롤백 복원 하달");

            var results = await RemoteCommanderManager.Instance.BatchRollbackSafepointAsync(targets);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"세이프포인트 롤백 완료 ({success}/{targets.Count}대 복원 성공)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message}");
            }

            IsExecutingBatch = false;
            await RefreshAllTelemetryAsync();
        }

        private async Task ExecuteBatchZeroTraceResetAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (targets.Count == 0 || IsExecutingBatch) return;

            IsExecutingBatch = true;
            CommanderStatus = $"선택된 {targets.Count}대 PC Zero-Trace 안티포렌식 완전 초기화 배포 중...";
            AddCommanderLog($"[Fleet Action] {targets.Count}개 노드 대상 Zero-Trace 완전 초기화(흔적/자격증명/세션 영구 소거) 하달");

            var results = await RemoteCommanderManager.Instance.BatchZeroTraceResetAsync(targets);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"Zero-Trace 완전 초기화 완료 ({success}/{targets.Count}대 적용 성공)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message}");
            }

            IsExecutingBatch = false;
            await RefreshAllTelemetryAsync();
        }

        private async Task ExecuteBatchAccountPurgeAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (targets.Count == 0 || IsExecutingBatch) return;

            IsExecutingBatch = true;
            string scenarioName = AccountPurgeScenario.ToLowerInvariant() switch
            {
                "workplace" => "사무실/업무 PC 반납 (Workplace)",
                "full" => "PC 양도 / 판매 (Zero-Trace 전체)",
                "public" => "공용 PC / PC방 이용 후 정리 (Public)",
                "developer" => "개발자 / 보안 점검 (Developer)",
                _ => AccountPurgeScenario
            };

            CommanderStatus = $"선택된 {targets.Count}대 PC 통합 계정 & 세션 토큰 원격 정화 배포 중 ({scenarioName})...";
            AddCommanderLog($"[Fleet Action] {targets.Count}개 노드 대상 계정·세션 정화 하달 (시나리오: {scenarioName}, 프로세스 사전종료: {(TerminateProcessesBeforePurge ? "ON" : "OFF")})");

            var results = await RemoteCommanderManager.Instance.BatchPurgeAccountSessionsAsync(
                targets,
                AccountPurgeScenario,
                TerminateProcessesBeforePurge);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"계정 & 세션 토큰 정화 완료 ({success}/{targets.Count}대 적용 성공)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message}");
            }

            IsExecutingBatch = false;
            IsAccountPurgeModalOpen = false;
            await RefreshAllTelemetryAsync();
        }

        private async Task ExecuteBatchSyncDefaultProfileAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (targets.Count == 0 || IsExecutingBatch) return;

            IsExecutingBatch = true;
            CommanderStatus = $"선택된 {targets.Count}대 PC에 Windows 기본 프로필(Default) 덮어쓰기 복제 배포 중...";
            AddCommanderLog($"[Fleet Action] {targets.Count}개 노드 대상 Windows 기본 프로필(Default) 복제 배포 하달 (기존 원본 백업: {(BackupDefaultProfile ? "ON" : "OFF")})");

            var results = await RemoteCommanderManager.Instance.BatchSyncDefaultProfileAsync(targets, BackupDefaultProfile);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"기본 프로필 복제 완료 ({success}/{targets.Count}대 배포 성공)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message}");
            }

            IsExecutingBatch = false;
            IsSpecialOpsModalOpen = false;
            await RefreshAllTelemetryAsync();
        }

        private async Task ExecuteBatchBrowserFactoryResetAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (targets.Count == 0 || IsExecutingBatch) return;

            IsExecutingBatch = true;
            string bTargetName = BrowserResetTarget.ToLowerInvariant() switch
            {
                "all" => "전체 브라우저 (Chrome, Edge, Whale, Firefox)",
                "chrome" => "Google Chrome",
                "edge" => "Microsoft Edge",
                "whale" => "Naver Whale",
                "firefox" => "Mozilla Firefox",
                _ => BrowserResetTarget
            };

            CommanderStatus = $"선택된 {targets.Count}대 PC에 브라우저 공장 초기화({bTargetName}) 배포 중...";
            AddCommanderLog($"[Fleet Action 경고] {targets.Count}개 노드 대상 브라우저 공장 초기화 하달 ({bTargetName} 프로필 영구 소거)");

            var results = await RemoteCommanderManager.Instance.BatchBrowserFactoryResetAsync(targets, BrowserResetTarget);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"브라우저 공장 초기화 완료 ({success}/{targets.Count}대 초기화 성공)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message}");
            }

            IsExecutingBatch = false;
            IsSpecialOpsModalOpen = false;
            await RefreshAllTelemetryAsync();
        }

        private async Task ExecuteBatchForensicsWipeAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (targets.Count == 0 || IsExecutingBatch) return;

            IsExecutingBatch = true;
            string drive = string.IsNullOrWhiteSpace(ForensicsTargetDrive) ? "C:" : ForensicsTargetDrive.Trim();

            CommanderStatus = $"선택된 {targets.Count}대 PC에 드라이브 {drive} 포렌식 난수 파쇄(cipher /w) 백그라운드 구동 중...";
            AddCommanderLog($"[Fleet Action] {targets.Count}개 노드 대상 디스크 {drive} 포렌식 빈 공간 난수 덮어쓰기 하달 (Recuva 등 복원 차단)");

            var results = await RemoteCommanderManager.Instance.BatchForensicsWipeAsync(targets, drive);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"포렌식 난수 파쇄 명령 접수 완료 ({success}/{targets.Count}대 백그라운드 시작됨)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message}");
            }

            IsExecutingBatch = false;
            IsSpecialOpsModalOpen = false;
        }

        private async Task ExecuteBatchDeployScheduleAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (targets.Count == 0 || IsExecutingBatch) return;

            IsExecutingBatch = true;
            string dayStr = ScheduleDay switch { 1 => "월요일", 2 => "화요일", 3 => "수요일", 4 => "목요일", 5 => "금요일", 6 => "토요일", _ => "일요일" };
            string freqStr = ScheduleFrequency.Equals("DAILY", StringComparison.OrdinalIgnoreCase) ? "매일" : $"매주 {dayStr}";
            
            CommanderStatus = $"선택된 {targets.Count}대 PC에 주간 자동 스케줄러 일괄 배포 중...";
            AddCommanderLog($"[Fleet Schedule] {targets.Count}대 노드 대상 자동 스케줄러 배포 하달 ({freqStr} {ScheduleHour:D2}:{ScheduleMinute:D2}, 프로필: {ScheduleProfile}, RAM압축: {(ScheduleAutoTrimRam ? "ON" : "OFF")})");

            var results = await RemoteCommanderManager.Instance.BatchConfigureScheduleAsync(
                targets,
                ScheduleFrequency,
                ScheduleDay,
                ScheduleHour,
                ScheduleMinute,
                ScheduleProfile,
                ScheduleAutoTrimRam);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"주간 스케줄러 일괄 배포 완료 ({success}/{targets.Count}대 등록 성공)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message}");
            }

            IsExecutingBatch = false;
            IsScheduleModalOpen = false;
        }

        private async Task ExecuteBatchUnregisterScheduleAsync()
        {
            var targets = RemoteNodes.Where(n => n.IsSelected && n.IsOnline).ToList();
            if (targets.Count == 0 || IsExecutingBatch) return;

            IsExecutingBatch = true;
            CommanderStatus = $"선택된 {targets.Count}대 PC 자동 스케줄러 등록 일괄 해제 중...";
            AddCommanderLog($"[Fleet Schedule] {targets.Count}대 노드 대상 스케줄러 작업 일괄 삭제 하달");

            var results = await RemoteCommanderManager.Instance.BatchUnregisterScheduleAsync(targets);

            int success = results.Count(r => r.Response.Success);
            CommanderStatus = $"스케줄러 일괄 등록 해제 완료 ({success}/{targets.Count}대 해제 성공)";
            foreach (var res in results)
            {
                AddCommanderLog($"  └ [{res.Node.Name}] {res.Response.Message}");
            }

            IsExecutingBatch = false;
            IsScheduleModalOpen = false;
        }

        public async Task ExecuteSingleNodeAsync(
            RemoteNodeItem? node,
            string commandType,
            string? presetName = null,
            int quickType = 0,
            string? accountPreset = "workplace",
            bool terminateProcesses = true,
            string? browserTarget = "all",
            string? drive = "C:")
        {
            if (node == null || IsExecutingBatch) return;

            AddCommanderLog($"[Single Action] 노드 [{node.Name}] ({node.IpAddress})에 개별 명령 전송: {commandType}");
            CommanderStatus = $"[{node.Name}]에 {commandType} 명령 전송 중...";

            var resp = await RemoteCommanderManager.Instance.SendSingleNodeCommandAsync(
                node,
                commandType,
                presetName,
                quickType,
                accountPreset,
                terminateProcesses,
                browserTarget,
                drive);

            if (resp.Success)
            {
                CommanderStatus = $"[{node.Name}] 명령 실행 성공: {resp.Message}";
                AddCommanderLog($"  └ [{node.Name}] {resp.Message} ({resp.ExecutionTimeMs}ms)");
            }
            else
            {
                CommanderStatus = $"[{node.Name}] 오류: {resp.Message}";
                AddCommanderLog($"  └ [오류] [{node.Name}] {resp.Message}");
            }

            // 개별 텔레메트리 갱신
            var tel = await RemoteCommanderManager.Instance.FetchTelemetryAsync(node.IpAddress, node.Port);
            if (tel != null)
            {
                node.CpuUsage = tel.CpuUsagePercent;
                node.RamUsage = tel.RamUsagePercent;
                node.RamText = tel.RamUsageText;
                node.DiskText = tel.DiskFreeText;
                node.OptimizationScore = tel.OptimizationScore;
            }
        }

        public void AddCustomNode(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) input = "192.168.1.100";
            input = input.Trim();

            string host = input;
            int port = 9870;

            if (input.Contains(':'))
            {
                var parts = input.Split(':');
                host = parts[0];
                if (parts.Length > 1 && int.TryParse(parts[1], out int p)) port = p;
            }

            // 호스트명이 입력된 경우 실제 IPv4 주소 자동 해석
            string resolvedIp = host;
            try
            {
                if (!IPAddress.TryParse(host, out _))
                {
                    var addresses = Dns.GetHostAddresses(host);
                    var ipv4 = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
                    if (ipv4 != null)
                    {
                        resolvedIp = ipv4.ToString();
                        AddCommanderLog($"[DNS 해석] 호스트 '{host}' -> 정확한 IPv4: {resolvedIp}");
                    }
                }
            }
            catch { }

            RemoteNodes.Add(new RemoteNodeItem
            {
                Name = $"PC-{host.ToUpperInvariant()}",
                IpAddress = resolvedIp,
                Port = port,
                GroupName = "수동 등록 PC",
                IsOnline = true,
                CpuUsage = 0,
                RamUsage = 0,
                RamText = "조회 대기",
                DiskText = "조회 대기",
                OptimizationScore = 100,
                LastSeen = "수동 추가됨",
                IsSelected = true
            });
            AddCommanderLog($"[Node 추가] PC 노드 등록 완료 ({resolvedIp}:{port})");
            NotifyCounts();
        }

        private void SetAllSelection(bool isSelected)
        {
            foreach (var node in RemoteNodes)
            {
                node.IsSelected = isSelected;
            }
            NotifyCounts();
        }

        public void AddCommanderLog(string msg)
        {
            // 1. 컴퓨터 디스크 파일에 실시간 자동 저장
            if (AutoSaveLogs)
            {
                LoggerService.Instance.LogCommander(msg);
            }

            // 2. UI 실시간 콘솔 갱신
            var app = System.Windows.Application.Current;
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string entry = $"[{timestamp}] {msg}";

            if (app?.Dispatcher != null && !app.Dispatcher.CheckAccess())
            {
                app.Dispatcher.BeginInvoke(() =>
                {
                    CommanderLogs.Insert(0, entry);
                    if (CommanderLogs.Count > 120) CommanderLogs.RemoveAt(CommanderLogs.Count - 1);
                });
            }
            else
            {
                CommanderLogs.Insert(0, entry);
                if (CommanderLogs.Count > 120) CommanderLogs.RemoveAt(CommanderLogs.Count - 1);
            }
        }

        private void NotifyCounts()
        {
            OnPropertyChanged(nameof(OnlineNodesCount));
            OnPropertyChanged(nameof(SelectedNodesCount));
            OnPropertyChanged(nameof(TotalNodesCount));
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
