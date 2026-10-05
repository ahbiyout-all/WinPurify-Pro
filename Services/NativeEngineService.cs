using System;
using System.IO;
using System.Runtime.InteropServices;

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

        // [Feature 5] 커널 소켓 TCP/IP 네트워크 스택 즉시 최적화기
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int OptimizeNetworkStackNative(int profileType);

        // [Feature 6] 계정 자격 증명 & 애플리케이션 세션 토큰 완전 정화 / 강제 로그아웃
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int PurifyWindowsCredentialsByFilter(string filterKeyword);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PurifyApplicationSessionTokens(int appType);

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
    }
}
