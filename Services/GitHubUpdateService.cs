using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WinPurifyPro.Services
{
    public class UpdateCheckResult
    {
        public bool IsUpdateAvailable { get; set; }
        public string CurrentVersion { get; set; } = "";
        public string LatestVersion { get; set; } = "";
        public string ReleaseTitle { get; set; } = "";
        public string ReleaseNotes { get; set; } = "";
        public string ReleaseUrl { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public long AssetSizeBytes { get; set; }
        public string ErrorMessage { get; set; } = "";
        public bool Success => string.IsNullOrEmpty(ErrorMessage);
    }

    /// <summary>
    /// GitHub Releases 기반 실시간 자동 업데이트 및 버전 감지 서비스
    /// Repository: https://github.com/ahbiyout-all/WinPurify-Pro
    /// </summary>
    public sealed class GitHubUpdateService
    {
        private static readonly Lazy<GitHubUpdateService> _instance = new(() => new GitHubUpdateService());
        public static GitHubUpdateService Instance => _instance.Value;

        public const string GitHubOwner = "ahbiyout-all";
        public const string GitHubRepo = "WinPurify-Pro";
        public const string GitHubReleasesApiUrl = "https://api.github.com/repos/ahbiyout-all/WinPurify-Pro/releases/latest";
        public const string GitHubRepoUrl = "https://github.com/ahbiyout-all/WinPurify-Pro";

        private readonly HttpClient _httpClient;

        private GitHubUpdateService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            // GitHub REST API requires a User-Agent header
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WinPurifyPro-AutoUpdater/4.40.0 (ahbiyout-all)");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
        }

        /// <summary>
        /// GitHub Releases API를 조회하여 최신 배포 버전 확인
        /// </summary>
        public async Task<UpdateCheckResult> CheckForUpdatesAsync()
        {
            string currentVer = AppEnvironment.AppVersion;
            var result = new UpdateCheckResult
            {
                CurrentVersion = currentVer
            };

            try
            {
                using var response = await _httpClient.GetAsync(GitHubReleasesApiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    result.ErrorMessage = $"GitHub API 응답 오류 (HTTP {(int)response.StatusCode}: {response.ReasonPhrase})";
                    return result;
                }

                string json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                string rawTag = root.TryGetProperty("tag_name", out var tagProp) ? tagProp.GetString() ?? "" : "";
                string cleanTag = rawTag.TrimStart('v', 'V').Trim();
                result.LatestVersion = cleanTag;
                result.ReleaseTitle = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : $"WinPurify Pro v{cleanTag}";
                result.ReleaseNotes = root.TryGetProperty("body", out var bodyProp) ? bodyProp.GetString() ?? "" : "";
                result.ReleaseUrl = root.TryGetProperty("html_url", out var urlProp) ? urlProp.GetString() ?? $"{GitHubRepoUrl}/releases" : $"{GitHubRepoUrl}/releases";

                // Locate installer asset in release (.exe or Setup)
                if (root.TryGetProperty("assets", out var assetsProp) && assetsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var asset in assetsProp.EnumerateArray())
                    {
                        string assetName = asset.TryGetProperty("name", out var aName) ? aName.GetString() ?? "" : "";
                        if (assetName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                            assetName.IndexOf("Setup", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            assetName.IndexOf("WinPurifyPro", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            result.DownloadUrl = asset.TryGetProperty("browser_download_url", out var dlProp) ? dlProp.GetString() ?? "" : "";
                            result.AssetSizeBytes = asset.TryGetProperty("size", out var sProp) ? sProp.GetInt64() : 0;
                            break;
                        }
                    }
                }

                // If no specific asset found, fallback to generic release download URL
                if (string.IsNullOrEmpty(result.DownloadUrl))
                {
                    result.DownloadUrl = $"{GitHubRepoUrl}/releases/download/v{cleanTag}/WinPurifyPro-Setup.exe";
                }

                result.IsUpdateAvailable = IsNewerVersion(cleanTag, currentVer);
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"업데이트 확인 중 네트워크 오류: {ex.Message}";
                LoggerService.Instance.LogException("GitHubUpdateCheck", ex);
            }

            return result;
        }

        /// <summary>
        /// 최신 인스톨러를 다운로드하고 자동 실행
        /// </summary>
        public async Task<bool> DownloadAndInstallUpdateAsync(string downloadUrl, string latestVersion, IProgress<int>? progress = null)
        {
            if (string.IsNullOrEmpty(downloadUrl)) return false;

            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "WinPurifyPro_Updates");
                Directory.CreateDirectory(tempDir);
                string installerPath = Path.Combine(tempDir, $"WinPurifyPro-v{latestVersion}-Setup.exe");

                using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    long totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    using var contentStream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = new FileStream(installerPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                    byte[] buffer = new byte[16384];
                    long totalRead = 0L;
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalRead += bytesRead;

                        if (totalBytes > 0 && progress != null)
                        {
                            int percent = (int)((totalRead * 100) / totalBytes);
                            progress.Report(percent);
                        }
                    }
                }

                progress?.Report(100);

                if (File.Exists(installerPath))
                {
                    LoggerService.Instance.LogMain($"[GitHub Update] 새 버전 v{latestVersion} 다운로드 완료: {installerPath}");

                    // Launch setup wizard with restart options
                    var psi = new ProcessStartInfo
                    {
                        FileName = installerPath,
                        Arguments = "/CLOSEAPPLICATIONS /RESTARTAPPLICATIONS",
                        UseShellExecute = true
                    };
                    Process.Start(psi);

                    // Exit current running app so files can be replaced cleanly
                    System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        System.Windows.Application.Current.Shutdown(0);
                    });

                    return true;
                }
            }
            catch (Exception ex)
            {
                LoggerService.Instance.LogException("GitHubUpdateDownload", ex);
            }

            return false;
        }

        /// <summary>
        /// SemVer 버전 비교 (latest > current)
        /// </summary>
        public static bool IsNewerVersion(string latest, string current)
        {
            try
            {
                if (Version.TryParse(latest, out var vLatest) && Version.TryParse(current, out var vCurrent))
                {
                    return vLatest > vCurrent;
                }

                // Fallback numeric segment comparison
                var partsLatest = latest.Split('.').Select(p => int.TryParse(p, out var n) ? n : 0).ToArray();
                var partsCurrent = current.Split('.').Select(p => int.TryParse(p, out var n) ? n : 0).ToArray();

                int len = Math.Max(partsLatest.Length, partsCurrent.Length);
                for (int i = 0; i < len; i++)
                {
                    int l = i < partsLatest.Length ? partsLatest[i] : 0;
                    int c = i < partsCurrent.Length ? partsCurrent[i] : 0;
                    if (l > c) return true;
                    if (l < c) return false;
                }
            }
            catch { }

            return false;
        }
    }
}
