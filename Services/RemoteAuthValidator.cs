using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WinPurifyPro.Models;

namespace WinPurifyPro.Services
{
    public class AuthValidationResult
    {
        public bool IsValid { get; set; }
        public string FailureCategory { get; set; } = "Unknown";
        public string ErrorPoint { get; set; } = "unknown_error";
        public string FailureReason { get; set; } = string.Empty;
        public string ExecutionDetails { get; set; } = string.Empty;
        public long NodeTimestamp { get; set; }
        public long SenderTimestamp { get; set; }
        public long ClockOffsetSeconds { get; set; }
        public string AuthScheme { get; set; } = "None";
    }

    /// <summary>
    /// 원격 관제 HMAC-SHA256 인증, Challenge-Response, Anti-Replay 및 시계 오차 자동 보정 검증기.
    /// - 평문 JSON SecretKey 폴백 및 하드코딩 레거시 키 전면 거부.
    /// - 단일 로컬 작업실 환경에서 시계 오차를 자동 학습하고 Nonce 재전송 공격을 차단합니다.
    /// </summary>
    public static class RemoteAuthValidator
    {
        private static readonly ConcurrentDictionary<string, DateTime> _seenNonces = new();
        private static readonly ConcurrentDictionary<string, long> _clientClockOffsets = new();
        private static readonly ConcurrentDictionary<string, (DateTime ExpiresAt, string ClientIp)> _activeChallenges = new();
        private static DateTime _lastNonceCleanup = DateTime.UtcNow;
        private static DateTime _lastChallengeCleanup = DateTime.UtcNow;
        private static readonly object _cleanupLock = new object();

        public static AuthChallengeResponseDto IssueChallenge(string clientIp)
        {
            string challengeNonce = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            _activeChallenges[challengeNonce] = (DateTime.UtcNow.AddSeconds(60), clientIp);
            CleanupExpiredChallenges();

            return new AuthChallengeResponseDto
            {
                Success = true,
                ChallengeNonce = challengeNonce,
                ExpiresInSeconds = 60,
                ServerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Message = "Challenge nonce issued successfully"
            };
        }

        public static (bool IsAuthenticated, string ErrorPoint, string Reason) VerifyTelemetryRequest(
            NameValueCollection headers,
            string? secFetchSite,
            string method,
            string path,
            string clientIp,
            string clusterSecretKey)
        {
            if (!string.IsNullOrEmpty(secFetchSite) && secFetchSite.Equals("cross-site", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "cross_site_blocked", "Cross-site telemetry request rejected");
            }

            // Localhost IPC calls from same machine are permitted seamlessly
            if (clientIp == "127.0.0.1" || clientIp == "::1")
            {
                return (true, "none", "Local loopback telemetry access granted");
            }

            string? headerSig = headers["X-WinPurify-Signature"];
            string? headerTime = headers["X-WinPurify-Timestamp"];
            string? headerNonce = headers["X-WinPurify-Nonce"];

            if (string.IsNullOrWhiteSpace(headerSig) || string.IsNullOrWhiteSpace(headerTime) || string.IsNullOrWhiteSpace(headerNonce))
            {
                return (false, "telemetry_auth_required", "Telemetry endpoint requires HMAC-SHA256 authentication headers");
            }

            if (!long.TryParse(headerTime, out long senderTs))
            {
                return (false, "invalid_timestamp", "Invalid X-WinPurify-Timestamp header");
            }

            if (headerNonce.Length < 6 || !_seenNonces.TryAdd($"tel:{headerNonce}", DateTime.UtcNow))
            {
                return (false, "nonce_replayed", "Telemetry nonce invalid or replayed");
            }
            CleanupExpiredNonces();

            long currentTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _clientClockOffsets.TryGetValue(clientIp, out long learnedOffset);
            long timeDiff = Math.Abs(currentTs - (senderTs + learnedOffset));
            if (timeDiff > 300)
            {
                return (false, "timestamp_expired", $"Telemetry timestamp expired (diff: {timeDiff}s)");
            }

            string canonical = $"{method.ToUpperInvariant()}:{path.ToLowerInvariant()}:{headerTime}:{headerNonce}";
            string expected = RemoteAgentService.ComputeHmacSha256(canonical, clusterSecretKey);

            if (FixedTimeEqualsHex(headerSig, expected))
            {
                _clientClockOffsets[clientIp] = currentTs - senderTs;
                return (true, "none", "Authenticated");
            }

            return (false, "signature_mismatch", "Telemetry HMAC signature mismatch");
        }

