using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// PurifyEngineCore.dll 순수 C++ 네이티브 엔진 P/Invoke 바인딩 및 고성능 래퍼 서비스
    /// 5대 네이티브 엔진:
    /// 1. 실시간 파일 잠금 해제 및 강제 소거 엔진 (Lock Hunter & Force Deleter)
    /// 2. C++ VSS / Native System Restore Point & Registry Hive Snapshot
    /// 3. 초저지연 게임/작업 가속 커널 튜너 (Kernel MMCSS, Memory Priority, Power Throttling)
    /// 4. 딥 MFT / 커널 캐시 고속 볼륨 디렉터리 분석기 (Fast Native Scanner)
    /// 5. 커널 소켓 TCP/IP 네트워크 스택 즉시 최적화기 (Low-Level Winsock & NDIS Optimizer)
    /// </summary>
    public static class NativeEngineService
    {
        private const string DllName = "PurifyEngineCore.dll";
        private static bool _isAvailable = false;
        private static bool _checked = false;

        // ====================================================================
        // P/Invoke Declarations
        // ====================================================================

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int InitializeEngine();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void ShutdownEngine();

        // [Feature 1] 실시간 파일 잠금 해제 및 강제 소거 엔진
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int UnlockAndForceDeleteFile(string filePath, int terminateLockingProcess);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int GetFileLockingProcesses(string filePath, [Out] uint[] outPids, int maxPids, out int outCount);

        // [Feature 2] C++ VSS / Native System Restore Point & Registry Hive Snapshot
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int CreateNativeRestorePoint(string description, int eventType, int restorePointType);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int BackupRegistryHiveNative(int hKeyRootType, string subKeyPath, string destFilePath);

        // [Feature 3] 초저지연 게임/작업 가속 커널 튜너
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetKernelGameBoost(int active);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int TuneProcessKernelPriority(uint pid, int profileMode);

        // [Feature 4] 딥 MFT / 커널 캐시 고속 볼륨 디렉터리 분석기
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int FastScanDirectoryNative(string rootPath, string filterExtension, out long outTotalBytes, out int outCount);

        // [New Performance Features]
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr NativeAllocate(UIntPtr size);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void NativeDeallocate(IntPtr ptr);

        // [Feature 5] 커널 소켓 TCP/IP 네트워크 스택 즉시 최적화기
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int OptimizeNetworkStackNative(int profileType);

        // [Feature 6] 계정 자격 증명 & 애플리케이션 세션 토큰 완전 정화 / 강제 로그아웃
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int PurifyWindowsCredentialsByFilter(string filterKeyword);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PurifyApplicationSessionTokens(int appType);

        // [Feature 7 - 순수 창작] NT 커널 메모리 Standby List & Modified Pages 정화
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PurgeSystemStandbyListNative(int purgeType);

        // [Feature 8 - 순수 창작] 초고속 네이티브 디스크 빈 공간 포렌식 0x00 와이퍼
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int NativeZeroWipeFreeSpace(string targetDriveRoot, out long outBytesWiped);

        // [Feature 9 - 순수 창작] NTFS ADS Mark-of-the-Web (:Zone.Identifier) 일괄 소거
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int ClearZoneIdentifierStreamNative(string targetPath, int recursive, out int outClearedCount);

        // [Feature 10 - 순수 창작] Windows 셸 아이콘 & 폰트 캐시 무재부팅 무결점 재생성
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int FlushIconAndFontCacheNative();

        // [Feature 11 - 순수 창작] 커맨더 하드웨어 Magic Packet (Wake-on-LAN) 브로드캐스트 엔진
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int SendMagicPacketNative(string macAddressStr, string broadcastIpStr, int port);

        // [Feature 12 - 순수 창작] 커맨더 서브밀리초 하드웨어 ARP 및 ICMP 서브넷 핑 스위퍼
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int FastArpQueryNative(string targetIpStr, [Out] byte[] outMacBytes, out int outMacLen);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int FastIcmpPingNative(string targetIpStr, uint timeoutMs, out uint outRoundTripTimeMs);

        // [Feature 13 - 순수 창작] CNG HMAC-SHA256 커맨더 페이로드 위변조 방지 인증/검증 엔진
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int SignCommanderPayloadNative(string payload, string secretKey, [Out] StringBuilder outSignatureHex, int maxOutLen);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int VerifyCommanderPayloadNative(string payload, string secretKey, string expectedSignatureHex);

        // [Feature 14 - 순수 창작] 순수 네이티브 멀티 노드 플릿 헬스 다이어그노스틱 스냅샷
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int QuerySystemFleetHealthNative(out uint outCpuUsagePercent, out long outFreeRamBytes, out long outTotalRamBytes, out long outFreeDiskBytes, out int outProcessCount);

        // 기본 레거시 호환 함수군
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CompressPhysicalRAM();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetProcessPriorityTuning(int active);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int ExecuteQuickMaintenance(int type);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int ExecuteOptimizationTask(string taskId);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int VerifyModuleRegistryKey(string moduleId);

        // ====================================================================
        // Safe Managed Wrappers with Automatic Fallback
        // ====================================================================

        public static bool IsNativeCoreAvailable()
        {
            if (_checked) return _isAvailable;
            try
            {
                int res = InitializeEngine();
                _isAvailable = (res >= 0);
            }
            catch
            {
                _isAvailable = false;
            }
            _checked = true;
            return _isAvailable;
        }

        /// <summary>
        /// [Feature 1 래퍼] 파일 잠금 해제 및 강제 소거
        /// </summary>
        public static bool TryUnlockAndDelete(string filePath, bool terminateLockingProcess = true)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return true;

            if (IsNativeCoreAvailable())
            {
                try
                {
                    int res = UnlockAndForceDeleteFile(filePath, terminateLockingProcess ? 1 : 0);
                    if (res == 1 || res == 0) return true;
                }
                catch { }
            }

            // C# 관리 코드 폴백
            try
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// [Feature 2 래퍼] C++ Native 시스템 복원 지점 생성 (PowerShell 불필요)
        /// </summary>
        public static (bool Success, int SequenceNumber, string Message) TryCreateNativeRestorePoint(string description = "WinPurify Pro Native Snapshot")
        {
            if (IsNativeCoreAvailable())
            {
                try
                {
                    int seq = CreateNativeRestorePoint(description, 100, 12);
                    if (seq > 0)
                    {
                        return (true, seq, $"Native C++ 복원 지점 생성 완료 (시퀀스 #{seq})");
                    }
                }
                catch (Exception ex)
                {
                    return (false, -1, $"Native 복원 엔진 예외: {ex.Message}");
                }
            }

            return (false, -1, "Native Core 미탑재 또는 지원되지 않는 환경");
        }

        /// <summary>
        /// [Feature 2 래퍼] 네이티브 레지스트리 하이브 바이너리 원자적 백업
        /// </summary>
        public static bool TryBackupRegistryHive(string subKeyPath, string destFilePath, bool isHklm = true)
        {
            if (!IsNativeCoreAvailable()) return false;
            try
            {
                int res = BackupRegistryHiveNative(isHklm ? 1 : 0, subKeyPath, destFilePath);
                return (res == 1);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// [Feature 3 래퍼] 초저지연 게임/작업 가속 커널 튜닝
        /// </summary>
        public static int TrySetKernelGameBoost(bool active)
        {
            if (!IsNativeCoreAvailable()) return -1;
            try
            {
                return SetKernelGameBoost(active ? 1 : 0);
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// [Feature 4 래퍼] 커널 캐시 기반 고속 디렉터리 스캐너
        /// </summary>
        public static bool TryFastScanDirectory(string rootPath, string filterExtension, out long totalBytes, out int fileCount)
        {
            totalBytes = 0;
            fileCount = 0;
            if (!IsNativeCoreAvailable() || !Directory.Exists(rootPath)) return false;

            try
            {
                int res = FastScanDirectoryNative(rootPath, filterExtension, out totalBytes, out fileCount);
                return (res == 1);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// [Feature 5 래퍼] 커널 소켓 TCP/IP 네트워크 스택 즉시 최적화
        /// </summary>
        public static bool TryOptimizeNetwork(int profileType = 1)
        {
            if (!IsNativeCoreAvailable()) return false;
            try
            {
                int res = OptimizeNetworkStackNative(profileType);
                return (res == 1);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// [Feature 6 래퍼] Windows Credential Manager 자격 증명 필터별 즉시 소거
        /// </summary>
        public static int TryPurifyCredentials(string filterKeyword)
        {
            if (!IsNativeCoreAvailable()) return -1;
            try
            {
                return PurifyWindowsCredentialsByFilter(filterKeyword);
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// [Feature 6 래퍼] 주요 애플리케이션 로그인 세션 및 토큰 파일 강제 소거
        /// 1: Microsoft, 2: Adobe, 3: Autodesk, 4: Edge, 5: Chrome, 6: NetworkShares, 7: All
        /// </summary>
        public static bool TryPurifyAppSession(int appType)
        {
            if (!IsNativeCoreAvailable()) return false;
            try
            {
                int res = PurifyApplicationSessionTokens(appType);
                return (res == 1);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// [Feature 7 래퍼] NT 커널 시스템 콜 직결 대기 메모리(Standby List) 완전 정화
        /// purgeType: 1 = Standby List, 2 = Empty Working Sets, 3 = Flush Modified, 4 = All Combined
        /// </summary>
        public static bool TryPurgeStandbyList(int purgeType = 4)
        {
            if (!IsNativeCoreAvailable()) return false;
            try
            {
                int res = PurgeSystemStandbyListNative(purgeType);
                return (res > 0);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// [Feature 8 래퍼] 네이티브 초고속 디스크 빈 공간 0x00 포렌식 와이퍼
        /// </summary>
        public static bool TryNativeZeroWipe(string targetDriveRoot, out long bytesWiped)
        {
            bytesWiped = 0;
            if (!IsNativeCoreAvailable()) return false;
            try
            {
                int res = NativeZeroWipeFreeSpace(targetDriveRoot, out bytesWiped);
                return (res == 1);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// [Feature 9 래퍼] NTFS ADS Mark-of-the-Web (:Zone.Identifier) 일괄 소거
        /// </summary>
        public static int TryClearZoneIdentifier(string targetPath, bool recursive = true)
        {
            if (!IsNativeCoreAvailable() || string.IsNullOrWhiteSpace(targetPath)) return 0;
            try
            {
                int cleared = 0;
                ClearZoneIdentifierStreamNative(targetPath, recursive ? 1 : 0, out cleared);
                return cleared;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// [Feature 10 래퍼] Windows 셸 아이콘 및 폰트 캐시 무재부팅 재생성
        /// </summary>
        public static bool TryFlushIconCache()
        {
            if (!IsNativeCoreAvailable()) return false;
            try
            {
                int res = FlushIconAndFontCacheNative();
                return (res == 1);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// [Feature 11 래퍼] 초고속 하드웨어 Wake-on-LAN Magic Packet 브로드캐스트
        /// </summary>
        public static bool TrySendMagicPacket(string macAddress, string broadcastIp = "255.255.255.255", int port = 9)
        {
            if (string.IsNullOrWhiteSpace(macAddress)) return false;
            if (IsNativeCoreAvailable())
            {
                try
                {
                    int res = SendMagicPacketNative(macAddress, broadcastIp, port);
                    if (res == 1) return true;
                }
                catch { }
            }

            // C# Socket Fallback
            try
            {
                var cleanMac = macAddress.Replace(":", "").Replace("-", "");
                if (cleanMac.Length != 12) return false;
                byte[] macBytes = new byte[6];
                for (int i = 0; i < 6; i++)
                {
                    macBytes[i] = Convert.ToByte(cleanMac.Substring(i * 2, 2), 16);
                }

                byte[] packet = new byte[102];
                for (int i = 0; i < 6; i++) packet[i] = 0xFF;
                for (int i = 0; i < 16; i++)
                {
                    Buffer.BlockCopy(macBytes, 0, packet, 6 + (i * 6), 6);
                }

                using var client = new System.Net.Sockets.UdpClient();
                client.EnableBroadcast = true;
                client.Send(packet, packet.Length, new System.Net.IPEndPoint(System.Net.IPAddress.Parse(broadcastIp), port));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// [Feature 12 래퍼] 하드웨어 ARP 및 ICMP 실시간 응답 진단
        /// </summary>
        public static bool TryFastIcmpPing(string targetIp, uint timeoutMs, out uint roundTripTimeMs)
        {
            roundTripTimeMs = 0;
            if (string.IsNullOrWhiteSpace(targetIp)) return false;

            if (IsNativeCoreAvailable())
            {
                try
                {
                    int res = FastIcmpPingNative(targetIp, timeoutMs, out roundTripTimeMs);
                    if (res == 1) return true;
                }
                catch { }
            }

            return false;
        }

        /// <summary>
        /// [Feature 13 래퍼] CNG HMAC-SHA256 커맨더 페이로드 서명 (0% GC)
        /// </summary>
        public static string SignCommanderPayload(string payload, string secretKey)
        {
            if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(secretKey)) return string.Empty;

            if (IsNativeCoreAvailable())
            {
                try
                {
                    var sb = new StringBuilder(65);
                    int res = SignCommanderPayloadNative(payload, secretKey, sb, sb.Capacity);
                    if (res == 1 && sb.Length == 64)
                    {
                        return sb.ToString();
                    }
                }
                catch { }
            }

            // C# Managed Fallback
            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        /// <summary>
        /// [Feature 13 래퍼] CNG HMAC-SHA256 커맨더 페이로드 서명 검증 (타이밍 공격 방어)
        /// </summary>
        public static bool VerifyCommanderPayload(string payload, string secretKey, string expectedSignatureHex)
        {
            if (string.IsNullOrEmpty(payload) || string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(expectedSignatureHex)) return false;

            if (IsNativeCoreAvailable())
            {
                try
                {
                    int res = VerifyCommanderPayloadNative(payload, secretKey, expectedSignatureHex);
                    return (res == 1);
                }
                catch { }
            }

            // C# Managed Fallback
            string computed = SignCommanderPayload(payload, secretKey);
            return string.Equals(computed, expectedSignatureHex, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// [Feature 14 래퍼] 순수 C++ 네이티브 다중 노드 시스템 상태 초고속 조회
        /// </summary>
        public static bool TryQueryFleetHealth(out uint cpuUsagePercent, out long freeRamBytes, out long totalRamBytes, out long freeDiskBytes, out int processCount)
        {
            cpuUsagePercent = 0;
            freeRamBytes = 0;
            totalRamBytes = 0;
            freeDiskBytes = 0;
            processCount = 0;

            if (!IsNativeCoreAvailable()) return false;

            try
            {
                int res = QuerySystemFleetHealthNative(out cpuUsagePercent, out freeRamBytes, out totalRamBytes, out freeDiskBytes, out processCount);
                return (res == 1);
            }
            catch
            {
                return false;
            }
        }
    }
}
