using System.Collections.Generic;
using WinPurifyPro.Models;

namespace WinPurifyPro
{
    public static class TaskDataSeeder
    {
        public static List<OptimizationTask> GetAllTasks()
        {
            var list = new List<OptimizationTask>();

            // ==========================================
            // 1. 개인정보 보호 (Privacy - 20 Modules)
            // ==========================================
            list.Add(new OptimizationTask
            {
                Id = "pri-telemetry",
                Name = "진단/텔레메트리 데이터",
                Description = "Windows 진단 및 원격 측정 설정 잔여 캐시 삭제",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\ProgramData\Microsoft\Diagnosis\DownloadedSettings",
                FastCliCommand = @"del /f /q /s ""C:\ProgramData\Microsoft\Diagnosis\DownloadedSettings\*""",
                PowerShellCommand = @"Remove-Item ""C:\ProgramData\Microsoft\Diagnosis\DownloadedSettings\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-quick-access",
                Name = "바로 가기 / 최근 항목",
                Description = "탐색기 바로 가기 및 최근 접근 파일 링크 초기화",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:APPDATA\Microsoft\Windows\Recent",
                FastCliCommand = @"del /f /q /s ""%AppData%\Microsoft\Windows\Recent\*""",
                PowerShellCommand = @"Remove-Item ""$env:APPDATA\Microsoft\Windows\Recent\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-search-history",
                Name = "Windows 검색 기록",
                Description = "작업 표시줄 및 시작 메뉴 검색 키워드 기록 삭제",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Search\RecentSearches"" /f",
                PowerShellCommand = @"Remove-ItemProperty -Path ""HKCU:\Software\Microsoft\Windows\CurrentVersion\Search\RecentSearches"" -Name ""*"" -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-shell-bags",
                Name = "폴더 뷰(Shell Bags) 초기화",
                Description = "탐색기 폴더 위치 및 크기 캐시 레지스트리 초기화",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKCU\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags"" /f & reg delete ""HKCU\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU"" /f",
                PowerShellCommand = @"Remove-Item -Path ""HKCU:\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags"" -Recurse -Force; Remove-Item -Path ""HKCU:\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-jump-lists",
                Name = "작업 표시줄 점프 목록",
                Description = "작업 표시줄 아이콘 우클릭 시 표시되는 최근 고정 목록 초기화",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:APPDATA\Microsoft\Windows\Recent\AutomaticDestinations",
                FastCliCommand = @"del /f /q /s ""%AppData%\Microsoft\Windows\Recent\AutomaticDestinations\*""",
                PowerShellCommand = @"Remove-Item ""$env:APPDATA\Microsoft\Windows\Recent\AutomaticDestinations\*"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-run-mru",
                Name = "실행/검색 MRU 기록",
                Description = "Windows 실행(Win+R) 대화상자 명령어 입력 기록 삭제",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU"" /f",
                PowerShellCommand = @"Remove-ItemProperty -Path ""HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU"" -Name ""*"" -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-recent-docs",
                Name = "레지스트리 최근 문서 목록",
                Description = "레지스트리에 기록된 최근 열어본 문서 참조 기록 삭제",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs"" /f",
                PowerShellCommand = @"Remove-Item -Path ""HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-clipboard",
                Name = "클립보드 기록 비우기",
                Description = "시스템 클립보드에 복사된 텍스트 및 미디어 캐시 소거",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"cmd /c ""echo off | clip""",
                PowerShellCommand = @"echo off | clip",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-typed-paths",
                Name = "탐색기 주소창 입력 기록",
                Description = "파일 탐색기 상단 주소 표시줄에 직접 타이핑한 경로 목록 소거",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths"" /f",
                PowerShellCommand = @"Remove-ItemProperty -Path ""HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths"" -Name ""*""",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-user-assist",
                Name = "UserAssist 앱 실행 통계",
                Description = "Windows 내부에서 앱 실행 빈도를 암호화 기록하는 UserAssist 키 초기화",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\UserAssist"" /f",
                PowerShellCommand = @"Get-ChildItem ""HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\UserAssist"" | Remove-Item -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-activity-timeline",
                Name = "활동 타임라인 기록",
                Description = "Windows 타임라인 사용자 활동 기록 및 캐시 삭제",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\ConnectedDevicesPlatform",
                FastCliCommand = @"sc stop ""CDPSvc"" & del /f /q /s ""%LocalAppData%\ConnectedDevicesPlatform\*"" & sc start ""CDPSvc""",
                PowerShellCommand = @"Get-Service -Name ""CDPSvc"" | Stop-Service -Force; Remove-Item ""$env:LOCALAPPDATA\ConnectedDevicesPlatform\*"" -Recurse -Force; Start-Service ""CDPSvc""",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-office-mru",
                Name = "Office 최근 사용 목록",
                Description = "Microsoft Office 제품군의 최근 열어본 파일 기록 삭제",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                PowerShellCommand = @"Get-ChildItem ""HKCU:\Software\Microsoft\Office\*\*\User MRU"" | Remove-Item -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-winrar-mru",
                Name = "WinRAR 압축 해제 기록",
                Description = "WinRAR에서 최근 압축/해제한 파일 및 폴더 기록 삭제",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKCU\Software\WinRAR\ArcHistory"" /f",
                PowerShellCommand = @"Remove-Item -Path ""HKCU:\Software\WinRAR\ArcHistory"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-paint-mru",
                Name = "그림판 최근 목록",
                Description = "그림판 앱의 최근 작업 이미지 파일 목록 삭제",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Applets\Paint\Recent File List"" /f",
                PowerShellCommand = @"Remove-Item -Path ""HKCU:\Software\Microsoft\Windows\CurrentVersion\Applets\Paint\Recent File List"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-wordpad-mru",
                Name = "워드패드 최근 목록",
                Description = "워드패드 앱의 최근 작업 문서 목록 삭제",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Applets\Wordpad\Recent File List"" /f",
                PowerShellCommand = @"Remove-Item -Path ""HKCU:\Software\Microsoft\Windows\CurrentVersion\Applets\Wordpad\Recent File List"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-wallpaper-mru",
                Name = "배경화면 변경 기록",
                Description = "설정 앱의 배경화면 최근 선택 기록 초기화",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                PowerShellCommand = @"Remove-ItemProperty -Path ""HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers"" -Name ""BackgroundHistoryPath*""",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-explorer-breadcrumb",
                Name = "탐색기 이동 경로 캐시",
                Description = "탐색기 상단 브레드크럼 네비게이션 캐시 정리",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths"" /f",
                PowerShellCommand = @"Remove-ItemProperty -Path ""HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths"" -Name ""*""",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-taskbar-mru",
                Name = "작업 표시줄 고정 링크 정리",
                Description = "작업 표시줄 핀 고정 바로가기 캐시 초기화",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Deep,
                ScanTargetFolder = @"$env:APPDATA\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar",
                FastCliCommand = @"del /f /q /s ""%AppData%\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar\*""",
                PowerShellCommand = @"Remove-Item ""$env:APPDATA\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar\*"" -Force",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-thumb-global",
                Name = "전체 썸네일 데이터베이스",
                Description = "손상되었거나 누적된 탐색기 미리보기 썸네일 DB 파일 일괄 초기화",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Microsoft\Windows\Explorer",
                FastCliCommand = @"taskkill /F /IM explorer.exe & del /s /q /f /a s %LocalAppData%\Microsoft\Windows\Explorer\thumbcache_*.db & start explorer.exe",
                PowerShellCommand = @"taskkill /F /IM explorer.exe; del /s /q /f /a s %LocalAppData%\Microsoft\Windows\Explorer\thumbcache_*.db; start explorer.exe",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-generic-mru-list",
                Name = "범용 최근 사용 목록",
                Description = "기타 서드파티 및 시스템 공통 MRU 레지스트리 잔여 기록 소거",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                PowerShellCommand = @"Get-ChildItem ""HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\*MRU*"" | Remove-Item -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-cred-vault",
                Name = "Windows 자격 증명 관리자 & Vault 완전 소거",
                Description = "Windows 자격 증명 관리자 및 Vault에 저장된 웹/네트워크/원격 로그인 계정 전체 소거",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Deep,
                FastCliCommand = @"cmdkey /list | for /f ""tokens=1,2 delims= "" %i in ('findstr Target') do cmdkey /delete:%j",
                PowerShellCommand = @"cmdkey /list | Select-String ""Target: "" | ForEach-Object { $t = ($_ -replace "".*Target: "","""").Trim(); cmdkey /delete:$t }",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-rdp-history",
                Name = "원격 데스크톱 (MSTSC / RDP) 접속 이력",
                Description = "MSTSC 원격 데스크톱 연결에 저장된 이전 서버 IP, 접속 기록 및 Default.rdp 삭제",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKCU\Software\Microsoft\Terminal Server Client\Default"" /va /f & reg delete ""HKCU\Software\Microsoft\Terminal Server Client\Servers"" /f & del /f /q ""%USERPROFILE%\Documents\Default.rdp""",
                PowerShellCommand = @"Remove-ItemProperty -Path ""HKCU:\Software\Microsoft\Terminal Server Client\Default"" -Name ""*"" -ErrorAction SilentlyContinue; Remove-Item -Path ""HKCU:\Software\Microsoft\Terminal Server Client\Servers"" -Recurse -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:USERPROFILE\Documents\Default.rdp"" -Force -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-usbstor-history",
                Name = "USB 저장장치 연결 포렌식 이력",
                Description = "과거 시스템에 연결되었던 USB 메모리/외장하드 일련번호 및 연결 장치 히스토리 소거",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Deep,
                FastCliCommand = @"reg delete ""HKLM\SYSTEM\CurrentControlSet\Enum\USBSTOR"" /f",
                PowerShellCommand = @"Remove-Item ""HKLM:\SYSTEM\CurrentControlSet\Enum\USBSTOR\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-bam-dam-history",
                Name = "BAM/DAM 프로세스 실행 통계 이력",
                Description = "Background Activity Moderator에 기록된 모든 실행 파일(.exe) 전체 경로와 타임스탬프 초기화",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\bam\State\UserSettings"" /va /f & reg delete ""HKLM\SYSTEM\CurrentControlSet\Services\dam\State\UserSettings"" /va /f",
                PowerShellCommand = @"Get-ChildItem ""HKLM:\SYSTEM\CurrentControlSet\Services\bam\State\UserSettings"" -ErrorAction SilentlyContinue | Remove-ItemProperty -Name ""*"" -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-comdlg32-opensave",
                Name = "파일 열기/저장 다이얼로그 MRU",
                Description = "모든 프로그램의 '열기'/'다른 이름으로 저장' 창에서 접근했던 폴더 및 파일 경로 기록 삭제",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\OpenSavePidlMRU"" /f & reg delete ""HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedPidlMRU"" /f",
                PowerShellCommand = @"Remove-Item ""HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\OpenSavePidlMRU\*"" -Recurse -Force -ErrorAction SilentlyContinue; Remove-Item ""HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\ComDlg32\LastVisitedPidlMRU\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-networklist-history",
                Name = "과거 접속 Wi-Fi/유선 네트워크 프로필",
                Description = "과거에 연결했던 무선 공유기 SSID 및 네트워크 식별 프로필 목록 소거",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Deep,
                FastCliCommand = @"reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Profiles"" /f & reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Signatures\Unmanaged"" /f",
                PowerShellCommand = @"Remove-Item ""HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkList\Profiles\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "pri-psreadline-history",
                Name = "PowerShell 콘솔 명령어 입력 기록",
                Description = "PowerShell 콘솔에서 입력했던 모든 스크립트/명령어 히스토리 파일(ConsoleHost_history.txt) 소거",
                Category = TaskCategory.Privacy,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:APPDATA\Microsoft\Windows\PowerShell\PSReadLine",
                FastCliCommand = @"del /f /q ""%AppData%\Microsoft\Windows\PowerShell\PSReadLine\ConsoleHost_history.txt""",
                PowerShellCommand = @"Remove-Item ""$env:APPDATA\Microsoft\Windows\PowerShell\PSReadLine\ConsoleHost_history.txt"" -Force -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            // ==========================================
            // 2. 브라우저 최적화 (Browser - 15 Modules)
            // ==========================================
            list.Add(new OptimizationTask
            {
                Id = "brw-edge-cache",
                Name = "Microsoft Edge 웹 캐시",
                Description = "Edge 브라우저 임시 인터넷 파일 및 이미지 캐시 삭제",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Cache",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\Microsoft\Edge\User Data\Default\Cache\*""",
                PowerShellCommand = @"Get-ChildItem ""$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Cache\*"" -Recurse | Remove-Item -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-edge-history",
                Name = "Microsoft Edge 방문 기록",
                Description = "Edge 브라우저 URL 방문 이력 데이터베이스 삭제",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"del /f /q ""%LocalAppData%\Microsoft\Edge\User Data\Default\History""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\History"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-edge-cookies",
                Name = "Microsoft Edge 쿠키/세션",
                Description = "Edge 브라우저 쿠키 및 사이트 저장 데이터 소거",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Deep,
                FastCliCommand = @"del /f /q ""%LocalAppData%\Microsoft\Edge\User Data\Default\Cookies""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Cookies"" -Force",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-chrome-cache",
                Name = "Google Chrome 웹 캐시",
                Description = "Chrome 브라우저 임시 인터넷 캐시 파일 삭제",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Cache",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\Google\Chrome\User Data\Default\Cache\*""",
                PowerShellCommand = @"Get-ChildItem ""$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Cache\*"" -Recurse | Remove-Item -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-chrome-history",
                Name = "Google Chrome 방문 기록",
                Description = "Chrome 브라우저 방문 이력 데이터베이스 삭제",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"del /f /q ""%LocalAppData%\Google\Chrome\User Data\Default\History""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Google\Chrome\User Data\Default\History"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-chrome-cookies",
                Name = "Google Chrome 쿠키/세션",
                Description = "Chrome 브라우저 저장 쿠키 및 로그인 세션 데이터 소거",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Deep,
                FastCliCommand = @"del /f /q ""%LocalAppData%\Google\Chrome\User Data\Default\Cookies""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Cookies"" -Force",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-edge-sessions",
                Name = "Edge 활성 세션 데이터",
                Description = "Edge 탭 복원 및 이전 세션 스냅샷 파일 삭제",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Sessions",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\Microsoft\Edge\User Data\Default\Sessions\*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Sessions\*"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-chrome-gpu-cache",
                Name = "Chrome GPU 가속 캐시",
                Description = "Google Chrome 하드웨어 가속 셰이더 및 GPU 캐시 삭제",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Google\Chrome\User Data\Default\GPUCache",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\Google\Chrome\User Data\Default\GPUCache\*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Google\Chrome\User Data\Default\GPUCache\*"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-edge-media-cache",
                Name = "Edge 미디어 스트리밍 캐시",
                Description = "동영상 및 오디오 스트리밍 임시 버퍼 캐시 소거",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Media Cache",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\Microsoft\Edge\User Data\Default\Media Cache\*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Media Cache\*"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-chrome-downloads",
                Name = "Chrome 다운로드 이력",
                Description = "Chrome 브라우저 파일 다운로드 목록 기록 삭제 (실제 파일 유지)",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Download Service",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\Google\Chrome\User Data\Default\Download Service\*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Download Service\*"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-chrome-shader-cache",
                Name = "Chrome 셰이더 컴파일 캐시",
                Description = "WebGL 및 캔버스 렌더링 셰이더 캐시 삭제",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Google\Chrome\User Data\ShaderCache",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\Google\Chrome\User Data\ShaderCache\*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Google\Chrome\User Data\ShaderCache\*"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-edge-dom-storage",
                Name = "Edge 로컬 스토리지(DOM)",
                Description = "Edge 브라우저 사이트별 로컬 DOM Storage 데이터 삭제",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Deep,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Local Storage",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\Microsoft\Edge\User Data\Default\Local Storage\*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Local Storage\*"" -Force",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-ie-temporary-files",
                Name = "Legacy IE 임시 인터넷 파일",
                Description = "구형 Internet Explorer/Trident 엔진 임시 웹 캐시 소거",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 8",
                PowerShellCommand = @"RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 8",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-ie-history-legacy",
                Name = "Legacy IE 방문 기록",
                Description = "구형 IE 브라우저 히스토리 기록 삭제",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 1",
                PowerShellCommand = @"RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 1",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-ie-form-data-legacy",
                Name = "Legacy IE 자동완성 양식",
                Description = "구형 IE 양식 자동 완성 데이터 초기화",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 16",
                PowerShellCommand = @"RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 16",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-autofill-cards",
                Name = "브라우저 양식 자동완성 & 결제 카드 정보",
                Description = "Chrome, Edge, Whale 등 브라우저에 자동 완성된 주소, 폼 입력값 및 결제 카드 DB 소거",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Deep,
                FastCliCommand = @"del /f /q ""%LocalAppData%\Google\Chrome\User Data\Default\Web Data*"" & del /f /q ""%LocalAppData%\Microsoft\Edge\User Data\Default\Web Data*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Web Data*"" -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Web Data*"" -Force -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-saved-passwords",
                Name = "브라우저 저장된 로그인 계정/비밀번호",
                Description = "Chrome, Edge, Whale 브라우저에 저장된 사이트 로그인 ID 및 비밀번호 DB(Login Data) 완전 소거",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Deep,
                FastCliCommand = @"del /f /q ""%LocalAppData%\Google\Chrome\User Data\Default\Login Data*"" & del /f /q ""%LocalAppData%\Microsoft\Edge\User Data\Default\Login Data*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Login Data*"" -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Login Data*"" -Force -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-kakaotalk-session",
                Name = "카카오톡 로컬 세션 & 자동 로그인 토큰",
                Description = "카카오톡 PC버전의 로컬 캐시, 사용자 프로필 세션 및 자동 로그인 토큰 데이터 완전 소거",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:APPDATA\Kakao\KakaoTalk",
                FastCliCommand = @"del /f /q /s ""%AppData%\Kakao\KakaoTalk\users\*"" & del /f /q /s ""%LocalAppData%\Kakao\KakaoTalk\tmp\*""",
                PowerShellCommand = @"Remove-Item ""$env:APPDATA\Kakao\KakaoTalk\users\*"" -Recurse -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:LOCALAPPDATA\Kakao\KakaoTalk\tmp\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-discord-token",
                Name = "Discord 로그인 토큰 & 로컬 스토리지",
                Description = "Discord 데스크톱 앱의 자동 로그인 인증 토큰, 세션 스토리지 및 캐시 완전 소거",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:APPDATA\discord\Local Storage",
                FastCliCommand = @"del /f /q /s ""%AppData%\discord\Local Storage\leveldb\*"" & del /f /q /s ""%AppData%\discord\Session Storage\*""",
                PowerShellCommand = @"Remove-Item ""$env:APPDATA\discord\Local Storage\leveldb\*"" -Recurse -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:APPDATA\discord\Session Storage\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-teams-slack-tokens",
                Name = "Teams & Slack 협업툴 로그인 세션",
                Description = "Microsoft Teams 및 Slack 데스크톱 클라이언트의 로컬 인증 세션 토큰 및 워크스페이스 캐시 삭제",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:APPDATA\Slack\Local Storage",
                FastCliCommand = @"del /f /q /s ""%AppData%\Slack\Local Storage\leveldb\*"" & del /f /q /s ""%AppData%\Microsoft\Teams\Local Storage\*""",
                PowerShellCommand = @"Remove-Item ""$env:APPDATA\Slack\Local Storage\leveldb\*"" -Recurse -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:APPDATA\Microsoft\Teams\Local Storage\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "brw-cloud-drive-tokens",
                Name = "클라우드 드라이브 (OneDrive/Google/Dropbox)",
                Description = "OneDrive, Google Drive, Dropbox 클라이언트의 로컬 동기화 인덱스 및 세션 캐시 정리",
                Category = TaskCategory.Browser,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Microsoft\OneDrive\logs",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\Microsoft\OneDrive\logs\*"" & del /f /q /s ""%LocalAppData%\Google\DriveFS\Logs\*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Microsoft\OneDrive\logs\*"" -Recurse -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:LOCALAPPDATA\Google\DriveFS\Logs\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            // ==========================================
            // 3. 보안 센터 (Security - 10 Modules)
            // ==========================================
            list.Add(new OptimizationTask
            {
                Id = "sec-defender-scan-logs",
                Name = "Defender 검사 이력 로그",
                Description = "Windows Defender 바이러스 검사 결과 히스토리 로그 소거",
                Category = TaskCategory.Security,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\ProgramData\Microsoft\Windows Defender\Scans\History\Results",
                FastCliCommand = @"del /f /q /s ""C:\ProgramData\Microsoft\Windows Defender\Scans\History\Results\*""",
                PowerShellCommand = @"Remove-Item ""C:\ProgramData\Microsoft\Windows Defender\Scans\History\Results\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sec-defender-quarantine",
                Name = "Defender 격리소 파일 소거",
                Description = "Defender에 의해 격리된 무효 악성코드 파편 영구 소거",
                Category = TaskCategory.Security,
                Risk = RiskLevel.Deep,
                ScanTargetFolder = @"C:\ProgramData\Microsoft\Windows Defender\Quarantine",
                FastCliCommand = @"del /f /q /s ""C:\ProgramData\Microsoft\Windows Defender\Quarantine\*""",
                PowerShellCommand = @"Remove-Item ""C:\ProgramData\Microsoft\Windows Defender\Quarantine\*"" -Recurse -Force",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "sec-firewall-log-files",
                Name = "Windows 방화벽 로그",
                Description = "방화벽 패킷 드롭 및 연결 성공 로그 텍스트 파일 삭제",
                Category = TaskCategory.Security,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\System32\LogFiles\Firewall",
                FastCliCommand = @"del /f /q ""C:\Windows\System32\LogFiles\Firewall\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\System32\LogFiles\Firewall\*"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sec-security-health",
                Name = "보안 상태 대시보드 캐시",
                Description = "Windows 보안 센터 대시보드 상태 알림 캐시 정리",
                Category = TaskCategory.Security,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Microsoft\Windows\SecurityHealth",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\Microsoft\Windows\SecurityHealth\*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Microsoft\Windows\SecurityHealth\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sec-defender-support",
                Name = "Defender 지원/진단 데이터",
                Description = "백신 오류 지원 보고서 및 진단 로그 파일 소거",
                Category = TaskCategory.Security,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\ProgramData\Microsoft\Windows Defender\Support",
                FastCliCommand = @"del /f /q /s ""C:\ProgramData\Microsoft\Windows Defender\Support\*""",
                PowerShellCommand = @"Remove-Item ""C:\ProgramData\Microsoft\Windows Defender\Support\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sec-audit-policy-logs",
                Name = "시스템 감사 보안 로그",
                Description = "보안 이벤트 로그 파일 일괄 비우기",
                Category = TaskCategory.Security,
                Risk = RiskLevel.Deep,
                FastCliCommand = @"wevtutil cl Security",
                PowerShellCommand = @"wevtutil cl Security",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "sec-defender-signatures",
                Name = "구버전 백신 시그니처",
                Description = "새 버전 업데이트 후 잔류한 이전 바이러스 정의 파일 정리",
                Category = TaskCategory.Security,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\ProgramData\Microsoft\Windows Defender\Definition Updates",
                FastCliCommand = @"del /f /q /s ""C:\ProgramData\Microsoft\Windows Defender\Definition Updates\*""",
                PowerShellCommand = @"Remove-Item ""C:\ProgramData\Microsoft\Windows Defender\Definition Updates\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sec-vault-credentials",
                Name = "자격 증명 보관함 캐시",
                Description = "Windows Vault 임시 암호화 토큰 캐시 정리",
                Category = TaskCategory.Security,
                Risk = RiskLevel.Risky,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Microsoft\Vault",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Microsoft\Vault\*"" -Recurse -Force",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "sec-defender-reporting",
                Name = "Defender 보고 데이터",
                Description = "클라우드 진단 전송을 위한 로컬 백신 리포팅 캐시 삭제",
                Category = TaskCategory.Security,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\ProgramData\Microsoft\Windows Defender\Reporting",
                FastCliCommand = @"del /f /q /s ""C:\ProgramData\Microsoft\Windows Defender\Reporting\*""",
                PowerShellCommand = @"Remove-Item ""C:\ProgramData\Microsoft\Windows Defender\Reporting\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sec-smartscreen-logs",
                Name = "SmartScreen 검사 로그",
                Description = "다운로드 실행 파일에 대한 SmartScreen 필터 검사 로그 삭제",
                Category = TaskCategory.Security,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Microsoft\Windows\SmartScreen",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\Microsoft\Windows\SmartScreen\*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Microsoft\Windows\SmartScreen\*"" -Recurse -Force",
                IsSelected = true
            });

            // ==========================================
            // 사용자 계정 & 자격 증명 정화 (Account & Credential Purge - 특수 기능)
            // ==========================================
            list.Add(new OptimizationTask
            {
                Id = "sec-ms-account-purge",
                Name = "Microsoft 계정 & Office WAM 토큰 소거 (강제 로그아웃)",
                Description = "Windows 자격 증명 관리자의 MicrosoftAccount/SSO_POP 토큰 및 WAM(IdentityCache/OneAuth/TokenBroker) 캐시를 소거하여 Office/Windows Live에서 강제 로그아웃합니다. [특수 기능: 자동 선택 제외 / 전용 센터 연동]",
                Category = TaskCategory.Special,
                SubCategory = "Account",
                Risk = RiskLevel.Deep,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Microsoft\IdentityCache",
                FastCliCommand = @"cmd /c ""rd /s /q %LocalAppData%\Microsoft\IdentityCache & rd /s /q %LocalAppData%\Microsoft\OneAuth & rd /s /q %LocalAppData%\Microsoft\TokenBroker & reg delete HKCU\Software\Microsoft\Office\16.0\Common\Identity /f""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Microsoft\IdentityCache\*"" -Recurse -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:LOCALAPPDATA\Microsoft\OneAuth\*"" -Recurse -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:LOCALAPPDATA\Microsoft\TokenBroker\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "sec-adobe-session-purge",
                Name = "Adobe Creative Cloud 계정 로그인 세션 & OOBE 소거",
                Description = "Adobe Creative Cloud, Photoshop, Acrobat의 OOBE 인증 토큰 및 로컬 세션 캐시를 소거하여 모든 Adobe 앱에서 즉시 로그아웃합니다. [특수 기능: 자동 선택 제외 / 전용 센터 연동]",
                Category = TaskCategory.Special,
                SubCategory = "Account",
                Risk = RiskLevel.Deep,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Adobe\OOBE",
                FastCliCommand = @"cmd /c ""rd /s /q %LocalAppData%\Adobe\OOBE & rd /s /q %AppData%\Adobe\OOBE & rd /s /q ""%LocalAppData%\Adobe\Creative Cloud Libraries""""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Adobe\OOBE\*"" -Recurse -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:APPDATA\Adobe\OOBE\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "sec-autodesk-session-purge",
                Name = "Autodesk (AutoCAD) 로그인 상태 & 토큰 소거",
                Description = "AutoCAD 및 Autodesk 제품군의 Web Services LoginState.xml 세션 및 AdskIdentityManager 토큰을 초기화하여 로그아웃합니다. [특수 기능: 자동 선택 제외 / 전용 센터 연동]",
                Category = TaskCategory.Special,
                SubCategory = "Account",
                Risk = RiskLevel.Deep,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Autodesk\Web Services",
                FastCliCommand = @"cmd /c ""del /f /q ""%LocalAppData%\Autodesk\Web Services\LoginState.xml"" & rd /s /q ""%AppData%\Autodesk\AdskIdentityManager""""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Autodesk\Web Services\LoginState.xml"" -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:APPDATA\Autodesk\AdskIdentityManager\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "sec-edge-login-purge",
                Name = "Microsoft Edge 브라우저 로그인 세션 & Login Data 소거",
                Description = "Edge 브라우저 프로필의 자동 로그인 쿠키, Login Data(비밀번호), Token Service 및 탭 복원 세션을 소거하여 모든 사이트에서 로그아웃합니다. [특수 기능: 자동 선택 제외 / 전용 센터 연동]",
                Category = TaskCategory.Special,
                SubCategory = "Account",
                Risk = RiskLevel.Deep,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Sessions",
                FastCliCommand = @"cmd /c ""del /f /q ""%LocalAppData%\Microsoft\Edge\User Data\Default\Network\Cookies*"" & del /f /q ""%LocalAppData%\Microsoft\Edge\User Data\Default\Login Data*"" & del /f /q ""%LocalAppData%\Microsoft\Edge\User Data\Default\Token Service*"" & rd /s /q ""%LocalAppData%\Microsoft\Edge\User Data\Default\Sessions""""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Network\Cookies*"" -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Login Data*"" -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Sessions\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "sec-chrome-login-purge",
                Name = "Google Chrome 브라우저 로그인 세션 & 동기화 토큰 소거",
                Description = "Google Chrome 브라우저의 구글 계정 동기화 토큰, 자동 로그인 쿠키, Login Data 및 세션을 초기화하여 모든 웹사이트에서 로그아웃합니다. [특수 기능: 자동 선택 제외 / 전용 센터 연동]",
                Category = TaskCategory.Special,
                SubCategory = "Account",
                Risk = RiskLevel.Deep,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Sessions",
                FastCliCommand = @"cmd /c ""del /f /q ""%LocalAppData%\Google\Chrome\User Data\Default\Network\Cookies*"" & del /f /q ""%LocalAppData%\Google\Chrome\User Data\Default\Login Data*"" & rd /s /q ""%LocalAppData%\Google\Chrome\User Data\Default\Accounts"" & rd /s /q ""%LocalAppData%\Google\Chrome\User Data\Default\Sessions""""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Network\Cookies*"" -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Login Data*"" -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Sessions\*"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "sec-wincred-generic-purge",
                Name = "Windows 자격 증명 관리자 일반(Generic) 암호 일괄 소거",
                Description = "Windows 자격 증명 관리자에 자동 저장된 모든 일반 웹/앱/서버 로그인 자격 증명을 일괄 제거합니다. [특수 기능: 자동 선택 제외 / 전용 센터 연동]",
                Category = TaskCategory.Special,
                SubCategory = "Account",
                Risk = RiskLevel.Risky,
                FastCliCommand = @"powershell -Command ""[Windows.Security.Credentials.PasswordVault,Windows.Security.Credentials,ContentType=WindowsRuntime] | Out-Null; (cmdkey /list | Select-String 'Target:').ForEach({ cmdkey /delete:($_ -replace '.*Target:\s*','').Trim() })""",
                PowerShellCommand = @"(cmdkey /list | Select-String 'Target:').ForEach({ cmdkey /delete:($_ -replace '.*Target:\s*','').Trim() })",
                IsSelected = false
            });

            // ==========================================
            // 브라우저 100% 공장 초기화 (설치 직후 완전 초기 상태 - 특수 기능)
            // ==========================================
            list.Add(new OptimizationTask
            {
                Id = "spc-chrome-factory-reset",
                Name = "[공장초기화] Google Chrome 전체 프로필 & 데이터 영구 소거",
                Description = "Chrome 프로세스를 강제 종료하고 User Data 디렉터리를 완전 소거하여 최초 설치 상태(북마크/비밀번호/확장앱/캐시 완전 제거)로 초기화합니다. [특수 기능: 파괴적 작업 / 자동 선택 제외]",
                Category = TaskCategory.Special,
                SubCategory = "FactoryReset",
                Risk = RiskLevel.Risky,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Google\Chrome\User Data",
                FastCliCommand = @"cmd /c ""taskkill /f /im chrome.exe 2>nul & rd /s /q ""%LocalAppData%\Google\Chrome\User Data""""",
                PowerShellCommand = @"Stop-Process -Name ""chrome"" -Force -ErrorAction SilentlyContinue; Start-Sleep -Milliseconds 500; Remove-Item ""$env:LOCALAPPDATA\Google\Chrome\User Data"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "spc-edge-factory-reset",
                Name = "[공장초기화] Microsoft Edge 전체 프로필 & 데이터 영구 소거",
                Description = "Edge 프로세스를 강제 종료하고 User Data 디렉터리를 완전 소거하여 최초 설치 상태(북마크/비밀번호/확장앱/캐시 완전 제거)로 초기화합니다. [특수 기능: 파괴적 작업 / 자동 선택 제외]",
                Category = TaskCategory.Special,
                SubCategory = "FactoryReset",
                Risk = RiskLevel.Risky,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Microsoft\Edge\User Data",
                FastCliCommand = @"cmd /c ""taskkill /f /im msedge.exe 2>nul & rd /s /q ""%LocalAppData%\Microsoft\Edge\User Data""""",
                PowerShellCommand = @"Stop-Process -Name ""msedge"" -Force -ErrorAction SilentlyContinue; Start-Sleep -Milliseconds 500; Remove-Item ""$env:LOCALAPPDATA\Microsoft\Edge\User Data"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "spc-whale-factory-reset",
                Name = "[공장초기화] Naver Whale 전체 프로필 & 데이터 영구 소거",
                Description = "Whale 프로세스를 강제 종료하고 User Data 디렉터리를 완전 소거하여 최초 설치 상태(북마크/비밀번호/확장앱/캐시 완전 제거)로 초기화합니다. [특수 기능: 파괴적 작업 / 자동 선택 제외]",
                Category = TaskCategory.Special,
                SubCategory = "FactoryReset",
                Risk = RiskLevel.Risky,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Naver\Naver Whale\User Data",
                FastCliCommand = @"cmd /c ""taskkill /f /im whale.exe 2>nul & rd /s /q ""%LocalAppData%\Naver\Naver Whale\User Data""""",
                PowerShellCommand = @"Stop-Process -Name ""whale"" -Force -ErrorAction SilentlyContinue; Start-Sleep -Milliseconds 500; Remove-Item ""$env:LOCALAPPDATA\Naver\Naver Whale\User Data"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "spc-firefox-factory-reset",
                Name = "[공장초기화] Mozilla Firefox 전체 프로필 & 데이터 영구 소거",
                Description = "Firefox 프로세스를 강제 종료하고 AppData/LocalAppData의 Firefox Profiles 폴더를 완전 소거하여 최초 설치 상태로 초기화합니다. [특수 기능: 파괴적 작업 / 자동 선택 제외]",
                Category = TaskCategory.Special,
                SubCategory = "FactoryReset",
                Risk = RiskLevel.Risky,
                ScanTargetFolder = @"$env:APPDATA\Mozilla\Firefox\Profiles",
                FastCliCommand = @"cmd /c ""taskkill /f /im firefox.exe 2>nul & rd /s /q ""%AppData%\Mozilla\Firefox\Profiles"" & rd /s /q ""%LocalAppData%\Mozilla\Firefox\Profiles""""",
                PowerShellCommand = @"Stop-Process -Name ""firefox"" -Force -ErrorAction SilentlyContinue; Start-Sleep -Milliseconds 500; Remove-Item ""$env:APPDATA\Mozilla\Firefox\Profiles"" -Recurse -Force -ErrorAction SilentlyContinue; Remove-Item ""$env:LOCALAPPDATA\Mozilla\Firefox\Profiles"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = false
            });

            // ==========================================
            // Windows 기본 프로필(Default Profile) 덮어쓰기 복제 (Method A - 특수 기능)
            // ==========================================
            list.Add(new OptimizationTask
            {
                Id = "spc-sync-default-user-profile",
                Name = "[기본 프로필 복제] 현재 사용자 설정을 Windows 기본 프로필(Default)로 복제",
                Description = "현재 계정의 테마, 작업표시줄, 탐색기 옵션, 바탕화면 레지스트리(reg save HKCU)를 C:\\Users\\Default로 복제하여 향후 생성되는 모든 신규 사용자 계정에 동일하게 자동 적용합니다. (기존 Default 프로필 자동 백업 및 Everyone/Users ACL 권한 자동 보정) [특수 기능: 시스템 템플릿 변경 / 자동 선택 제외]",
                Category = TaskCategory.Special,
                SubCategory = "Profile",
                Risk = RiskLevel.Risky,
                ScanTargetFolder = @"C:\Users\Default",
                FastCliCommand = @"cmd /c ""if not exist ""C:\Users\Default_Backup_WinPurify"" (robocopy ""C:\Users\Default"" ""C:\Users\Default_Backup_WinPurify"" /E /ZB /R:1 /W:1 2>nul & copy /y ""C:\Users\Default\NTUSER.DAT"" ""C:\Users\Default_Backup_WinPurify\NTUSER.DAT"" 2>nul) & reg save HKCU ""%TEMP%\winpurify_ntuser.dat"" /y & attrib -h -s -r ""C:\Users\Default\NTUSER.DAT"" 2>nul & copy /y ""%TEMP%\winpurify_ntuser.dat"" ""C:\Users\Default\NTUSER.DAT"" & attrib +h +s ""C:\Users\Default\NTUSER.DAT"" & del /f /q ""%TEMP%\winpurify_ntuser.dat"" 2>nul & robocopy ""%APPDATA%\Microsoft\Windows\Themes"" ""C:\Users\Default\AppData\Roaming\Microsoft\Windows\Themes"" /E /ZB /R:1 /W:1 2>nul & icacls ""C:\Users\Default"" /grant ""Everyone:(OI)(CI)F"" /grant ""BUILTIN\Users:(OI)(CI)RX"" /inheritance:e /T /C /Q 2>nul & icacls ""C:\Users\Default\NTUSER.DAT"" /grant ""Everyone:F"" /grant ""BUILTIN\Users:R"" /inheritance:e /C /Q 2>nul""",
                PowerShellCommand = @"$def = 'C:\Users\Default'; $bak = 'C:\Users\Default_Backup_WinPurify'; $tmp = ""$env:TEMP\winpurify_ntuser_snap.dat""; if (-not (Test-Path $bak)) { robocopy $def $bak /E /ZB /R:1 /W:1 > $null; if (Test-Path ""$def\NTUSER.DAT"") { Copy-Item ""$def\NTUSER.DAT"" ""$bak\NTUSER.DAT"" -Force -ErrorAction SilentlyContinue } }; Remove-Item $tmp -Force -ErrorAction SilentlyContinue; reg.exe save HKCU $tmp /y; if (Test-Path $tmp) { attrib -h -s -r ""$def\NTUSER.DAT"" 2>$null; Copy-Item $tmp ""$def\NTUSER.DAT"" -Force; attrib +h +s ""$def\NTUSER.DAT""; Remove-Item $tmp -Force }; robocopy ""$env:APPDATA\Microsoft\Windows\Themes"" ""$def\AppData\Roaming\Microsoft\Windows\Themes"" /E /ZB /R:1 /W:1 > $null; icacls $def /grant ""Everyone:(OI)(CI)F"" /grant ""BUILTIN\Users:(OI)(CI)RX"" /inheritance:e /T /C /Q > $null; icacls ""$def\NTUSER.DAT"" /grant ""Everyone:F"" /grant ""BUILTIN\Users:R"" /inheritance:e /C /Q > $null",
                IsSelected = false
            });

            // ==========================================
            // 4. 레지스트리 (Registry - 15 Modules)
            // ==========================================
            list.Add(new OptimizationTask
            {
                Id = "reg-mui-cache-clean",
                Name = "MUI 인터페이스 캐시",
                Description = "실행되었던 프로그램들의 다국어 셸 텍스트 레지스트리 캐시 정리",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Safe,
                PowerShellCommand = @"Get-ChildItem ""HKCU:\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache"" | ForEach-Object { Remove-ItemProperty -Path $_.PSPath -Name ""*"" }",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "reg-compat-flags",
                Name = "호환성 레이어 기록",
                Description = "이전 실행 앱에 임시 부여되었던 AppCompat 호환성 플래그 초기화",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKCU\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"" /f",
                PowerShellCommand = @"Remove-Item ""HKCU:\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "reg-shell-ext-broken",
                Name = "셸 익스텐션 무효 파편",
                Description = "삭제된 프로그램의 탐색기 우클릭 셸 확장 레지스트리 참조 제거",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Safe,
                PowerShellCommand = @"Get-ChildItem ""HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved"" | ForEach-Object { if(-not (Test-Path $_.Name)) { Remove-ItemProperty -Path ""HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved"" -Name $_.PSChildName } }",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "reg-firewall-orphans",
                Name = "방화벽 고립 규칙",
                Description = "이미 삭제된 프로그램 경로를 가리키는 유령 방화벽 허용 규칙 정리",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Safe,
                PowerShellCommand = @"Get-NetFirewallRule | Where-Object { $_.AppPath -and -not (Test-Path $_.AppPath) } | Remove-NetFirewallRule",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "reg-clsid-orphans",
                Name = "고립된 CLSID 클래스 식별자",
                Description = "연결 대상 COM 서버/DLL이 존재하지 않는 고립 CLSID 레지스트리 정리",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Deep,
                PowerShellCommand = @"Get-ChildItem ""HKCR:\CLSID"" | ForEach-Object { if($_.GetValue("""") -eq $null -and -not $_.HasSubKeys) { Remove-Item $_.PSPath -Force } }",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "reg-app-paths-invalid",
                Name = "무효 앱 실행 경로 (App Paths)",
                Description = "실행 파일이 삭제되었으나 레지스트리에 남아있는 App Paths 키 소거",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Safe,
                PowerShellCommand = @"Get-ChildItem ""HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths"" | ForEach-Object { if(-not (Test-Path (Get-ItemProperty $_.PSPath).""(default)"")) { Remove-Item $_.PSPath -Force } }",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "reg-shared-dlls",
                Name = "공유 DLL 잔여물",
                Description = "디스크에 존재하지 않는 공유 동적 라이브러리(SharedDlls) 카운터 정리",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Deep,
                PowerShellCommand = @"Get-ItemProperty ""HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\SharedDlls"" | Get-Member -MemberType NoteProperty | ForEach-Object { if(-not (Test-Path $_.Name)) { Remove-ItemProperty ""HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\SharedDlls"" -Name $_.Name } }",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "reg-sound-events",
                Name = "무효 사운드 알림 이벤트",
                Description = "삭제된 애플리케이션이 등록해 둔 사운드 이벤트 연결 키 정리",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKCU\AppEvents\Schemes\Apps\.Default"" /f",
                PowerShellCommand = @"Remove-Item ""HKCU:\AppEvents\Schemes\Apps\.Default"" -Recurse -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "reg-font-associates",
                Name = "글꼴 연결 오류 수정",
                Description = "시스템 폰트 대체 목록 중 무효한 항목 정리",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Safe,
                PowerShellCommand = @"Get-ItemProperty ""HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\FontSubstitutes"" | Get-Member -MemberType NoteProperty | ForEach-Object { if(-not $_.Name) { Remove-ItemProperty ""HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\FontSubstitutes"" -Name $_.Name } }",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "reg-help-paths",
                Name = "도움말 파일 경로 파편",
                Description = "오래된 도움말 파일(HLP/CHM) 경로 레지스트리 잔재 제거",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKLM\SOFTWARE\Microsoft\Windows\Help"" /f",
                PowerShellCommand = @"Remove-Item ""HKLM:\SOFTWARE\Microsoft\Windows\Help"" -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "reg-installer-fragments",
                Name = "인스톨러 분리 파편",
                Description = "삭제된 MSI 설치 패키지의 유령 레지스트리 키 정리",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Deep,
                PowerShellCommand = @"Get-ChildItem ""HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products"" | ForEach-Object { if(-not (Test-Path $_.PSPath)) { Remove-Item $_.PSPath -Recurse -Force } }",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "reg-user-classes",
                Name = "사용자 클래스 파편",
                Description = "HKCU Software Classes 내의 빈 하위 키 정리",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Safe,
                PowerShellCommand = @"Get-ChildItem ""HKCU:\Software\Classes\CLSID"" | ForEach-Object { if(-not $_.HasSubKeys) { Remove-Item $_.PSPath -Force } }",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "reg-run-keys-invalid",
                Name = "자동 실행 무효 키",
                Description = "삭제된 프로그램이 Run 레지스트리에 남겨둔 시작 프로그램 찌꺼기 제거",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Safe,
                PowerShellCommand = @"Get-ItemProperty ""HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"" | Get-Member -MemberType NoteProperty | ForEach-Object { if(-not (Test-Path (Get-ItemProperty ""HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"").$($_.Name))) { Remove-ItemProperty ""HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run"" -Name $_.Name } }",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "reg-start-menu-links",
                Name = "시작 메뉴 무효 바로가기",
                Description = "시작 메뉴 프로그램 목록 중 대상이 없는 빈 LNK 바로가기 제거",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Safe,
                PowerShellCommand = @"Get-ChildItem ""$env:ProgramData\Microsoft\Windows\Start Menu\Programs"" -Recurse -Include *.lnk | ForEach-Object { if(-not (Test-Path $_.FullName)) { Remove-Item $_.FullName -Force } }",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "reg-typelib-orphans",
                Name = "TypeLib 분리 파편",
                Description = "참조 대상이 없는 고립된 TypeLib 레지스트리 인터페이스 정의 소거",
                Category = TaskCategory.Registry,
                Risk = RiskLevel.Risky,
                PowerShellCommand = @"Get-ChildItem ""HKCR:\TypeLib"" | ForEach-Object { if(-not $_.HasSubKeys) { Remove-Item $_.PSPath -Force } }",
                IsSelected = false
            });

            // ==========================================
            // 5. 업데이트 관리 (Update - 15 Modules)
            // ==========================================
            list.Add(new OptimizationTask
            {
                Id = "upd-cache-clean",
                Name = "업데이트 다운로드 캐시",
                Description = "설치 완료된 Windows Update 다운로드 임시 패키지 소거",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\SoftwareDistribution\Download",
                FastCliCommand = @"sc stop wuauserv & del /f /q /s ""C:\Windows\SoftwareDistribution\Download\*"" & sc start wuauserv",
                PowerShellCommand = @"Stop-Service wuauserv -Force; Remove-Item ""C:\Windows\SoftwareDistribution\Download\*"" -Recurse -Force; Start-Service wuauserv",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "upd-winsxs-reset",
                Name = "WinSxS 정밀 압축/청소",
                Description = "DISM을 통한 대체된 구버전 구성요소 백업 정리 및 베이스라인 리셋",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Deep,
                FastCliCommand = @"Dism.exe /online /Cleanup-Image /StartComponentCleanup /ResetBase",
                PowerShellCommand = @"Dism.exe /online /Cleanup-Image /StartComponentCleanup /ResetBase",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "upd-delivery-opt",
                Name = "전송 최적화 캐시 (Delivery Optimization)",
                Description = "P2P 업데이트 공유를 위해 저장된 로컬 전송 최적화 캐시 파일 삭제",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache",
                FastCliCommand = @"sc stop DoSvc & del /f /q /s ""C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache\*"" & sc start DoSvc",
                PowerShellCommand = @"Stop-Service DoSvc -Force; Remove-Item ""C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache\*"" -Recurse -Force; Start-Service DoSvc",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "upd-bits-queue-reset",
                Name = "BITS 전송 대기열 초기화",
                Description = "백그라운드 지능형 전송 서비스 대기열 초기화",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"bitsadmin /reset /allusers",
                PowerShellCommand = @"bitsadmin /reset /allusers",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "upd-windows-old-del",
                Name = "이전 Windows 설치본 (Windows.old)",
                Description = "대규모 기능 업데이트 후 잔류한 이전 OS 롤백용 거대 폴더 삭제",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Deep,
                ScanTargetFolder = @"C:\Windows.old",
                FastCliCommand = @"rmdir /s /q ""C:\Windows.old""",
                PowerShellCommand = @"Remove-Item ""C:\Windows.old"" -Recurse -Force -ErrorAction SilentlyContinue",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "upd-driver-store-tmp",
                Name = "드라이버 설치 임시 잔해",
                Description = "DriverStore 설치 중 생성된 임시 압축 해제 잔해 소거",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\System32\DriverStore\Temp",
                FastCliCommand = @"del /f /q /s ""C:\Windows\System32\DriverStore\Temp\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\System32\DriverStore\Temp\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "upd-msi-patch-cache",
                Name = "MSI 패치 백업 캐시",
                Description = "윈도우 인스톨러 $PatchCache$ 폴더 내의 백업 패치 데이터 정리",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\Installer\$PatchCache$",
                FastCliCommand = @"del /f /q /s ""C:\Windows\Installer\$PatchCache$\$*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\Installer\$PatchCache$\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "upd-cbs-logs-all",
                Name = "CBS 매니페스트 로그",
                Description = "Windows 서비스 및 업데이트 처리 CBS(Component-Based Servicing) 로그 소거",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\Logs\CBS",
                FastCliCommand = @"del /f /q /s ""C:\Windows\Logs\CBS\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\Logs\CBS\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "upd-store-cache-dat",
                Name = "Store 패키지 데이터 캐시",
                Description = "SoftwareDistribution DataStore 데이터베이스 임시 트랜잭션 파일 정리",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\SoftwareDistribution\DataStore",
                FastCliCommand = @"del /f /q /s ""C:\Windows\SoftwareDistribution\DataStore\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\SoftwareDistribution\DataStore\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "upd-pending-rename",
                Name = "파일 이름 변경 대기열 소거",
                Description = "재부팅 시 처리 예정으로 남아있는 잔여 파일 이름 변경 대기열 초기화",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKLM\SYSTEM\CurrentControlSet\Control\Session Manager"" /v PendingFileRenameOperations /f",
                PowerShellCommand = @"Remove-ItemProperty -Path ""HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager"" -Name ""PendingFileRenameOperations""",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "upd-manifest-cache",
                Name = "컴포넌트 매니페스트 캐시",
                Description = "WinSxS 내부 ManifestCache 임시 참조 데이터 정리",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\WinSxS\ManifestCache",
                FastCliCommand = @"del /f /q ""C:\Windows\WinSxS\ManifestCache\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\WinSxS\ManifestCache\*"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "upd-reboot-required",
                Name = "시스템 재부팅 보류 플래그 초기화",
                Description = "업데이트 완료 후 남아있는 불필요한 RebootRequired 플래그 정리",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"reg delete ""HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired"" /f",
                PowerShellCommand = @"Remove-ItemProperty -Path ""HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired"" -Name ""*""",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "upd-wu-text-logs",
                Name = "Windows Update 텍스트 로그",
                Description = "루트 디렉터리의 레거시 WindowsUpdate.log 텍스트 로그 삭제",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"del /f /q ""C:\Windows\WindowsUpdate.log""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\WindowsUpdate.log"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "upd-office-temp-cache",
                Name = "Office 배포 패키지 캐시",
                Description = "MS Office 업데이트 설치 시 다운로드된 임시 압축 해제 캐시 삭제",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\Temp\OfficeSetup",
                FastCliCommand = @"del /f /q /s ""C:\Windows\Temp\OfficeSetup\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\Temp\OfficeSetup\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "upd-sqm-data-all",
                Name = "SQM 품질 개선 통계 데이터",
                Description = "서비스 프로필에 저장된 Windows 소프트웨어 품질 메트릭스 파일 삭제",
                Category = TaskCategory.Update,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\ServiceProfiles\LocalService\AppData\Local\Microsoft\Windows\WSQM",
                FastCliCommand = @"del /f /q /s ""C:\Windows\ServiceProfiles\LocalService\AppData\Local\Microsoft\Windows\WSQM\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\ServiceProfiles\LocalService\AppData\Local\Microsoft\Windows\WSQM\*"" -Recurse -Force",
                IsSelected = true
            });

            // ==========================================
            // 6. 시스템 최적화 (System - 15 Modules)
            // ==========================================
            list.Add(new OptimizationTask
            {
                Id = "sys-dns-cache-flush",
                Name = "DNS 해석기 캐시 초기화",
                Description = "로컬 DNS 리졸버 캐시를 비워 도메인 해석 속도 및 최신 IP 반영",
                Category = TaskCategory.System,
                Risk = RiskLevel.Safe,
                NativeActionType = 2, // Native DNS Flush
                FastCliCommand = @"ipconfig /flushdns",
                PowerShellCommand = @"ipconfig /flushdns",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sys-icon-cache-reset",
                Name = "시스템 아이콘 캐시 재생성",
                Description = "손상되었거나 깨진 파일/프로그램 아이콘 캐시 파일(IconCache.db) 재생성",
                Category = TaskCategory.System,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Microsoft\Windows\Explorer",
                FastCliCommand = @"taskkill /F /IM explorer.exe & del /f /q ""%LocalAppData%\Microsoft\Windows\Explorer\iconcache_*.db"" & start explorer.exe",
                PowerShellCommand = @"taskkill /F /IM explorer.exe; Remove-Item ""$env:LOCALAPPDATA\Microsoft\Windows\Explorer\iconcache_*.db"" -Force; start explorer.exe",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sys-shader-cache",
                Name = "DirectX 그래픽 셰이더 캐시",
                Description = "D3DSCache 폴더에 컴파일되어 적체된 구형 DirectX 셰이더 캐시 소거",
                Category = TaskCategory.System,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\D3DSCache",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\D3DSCache\*""",
                PowerShellCommand = @"Get-ChildItem -Path ""$env:LOCALAPPDATA\D3DSCache\*"" -Recurse | Remove-Item -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sys-net-stack-reset",
                Name = "네트워크 스택 / IP 리셋",
                Description = "TCP/IP 및 Winsock 스택을 초기화하여 네트워크 지연 및 핑 안정화",
                Category = TaskCategory.System,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"netsh winsock reset & netsh int ip reset",
                PowerShellCommand = @"netsh winsock reset; netsh int ip reset",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sys-spooler-reset",
                Name = "프린터 인쇄 대기열 초기화",
                Description = "인쇄 오류로 멈춘 스풀러 서비스 및 대기열(PRINTERS) 임시 파일 강제 초기화",
                Category = TaskCategory.System,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\System32\spool\PRINTERS",
                FastCliCommand = @"sc stop Spooler & del /f /q ""C:\Windows\System32\spool\PRINTERS\*"" & sc start Spooler",
                PowerShellCommand = @"Stop-Service Spooler; Remove-Item ""C:\Windows\System32\spool\PRINTERS\*"" -Force; Start-Service Spooler",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sys-wsreset-cache",
                Name = "MS Store 서비스 캐시 리셋",
                Description = "Microsoft Store 앱의 실행 장애 및 캐시 리셋 (wsreset)",
                Category = TaskCategory.System,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"wsreset.exe",
                PowerShellCommand = @"wsreset.exe",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sys-timesync-all",
                Name = "표준 시간 서버 동기화",
                Description = "Windows Time 서비스 표준 시간 서버 즉시 동기화",
                Category = TaskCategory.System,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"w32tm /resync",
                PowerShellCommand = @"w32tm /resync",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sys-wmi-salvage",
                Name = "WMI 관리 데이터베이스 정합성 검사",
                Description = "손상된 WMI 저장소 점검 및 자동 복구 (salvagerepository)",
                Category = TaskCategory.System,
                Risk = RiskLevel.Deep,
                FastCliCommand = @"winmgmt /salvagerepository",
                PowerShellCommand = @"winmgmt /salvagerepository",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "sys-winhttp-proxy",
                Name = "시스템 프록시 구성 초기화",
                Description = "악성코드나 비정상 종료로 잘못 설정된 WinHTTP 프록시를 다이렉트로 초기화",
                Category = TaskCategory.System,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"netsh winhttp reset proxy",
                PowerShellCommand = @"netsh winhttp reset proxy",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sys-arp-cache-flush",
                Name = "ARP 테이블 캐시 초기화",
                Description = "로컬 라우터 및 네트워크 ARP 캐시 테이블을 비워 네트워크 충돌 해결",
                Category = TaskCategory.System,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"netsh interface ip delete arpcache",
                PowerShellCommand = @"netsh interface ip delete arpcache",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sys-perf-counters",
                Name = "성능 측정 카운터 리셋",
                Description = "손상된 시스템 리소스 모니터 성능 카운터 레지스트리 복원 (lodctr /r)",
                Category = TaskCategory.System,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"lodctr /r",
                PowerShellCommand = @"lodctr /r",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sys-com-catalog-reset",
                Name = "COM+ 구성 카탈로그 캐시 정리",
                Description = "Registration 폴더 내의 COM+ 임시 카탈로그 백업 파일 정리",
                Category = TaskCategory.System,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\Registration",
                FastCliCommand = @"del /f /q ""C:\Windows\Registration\*.clb""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\Registration\*.clb"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sys-winsock-reset",
                Name = "Winsock 소켓 카탈로그 리셋",
                Description = "Winsock 레이어 소켓 프로토콜 카탈로그 전체 기본값 복원",
                Category = TaskCategory.System,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"netsh winsock reset",
                PowerShellCommand = @"netsh winsock reset",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sys-system32-temp",
                Name = "System32 루트 임시 파일",
                Description = "C:\\Windows\\System32\\Temp 폴더 내의 시스템 프로세스 잔여물 소거",
                Category = TaskCategory.System,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\System32\Temp",
                FastCliCommand = @"del /f /q /s ""C:\Windows\System32\Temp\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\System32\Temp\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "sys-event-logs-clear",
                Name = "Windows 전체 이벤트 로그 비우기",
                Description = "용량을 차지하는 누적된 애플리케이션 및 시스템 이벤트 로그 전체 비우기",
                Category = TaskCategory.System,
                Risk = RiskLevel.Deep,
                PowerShellCommand = @"wevtutil el | Foreach-Object {wevtutil cl ""$_""}",
                IsSelected = false
            });

            // ==========================================
            // 7. 저장공간 (Storage - 25 Modules)
            // ==========================================
            list.Add(new OptimizationTask
            {
                Id = "stg-windows-temp",
                Name = "Windows 공용 임시 폴더 (%WinDir%\\Temp)",
                Description = "윈도우 운영체제 공용 임시 디렉터리 내의 잔여 파일 완전 삭제",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\Temp",
                FastCliCommand = @"del /f /q /s ""C:\Windows\Temp\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\Temp\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-user-temp-data",
                Name = "사용자 프로필 임시 폴더 (%TEMP%)",
                Description = "현재 로그인 사용자 계정의 임시 파일 디렉터리 일괄 비우기",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                NativeActionType = 3, // Native Temp Clear
                ScanTargetFolder = @"$env:TEMP",
                FastCliCommand = @"del /f /q /s ""%TEMP%\*""",
                PowerShellCommand = @"Remove-Item ""$env:TEMP\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-recycle-bin-all",
                Name = "모든 드라이브 휴지통 완전 비우기",
                Description = "C, D 등 모든 시스템 드라이브의 휴지통 파일 영구 무음 소거",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                NativeActionType = 1, // Native SHEmptyRecycleBinW
                FastCliCommand = @"cmd.exe /c ""rd /s /q %systemdrive%\$Recycle.bin""",
                PowerShellCommand = @"Clear-RecycleBin -Force -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-crash-memory-dump",
                Name = "시스템 전체 메모리 덤프 (MEMORY.DMP)",
                Description = "블루스크린(BSOD) 발생 시 디스크에 생성된 수 GB 크기의 메모리 덤프 파일 삭제",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"del /f /q ""C:\Windows\memory.dmp""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\memory.dmp"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-prefetch-all",
                Name = "Prefetch 가속 캐시 정리",
                Description = "Windows\\Prefetch 내의 구형 프로그램 가속 캐시 파일 정리",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\Prefetch",
                FastCliCommand = @"del /f /q /s ""C:\Windows\Prefetch\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\Prefetch\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-wer-reports",
                Name = "사용자 계정 오류 보고서 (WER)",
                Description = "Windows 오류 보고 서비스(WER)에 누적된 크래시 로그 및 메모리 스냅샷 소거",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Microsoft\Windows\WER",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\Microsoft\Windows\WER\*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Microsoft\Windows\WER\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-app-crash-dumps",
                Name = "개별 앱 크래시 덤프 (CrashDumps)",
                Description = "서드파티 애플리케이션 오류 시 생성된 미니 덤프(.dmp) 파일 정리",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\CrashDumps",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\CrashDumps\*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\CrashDumps\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-font-render-cache",
                Name = "시스템 글꼴 렌더링 캐시",
                Description = "Windows FontCache 서비스의 손상되거나 비대해진 글꼴 렌더링 캐시 초기화",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\ServiceProfiles\LocalService\AppData\Local\FontCache",
                FastCliCommand = @"sc stop FontCache & del /f /q ""C:\Windows\ServiceProfiles\LocalService\AppData\Local\FontCache\*"" & sc start FontCache",
                PowerShellCommand = @"Stop-Service FontCache; Remove-Item ""C:\Windows\ServiceProfiles\LocalService\AppData\Local\FontCache\*"" -Force; Start-Service FontCache",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-iis-log-files",
                Name = "IIS 웹 서버 로그 파일",
                Description = "로컬 IIS 웹 서버 로그 디렉터리(inetpub\\logs) 잔여 데이터 삭제",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\inetpub\logs\LogFiles",
                FastCliCommand = @"del /f /q /s ""C:\inetpub\logs\LogFiles\*""",
                PowerShellCommand = @"Remove-Item ""C:\inetpub\logs\LogFiles\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-catroot2-cleanup",
                Name = "암호화 카탈로그 저장소 캐시 (Catroot2)",
                Description = "CryptSvc 서명 카탈로그 임시 트랜잭션 파일 초기화",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\System32\catroot2",
                FastCliCommand = @"sc stop CryptSvc & del /f /q /s ""C:\Windows\System32\catroot2\*"" & sc start CryptSvc",
                PowerShellCommand = @"Stop-Service CryptSvc; Remove-Item ""C:\Windows\System32\catroot2\*"" -Recurse -Force; Start-Service CryptSvc",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-nvidia-gl-cache",
                Name = "NVIDIA OpenGL 셰이더 캐시",
                Description = "NVIDIA 그래픽 카드 드라이버 OpenGL/Vulkan 셰이더 캐시 소거",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Local\NVIDIA\GLCache",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\Local\NVIDIA\GLCache\*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Local\NVIDIA\GLCache\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-directx-shader",
                Name = "DirectX 범용 셰이더 캐시",
                Description = "DirectX 게임 플레이 중 생성된 셰이더 컴파일 캐시 정리",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\D3DSCache",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\D3DSCache\*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\D3DSCache\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-minidumps-all",
                Name = "시스템 미니덤프 진단 요약본 (Minidump)",
                Description = "C:\\Windows\\Minidump 내의 소형 크래시 분석 파일들 소거",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\Minidump",
                FastCliCommand = @"del /f /q ""C:\Windows\Minidump\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\Minidump\*"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-inf-install-logs",
                Name = "드라이버 설치 이력 로그 (setupapi.dev.log)",
                Description = "장치 드라이버 설치 과정에서 적체된 거대 로그 텍스트 파일 삭제",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"del /f /q ""C:\Windows\inf\setupapi.dev.log""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\inf\setupapi.dev.log"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-bits-cache",
                Name = "BITS 백그라운드 전송 캐시",
                Description = "ProgramData Network Downloader 임시 큐 파일 삭제",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\ProgramData\Microsoft\Network\Downloader",
                FastCliCommand = @"del /f /q /s ""C:\ProgramData\Microsoft\Network\Downloader\*""",
                PowerShellCommand = @"Remove-Item ""C:\ProgramData\Microsoft\Network\Downloader\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-package-cache",
                Name = "대형 앱 패키지 설치 캐시",
                Description = "Visual Studio / C++ Redistributable 등 인스톨러 캐시 정리",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Deep,
                ScanTargetFolder = @"C:\ProgramData\Package Cache",
                FastCliCommand = @"del /f /q /s ""C:\ProgramData\Package Cache\*""",
                PowerShellCommand = @"Remove-Item ""C:\ProgramData\Package Cache\*"" -Recurse -Force",
                IsSelected = false
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-setupapi-breadcrumb",
                Name = "장치 드라이버 로딩 로그",
                Description = "Windows inf 폴더 내의 잔여 드라이버 셋업 로그 정리",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"del /f /q ""C:\Windows\inf\setupapi.dev.log""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\inf\setupapi.dev.log"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-win-logfiles-all",
                Name = "Windows 범용 시스템 로그",
                Description = "System32 LogFiles 폴더 내의 각종 서비스 텍스트 로그 파일 일괄 소거",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\System32\LogFiles",
                FastCliCommand = @"del /f /q /s ""C:\Windows\System32\LogFiles\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\System32\LogFiles\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-desktop-bridge",
                Name = "데스크톱 브릿지 가상화 데이터",
                Description = "UWP / MSIX 가상화 앱 레포지토리의 임시 패키지 캐시 삭제",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\ProgramData\Microsoft\Windows\AppRepository",
                FastCliCommand = @"del /f /q /s ""C:\ProgramData\Microsoft\Windows\AppRepository\*""",
                PowerShellCommand = @"Remove-Item ""C:\ProgramData\Microsoft\Windows\AppRepository\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-dism-work-logs",
                Name = "DISM 이미지 복구 작업 로그",
                Description = "DISM 명령어 수행 시 기록된 디버깅 텍스트 로그 소거",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\Logs\DISM",
                FastCliCommand = @"del /f /q /s ""C:\Windows\Logs\DISM\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\Logs\DISM\*"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-local-temp-extra",
                Name = "AppData 로컬 임시 잔해 (%LocalAppData%\\Temp)",
                Description = "각종 프로그램이 AppData\\Local\\Temp에 생성해 둔 임시 파일 완전 소거",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"$env:LOCALAPPDATA\Temp",
                FastCliCommand = @"del /f /q /s ""%LocalAppData%\Temp\*""",
                PowerShellCommand = @"Remove-Item ""$env:LOCALAPPDATA\Temp\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-was-activation",
                Name = "WAS 서비스 실행 로그",
                Description = "Windows Process Activation Service 로그 파일 정리",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\System32\LogFiles\WAS",
                FastCliCommand = @"del /f /q /s ""C:\Windows\System32\LogFiles\WAS\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\System32\LogFiles\WAS\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-msi-installer-tmp",
                Name = "MSI 인스톨러 임시 데이터",
                Description = "C:\\Windows\\Installer 폴더 내의 .tmp 확장자 임시 파일 소거",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\Installer",
                FastCliCommand = @"del /f /q ""C:\Windows\Installer\*.tmp""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\Installer\*.tmp"" -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-sys-profile-temp",
                Name = "시스템 프로필 전용 임시 파일",
                Description = "LocalService 및 NetworkService 프로필 임시 폴더 잔여물 소거",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\Windows\ServiceProfiles\LocalService\AppData\Local\Temp",
                FastCliCommand = @"del /f /q /s ""C:\Windows\ServiceProfiles\LocalService\AppData\Local\Temp\*""",
                PowerShellCommand = @"Remove-Item ""C:\Windows\ServiceProfiles\LocalService\AppData\Local\Temp\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-global-wer-clear",
                Name = "전체 오류 보고 통합 저장소",
                Description = "ProgramData Microsoft WER 통합 크래시 저장소 완전 소거",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                ScanTargetFolder = @"C:\ProgramData\Microsoft\Windows\WER",
                FastCliCommand = @"del /f /q /s ""C:\ProgramData\Microsoft\Windows\WER\*""",
                PowerShellCommand = @"Remove-Item ""C:\ProgramData\Microsoft\Windows\WER\*"" -Recurse -Force",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-hiberfil-toggle",
                Name = "최대 절전 모드 파일 (hiberfil.sys) 해제",
                Description = "최대 절전 모드를 비활성화하여 물리 RAM 크기(16GB~64GB)만큼의 C: 시스템 디스크 공간 즉시 회수",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Deep,
                FastCliCommand = @"powercfg -h off",
                PowerShellCommand = @"powercfg -h off",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-dism-resetbase",
                Name = "DISM WinSxS 컴포넌트 베이스라인 리셋",
                Description = "구버전 윈도우 업데이트 컴포넌트 백업을 영구 정리 및 압축하여 3GB~10GB 대용량 공간 확보",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Deep,
                FastCliCommand = @"DISM.exe /Online /Cleanup-Image /StartComponentCleanup /ResetBase",
                PowerShellCommand = @"Start-Process -FilePath ""DISM.exe"" -ArgumentList ""/Online /Cleanup-Image /StartComponentCleanup /ResetBase"" -NoNewWindow -Wait",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-driver-store-backup",
                Name = "DriverStore 구버전 드라이버 백업 정리",
                Description = "DriverStore FileRepository에 누적된 구버전 그래픽/오디오 드라이버 패키지 소거",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"pnputil /enum-drivers",
                PowerShellCommand = @"pnputil /enum-drivers",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-wsearch-edb-shrink",
                Name = "Windows Search 색인 DB (Windows.edb) 압축",
                Description = "수 GB로 비대해진 Windows Search 인덱싱 데이터베이스를 초기화하여 대용량 공간 회수",
                Category = TaskCategory.Storage,
                Risk = RiskLevel.Safe,
                FastCliCommand = @"net stop wsearch & del /f /q ""C:\ProgramData\Microsoft\Search\Data\Applications\Windows\Windows.edb"" & net start wsearch",
                PowerShellCommand = @"Stop-Service -Name wsearch -Force -ErrorAction SilentlyContinue; Remove-Item ""C:\ProgramData\Microsoft\Search\Data\Applications\Windows\Windows.edb"" -Force -ErrorAction SilentlyContinue; Start-Service -Name wsearch -ErrorAction SilentlyContinue",
                IsSelected = true
            });

            list.Add(new OptimizationTask
            {
                Id = "stg-cipher-zero-wipe",
                Name = "디스크 빈 공간 난수 덮어쓰기 (자유 공간 포렌식 와이핑)",
                Description = "이미 삭제된 파일들의 복구 프로그램(Recuva 등)을 통한 복원을 원천 차단하기 위해 자유 공간에 0x00 난수 덮어쓰기 [특수 기능: 자동 선택 제외]",
                Category = TaskCategory.Special,
                SubCategory = "Forensics",
                Risk = RiskLevel.Risky,
                FastCliCommand = @"cipher /w:C:",
                PowerShellCommand = @"cipher /w:C:",
                IsSelected = false
            });

            for (int i = 0; i < list.Count; i++)
            {
                list[i].SequenceNumber = i + 1;
            }

            return list;
        }
    }
}
