using System;
using System.Collections.Generic;

namespace WinPurifyPro.Models
{
    public class NodeTelemetryDto
    {
        public string NodeName { get; set; } = Environment.MachineName;
        public string OsVersion { get; set; } = Environment.OSVersion.ToString();
        public string ClientIp { get; set; } = string.Empty;
        public double CpuUsagePercent { get; set; }
        public double RamUsagePercent { get; set; }
        public string RamUsageText { get; set; } = string.Empty;
        public string DiskFreeText { get; set; } = string.Empty;
        public int AppliedRulesCount { get; set; }
        public int TotalRulesCount { get; set; } = 144;
        public int OptimizationScore { get; set; }
        public bool IsNativeCoreActive { get; set; }
        public string LastCleanTime { get; set; } = "미실행";
        public string AgentStatus { get; set; } = "Online";
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public class BatchCommandRequest
    {
        public string CommandType { get; set; } = string.Empty; // "RamTrim", "CreateRestorePoint", "QuickMaintenance", "ApplyPreset", "GameBoost", "FlushDns", "Diagnostics", "Rollback", "ZeroTraceReset", "AccountPurge", "ProfileSync", "BrowserFactoryReset", "ForensicsWipe", "Reboot", "Shutdown"
        public string? PresetName { get; set; } // "Safe", "Deep", "Gaming", "Privacy", "ZeroTrace"
        public int QuickType { get; set; } // 0: All(통합 고속 정화), 1: RecycleBin(휴지통), 2: DnsFlush(DNS 플러시), 3: TempPurge(임시파일)
        public string? AccountPurgePreset { get; set; } = "workplace"; // "workplace", "full", "public", "developer"
        public bool TerminateProcessesBeforePurge { get; set; } = true;
        public List<string>? TargetAccountIds { get; set; }
        public string? BrowserTarget { get; set; } = "all"; // "all", "chrome", "edge", "whale", "firefox"
        public string? ForensicsTargetDrive { get; set; } = "C:"; // e.g. "C:"
        public bool BackupExistingDefaultProfile { get; set; } = true;
        public string? ScheduleFrequency { get; set; } = "WEEKLY"; // "WEEKLY", "DAILY", "STARTUP", "INTERVAL"
        public int ScheduleIntervalHours { get; set; } = 4;
        public int ScheduleStartupDelayMinutes { get; set; } = 1;
        public string? ScheduleCustomModules { get; set; }
        public bool ScheduleCreateSafepoint { get; set; } = true;
        public int ScheduleDay { get; set; } = 0; // 0=Sun, 1=Mon, ..., 6=Sat
        public int ScheduleHour { get; set; } = 3;
        public int ScheduleMinute { get; set; } = 0;
        public string? ScheduleProfile { get; set; } = "Safe";
        public bool ScheduleAutoTrimRam { get; set; } = true;
        public string SecretKey { get; set; } = string.Empty;
        public long Timestamp { get; set; }
        public string Nonce { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public string? ChallengeNonce { get; set; }
        public string? ChallengeSignature { get; set; }
    }

    public class AuthChallengeResponseDto
    {
        public bool Success { get; set; } = true;
        public string ChallengeNonce { get; set; } = string.Empty;
        public long ExpiresInSeconds { get; set; } = 60;
        public long ServerTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public string Message { get; set; } = "Challenge nonce issued successfully";
    }

    public class CommandResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ExecutionDetails { get; set; } = string.Empty;
        public long ExecutionTimeMs { get; set; }
        public string? AuthErrorPoint { get; set; }
        public string? AuthCategory { get; set; }
        public long HandshakeLatencyMs { get; set; }
    }

    public class RemoteNodeItem : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));

        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        
        private string _name = "Unknown Node";
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _ipAddress = "127.0.0.1";
        public string IpAddress { get => _ipAddress; set { _ipAddress = value; OnPropertyChanged(); OnPropertyChanged(nameof(ClientIp)); } }
        public string ClientIp { get => IpAddress; set => IpAddress = value; }

        private int _port = 9870;
        public int Port { get => _port; set { _port = value; OnPropertyChanged(); } }

        private string _groupName = "기본 그룹";
        public string GroupName { get => _groupName; set { _groupName = value; OnPropertyChanged(); } }

        private bool _isOnline;
        public bool IsOnline { get => _isOnline; set { _isOnline = value; OnPropertyChanged(); } }

        private double _cpuUsage;
        public double CpuUsage { get => _cpuUsage; set { _cpuUsage = value; OnPropertyChanged(); } }

        private double _ramUsage;
        public double RamUsage { get => _ramUsage; set { _ramUsage = value; OnPropertyChanged(); } }

        private string _ramText = "0 / 0 GB";
        public string RamText { get => _ramText; set { _ramText = value; OnPropertyChanged(); } }

        private string _diskText = "0 GB Free";
        public string DiskText { get => _diskText; set { _diskText = value; OnPropertyChanged(); } }

        private int _optimizationScore = 100;
        public int OptimizationScore { get => _optimizationScore; set { _optimizationScore = value; OnPropertyChanged(); } }

        private int _appliedRules;
        public int AppliedRules { get => _appliedRules; set { _appliedRules = value; OnPropertyChanged(); } }

        private int _totalRules = 144;
        public int TotalRules { get => _totalRules; set { _totalRules = value; OnPropertyChanged(); } }

        private string _lastSeen = "방금 전";
        public string LastSeen { get => _lastSeen; set { _lastSeen = value; OnPropertyChanged(); } }

        private string _macAddress = string.Empty;
        public string MacAddress { get => _macAddress; set { _macAddress = value; OnPropertyChanged(); } }

        private uint _pingMs;
        public uint PingMs { get => _pingMs; set { _pingMs = value; OnPropertyChanged(); } }

        private bool _isSelected = true;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }

        private long _handshakeLatencyMs;
        public long HandshakeLatencyMs { get => _handshakeLatencyMs; set { _handshakeLatencyMs = value; OnPropertyChanged(); } }

        private string _lastAuthErrorPoint = "None";
        public string LastAuthErrorPoint { get => _lastAuthErrorPoint; set { _lastAuthErrorPoint = value; OnPropertyChanged(); } }

        private string _lastAuthCategory = "None";
        public string LastAuthCategory { get => _lastAuthCategory; set { _lastAuthCategory = value; OnPropertyChanged(); } }
    }
}
