using System;
using System.IO;
using Microsoft.Win32;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// 애플리케이션 실행 환경 (포터블 무설치 모드 vs 데스크탑 인스톨 서비스 모드) 감지 및 관리
    /// </summary>
    public static class AppEnvironment
    {
        public const string ServiceName = "WinPurifyAgentService";
        public const string ServiceDisplayName = "WinPurify Pro Remote Agent Service";
        public const string ServiceDescription = "WinPurify Pro 중앙 관제(Central Commander) 원격 최적화 및 유지보수 명령 수신 백그라운드 서비스";

        /// <summary>
        /// 애플리케이션 버전 (어셈블리 리플렉션 및 기본값 반환)
        /// </summary>
        public static string AppVersion
        {
            get
            {
                var ver = typeof(AppEnvironment).Assembly.GetName().Version;
                return ver != null ? $"{ver.Major}.{ver.Minor}.{ver.Build}" : "4.43.0";
            }
        }

        private static bool? _isInstalledCache;
        private static bool? _isServiceModeCache;
        private static bool? _isCommanderModeCache;

        /// <summary>
        /// 중앙 관제 콘솔 전용 구동 모드 (--commander, -commander, /commander, --fleet 인수 수신) 여부
        /// </summary>
        public static bool IsCommanderMode
        {
            get
            {
                if (!_isCommanderModeCache.HasValue)
                {
                    var args = Environment.GetCommandLineArgs();
                    bool found = false;
                    foreach (var arg in args)
                    {
                        if (string.Equals(arg, "--commander", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(arg, "-commander", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(arg, "/commander", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(arg, "--fleet", StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }
                    _isCommanderModeCache = found;
                }
                return _isCommanderModeCache.Value;
            }
        }

        /// <summary>
        /// 서비스 구동 모드 (--service 인수 수신) 여부
        /// </summary>
        public static bool IsServiceMode
        {
            get
            {
                if (!_isServiceModeCache.HasValue)
                {
                    var args = Environment.GetCommandLineArgs();
                    bool found = false;
                    foreach (var arg in args)
                    {
                        if (string.Equals(arg, "--service", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(arg, "-service", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(arg, "/service", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(arg, "--agent-service", StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            break;
                        }
                    }
                    _isServiceModeCache = found;
                }
                return _isServiceModeCache.Value;
            }
        }

        /// <summary>
        /// 데스크탑 인스톨러를 통해 설치된 환경인지 여부
        /// </summary>
        public static bool IsInstalled
        {
            get
            {
                if (!_isInstalledCache.HasValue)
                {
                    _isInstalledCache = DetectInstalledEnvironment();
                }
                return _isInstalledCache.Value;
            }
        }

        /// <summary>
        /// 휴대용/단일 파일 무설치(포터블) 모드인지 여부
        /// (포터블 모드에서는 보안상 원격 커맨더 명령 수신 에이전트를 가동하지 않음)
        /// </summary>
        public static bool IsPortable => !IsInstalled && !IsServiceMode;

        private static bool? _isCommanderMenuEnabledCache;

        /// <summary>
        /// 다중 PC 중앙 관제(Commander) 메뉴 노출 여부.
        /// 일반 사용자의 오동작(사내/네트워크 PC 일괄 조작 실수)을 방지하기 위해 메인 WinPurify Pro 메뉴에 항상 등록하지 않으며,
        /// 명시적 관리자 인수(--commander, --fleet, --admin) 또는 'commander.tag' 파일이 존재할 때만 조건부 활성화됩니다.
        /// </summary>
        public static bool IsCommanderMenuEnabled
        {
            get
            {
                if (!_isCommanderMenuEnabledCache.HasValue)
                {
                    bool enabled = false;

                    // 1. 커맨드라인 인수 확인 (--commander, -commander, --fleet, --admin, --enable-commander)
                    try
                    {
                        var args = Environment.GetCommandLineArgs();
                        foreach (var arg in args)
                        {
                            if (string.Equals(arg, "--commander", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(arg, "-commander", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(arg, "/commander", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(arg, "--fleet", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(arg, "--admin", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(arg, "--enable-commander", StringComparison.OrdinalIgnoreCase))
                            {
                                enabled = true;
                                break;
                            }
                        }
                    }
                    catch { }

                    // 2. 관리자 활성화 태그 파일 (commander.tag) 존재 여부 확인
                    if (!enabled)
                    {
                        try
                        {
                            string appDir = AppDomain.CurrentDomain.BaseDirectory;
                            if (File.Exists(Path.Combine(appDir, "commander.tag")))
                            {
                                enabled = true;
                            }
                        }
                        catch { }
                    }

                    _isCommanderMenuEnabledCache = enabled;
                }
                return _isCommanderMenuEnabledCache.Value;
            }
        }

        private static bool DetectInstalledEnvironment()
        {
            try
            {
                // 1. 애플리케이션 실행 디렉토리에 installed.tag 존재 확인
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string tagPath = Path.Combine(appDir, "installed.tag");
                if (File.Exists(tagPath))
                {
                    return true;
                }

                // 2. Inno Setup Uninstall Registry 확인
                const string uninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{6904B914-A0E9-4BFA-BA5F-BCB0BEA7EF0F}_is1";
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = hklm.OpenSubKey(uninstallKeyPath))
                {
                    if (key != null) return true;
                }

                using (var hkcu = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
                using (var key = hkcu.OpenSubKey(uninstallKeyPath))
                {
                    if (key != null) return true;
                }

                // 3. WinPurifyAgentService 윈도우 서비스 등록 여부 확인
                const string serviceKeyPath = @"SYSTEM\CurrentControlSet\Services\" + ServiceName;
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                using (var key = hklm.OpenSubKey(serviceKeyPath))
                {
                    if (key != null) return true;
                }
            }
            catch
            {
                // 권한 등 오류 시 보수적으로 미설치(포터블) 간주
            }

            return false;
        }

        /// <summary>
        /// 윈도우 서비스가 현재 시스템에 등록되어 있는지 확인
        /// </summary>
        public static bool IsWindowsServiceRegistered()
        {
            try
            {
                const string serviceKeyPath = @"SYSTEM\CurrentControlSet\Services\" + ServiceName;
                using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using var key = hklm.OpenSubKey(serviceKeyPath);
                return key != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
