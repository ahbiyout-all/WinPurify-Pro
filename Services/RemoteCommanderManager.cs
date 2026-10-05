using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WinPurifyPro.Models;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// 중앙 관제 콘솔(Commander)에서 네트워크 상의 다중 PC 노드를 원격 제어하는 클라이언트 매니저
    /// </summary>
    public class RemoteCommanderManager
    {
        public static RemoteCommanderManager Instance { get; } = new RemoteCommanderManager();

        private readonly HttpClient _httpClient;
        private string _clusterSecretKey = string.Empty;
        public string ClusterSecretKey
        {
            get => !string.IsNullOrEmpty(_clusterSecretKey) ? _clusterSecretKey : RemoteAgentService.Instance.ClusterSecretKey;
            set => _clusterSecretKey = value;
        }

        private RemoteCommanderManager()
        {
            // 시스템 복원 지점 생성(VSS 스냅샷) 등 무거운 OS 레벨 배치 명령을 온전히 수신할 수 있도록 타임아웃 90초 설정
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(90) };
        }

        /// <summary>
        /// 모든 활성 물리 LAN 어댑터의 브로드캐스트 주소와 255.255.255.255로 동시 PING을 발송하여
        /// 가상 스위치(WSL/Hyper-V)에 방해받지 않고 LAN 상의 모든 WinPurify Pro 노드를 정확한 Client IP로 자동 검색합니다.
        /// </summary>
        public async Task<List<RemoteNodeItem>> DiscoverLocalNodesAsync(int timeoutMs = 2500)
        {
            var discoveredNodes = new List<RemoteNodeItem>();
            using var udp = new UdpClient();
            udp.EnableBroadcast = true;

            try
            {
                byte[] pingBytes = Encoding.UTF8.GetBytes("WINPURIFY_DISCOVER_PING");

                // 1. 글로벌 브로드캐스트 및 각 활성 인터페이스의 서브넷 브로드캐스트로 동시 발송
                var targets = GetBroadcastEndpoints(RemoteAgentService.DefaultDiscoveryPort);
                foreach (var ep in targets)
                {
                    try
                    {
                        await udp.SendAsync(pingBytes, pingBytes.Length, ep);
                    }
                    catch { }
                }

                using var cts = new CancellationTokenSource(timeoutMs);
                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        var receiveTask = udp.ReceiveAsync();
                        var completed = await Task.WhenAny(receiveTask, Task.Delay(timeoutMs, cts.Token));
                        if (completed == receiveTask)
                        {
                            var result = await receiveTask;
                            string message = Encoding.UTF8.GetString(result.Buffer);
                            if (message.StartsWith("WINPURIFY_DISCOVER_PONG"))
                            {
                                var parts = message.Split('|');
                                string nodeName = parts.Length > 1 ? parts[1] : result.RemoteEndPoint.Address.ToString();
                                int port = parts.Length > 2 && int.TryParse(parts[2], out int p) ? p : RemoteAgentService.DefaultHttpPort;
                                string osVer = parts.Length > 3 ? parts[3] : Environment.OSVersion.ToString();
                                string status = parts.Length > 4 ? parts[4] : "ONLINE";
                                string reportedClientIp = parts.Length > 5 ? parts[5] : string.Empty;

                                // 정확한 클라이언트 IPv4 결정 및 정규화
                                IPAddress remoteIp = result.RemoteEndPoint.Address;
                                if (remoteIp.IsIPv4MappedToIPv6) remoteIp = remoteIp.MapToIPv4();
                                string finalClientIp = remoteIp.ToString();

                                // 에이전트가 패킷에 탑재한 물리 LAN IP가 유효하면 우선 채택
                                if (!string.IsNullOrWhiteSpace(reportedClientIp) &&
                                    IPAddress.TryParse(reportedClientIp, out var parsedReported) &&
                                    !IPAddress.IsLoopback(parsedReported) &&
                                    !reportedClientIp.StartsWith("169.254."))
                                {
                                    finalClientIp = reportedClientIp;
                                }
                                else if (IPAddress.IsLoopback(remoteIp))
                                {
                                    // 로컬 루프백이면 시스템의 실제 물리 LAN IP로 보정
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
                                        IsOnline = true,
                                        GroupName = isLocalHost ? "로컬 호스트" : "사내/홈 네트워크",
                                        LastSeen = "방금 감지됨"
                                    });
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch { break; }
                }
            }
            catch { }

            // 로컬 노드가 아직 등록되지 않은 경우 정확한 로컬 LAN IP로 기본 1개 보장
            string localLanIp = RemoteAgentService.GetBestLocalIPv4Address();
            if (!discoveredNodes.Any(n => n.IpAddress == localLanIp || n.IpAddress == "127.0.0.1"))
            {
                discoveredNodes.Add(new RemoteNodeItem
                {
                    Name = Environment.MachineName + " (현재 PC)",
                    IpAddress = localLanIp,
                    Port = RemoteAgentService.DefaultHttpPort,
                    IsOnline = true,
                    GroupName = "로컬 호스트",
                    LastSeen = "로컬"
                });
            }

            return discoveredNodes;
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

        /// <summary>
        /// 특정 노드의 최신 텔레메트리 정보를 조회합니다.
        /// </summary>
        public async Task<NodeTelemetryDto?> FetchTelemetryAsync(string ip, int port)
        {
            string localLanIp = RemoteAgentService.GetBestLocalIPv4Address();
            bool isLocal = (ip == "127.0.0.1" || ip == "localhost" || ip == "::1" || ip == localLanIp);

            // 로컬 노드 대상인데 에이전트 서비스가 중지되어 있으면 자동 구동
            if (isLocal && !RemoteAgentService.Instance.IsRunning)
            {
                try { RemoteAgentService.Instance.StartAgent(); } catch { }
            }

            try
            {
                using var cts = new CancellationTokenSource(4000);
                string url = $"http://{ip}:{port}/api/v1/telemetry";
                string json = await _httpClient.GetStringAsync(url, cts.Token);
                return JsonSerializer.Deserialize<NodeTelemetryDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                // 로컬 노드인 경우 HTTP 요청 실패 시 로컬 텔레메트리 직접 조회로 무결성 보장
                if (isLocal)
                {
                    try
                    {
                        return RemoteAgentService.Instance.CollectCurrentTelemetry();
                    }
                    catch { }
                }

                return null;
            }
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 RAM 일괄 정화 명령을 병렬 전송합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchPurgeRamAsync(IEnumerable<RemoteNodeItem> targetNodes)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/ram-clean", new BatchCommandRequest
                    {
                        CommandType = "RamTrim",
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 복원 지점 생성 명령을 병렬 전송합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchCreateRestorePointAsync(IEnumerable<RemoteNodeItem> targetNodes)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/restore-point", new BatchCommandRequest
                    {
                        CommandType = "CreateRestorePoint",
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 최적화 프리셋(Safe, Deep, Gaming, Privacy 등)을 일괄 배포 및 적용합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchApplyPresetAsync(IEnumerable<RemoteNodeItem> targetNodes, string presetName)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/apply-preset", new BatchCommandRequest
                    {
                        CommandType = "ApplyPreset",
                        PresetName = presetName,
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 C++ 고속 유지보수(휴지통, DNS, Temp 파일) 명령을 병렬 전송합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchQuickMaintenanceAsync(IEnumerable<RemoteNodeItem> targetNodes, int quickType = 0)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/quick-maintenance", new BatchCommandRequest
                    {
                        CommandType = "QuickMaintenance",
                        QuickType = quickType,
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 e스포츠 게이밍 부스트(프로세스 우선순위 격상 및 지연시간 단축)를 병렬 배포합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchGameBoostAsync(IEnumerable<RemoteNodeItem> targetNodes)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/game-boost", new BatchCommandRequest
                    {
                        CommandType = "GameBoost",
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 DNS 캐시 플러시 및 네트워크 소켓 초기화 명령을 전송합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchFlushDnsAsync(IEnumerable<RemoteNodeItem> targetNodes)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/dns-flush", new BatchCommandRequest
                    {
                        CommandType = "FlushDns",
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 원격 무결성 및 자가진단 헬스체크 명령을 전송합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchRunDiagnosticsAsync(IEnumerable<RemoteNodeItem> targetNodes)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/diagnostics", new BatchCommandRequest
                    {
                        CommandType = "Diagnostics",
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 주간/일간 자동 스케줄러를 일괄 배포 및 등록합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchConfigureScheduleAsync(
            IEnumerable<RemoteNodeItem> targetNodes,
            string frequency,
            int dayOfWeek,
            int hour,
            int minute,
            string profile,
            bool autoTrimRam)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/schedule", new BatchCommandRequest
                    {
                        CommandType = "ConfigureSchedule",
                        ScheduleFrequency = frequency,
                        ScheduleDay = dayOfWeek,
                        ScheduleHour = hour,
                        ScheduleMinute = minute,
                        ScheduleProfile = profile,
                        ScheduleAutoTrimRam = autoTrimRam,
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에서 자동 스케줄러 등록을 일괄 해제합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchUnregisterScheduleAsync(IEnumerable<RemoteNodeItem> targetNodes)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/unschedule", new BatchCommandRequest
                    {
                        CommandType = "UnregisterSchedule",
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 원격 안전 재부팅 명령을 전송합니다.
        /// (원격 노드 전송 완료 후 로컬 머신이 있을 경우 맨 마지막에 재부팅하여 패킷 중단 방지)
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchRebootAsync(IEnumerable<RemoteNodeItem> targetNodes)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            string localLan = RemoteAgentService.GetBestLocalIPv4Address();

            var nodeList = targetNodes.ToList();
            var remoteOnly = nodeList.Where(n => !(n.IpAddress == "127.0.0.1" || n.IpAddress == "localhost" || n.IpAddress == "::1" || n.IpAddress == localLan)).ToList();
            var localOnly = nodeList.Where(n => (n.IpAddress == "127.0.0.1" || n.IpAddress == "localhost" || n.IpAddress == "::1" || n.IpAddress == localLan)).ToList();

            // 1. 원격 노드들에게 먼저 병렬 전송
            var tasks = new List<Task>();
            foreach (var node in remoteOnly)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/reboot", new BatchCommandRequest
                    {
                        CommandType = "Reboot",
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }

            // 2. 로컬 노드(자신)가 포함되어 있다면 모든 원격 전송 완료 후 마지막에 처리
            foreach (var node in localOnly)
            {
                var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/reboot", new BatchCommandRequest
                {
                    CommandType = "Reboot",
                    SecretKey = ClusterSecretKey
                });

                // 만약 HTTP 실패 시 SystemPowerService를 직접 구동하여 무조건 재부팅 보장
                if (!resp.Success)
                {
                    try
                    {
                        SystemPowerService.ScheduleForceReboot(3);
                        resp = new CommandResponseDto { Success = true, Message = "[로컬 직접 실행] 3초 후 4-Tier 시스템 강제 재부팅(/r /f) 시작" };
                    }
                    catch { }
                }

                lock (results)
                {
                    results.Add((node, resp));
                }
            }

            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 원격 시스템 안전 종료(Shutdown) 명령을 전송합니다.
        /// (원격 노드 전송 완료 후 로컬 머신이 있을 경우 맨 마지막에 종료하여 패킷 중단 방지)
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchShutdownAsync(IEnumerable<RemoteNodeItem> targetNodes)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            string localLan = RemoteAgentService.GetBestLocalIPv4Address();

            var nodeList = targetNodes.ToList();
            var remoteOnly = nodeList.Where(n => !(n.IpAddress == "127.0.0.1" || n.IpAddress == "localhost" || n.IpAddress == "::1" || n.IpAddress == localLan)).ToList();
            var localOnly = nodeList.Where(n => (n.IpAddress == "127.0.0.1" || n.IpAddress == "localhost" || n.IpAddress == "::1" || n.IpAddress == localLan)).ToList();

            // 1. 원격 노드들에게 먼저 병렬 전송
            var tasks = new List<Task>();
            foreach (var node in remoteOnly)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/shutdown", new BatchCommandRequest
                    {
                        CommandType = "Shutdown",
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }

            // 2. 로컬 노드(자신)가 포함되어 있다면 모든 원격 전송 완료 후 마지막에 처리
            foreach (var node in localOnly)
            {
                var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/shutdown", new BatchCommandRequest
                {
                    CommandType = "Shutdown",
                    SecretKey = ClusterSecretKey
                });

                // 만약 HTTP 실패 시 SystemPowerService를 직접 구동하여 무조건 시스템 종료 보장
                if (!resp.Success)
                {
                    try
                    {
                        SystemPowerService.ScheduleForceShutdown(3);
                        resp = new CommandResponseDto { Success = true, Message = "[로컬 직접 실행] 3초 후 4-Tier 시스템 강제 종료(/s /f) 시작" };
                    }
                    catch { }
                }

                lock (results)
                {
                    results.Add((node, resp));
                }
            }

            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 최신 세이프포인트(백업 레지스트리 및 시스템 상태) 1-Click 롤백 복원을 명령합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchRollbackSafepointAsync(IEnumerable<RemoteNodeItem> targetNodes)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/rollback", new BatchCommandRequest
                    {
                        CommandType = "Rollback",
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 모든 사용 흔적, 자격증명, 세션 캐시를 백지화하는 Zero-Trace 안티포렌식 완전 초기화를 명령합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchZeroTraceResetAsync(IEnumerable<RemoteNodeItem> targetNodes)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/zero-trace-reset", new BatchCommandRequest
                    {
                        CommandType = "ZeroTraceReset",
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 통합 계정 & 세션 토큰 정화(강제 로그아웃/자격증명 파쇄) 원격 명령을 병렬 전송합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchPurgeAccountSessionsAsync(
            IEnumerable<RemoteNodeItem> targetNodes,
            string scenarioPreset = "workplace",
            bool terminateProcesses = true,
            List<string>? targetIds = null)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/account-purge", new BatchCommandRequest
                    {
                        CommandType = "AccountPurge",
                        AccountPurgePreset = scenarioPreset,
                        TerminateProcessesBeforePurge = terminateProcesses,
                        TargetAccountIds = targetIds,
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 현재 사용자 설정을 Windows 기본 프로필(Default)로 복제 배포하는 원격 명령을 병렬 전송합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchSyncDefaultProfileAsync(
            IEnumerable<RemoteNodeItem> targetNodes,
            bool backupExisting = true)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/profile-sync", new BatchCommandRequest
                    {
                        CommandType = "ProfileSync",
                        BackupExistingDefaultProfile = backupExisting,
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 브라우저 전체 프로필 및 사용자 데이터 공장 초기화(영구 소거) 명령을 병렬 전송합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchBrowserFactoryResetAsync(
            IEnumerable<RemoteNodeItem> targetNodes,
            string browserTarget = "all")
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/browser-factory-reset", new BatchCommandRequest
                    {
                        CommandType = "BrowserFactoryReset",
                        BrowserTarget = browserTarget,
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 선택된 다중 PC 노드들에게 디스크 빈 공간 난수 덮어쓰기 포렌식 파쇄(cipher /w) 명령을 병렬 전송합니다.
        /// </summary>
        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchForensicsWipeAsync(
            IEnumerable<RemoteNodeItem> targetNodes,
            string targetDrive = "C:")
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = new List<Task>();

            foreach (var node in targetNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, "/api/v1/batch/forensics-wipe", new BatchCommandRequest
                    {
                        CommandType = "ForensicsWipe",
                        ForensicsTargetDrive = targetDrive,
                        SecretKey = ClusterSecretKey
                    });

                    lock (results)
                    {
                        results.Add((node, resp));
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// 단일 노드에 특정 커맨드를 즉시 실행합니다.
        /// </summary>
        public async Task<CommandResponseDto> SendSingleNodeCommandAsync(
            RemoteNodeItem node,
            string commandType,
            string? presetName = null,
            int quickType = 0,
            string? accountPreset = "workplace",
            bool terminateProcesses = true,
            string? browserTarget = "all",
            string? drive = "C:")
        {
            string path = commandType.ToLowerInvariant() switch
            {
                "ramtrim" => "/api/v1/batch/ram-clean",
                "createrestorepoint" => "/api/v1/batch/restore-point",
                "quickmaintenance" => "/api/v1/batch/quick-maintenance",
                "applypreset" => "/api/v1/batch/apply-preset",
                "gameboost" => "/api/v1/batch/game-boost",
                "flushdns" => "/api/v1/batch/dns-flush",
                "diagnostics" => "/api/v1/batch/diagnostics",
                "rollback" => "/api/v1/batch/rollback",
                "zerotracereset" => "/api/v1/batch/zero-trace-reset",
                "accountpurge" or "zerotraceaccountpurge" or "accountsessionpurge" => "/api/v1/batch/account-purge",
                "profilesync" or "defaultprofilereplication" => "/api/v1/batch/profile-sync",
                "browserfactoryreset" => "/api/v1/batch/browser-factory-reset",
                "forensicswipe" => "/api/v1/batch/forensics-wipe",
                "reboot" => "/api/v1/batch/reboot",
                "shutdown" => "/api/v1/batch/shutdown",
                _ => $"/api/v1/batch/{commandType.ToLowerInvariant()}"
            };

            return await SendSignedCommandAsync(node.IpAddress, node.Port, path, new BatchCommandRequest
            {
                CommandType = commandType,
                PresetName = presetName,
                QuickType = quickType,
                AccountPurgePreset = accountPreset,
                TerminateProcessesBeforePurge = terminateProcesses,
                BrowserTarget = browserTarget ?? "all",
                ForensicsTargetDrive = drive ?? "C:",
                SecretKey = ClusterSecretKey
            });
        }

        private async Task<CommandResponseDto> SendSignedCommandAsync(string ip, int port, string path, BatchCommandRequest req)
        {
            string localLanIp = RemoteAgentService.GetBestLocalIPv4Address();
            bool isLocal = (ip == "127.0.0.1" || ip == "localhost" || ip == "::1" || ip == localLanIp);

            // 로컬 노드 대상인데 에이전트 서비스가 중지되어 있으면 자동 구동 시도
            if (isLocal && !RemoteAgentService.Instance.IsRunning)
            {
                try { RemoteAgentService.Instance.StartAgent(); } catch { }
            }

            try
            {
                long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string nonce = Guid.NewGuid().ToString("N");
                req.Timestamp = ts;
                req.Nonce = nonce;

                string json = JsonSerializer.Serialize(req);
                string signature = RemoteAgentService.ComputeHmacSha256(json, ClusterSecretKey);

                using var requestMsg = new HttpRequestMessage(HttpMethod.Post, $"http://{ip}:{port}{path}")
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };

                requestMsg.Headers.Add("X-WinPurify-Signature", signature);
                requestMsg.Headers.Add("X-WinPurify-Timestamp", ts.ToString());
                requestMsg.Headers.Add("X-WinPurify-Nonce", nonce);

                var response = await _httpClient.SendAsync(requestMsg);
                string respJson = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<CommandResponseDto>(respJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                           ?? new CommandResponseDto { Success = true, Message = "Command executed successfully" };
                }
                else
                {
                    // 로컬 노드인 경우 HTTP 에러 반환 시 로컬 직접 실행으로 완벽 대체
                    if (isLocal)
                    {
                        try
                        {
                            var directResp = await RemoteAgentService.Instance.ExecuteBatchCommandAsync(path, req);
                            directResp.Message = $"[로컬 직접 실행] {directResp.Message}";
                            return directResp;
                        }
                        catch
                        {
                            if (req.CommandType.Equals("Reboot", StringComparison.OrdinalIgnoreCase))
                            {
                                SystemPowerService.ScheduleForceReboot(3);
                                return new CommandResponseDto { Success = true, Message = "[로컬 비상 직접 실행] 3초 후 4-Tier 시스템 강제 재부팅(/r /f) 시작" };
                            }
                            else if (req.CommandType.Equals("Shutdown", StringComparison.OrdinalIgnoreCase))
                            {
                                SystemPowerService.ScheduleForceShutdown(3);
                                return new CommandResponseDto { Success = true, Message = "[로컬 비상 직접 실행] 3초 후 4-Tier 시스템 강제 종료(/s /f) 시작" };
                            }
                        }
                    }

                    return new CommandResponseDto { Success = false, Message = $"HTTP {(int)response.StatusCode}: {respJson}" };
                }
            }
            catch (Exception ex)
            {
                // 로컬 노드 대상인데 소켓 거부(WSAECONNREFUSED) 또는 네트워크 차단 발생 시 로컬 네이티브 엔진으로 직접 무중단 실행
                if (isLocal)
                {
                    try
                    {
                        var directResp = await RemoteAgentService.Instance.ExecuteBatchCommandAsync(path, req);
                        directResp.Message = $"[로컬 엔진 직접 실행] {directResp.Message}";
                        return directResp;
                    }
                    catch (Exception localEx)
                    {
                        if (req.CommandType.Equals("Reboot", StringComparison.OrdinalIgnoreCase))
                        {
                            SystemPowerService.ScheduleForceReboot(3);
                            return new CommandResponseDto { Success = true, Message = "[로컬 비상 직접 실행] 3초 후 4-Tier 시스템 강제 재부팅(/r /f) 시작" };
                        }
                        else if (req.CommandType.Equals("Shutdown", StringComparison.OrdinalIgnoreCase))
                        {
                            SystemPowerService.ScheduleForceShutdown(3);
                            return new CommandResponseDto { Success = true, Message = "[로컬 비상 직접 실행] 3초 후 4-Tier 시스템 강제 종료(/s /f) 시작" };
                        }
                        return new CommandResponseDto { Success = false, Message = $"로컬 직접 실행 오류: {localEx.Message}" };
                    }
                }

                return new CommandResponseDto { Success = false, Message = $"Connection Error: {ex.Message} ({ip}:{port})" };
            }
        }
    }
}
