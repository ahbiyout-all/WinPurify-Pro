using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WinPurifyPro.Services
{
    public static class RobocopyScannerService
    {
        public static string ResolveEnvironmentPath(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath)) return string.Empty;

            string path = rawPath.Trim();
            path = path.Replace("$env:LOCALAPPDATA", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StringComparison.OrdinalIgnoreCase);
            path = path.Replace("$env:APPDATA", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), StringComparison.OrdinalIgnoreCase);
            path = path.Replace("$env:TEMP", Path.GetTempPath().TrimEnd('\\'), StringComparison.OrdinalIgnoreCase);
            path = path.Replace("$env:ProgramData", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), StringComparison.OrdinalIgnoreCase);
            path = path.Replace("%LocalAppData%", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StringComparison.OrdinalIgnoreCase);
            path = path.Replace("%AppData%", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), StringComparison.OrdinalIgnoreCase);
            path = path.Replace("%TEMP%", Path.GetTempPath().TrimEnd('\\'), StringComparison.OrdinalIgnoreCase);
            path = path.Replace("%ProgramData%", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), StringComparison.OrdinalIgnoreCase);
            path = Environment.ExpandEnvironmentVariables(path);

            return path.TrimEnd('\\', '*', ' ');
        }

        public static async Task<long> GetFolderSizeFastAsync(string rawPath)
        {
            return await Task.Run(() =>
            {
                string targetDir = ResolveEnvironmentPath(rawPath);
                if (string.IsNullOrWhiteSpace(targetDir) || !Directory.Exists(targetDir))
                {
                    return 0L;
                }

                // 1. Tier 1: PurifyEngineCore.dll Native C++ FindFirstFileExW Fast Scanner (0ms process spawn overhead)
                if (NativeEngineService.TryFastScanDirectory(targetDir, "*.*", out long nativeBytes, out int fileCount))
                {
                    return nativeBytes;
                }

                // 2. Tier 2: Robocopy Non-Copy Virtual Pipeline
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "robocopy.exe",
                        Arguments = $"\"{targetDir}\" NULL_VIRTUAL /L /S /NJH /NJS /BYTES /XJD /R:0 /W:0 /NDL /NFL",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(psi);
                    if (process == null) return GetFallbackDirectorySize(targetDir);

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(3000);

                    // 1. Regex match for English Bytes summary (Bytes : <digits>)
                    var match = Regex.Match(output, @"Bytes\s*:\s*(\d+)", RegexOptions.IgnoreCase);
                    if (match.Success && long.TryParse(match.Groups[1].Value, out long bytes))
                    {
                        return bytes;
                    }

                    // 2. Regex match for Localized (Korean: 바이트 : <digits>, Japanese: バイト, etc.)
                    var localizedMatch = Regex.Match(output, @"(?:바이트|バイト|Bytes|Octets|Bytes)\s*:\s*(\d+)", RegexOptions.IgnoreCase);
                    if (localizedMatch.Success && long.TryParse(localizedMatch.Groups[1].Value, out long locBytes))
                    {
                        return locBytes;
                    }

                    // 3. Language-Agnostic Column Token Fallback
                    // Robocopy summary line format: [Label] : [Dirs] [Files] [Bytes] [Other]
                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (line.Contains(":") && (line.Contains("Total") || line.Contains("합계") || line.Contains("合計")))
                        {
                            var tokens = line.Split(new[] { ' ', '\t', ':' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var token in tokens)
                            {
                                if (long.TryParse(token, out long parsedVal) && parsedVal > 1000)
                                {
                                    return parsedVal;
                                }
                            }
                        }
                    }

                    return GetFallbackDirectorySize(targetDir);
                }
                catch
                {
                    return GetFallbackDirectorySize(targetDir);
                }
            });
        }

        private static long GetFallbackDirectorySize(string targetDir)
        {
            try
            {
                long total = 0;
                var di = new DirectoryInfo(targetDir);
                foreach (var fi in di.EnumerateFiles("*", new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true }))
                {
                    total += fi.Length;
                }
                return total;
            }
            catch
            {
                return 0L;
            }
        }
    }
}
