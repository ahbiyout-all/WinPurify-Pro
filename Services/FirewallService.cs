using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// Windows 방화벽 예외 규칙(Inbound / Outbound) 자동 등록 및 해제 서비스
    /// - 버전 정보가 포함된 규칙명 (예: "WinPurify Pro v4.23.3", "WinPurify Central Commander v4.23.3")
    /// - 범용 클라이언트 규칙명 (예: "WinPurify Pro Main Client", "WinPurify Central Commander")
    /// - 인스톨러 및 애플리케이션 런타임 양방향 완벽 지원
    /// </summary>
    public static class FirewallService
    {
        public static string AppVersion
        {
            get
            {
                var ver = Assembly.GetExecutingAssembly().GetName().Version;
                return ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "4.23.3";
            }
        }

        /// <summary>
        /// 현재 실행 중인 메인 애플리케이션에 대해 버전 정보가 포함된 방화벽 규칙을 백그라운드에서 자동 등록합니다.
        /// </summary>
        public static void EnsureAppFirewallRulesAsync()
        {
            Task.Run(() =>
            {
                try
                {
                    string exePath = Process.GetCurrentProcess().MainModule?.FileName 
                                     ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WinPurifyPro.exe");
                    string version = AppVersion;

                    RegisterRule($"WinPurify Pro v{version}", "in", exePath, $"WinPurify Pro v{version} 메인 클라이언트 인바운드 방화벽 허용 규칙");
                    RegisterRule($"WinPurify Pro v{version} (Outbound)", "out", exePath, $"WinPurify Pro v{version} 메인 클라이언트 아웃바운드 방화벽 허용 규칙");
                    RegisterRule("WinPurify Pro Main Client", "in", exePath, "WinPurify Pro 메인 클라이언트 범용 인바운드 방화벽 허용 규칙");
                    RegisterRule("WinPurify Pro Main Client Outbound", "out", exePath, "WinPurify Pro 메인 클라이언트 범용 아웃바운드 방화벽 허용 규칙");
                }
                catch
                {
                    // 비관리자 권한 또는 방화벽 서비스 비활성화 시 예외 무시
                }
            });
        }

        /// <summary>
        /// Central Commander 실행 파일에 대해 버전 정보가 포함된 방화벽 규칙을 등록합니다.
        /// </summary>
        public static void EnsureCommanderFirewallRulesAsync()
        {
            Task.Run(() =>
            {
                try
                {
                    string exePath = Process.GetCurrentProcess().MainModule?.FileName 
                                     ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WinPurifyCommander.exe");
                    string version = AppVersion;

                    RegisterRule($"WinPurify Central Commander v{version}", "in", exePath, $"WinPurify Central Commander v{version} 원격 관제 콘솔 인바운드 방화벽 허용 규칙");
                    RegisterRule($"WinPurify Central Commander v{version} (Outbound)", "out", exePath, $"WinPurify Central Commander v{version} 원격 관제 콘솔 아웃바운드 방화벽 허용 규칙");
                    RegisterRule("WinPurify Central Commander", "in", exePath, "WinPurify Central Commander 원격 관제 콘솔 범용 인바운드 방화벽 허용 규칙");
                    RegisterRule("WinPurify Central Commander Outbound", "out", exePath, "WinPurify Central Commander 원격 관제 콘솔 범용 아웃바운드 방화벽 허용 규칙");
                }
                catch
                {
                    // 비관리자 권한 또는 방화벽 서비스 비활성화 시 예외 무시
                }
            });
        }

        /// <summary>
        /// 원격 관제 통신을 위한 포트 기반(TCP 9870 HTTP REST 및 UDP 9871 Discovery) 방화벽 인바운드/아웃바운드 규칙을 등록합니다.
        /// (인스톨러 기본 체크 '원격포트 열기' 옵션과 100% 동기화)
        /// </summary>
        public static void EnsureRemotePortsFirewallRulesAsync(int tcpPort = 9870, int udpPort = 9871)
        {
            Task.Run(() =>
            {
                try
                {
                    // 1. TCP REST API 엔드포인트 규칙
                    RegisterPortRule("WinPurify Remote Port TCP " + tcpPort, "in", "TCP", tcpPort, $"WinPurify Pro 원격 관제 HTTP REST 수신 포트(TCP {tcpPort})");
                    RegisterPortRule("WinPurify Remote Port TCP " + tcpPort + " Outbound", "out", "TCP", tcpPort, $"WinPurify Pro 원격 관제 HTTP REST 응답 포트(TCP {tcpPort} Outbound)");

                    // 2. UDP Discovery 브로드캐스트 규칙
                    RegisterPortRule("WinPurify Remote Discovery UDP " + udpPort, "in", "UDP", udpPort, $"WinPurify Pro 원격 자동 탐색 브로드캐스트 포트(UDP {udpPort})");
                    RegisterPortRule("WinPurify Remote Discovery UDP " + udpPort + " Outbound", "out", "UDP", udpPort, $"WinPurify Pro 원격 자동 탐색 브로드캐스트 송신 포트(UDP {udpPort} Outbound)");
                }
                catch
                {
                    // 비관리자 권한 또는 방화벽 서비스 비활성화 시 예외 무시
                }
            });
        }

        /// <summary>
        /// 원격 관제 통신 전용 포트 방화벽 규칙을 삭제합니다.
        /// </summary>
        public static void RemoveRemotePortsFirewallRulesAsync(int tcpPort = 9870, int udpPort = 9871)
        {
            Task.Run(() =>
            {
                try
                {
                    RunNetsh($"advfirewall firewall delete rule name=\"WinPurify Remote Port TCP {tcpPort}\"");
                    RunNetsh($"advfirewall firewall delete rule name=\"WinPurify Remote Port TCP {tcpPort} Outbound\"");
                    RunNetsh($"advfirewall firewall delete rule name=\"WinPurify Remote Discovery UDP {udpPort}\"");
                    RunNetsh($"advfirewall firewall delete rule name=\"WinPurify Remote Discovery UDP {udpPort} Outbound\"");
                }
                catch { }
            });
        }

        private static void RegisterPortRule(string ruleName, string direction, string protocol, int port, string description)
        {
            try
            {
                RunNetsh($"advfirewall firewall delete rule name=\"{ruleName}\"");
                string args = $"advfirewall firewall add rule name=\"{ruleName}\" dir={direction} action=allow protocol={protocol} localport={port} enable=yes profile=any description=\"{description}\"";
                RunNetsh(args);
            }
            catch { }
        }

        private static void RegisterRule(string ruleName, string direction, string programPath, string description)
        {
            try
            {
                // 기존 동일 규칙명이 있을 경우 중복 방지를 위해 삭제 후 재생성
                RunNetsh($"advfirewall firewall delete rule name=\"{ruleName}\"");

                // 인바운드 / 아웃바운드 규칙 등록
                string args = $"advfirewall firewall add rule name=\"{ruleName}\" dir={direction} action=allow program=\"{programPath}\" enable=yes profile=any description=\"{description}\"";
                RunNetsh(args);
            }
            catch { }
        }

        private static void RunNetsh(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netsh.exe",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using var proc = Process.Start(psi);
            proc?.WaitForExit(3000);
        }
    }
}
