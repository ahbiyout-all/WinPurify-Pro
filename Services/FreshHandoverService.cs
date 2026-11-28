using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Win32;
using WinPurifyPro.Models;

namespace WinPurifyPro.Services
{
    /// <summary>
    /// 이전 사용자 계정·프로필·개인폴더 및 모든 애플리케이션/브라우저/자격증명 로그인 세션을
    /// 자동 탐지하고 완전히 소거하여 새 사용자에게 깨끗한 초기 환경을 제공하는 핵심 인수인계 엔진
    /// </summary>
    public static class FreshHandoverService
    {
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

        [DllImport("userenv.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool DeleteProfileW(string lpSidString, string? lpProfilePath, string? lpComputerName);

        private static readonly Regex StrictDomainSidRegex = new(@"^S-1-5-21-\d+(-\d+)+$", RegexOptions.Compiled);

        private static readonly HashSet<string> ProtectedSystemSids = new(StringComparer.OrdinalIgnoreCase)
        {
            "S-1-5-18", // LocalSystem
            "S-1-5-19", // LocalService
            "S-1-5-20"  // NetworkService
        };

        private static readonly HashSet<string> ProtectedSystemFolderNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Public",
            "Default",
            "Default User",
            "All Users",
            "defaultuser0",
            "WDAGUtilityAccount"
        };

        /// <summary>
        /// 자기 보호(Self-Preservation) 및 OS 핵심 디렉터리 보호 가드
        /// </summary>
        public static bool IsPathProtected(string? candidatePath)
        {
            if (string.IsNullOrWhiteSpace(candidatePath)) return true;
            try
            {
                string fullCandidate = Path.GetFullPath(candidatePath).TrimEnd('\\');
                string appDir = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\');
                string programDataWinPurify = Path.GetFullPath(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "WinPurifyPro")
                ).TrimEnd('\\');

                // WinPurify Pro 실행 경로와 후보 경로가 포함/겹치면 절대 삭제 금지
                if (fullCandidate.Equals(appDir, StringComparison.OrdinalIgnoreCase) ||
                    appDir.StartsWith(fullCandidate + "\\", StringComparison.OrdinalIgnoreCase) ||
                    fullCandidate.Equals(programDataWinPurify, StringComparison.OrdinalIgnoreCase) ||
                    programDataWinPurify.StartsWith(fullCandidate + "\\", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                string windowsDir = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.Windows)).TrimEnd('\\');
                string sysDriveRoot = Path.GetPathRoot(windowsDir)?.TrimEnd('\\') ?? "C:";
                string usersRoot = Path.Combine(sysDriveRoot + "\\", "Users").TrimEnd('\\');
                string programFiles = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)).TrimEnd('\\');
                string programFilesX86 = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)).TrimEnd('\\');
                string commonAppData = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)).TrimEnd('\\');
                string currentUserProfile = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)).TrimEnd('\\');

                if (fullCandidate.Equals(sysDriveRoot, StringComparison.OrdinalIgnoreCase) ||
                    fullCandidate.Equals(windowsDir, StringComparison.OrdinalIgnoreCase) ||
                    fullCandidate.Equals(usersRoot, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(programFiles) && (fullCandidate.Equals(programFiles, StringComparison.OrdinalIgnoreCase) || programFiles.StartsWith(fullCandidate + "\\", StringComparison.OrdinalIgnoreCase))) ||
                    (!string.IsNullOrEmpty(programFilesX86) && (fullCandidate.Equals(programFilesX86, StringComparison.OrdinalIgnoreCase) || programFilesX86.StartsWith(fullCandidate + "\\", StringComparison.OrdinalIgnoreCase))) ||
                    (!string.IsNullOrEmpty(commonAppData) && (fullCandidate.Equals(commonAppData, StringComparison.OrdinalIgnoreCase) || commonAppData.StartsWith(fullCandidate + "\\", StringComparison.OrdinalIgnoreCase))) ||
                    (!string.IsNullOrEmpty(currentUserProfile) && (fullCandidate.Equals(currentUserProfile, StringComparison.OrdinalIgnoreCase) || currentUserProfile.StartsWith(fullCandidate + "\\", StringComparison.OrdinalIgnoreCase))))
                {
                    return true;
                }

                string folderName = Path.GetFileName(fullCandidate);
                if (fullCandidate.StartsWith(usersRoot + "\\", StringComparison.OrdinalIgnoreCase) &&
                    fullCandidate.Count(c => c == '\\') == usersRoot.Count(c => c == '\\') + 1 &&
                    ProtectedSystemFolderNames.Contains(folderName))
                {
                    return true;
                }
            }
            catch
            {
                return true;
            }
            return false;
        }

        private static bool IsReparsePointPath(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    return (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
                }
                if (Directory.Exists(path))
                {
                    return (new DirectoryInfo(path).Attributes & FileAttributes.ReparsePoint) != 0;
                }
            }
            catch { }
            return false;
        }

        public static string GetCurrentUserSid()
        {
            try
            {
                return WindowsIdentity.GetCurrent().User?.Value ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// PC 내 모든 Windows 사용자 프로필(레지스트리 SID + C:\Users\* 잔여 폴더) 전수 스캔
        /// </summary>
        public static async Task<List<DetectedUserProfile>> ScanUserProfilesAsync(Action<string>? logger = null)
        {
            return await Task.Run(async () =>
            {
                var results = new List<DetectedUserProfile>();
                string currentSid = GetCurrentUserSid();
                string currentUsername = Environment.UserName;
                var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                logger?.Invoke("[Handover Scan] Windows 사용자 프로필 및 SID 레지스트리 열거 시작...");

                try
                {
                    using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    using var profileListKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList");
                    if (profileListKey != null)
                    {
                        foreach (string sid in profileListKey.GetSubKeyNames())
                        {
                            try
                            {
                                using var subKey = profileListKey.OpenSubKey(sid);
                                if (subKey == null) continue;

                                string rawPath = subKey.GetValue("ProfileImagePath")?.ToString() ?? string.Empty;
                                string expandedPath = Environment.ExpandEnvironmentVariables(rawPath).TrimEnd('\\');
                                if (string.IsNullOrWhiteSpace(expandedPath)) continue;

                                bool isProtectedSid = ProtectedSystemSids.Contains(sid) || !sid.StartsWith("S-1-5-21-", StringComparison.OrdinalIgnoreCase);
                                string folderName = Path.GetFileName(expandedPath);
                                bool isCurrent = !string.IsNullOrEmpty(currentSid)
                                    ? sid.Equals(currentSid, StringComparison.OrdinalIgnoreCase)
                                    : folderName.Equals(currentUsername, StringComparison.OrdinalIgnoreCase);

                                bool isSysProtected = isProtectedSid || ProtectedSystemFolderNames.Contains(folderName) || IsPathProtected(expandedPath);
                                string resolvedUsername = TryResolveAccountNameFromSid(sid) ?? folderName;

                                DateTime? lastLoadTime = ReadProfileLoadTime(subKey);
                                long sizeBytes = Directory.Exists(expandedPath) && !isProtectedSid
                                    ? await RobocopyScannerService.GetFolderSizeFastAsync(expandedPath)
                                    : 0L;

                                int appDataCount = CountAppDataSubdirs(expandedPath);
                                bool hasNtUser = Directory.Exists(expandedPath) && File.Exists(Path.Combine(expandedPath, "NTUSER.DAT"));

                                seenPaths.Add(expandedPath);

                                results.Add(new DetectedUserProfile
                                {
                                    Sid = sid,
                                    Username = resolvedUsername,
                                    ProfilePath = expandedPath,
                                    AccountType = DetermineAccountType(sid, resolvedUsername),
                                    LastUsedTimeText = lastLoadTime?.ToString("yyyy-MM-dd HH:mm") ?? "기록 없음",
                                    IsCurrentSession = isCurrent,
                                    IsSystemProtected = isSysProtected,
                                    IsOrphanedDirectory = false,
                                    HasNtUserDat = hasNtUser,
                                    AppDataSubdirCount = appDataCount,
                                    ProtectionReason = isCurrent
                                        ? "현재 로그인된 실행 계정 (OS 보호됨)"
                                        : (isSysProtected ? "Windows 시스템 필수 프로필 (보호됨)" : string.Empty),
                                    EstimatedBytes = sizeBytes,
                                    IsSelected = !isSysProtected && !isCurrent,
                                    StatusText = isCurrent ? "현재 세션 (보호됨)" : (isSysProtected ? "시스템 보호" : "이전 사용자 감지됨")
                                });
                            }
                            catch { }
                        }
                    }
                }
                catch { }

                // 레지스트리에서 누락된 고립 C:\Users\<User> 디렉터리 추가 탐지
                try
                {
                    string sysDriveRoot = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.Windows)) ?? @"C:\";
                    string usersDir = Path.Combine(sysDriveRoot, "Users");
                    if (Directory.Exists(usersDir))
                    {
                        foreach (string dir in Directory.GetDirectories(usersDir))
                        {
                            string cleanDir = dir.TrimEnd('\\');
                            if (seenPaths.Contains(cleanDir)) continue;

                            string folderName = Path.GetFileName(cleanDir);
                            if (ProtectedSystemFolderNames.Contains(folderName)) continue;

                            var di = new DirectoryInfo(cleanDir);
                            if ((di.Attributes & FileAttributes.ReparsePoint) != 0) continue;

                            bool isSysProtected = IsPathProtected(cleanDir);
                            bool isCurrent = folderName.Equals(currentUsername, StringComparison.OrdinalIgnoreCase);
                            long sizeBytes = await RobocopyScannerService.GetFolderSizeFastAsync(cleanDir);
                            int appDataCount = CountAppDataSubdirs(cleanDir);
                            bool hasNtUser = File.Exists(Path.Combine(cleanDir, "NTUSER.DAT"));

                            results.Add(new DetectedUserProfile
                            {
                                Sid = "ORPHANED-FOLDER",
                                Username = folderName,
                                ProfilePath = cleanDir,
                                AccountType = "고립 프로필 폴더",
                                LastUsedTimeText = di.LastWriteTime.ToString("yyyy-MM-dd HH:mm"),
                                IsCurrentSession = isCurrent,
                                IsSystemProtected = isSysProtected,
                                IsOrphanedDirectory = true,
                                HasNtUserDat = hasNtUser,
                                AppDataSubdirCount = appDataCount,
                                ProtectionReason = isSysProtected ? "보호 경로 포함 (제외됨)" : string.Empty,
                                EstimatedBytes = sizeBytes,
                                IsSelected = !isSysProtected && !isCurrent,
                                StatusText = isSysProtected ? "경로 보호" : "고립 사용자 폴더 감지됨"
                            });
                        }
                    }
                }
                catch { }

                return results
                    .OrderByDescending(p => !p.IsSystemProtected && !p.IsCurrentSession)
                    .ThenBy(p => p.IsSystemProtected)
                    .ThenByDescending(p => p.EstimatedBytes)
                    .ToList();
            });
        }

        /// <summary>
        /// 모든 사용자 프로필 및 다중 브라우저 프로필을 아우르는 앱 데이터/로그인 세션/자격증명 전수 스캔
        /// </summary>
        public static async Task<List<DetectedSessionArtifact>> ScanSessionArtifactsAsync(
            IEnumerable<DetectedUserProfile> profiles,
            Action<string>? logger = null)
        {
            return await Task.Run(async () =>
            {
                var artifacts = new List<DetectedSessionArtifact>();
                var scanProfileRoots = profiles
                    .Where(p => !p.IsSystemProtected && Directory.Exists(p.ProfilePath))
                    .Select(p => p.ProfilePath)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                string currentUserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).TrimEnd('\\');
                if (!string.IsNullOrEmpty(currentUserProfile) &&
                    Directory.Exists(currentUserProfile) &&
                    !scanProfileRoots.Contains(currentUserProfile, StringComparer.OrdinalIgnoreCase))
                {
                    scanProfileRoots.Insert(0, currentUserProfile);
                }

                logger?.Invoke($"[Handover Scan] {scanProfileRoots.Count}개 사용자 프로필 및 다중 브라우저/앱 세션 정밀 검사 중...");

                var credNames = GetAllWindowsCredentialNames();

                // 1. 웹 브라우저 다중 프로필 (Chrome, Edge, Naver Whale, Brave, Firefox)
                artifacts.Add(await ScanChromiumMultiProfileAsync(
                    id: "browser-chrome",
                    title: "Google Chrome 전 프로필 (계정 동기화 · 쿠키 · 저장 암호)",
                    browserRelPath: Path.Combine("AppData", "Local", "Google", "Chrome", "User Data"),
                    processNames: new[] { "chrome" },
                    userRoots: scanProfileRoots
                ));

                artifacts.Add(await ScanChromiumMultiProfileAsync(
                    id: "browser-edge",
                    title: "Microsoft Edge 전 프로필 (MS 계정 · 쿠키 · 암호 · 세션)",
                    browserRelPath: Path.Combine("AppData", "Local", "Microsoft", "Edge", "User Data"),
                    processNames: new[] { "msedge" },
                    userRoots: scanProfileRoots
                ));

                artifacts.Add(await ScanChromiumMultiProfileAsync(
                    id: "browser-whale",
                    title: "Naver Whale 전 프로필 (네이버 로그인 · 쿠키 · 인증 데이터)",
                    browserRelPath: Path.Combine("AppData", "Local", "Naver", "Naver Whale", "User Data"),
                    processNames: new[] { "whale" },
                    userRoots: scanProfileRoots
                ));

                artifacts.Add(await ScanChromiumMultiProfileAsync(
                    id: "browser-brave",
                    title: "Brave 브라우저 전 프로필 (세션 · 쿠키 · Web Data)",
                    browserRelPath: Path.Combine("AppData", "Local", "BraveSoftware", "Brave-Browser", "User Data"),
                    processNames: new[] { "brave" },
                    userRoots: scanProfileRoots
                ));

                artifacts.Add(await ScanSimpleAppPathsAsync(
                    id: "browser-firefox",
                    title: "Mozilla Firefox 전 프로필 (logins.json · cookies.sqlite)",
                    category: HandoverDomainCategory.BrowserSession,
                    categoryLabel: "브라우저 세션",
                    description: "모든 사용자 계정의 Firefox Profiles 폴더 내 저장된 비밀번호(logins.json/key4.db) 및 쿠키를 소거합니다.",
                    processNames: new[] { "firefox" },
                    userRoots: scanProfileRoots,
                    relativePaths: new[] { Path.Combine("AppData", "Roaming", "Mozilla", "Firefox", "Profiles") }
                ));

                // 2. OS / 오피스 / 메신저 / 클라우드 / 크리에이티브 / 개발자 앱 데이터 및 로그인 세션
                artifacts.Add(await ScanSimpleAppPathsAsync(
                    id: "app-microsoft-office",
                    title: "Microsoft 계정 · WAM TokenBroker · Office/Teams 인증",
                    category: HandoverDomainCategory.AppSession,
                    categoryLabel: "OS & 오피스",
                    description: "IdentityCache, OneAuth, TokenBroker 및 Office/MicrosoftAccount 자격 증명을 일괄 제거합니다.",
                    processNames: new[] { "Teams", "ms-teams", "ONENOTE", "OUTLOOK", "WINWORD", "EXCEL" },
                    userRoots: scanProfileRoots,
                    relativePaths: new[]
                    {
                        Path.Combine("AppData", "Local", "Microsoft", "IdentityCache"),
                        Path.Combine("AppData", "Local", "Microsoft", "OneAuth"),
                        Path.Combine("AppData", "Local", "Microsoft", "TokenBroker")
                    },
                    credKeywords: new[] { "microsoft", "windowslive", "sso_pop_", "office", "onedrive", "teams" },
                    credNames: credNames,
                    regDeleteCommands: new[] { @"reg delete ""HKCU\Software\Microsoft\Office\16.0\Common\Identity"" /f" }
                ));

                artifacts.Add(await ScanSimpleAppPathsAsync(
                    id: "app-messengers",
                    title: "메신저 자동 로그인 (카카오톡 · Discord · Telegram · Slack · Zoom)",
                    category: HandoverDomainCategory.AppSession,
                    categoryLabel: "메신저 & 협업",
                    description: "카카오톡 users 프로필, Discord LevelDB 토큰, Telegram tdata, Slack/Zoom 로컬 세션을 모든 사용자에서 제거합니다.",
                    processNames: new[] { "KakaoTalk", "Discord", "Telegram", "slack", "Zoom" },
                    userRoots: scanProfileRoots,
                    relativePaths: new[]
                    {
                        Path.Combine("AppData", "Local", "Kakao", "KakaoTalk", "users"),
                        Path.Combine("AppData", "Roaming", "Kakao", "KakaoTalk"),
                        Path.Combine("AppData", "Roaming", "discord", "Local Storage", "leveldb"),
                        Path.Combine("AppData", "Roaming", "discord", "Session Storage"),
                        Path.Combine("AppData", "Roaming", "Telegram Desktop", "tdata"),
                        Path.Combine("AppData", "Roaming", "Slack", "Local Storage"),
                        Path.Combine("AppData", "Roaming", "Zoom", "data")
                    },
                    regDeleteCommands: new[]
                    {
                        @"reg delete ""HKCU\Software\Kakao\KakaoTalk"" /v ""AutoLogin"" /f",
                        @"reg delete ""HKCU\Software\Kakao\KakaoTalk"" /v ""SavePassword"" /f"
                    }
                ));

                artifacts.Add(await ScanSimpleAppPathsAsync(
                    id: "app-creative-cad",
                    title: "Adobe Creative Cloud & Autodesk (AutoCAD) 로그인 세션",
                    category: HandoverDomainCategory.AppSession,
                    categoryLabel: "디자인 & CAD",
                    description: "Adobe OOBE 인증 캐시와 Autodesk LoginState.xml 및 AdskIdentityManager 세션을 로그아웃 상태로 초기화합니다.",
                    processNames: new[] { "Creative Cloud", "Adobe Desktop Service", "Photoshop", "Acrobat", "acad", "AdskIdentityManager" },
                    userRoots: scanProfileRoots,
                    relativePaths: new[]
                    {
                        Path.Combine("AppData", "Local", "Adobe", "OOBE"),
                        Path.Combine("AppData", "Roaming", "Adobe", "OOBE"),
                        Path.Combine("AppData", "Local", "Autodesk", "Web Services", "LoginState.xml"),
                        Path.Combine("AppData", "Roaming", "Autodesk", "AdskIdentityManager")
                    },
                    credKeywords: new[] { "adobe", "autodesk", "adsk" },
                    credNames: credNames
                ));

                artifacts.Add(await ScanSimpleAppPathsAsync(
                    id: "app-clouddrive-dev",
                    title: "클라우드 드라이브 & 개발자 키 (OneDrive/GoogleDrive/Notion/.ssh/Git)",
                    category: HandoverDomainCategory.AppSession,
                    categoryLabel: "클라우드 & 개발",
                    description: "OneDrive/Google Drive/Dropbox/Notion 세션 및 Git 자격 증명, SSH 개인키(.ssh), AWS/GCloud CLI 토큰을 제거합니다.",
                    processNames: new[] { "GoogleDriveFS", "OneDrive", "Dropbox", "Notion", "GitHubDesktop" },
                    userRoots: scanProfileRoots,
                    relativePaths: new[]
                    {
                        Path.Combine("AppData", "Local", "Google", "DriveFS"),
                        Path.Combine("AppData", "Local", "Microsoft", "OneDrive", "settings"),
                        Path.Combine("AppData", "Local", "Dropbox"),
                        Path.Combine("AppData", "Roaming", "Notion", "Local Storage"),
                        Path.Combine("AppData", "Roaming", "GitHub Desktop"),
                        ".ssh",
                        ".aws",
                        Path.Combine("AppData", "Roaming", "gcloud")
                    },
                    credKeywords: new[] { "git", "github", "gitlab", "bitbucket", "azure", "aws" },
                    credNames: credNames
                ));

                artifacts.Add(await ScanSimpleAppPathsAsync(
                    id: "app-certs-gaming",
                    title: "금융/공공 인증서(NPKI/GPKI) & 게임 플랫폼 (Steam/Riot/Epic)",
                    category: HandoverDomainCategory.AppSession,
                    categoryLabel: "인증서 & 게임",
                    description: "공동인증서(NPKI/GPKI)와 Steam/Riot/Epic Games 자동 로그인 토큰을 소거합니다.",
                    processNames: new[] { "steam", "RiotClientServices", "EpicGamesLauncher", "LeagueClient" },
                    userRoots: scanProfileRoots,
                    relativePaths: new[]
                    {
                        Path.Combine("AppData", "LocalLow", "NPKI"),
                        Path.Combine("AppData", "LocalLow", "GPKI"),
                        Path.Combine("AppData", "Local", "Riot Games", "Riot Client", "Data"),
                        Path.Combine("AppData", "Local", "EpicGamesLauncher", "Saved", "Config")
                    },
                    extraAbsolutePaths: new[]
                    {
                        @"C:\NPKI",
                        @"C:\GPKI",
                        @"C:\Program Files (x86)\Steam\config\loginusers.vdf"
                    }
                ));

                artifacts.Add(await ScanSimpleAppPathsAsync(
                    id: "app-ai-dev-workstation",
                    title: "AI · 개발 도구 세션 (Cursor · VS Code · GitHub CLI · Docker · Postman · Figma)",
                    category: HandoverDomainCategory.AppSession,
                    categoryLabel: "AI & 개발 도구",
                    description: "Cursor/VS Code Copilot 인증 상태, GitHub CLI hosts.yml, Docker config, Postman/Figma 로그인 토큰을 소거합니다.",
                    processNames: new[] { "Cursor", "Code", "Docker Desktop", "Postman", "Figma" },
                    userRoots: scanProfileRoots,
                    relativePaths: new[]
                    {
                        Path.Combine("AppData", "Roaming", "Cursor", "User", "globalStorage"),
                        Path.Combine("AppData", "Roaming", "Code", "User", "globalStorage"),
                        Path.Combine("AppData", "Roaming", "GitHub CLI"),
                        ".docker",
                        Path.Combine("AppData", "Roaming", "Postman"),
                        Path.Combine("AppData", "Roaming", "Figma")
                    },
                    credKeywords: new[] { "vscode", "cursor", "github", "docker", "figma", "postman" },
                    credNames: credNames
                ));

                // 3. Windows OS 자격 증명, RDP/Wi-Fi/SMB 공유 및 셸 실행 이력
                artifacts.Add(await ScanSystemCredentialAndHistoryAsync(scanProfileRoots, credNames));

                // 4. 현재 사용자 개인 폴더 (바탕화면, 문서, 다운로드, 사진, 동영상) - FullMachineReset 시 선택
                artifacts.Add(await ScanPersonalFoldersAsync(currentUserProfile));

                // 5. 사용자 지정 보존 예외(Whitelist) 정책 평가
                var exclusions = HandoverPolicyService.LoadExclusions();
                if (exclusions.Count > 0)
                {
                    foreach (var art in artifacts)
                    {
                        if (HandoverPolicyService.IsArtifactExcluded(art, exclusions, out string reason))
                        {
                            art.IsExcludedByWhitelist = true;
                            art.ExclusionReason = reason;
                            art.IsSelected = false;
                            art.StatusText = reason;
                        }
                        else if (art.MatchedPaths.Count > 0)
                        {
                            int beforeCount = art.MatchedPaths.Count;
                            art.MatchedPaths = art.MatchedPaths
                                .Where(p => !HandoverPolicyService.IsPathExcluded(p, exclusions, out _))
                                .ToList();

                            if (art.MatchedPaths.Count < beforeCount)
                            {
                                long recalcBytes = 0;
                                foreach (var remPath in art.MatchedPaths)
                                {
                                    try
                                    {
                                        if (File.Exists(remPath))
                                        {
                                            recalcBytes += new FileInfo(remPath).Length;
                                        }
                                        else if (Directory.Exists(remPath) && !IsReparsePointPath(remPath))
                                        {
                                            recalcBytes += await RobocopyScannerService.GetFolderSizeFastAsync(remPath);
                                        }
                                    }
                                    catch { }
                                }
                                art.EstimatedBytes = recalcBytes;
                                if (art.MatchedPaths.Count == 0 && art.CredentialKeywords.Count == 0 && art.RegistryDeleteCommands.Count == 0)
                                {
                                    art.DetectedCount = 0;
                                    art.IsExcludedByWhitelist = true;
                                    art.ExclusionReason = "포함 경로 전체가 보존 예외로 설정됨";
                                    art.IsSelected = false;
                                    art.StatusText = art.ExclusionReason;
                                }
                                else
                                {
                                    int nonPathMatches = Math.Max(0, art.DetectedCount - beforeCount);
                                    art.DetectedCount = art.MatchedPaths.Count + nonPathMatches;
                                    art.StatusText = $"경로 {art.MatchedPaths.Count}건 (일부 보존 예외 제외됨)";
                                }
                            }
                        }
                    }
                }

                return artifacts;
            });
        }

        private static async Task<DetectedSessionArtifact> ScanChromiumMultiProfileAsync(
            string id,
            string title,
            string browserRelPath,
            string[] processNames,
            List<string> userRoots)
        {
            var matchedPaths = new List<string>();
            int profileFoldersCount = 0;
            long totalBytes = 0;

            foreach (var root in userRoots)
            {
                string userDataDir = Path.Combine(root, browserRelPath);
                if (!Directory.Exists(userDataDir)) continue;

                try
                {
                    var candidateProfiles = new List<string>();
                    string defaultDir = Path.Combine(userDataDir, "Default");
                    if (Directory.Exists(defaultDir)) candidateProfiles.Add(defaultDir);

                    foreach (var sub in Directory.GetDirectories(userDataDir, "Profile *"))
                    {
                        candidateProfiles.Add(sub);
                    }
                    string guestDir = Path.Combine(userDataDir, "Guest Profile");
                    if (Directory.Exists(guestDir)) candidateProfiles.Add(guestDir);

                    foreach (var profDir in candidateProfiles)
                    {
                        bool hasSessionData = false;
                        string[] sensitiveSubPaths =
                        {
                            Path.Combine(profDir, "Network", "Cookies"),
                            Path.Combine(profDir, "Network", "Cookies-journal"),
                            Path.Combine(profDir, "Cookies"),
                            Path.Combine(profDir, "Login Data"),
                            Path.Combine(profDir, "Login Data For Account"),
                            Path.Combine(profDir, "Web Data"),
                            Path.Combine(profDir, "Accounts"),
                            Path.Combine(profDir, "Sessions"),
                            Path.Combine(profDir, "Session Storage"),
                            Path.Combine(profDir, "Local Storage")
                        };

                        foreach (var sp in sensitiveSubPaths)
                        {
                            if (File.Exists(sp))
                            {
                                matchedPaths.Add(sp);
                                totalBytes += new FileInfo(sp).Length;
                                hasSessionData = true;
                            }
                            else if (Directory.Exists(sp))
                            {
                                matchedPaths.Add(sp);
                                totalBytes += await RobocopyScannerService.GetFolderSizeFastAsync(sp);
                                hasSessionData = true;
                            }
                        }

                        if (hasSessionData) profileFoldersCount++;
                    }
                }
                catch { }
            }

            return new DetectedSessionArtifact
            {
                ArtifactId = id,
                Title = title,
                DomainCategory = HandoverDomainCategory.BrowserSession,
                CategoryLabel = "브라우저 다중 프로필",
                UserScope = $"전체 사용자 ({userRoots.Count}개 프로필 스캔)",
                TargetPathsSummary = $@"%USERPROFILE%\{browserRelPath}\[Default / Profile *] (Cookies, Login Data, Accounts, Sessions)",
                Description = "모든 Windows 사용자 계정의 기본 및 보조 브라우저 프로필에 저장된 로그인 쿠키, 자동완성 비밀번호, 계정 토큰을 제거합니다.",
                AssociatedProcessNames = processNames,
                MatchedPaths = matchedPaths,
                DetectedCount = profileFoldersCount > 0 ? profileFoldersCount : matchedPaths.Count,
                EstimatedBytes = totalBytes,
                IsSelected = matchedPaths.Count > 0,
                StatusText = matchedPaths.Count > 0
                    ? $"{profileFoldersCount}개 프로필 세션 감지"
                    : "발견된 세션 없음"
            };
        }

        private static async Task<DetectedSessionArtifact> ScanSimpleAppPathsAsync(
            string id,
            string title,
            HandoverDomainCategory category,
            string categoryLabel,
            string description,
            string[] processNames,
            List<string> userRoots,
            string[] relativePaths,
            string[]? credKeywords = null,
            List<string>? credNames = null,
            string[]? extraAbsolutePaths = null,
            string[]? regDeleteCommands = null)
        {
            var matchedPaths = new List<string>();
            long totalBytes = 0;

            foreach (var root in userRoots)
            {
                foreach (var rel in relativePaths)
                {
                    string candidate = Path.Combine(root, rel);
                    if (IsPathProtected(candidate)) continue;

                    if (File.Exists(candidate) && !IsReparsePointPath(candidate))
                    {
                        matchedPaths.Add(candidate);
                        try { totalBytes += new FileInfo(candidate).Length; } catch { }
                    }
                    else if (Directory.Exists(candidate) && !IsReparsePointPath(candidate))
                    {
                        matchedPaths.Add(candidate);
                        totalBytes += await RobocopyScannerService.GetFolderSizeFastAsync(candidate);
                    }
                }
            }

            if (extraAbsolutePaths != null)
            {
                foreach (var abs in extraAbsolutePaths)
                {
                    if (IsPathProtected(abs) || IsReparsePointPath(abs)) continue;
                    if (File.Exists(abs))
                    {
                        matchedPaths.Add(abs);
                        try { totalBytes += new FileInfo(abs).Length; } catch { }
                    }
                    else if (Directory.Exists(abs))
                    {
                        matchedPaths.Add(abs);
                        totalBytes += await RobocopyScannerService.GetFolderSizeFastAsync(abs);
                    }
                }
            }

            int credMatches = 0;
            if (credKeywords != null && credNames != null)
            {
                credMatches = credNames.Count(c => credKeywords.Any(k => c.Contains(k, StringComparison.OrdinalIgnoreCase)));
            }

            int totalDetected = matchedPaths.Count + credMatches;

            return new DetectedSessionArtifact
            {
                ArtifactId = id,
                Title = title,
                DomainCategory = category,
                CategoryLabel = categoryLabel,
                UserScope = $"전체 사용자 ({userRoots.Count}개 계정)",
                TargetPathsSummary = string.Join(", ", relativePaths.Take(3)),
                Description = description,
                AssociatedProcessNames = processNames,
                MatchedPaths = matchedPaths,
                CredentialKeywords = credKeywords?.ToList() ?? new List<string>(),
                RegistryDeleteCommands = regDeleteCommands?.ToList() ?? new List<string>(),
                DetectedCount = totalDetected,
                EstimatedBytes = totalBytes,
                IsSelected = totalDetected > 0,
                StatusText = totalDetected > 0
                    ? $"경로 {matchedPaths.Count}건{(credMatches > 0 ? $" / 자격증명 {credMatches}건" : "")}"
                    : "발견된 세션 없음"
            };
        }

        private static async Task<DetectedSessionArtifact> ScanSystemCredentialAndHistoryAsync(
            List<string> userRoots,
            List<string> credNames)
        {
            var matchedPaths = new List<string>();
            long totalBytes = 0;

            foreach (var root in userRoots)
            {
                var shellTargets = new[]
                {
                    Path.Combine(root, "AppData", "Roaming", "Microsoft", "Windows", "Recent", "AutomaticDestinations"),
                    Path.Combine(root, "AppData", "Roaming", "Microsoft", "Windows", "Recent", "CustomDestinations")
                };

                foreach (var path in shellTargets)
                {
                    if (Directory.Exists(path))
                    {
                        matchedPaths.Add(path);
                        totalBytes += await RobocopyScannerService.GetFolderSizeFastAsync(path);
                    }
                }
            }

            int rdpEntryCount = 0;
            try
            {
                using var rdpKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Terminal Server Client\Servers");
                if (rdpKey != null) rdpEntryCount = rdpKey.GetSubKeyNames().Length;
            }
            catch { }

            int totalDetected = credNames.Count + matchedPaths.Count + rdpEntryCount;

            return new DetectedSessionArtifact
            {
                ArtifactId = "os-cred-rdp-mru",
                Title = "Windows 자격 증명 볼트 · RDP 접속 기록 · SMB 공유 · 점프리스트",
                DomainCategory = HandoverDomainCategory.CredentialAndNetwork,
                CategoryLabel = "자격 증명 & 이력",
                UserScope = "시스템 전역 & 전 사용자",
                TargetPathsSummary = @"Credential Vault (Generic/Domain), RDP Servers MRU, net use *, AutomaticDestinations",
                Description = "Windows 자격 증명 관리자의 저장 암호, 원격 데스크톱(RDP) 접속 서버 이력, 네트워크 드라이브(SMB) 인증, 최근 사용 점프리스트를 소거합니다.",
                AssociatedProcessNames = Array.Empty<string>(),
                MatchedPaths = matchedPaths,
                CredentialKeywords = new List<string> { "*" },
                RegistryDeleteCommands = new List<string>
                {
                    @"reg delete ""HKCU\Software\Microsoft\Terminal Server Client\Servers"" /f",
                    @"reg delete ""HKCU\Software\Microsoft\Terminal Server Client\Default"" /f",
                    @"reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU"" /f",
                    @"net use * /delete /y"
                },
                DetectedCount = totalDetected,
                EstimatedBytes = totalBytes,
                IsSelected = totalDetected > 0,
                StatusText = $"자격증명 {credNames.Count}건 / RDP {rdpEntryCount}건 / 이력 {matchedPaths.Count}건"
            };
        }

        private static async Task<DetectedSessionArtifact> ScanPersonalFoldersAsync(string currentUserProfile)
        {
            var matchedPaths = new List<string>();
            long totalBytes = 0;
            int filesCount = 0;

            if (!string.IsNullOrWhiteSpace(currentUserProfile) && Directory.Exists(currentUserProfile))
            {
                var personalDirs = new[] { "Desktop", "Documents", "Downloads", "Pictures", "Videos" };
                foreach (var folder in personalDirs)
                {
                    string target = Path.Combine(currentUserProfile, folder);
                    if (Directory.Exists(target) && !IsPathProtected(target))
                    {
                        matchedPaths.Add(target);
                        totalBytes += await RobocopyScannerService.GetFolderSizeFastAsync(target);
                        try
                        {
                            filesCount += Directory.EnumerateFileSystemEntries(target).Count();
                        }
                        catch { }
                    }
                }
            }

            return new DetectedSessionArtifact
            {
                ArtifactId = "current-personal-folders",
                Title = $"현재 사용자 개인 폴더 ({Path.GetFileName(currentUserProfile)}: 바탕화면·문서·다운로드 등)",
                DomainCategory = HandoverDomainCategory.PersonalFolder,
                CategoryLabel = "현재 계정 개인 파일",
                UserScope = Environment.UserName,
                TargetPathsSummary = $@"{currentUserProfile}\(Desktop, Documents, Downloads, Pictures, Videos)",
                Description = "PC를 타인에게 완전 양도할 때 현재 로그인 계정의 바탕화면·문서·다운로드 내 잔여 파일까지 일괄 비웁니다. (기본 모드에서는 안전을 위해 선택 해제)",
                AssociatedProcessNames = Array.Empty<string>(),
                MatchedPaths = matchedPaths,
                DetectedCount = filesCount,
                EstimatedBytes = totalBytes,
                IsSelected = false, // 기본 모드 보호: FullMachineReset 시에만 활성화
                StatusText = filesCount > 0 ? $"직접 항목 {filesCount}개 ({DetectedUserProfile.FormatBytes(totalBytes)})" : "개인 폴더 비어 있음"
            };
        }

        /// <summary>
        /// 시나리오 프리셋 일괄 적용
        /// </summary>
        public static void ApplyHandoverPreset(
            IEnumerable<DetectedUserProfile> profiles,
            IEnumerable<DetectedSessionArtifact> artifacts,
            HandoverPresetMode preset)
        {
            foreach (var p in profiles)
            {
                if (p.IsSystemProtected || p.IsCurrentSession)
                {
                    p.IsSelected = false;
                    continue;
                }

                p.IsSelected = preset != HandoverPresetMode.SessionsOnly;
            }

            foreach (var a in artifacts)
            {
                if (a.IsProtected || a.IsExcludedByWhitelist)
                {
                    a.IsSelected = false;
                    continue;
                }

                bool hasFindings = a.DetectedCount > 0 || a.EstimatedBytes > 0 || a.MatchedPaths.Count > 0;
                if (a.DomainCategory == HandoverDomainCategory.PersonalFolder)
                {
                    a.IsSelected = (preset == HandoverPresetMode.FullMachineReset) && hasFindings;
                }
                else
                {
                    a.IsSelected = hasFindings;
                }
            }
        }

        /// <summary>
        /// 선택된 이전 사용자 프로필 + 앱/브라우저/자격증명 세션 일괄 소거, 잠금 항목 재부팅 삭제 예약 및 2차 검증 실행
        /// </summary>
        public static async Task<HandoverVerificationSummary> ExecuteHandoverPurgeAsync(
            IEnumerable<DetectedUserProfile> profiles,
            IEnumerable<DetectedSessionArtifact> artifacts,
            bool terminateProcessesBeforePurge = true,
            bool createSafepoint = true,
            Action<string>? logger = null,
            bool schedulePendingReboot = true,
            string presetUsed = nameof(HandoverPresetMode.PreviousUsersAndSessions),
            IProgress<HandoverProgressReport>? progress = null)
        {
            return await Task.Run(async () =>
            {
                var exclusions = HandoverPolicyService.LoadExclusions();
                int excludedCount = artifacts.Count(a => a.IsExcludedByWhitelist || HandoverPolicyService.IsArtifactExcluded(a, exclusions, out _));

                var selectedProfiles = profiles
                    .Where(p => p.IsSelected && !p.IsSystemProtected && !p.IsCurrentSession && !HandoverPolicyService.IsPathExcluded(p.ProfilePath, exclusions, out _))
                    .ToList();
                var selectedArtifacts = artifacts
                    .Where(a => a.IsSelected && !a.IsProtected && !a.IsExcludedByWhitelist && !HandoverPolicyService.IsArtifactExcluded(a, exclusions, out _))
                    .ToList();

                long reclaimedBytes = 0;
                int purgedProfiles = 0;
                int purgedSessions = 0;
                bool hadUnresolvedErrors = false;
                var pendingRebootPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var lockedHints = new List<string>();
                var snapshots = new List<HandoverPurgedItemSnapshot>();

                void EmitProgress(int step, string title, int pct, string item, string? log = null)
                {
                    if (!string.IsNullOrEmpty(log)) logger?.Invoke(log);
                    progress?.Report(new HandoverProgressReport
                    {
                        StepIndex = step,
                        StepTitle = title,
                        Percent = pct,
                        CurrentItemLabel = item,
                        LogLine = log
                    });
                }

                if (createSafepoint)
                {
                    EmitProgress(1, "Live Safepoint 스냅샷 백업", 8, "레지스트리 스냅샷", "[Handover Safepoint] 소거 전 레지스트리 세이프포인트 스냅샷 생성 중...");
                    await LiveSafepointService.CreateSnapshotDetailedAsync("WinPurify Pro Fresh Handover Pre-Purge");
                }

                // 1. 잠금 프로세스 선행 종료
                if (terminateProcessesBeforePurge)
                {
                    EmitProgress(1, "잠금 프로세스 안전 해제", 16, "브라우저·메신저·개발도구 종료", "[Handover Step 1/4] 파일·세션 잠금 해제를 위한 브라우저 및 앱 프로세스 안전 종료 중...");
                    int currentPid = Process.GetCurrentProcess().Id;
                    var procNames = selectedArtifacts
                        .SelectMany(a => a.AssociatedProcessNames)
                        .Distinct(StringComparer.OrdinalIgnoreCase);

                    foreach (string procName in procNames)
                    {
                        try
                        {
                            foreach (var proc in Process.GetProcessesByName(procName))
                            {
                                using (proc)
                                {
                                    if (proc.Id == currentPid) continue;
                                    try
                                    {
                                        proc.Kill(true);
                                        proc.WaitForExit(1500);
                                    }
                                    catch { }
                                }
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    EmitProgress(1, "잠금 프로세스 확인 생략", 16, "사용자 옵션", "[Handover Step 1/4] 프로세스 선종료 옵션이 꺼져 있어 직접 소거로 진행합니다.");
                }

                // 2. 선택된 앱/브라우저 다중 프로필/자격증명/개인폴더 소거
                int totalArts = Math.Max(1, selectedArtifacts.Count);
                EmitProgress(2, "앱·브라우저·자격증명 소거", 20, $"{selectedArtifacts.Count}개 영역", $"[Handover Step 2/4] 선택된 세션·앱 데이터 {selectedArtifacts.Count}개 영역 소거 시작...");
                for (int idx = 0; idx < selectedArtifacts.Count; idx++)
                {
                    var artifact = selectedArtifacts[idx];
                    int pct = 20 + (int)((idx + 1) * 45.0 / totalArts);
                    EmitProgress(2, "앱·브라우저·자격증명 소거", pct, artifact.Title);
                    artifact.StatusText = "소거 중...";
                    artifact.PendingRebootPaths.Clear();
                    var artifactLockedPaths = new List<string>();
                    long itemBytes = artifact.EstimatedBytes;
                    string pathsSummary = artifact.TargetPathsSummary;

                    try
                    {
                        if (artifact.DomainCategory == HandoverDomainCategory.PersonalFolder)
                        {
                            foreach (var dir in artifact.MatchedPaths)
                            {
                                if (!IsPathProtected(dir) && !HandoverPolicyService.IsPathExcluded(dir, exclusions, out _) && Directory.Exists(dir))
                                {
                                    EmptyDirectoryContentsPreservingRoot(dir, artifactLockedPaths);
                                }
                            }
                        }
                        else
                        {
                            foreach (var targetPath in artifact.MatchedPaths)
                            {
                                if (IsPathProtected(targetPath) || HandoverPolicyService.IsPathExcluded(targetPath, exclusions, out _)) continue;
                                if (File.Exists(targetPath))
                                {
                                    if (!NativeEngineService.TryUnlockAndDelete(targetPath, terminateProcessesBeforePurge) && File.Exists(targetPath))
                                    {
                                        artifactLockedPaths.Add(targetPath);
                                    }
                                }
                                else if (Directory.Exists(targetPath))
                                {
                                    ForceDeleteDirectory(targetPath, terminateProcessesBeforePurge, artifactLockedPaths);
                                }
                            }
                        }

                        if (artifact.CredentialKeywords.Count > 0)
                        {
                            if (artifact.CredentialKeywords.Contains("*"))
                            {
                                NativeEngineService.TryPurifyCredentials("all");
                                DeleteCredentialsByKeywords(Array.Empty<string>(), deleteAllGeneric: true);
                            }
                            else
                            {
                                foreach (var kw in artifact.CredentialKeywords)
                                {
                                    NativeEngineService.TryPurifyCredentials(kw);
                                }
                                DeleteCredentialsByKeywords(artifact.CredentialKeywords, deleteAllGeneric: false);
                            }
                        }

                        foreach (var cmd in artifact.RegistryDeleteCommands)
                        {
                            ExecuteCleanupRuleWithoutShell(cmd);
                        }

                        int scheduledForArtifact = 0;
                        int unscheduledForArtifact = 0;
                        foreach (var locked in artifactLockedPaths.Distinct(StringComparer.OrdinalIgnoreCase))
                        {
                            if (schedulePendingReboot && HandoverPolicyService.SchedulePendingFileDelete(locked, out var norm))
                            {
                                pendingRebootPaths.Add(norm);
                                artifact.PendingRebootPaths.Add(norm);
                                scheduledForArtifact++;
                            }
                            else
                            {
                                unscheduledForArtifact++;
                            }
                        }

                        if (artifactLockedPaths.Count > 0)
                        {
                            string runningProcs = FindRunningAssociatedProcesses(artifact.AssociatedProcessNames);
                            string hint = string.IsNullOrEmpty(runningProcs)
                                ? $"{artifact.Title}: 시스템 서비스/커널 핸들 잠금 ({artifactLockedPaths.Count}건)"
                                : $"{artifact.Title}: 실행 중 프로세스({runningProcs}) 점유";
                            artifact.LockedHints = hint;
                            lockedHints.Add(hint);
                        }

                        artifact.IsSelected = false;

                        if (artifactLockedPaths.Count == 0)
                        {
                            reclaimedBytes += itemBytes;
                            purgedSessions++;
                            artifact.DetectedCount = 0;
                            artifact.EstimatedBytes = 0;
                            artifact.MatchedPaths.Clear();
                            artifact.StatusText = "소거 완료";
                            logger?.Invoke($"[Handover 완료] {artifact.Title}");
                            snapshots.Add(new HandoverPurgedItemSnapshot { Title = artifact.Title, Category = artifact.CategoryLabel, EstimatedBytes = itemBytes, Status = "소거 완료", PathsSummary = pathsSummary });
                        }
                        else if (unscheduledForArtifact == 0 && scheduledForArtifact > 0)
                        {
                            reclaimedBytes += itemBytes;
                            purgedSessions++;
                            artifact.StatusText = $"재부팅 삭제 예약 ({scheduledForArtifact}건)";
                            logger?.Invoke($"[Handover 재부팅 예약] {artifact.Title}: 잠금 {scheduledForArtifact}건을 PendingFileRenameOperations로 예약했습니다.");
                            snapshots.Add(new HandoverPurgedItemSnapshot { Title = artifact.Title, Category = artifact.CategoryLabel, EstimatedBytes = itemBytes, Status = $"재부팅 예약 ({scheduledForArtifact}건)", PathsSummary = pathsSummary });
                        }
                        else
                        {
                            hadUnresolvedErrors = true;
                            artifact.StatusText = "일부 잠금 항목 확인 필요";
                            logger?.Invoke($"[Handover 경고] {artifact.Title}: 일부 파일이 잠겨 있어 완전히 삭제되지 않았습니다.");
                            snapshots.Add(new HandoverPurgedItemSnapshot { Title = artifact.Title, Category = artifact.CategoryLabel, EstimatedBytes = 0, Status = "일부 잠금 잔존", PathsSummary = pathsSummary });
                        }
                    }
                    catch (Exception ex)
                    {
                        hadUnresolvedErrors = true;
                        artifact.StatusText = $"일부 오류: {ex.Message}";
                        logger?.Invoke($"[Handover 경고] {artifact.Title} 처리 중 예외: {ex.Message}");
                        snapshots.Add(new HandoverPurgedItemSnapshot { Title = artifact.Title, Category = artifact.CategoryLabel, EstimatedBytes = itemBytes, Status = $"오류: {ex.Message}", PathsSummary = pathsSummary });
                    }
                }

                // 3. 비활성 이전 Windows 사용자 프로필 및 SID 레지스트리 완전 제거
                int totalProfs = Math.Max(1, selectedProfiles.Count);
                EmitProgress(3, "이전 사용자 프로필·SID 제거", 68, $"{selectedProfiles.Count}개 계정", $"[Handover Step 3/4] 이전 Windows 사용자 프로필 {selectedProfiles.Count}개 제거 중...");
                for (int idx = 0; idx < selectedProfiles.Count; idx++)
                {
                    var profile = selectedProfiles[idx];
                    if (IsPathProtected(profile.ProfilePath) || profile.IsCurrentSession) continue;

                    int pct = 68 + (int)((idx + 1) * 20.0 / totalProfs);
                    EmitProgress(3, "이전 사용자 프로필·SID 제거", pct, profile.Username);
                    profile.StatusText = "프로필 제거 중...";
                    var profileLocked = new List<string>();
                    long profBytes = profile.EstimatedBytes;

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(profile.Sid) && StrictDomainSidRegex.IsMatch(profile.Sid))
                        {
                            RunDirectProcess("reg.exe", new[] { "unload", $@"HKU\{profile.Sid}" });
                            RunDirectProcess("reg.exe", new[] { "unload", $@"HKU\{profile.Sid}_Classes" });

                            bool deletedViaApi = false;
                            try
                            {
                                deletedViaApi = DeleteProfileW(profile.Sid, profile.ProfilePath, null);
                            }
                            catch { }

                            if (!deletedViaApi)
                            {
                                RunDirectProcess("powershell.exe", new[]
                                {
                                    "-NoProfile",
                                    "-Command",
                                    $"Get-CimInstance Win32_UserProfile | Where-Object {{ $_.SID -eq '{profile.Sid}' -and -not $_.Special -and -not $_.Loaded }} | Remove-CimInstance -ErrorAction SilentlyContinue"
                                });
                            }
                        }

                        bool dirClean = true;
                        if (Directory.Exists(profile.ProfilePath) && !IsPathProtected(profile.ProfilePath))
                        {
                            dirClean = ForceDeleteDirectory(profile.ProfilePath, terminateProcessesBeforePurge, profileLocked);
                        }

                        if (dirClean && !Directory.Exists(profile.ProfilePath))
                        {
                            reclaimedBytes += profBytes;
                            purgedProfiles++;
                            profile.EstimatedBytes = 0;
                            profile.IsSelected = false;
                            profile.StatusText = "완전 제거됨";
                            logger?.Invoke($"[Handover 프로필 제거] '{profile.Username}' ({profile.ProfilePath}) 소거 완료");
                            snapshots.Add(new HandoverPurgedItemSnapshot { Title = $"Windows 프로필: {profile.Username}", Category = "사용자 프로필", EstimatedBytes = profBytes, Status = "완전 제거됨", PathsSummary = profile.ProfilePath });
                        }
                        else
                        {
                            int queued = 0;
                            if (schedulePendingReboot)
                            {
                                foreach (var lp in profileLocked.Distinct(StringComparer.OrdinalIgnoreCase))
                                {
                                    if (HandoverPolicyService.SchedulePendingFileDelete(lp, out var norm))
                                    {
                                        pendingRebootPaths.Add(norm);
                                        queued++;
                                    }
                                }
                                if (Directory.Exists(profile.ProfilePath) && HandoverPolicyService.SchedulePendingFileDelete(profile.ProfilePath, out var normDir))
                                {
                                    pendingRebootPaths.Add(normDir);
                                    queued++;
                                }
                            }

                            profile.IsSelected = false;
                            if (queued > 0)
                            {
                                profile.PendingRebootScheduled = true;
                                profile.StatusText = $"재부팅 삭제 예약 ({queued}건)";
                                purgedProfiles++;
                                reclaimedBytes += profBytes;
                                lockedHints.Add($"프로필 '{profile.Username}': 잠금 파일 {queued}건 재부팅 시 자동 삭제 예약됨");
                                logger?.Invoke($"[Handover 재부팅 예약] 프로필 '{profile.Username}' 잠금 항목 {queued}건을 재부팅 삭제 대기열에 등록했습니다.");
                                snapshots.Add(new HandoverPurgedItemSnapshot { Title = $"Windows 프로필: {profile.Username}", Category = "사용자 프로필", EstimatedBytes = profBytes, Status = $"재부팅 예약 ({queued}건)", PathsSummary = profile.ProfilePath });
                            }
                            else
                            {
                                hadUnresolvedErrors = true;
                                profile.StatusText = "일부 파일 잠김 (수동 확인 필요)";
                                logger?.Invoke($"[Handover 경고] 프로필 '{profile.Username}' 일부 파일이 잠겨 남아 있습니다.");
                                snapshots.Add(new HandoverPurgedItemSnapshot { Title = $"Windows 프로필: {profile.Username}", Category = "사용자 프로필", EstimatedBytes = 0, Status = "일부 파일 잠김", PathsSummary = profile.ProfilePath });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        hadUnresolvedErrors = true;
                        profile.StatusText = $"제거 오류: {ex.Message}";
                        logger?.Invoke($"[Handover 경고] 프로필 '{profile.Username}' 제거 실패: {ex.Message}");
                    }
                }

                // 4. 2차 무결성 검증 재스캔 (디스크 및 자격 증명 실제 재확인)
                EmitProgress(4, "2차 무결성 검증 재스캔", 92, "잔여 흔적 교차 확인", "[Handover Step 4/4] 2차 무결성 검증 재스캔 실행 중...");
                var postProfiles = await ScanUserProfilesAsync(null);
                var postArtifacts = await ScanSessionArtifactsAsync(postProfiles, null);

                var selectedProfilePaths = new HashSet<string>(selectedProfiles.Select(p => p.ProfilePath), StringComparer.OrdinalIgnoreCase);
                int remainingSelectedProfiles = postProfiles.Count(p => selectedProfilePaths.Contains(p.ProfilePath));
                int remainingPrev = postProfiles.Count(p => !p.IsSystemProtected && !p.IsCurrentSession);

                var postArtifactMap = postArtifacts.ToDictionary(a => a.ArtifactId, StringComparer.OrdinalIgnoreCase);

                int remainingSessions = 0;
                foreach (var art in selectedArtifacts)
                {
                    if (art.DomainCategory == HandoverDomainCategory.PersonalFolder) continue;
                    if (postArtifactMap.TryGetValue(art.ArtifactId, out var rescanned) && !rescanned.IsExcludedByWhitelist && rescanned.DetectedCount > 0)
                    {
                        remainingSessions++;
                        art.DetectedCount = rescanned.DetectedCount;
                        art.EstimatedBytes = rescanned.EstimatedBytes;
                        art.MatchedPaths = rescanned.MatchedPaths;
                        if (art.PendingRebootPaths.Count == 0)
                        {
                            art.StatusText = $"잔여 항목 {rescanned.DetectedCount}건 확인";
                        }
                    }
                }

                bool remainingPersonal = selectedArtifacts.Any(a => a.DomainCategory == HandoverDomainCategory.PersonalFolder)
                    && postArtifactMap.TryGetValue("current-personal-folders", out var postPersonal)
                    && !postPersonal.IsExcludedByWhitelist
                    && postPersonal.DetectedCount > 0;

                bool hasPendingReboot = pendingRebootPaths.Count > 0;
                bool cleanVerified = remainingSelectedProfiles == 0 && remainingSessions == 0 && !remainingPersonal && !hadUnresolvedErrors && !hasPendingReboot;

                string summaryMsg;
                if (cleanVerified)
                {
                    summaryMsg = $"새 사용자 환경 인수인계 검증 완료: 프로필 {purgedProfiles}개 · 세션 {purgedSessions}건 소거 ({DetectedUserProfile.FormatBytes(reclaimedBytes)} 확보 · 예외 보존 {excludedCount}건)";
                }
                else if (hasPendingReboot && !hadUnresolvedErrors)
                {
                    summaryMsg = $"초기화 및 재부팅 자동 소거 예약 완료: 프로필 {purgedProfiles}개 · 세션 {purgedSessions}건 처리, 잠금 {pendingRebootPaths.Count}건 재부팅 시 자동 제거 대기";
                }
                else
                {
                    summaryMsg = $"인수인계 정화 완료 (확인 필요): 미제거 선택 프로필 {remainingSelectedProfiles}개 · 잔여 세션 {remainingSessions}건 · 재부팅 예약 {pendingRebootPaths.Count}건";
                }

                EmitProgress(4, "검증 완료", 100, "완료", $"[Handover 검증] {summaryMsg}");

                return new HandoverVerificationSummary
                {
                    CompletedAt = DateTime.Now,
                    MachineName = Environment.MachineName,
                    OperatorUser = Environment.UserName,
                    PresetUsed = presetUsed,
                    PurgedProfilesCount = purgedProfiles,
                    PurgedSessionsCount = purgedSessions,
                    ReclaimedBytes = reclaimedBytes,
                    RemainingPreviousProfiles = remainingPrev,
                    RemainingActiveSessions = remainingSessions,
                    ExcludedItemsCount = excludedCount,
                    IsCleanHandoverVerified = cleanVerified,
                    SummaryMessage = summaryMsg,
                    PendingRebootPaths = pendingRebootPaths.OrderBy(p => p).ToList(),
                    LockedProcessHints = lockedHints.Distinct().ToList(),
                    PurgedItemsSnapshot = snapshots
                };
            });
        }

        /// <summary>
        /// 무인 CLI 인수인계 모드 (--handover-auto / --zero-trace) 실행 엔트리
        /// </summary>
        public static async Task<int> ExecuteHeadlessHandoverAsync(string[] args)
        {
            var preset = HandoverPresetMode.PreviousUsersAndSessions;
            foreach (string arg in args)
            {
                if (arg.Equals("--sessions-only", StringComparison.OrdinalIgnoreCase))
                {
                    preset = HandoverPresetMode.SessionsOnly;
                }
                else if (arg.Equals("--full-reset", StringComparison.OrdinalIgnoreCase))
                {
                    preset = HandoverPresetMode.FullMachineReset;
                }
            }

            LoggerService.Instance.LogMain($"[Headless Handover] 자동 클린 인수인계 시작 (Preset: {preset})");
            var profiles = await ScanUserProfilesAsync(m => LoggerService.Instance.LogMain(m));
            var artifacts = await ScanSessionArtifactsAsync(profiles, m => LoggerService.Instance.LogMain(m));
            ApplyHandoverPreset(profiles, artifacts, preset);

            var summary = await ExecuteHandoverPurgeAsync(
                profiles,
                artifacts,
                terminateProcessesBeforePurge: true,
                createSafepoint: true,
                logger: m => LoggerService.Instance.LogMain(m)
            );

            LoggerService.Instance.LogMain($"[Headless Handover 완료] {summary.SummaryMessage}");
            return summary.IsCleanHandoverVerified ? 0 : 2;
        }

        // ====================================================================
        // Internal Win32 / Filesystem / Registry Helpers
        // ====================================================================

        private static string? TryResolveAccountNameFromSid(string sidString)
        {
            try
            {
                var sid = new SecurityIdentifier(sidString);
                var ntAccount = sid.Translate(typeof(NTAccount)) as NTAccount;
                return ntAccount?.Value;
            }
            catch
            {
                return null;
            }
        }

        private static string DetermineAccountType(string sid, string username)
        {
            if (ProtectedSystemSids.Contains(sid)) return "OS 시스템 서비스";
            if (username.Contains("AzureAD\\", StringComparison.OrdinalIgnoreCase)) return "Microsoft Entra / MS 계정";
            if (username.Contains("\\")) return "로컬/도메인 계정";
            return "사용자 계정";
        }

        private static DateTime? ReadProfileLoadTime(RegistryKey profileKey)
        {
            try
            {
                object? highObj = profileKey.GetValue("LocalProfileLoadTimeHigh");
                object? lowObj = profileKey.GetValue("LocalProfileLoadTimeLow");
                if (highObj is int high && lowObj is int low)
                {
                    long fileTime = ((long)(uint)high << 32) | (uint)low;
                    if (fileTime > 0)
                    {
                        return DateTime.FromFileTime(fileTime);
                    }
                }
            }
            catch { }
            return null;
        }

        private static int CountAppDataSubdirs(string profilePath)
        {
            int count = 0;
            try
            {
                string local = Path.Combine(profilePath, "AppData", "Local");
                string roaming = Path.Combine(profilePath, "AppData", "Roaming");
                if (Directory.Exists(local)) count += Directory.GetDirectories(local).Length;
                if (Directory.Exists(roaming)) count += Directory.GetDirectories(roaming).Length;
            }
            catch { }
            return count;
        }

        private static List<string> GetAllWindowsCredentialNames()
        {
            var list = new List<string>();
            IntPtr pCreds = IntPtr.Zero;
            try
            {
                if (CredEnumerateW(null, 0, out uint count, out pCreds) && pCreds != IntPtr.Zero)
                {
                    int ptrSize = Marshal.SizeOf<IntPtr>();
                    for (int i = 0; i < count; i++)
                    {
                        IntPtr credPtr = Marshal.ReadIntPtr(pCreds, i * ptrSize);
                        if (credPtr == IntPtr.Zero) continue;
                        var cred = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
                        if (cred.TargetName != IntPtr.Zero)
                        {
                            string? targetName = Marshal.PtrToStringUni(cred.TargetName);
                            if (!string.IsNullOrWhiteSpace(targetName))
                            {
                                list.Add(targetName);
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                if (pCreds != IntPtr.Zero)
                {
                    try { CredFree(pCreds); } catch { }
                }
            }
            return list;
        }

        private static void DeleteCredentialsByKeywords(IEnumerable<string> keywords, bool deleteAllGeneric)
        {
            IntPtr pCreds = IntPtr.Zero;
            try
            {
                var kwList = keywords.ToList();
                if (!CredEnumerateW(null, 0, out uint count, out pCreds) || pCreds == IntPtr.Zero)
                {
                    return;
                }

                int ptrSize = Marshal.SizeOf<IntPtr>();
                var toDelete = new List<(string Target, uint Type)>();

                for (int i = 0; i < count; i++)
                {
                    IntPtr credPtr = Marshal.ReadIntPtr(pCreds, i * ptrSize);
                    if (credPtr == IntPtr.Zero) continue;
                    var cred = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
                    if (cred.TargetName == IntPtr.Zero) continue;

                    string? target = Marshal.PtrToStringUni(cred.TargetName);
                    if (string.IsNullOrWhiteSpace(target)) continue;

                    if (deleteAllGeneric || kwList.Any(k => target.Contains(k, StringComparison.OrdinalIgnoreCase)))
                    {
                        toDelete.Add((target, cred.Type));
                    }
                }

                foreach (var item in toDelete)
                {
                    try
                    {
                        if (!CredDeleteW(item.Target, item.Type, 0))
                        {
                            CredDeleteW(item.Target, CRED_TYPE_GENERIC, 0);
                            CredDeleteW(item.Target, CRED_TYPE_DOMAIN_PASSWORD, 0);
                        }
                    }
                    catch { }
                }
            }
            catch { }
            finally
            {
                if (pCreds != IntPtr.Zero)
                {
                    try { CredFree(pCreds); } catch { }
                }
            }
        }

        private static string FindRunningAssociatedProcesses(string[] processNames)
        {
            if (processNames == null || processNames.Length == 0) return string.Empty;
            var active = new List<string>();
            foreach (var name in processNames)
            {
                try
                {
                    var procs = Process.GetProcessesByName(name);
                    if (procs.Length > 0)
                    {
                        active.Add(name);
                    }
                    foreach (var p in procs) p.Dispose();
                }
                catch { }
            }
            return string.Join(", ", active);
        }

        public static bool ForceDeleteDirectory(string dirPath, bool terminateLockers, List<string>? lockedCollector = null)
        {
            if (IsPathProtected(dirPath)) return false;
            if (!Directory.Exists(dirPath)) return true;

            if (IsReparsePointPath(dirPath))
            {
                try
                {
                    Directory.Delete(dirPath, false);
                    return !Directory.Exists(dirPath);
                }
                catch
                {
                    return false;
                }
            }

            try
            {
                foreach (string file in Directory.GetFiles(dirPath))
                {
                    if (IsReparsePointPath(file))
                    {
                        try { File.Delete(file); } catch { }
                        continue;
                    }
                    if (!NativeEngineService.TryUnlockAndDelete(file, terminateLockers) && File.Exists(file))
                    {
                        lockedCollector?.Add(file);
                    }
                }

                foreach (string sub in Directory.GetDirectories(dirPath))
                {
                    ForceDeleteDirectory(sub, terminateLockers, lockedCollector);
                }

                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, false);
                }
            }
            catch { }

            bool gone = !Directory.Exists(dirPath);
            if (!gone && lockedCollector != null)
            {
                lockedCollector.Add(dirPath);
            }
            return gone;
        }

        private static bool EmptyDirectoryContentsPreservingRoot(string dirPath, List<string>? lockedCollector = null)
        {
            if (IsPathProtected(dirPath)) return false;
            if (!Directory.Exists(dirPath) || IsReparsePointPath(dirPath)) return false;
            bool allClean = true;
            try
            {
                foreach (string file in Directory.GetFiles(dirPath))
                {
                    if (IsReparsePointPath(file))
                    {
                        try { File.Delete(file); } catch { }
                        continue;
                    }
                    if (!NativeEngineService.TryUnlockAndDelete(file, terminateLockingProcess: false) && File.Exists(file))
                    {
                        allClean = false;
                        lockedCollector?.Add(file);
                    }
                }
                foreach (string sub in Directory.GetDirectories(dirPath))
                {
                    if (!ForceDeleteDirectory(sub, terminateLockers: false, lockedCollector))
                    {
                        allClean = false;
                    }
                }
            }
            catch
            {
                allClean = false;
            }
            return allClean;
        }

        private static void ExecuteCleanupRuleWithoutShell(string rule)
        {
            if (string.IsNullOrWhiteSpace(rule)) return;
            try
            {
                var tokens = SplitCommandTokens(rule);
                if (tokens.Count == 0) return;

                string exe = tokens[0].ToLowerInvariant();
                if (exe == "reg" || exe == "reg.exe")
                {
                    RunDirectProcess("reg.exe", tokens.Skip(1));
                }
                else if (exe == "net" || exe == "net.exe")
                {
                    RunDirectProcess("net.exe", tokens.Skip(1));
                }
            }
            catch { }
        }

        private static List<string> SplitCommandTokens(string command)
        {
            var tokens = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            foreach (char c in command)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }
                if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                    continue;
                }
                current.Append(c);
            }

            if (current.Length > 0)
            {
                tokens.Add(current.ToString());
            }
            return tokens;
        }

        private static void RunDirectProcess(string fileName, IEnumerable<string> arguments)
        {
            try
            {
                var psi = new ProcessStartInfo(fileName)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                foreach (var arg in arguments)
                {
                    psi.ArgumentList.Add(arg);
                }
                using var proc = Process.Start(psi);
                proc?.WaitForExit(5000);
            }
            catch { }
        }
    }
}
