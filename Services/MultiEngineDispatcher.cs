using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinPurifyPro.Models;

namespace WinPurifyPro.Services
{
    public class ExecutionResult
    {
        public bool Success { get; set; }
        public ExecutionMode UsedMode { get; set; }
        public string OutputMessage { get; set; } = string.Empty;
        public string FailureDiagnosis { get; set; } = string.Empty;
        public long ExecutionTimeMs { get; set; }
        public List<EngineAttemptTrace> Traces { get; set; } = new();
    }

    public static class MultiEngineDispatcher
    {
        public static bool IsProcessElevated()
        {
            try
            {
                if (!OperatingSystem.IsWindows()) return false;
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        public static async Task<ExecutionResult> ExecuteTaskAsync(OptimizationTask task, CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();
            var traces = new List<EngineAttemptTrace>();
            int timeoutMs = Math.Max(10, task.TimeoutSeconds > 0 ? task.TimeoutSeconds : 15) * 1000;
            bool isElevated = IsProcessElevated();

            // 1. First-Class Pass: Native C++ Core
            if (task.NativeActionType > 0)
            {
                var nativeSw = Stopwatch.StartNew();
                if (NativeEngineService.IsNativeCoreAvailable())
                {
                    try
                    {
                        int res = NativeEngineService.ExecuteQuickMaintenance(task.NativeActionType);
                        nativeSw.Stop();
                        if (res == 1)
                        {
                            sw.Stop();
                            traces.Add(new EngineAttemptTrace
                            {
                                EngineName = "NativeDll",
                                Succeeded = true,
                                ExitCode = res,
                                DurationMs = nativeSw.ElapsedMilliseconds,
                                OutputSummary = $"NativeActionType={task.NativeActionType} succeeded"
                            });
                            return new ExecutionResult
                            {
                                Success = true,
                                UsedMode = ExecutionMode.NativeDll,
                                OutputMessage = $"Native C++ Kernel API Success ({nativeSw.ElapsedMilliseconds}ms)",
                                ExecutionTimeMs = sw.ElapsedMilliseconds,
                                Traces = traces
                            };
                        }

                        traces.Add(new EngineAttemptTrace
                        {
                            EngineName = "NativeDll",
                            Succeeded = false,
                            ExitCode = res,
                            DurationMs = nativeSw.ElapsedMilliseconds,
                            ErrorDetail = $"Native C++ returned non-success code ({res}); failing over to FastCli."
                        });
                    }
                    catch (Exception ex)
                    {
                        nativeSw.Stop();
                        traces.Add(new EngineAttemptTrace
                        {
                            EngineName = "NativeDll",
                            Succeeded = false,
                            ExitCode = -1,
                            DurationMs = nativeSw.ElapsedMilliseconds,
                            ErrorDetail = $"Native P/Invoke Exception: {ex.Message}"
                        });
                    }
                }
                else
                {
                    nativeSw.Stop();
                    traces.Add(new EngineAttemptTrace
                    {
                        EngineName = "NativeDll",
                        Succeeded = false,
                        ExitCode = -2,
                        DurationMs = nativeSw.ElapsedMilliseconds,
                        ErrorDetail = "PurifyEngineCore.dll unavailable; failing over to FastCli."
                    });
                }
            }

            // 2. Second-Class Pass: Fast Windows CLI (sc / reg / del / DISM) with Dynamic Timeout & Full Output Capture
            if (!string.IsNullOrWhiteSpace(task.FastCliCommand))
            {
                var cliResult = await RunCliCommandAsync(task.FastCliCommand, timeoutMs, cancellationToken);
                traces.AddRange(cliResult.Traces);

                if (cliResult.Success)
                {
                    sw.Stop();
                    cliResult.ExecutionTimeMs = sw.ElapsedMilliseconds;
                    cliResult.Traces = traces;
                    return cliResult;
                }
            }

            // 3. Third-Class Pass: PowerShell Graceful Sandbox with Dynamic Timeout & Stderr Telemetry
            if (!string.IsNullOrWhiteSpace(task.PowerShellCommand))
            {
                var psTimeoutMs = Math.Max(timeoutMs, 20000);
                var psResult = await RunPowerShellCommandAsync(task.PowerShellCommand, psTimeoutMs, cancellationToken);
                traces.AddRange(psResult.Traces);
                sw.Stop();
                psResult.ExecutionTimeMs = sw.ElapsedMilliseconds;
                psResult.Traces = traces;

                if (!psResult.Success)
                {
                    psResult.FailureDiagnosis = BuildFailureDiagnosis(task, traces, isElevated);
                    psResult.OutputMessage = $"{psResult.OutputMessage} | 진단: {psResult.FailureDiagnosis}";
                }
                return psResult;
            }

            // 4. Both FastCli and PowerShell absent or FastCli failed with no PowerShell fallback
            sw.Stop();
            if (traces.Count > 0 && traces.All(t => !t.Succeeded))
            {
                string diag = BuildFailureDiagnosis(task, traces, isElevated);
                var lastTrace = traces.Last();
                return new ExecutionResult
                {
                    Success = false,
                    UsedMode = ExecutionMode.FastCli,
                    OutputMessage = $"{lastTrace.ErrorDetail} | 진단: {diag}",
                    FailureDiagnosis = diag,
                    ExecutionTimeMs = sw.ElapsedMilliseconds,
                    Traces = traces
                };
            }

            return new ExecutionResult
            {
                Success = true,
                UsedMode = ExecutionMode.FastCli,
                OutputMessage = "Task bypassed (no executable action required)",
                ExecutionTimeMs = sw.ElapsedMilliseconds,
                Traces = traces
            };
        }

        private static string BuildFailureDiagnosis(OptimizationTask task, List<EngineAttemptTrace> traces, bool isElevated)
        {
            string combinedErr = string.Join(" | ", traces.Where(t => !string.IsNullOrWhiteSpace(t.ErrorDetail)).Select(t => t.ErrorDetail));
            string lower = combinedErr.ToLowerInvariant();

            if ((task.RequiresAdmin && !isElevated) || lower.Contains("access is denied") || lower.Contains("액세스가 거부") || lower.Contains("exitcode=5"))
            {
                return "관리자 권한(UAC) 부족 - 관리자 권한으로 앱을 재실행하십시오.";
            }

            if (lower.Contains("used by another process") || lower.Contains("다른 프로세스") || lower.Contains("sharing violation") || lower.Contains("exitcode=32"))
            {
                return "파일 잠금 충돌(Sharing Violation) - 관련 앱 종료 또는 Native Lock Hunter 실행 필요.";
            }

            if (lower.Contains("timeout"))
            {
                return $"지정된 타임아웃({task.TimeoutSeconds}초) 초과 - 대용량 캐시 또는 I/O 지연 감지.";
            }

            if (lower.Contains("cannot find") || lower.Contains("지정된 경로") || lower.Contains("찾을 수 없") || lower.Contains("not found"))
            {
                return "대상 경로나 레지스트리 키가 이미 정리되었거나 시스템에 존재하지 않습니다.";
            }

            return string.IsNullOrWhiteSpace(combinedErr)
                ? "다중 엔진(C++/CLI/PowerShell) 모두 0이 아닌 종료 코드를 반환했습니다."
                : TruncateText(combinedErr, 180);
        }

        private static async Task<ExecutionResult> RunCliCommandAsync(string command, int timeoutMs, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            string resolved = RobocopyScannerService.ResolveEnvironmentPath(command);

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{resolved}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = new Process { StartInfo = psi };
            try
            {
                if (!process.Start())
                {
                    sw.Stop();
                    var startErr = "cmd.exe 프로세스 시작 실패";
                    return new ExecutionResult
                    {
                        Success = false,
                        UsedMode = ExecutionMode.FastCli,
                        OutputMessage = startErr,
                        Traces = { new EngineAttemptTrace { EngineName = "FastCli", Succeeded = false, ExitCode = -1, DurationMs = sw.ElapsedMilliseconds, ErrorDetail = startErr } }
                    };
                }

                // Read streams concurrently BEFORE WaitForExitAsync to prevent OS pipe buffer deadlocks
                var stdoutTask = process.StandardOutput.ReadToEndAsync();
                var stderrTask = process.StandardError.ReadToEndAsync();

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeoutMs);

                await process.WaitForExitAsync(cts.Token);
                string stdout = TruncateText((await stdoutTask).Trim(), 200);
                string stderr = TruncateText((await stderrTask).Trim(), 200);
                sw.Stop();

                bool ok = (process.ExitCode == 0);
                var trace = new EngineAttemptTrace
                {
                    EngineName = "FastCli",
                    Succeeded = ok,
                    ExitCode = process.ExitCode,
                    DurationMs = sw.ElapsedMilliseconds,
                    OutputSummary = stdout,
                    ErrorDetail = ok ? string.Empty : $"ExitCode={process.ExitCode}{(string.IsNullOrEmpty(stderr) ? "" : $", Stderr={stderr}")}"
                };

                return new ExecutionResult
                {
                    Success = ok,
                    UsedMode = ExecutionMode.FastCli,
                    OutputMessage = ok
                        ? $"FastCli Success ({sw.ElapsedMilliseconds}ms)"
                        : $"CLI ExitCode: {process.ExitCode}{(string.IsNullOrEmpty(stderr) ? "" : $" ({stderr})")}",
                    Traces = { trace }
                };
            }
            catch (OperationCanceledException)
            {
                sw.Stop();
                try { process.Kill(true); } catch { }
                string msg = $"CLI Execution Timeout ({timeoutMs / 1000}s)";
                return new ExecutionResult
                {
                    Success = false,
                    UsedMode = ExecutionMode.FastCli,
                    OutputMessage = msg,
                    Traces = { new EngineAttemptTrace { EngineName = "FastCli", Succeeded = false, ExitCode = -408, DurationMs = sw.ElapsedMilliseconds, ErrorDetail = msg } }
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                string msg = $"CLI Exception: {ex.Message}";
                return new ExecutionResult
                {
                    Success = false,
                    UsedMode = ExecutionMode.FastCli,
                    OutputMessage = msg,
                    Traces = { new EngineAttemptTrace { EngineName = "FastCli", Succeeded = false, ExitCode = -1, DurationMs = sw.ElapsedMilliseconds, ErrorDetail = msg } }
                };
            }
        }

        private static async Task<ExecutionResult> RunPowerShellCommandAsync(string script, int timeoutMs, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            byte[] bytes = Encoding.Unicode.GetBytes(script);
            string encoded = Convert.ToBase64String(bytes);

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encoded}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = new Process { StartInfo = psi };
            try
            {
                if (!process.Start())
                {
                    sw.Stop();
                    string startErr = "powershell.exe 프로세스 시작 실패";
                    return new ExecutionResult
                    {
                        Success = false,
                        UsedMode = ExecutionMode.PowerShellSandbox,
                        OutputMessage = startErr,
                        Traces = { new EngineAttemptTrace { EngineName = "PowerShellSandbox", Succeeded = false, ExitCode = -1, DurationMs = sw.ElapsedMilliseconds, ErrorDetail = startErr } }
                    };
                }

                var stdoutTask = process.StandardOutput.ReadToEndAsync();
                var stderrTask = process.StandardError.ReadToEndAsync();

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeoutMs);

                await process.WaitForExitAsync(cts.Token);
                string stdout = TruncateText((await stdoutTask).Trim(), 200);
                string stderr = TruncateText((await stderrTask).Trim(), 200);
                sw.Stop();

                bool ok = (process.ExitCode == 0);
                var trace = new EngineAttemptTrace
                {
                    EngineName = "PowerShellSandbox",
                    Succeeded = ok,
                    ExitCode = process.ExitCode,
                    DurationMs = sw.ElapsedMilliseconds,
                    OutputSummary = stdout,
                    ErrorDetail = ok ? string.Empty : $"ExitCode={process.ExitCode}{(string.IsNullOrEmpty(stderr) ? "" : $", Stderr={stderr}")}"
                };

                return new ExecutionResult
                {
                    Success = ok,
                    UsedMode = ExecutionMode.PowerShellSandbox,
                    OutputMessage = ok
                        ? $"PowerShell Success ({sw.ElapsedMilliseconds}ms)"
                        : $"PowerShell ExitCode: {process.ExitCode}{(string.IsNullOrEmpty(stderr) ? "" : $" ({stderr})")}",
                    Traces = { trace }
                };
            }
            catch (OperationCanceledException)
            {
                sw.Stop();
                try { process.Kill(true); } catch { }
                string msg = $"PowerShell Execution Timeout ({timeoutMs / 1000}s)";
                return new ExecutionResult
                {
                    Success = false,
                    UsedMode = ExecutionMode.PowerShellSandbox,
                    OutputMessage = msg,
                    Traces = { new EngineAttemptTrace { EngineName = "PowerShellSandbox", Succeeded = false, ExitCode = -408, DurationMs = sw.ElapsedMilliseconds, ErrorDetail = msg } }
                };
            }
            catch (Exception ex)
            {
                sw.Stop();
                string msg = $"PowerShell Exception: {ex.Message}";
                return new ExecutionResult
                {
                    Success = false,
                    UsedMode = ExecutionMode.PowerShellSandbox,
                    OutputMessage = msg,
                    Traces = { new EngineAttemptTrace { EngineName = "PowerShellSandbox", Succeeded = false, ExitCode = -1, DurationMs = sw.ElapsedMilliseconds, ErrorDetail = msg } }
                };
            }
        }

        private static string TruncateText(string input, int maxLen)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            string singleLine = input.Replace("\r", " ").Replace("\n", " ").Trim();
            return singleLine.Length <= maxLen ? singleLine : singleLine.Substring(0, maxLen) + "...";
        }
    }
}
