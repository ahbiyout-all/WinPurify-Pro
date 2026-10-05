using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WinPurifyPro.Services
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;

        public MEMORYSTATUSEX()
        {
            dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }

    public class SystemMetrics
    {
        public double CpuUsagePercent { get; set; }
        public double RamUsedGb { get; set; }
        public double RamTotalGb { get; set; }
        public double RamUsagePercent { get; set; }
        public double SystemDriveFreeGb { get; set; }
        public double SystemDriveTotalGb { get; set; }
        public double SystemDriveUsagePercent { get; set; }
    }

    public static class SystemDiagnosticsService
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        private static PerformanceCounter? _cpuCounter;
        private static bool _counterFailed = false;

        static SystemDiagnosticsService()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue(); // First call returns 0
            }
            catch
            {
                _counterFailed = true;
            }
        }

        public static async Task<SystemMetrics> GetCurrentMetricsAsync()
        {
            return await Task.Run(() =>
            {
                var metrics = new SystemMetrics();

                // 1. RAM Status via GlobalMemoryStatusEx
                try
                {
                    var memStatus = new MEMORYSTATUSEX();
                    if (GlobalMemoryStatusEx(memStatus))
                    {
                        double totalGb = memStatus.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                        double availGb = memStatus.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
                        metrics.RamTotalGb = totalGb;
                        metrics.RamUsedGb = Math.Max(0, totalGb - availGb);
                        metrics.RamUsagePercent = memStatus.dwMemoryLoad;
                    }
                }
                catch { }

                // 2. CPU Usage with Safety Guard
                try
                {
                    if (!_counterFailed && _cpuCounter != null)
                    {
                        metrics.CpuUsagePercent = Math.Round(_cpuCounter.NextValue(), 1);
                    }
                    else
                    {
                        metrics.CpuUsagePercent = 5.0; // Safe estimation fallback
                    }
                }
                catch
                {
                    _counterFailed = true;
                    metrics.CpuUsagePercent = 5.0;
                }

                // 3. System Drive Disk Free Space & Usage %
                try
                {
                    string systemDrive = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
                    var driveInfo = new DriveInfo(systemDrive);
                    if (driveInfo.IsReady)
                    {
                        double freeGb = driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                        double totalGb = driveInfo.TotalSize / (1024.0 * 1024.0 * 1024.0);
                        metrics.SystemDriveFreeGb = Math.Round(freeGb, 1);
                        metrics.SystemDriveTotalGb = Math.Round(totalGb, 1);

                        if (totalGb > 0)
                        {
                            double usedGb = totalGb - freeGb;
                            metrics.SystemDriveUsagePercent = Math.Round((usedGb / totalGb) * 100.0, 1);
                        }
                    }
                }
                catch { }

                return metrics;
            });
        }

        public static (double usedGb, double totalGb, double usagePercent) GetSystemMemoryInfo()
        {
            try
            {
                var memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    double totalGb = memStatus.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                    double availGb = memStatus.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
                    double usedGb = Math.Max(0, totalGb - availGb);
                    return (Math.Round(usedGb, 1), Math.Round(totalGb, 1), memStatus.dwMemoryLoad);
                }
            }
            catch { }
            return (0, 0, 0);
        }

        public static double GetSystemDriveFreeSpaceGb()
        {
            try
            {
                string systemDrive = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
                var driveInfo = new DriveInfo(systemDrive);
                if (driveInfo.IsReady)
                {
                    return Math.Round(driveInfo.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0), 1);
                }
            }
            catch { }
            return 0;
        }
    }
}
