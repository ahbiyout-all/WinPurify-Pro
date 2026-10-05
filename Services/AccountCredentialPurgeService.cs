using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WinPurifyPro.Services
{
    public class AccountSessionTarget : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string IconEmoji { get; set; } = "🔑";
        public string Description { get; set; } = string.Empty;
        public string TechnicalPaths { get; set; } = string.Empty;

        // XAML Property Aliases for WPF Binding Compatibility
        public string Icon => IconEmoji;
        public string ServiceName => Name;
        public string TargetSummary => TechnicalPaths;

        public string[] AssociatedProcessNames { get; set; } = Array.Empty<string>();

        private bool _isSelected = true;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        private bool _isDetected = false;
        public bool IsDetected
        {
            get => _isDetected;
            set { _isDetected = value; OnPropertyChanged(); }
        }

        private string _detectedSummary = "스캔 대기 중...";
        public string DetectedSummary
        {
            get => _detectedSummary;
            set { _detectedSummary = value; OnPropertyChanged(); }
        }

        private string _status = "준비";
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }

    /// <summary>
    /// 로그인된 사용자 계정정보 및 세션 토큰 (Microsoft, Adobe, Autodesk, 브라우저, 메신저, 금융인증서, 클라우드, 게임, 개발자)
    /// 완벽 검출 및 원클릭 강제 로그아웃/자격 증명 정화 오케스트레이션 서비스
    /// </summary>
    public static class AccountCredentialPurgeService
    {
        // ====================================================================
        // Windows Credential Manager Native P/Invoke
        // ====================================================================
        private const int CRED_TYPE_GENERIC = 1;
        private const int CRED_TYPE_DOMAIN_PASSWORD = 2;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
        {
            public uint Flags;
            public uint Type;
            public IntPtr TargetName;
            public IntPtr Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            public IntPtr UserName;
        }

        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredEnumerateW(string? filter, int flag, out uint count, out IntPtr pCredentials);

        [DllImport("Advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredDeleteW(string target, uint type, int flags);

        [DllImport("Advapi32.dll", SetLastError = true)]
        private static extern void CredFree(IntPtr buffer);

        /// <summary>
        /// 기본 지원 15개 통합 계정 및 세션 타깃 목록 생성
        /// </summary>
        public static List<AccountSessionTarget> CreateDefaultTargets()
        {
            return new List<AccountSessionTarget>
            {
                new AccountSessionTarget
                {
                    Id = "microsoft",
                    Name = "Microsoft 계정 & Office/Teams 세션",
                    Category = "운영체제 및 오피스",
                    IconEmoji = "🪟",
                    Description = "Windows Credential Manager의 MicrosoftAccount, WindowsLive, SSO_POP 자격 증명과 WAM(Web Account Manager), Office/Teams 로그인 토큰을 완전히 소거하여 강제 로그아웃합니다.",
                    TechnicalPaths = @"Credential Manager (MicrosoftAccount:*, SSO_POP_*), %LOCALAPPDATA%\Microsoft\IdentityCache, OneAuth, TokenBroker",
                    AssociatedProcessNames = new[] { "Teams", "ms-teams", "ONENOTE", "OUTLOOK", "WINWORD", "EXCEL" },
                    IsSelected = true
                },
                new AccountSessionTarget
                {
                    Id = "adobe",
                    Name = "Adobe Creative Cloud 계정 세션",
                    Category = "디자인 & 크리에이티브",
                    IconEmoji = "🎨",
                    Description = "Adobe Creative Cloud, Photoshop, Acrobat 등의 OOBE 로그인 세션 토큰 및 Windows 자격 증명을 제거하여 계정을 로그아웃 상태로 전환합니다.",
                    TechnicalPaths = @"Credential Manager (Adobe*), %LOCALAPPDATA%\Adobe\OOBE, %APPDATA%\Adobe\OOBE, Creative Cloud Libraries",
                    AssociatedProcessNames = new[] { "Creative Cloud", "Adobe Desktop Service", "AdobeUpdateService", "Photoshop", "Acrobat" },
                    IsSelected = true
                },
                new AccountSessionTarget
                {
                    Id = "autodesk",
                    Name = "Autodesk (AutoCAD) 로그인 상태",
                    Category = "CAD & 엔지니어링",
                    IconEmoji = "📐",
                    Description = "AutoCAD, Revit, Inventor 등 Autodesk 제품군의 Web Services LoginState.xml 세션 및 AdskIdentityManager 자격 증명을 완전히 초기화합니다.",
                    TechnicalPaths = @"%LOCALAPPDATA%\Autodesk\Web Services\LoginState.xml, %APPDATA%\Autodesk\AdskIdentityManager",
                    AssociatedProcessNames = new[] { "acad", "AdskIdentityManager", "AdskLicensingAgent" },
                    IsSelected = true
                },
                new AccountSessionTarget
                {
                    Id = "edge",
                    Name = "Microsoft Edge 브라우저 로그인 세션",
                    Category = "웹 브라우저",
                    IconEmoji = "🌐",
                    Description = "Edge 브라우저 프로필에 저장된 자동 로그인 쿠키, Login Data(저장된 비밀번호), Token Service 및 브라우징 세션을 초기화합니다.",
                    TechnicalPaths = @"%LOCALAPPDATA%\Microsoft\Edge\User Data\Default (Cookies, Login Data, Token Service, Sessions)",
                    AssociatedProcessNames = new[] { "msedge" },
                    IsSelected = true
                },
                new AccountSessionTarget
                {
                    Id = "chrome",
                    Name = "Google Chrome 브라우저 로그인 세션",
                    Category = "웹 브라우저",
                    IconEmoji = "⚡",
                    Description = "Chrome 브라우저의 구글 계정 동기화 세션, 자동 로그인 쿠키, Login Data, Account 토큰을 초기화하여 모든 웹사이트에서 로그아웃합니다.",
                    TechnicalPaths = @"%LOCALAPPDATA%\Google\Chrome\User Data\Default (Cookies, Login Data, Accounts, Sessions)",
                    AssociatedProcessNames = new[] { "chrome" },
                    IsSelected = true
                },
                new AccountSessionTarget
                {
                    Id = "kakaotalk",
                    Name = "카카오톡 (KakaoTalk) 자동로그인 & 기기인증 토큰",
                    Category = "메신저 & SNS",
                    IconEmoji = "💬",
                    Description = "카카오톡 PC버전의 사용자 프로필 폴더, 기기 인증 세션 캐시 및 Windows 레지스트리 자동로그인/암호저장 플래그를 소거하여 즉시 로그아웃합니다.",
                    TechnicalPaths = @"%LOCALAPPDATA%\Kakao\KakaoTalk\users, %APPDATA%\Kakao, HKCU\Software\Kakao\KakaoTalk",
                    AssociatedProcessNames = new[] { "KakaoTalk" },
                    IsSelected = true
                },
                new AccountSessionTarget
                {
                    Id = "discord",
                    Name = "디스코드 (Discord) 인증 토큰 & 세션 스토리지",
                    Category = "메신저 & SNS",
                    IconEmoji = "🎧",
                    Description = "디스코드 클라이언트의 LevelDB 인증 토큰 스토리지, Session Storage, 통화 및 계정 캐시를 소거하여 모든 디스코드 세션을 강제 해제합니다.",
                    TechnicalPaths = @"%APPDATA%\discord\Local Storage\leveldb, %APPDATA%\discord\Session Storage, Cache",
                    AssociatedProcessNames = new[] { "Discord" },
                    IsSelected = true
                },
                new AccountSessionTarget
                {
                    Id = "telegram",
                    Name = "텔레그램 (Telegram Desktop) tdata 인증 키 & 세션",
                    Category = "메신저 & SNS",
                    IconEmoji = "✈️",
                    Description = "텔레그램 PC버전의 암호화된 계정 키가 보관된 tdata 세션 폴더를 제거하여 로컬 기기에서의 자동 로그인을 완전히 해제합니다.",
                    TechnicalPaths = @"%APPDATA%\Telegram Desktop\tdata (user_data, key_datas, sessions)",
                    AssociatedProcessNames = new[] { "Telegram" },
                    IsSelected = true
                },
                new AccountSessionTarget
                {
                    Id = "collab",
                    Name = "협업 도구 (Slack / Zoom / New Teams) 세션",
                    Category = "업무 & 협업",
                    IconEmoji = "👥",
                    Description = "기업용 메신저 Slack의 워크스페이스 세션, Zoom 회의 계정 데이터, New Teams(UWP) 로컬 캐시를 일괄 소거하여 사내 세션을 안전하게 정리합니다.",
                    TechnicalPaths = @"%APPDATA%\Slack, %APPDATA%\Zoom\data, %LOCALAPPDATA%\Packages\MSTeams_*\LocalCache",
                    AssociatedProcessNames = new[] { "slack", "Zoom", "Teams", "ms-teams" },
                    IsSelected = true
                },
                new AccountSessionTarget
                {
                    Id = "npki",
                    Name = "금융 & 공공 공동인증서 (NPKI / GPKI 전자서명키)",
                    Category = "금융 & 공공 보안",
                    IconEmoji = "🏦",
                    Description = "은행, 증권, 국세청 홈택스 등에 사용되는 NPKI(공동인증서 signCert.der, signPri.key) 및 공공 GPKI 인증서 폴더를 로컬 디스크에서 완전히 영구 소거합니다.",
                    TechnicalPaths = @"%USERPROFILE%\AppData\LocalLow\NPKI, GPKI, C:\NPKI, C:\GPKI",
                    AssociatedProcessNames = Array.Empty<string>(),
                    IsSelected = false
                },
                new AccountSessionTarget
                {
                    Id = "clouddrive",
                    Name = "클라우드 동기화 드라이브 (Google Drive, OneDrive, Dropbox, Notion)",
                    Category = "클라우드 & 스토리지",
                    IconEmoji = "☁️",
                    Description = "Google Drive 데스크톱, OneDrive, Dropbox, Notion의 로컬 인증 캐시 및 동기화 세션을 소거하여 로컬 파일 접근 권한을 해제합니다.",
                    TechnicalPaths = @"%LOCALAPPDATA%\Google\DriveFS, %LOCALAPPDATA%\Microsoft\OneDrive\settings, %LOCALAPPDATA%\Dropbox, %APPDATA%\Notion",
                    AssociatedProcessNames = new[] { "GoogleDriveFS", "OneDrive", "Dropbox", "Notion" },
                    IsSelected = false
                },
                new AccountSessionTarget
                {
                    Id = "gaming",
                    Name = "게임 플랫폼 세션 (Steam, Riot Games, Epic Games)",
                    Category = "게임 & 엔터테인먼트",
                    IconEmoji = "🕹️",
                    Description = "Steam Guard 인증 토큰(ssfn*) 및 자동로그인 vdf/레지스트리, Riot Games Client 로그인 토큰, Epic Games Launcher 인증 세션을 일괄 해제합니다.",
                    TechnicalPaths = @"Steam\config\loginusers.vdf, ssfn*, %LOCALAPPDATA%\Riot Games, %LOCALAPPDATA%\EpicGamesLauncher",
                    AssociatedProcessNames = new[] { "steam", "RiotClientServices", "EpicGamesLauncher", "LeagueClient" },
                    IsSelected = false
                },
                new AccountSessionTarget
                {
                    Id = "developer",
                    Name = "개발자 자격 증명 (Git/GitHub, SSH 개인키, AWS/GCloud CLI)",
                    Category = "개발자 & 엔지니어링",
                    IconEmoji = "💻",
                    Description = "GitHub Desktop 토큰, Windows Credential의 git:https:// 자격 증명, SSH 개인키(id_rsa, id_ed25519), AWS/GCloud CLI 인증 캐시를 안전하게 소거합니다.",
                    TechnicalPaths = @"%APPDATA%\GitHub Desktop, %USERPROFILE%\.ssh, %USERPROFILE%\.aws, %APPDATA%\gcloud",
                    AssociatedProcessNames = new[] { "GitHubDesktop" },
                    IsSelected = false
                },
                new AccountSessionTarget
                {
                    Id = "wincred",
                    Name = "Windows 자격 증명 관리자 (일반/웹 암호 일괄)",
                    Category = "보안 볼트",
                    IconEmoji = "🔐",
                    Description = "Windows 자격 증명 관리자(Credential Vault)에 자동 저장된 모든 일반(Generic) 웹사이트 암호 및 앱 로그인 자격 증명을 일괄 파쇄합니다.",
                    TechnicalPaths = @"Windows Credential Manager -> 일반 자격 증명 (Generic Credentials) 일체",
                    AssociatedProcessNames = Array.Empty<string>(),
                    IsSelected = false
                },
                new AccountSessionTarget
                {
                    Id = "netshares",
                    Name = "네트워크 드라이브 & SMB 공유 연결 세션",
                    Category = "네트워크",
                    IconEmoji = "📁",
                    Description = "사내 NAS, 공유 폴더, 네트워크 드라이브에 로그인된 모든 SMB 인증 세션을 일괄 해제합니다 (net use * /delete /y).",
                    TechnicalPaths = @"Windows Net Session & SMB Client Credentials",
                    AssociatedProcessNames = Array.Empty<string>(),
                    IsSelected = false
                }
            };
        }

        /// <summary>
        /// 상황별 원클릭 시나리오 프리셋 적용
        /// </summary>
        public static void ApplyScenarioPreset(IEnumerable<AccountSessionTarget> targets, string presetId)
        {
            var targetList = targets.ToList();
            switch (presetId.ToLowerInvariant())
            {
                case "workplace": // 🏢 사무실/업무 PC 반납
                    var workplaceIds = new HashSet<string> { "microsoft", "edge", "chrome", "collab", "npki", "clouddrive", "netshares", "wincred" };
                    foreach (var t in targetList) t.IsSelected = workplaceIds.Contains(t.Id);
                    break;

                case "full": // 💻 PC 양도 / 중고 판매 (Zero-Trace All)
                    foreach (var t in targetList) t.IsSelected = true;
                    break;

                case "public": // ☕ 공용 PC / PC방 이용 후 정리
                    var publicIds = new HashSet<string> { "chrome", "edge", "kakaotalk", "discord", "telegram", "gaming", "npki" };
                    foreach (var t in targetList) t.IsSelected = publicIds.Contains(t.Id);
                    break;

                case "developer": // 👨‍💻 개발자 / 보안 점검
                    var devIds = new HashSet<string> { "developer", "wincred", "netshares", "microsoft", "chrome" };
                    foreach (var t in targetList) t.IsSelected = devIds.Contains(t.Id);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 전체 계정 세션 및 자격 증명 감지 스캔
        /// </summary>
        public static async Task ScanAllTargetsAsync(IEnumerable<AccountSessionTarget> targets)
        {
            await Task.Run(() =>
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                // 1. Windows Credential Manager 자격 증명 열거
                var credentials = GetWindowsCredentialTargetNames();

                foreach (var target in targets)
                {
                    target.Status = "검사 중...";
                    int detectedCount = 0;
                    var details = new List<string>();

                    switch (target.Id)
                    {
                        case "microsoft":
                            int msCreds = credentials.Count(c =>
                                c.Contains("microsoft", StringComparison.OrdinalIgnoreCase) ||
                                c.Contains("windowslive", StringComparison.OrdinalIgnoreCase) ||
                                c.Contains("sso_pop_", StringComparison.OrdinalIgnoreCase) ||
                                c.Contains("onedrive", StringComparison.OrdinalIgnoreCase) ||
                                c.Contains("office", StringComparison.OrdinalIgnoreCase) ||
                                c.Contains("xbox", StringComparison.OrdinalIgnoreCase));
                            if (msCreds > 0) details.Add($"자격 증명 {msCreds}건");

                            var msDirs = new[]
                            {
                                Path.Combine(localAppData, "Microsoft", "IdentityCache"),
                                Path.Combine(localAppData, "Microsoft", "OneAuth"),
                                Path.Combine(localAppData, "Microsoft", "TokenBroker")
                            };
                            int dirCount = msDirs.Count(Directory.Exists);
                            if (dirCount > 0) details.Add($"토큰 캐시 {dirCount}개 폴더");

                            detectedCount = msCreds + dirCount;
                            break;

                        case "adobe":
                            int adobeCreds = credentials.Count(c => c.Contains("adobe", StringComparison.OrdinalIgnoreCase));
                            if (adobeCreds > 0) details.Add($"자격 증명 {adobeCreds}건");

                            var oobeDirs = new[]
                            {
                                Path.Combine(localAppData, "Adobe", "OOBE"),
                                Path.Combine(appData, "Adobe", "OOBE")
                            };
                            int oobeCount = oobeDirs.Count(Directory.Exists);
                            if (oobeCount > 0) details.Add($"OOBE 세션 {oobeCount}개 폴더");

                            detectedCount = adobeCreds + oobeCount;
                            break;

                        case "autodesk":
                            int adskCreds = credentials.Count(c =>
                                c.Contains("autodesk", StringComparison.OrdinalIgnoreCase) ||
                                c.Contains("adsk", StringComparison.OrdinalIgnoreCase));
                            if (adskCreds > 0) details.Add($"자격 증명 {adskCreds}건");

                            var loginStateXml = Path.Combine(localAppData, "Autodesk", "Web Services", "LoginState.xml");
                            if (File.Exists(loginStateXml)) details.Add("LoginState.xml 세션 파일");

                            var adskIdDir = Path.Combine(appData, "Autodesk", "AdskIdentityManager");
                            if (Directory.Exists(adskIdDir)) details.Add("AdskIdentityManager 폴더");

                            detectedCount = adskCreds + (File.Exists(loginStateXml) ? 1 : 0) + (Directory.Exists(adskIdDir) ? 1 : 0);
                            break;

                        case "edge":
                            var edgeDefault = Path.Combine(localAppData, "Microsoft", "Edge", "User Data", "Default");
                            if (Directory.Exists(edgeDefault))
                            {
                                int edgeFiles = 0;
                                if (File.Exists(Path.Combine(edgeDefault, "Login Data"))) edgeFiles++;
                                if (File.Exists(Path.Combine(edgeDefault, "Network", "Cookies"))) edgeFiles++;
                                if (Directory.Exists(Path.Combine(edgeDefault, "Sessions"))) edgeFiles++;
                                if (edgeFiles > 0) details.Add($"세션/쿠키/LoginData {edgeFiles}개 항목");
                                detectedCount = edgeFiles;
                            }
                            break;

                        case "chrome":
                            var chromeDefault = Path.Combine(localAppData, "Google", "Chrome", "User Data", "Default");
                            if (Directory.Exists(chromeDefault))
                            {
                                int chromeFiles = 0;
                                if (File.Exists(Path.Combine(chromeDefault, "Login Data"))) chromeFiles++;
                                if (File.Exists(Path.Combine(chromeDefault, "Network", "Cookies"))) chromeFiles++;
                                if (Directory.Exists(Path.Combine(chromeDefault, "Sessions"))) chromeFiles++;
                                if (chromeFiles > 0) details.Add($"세션/쿠키/LoginData {chromeFiles}개 항목");
                                detectedCount = chromeFiles;
                            }
                            break;

                        case "kakaotalk":
                            var kakaoUsers = Path.Combine(localAppData, "Kakao", "KakaoTalk", "users");
                            var kakaoApp = Path.Combine(appData, "Kakao");
                            if (Directory.Exists(kakaoUsers)) details.Add("사용자 계정 프로필 폴더");
                            if (Directory.Exists(kakaoApp)) details.Add("Kakao 세션 캐시");
                            detectedCount = (Directory.Exists(kakaoUsers) ? 1 : 0) + (Directory.Exists(kakaoApp) ? 1 : 0);
                            break;

                        case "discord":
                            var discordLevelDb = Path.Combine(appData, "discord", "Local Storage", "leveldb");
                            var discordSession = Path.Combine(appData, "discord", "Session Storage");
                            if (Directory.Exists(discordLevelDb)) details.Add("Discord LevelDB 토큰");
                            if (Directory.Exists(discordSession)) details.Add("Session Storage");
                            detectedCount = (Directory.Exists(discordLevelDb) ? 1 : 0) + (Directory.Exists(discordSession) ? 1 : 0);
                            break;

                        case "telegram":
                            var tdata = Path.Combine(appData, "Telegram Desktop", "tdata");
                            if (Directory.Exists(tdata))
                            {
                                details.Add("Telegram Desktop tdata 계정 키");
                                detectedCount = 1;
                            }
                            break;

                        case "collab":
                            int collabItems = 0;
                            if (Directory.Exists(Path.Combine(appData, "Slack"))) { details.Add("Slack 캐시"); collabItems++; }
                            if (Directory.Exists(Path.Combine(appData, "Zoom", "data"))) { details.Add("Zoom 데이터"); collabItems++; }
                            if (Directory.Exists(Path.Combine(appData, "Microsoft", "Teams"))) { details.Add("Teams 클래식"); collabItems++; }
                            detectedCount = collabItems;
                            break;

                        case "npki":
                            var npkiDir = Path.Combine(userProfile, "AppData", "LocalLow", "NPKI");
                            var gpkiDir = Path.Combine(userProfile, "AppData", "LocalLow", "GPKI");
                            int certsFound = 0;
                            if (Directory.Exists(npkiDir))
                            {
                                int certFiles = Directory.GetFiles(npkiDir, "*.der", SearchOption.AllDirectories).Length;
                                details.Add($"공동인증서(NPKI) {certFiles}개 키 세트");
                                certsFound += Math.Max(1, certFiles);
                            }
                            if (Directory.Exists(gpkiDir))
                            {
                                details.Add("행정전자서명(GPKI) 키");
                                certsFound++;
                            }
                            if (Directory.Exists(@"C:\NPKI")) { details.Add("C:\\NPKI 루트 인증서"); certsFound++; }
                            detectedCount = certsFound;
                            break;

                        case "clouddrive":
                            int driveCount = 0;
                            if (Directory.Exists(Path.Combine(localAppData, "Google", "DriveFS"))) { details.Add("Google Drive"); driveCount++; }
                            if (Directory.Exists(Path.Combine(localAppData, "Microsoft", "OneDrive", "settings"))) { details.Add("OneDrive"); driveCount++; }
                            if (Directory.Exists(Path.Combine(localAppData, "Dropbox"))) { details.Add("Dropbox"); driveCount++; }
                            if (Directory.Exists(Path.Combine(appData, "Notion"))) { details.Add("Notion"); driveCount++; }
                            detectedCount = driveCount;
                            break;

                        case "gaming":
                            int gameCount = 0;
                            var steamVdf = @"C:\Program Files (x86)\Steam\config\loginusers.vdf";
                            if (File.Exists(steamVdf)) { details.Add("Steam 로그인 세션"); gameCount++; }
                            if (Directory.Exists(Path.Combine(localAppData, "Riot Games", "Riot Client", "Data"))) { details.Add("Riot Client"); gameCount++; }
                            if (Directory.Exists(Path.Combine(localAppData, "EpicGamesLauncher"))) { details.Add("Epic Games"); gameCount++; }
                            detectedCount = gameCount;
                            break;

                        case "developer":
                            int devCount = 0;
                            int gitCreds = credentials.Count(c => c.Contains("git", StringComparison.OrdinalIgnoreCase) || c.Contains("github", StringComparison.OrdinalIgnoreCase));
                            if (gitCreds > 0) { details.Add($"Git 자격증명 {gitCreds}건"); devCount += gitCreds; }
                            var sshDir = Path.Combine(userProfile, ".ssh");
                            if (Directory.Exists(sshDir) && Directory.GetFiles(sshDir).Length > 0) { details.Add("SSH 키 세트"); devCount++; }
                            if (Directory.Exists(Path.Combine(userProfile, ".aws"))) { details.Add("AWS CLI 인증"); devCount++; }
                            if (Directory.Exists(Path.Combine(appData, "gcloud"))) { details.Add("GCloud CLI"); devCount++; }
                            detectedCount = devCount;
                            break;

                        case "wincred":
                            int totalCreds = credentials.Count;
                            if (totalCreds > 0) details.Add($"총 {totalCreds}개 자격 증명 보관 중");
                            detectedCount = totalCreds;
                            break;

                        case "netshares":
                            details.Add("활성 네트워크 드라이브 및 SMB 인증 캐시");
                            detectedCount = 1;
                            break;
                    }

                    target.IsDetected = (detectedCount > 0);
                    target.DetectedSummary = (detectedCount > 0)
                        ? string.Join(", ", details)
                        : "발견된 활성 로그인 세션 없음 (정상)";
                    target.Status = target.IsDetected ? "정화 대기" : "정상 (미감지)";
                }
            });
        }

        /// <summary>
        /// 선택된 계정 세션 대상 일괄 정화 / 강제 로그아웃
        /// </summary>
        /// <param name="targets">정화 대상 목록</param>
        /// <param name="terminateInUseProcesses">세션 잠금 방지를 위한 실행 중인 프로세스 선행 종료 여부</param>
        /// <param name="progressLogger">로그 콜백</param>
        public static async Task<int> PurgeTargetsAsync(
            IEnumerable<AccountSessionTarget> targets,
            bool terminateInUseProcesses = true,
            Action<string>? progressLogger = null)
        {
            return await Task.Run(() =>
            {
                int totalPurged = 0;
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                var selectedTargets = targets.Where(t => t.IsSelected).ToList();

                // 1. 소거 전 잠금 프로세스 선행 종료 (Process Pre-Termination)
                if (terminateInUseProcesses)
                {
                    progressLogger?.Invoke("[*] [Pre-Termination] 세션 잠금 방지를 위한 대상 프로세스 자동 종료 시작...");
                    NativeEngineService.TryPurifyAppSession(16); // C++ Native Pre-termination

                    foreach (var target in selectedTargets)
                    {
                        foreach (var procName in target.AssociatedProcessNames)
                        {
                            try
                            {
                                var runningProcs = Process.GetProcessesByName(procName);
                                foreach (var p in runningProcs)
                                {
                                    try
                                    {
                                        p.Kill();
                                        p.WaitForExit(1000);
                                    }
                                    catch { }
                                }
                            }
                            catch { }
                        }
                    }
                    progressLogger?.Invoke("[+] [Pre-Termination] 관련 프로세스 종료 완료.");
                }

                // 2. 각 타깃별 정화 수행
                foreach (var target in selectedTargets)
                {
                    target.Status = "로그아웃 정화 중...";
                    progressLogger?.Invoke($"[*] '{target.Name}' 정화 및 로그아웃 실행 중...");

                    try
                    {
                        switch (target.Id)
                        {
                            case "microsoft":
                                NativeEngineService.TryPurifyCredentials("microsoft");
                                NativeEngineService.TryPurifyAppSession(1);

                                DeleteWindowsCredentialsByKeyword("microsoft");
                                DeleteWindowsCredentialsByKeyword("windowslive");
                                DeleteWindowsCredentialsByKeyword("sso_pop_");
                                DeleteWindowsCredentialsByKeyword("onedrive");
                                DeleteWindowsCredentialsByKeyword("office");

                                SafeDeleteDirectory(Path.Combine(localAppData, "Microsoft", "IdentityCache"));
                                SafeDeleteDirectory(Path.Combine(localAppData, "Microsoft", "OneAuth"));
                                SafeDeleteDirectory(Path.Combine(localAppData, "Microsoft", "TokenBroker"));
                                SafeExecuteCli(@"reg delete ""HKCU\Software\Microsoft\Office\16.0\Common\Identity"" /f");
                                totalPurged++;
                                break;

                            case "adobe":
                                NativeEngineService.TryPurifyCredentials("adobe");
                                NativeEngineService.TryPurifyAppSession(2);

                                DeleteWindowsCredentialsByKeyword("adobe");
                                SafeDeleteDirectory(Path.Combine(localAppData, "Adobe", "OOBE"));
                                SafeDeleteDirectory(Path.Combine(appData, "Adobe", "OOBE"));
                                SafeDeleteDirectory(Path.Combine(localAppData, "Adobe", "Creative Cloud Libraries"));
                                totalPurged++;
                                break;

                            case "autodesk":
                                NativeEngineService.TryPurifyCredentials("autodesk");
                                NativeEngineService.TryPurifyCredentials("adsk");
                                NativeEngineService.TryPurifyAppSession(3);

                                DeleteWindowsCredentialsByKeyword("autodesk");
                                DeleteWindowsCredentialsByKeyword("adsk");
                                SafeDeleteFile(Path.Combine(localAppData, "Autodesk", "Web Services", "LoginState.xml"));
                                SafeDeleteDirectory(Path.Combine(appData, "Autodesk", "AdskIdentityManager"));
                                SafeDeleteDirectory(Path.Combine(localAppData, "Autodesk", "Common", "Identity"));
                                totalPurged++;
                                break;

                            case "edge":
                                NativeEngineService.TryPurifyAppSession(4);
                                var edgeDefault = Path.Combine(localAppData, "Microsoft", "Edge", "User Data", "Default");
                                SafeDeleteFile(Path.Combine(edgeDefault, "Network", "Cookies"));
                                SafeDeleteFile(Path.Combine(edgeDefault, "Network", "Cookies-journal"));
                                SafeDeleteFile(Path.Combine(edgeDefault, "Login Data"));
                                SafeDeleteFile(Path.Combine(edgeDefault, "Login Data-journal"));
                                SafeDeleteFile(Path.Combine(edgeDefault, "Login Data For Account"));
                                SafeDeleteFile(Path.Combine(edgeDefault, "Web Data"));
                                SafeDeleteFile(Path.Combine(edgeDefault, "Token Service"));
                                SafeDeleteDirectory(Path.Combine(edgeDefault, "Sessions"));
                                totalPurged++;
                                break;

                            case "chrome":
                                NativeEngineService.TryPurifyAppSession(5);
                                var chromeDefault = Path.Combine(localAppData, "Google", "Chrome", "User Data", "Default");
                                SafeDeleteFile(Path.Combine(chromeDefault, "Network", "Cookies"));
                                SafeDeleteFile(Path.Combine(chromeDefault, "Network", "Cookies-journal"));
                                SafeDeleteFile(Path.Combine(chromeDefault, "Login Data"));
                                SafeDeleteFile(Path.Combine(chromeDefault, "Login Data-journal"));
                                SafeDeleteFile(Path.Combine(chromeDefault, "Login Data For Account"));
                                SafeDeleteFile(Path.Combine(chromeDefault, "Web Data"));
                                SafeDeleteDirectory(Path.Combine(chromeDefault, "Accounts"));
                                SafeDeleteDirectory(Path.Combine(chromeDefault, "Sessions"));
                                totalPurged++;
                                break;

                            case "kakaotalk":
                                NativeEngineService.TryPurifyAppSession(8);
                                SafeDeleteDirectory(Path.Combine(localAppData, "Kakao", "KakaoTalk", "users"));
                                SafeDeleteDirectory(Path.Combine(appData, "Kakao", "KakaoTalk"));
                                SafeExecuteCli(@"reg delete ""HKCU\Software\Kakao\KakaoTalk"" /v ""AutoLogin"" /f");
                                SafeExecuteCli(@"reg delete ""HKCU\Software\Kakao\KakaoTalk"" /v ""SavePassword"" /f");
                                totalPurged++;
                                break;

                            case "discord":
                                NativeEngineService.TryPurifyAppSession(9);
                                SafeDeleteDirectory(Path.Combine(appData, "discord", "Local Storage", "leveldb"));
                                SafeDeleteDirectory(Path.Combine(appData, "discord", "Session Storage"));
                                SafeDeleteDirectory(Path.Combine(appData, "discord", "Cache"));
                                totalPurged++;
                                break;

                            case "telegram":
                                NativeEngineService.TryPurifyAppSession(10);
                                SafeDeleteDirectory(Path.Combine(appData, "Telegram Desktop", "tdata"));
                                totalPurged++;
                                break;

                            case "collab":
                                NativeEngineService.TryPurifyAppSession(11);
                                SafeDeleteDirectory(Path.Combine(appData, "Slack", "Local Storage"));
                                SafeDeleteDirectory(Path.Combine(appData, "Zoom", "data"));
                                SafeDeleteDirectory(Path.Combine(appData, "Microsoft", "Teams"));
                                SafeExecuteCli(@"powershell -Command ""Get-ChildItem -Path $env:LOCALAPPDATA\Packages -Filter MSTeams_* | ForEach-Object { Remove-Item -Path $_.FullName\LocalCache -Recurse -Force -ErrorAction SilentlyContinue }""");
                                totalPurged++;
                                break;

                            case "npki":
                                NativeEngineService.TryPurifyAppSession(12);
                                SafeDeleteDirectory(Path.Combine(userProfile, "AppData", "LocalLow", "NPKI"));
                                SafeDeleteDirectory(Path.Combine(userProfile, "AppData", "LocalLow", "GPKI"));
                                SafeDeleteDirectory(@"C:\NPKI");
                                SafeDeleteDirectory(@"C:\GPKI");
                                totalPurged++;
                                break;

                            case "clouddrive":
                                NativeEngineService.TryPurifyAppSession(13);
                                SafeDeleteDirectory(Path.Combine(localAppData, "Google", "DriveFS"));
                                SafeDeleteDirectory(Path.Combine(localAppData, "Microsoft", "OneDrive", "settings"));
                                SafeDeleteDirectory(Path.Combine(localAppData, "Dropbox"));
                                SafeDeleteDirectory(Path.Combine(appData, "Notion", "Local Storage"));
                                totalPurged++;
                                break;

                            case "gaming":
                                NativeEngineService.TryPurifyAppSession(14);
                                SafeDeleteFile(@"C:\Program Files (x86)\Steam\config\loginusers.vdf");
                                SafeExecuteCli(@"del /f /q ""C:\Program Files (x86)\Steam\ssfn*""");
                                SafeExecuteCli(@"reg delete ""HKCU\Software\Valve\Steam"" /v ""AutoLoginUser"" /f");
                                SafeDeleteDirectory(Path.Combine(localAppData, "Riot Games", "Riot Client", "Data"));
                                SafeDeleteDirectory(Path.Combine(localAppData, "EpicGamesLauncher", "Saved", "Config"));
                                totalPurged++;
                                break;

                            case "developer":
                                NativeEngineService.TryPurifyAppSession(15);
                                DeleteWindowsCredentialsByKeyword("git");
                                DeleteWindowsCredentialsByKeyword("github");
                                SafeDeleteDirectory(Path.Combine(appData, "GitHub Desktop"));
                                SafeExecuteCli(@"del /f /q """ + Path.Combine(userProfile, ".ssh", "id_*") + @"""");
                                SafeExecuteCli(@"del /f /q """ + Path.Combine(userProfile, ".ssh", "known_hosts") + @"""");
                                SafeDeleteDirectory(Path.Combine(userProfile, ".aws"));
                                SafeDeleteDirectory(Path.Combine(appData, "gcloud"));
                                totalPurged++;
                                break;

                            case "wincred":
                                NativeEngineService.TryPurifyCredentials("all");
                                DeleteAllGenericCredentials();
                                totalPurged++;
                                break;

                            case "netshares":
                                NativeEngineService.TryPurifyAppSession(6);
                                SafeExecuteCli("net use * /delete /y");
                                totalPurged++;
                                break;
                        }

                        target.Status = "로그아웃 완료";
                        target.IsDetected = false;
                        target.DetectedSummary = "세션/자격 증명 완전 소거됨";
                        progressLogger?.Invoke($"[+] '{target.Name}' 로그아웃 및 자격 증명 소거 완료");
                    }
                    catch (Exception ex)
                    {
                        target.Status = "오류 발생";
                        progressLogger?.Invoke($"[!] '{target.Name}' 정화 실패: {ex.Message}");
                    }
                }

                return totalPurged;
            });
        }

        // ====================================================================
        // Helpers
        // ====================================================================

        private static List<string> GetWindowsCredentialTargetNames()
        {
            var list = new List<string>();
            try
            {
                if (CredEnumerateW(null, 0, out uint count, out IntPtr pCreds) && pCreds != IntPtr.Zero)
                {
                    int ptrSize = IntPtr.Size;
                    for (int i = 0; i < count; i++)
                    {
                        IntPtr credPtr = Marshal.ReadIntPtr(pCreds, i * ptrSize);
                        if (credPtr != IntPtr.Zero)
                        {
                            var cred = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
                            if (cred.TargetName != IntPtr.Zero)
                            {
                                string? targetName = Marshal.PtrToStringUni(cred.TargetName);
                                if (!string.IsNullOrEmpty(targetName))
                                {
                                    list.Add(targetName);
                                }
                            }
                        }
                    }
                    CredFree(pCreds);
                }
            }
            catch
            {
                // Fallback to cmdkey
                try
                {
                    var psi = new ProcessStartInfo("cmdkey", "/list")
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using var proc = Process.Start(psi);
                    if (proc != null)
                    {
                        string output = proc.StandardOutput.ReadToEnd();
                        proc.WaitForExit(3000);
                        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            if (line.Contains("대상:", StringComparison.OrdinalIgnoreCase) ||
                                line.Contains("Target:", StringComparison.OrdinalIgnoreCase))
                            {
                                var parts = line.Split(':');
                                if (parts.Length >= 2) list.Add(parts[1].Trim());
                            }
                        }
                    }
                }
                catch { }
            }
            return list;
        }

        private static void DeleteWindowsCredentialsByKeyword(string keyword)
        {
            try
            {
                if (CredEnumerateW(null, 0, out uint count, out IntPtr pCreds) && pCreds != IntPtr.Zero)
                {
                    int ptrSize = IntPtr.Size;
                    for (int i = 0; i < count; i++)
                    {
                        IntPtr credPtr = Marshal.ReadIntPtr(pCreds, i * ptrSize);
                        if (credPtr != IntPtr.Zero)
                        {
                            var cred = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
                            if (cred.TargetName != IntPtr.Zero)
                            {
                                string? targetName = Marshal.PtrToStringUni(cred.TargetName);
                                if (!string.IsNullOrEmpty(targetName) &&
                                    targetName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                                {
                                    CredDeleteW(targetName, cred.Type, 0);
                                }
                            }
                        }
                    }
                    CredFree(pCreds);
                }
            }
            catch { }
        }

        private static void DeleteAllGenericCredentials()
        {
            try
            {
                if (CredEnumerateW(null, 0, out uint count, out IntPtr pCreds) && pCreds != IntPtr.Zero)
                {
                    int ptrSize = IntPtr.Size;
                    for (int i = 0; i < count; i++)
                    {
                        IntPtr credPtr = Marshal.ReadIntPtr(pCreds, i * ptrSize);
                        if (credPtr != IntPtr.Zero)
                        {
                            var cred = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
                            if (cred.TargetName != IntPtr.Zero &&
                                (cred.Type == CRED_TYPE_GENERIC || cred.Type == CRED_TYPE_DOMAIN_PASSWORD))
                            {
                                string? targetName = Marshal.PtrToStringUni(cred.TargetName);
                                if (!string.IsNullOrEmpty(targetName))
                                {
                                    CredDeleteW(targetName, cred.Type, 0);
                                }
                            }
                        }
                    }
                    CredFree(pCreds);
                }
            }
            catch { }
        }

        private static void SafeDeleteDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch { }
        }

        private static void SafeDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.SetAttributes(path, FileAttributes.Normal);
                    File.Delete(path);
                }
            }
            catch { }
        }

        private static void SafeExecuteCli(string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo("cmd.exe", $"/c {arguments}")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(4000);
            }
            catch { }
        }
    }
}
