using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WinPurifyPro.Models;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// 소규모 작업실(단일 공유기/스위치 LAN) 환경용 2단계 하이브리드 노드 탐색기
    /// 1단계: 다중 물리 NIC 서브넷 브로드캐스트 UDP 9871 PING/PONG (TLS 포트 및 인증서 지문 포함)
    /// 2단계: 공용 방화벽 프로필/스위치 브로드캐스트 차단 PC 복구를 위한 고속 /24 서브넷 병렬 TCP/ICMP 프로브
    /// </summary>
    public static class HybridNodeDiscoveryService
    {
        public static async Task<List<RemoteNodeItem>> DiscoverStudioNodesAsync(int udpTimeoutMs = 2200, bool enableSubnetProbeFallback = true)
        {
            var discoveredNodes = new List<RemoteNodeItem>();

            // Stage 1: Multi-Interface UDP Broadcast Discovery
            await RunUdpBroadcastDiscoveryAsync(discoveredNodes, udpTimeoutMs);

            // Stage 2: Fast /24 Local Subnet Parallel Port Probe (recovers nodes blocking UDP broadcast)
            if (enableSubnetProbeFallback)
            {
                await RunSubnetProbeFallbackAsync(discoveredNodes);
            }

            // Always ensure local host node is present
            string localLanIp = RemoteAgentService.GetBestLocalIPv4Address();
            string localThumbprint = ClusterSecurityVault.GetLocalTlsThumbprint();
            if (!discoveredNodes.Any(n => n.IpAddress == localLanIp || n.IpAddress == "127.0.0.1"))
            {
                discoveredNodes.Insert(0, new RemoteNodeItem
                {
                    Name = Environment.MachineName + " (현재 PC)",
                    IpAddress = localLanIp,
                    Port = RemoteAgentService.DefaultHttpPort,
                    TlsPort = RemoteAgentService.DefaultTlsPort,
                    TlsEnabled = true,
                    TlsThumbprint = localThumbprint,
                    IsOnline = true,
                    GroupName = "로컬 호스트",
                    LastSeen = "로컬 실행 중"
                });
            }

            return discoveredNodes;
        }

        private static async Task RunUdpBroadcastDiscoveryAsync(List<RemoteNodeItem> discoveredNodes, int timeoutMs)
        {
            using var udp = new UdpClient();
            udp.EnableBroadcast = true;

            try
            {
                byte[] pingBytes = Encoding.UTF8.GetBytes("WINPURIFY_DISCOVER_PING");
                var targets = GetBroadcastEndpoints(RemoteAgentService.DefaultDiscoveryPort);

                foreach (var ep in targets)
                {
                    try { await udp.SendAsync(pingBytes, pingBytes.Length, ep); } catch { }
                }

                using var cts = new CancellationTokenSource(timeoutMs);
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        var receiveTask = udp.ReceiveAsync();
                        var completed = await Task.WhenAny(receiveTask, Task.Delay(timeoutMs, cts.Token));
                        if (completed != receiveTask) break;

                        var result = await receiveTask;
                        string message = Encoding.UTF8.GetString(result.Buffer);
                        if (!message.StartsWith("WINPURIFY_DISCOVER_PONG")) continue;

                        // PONG format:
                        // Legacy: WINPURIFY_DISCOVER_PONG|Machine|HttpPort|OsVer|ONLINE|BestIp
                        // v4.69+: WINPURIFY_DISCOVER_PONG|Machine|HttpPort|OsVer|ONLINE|BestIp|TlsPort|TlsThumbprint
                        var parts = message.Split('|');
                        string nodeName = parts.Length > 1 ? parts[1] : result.RemoteEndPoint.Address.ToString();
                        int port = parts.Length > 2 && int.TryParse(parts[2], out int p) ? p : RemoteAgentService.DefaultHttpPort;
                        string reportedClientIp = parts.Length > 5 ? parts[5] : string.Empty;
                        int tlsPort = parts.Length > 6 && int.TryParse(parts[6], out int tp) ? tp : RemoteAgentService.DefaultTlsPort;
                        string thumbprint = parts.Length > 7 ? parts[7].Trim() : string.Empty;
                        bool tlsSupported = parts.Length > 6 && tlsPort > 0;

                        IPAddress remoteIp = result.RemoteEndPoint.Address;
                        if (remoteIp.IsIPv4MappedToIPv6) remoteIp = remoteIp.MapToIPv4();
                        string finalClientIp = remoteIp.ToString();

                        if (!string.IsNullOrWhiteSpace(reportedClientIp) &&
                            IPAddress.TryParse(reportedClientIp, out var parsedReported) &&
                            !IPAddress.IsLoopback(parsedReported) &&
                            !reportedClientIp.StartsWith("169.254."))
                        {
                            finalClientIp = reportedClientIp;
                        }
                        else if (IPAddress.IsLoopback(remoteIp))
                        {
                            finalClientIp = RemoteAgentService.GetBestLocalIPv4Address();
                        }

                        string bestLocal = RemoteAgentService.GetBestLocalIPv4Address();
                        bool isLocalHost = (finalClientIp == "127.0.0.1" || finalClientIp == bestLocal);

                        if (!discoveredNodes.Any(n => n.IpAddress == finalClientIp && n.Port == port))
                        {
                            discoveredNodes.Add(new RemoteNodeItem
                            {
                                Name = isLocalHost ? $"{nodeName} (현재 PC)" : nodeName,
                                IpAddress = finalClientIp,
                                Port = port,
                                TlsPort = tlsPort,
                                TlsEnabled = tlsSupported,
                                TlsThumbprint = thumbprint,
                                IsOnline = true,
                                GroupName = isLocalHost ? "로컬 호스트" : "작업실 LAN",
                                LastSeen = "방금 감지됨 (UDP)"
                            });
                        }
                    }
                    catch { break; }
                }
            }
            catch { }
        }

        private static async Task RunSubnetProbeFallbackAsync(List<RemoteNodeItem> discoveredNodes)
        {
            try
            {
                string localIp = RemoteAgentService.GetBestLocalIPv4Address();
                var octets = localIp.Split('.');
                if (octets.Length != 4) return;

                string subnetPrefix = $"{octets[0]}.{octets[1]}.{octets[2]}.";
                var existingIps = new HashSet<string>(discoveredNodes.Select(n => n.IpAddress), StringComparer.OrdinalIgnoreCase)
                {
                    localIp,
                    "127.0.0.1"
                };

                // Probe entire /24 host range (1..254) using ultra-fast bounded concurrency (160ms TCP SYN timeout)
                using var semaphore = new SemaphoreSlim(48);
                var probeTasks = new List<Task>();

                for (int host = 1; host <= 254; host++)
                {
                    string candidateIp = subnetPrefix + host;
                    if (existingIps.Contains(candidateIp)) continue;

                    probeTasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            bool tlsOpen = await IsTcpPortOpenAsync(candidateIp, RemoteAgentService.DefaultTlsPort, 160);
                            bool httpOpen = !tlsOpen && await IsTcpPortOpenAsync(candidateIp, RemoteAgentService.DefaultHttpPort, 160);

                            if (tlsOpen || httpOpen)
                            {
                                string hostLabel = $"STUDIO-PC-{host:D3}";
                                lock (discoveredNodes)
                                {
                                    if (!discoveredNodes.Any(n => n.IpAddress == candidateIp))
                                    {
                                        discoveredNodes.Add(new RemoteNodeItem
                                        {
                                            Name = hostLabel,
                                            IpAddress = candidateIp,
                                            Port = RemoteAgentService.DefaultHttpPort,
                                            TlsPort = RemoteAgentService.DefaultTlsPort,
                                            TlsEnabled = tlsOpen,
                                            IsOnline = true,
                                            GroupName = "서브넷 탐지 PC",
                                            LastSeen = tlsOpen ? "TCP:9874 서브넷 감지" : "TCP:9870 레거시 감지"
                                        });
                                    }
                                }
                            }
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }

                await Task.WhenAll(probeTasks);
            }
            catch { }
        }

        private static async Task<bool> IsTcpPortOpenAsync(string ip, int port, int timeoutMs)
        {
            try
            {
                using var tcp = new TcpClient();
                using var cts = new CancellationTokenSource(timeoutMs);
                await tcp.ConnectAsync(ip, port, cts.Token);
                return tcp.Connected;
            }
            catch
            {
                return false;
            }
        }

        private static List<IPEndPoint> GetBroadcastEndpoints(int port)
        {
            var list = new List<IPEndPoint> { new IPEndPoint(IPAddress.Broadcast, port) };

            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up
                               && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var nic in interfaces)
                {
                    foreach (var uni in nic.GetIPProperties().UnicastAddresses)
                    {
                        if (uni.Address.AddressFamily == AddressFamily.InterNetwork && uni.IPv4Mask != null)
                        {
                            byte[] ipBytes = uni.Address.GetAddressBytes();
                            byte[] maskBytes = uni.IPv4Mask.GetAddressBytes();
                            if (ipBytes.Length == 4 && maskBytes.Length == 4)
                            {
                                byte[] broadcast = new byte[4];
                                for (int i = 0; i < 4; i++)
                                {
                                    broadcast[i] = (byte)(ipBytes[i] | (~maskBytes[i]));
                                }
                                var bIp = new IPAddress(broadcast);
                                if (!list.Any(ep => ep.Address.Equals(bIp)))
                                {
                                    list.Add(new IPEndPoint(bIp, port));
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            return list;
        }
    }
}
