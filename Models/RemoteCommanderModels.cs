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
    }

    public class CommandResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ExecutionDetails { get; set; } = string.Empty;
        public long ExecutionTimeMs { get; set; }
    }

    public class RemoteNodeItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "Unknown Node";
        public string IpAddress { get; set; } = "127.0.0.1";
        public string ClientIp { get => IpAddress; set => IpAddress = value; }
        public int Port { get; set; } = 9870;
        public string GroupName { get; set; } = "기본 그룹";
        public bool IsOnline { get; set; }
        public double CpuUsage { get; set; }
        public double RamUsage { get; set; }
        public string RamText { get; set; } = "0 / 0 GB";
        public string DiskText { get; set; } = "0 GB Free";
        public int OptimizationScore { get; set; } = 100;
        public int AppliedRules { get; set; }
        public int TotalRules { get; set; } = 144;
        public string LastSeen { get; set; } = "방금 전";
        public bool IsSelected { get; set; } = true;
    }
}
