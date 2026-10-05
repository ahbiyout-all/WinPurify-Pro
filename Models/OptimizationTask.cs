using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WinPurifyPro.Models
{
    public enum TaskCategory
    {
        Privacy,
        Browser,
        Security,
        Registry,
        Update,
        System,
        Storage,
        Special
    }

    public enum RiskLevel
    {
        Safe,
        Deep,
        Risky
    }

    public enum ExecutionMode
    {
        NativeDll,
        FastCli,
        PowerShellSandbox
    }

    public enum TaskStatus
    {
        Pending,
        Scanning,
        Scanned,
        Optimizing,
        Completed,
        Failed,
        Skipped
    }

    public class OptimizationTask : INotifyPropertyChanged
    {
        public int SequenceNumber { get; set; } = 0;
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskCategory Category { get; set; }
        public string SubCategory { get; set; } = string.Empty;
        public RiskLevel Risk { get; set; } = RiskLevel.Safe;
        public string ScanTargetFolder { get; set; } = string.Empty;
        public string PowerShellCommand { get; set; } = string.Empty;
        public string FastCliCommand { get; set; } = string.Empty;
        public int NativeActionType { get; set; } = 0; // 0: None, 1: RecycleBin, 2: DnsFlush, 3: TempPurge, 4: RamTrim

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(); } }
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

        public string FormattedSize
        {
            get
            {
                if (EstimatedBytes <= 0) return "0 KB";
                if (EstimatedBytes < 1024 * 1024) return $"{(EstimatedBytes / 1024.0):F1} KB";
                if (EstimatedBytes < 1024 * 1024 * 1024) return $"{(EstimatedBytes / (1024.0 * 1024.0)):F1} MB";
                return $"{(EstimatedBytes / (1024.0 * 1024.0 * 1024.0)):F2} GB";
            }
        }

        private TaskStatus _status = TaskStatus.Pending;
        public TaskStatus Status
        {
            get => _status;
            set { if (_status != value) { _status = value; OnPropertyChanged(); } }
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { if (_statusMessage != value) { _statusMessage = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
