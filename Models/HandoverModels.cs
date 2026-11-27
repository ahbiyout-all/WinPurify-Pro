using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinPurifyPro.Models
{
    public enum HandoverPresetMode
    {
        PreviousUsersAndSessions,
        SessionsOnly,
        FullMachineReset
    }

    public enum HandoverScanPhase
    {
        Idle,
        Scanning,
        Ready,
        Purging,
        Verified
    }

    public enum WorkspaceViewMode
    {
        Handover,
        Modules144,
        Commander,
        Diagnostics
    }

    public enum HandoverDomainCategory
    {
        UserProfile,
        BrowserSession,
        AppSession,
        CredentialAndNetwork,
        PersonalFolder
    }

    public class HandoverExclusionRule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string RuleType { get; set; } = "PathPrefix"; // "ArtifactId" | "PathPrefix"
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class HandoverProgressReport
    {
        public int StepIndex { get; set; }
        public string StepTitle { get; set; } = string.Empty;
        public int Percent { get; set; }
        public string CurrentItemLabel { get; set; } = string.Empty;
        public string? LogLine { get; set; }
    }

    public class HandoverPurgedItemSnapshot
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public long EstimatedBytes { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PathsSummary { get; set; } = string.Empty;
    }

    public class DetectedUserProfile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public string Sid { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string ProfilePath { get; set; } = string.Empty;
        public string AccountType { get; set; } = "로컬 계정";
        public string LastUsedTimeText { get; set; } = "-";
        public bool IsCurrentSession { get; set; }
        public bool IsSystemProtected { get; set; }
        public bool IsDeletable => !IsSystemProtected && !IsCurrentSession;
        public bool IsOrphanedDirectory { get; set; }
        public bool HasNtUserDat { get; set; }
        public int AppDataSubdirCount { get; set; }
        public string ProtectionReason { get; set; } = string.Empty;
        public bool PendingRebootScheduled { get; set; }
        public string LockedHints { get; set; } = string.Empty;

        private long _estimatedBytes;
        public long EstimatedBytes
        {
            get => _estimatedBytes;
            set
            {
                if (_estimatedBytes != value)
                {
                    _estimatedBytes = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FormattedSize));
                }
            }
        }

        public string FormattedSize => FormatBytes(EstimatedBytes);

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                bool target = IsSystemProtected || IsCurrentSession ? false : value;
                if (_isSelected != target)
                {
                    _isSelected = target;
                    OnPropertyChanged();
                }
            }
        }

        private string _statusText = "대기";
        public string StatusText
        {
            get => _statusText;
            set { if (_statusText != value) { _statusText = value; OnPropertyChanged(); } }
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes <= 0) return "0 MB";
            if (bytes < 1024L * 1024L) return $"{bytes / 1024.0:F0} KB";
            if (bytes < 1024L * 1024L * 1024L) return $"{bytes / (1024.0 * 1024.0):F1} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
        }
    }

    public class DetectedSessionArtifact : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public string ArtifactId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public HandoverDomainCategory DomainCategory { get; set; }
        public string CategoryLabel { get; set; } = string.Empty;
        public string UserScope { get; set; } = string.Empty;
        public string TargetPathsSummary { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string[] AssociatedProcessNames { get; set; } = Array.Empty<string>();
        public List<string> MatchedPaths { get; set; } = new();
        public List<string> CredentialKeywords { get; set; } = new();
        public List<string> RegistryDeleteCommands { get; set; } = new();
        public List<string> PendingRebootPaths { get; set; } = new();
        public string LockedHints { get; set; } = string.Empty;

        private bool _isProtected;
        public bool IsProtected
        {
            get => _isProtected;
            set
            {
                if (_isProtected != value)
                {
                    _isProtected = value;
                    if (value) _isSelected = false;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsSelectable));
                }
            }
        }

        private bool _isExcludedByWhitelist;
        public bool IsExcludedByWhitelist
        {
            get => _isExcludedByWhitelist;
            set
            {
                if (_isExcludedByWhitelist != value)
                {
                    _isExcludedByWhitelist = value;
                    if (value) _isSelected = false;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsSelectable));
                }
            }
        }

        public string ExclusionReason { get; set; } = string.Empty;
        public bool IsSelectable => !IsProtected && !IsExcludedByWhitelist;

        private int _detectedCount;
        public int DetectedCount
        {
            get => _detectedCount;
            set { if (_detectedCount != value) { _detectedCount = value; OnPropertyChanged(); } }
        }

        private long _estimatedBytes;
        public long EstimatedBytes
        {
            get => _estimatedBytes;
            set
            {
                if (_estimatedBytes != value)
                {
                    _estimatedBytes = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FormattedSize));
                }
            }
        }

        public string FormattedSize => DetectedUserProfile.FormatBytes(EstimatedBytes);

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                bool target = (IsProtected || IsExcludedByWhitelist) ? false : value;
                if (_isSelected != target)
                {
                    _isSelected = target;
                    OnPropertyChanged();
                }
            }
        }

        private string _statusText = "대기";
        public string StatusText
        {
            get => _statusText;
            set { if (_statusText != value) { _statusText = value; OnPropertyChanged(); } }
        }

        public string AssociatedProcessesSummary => AssociatedProcessNames.Length > 0
            ? string.Join(", ", AssociatedProcessNames)
            : "연관 백그라운드 프로세스 없음";
    }

    public class HandoverScanResult
    {
        public DateTime ScannedAt { get; set; } = DateTime.Now;
        public List<DetectedUserProfile> Profiles { get; set; } = new();
        public List<DetectedSessionArtifact> Artifacts { get; set; } = new();
        public long TotalReclaimableBytes { get; set; }
        public int PreviousUserProfilesCount { get; set; }
        public int ActiveSessionsCount { get; set; }
    }

    public class HandoverVerificationSummary
    {
        public DateTime CompletedAt { get; set; } = DateTime.Now;
        public string MachineName { get; set; } = Environment.MachineName;
        public string OperatorUser { get; set; } = Environment.UserName;
        public string PresetUsed { get; set; } = nameof(HandoverPresetMode.PreviousUsersAndSessions);
        public int PurgedProfilesCount { get; set; }
        public int PurgedSessionsCount { get; set; }
        public long ReclaimedBytes { get; set; }
        public int RemainingPreviousProfiles { get; set; }
        public int RemainingActiveSessions { get; set; }
        public int ExcludedItemsCount { get; set; }
        public bool IsCleanHandoverVerified { get; set; }
        public string SummaryMessage { get; set; } = string.Empty;
        public List<string> PendingRebootPaths { get; set; } = new();
        public List<string> LockedProcessHints { get; set; } = new();
        public List<HandoverPurgedItemSnapshot> PurgedItemsSnapshot { get; set; } = new();
        public int PendingRebootCount => PendingRebootPaths.Count;
        public bool HasPendingReboot => PendingRebootPaths.Count > 0;
        public string FormattedReclaimedSize => DetectedUserProfile.FormatBytes(ReclaimedBytes);
    }
}
