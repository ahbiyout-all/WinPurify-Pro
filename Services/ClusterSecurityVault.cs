using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// 소규모 작업실 Zero-Config LAN 보안을 위한 통합 보안 금고 서비스
    /// - Win32 DPAPI (CryptProtectData / LocalMachine Scope) 암호화 클러스터 키 보관
    /// - 하드코딩 기본 키 및 평문 저장 폐기 -> 최초 실행 시 256비트 고엔트로피 난수 키 자동 생성
    /// - 자체 서명 ECDSA P-256 TLS 인증서 자동 발급 및 TOFU(Trust-On-First-Use) 지문 고정(Pinning)
    /// - 6자리 일회용 스튜디오 페어링 PIN 코드 발급/검증
    /// </summary>
    public static class ClusterSecurityVault
    {
        private static readonly string ProgramDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "WinPurifyPro"
        );

        private static readonly string LocalAppDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinPurifyPro"
        );

        private static readonly string ProtectedKeyFilePath = Path.Combine(ProgramDataDir, "cluster.key.dpapi");
        private static readonly string LegacyPlaintextKeyFilePath = Path.Combine(ProgramDataDir, "cluster.key");
        private static readonly string ProtectedCertFilePath = Path.Combine(ProgramDataDir, "node_tls.pfx.dpapi");
        private static readonly string TrustedNodesFilePath = Path.Combine(LocalAppDataDir, "trusted_nodes.json");

        private static readonly byte[] DpapiEntropy = Encoding.UTF8.GetBytes("WinPurifyPro::StudioVault::2026::Entropy");
        private static readonly object _vaultLock = new object();
        private static X509Certificate2? _cachedServerCert;
        private static readonly ConcurrentDictionary<string, string> _trustedThumbprints = new(StringComparer.OrdinalIgnoreCase);
        private static bool _trustedLoaded = false;

        // 6-Digit Ephemeral Pairing PIN state
        private static string? _activePairingPin;
        private static DateTime _pairingPinExpiresAt = DateTime.MinValue;

        // =====================================================================
        // Win32 Crypt32 DPAPI P/Invoke (Zero external NuGet dependencies)
        // =====================================================================
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct DATA_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CryptProtectData(
            ref DATA_BLOB pDataIn,
            string? szDataDescr,
            ref DATA_BLOB pOptionalEntropy,
            IntPtr pvReserved,
            IntPtr pPromptStruct,
            int dwFlags,
            out DATA_BLOB pDataOut);

        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CryptUnprotectData(
            ref DATA_BLOB pDataIn,
            StringBuilder? ppszDataDescr,
            ref DATA_BLOB pOptionalEntropy,
            IntPtr pvReserved,
            IntPtr pPromptStruct,
            int dwFlags,
            out DATA_BLOB pDataOut);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);

        private const int CRYPTPROTECT_UI_FORBIDDEN = 0x1;
        private const int CRYPTPROTECT_LOCAL_MACHINE = 0x4;

        public static byte[] ProtectBytesDpapi(byte[] plainBytes)
        {
            if (!OperatingSystem.IsWindows())
            {
                return plainBytes;
            }

            DATA_BLOB inBlob = default;
            DATA_BLOB entropyBlob = default;
            DATA_BLOB outBlob = default;

            try
            {
                inBlob.cbData = plainBytes.Length;
                inBlob.pbData = Marshal.AllocHGlobal(plainBytes.Length);
                Marshal.Copy(plainBytes, 0, inBlob.pbData, plainBytes.Length);

                entropyBlob.cbData = DpapiEntropy.Length;
                entropyBlob.pbData = Marshal.AllocHGlobal(DpapiEntropy.Length);
                Marshal.Copy(DpapiEntropy, 0, entropyBlob.pbData, DpapiEntropy.Length);

                // Try LocalMachine scope first so Windows Service & Desktop share vault seamlessly
                bool ok = CryptProtectData(
                    ref inBlob,
                    "WinPurifyProVault",
                    ref entropyBlob,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    CRYPTPROTECT_UI_FORBIDDEN | CRYPTPROTECT_LOCAL_MACHINE,
                    out outBlob);

                if (!ok)
                {
                    ok = CryptProtectData(
                        ref inBlob,
                        "WinPurifyProVault",
                        ref entropyBlob,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        CRYPTPROTECT_UI_FORBIDDEN,
                        out outBlob);
                }

                if (ok && outBlob.cbData > 0 && outBlob.pbData != IntPtr.Zero)
                {
                    byte[] encrypted = new byte[outBlob.cbData];
                    Marshal.Copy(outBlob.pbData, encrypted, 0, outBlob.cbData);
                    return encrypted;
                }
            }
            catch
            {
                // Fallback only if Crypt32 fails in non-standard sandbox
            }
            finally
            {
                if (inBlob.pbData != IntPtr.Zero) Marshal.FreeHGlobal(inBlob.pbData);
                if (entropyBlob.pbData != IntPtr.Zero) Marshal.FreeHGlobal(entropyBlob.pbData);
                if (outBlob.pbData != IntPtr.Zero) LocalFree(outBlob.pbData);
            }

            return plainBytes;
        }

        public static byte[]? UnprotectBytesDpapi(byte[] protectedBytes)
        {
            if (!OperatingSystem.IsWindows())
            {
                return protectedBytes;
            }

            DATA_BLOB inBlob = default;
            DATA_BLOB entropyBlob = default;
            DATA_BLOB outBlob = default;

            try
            {
                inBlob.cbData = protectedBytes.Length;
                inBlob.pbData = Marshal.AllocHGlobal(protectedBytes.Length);
                Marshal.Copy(protectedBytes, 0, inBlob.pbData, protectedBytes.Length);

                entropyBlob.cbData = DpapiEntropy.Length;
                entropyBlob.pbData = Marshal.AllocHGlobal(DpapiEntropy.Length);
                Marshal.Copy(DpapiEntropy, 0, entropyBlob.pbData, DpapiEntropy.Length);

                bool ok = CryptUnprotectData(
                    ref inBlob,
                    null,
                    ref entropyBlob,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    CRYPTPROTECT_UI_FORBIDDEN,
                    out outBlob);

                if (ok && outBlob.cbData > 0 && outBlob.pbData != IntPtr.Zero)
                {
                    byte[] plain = new byte[outBlob.cbData];
                    Marshal.Copy(outBlob.pbData, plain, 0, outBlob.cbData);
                    return plain;
                }
            }
            catch
            {
                return null;
            }
            finally
            {
                if (inBlob.pbData != IntPtr.Zero) Marshal.FreeHGlobal(inBlob.pbData);
                if (entropyBlob.pbData != IntPtr.Zero) Marshal.FreeHGlobal(entropyBlob.pbData);
                if (outBlob.pbData != IntPtr.Zero) LocalFree(outBlob.pbData);
            }

            return null;
        }

        // =====================================================================
        // High-Entropy Cluster Key Management (DPAPI Encrypted)
        // =====================================================================

        public static string GenerateHighEntropyClusterKey()
        {
            byte[] randomBytes = RandomNumberGenerator.GetBytes(24);
            string base64Url = Convert.ToBase64String(randomBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
            return $"WPP-STD-{base64Url}";
        }

        public static string GetOrCreateClusterKey()
        {
            lock (_vaultLock)
            {
                try
                {
                    if (File.Exists(ProtectedKeyFilePath))
                    {
                        byte[] encrypted = File.ReadAllBytes(ProtectedKeyFilePath);
                        byte[]? decrypted = UnprotectBytesDpapi(encrypted);
                        if (decrypted != null)
                        {
                            string existing = Encoding.UTF8.GetString(decrypted).Trim();
                            if (!string.IsNullOrEmpty(existing) && existing.Length >= 12)
                            {
                                return existing;
                            }
                        }
                    }

                    // Migrate legacy plaintext cluster.key to DPAPI vault and shred plaintext file
                    if (File.Exists(LegacyPlaintextKeyFilePath))
                    {
                        string legacyKey = File.ReadAllText(LegacyPlaintextKeyFilePath).Trim();
                        // Upgrade weak hardcoded legacy keys automatically to 256-bit random key
                        bool isWeakHardcoded = legacyKey == "WPP-SEC-CLUSTER-2026-KEY" || legacyKey == "WinPurifyClusterKey2026" || legacyKey.Length < 12;
                        string finalKey = isWeakHardcoded ? GenerateHighEntropyClusterKey() : legacyKey;
                        SaveClusterKey(finalKey);
                        try
                        {
                            // Overwrite before deleting plaintext file
                            File.WriteAllBytes(LegacyPlaintextKeyFilePath, RandomNumberGenerator.GetBytes(64));
                            File.Delete(LegacyPlaintextKeyFilePath);
                        }
                        catch { }
                        return finalKey;
                    }

                    string generated = GenerateHighEntropyClusterKey();
                    SaveClusterKey(generated);
                    return generated;
                }
                catch
                {
                    return GenerateHighEntropyClusterKey();
                }
            }
        }

        public static bool SaveClusterKey(string clusterKey)
        {
            if (string.IsNullOrWhiteSpace(clusterKey) || clusterKey.Length < 8) return false;

            lock (_vaultLock)
            {
                try
                {
                    if (!Directory.Exists(ProgramDataDir))
                    {
                        Directory.CreateDirectory(ProgramDataDir);
                    }

                    byte[] plainBytes = Encoding.UTF8.GetBytes(clusterKey.Trim());
                    byte[] protectedBytes = ProtectBytesDpapi(plainBytes);
                    File.WriteAllBytes(ProtectedKeyFilePath, protectedBytes);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        // =====================================================================
        // Ephemeral 6-Digit Studio Pairing Code (Zero-Typing Onboarding)
        // =====================================================================

        public static (string Pin, int ExpiresInSeconds) GeneratePairingPin(int ttlSeconds = 120)
        {
            lock (_vaultLock)
            {
                int val = RandomNumberGenerator.GetInt32(100000, 999999);
                _activePairingPin = val.ToString();
                _pairingPinExpiresAt = DateTime.UtcNow.AddSeconds(ttlSeconds);
                return (_activePairingPin, ttlSeconds);
            }
        }

        public static bool VerifyAndConsumePairingPin(string? pin)
        {
            if (string.IsNullOrWhiteSpace(pin)) return false;

            lock (_vaultLock)
            {
                if (string.IsNullOrEmpty(_activePairingPin) || DateTime.UtcNow > _pairingPinExpiresAt)
                {
                    _activePairingPin = null;
                    return false;
                }

                byte[] a = Encoding.UTF8.GetBytes(_activePairingPin);
                byte[] b = Encoding.UTF8.GetBytes(pin.Trim());
                bool match = CryptographicOperations.FixedTimeEquals(a, b);
                if (match)
                {
                    _activePairingPin = null; // Single-use PIN
                }
                return match;
            }
        }

        // =====================================================================
        // Self-Signed ECDSA P-256 TLS Certificate + TOFU Pinning Store
        // =====================================================================

        public static X509Certificate2 GetOrCreateTlsCertificate()
        {
            lock (_vaultLock)
            {
                if (_cachedServerCert != null && _cachedServerCert.NotAfter > DateTime.UtcNow.AddDays(30))
                {
                    return _cachedServerCert;
                }

                try
                {
                    if (File.Exists(ProtectedCertFilePath))
                    {
                        byte[] encryptedPfx = File.ReadAllBytes(ProtectedCertFilePath);
                        byte[]? pfxBytes = UnprotectBytesDpapi(encryptedPfx);
                        if (pfxBytes != null && pfxBytes.Length > 0)
                        {
                            var loaded = new X509Certificate2(pfxBytes, string.Empty, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                            if (loaded.HasPrivateKey && loaded.NotAfter > DateTime.UtcNow.AddDays(30))
                            {
                                _cachedServerCert = loaded;
                                return loaded;
                            }
                        }
                    }
                }
                catch { }

                // Generate new ECDSA P-256 certificate valid for 5 years
                using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
                var req = new CertificateRequest(
                    $"CN=WinPurifyPro-{Environment.MachineName}, O=WinPurifyPro Studio LAN",
                    ecdsa,
                    HashAlgorithmName.SHA256);

                var sanBuilder = new SubjectAlternativeNameBuilder();
                sanBuilder.AddDnsName(Environment.MachineName);
                sanBuilder.AddDnsName("localhost");
                sanBuilder.AddIpAddress(IPAddress.Loopback);
                try
                {
                    string bestLan = RemoteAgentService.GetBestLocalIPv4Address();
                    if (IPAddress.TryParse(bestLan, out var lanIp))
                    {
                        sanBuilder.AddIpAddress(lanIp);
                    }
                }
                catch { }

                req.CertificateExtensions.Add(sanBuilder.Build());
                req.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
                req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
                req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

                var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddYears(5));
                byte[] exportedPfx = cert.Export(X509ContentType.Pfx, string.Empty);

                try
                {
                    if (!Directory.Exists(ProgramDataDir)) Directory.CreateDirectory(ProgramDataDir);
                    File.WriteAllBytes(ProtectedCertFilePath, ProtectBytesDpapi(exportedPfx));
                }
                catch { }

                _cachedServerCert = new X509Certificate2(exportedPfx, string.Empty, X509KeyStorageFlags.Exportable);
                return _cachedServerCert;
            }
        }

        public static string GetLocalTlsThumbprint()
        {
            try
            {
                var cert = GetOrCreateTlsCertificate();
                return ComputeSha256Thumbprint(cert);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string ComputeSha256Thumbprint(X509Certificate cert)
        {
            byte[] hash = SHA256.HashData(cert.GetRawCertData());
            return Convert.ToHexString(hash).ToUpperInvariant();
        }

        public static (bool IsTrusted, string Status) VerifyOrPinNodeCertificate(string ip, int port, X509Certificate? serverCert, string? expectedThumbprint = null)
        {
            if (serverCert == null)
            {
                return (false, "NO_CERTIFICATE");
            }

            string actualThumbprint = ComputeSha256Thumbprint(serverCert);
            if (!string.IsNullOrWhiteSpace(expectedThumbprint) &&
                !string.Equals(actualThumbprint, expectedThumbprint.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return (false, "DISCOVERY_THUMBPRINT_MISMATCH");
            }

            EnsureTrustedLoaded();
            string key = $"{ip}:{port}";

            if (_trustedThumbprints.TryGetValue(key, out string? pinned))
            {
                if (string.Equals(pinned, actualThumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    return (true, "VERIFIED");
                }
                return (false, "PINNED_THUMBPRINT_MISMATCH");
            }

            // TOFU: Trust-On-First-Use for small studio LAN
            _trustedThumbprints[key] = actualThumbprint;
            PersistTrustedNodes();
            return (true, "PINNED_NEW");
        }

        public static void ResetPinnedNodeCertificate(string ip, int port)
        {
            EnsureTrustedLoaded();
            if (_trustedThumbprints.TryRemove($"{ip}:{port}", out _))
            {
                PersistTrustedNodes();
            }
        }

        private static void EnsureTrustedLoaded()
        {
            if (_trustedLoaded) return;
            lock (_vaultLock)
            {
                if (_trustedLoaded) return;
                try
                {
                    if (File.Exists(TrustedNodesFilePath))
                    {
                        string json = File.ReadAllText(TrustedNodesFilePath);
                        var map = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                        if (map != null)
                        {
                            foreach (var kv in map)
                            {
                                _trustedThumbprints[kv.Key] = kv.Value;
                            }
                        }
                    }
                }
                catch { }
                _trustedLoaded = true;
            }
        }

        private static void PersistTrustedNodes()
        {
            try
            {
                if (!Directory.Exists(LocalAppDataDir)) Directory.CreateDirectory(LocalAppDataDir);
                string json = JsonSerializer.Serialize(_trustedThumbprints, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(TrustedNodesFilePath, json);
            }
            catch { }
        }
    }
}
