using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WinPurifyPro.Models;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// 중앙 관제 콘솔(Commander)에서 네트워크 상의 다중 PC 노드를 원격 제어하는 클라이언트 매니저
    /// - TLS 1.3 (TCP 9874) 우선 암호화 채널 및 TOFU 인증서 지문 고정
    /// - 텔레메트리 요청 HMAC-SHA256 서명 인증 및 6자리 일회용 PIN 페어링 지원
    /// - 평문 SecretKey 네트워크 전송 완전 폐기
    /// </summary>
    public class RemoteCommanderManager
    {
        public static RemoteCommanderManager Instance { get; } = new RemoteCommanderManager();

        private readonly HttpClient _httpClient;
        private readonly HttpClient _tlsHttpClient;
        private readonly ConcurrentDictionary<string, string> _expectedThumbprints = new(StringComparer.OrdinalIgnoreCase);

        private string _clusterSecretKey = string.Empty;
        public string ClusterSecretKey
        {
            get
            {
                if (string.IsNullOrEmpty(_clusterSecretKey))
                {
                    _clusterSecretKey = ClusterSecurityVault.GetOrCreateClusterKey();
                }
                return _clusterSecretKey;
            }
            set => _clusterSecretKey = value;
        }

        public bool AllowLegacyHttpFallback { get; set; } = true;

        private RemoteCommanderManager()
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(90) };

            var tlsHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (requestMessage, cert, chain, sslPolicyErrors) =>
                {
                    if (requestMessage?.RequestUri == null || cert == null) return false;
                    string host = requestMessage.RequestUri.Host;
                    int port = requestMessage.RequestUri.Port;

                    _expectedThumbprints.TryGetValue(host, out string? expected);
                    var (isTrusted, _) = ClusterSecurityVault.VerifyOrPinNodeCertificate(host, port, cert, expected);
                    return isTrusted;
                }
            };

            _tlsHttpClient = new HttpClient(tlsHandler) { Timeout = TimeSpan.FromSeconds(90) };
        }

        public void RegisterExpectedThumbprint(string ip, string? thumbprint)
        {
            if (!string.IsNullOrWhiteSpace(ip) && !string.IsNullOrWhiteSpace(thumbprint))
            {
                _expectedThumbprints[ip.Trim()] = thumbprint.Trim();
            }
        }

        public async Task<List<RemoteNodeItem>> DiscoverLocalNodesAsync(int timeoutMs = 2500)
        {
            var nodes = await HybridNodeDiscoveryService.DiscoverStudioNodesAsync(timeoutMs, enableSubnetProbeFallback: true);
            foreach (var node in nodes)
            {
                if (!string.IsNullOrWhiteSpace(node.TlsThumbprint))
                {
                    RegisterExpectedThumbprint(node.IpAddress, node.TlsThumbprint);
                }
            }
            return nodes;
        }

        /// <summary>
        /// 작업실 노드와 6자리 일회용 PIN으로 TLS 페어링하여 클러스터 키를 안전하게 동기화합니다.
        /// </summary>
        public async Task<StudioPairResponseDto> PairWithNodeAsync(string ip, int tlsPort, string pairingPin)
        {
            try
            {
                var reqDto = new StudioPairRequestDto
                {
                    PairingPin = pairingPin.Trim(),
                    CommanderMachineName = Environment.MachineName
                };
                string json = JsonSerializer.Serialize(reqDto);
                string url = $"https://{ip}:{tlsPort}/api/v1/auth/pair";

                using var cts = new CancellationTokenSource(8000);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await _tlsHttpClient.PostAsync(url, content, cts.Token);
                string respJson = await resp.Content.ReadAsStringAsync(cts.Token);

                var pairResp = JsonSerializer.Deserialize<StudioPairResponseDto>(respJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                               ?? new StudioPairResponseDto { Success = false, Message = "응답 파싱 실패" };

                if (resp.IsSuccessStatusCode && pairResp.Success && !string.IsNullOrWhiteSpace(pairResp.ClusterKey))
                {
                    ClusterSecretKey = pairResp.ClusterKey;
                    ClusterSecurityVault.SaveClusterKey(pairResp.ClusterKey);
                    RegisterExpectedThumbprint(ip, pairResp.TlsThumbprint);
                }

                return pairResp;
            }
            catch (Exception ex)
            {
                return new StudioPairResponseDto
                {
                    Success = false,
                    Message = $"TLS 페어링 연결 오류 ({ip}:{tlsPort}): {ex.Message}"
                };
            }
        }

        /// <summary>
        /// 특정 노드의 최신 텔레메트리 정보를 TLS 우선 + HMAC 서명 헤더로 조회합니다.
        /// </summary>
        public async Task<NodeTelemetryDto?> FetchTelemetryAsync(string ip, int port, int tlsPort = RemoteAgentService.DefaultTlsPort)
        {
            string localLanIp = RemoteAgentService.GetBestLocalIPv4Address();
            bool isLocal = (ip == "127.0.0.1" || ip == "localhost" || ip == "::1" || ip == localLanIp);

            if (isLocal && !RemoteAgentService.Instance.IsRunning)
            {
                try { RemoteAgentService.Instance.StartAgent(); } catch { }
            }

            // 1. Try encrypted TLS endpoint first
            try
            {
                using var cts = new CancellationTokenSource(3500);
                using var reqMsg = CreateSignedGetRequest($"https://{ip}:{tlsPort}/api/v1/telemetry", "/api/v1/telemetry");
                var resp = await _tlsHttpClient.SendAsync(reqMsg, cts.Token);
                if (resp.IsSuccessStatusCode)
                {
                    string json = await resp.Content.ReadAsStringAsync(cts.Token);
                    var dto = JsonSerializer.Deserialize<NodeTelemetryDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dto != null)
                    {
                        dto.TlsSupported = true;
                        RegisterExpectedThumbprint(ip, dto.TlsThumbprint);
                        return dto;
                    }
                }
            }
            catch { }

            // 2. Legacy HTTP fallback (still signed)
            if (AllowLegacyHttpFallback)
            {
                try
                {
                    using var cts = new CancellationTokenSource(3500);
                    using var reqMsg = CreateSignedGetRequest($"http://{ip}:{port}/api/v1/telemetry", "/api/v1/telemetry");
                    var resp = await _httpClient.SendAsync(reqMsg, cts.Token);
                    if (resp.IsSuccessStatusCode)
                    {
                        string json = await resp.Content.ReadAsStringAsync(cts.Token);
                        var dto = JsonSerializer.Deserialize<NodeTelemetryDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (dto != null)
                        {
                            dto.TlsSupported = false;
                            dto.SecurityMode = "HTTP (레거시 - 업그레이드 권장)";
                            return dto;
                        }
                    }
                }
                catch { }
            }

            if (isLocal)
            {
                try { return RemoteAgentService.Instance.CollectCurrentTelemetry(); } catch { }
            }

            return null;
        }

        private HttpRequestMessage CreateSignedGetRequest(string url, string canonicalPath)
        {
            long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string nonce = Guid.NewGuid().ToString("N");
            string canonical = $"GET:{canonicalPath.ToLowerInvariant()}:{ts}:{nonce}";
            string sig = RemoteAgentService.ComputeHmacSha256(canonical, ClusterSecretKey);

            var msg = new HttpRequestMessage(HttpMethod.Get, url);
            msg.Headers.Add("X-WinPurify-Signature", sig);
            msg.Headers.Add("X-WinPurify-Timestamp", ts.ToString());
            msg.Headers.Add("X-WinPurify-Nonce", nonce);
            return msg;
        }

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchPurgeRamAsync(IEnumerable<RemoteNodeItem> targetNodes) =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/ram-clean", new BatchCommandRequest { CommandType = "RamTrim" }));

        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchWakeOnLanAsync(IEnumerable<RemoteNodeItem> targetNodes)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = targetNodes.Select(node => Task.Run(() =>
            {
                bool sent = !string.IsNullOrWhiteSpace(node.MacAddress) && NativeEngineService.TrySendMagicPacket(node.MacAddress);
                var resp = new CommandResponseDto
                {
                    Success = sent,
                    Message = sent
                        ? $"네이티브 하드웨어 WOL 매직 패킷 송신 완료 ({node.Name} / {node.MacAddress})"
                        : $"WOL 패킷 송신 실패 (MAC 주소 미등록 또는 소켓 오류: {node.Name})"
                };
                lock (results) results.Add((node, resp));
            })).ToList();

            await Task.WhenAll(tasks);
            return results;
        }

        public uint PingNodeNative(string ip)
            => NativeEngineService.TryFastIcmpPing(ip, 1000, out uint rtt) ? rtt : 0;

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchCreateRestorePointAsync(IEnumerable<RemoteNodeItem> targetNodes) =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/restore-point", new BatchCommandRequest { CommandType = "CreateRestorePoint" }));

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchApplyPresetAsync(IEnumerable<RemoteNodeItem> targetNodes, string presetName) =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/apply-preset", new BatchCommandRequest { CommandType = "ApplyPreset", PresetName = presetName }));

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchQuickMaintenanceAsync(IEnumerable<RemoteNodeItem> targetNodes, int quickType = 0) =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/quick-maintenance", new BatchCommandRequest { CommandType = "QuickMaintenance", QuickType = quickType }));

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchGameBoostAsync(IEnumerable<RemoteNodeItem> targetNodes) =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/game-boost", new BatchCommandRequest { CommandType = "GameBoost" }));

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchFlushDnsAsync(IEnumerable<RemoteNodeItem> targetNodes) =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/dns-flush", new BatchCommandRequest { CommandType = "FlushDns" }));

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchRunDiagnosticsAsync(IEnumerable<RemoteNodeItem> targetNodes) =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/diagnostics", new BatchCommandRequest { CommandType = "Diagnostics" }));

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchConfigureScheduleAsync(
            IEnumerable<RemoteNodeItem> targetNodes, string frequency, int dayOfWeek, int hour, int minute, string profile, bool autoTrimRam) =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/schedule", new BatchCommandRequest
            {
                CommandType = "ConfigureSchedule",
                ScheduleFrequency = frequency,
                ScheduleDay = dayOfWeek,
                ScheduleHour = hour,
                ScheduleMinute = minute,
                ScheduleProfile = profile,
                ScheduleAutoTrimRam = autoTrimRam
            }));

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchUnregisterScheduleAsync(IEnumerable<RemoteNodeItem> targetNodes) =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/unschedule", new BatchCommandRequest { CommandType = "UnregisterSchedule" }));

        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchRebootAsync(IEnumerable<RemoteNodeItem> targetNodes)
        {
            return await ExecuteOrderedPowerBatchAsync(targetNodes, "Reboot", "/api/v1/batch/reboot", isReboot: true);
        }

        public async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchShutdownAsync(IEnumerable<RemoteNodeItem> targetNodes)
        {
            return await ExecuteOrderedPowerBatchAsync(targetNodes, "Shutdown", "/api/v1/batch/shutdown", isReboot: false);
        }

        private async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> ExecuteOrderedPowerBatchAsync(
            IEnumerable<RemoteNodeItem> targetNodes, string cmdType, string path, bool isReboot)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            string localLan = RemoteAgentService.GetBestLocalIPv4Address();

            var nodeList = targetNodes.ToList();
            var remoteOnly = nodeList.Where(n => !(n.IpAddress == "127.0.0.1" || n.IpAddress == "localhost" || n.IpAddress == "::1" || n.IpAddress == localLan)).ToList();
            var localOnly = nodeList.Where(n => (n.IpAddress == "127.0.0.1" || n.IpAddress == "localhost" || n.IpAddress == "::1" || n.IpAddress == localLan)).ToList();

            if (remoteOnly.Count > 0)
            {
                var remoteResults = await ExecuteBatchFanOutAsync(remoteOnly, _ => (path, new BatchCommandRequest { CommandType = cmdType }));
                results.AddRange(remoteResults);
            }

            foreach (var node in localOnly)
            {
                var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, node.TlsPort, path, new BatchCommandRequest { CommandType = cmdType });
                if (!resp.Success)
                {
                    try
                    {
                        if (isReboot) SystemPowerService.ScheduleForceReboot(3);
                        else SystemPowerService.ScheduleForceShutdown(3);
                        resp = new CommandResponseDto
                        {
                            Success = true,
                            Message = isReboot
                                ? "[로컬 직접 실행] 3초 후 4-Tier 시스템 강제 재부팅(/r /f) 시작"
                                : "[로컬 직접 실행] 3초 후 4-Tier 시스템 강제 종료(/s /f) 시작"
                        };
                    }
                    catch { }
                }
                results.Add((node, resp));
            }

            return results;
        }

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchRollbackSafepointAsync(IEnumerable<RemoteNodeItem> targetNodes) =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/rollback", new BatchCommandRequest { CommandType = "Rollback" }));

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchZeroTraceResetAsync(IEnumerable<RemoteNodeItem> targetNodes) =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/zero-trace-reset", new BatchCommandRequest { CommandType = "ZeroTraceReset" }));

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchPurgeAccountSessionsAsync(
            IEnumerable<RemoteNodeItem> targetNodes, string scenarioPreset = "workplace", bool terminateProcesses = true, List<string>? targetIds = null) =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/account-purge", new BatchCommandRequest
            {
                CommandType = "AccountPurge",
                AccountPurgePreset = scenarioPreset,
                TerminateProcessesBeforePurge = terminateProcesses,
                TargetAccountIds = targetIds
            }));

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchSyncDefaultProfileAsync(IEnumerable<RemoteNodeItem> targetNodes, bool backupExisting = true) =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/profile-sync", new BatchCommandRequest
            {
                CommandType = "ProfileSync",
                BackupExistingDefaultProfile = backupExisting
            }));

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchBrowserFactoryResetAsync(IEnumerable<RemoteNodeItem> targetNodes, string browserTarget = "all") =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/browser-factory-reset", new BatchCommandRequest
            {
                CommandType = "BrowserFactoryReset",
                BrowserTarget = browserTarget
            }));

        public Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> BatchForensicsWipeAsync(IEnumerable<RemoteNodeItem> targetNodes, string targetDrive = "C:") =>
            ExecuteBatchFanOutAsync(targetNodes, _ => ("/api/v1/batch/forensics-wipe", new BatchCommandRequest
            {
                CommandType = "ForensicsWipe",
                ForensicsTargetDrive = targetDrive
            }));

        private async Task<List<(RemoteNodeItem Node, CommandResponseDto Response)>> ExecuteBatchFanOutAsync(
            IEnumerable<RemoteNodeItem> targetNodes,
            Func<RemoteNodeItem, (string Path, BatchCommandRequest Request)> requestFactory)
        {
            var results = new List<(RemoteNodeItem Node, CommandResponseDto Response)>();
            var tasks = targetNodes.Select(node => Task.Run(async () =>
            {
                var (path, req) = requestFactory(node);
                var resp = await SendSignedCommandAsync(node.IpAddress, node.Port, node.TlsPort, path, req);
                lock (results) results.Add((node, resp));
            })).ToList();

            await Task.WhenAll(tasks);
            return results;
        }

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
            string safeCmd = (commandType ?? string.Empty).Trim();
            string path = safeCmd.ToLowerInvariant() switch
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
                _ => $"/api/v1/batch/{safeCmd.ToLowerInvariant()}"
            };

            return await SendSignedCommandAsync(node.IpAddress, node.Port, node.TlsPort, path, new BatchCommandRequest
            {
                CommandType = commandType,
                PresetName = presetName,
                QuickType = quickType,
                AccountPurgePreset = accountPreset,
                TerminateProcessesBeforePurge = terminateProcesses,
                BrowserTarget = browserTarget ?? "all",
                ForensicsTargetDrive = drive ?? "C:"
            });
        }

        public async Task<(string? Nonce, bool UsedTls)> FetchChallengeNonceAsync(string ip, int port, int tlsPort = RemoteAgentService.DefaultTlsPort)
        {
            try
            {
                using var cts = new CancellationTokenSource(3500);
                var resp = await _tlsHttpClient.GetAsync($"https://{ip}:{tlsPort}/api/v1/auth/challenge", cts.Token);
                if (resp.IsSuccessStatusCode)
                {
                    string json = await resp.Content.ReadAsStringAsync(cts.Token);
                    var obj = JsonSerializer.Deserialize<AuthChallengeResponseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (obj?.ChallengeNonce, true);
                }
            }
            catch { }

            if (AllowLegacyHttpFallback)
            {
                try
                {
                    using var cts = new CancellationTokenSource(3500);
                    var resp = await _httpClient.GetAsync($"http://{ip}:{port}/api/v1/auth/challenge", cts.Token);
                    if (resp.IsSuccessStatusCode)
                    {
                        string json = await resp.Content.ReadAsStringAsync(cts.Token);
                        var obj = JsonSerializer.Deserialize<AuthChallengeResponseDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return (obj?.ChallengeNonce, false);
                    }
                }
                catch { }
            }

            return (null, false);
        }

        private async Task<CommandResponseDto> SendSignedCommandAsync(
            string ip,
            int httpPort,
            int tlsPort,
            string path,
            BatchCommandRequest req,
            bool forceChallengeHandshake = false)
        {
            string localLanIp = RemoteAgentService.GetBestLocalIPv4Address();
            bool isLocal = (ip == "127.0.0.1" || ip == "localhost" || ip == "::1" || ip == localLanIp);

            if (isLocal && !RemoteAgentService.Instance.IsRunning)
            {
                try { RemoteAgentService.Instance.StartAgent(); } catch { }
            }

            // NEVER transmit SecretKey in JSON payload body
            req.SecretKey = string.Empty;

            bool isSensitiveCommand = forceChallengeHandshake ||
                string.Equals(req.CommandType, "Reboot", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(req.CommandType, "Shutdown", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(req.CommandType, "ZeroTraceReset", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(req.CommandType, "Rollback", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(req.CommandType, "ForensicsWipe", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(req.CommandType, "AccountPurge", StringComparison.OrdinalIgnoreCase);

            long handshakeLatency = 0;

            if (isSensitiveCommand)
            {
                var swHandshake = System.Diagnostics.Stopwatch.StartNew();
                var (challengeNonce, _) = await FetchChallengeNonceAsync(ip, httpPort, tlsPort);
                swHandshake.Stop();
                handshakeLatency = swHandshake.ElapsedMilliseconds;

                if (!string.IsNullOrEmpty(challengeNonce))
                {
                    try
                    {
                        long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        string nonce = Guid.NewGuid().ToString("N");
                        req.Timestamp = ts;
                        req.Nonce = nonce;
                        req.ChallengeNonce = challengeNonce;

                        string rawJson = JsonSerializer.Serialize(req);
                        string challengePayload = $"{challengeNonce}:{req.CommandType}:{rawJson}";
                        string challengeSig = RemoteAgentService.ComputeHmacSha256(challengePayload, ClusterSecretKey);

                        var response = await PostWithTlsPreferredAsync(ip, httpPort, tlsPort, path, rawJson, challengeSig, ts, nonce, challengeNonce);
                        string respJson = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            var dto = JsonSerializer.Deserialize<CommandResponseDto>(respJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                                   ?? new CommandResponseDto { Success = true, Message = "Executed via TLS + Challenge-Response" };
                            dto.HandshakeLatencyMs = handshakeLatency;
                            return dto;
                        }
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            return BuildAuthFailureDto(response, respJson, handshakeLatency);
                        }
                    }
                    catch { }
                }
            }

            try
            {
                long ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string nonce = Guid.NewGuid().ToString("N");
                req.Timestamp = ts;
                req.Nonce = nonce;
                req.ChallengeNonce = null;
                req.ChallengeSignature = null;

                string json = JsonSerializer.Serialize(req);
                string signature = RemoteAgentService.ComputeHmacSha256(json, ClusterSecretKey);

                var response = await PostWithTlsPreferredAsync(ip, httpPort, tlsPort, path, json, signature, ts, nonce, null);
                string respJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var successDto = JsonSerializer.Deserialize<CommandResponseDto>(respJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                           ?? new CommandResponseDto { Success = true, Message = "Command executed successfully" };
                    successDto.HandshakeLatencyMs = handshakeLatency;
                    return successDto;
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized && !forceChallengeHandshake)
                {
                    return await SendSignedCommandAsync(ip, httpPort, tlsPort, path, req, forceChallengeHandshake: true);
                }

                if (isLocal)
                {
                    var localFallback = await TryLocalDirectExecutionAsync(path, req, handshakeLatency);
                    if (localFallback != null) return localFallback;
                }

                return BuildAuthFailureDto(response, respJson, handshakeLatency);
            }
            catch (Exception ex)
            {
                if (isLocal)
                {
                    var localFallback = await TryLocalDirectExecutionAsync(path, req, handshakeLatency);
                    if (localFallback != null) return localFallback;
                }

                return new CommandResponseDto { Success = false, Message = $"Connection Error: {ex.Message} ({ip}:{tlsPort})", HandshakeLatencyMs = handshakeLatency };
            }
        }

        private async Task<HttpResponseMessage> PostWithTlsPreferredAsync(
            string ip, int httpPort, int tlsPort, string path, string jsonBody,
            string signature, long ts, string nonce, string? challengeNonce)
        {
            HttpRequestMessage BuildMsg(string scheme, int port)
            {
                var msg = new HttpRequestMessage(HttpMethod.Post, $"{scheme}://{ip}:{port}{path}")
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };
                msg.Headers.Add("X-WinPurify-Signature", signature);
                msg.Headers.Add("X-WinPurify-Timestamp", ts.ToString());
                msg.Headers.Add("X-WinPurify-Nonce", nonce);
                if (!string.IsNullOrEmpty(challengeNonce))
                {
                    msg.Headers.Add("X-WinPurify-Challenge-Nonce", challengeNonce);
                    msg.Headers.Add("X-WinPurify-Challenge-Signature", signature);
                }
                return msg;
            }

            try
            {
                using var tlsMsg = BuildMsg("https", tlsPort);
                return await _tlsHttpClient.SendAsync(tlsMsg);
            }
            catch when (AllowLegacyHttpFallback)
            {
                using var httpMsg = BuildMsg("http", httpPort);
                return await _httpClient.SendAsync(httpMsg);
            }
        }

        private static CommandResponseDto BuildAuthFailureDto(HttpResponseMessage response, string respJson, long handshakeLatency)
        {
            var failDto = new CommandResponseDto
            {
                Success = false,
                Message = $"HTTP {(int)response.StatusCode}: {respJson}",
                HandshakeLatencyMs = handshakeLatency
            };
            if (response.Headers.TryGetValues("X-WinPurify-Auth-Error-Point", out var errPoints)) failDto.AuthErrorPoint = errPoints.FirstOrDefault();
            if (response.Headers.TryGetValues("X-WinPurify-AuthCategory", out var categories)) failDto.AuthCategory = categories.FirstOrDefault();
            return failDto;
        }

        private static async Task<CommandResponseDto?> TryLocalDirectExecutionAsync(string path, BatchCommandRequest req, long handshakeLatency)
        {
            try
            {
                var directResp = await RemoteAgentService.Instance.ExecuteBatchCommandAsync(path, req);
                directResp.Message = $"[로컬 엔진 직접 실행] {directResp.Message}";
                directResp.HandshakeLatencyMs = handshakeLatency;
                directResp.TransportProtocol = "Local In-Process";
                return directResp;
            }
            catch
            {
                if (string.Equals(req.CommandType, "Reboot", StringComparison.OrdinalIgnoreCase))
                {
                    SystemPowerService.ScheduleForceReboot(3);
                    return new CommandResponseDto { Success = true, Message = "[로컬 비상 직접 실행] 3초 후 4-Tier 시스템 강제 재부팅(/r /f) 시작", HandshakeLatencyMs = handshakeLatency };
                }
                if (string.Equals(req.CommandType, "Shutdown", StringComparison.OrdinalIgnoreCase))
                {
                    SystemPowerService.ScheduleForceShutdown(3);
                    return new CommandResponseDto { Success = true, Message = "[로컬 비상 직접 실행] 3초 후 4-Tier 시스템 강제 종료(/s /f) 시작", HandshakeLatencyMs = handshakeLatency };
                }
                return null;
            }
        }
    }
}