        public static AuthValidationResult VerifySecurityToken(
            BatchCommandRequest? req,
            string rawBody,
            NameValueCollection headers,
            string? secFetchSite,
            string clientIp,
            string clusterSecretKey,
            Action<string>? log = null)
        {
            long currentTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var result = new AuthValidationResult
            {
                NodeTimestamp = currentTs
            };

            // 0. Cross-Origin 브라우저 Drive-by 요청 원천 차단
            if (!string.IsNullOrEmpty(secFetchSite) && secFetchSite.Equals("cross-site", StringComparison.OrdinalIgnoreCase))
            {
                result.IsValid = false;
                result.FailureCategory = "CrossSite";
                result.ErrorPoint = "cross_site_blocked";
                result.FailureReason = "Security Blocked: Cross-Site Drive-by Browser Request Rejected";
                result.ExecutionDetails = "Browser Sec-Fetch-Site header indicates cross-site origin.";
                log?.Invoke($"[RemoteAgent 보안 차단] Cross-Site 요청 거부 ({clientIp})");
                return result;
            }

            if (req == null)
            {
                result.IsValid = false;
                result.FailureCategory = "InvalidPayload";
                result.ErrorPoint = "invalid_payload";
                result.FailureReason = "Security Authentication Failed: Invalid or Empty JSON Request Body Payload";
                result.ExecutionDetails = "Deserialized BatchCommandRequest was null.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(clusterSecretKey))
            {
                result.IsValid = false;
                result.FailureCategory = "VaultUnavailable";
                result.ErrorPoint = "cluster_key_unavailable";
                result.FailureReason = "Node cluster key is not initialized in DPAPI vault.";
                return result;
            }

            // 1. Challenge-Response 서버 핸드셰이크 검증 (시계 오차 무관 일회용 Nonce 서명)
            string? challengeNonce = headers["X-WinPurify-Challenge-Nonce"];
            if (string.IsNullOrWhiteSpace(challengeNonce)) challengeNonce = req.ChallengeNonce;

            string? challengeSig = headers["X-WinPurify-Challenge-Signature"];
            if (string.IsNullOrWhiteSpace(challengeSig)) challengeSig = req.ChallengeSignature;
            if (string.IsNullOrWhiteSpace(challengeSig)) challengeSig = headers["X-WinPurify-Signature"];

            if (!string.IsNullOrWhiteSpace(challengeNonce))
            {
                if (!_activeChallenges.TryRemove(challengeNonce, out var challengeInfo))
                {
                    result.IsValid = false;
                    result.FailureCategory = "ChallengeInvalidOrConsumed";
                    result.ErrorPoint = "challenge_consumed";
                    result.FailureReason = $"Security Authentication Failed: Challenge Nonce '{challengeNonce}' is invalid or already consumed.";
                    result.ExecutionDetails = $"ChallengeNonce: {challengeNonce} | Status: Expired or Replayed";
                    log?.Invoke($"[RemoteAgent 보안 401] [challenge_consumed] {clientIp} -> Challenge Nonce 검증 실패");
                    return result;
                }

                if (!string.IsNullOrWhiteSpace(challengeInfo.ClientIp) &&
                    !string.Equals(challengeInfo.ClientIp, clientIp, StringComparison.OrdinalIgnoreCase))
                {
                    result.IsValid = false;
                    result.FailureCategory = "ChallengeIpMismatch";
                    result.ErrorPoint = "challenge_ip_mismatch";
                    result.FailureReason = $"Security Authentication Failed: Challenge Nonce '{challengeNonce}' was issued to a different IP address.";
                    result.ExecutionDetails = $"IssuedTo: {challengeInfo.ClientIp} | AttemptedBy: {clientIp}";
                    log?.Invoke($"[RemoteAgent 보안 401] [challenge_ip_mismatch] {clientIp} -> 타 IP 챌린지 재사용 차단");
                    return result;
                }

                if (DateTime.UtcNow > challengeInfo.ExpiresAt)
                {
                    result.IsValid = false;
                    result.FailureCategory = "ChallengeExpired";
                    result.ErrorPoint = "challenge_expired";
                    result.FailureReason = $"Security Authentication Failed: Challenge Nonce '{challengeNonce}' has expired (TTL: 60s).";
                    result.ExecutionDetails = $"ChallengeNonce: {challengeNonce} | Status: Expired";
                    log?.Invoke($"[RemoteAgent 보안 401] [challenge_expired] {clientIp} -> Challenge Nonce 만료됨");
                    return result;
                }

                string cmdType = req.CommandType ?? string.Empty;
                string candidate1 = RemoteAgentService.ComputeHmacSha256($"{challengeNonce}:{cmdType}:{rawBody}", clusterSecretKey);
                string candidate2 = RemoteAgentService.ComputeHmacSha256($"{challengeNonce}:{rawBody}", clusterSecretKey);
                string candidate3 = RemoteAgentService.ComputeHmacSha256(challengeNonce, clusterSecretKey);

                if (FixedTimeEqualsHex(challengeSig, candidate1) ||
                    FixedTimeEqualsHex(challengeSig, candidate2) ||
                    FixedTimeEqualsHex(challengeSig, candidate3))
                {
                    result.IsValid = true;
                    result.FailureCategory = "None";
                    result.ErrorPoint = "none";
                    result.AuthScheme = "Challenge-Response HMAC-SHA256";
                    log?.Invoke($"[RemoteAgent 보안 Challenge-Response 성공] 비대칭 챌린지 핸드셰이크 검증 성공 (Client: {clientIp}, Command: {cmdType})");
                    return result;
                }

                result.IsValid = false;
                result.FailureCategory = "ChallengeSignatureMismatch";
                result.ErrorPoint = "challenge_signature_mismatch";
                result.FailureReason = $"Security Authentication Failed: Challenge Signature Mismatch for Nonce '{challengeNonce}'.";
                result.ExecutionDetails = $"ChallengeSignatureMismatch: Nonce={challengeNonce}";
                log?.Invoke($"[RemoteAgent 보안 401] [challenge_signature_mismatch] {clientIp} -> Challenge 서명 불일치");
                return result;
            }

            // 2. Nonce Replay 공격 검증
            string? headerSig = headers["X-WinPurify-Signature"];
            string? headerTime = headers["X-WinPurify-Timestamp"];
            string? headerNonce = headers["X-WinPurify-Nonce"];

            if (!string.IsNullOrEmpty(headerNonce) && headerNonce.Length >= 6)
            {
                if (!_seenNonces.TryAdd(headerNonce, DateTime.UtcNow))
                {
                    result.IsValid = false;
                    result.FailureCategory = "ReplayAttack";
                    result.ErrorPoint = "nonce_replayed";
                    result.FailureReason = $"Security Authentication Failed: Replay Attack Detected. Duplicate Nonce '{headerNonce}' was already processed.";
                    result.ExecutionDetails = $"Nonce: {headerNonce} | Duplicate Replay Blocked";
                    log?.Invoke($"[RemoteAgent 보안 401] [nonce_replayed] {clientIp} -> Nonce '{headerNonce}' 중복 감지");
                    return result;
                }
                CleanupExpiredNonces();
            }

            // 3. HMAC-SHA256 디지털 서명 및 시계 오차(Clock Skew) 검증 (평문 SecretKey 폴백 없음!)
            bool hasSignatureHeaders = !string.IsNullOrEmpty(headerSig) && !string.IsNullOrEmpty(headerTime);
            if (hasSignatureHeaders && long.TryParse(headerTime, out long senderTs))
            {
                result.SenderTimestamp = senderTs;
                long rawOffset = currentTs - senderTs;
                result.ClockOffsetSeconds = rawOffset;

                _clientClockOffsets.TryGetValue(clientIp, out long learnedOffset);
                long adjustedSenderTs = senderTs + learnedOffset;
                long timeDiff = Math.Abs(currentTs - adjustedSenderTs);

                if (timeDiff <= 300)
                {
                    string expectedSig = RemoteAgentService.ComputeHmacSha256(rawBody, clusterSecretKey);
                    if (FixedTimeEqualsHex(headerSig, expectedSig))
                    {
                        _clientClockOffsets[clientIp] = rawOffset;
                        result.IsValid = true;
                        result.FailureCategory = "None";
                        result.ErrorPoint = "none";
                        result.AuthScheme = "HMAC-SHA256";
                        log?.Invoke($"[RemoteAgent 보안 인증] HMAC-SHA256 서명 검증 성공 (Client: {clientIp}, Skew: {rawOffset}s)");
                        return result;
                    }
                }
            }

            // 4. 평문 SecretKey 전송 및 레거시 기본 키 차단 진단 메시지
            result.IsValid = false;
            if (!string.IsNullOrWhiteSpace(req.SecretKey))
            {
                result.FailureCategory = "PlaintextSecretRejected";
                result.ErrorPoint = "plaintext_secret_deprecated";
                result.FailureReason = "Security Policy Violation: Plaintext SecretKey authentication and hardcoded default cluster keys are disabled. Pair this node via TLS 6-digit PIN and sign requests with HMAC-SHA256.";
                result.ExecutionDetails = "Plaintext SecretKey fallback removed for LAN zero-trust hardening.";
                log?.Invoke($"[RemoteAgent 보안 차단] 평문 SecretKey 인증 시도 거부 ({clientIp})");
                return result;
            }

            if (hasSignatureHeaders)
            {
                long rawOffset = Math.Abs(currentTs - result.SenderTimestamp);
                if (rawOffset > 300)
                {
                    result.FailureCategory = "ClockSkew";
                    result.ErrorPoint = "timestamp_expired";
                    result.FailureReason = $"Security Authentication Failed: Clock Skew Exceeded ({rawOffset}s > 300s). Retry with Challenge-Response.";
                    result.ExecutionDetails = $"TimestampExpired: Diff={rawOffset}s | NodeTime={currentTs} | SenderTime={result.SenderTimestamp}";
                }
                else
                {
                    result.FailureCategory = "SignatureMismatch";
                    result.ErrorPoint = "signature_mismatch";
                    result.FailureReason = "Security Authentication Failed: HMAC-SHA256 Signature Mismatch. Re-pair node with 6-digit PIN if cluster key changed.";
                    result.ExecutionDetails = $"SignatureMismatch: Client={clientIp}";
                }
            }
            else
            {
                result.FailureCategory = "MissingAuth";
                result.ErrorPoint = "missing_auth";
                result.FailureReason = "Security Authentication Failed: Missing X-WinPurify-Signature or Challenge headers.";
                result.ExecutionDetails = "MissingAuth: Unsigned request rejected.";
            }

            return result;
        }

        private static bool FixedTimeEqualsHex(string? a, string? b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return false;
            byte[] ba = Encoding.UTF8.GetBytes(a.Trim().ToUpperInvariant());
            byte[] bb = Encoding.UTF8.GetBytes(b.Trim().ToUpperInvariant());
            if (ba.Length != bb.Length) return false;
            return CryptographicOperations.FixedTimeEquals(ba, bb);
        }

        private static void CleanupExpiredNonces()
        {
            if (DateTime.UtcNow - _lastNonceCleanup < TimeSpan.FromSeconds(30)) return;
            lock (_cleanupLock)
            {
                if (DateTime.UtcNow - _lastNonceCleanup < TimeSpan.FromSeconds(30)) return;
                _lastNonceCleanup = DateTime.UtcNow;

                var expiredTime = DateTime.UtcNow.AddSeconds(-120);
                var expiredKeys = _seenNonces.Where(kvp => kvp.Value < expiredTime).Select(kvp => kvp.Key).ToList();
                foreach (var k in expiredKeys) _seenNonces.TryRemove(k, out _);
            }
        }

        private static void CleanupExpiredChallenges()
        {
            if (DateTime.UtcNow - _lastChallengeCleanup < TimeSpan.FromSeconds(30)) return;
            lock (_cleanupLock)
            {
                if (DateTime.UtcNow - _lastChallengeCleanup < TimeSpan.FromSeconds(30)) return;
                _lastChallengeCleanup = DateTime.UtcNow;

                var expiredKeys = _activeChallenges.Where(kvp => DateTime.UtcNow > kvp.Value.ExpiresAt).Select(kvp => kvp.Key).ToList();
                foreach (var k in expiredKeys) _activeChallenges.TryRemove(k, out _);
            }
        }
    }
}
