using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace WinPurifyPro.Services
{
    public class RemoteExecutionRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string FormattedTime => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        public string CommanderIp { get; set; } = "127.0.0.1";
        public string CommandType { get; set; } = "";
        public string CommandTitle { get; set; } = "";
        public string Details { get; set; } = "";
        public long ExecutionTimeMs { get; set; }
        public bool IsSuccess { get; set; } = true;
    }

    /// <summary>
    /// 원격 관제 콘솔(Central Commander)로부터 수신된 명령의 실행 이력을 로컬에 영구 보존하고 감사(Audit) 로그를 관리하는 서비스
    /// </summary>
    public class RemoteExecutionAuditService
    {
        public static RemoteExecutionAuditService Instance { get; } = new RemoteExecutionAuditService();

        private readonly object _lock = new();
        private readonly string _logFilePath;
        private readonly List<RemoteExecutionRecord> _records = new();

        public event Action<RemoteExecutionRecord>? RecordAdded;

        private RemoteExecutionAuditService()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                _logFilePath = Path.Combine(baseDir, "remote_execution_history.json");
                LoadRecords();
            }
            catch
            {
                _logFilePath = "remote_execution_history.json";
            }
        }

        private void LoadRecords()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_logFilePath))
                    {
                        string json = File.ReadAllText(_logFilePath);
                        var list = JsonSerializer.Deserialize<List<RemoteExecutionRecord>>(json);
                        if (list != null)
                        {
                            _records.Clear();
                            _records.AddRange(list.OrderByDescending(r => r.Timestamp).Take(200));
                        }
                    }
                }
                catch { }
            }
        }

        public void AddRecord(RemoteExecutionRecord record)
        {
            lock (_lock)
            {
                try
                {
                    _records.Insert(0, record);
                    if (_records.Count > 200)
                    {
                        _records.RemoveRange(200, _records.Count - 200);
                    }

                    string json = JsonSerializer.Serialize(_records, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_logFilePath, json);
                }
                catch { }
            }

            try
            {
                RecordAdded?.Invoke(record);
            }
            catch { }
        }

        public IReadOnlyList<RemoteExecutionRecord> GetRecentRecords(int count = 50)
        {
            lock (_lock)
            {
                return _records.Take(count).ToList();
            }
        }

        public void ClearHistory()
        {
            lock (_lock)
            {
                _records.Clear();
                try
                {
                    if (File.Exists(_logFilePath))
                    {
                        File.Delete(_logFilePath);
                    }
                }
                catch { }
            }
        }
    }
}
