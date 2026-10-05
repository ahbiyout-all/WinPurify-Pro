using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// 메인 앱과 커맨더 관제 콘솔의 모든 작업 내역을 실시간 파일로 기록하고 디스크에 자동 저장하는 스레드 안전 고성능 로거 서비스
    /// </summary>
    public class LoggerService
    {
        private static readonly Lazy<LoggerService> _instance = new Lazy<LoggerService>(() => new LoggerService());
        public static LoggerService Instance => _instance.Value;

        private readonly ConcurrentQueue<LogEntry> _logQueue = new ConcurrentQueue<LogEntry>();
        private readonly AutoResetEvent _hasNewLogs = new AutoResetEvent(false);
        private readonly Thread _workerThread;
        private volatile bool _isRunning = true;
        private readonly object _configLock = new object();

        private bool _autoSaveEnabled = true;
        private string _customLogDirectory = string.Empty;

        public bool AutoSaveEnabled
        {
            get { lock (_configLock) return _autoSaveEnabled; }
            set { lock (_configLock) _autoSaveEnabled = value; }
        }

        public class LogEntry
        {
            public DateTime Timestamp { get; set; } = DateTime.Now;
            public string Source { get; set; } = "Main"; // "Main" or "Commander" or "Audit"
            public string Level { get; set; } = "INFO";
            public string Message { get; set; } = string.Empty;

            public override string ToString()
            {
                return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Source.ToUpperInvariant()}] [{Level}] {Message}";
            }
        }

        private LoggerService()
        {
            _workerThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "WinPurify_FileLoggerWorker",
                Priority = ThreadPriority.BelowNormal
            };
            _workerThread.Start();

            // 앱 종료 시 플러시 보장
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Shutdown();
        }

        /// <summary>
        /// 로그 파일이 저장될 컴퓨터 디렉토리 경로 결정
        /// - 포터블 환경: 실행파일 디렉터리\Logs\
        /// - 정식 설치 환경: %AppData%\WinPurifyPro\Logs\
        /// </summary>
        public string GetLogDirectory()
        {
            lock (_configLock)
            {
                if (!string.IsNullOrWhiteSpace(_customLogDirectory) && Directory.Exists(_customLogDirectory))
                {
                    return _customLogDirectory;
                }
            }

            try
            {
                string baseDir;
                if (AppEnvironment.IsPortable)
                {
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;
                    baseDir = Path.Combine(appDir, "Logs");
                }
                else
                {
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    baseDir = Path.Combine(appData, "WinPurifyPro", "Logs");
                }

                if (!Directory.Exists(baseDir))
                {
                    Directory.CreateDirectory(baseDir);
                }
                return baseDir;
            }
            catch
            {
                // 실패 시 임시 폴더 대체
                string tempDir = Path.Combine(Path.GetTempPath(), "WinPurifyPro", "Logs");
                if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
                return tempDir;
            }
        }

        /// <summary>
        /// 오늘 날짜 기준 메인 앱 로그 파일 전체 경로
        /// </summary>
        public string GetMainLogFilePath()
        {
            string dir = GetLogDirectory();
            return Path.Combine(dir, $"WinPurifyPro_{DateTime.Now:yyyyMMdd}.log");
        }

        /// <summary>
        /// 오늘 날짜 기준 커맨더 콘솔 로그 파일 전체 경로
        /// </summary>
        public string GetCommanderLogFilePath()
        {
            string dir = GetLogDirectory();
            return Path.Combine(dir, $"WinPurifyCommander_{DateTime.Now:yyyyMMdd}.log");
        }

        /// <summary>
        /// 메인 앱 작업 로그 기록 및 자동 저장
        /// </summary>
        public void LogMain(string message, string level = "INFO")
        {
            EnqueueLog("Main", message, level);
        }

        /// <summary>
        /// 커맨더 관제 콘솔 작업 로그 기록 및 자동 저장
        /// </summary>
        public void LogCommander(string message, string level = "INFO")
        {
            EnqueueLog("Commander", message, level);
        }

        /// <summary>
        /// 예외 오류 로깅 헬퍼
        /// </summary>
        public void LogException(string context, Exception ex)
        {
            LogMain($"[EXCEPTION] [{context}] {ex.Message}\n{ex.StackTrace}", "ERROR");
        }

        /// <summary>
        /// 공통 엔트리 큐 등록
        /// </summary>
        public void EnqueueLog(string source, string message, string level = "INFO")
        {
            if (!AutoSaveEnabled && !message.StartsWith("[FATAL]", StringComparison.OrdinalIgnoreCase))
                return;

            _logQueue.Enqueue(new LogEntry
            {
                Timestamp = DateTime.Now,
                Source = source,
                Level = level,
                Message = message
            });

            _hasNewLogs.Set();
        }

        /// <summary>
        /// 백그라운드 스레드에서 파일 쓰기 일괄 처리 (I/O 병목 방지)
        /// </summary>
        private void ProcessLogQueue()
        {
            while (_isRunning)
            {
                _hasNewLogs.WaitOne(1000); // 1초 대기 또는 새 로그 발생 즉시 신호 수신
                FlushQueue();
            }
            // 최종 종료 전 잔여 큐 플러시
            FlushQueue();
        }

        private void FlushQueue()
        {
            if (_logQueue.IsEmpty) return;

            var mainBuffer = new StringBuilder();
            var commanderBuffer = new StringBuilder();

            while (_logQueue.TryDequeue(out var entry))
            {
                string line = entry.ToString();
                if (entry.Source.Equals("Commander", StringComparison.OrdinalIgnoreCase))
                {
                    commanderBuffer.AppendLine(line);
                }
                else
                {
                    mainBuffer.AppendLine(line);
                }
            }

            try
            {
                string dir = GetLogDirectory();

                if (mainBuffer.Length > 0)
                {
                    string mainPath = Path.Combine(dir, $"WinPurifyPro_{DateTime.Now:yyyyMMdd}.log");
                    File.AppendAllText(mainPath, mainBuffer.ToString(), Encoding.UTF8);
                }

                if (commanderBuffer.Length > 0)
                {
                    string cmdPath = Path.Combine(dir, $"WinPurifyCommander_{DateTime.Now:yyyyMMdd}.log");
                    File.AppendAllText(cmdPath, commanderBuffer.ToString(), Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoggerService Error] Failed to write log to disk: {ex.Message}");
            }
        }

        /// <summary>
        /// 로그 폴더를 Windows 파일 탐색기로 열기
        /// </summary>
        public bool OpenLogFolder()
        {
            try
            {
                string dir = GetLogDirectory();
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{dir}\"",
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoggerService Error] Failed to open log folder: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 현재 메모리의 로그 목록을 사용자가 지정한 파일로 즉시 저장/내보내기
        /// </summary>
        public bool ExportLogs(string destinationFilePath, IEnumerable<string> lines, string headerTitle = "WinPurify Pro Log Export")
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("================================================================================");
                sb.AppendLine($" {headerTitle} - Generated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($" Version: v{AppEnvironment.AppVersion} | System: {Environment.OSVersion}");
                sb.AppendLine("================================================================================");
                sb.AppendLine();

                foreach (var line in lines)
                {
                    sb.AppendLine(line);
                }

                string? dir = Path.GetDirectoryName(destinationFilePath);
                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(destinationFilePath, sb.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LoggerService Error] Failed to export logs: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 애플리케이션 종료 처리
        /// </summary>
        public void Shutdown()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _hasNewLogs.Set();
            try
            {
                _workerThread.Join(1500);
            }
            catch { }
        }
    }
}
