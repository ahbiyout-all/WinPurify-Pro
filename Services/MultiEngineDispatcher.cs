using System;
using System.Diagnostics;
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
        public long ExecutionTimeMs { get; set; }
    }

    public static class MultiEngineDispatcher
    {
        public static async Task<ExecutionResult> ExecuteTaskAsync(OptimizationTask task, CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();

            // 1. First-Class Pass: Native C++ Core
            if (task.NativeActionType > 0 && NativeEngineService.IsNativeCoreAvailable())
            {
                try
                {
                    int res = NativeEngineService.ExecuteQuickMaintenance(task.NativeActionType);
                    if (res == 1)
                    {
                        sw.Stop();
                        return new ExecutionResult
                        {
                            Success = true,
                            UsedMode = ExecutionMode.NativeDll,
                            OutputMessage = "Native C++ Kernel API Execution Success",
                            ExecutionTimeMs = sw.ElapsedMilliseconds
                        };
                    }
                }
                catch { }
            }

            // 2. Second-Class Pass: Fast Windows CLI (sc / reg / del / rmdir)
            if (!string.IsNullOrWhiteSpace(task.FastCliCommand))
            {
                try
                {
                    var cliResult = await RunCliCommandAsync(task.FastCliCommand, cancellationToken);
                    if (cliResult.Success)
                    {
                        sw.Stop();
                        cliResult.ExecutionTimeMs = sw.ElapsedMilliseconds;
                        return cliResult;
                    }
                }
                catch { }
            }

            // 3. Third-Class Pass: PowerShell Graceful Sandbox
            if (!string.IsNullOrWhiteSpace(task.PowerShellCommand))
            {
                try
                {
                    var psResult = await RunPowerShellCommandAsync(task.PowerShellCommand, cancellationToken);
                    sw.Stop();
                    psResult.ExecutionTimeMs = sw.ElapsedMilliseconds;
                    return psResult;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    return new ExecutionResult
                    {
                        Success = false,
                        UsedMode = ExecutionMode.PowerShellSandbox,
                        OutputMessage = $"PowerShell execution failed: {ex.Message}",
                        ExecutionTimeMs = sw.ElapsedMilliseconds
                    };
                }
            }

            sw.Stop();
            return new ExecutionResult
            {
                Success = true,
                UsedMode = ExecutionMode.FastCli,
                OutputMessage = "Task bypassed (no executable action required)",
                ExecutionTimeMs = sw.ElapsedMilliseconds
            };
        }

        private static async Task<ExecutionResult> RunCliCommandAsync(string command, CancellationToken cancellationToken)
        {
            string resolved = RobocopyScannerService.ResolveEnvironmentPath(command);
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{resolved}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            if (!process.Start())
            {
                return new ExecutionResult { Success = false, UsedMode = ExecutionMode.FastCli, OutputMessage = "Process failed to start" };
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(5000);

            try
            {
                await process.WaitForExitAsync(cts.Token);
                return new ExecutionResult
                {
                    Success = (process.ExitCode == 0),
                    UsedMode = ExecutionMode.FastCli,
                    OutputMessage = $"CLI ExitCode: {process.ExitCode}"
                };
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(true); } catch { }
                return new ExecutionResult { Success = false, UsedMode = ExecutionMode.FastCli, OutputMessage = "CLI Execution Timeout (5s)" };
            }
        }

        private static async Task<ExecutionResult> RunPowerShellCommandAsync(string script, CancellationToken cancellationToken)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(script);
            string encoded = Convert.ToBase64String(bytes);

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encoded}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            if (!process.Start())
            {
                return new ExecutionResult { Success = false, UsedMode = ExecutionMode.PowerShellSandbox, OutputMessage = "PowerShell failed to start" };
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(10000);

            try
            {
                await process.WaitForExitAsync(cts.Token);
                return new ExecutionResult
                {
                    Success = (process.ExitCode == 0),
                    UsedMode = ExecutionMode.PowerShellSandbox,
                    OutputMessage = $"PowerShell ExitCode: {process.ExitCode}"
                };
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(true); } catch { }
                return new ExecutionResult { Success = false, UsedMode = ExecutionMode.PowerShellSandbox, OutputMessage = "PowerShell Execution Timeout (10s)" };
            }
        }
    }
}
