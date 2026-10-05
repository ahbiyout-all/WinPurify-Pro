
# 🛡️ WinPurify v3.0: 115개 모듈 전수 기술 명세 (Technical Commands List)

본 문서는 WinPurify v3.0 엔진에 탑재된 총 115개의 최적화 모듈과 그에 대응하는 실제 PowerShell 명령어를 기술합니다. (제작자: AhBiYout) 

---

## 1. 개인정보 보호 (Privacy - 20 Modules)
사용자 활동 흔적 및 텔레메트리 데이터를 제거합니다.

| ID | 모듈명 | 실제 실행 명령어 (PowerShell/CMD) |
|:---|:---|:---|
| pri-telemetry | 진단/텔레메트리 | `Remove-Item "C:\ProgramData\Microsoft\Diagnosis\DownloadedSettings\*" -Recurse -Force -ErrorAction SilentlyContinue` |
| pri-quick-access | 바로 가기/최근 항목 | `Remove-Item "$env:APPDATA\Microsoft\Windows\Recent\*" -Recurse -Force -ErrorAction SilentlyContinue` |
| pri-search-history | Windows 검색 기록 | `Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Search\RecentSearches" -Name "*" -ErrorAction SilentlyContinue` |
| pri-shell-bags | 폴더 뷰(Shell Bags) 초기화 | `Remove-Item -Path "HKCU:\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags" -Recurse -Force; Remove-Item -Path "HKCU:\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU" -Recurse -Force` |
| pri-jump-lists | 작업 표시줄 점프 목록 | `Remove-Item "$env:APPDATA\Microsoft\Windows\Recent\AutomaticDestinations\*" -Force` |
| pri-run-mru | 실행/검색 MRU 기록 | `Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU" -Name "*" -ErrorAction SilentlyContinue` |
| pri-recent-docs | 레지스트리 최근 문서 | `Remove-Item -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\RecentDocs" -Recurse -Force` |
| pri-clipboard | 클립보드 기록 | `echo off | clip` |
| pri-typed-paths | 탐색기 주소창 입력 기록 | `Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths" -Name "*"` |
| pri-user-assist | UserAssist 앱 실행 통계 | `Get-ChildItem "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\UserAssist" | Remove-Item -Recurse -Force` |
| pri-activity-timeline | 활동 타임라인 기록 | `Get-Service -Name "CDPSvc" | Stop-Service -Force; Remove-Item "$env:LOCALAPPDATA\ConnectedDevicesPlatform\*" -Recurse -Force; Start-Service "CDPSvc"` |
| pri-office-mru | Office 최근 사용 목록 | `Get-ChildItem "HKCU:\Software\Microsoft\Office\*\*\User MRU" | Remove-Item -Recurse -Force` |
| pri-winrar-mru | WinRAR 사용 기록 | `Remove-Item -Path "HKCU:\Software\WinRAR\ArcHistory" -Force` |
| pri-paint-mru | 그림판 최근 목록 | `Remove-Item -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Applets\Paint\Recent File List" -Recurse -Force` |
| pri-wordpad-mru | 워드패드 최근 목록 | `Remove-Item -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Applets\Wordpad\Recent File List" -Recurse -Force` |
| pri-wallpaper-mru | 배경화면 변경 기록 | `Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers" -Name "BackgroundHistoryPath*"` |
| pri-explorer-breadcrumb| 탐색기 이동 경로 캐시 | `Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\TypedPaths" -Name "*"` |
| pri-taskbar-mru | 작업 표시줄 핀 고정 초기화 | `Remove-Item "$env:APPDATA\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar\*" -Force` |
| pri-thumb-global | 전체 썸네일 데이터베이스 | `taskkill /F /IM explorer.exe; del /s /q /f /a s %LocalAppData%\Microsoft\Windows\Explorer\thumbcache_*.db; start explorer.exe` |
| pri-generic-mru-list | 범용 최근 사용 목록 | `Get-ChildItem "HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\*MRU*" | Remove-Item -Recurse -Force` |

---

## 2. 브라우저 최적화 (Browser - 15 Modules)
웹 서핑 중 발생한 캐시와 개인 흔적을 소거합니다.

| ID | 모듈명 | 실제 실행 명령어 (PowerShell/CMD) |
|:---|:---|:---|
| brw-edge-cache | Edge 웹 캐시 | `Get-ChildItem "$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Cache\*" -Recurse | Remove-Item -Force` |
| brw-edge-history | Edge 방문 기록 | `Remove-Item "$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\History" -Force` |
| brw-edge-cookies | Edge 쿠키/로그인 데이터 | `Remove-Item "$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Cookies" -Force` |
| brw-chrome-cache | Chrome 웹 캐시 | `Get-ChildItem "$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Cache\*" -Recurse | Remove-Item -Force` |
| brw-chrome-history | Chrome 방문 기록 | `Remove-Item "$env:LOCALAPPDATA\Google\Chrome\User Data\Default\History" -Force` |
| brw-chrome-cookies | Chrome 쿠키/로그인 데이터 | `Remove-Item "$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Cookies" -Force` |
| brw-edge-sessions | Edge 활성 세션 데이터 | `Remove-Item "$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Sessions\*" -Force` |
| brw-chrome-gpu-cache | Chrome GPU 가속 캐시 | `Remove-Item "$env:LOCALAPPDATA\Google\Chrome\User Data\Default\GPUCache\*" -Force` |
| brw-edge-media-cache | Edge 미디어 스트리밍 캐시 | `Remove-Item "$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Media Cache\*" -Force` |
| brw-chrome-downloads | Chrome 다운로드 이력 | `Remove-Item "$env:LOCALAPPDATA\Google\Chrome\User Data\Default\Download Service\*" -Force` |
| brw-chrome-shader-cache| Chrome 셰이더 컴파일 캐시 | `Remove-Item "$env:LOCALAPPDATA\Google\Chrome\User Data\ShaderCache\*" -Force` |
| brw-edge-dom-storage | Edge 로컬 스토리지(DOM) | `Remove-Item "$env:LOCALAPPDATA\Microsoft\Edge\User Data\Default\Local Storage\*" -Force` |
| brw-ie-temporary-files | Legacy IE 임시 인터넷 파일 | `RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 8` |
| brw-ie-history-legacy | Legacy IE 방문 기록 | `RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 1` |
| brw-ie-form-data-legacy| Legacy IE 양식 데이터 | `RunDll32.exe InetCpl.cpl,ClearMyTracksByProcess 16` |

---

## 3. 보안 센터 (Security - 10 Modules)
보안 로그 및 격리된 위협 데이터를 정리합니다.

| ID | 모듈명 | 실제 실행 명령어 (PowerShell/CMD) |
|:---|:---|:---|
| sec-defender-scan-logs | Defender 검사 이력 로그 | `Remove-Item "C:\ProgramData\Microsoft\Windows Defender\Scans\History\Results\*" -Recurse -Force` |
| sec-defender-quarantine| Defender 격리소 파일 소거 | `Remove-Item "C:\ProgramData\Microsoft\Windows Defender\Quarantine\*" -Recurse -Force` |
| sec-firewall-log-files | Windows 방화벽 로그 | `Remove-Item "C:\Windows\System32\LogFiles\Firewall\*" -Force` |
| sec-security-health | 보안 상태 대시보드 캐시 | `Remove-Item "$env:LOCALAPPDATA\Microsoft\Windows\SecurityHealth\*" -Recurse -Force` |
| sec-defender-support | Defender 지원/진단 데이터 | `Remove-Item "C:\ProgramData\Microsoft\Windows Defender\Support\*" -Recurse -Force` |
| sec-audit-policy-logs | 시스템 감사 정책 로그 | `wevtutil cl Security` |
| sec-defender-signatures| 구버전 백신 시그니처 | `Remove-Item "C:\ProgramData\Microsoft\Windows Defender\Definition Updates\*" -Recurse -Force` |
| sec-vault-credentials | 자격 증명 보관함 캐시 | `Remove-Item "$env:LOCALAPPDATA\Microsoft\Vault\*" -Recurse -Force` |
| sec-defender-reporting | Defender 보고 데이터 | `Remove-Item "C:\ProgramData\Microsoft\Windows Defender\Reporting\*" -Recurse -Force` |
| sec-smartscreen-logs | SmartScreen 검사 로그 | `Remove-Item "$env:LOCALAPPDATA\Microsoft\Windows\SmartScreen\*" -Recurse -Force` |

---

## 4. 레지스트리 (Registry - 15 Modules)
레지스트리 하이브의 무효한 참조를 제거합니다.

| ID | 모듈명 | 실제 실행 명령어 (PowerShell/CMD) |
|:---|:---|:---|
| reg-mui-cache-clean | MUI 인터페이스 캐시 | `Get-ChildItem "HKCU:\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache" | ForEach-Object { Remove-ItemProperty -Path $_.PSPath -Name "*" }` |
| reg-compat-flags | 호환성 레이어 기록 | `Remove-Item "HKCU:\Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers" -Force` |
| reg-shell-ext-broken | 쉘 익스텐션 파편 | `Get-ChildItem "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" | ForEach-Object { if(-not (Test-Path $_.Name)) { Remove-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" -Name $_.PSChildName } }` |
| reg-firewall-orphans | 방화벽 무효 규칙 | `Get-NetFirewallRule | Where-Object { $_.AppPath -and -not (Test-Path $_.AppPath) } | Remove-NetFirewallRule` |
| reg-clsid-orphans | 부모 없는 CLSID | `Get-ChildItem "HKCR:\CLSID" | ForEach-Object { if($_.GetValue("") -eq $null -and -not $_.HasSubKeys) { Remove-Item $_.PSPath -Force } }` |
| reg-app-paths-invalid | 무효 앱 경로 정보 | `Get-ChildItem "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths" | ForEach-Object { if(-not (Test-Path (Get-ItemProperty $_.PSPath)."(default)")) { Remove-Item $_.PSPath -Force } }` |
| reg-shared-dlls | 공유 DLL 잔여물 | `Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\SharedDlls" | Get-Member -MemberType NoteProperty | ForEach-Object { if(-not (Test-Path $_.Name)) { Remove-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\SharedDlls" -Name $_.Name } }` |
| reg-sound-events | 무효 사운드 이벤트 | `Remove-Item "HKCU:\AppEvents\Schemes\Apps\.Default" -Recurse -ErrorAction SilentlyContinue` |
| reg-font-associates | 글꼴 연결 오류 수정 | `Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\FontSubstitutes" | Get-Member -MemberType NoteProperty | ForEach-Object { if(-not $_.Name) { Remove-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\FontSubstitutes" -Name $_.Name } }` |
| reg-help-paths | 도움말 파일 경로 파편 | `Remove-Item "HKLM:\SOFTWARE\Microsoft\Windows\Help" -ErrorAction SilentlyContinue` |
| reg-installer-fragments| 인스톨러 분리 파편 | `Get-ChildItem "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products" | ForEach-Object { if(-not (Test-Path $_.PSPath)) { Remove-Item $_.PSPath -Recurse -Force } }` |
| reg-user-classes | 사용자 클래스 파편 | `Get-ChildItem "HKCU:\Software\Classes\CLSID" | ForEach-Object { if(-not $_.HasSubKeys) { Remove-Item $_.PSPath -Force } }` |
| reg-run-keys-invalid | 자동 실행 무효 키 | `Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" | Get-Member -MemberType NoteProperty | ForEach-Object { if(-not (Test-Path (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run").$($_.Name))) { Remove-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" -Name $_.Name } }` |
| reg-start-menu-links | 시작 메뉴 무효 링크 | `Get-ChildItem "$env:ProgramData\Microsoft\Windows\Start Menu\Programs" -Recurse -Include *.lnk | ForEach-Object { if(-not (Test-Path $_.FullName)) { Remove-Item $_.FullName -Force } }` |
| reg-typelib-orphans | TypeLib 분리 파편 | `Get-ChildItem "HKCR:\TypeLib" | ForEach-Object { if(-not $_.HasSubKeys) { Remove-Item $_.PSPath -Force } }` |

---

## 5. 업데이트 관리 (Update - 15 Modules)
Windows Update 과정에서 발생한 거대 파편을 제거합니다.

| ID | 모듈명 | 실제 실행 명령어 (PowerShell/CMD) |
|:---|:---|:---|
| upd-cache-clean | 업데이트 다운로드 캐시 | `Stop-Service wuauserv -Force; Remove-Item "C:\Windows\SoftwareDistribution\Download\*" -Recurse -Force; Start-Service wuauserv` |
| upd-winsxs-reset | WinSxS 정밀 압축/청소 | `Dism.exe /online /Cleanup-Image /StartComponentCleanup /ResetBase` |
| upd-delivery-opt | 전송 최적화 캐시 소거 | `Stop-Service DoSvc -Force; Remove-Item "C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache\*" -Recurse -Force; Start-Service DoSvc` |
| upd-bits-queue-reset | BITS 전송 대기열 초기화 | `bitsadmin /reset /allusers` |
| upd-windows-old-del | 이전 Windows 설치본 | `Remove-Item "C:\Windows.old" -Recurse -Force -ErrorAction SilentlyContinue` |
| upd-driver-store-tmp | 드라이버 설치 임시 잔해 | `Remove-Item "C:\Windows\System32\DriverStore\Temp\*" -Recurse -Force` |
| upd-msi-patch-cache | MSI 패치 백업 캐시 | `Remove-Item "C:\Windows\Installer\$PatchCache$\*" -Recurse -Force` |
| upd-cbs-logs-all | CBS 매니페스트 로그 | `Remove-Item "C:\Windows\Logs\CBS\*" -Recurse -Force` |
| upd-store-cache-dat | Store 패키지 다운로드 캐시 | `Remove-Item "C:\Windows\SoftwareDistribution\DataStore\*" -Recurse -Force` |
| upd-pending-rename | 파일 이름 변경 대기열 소거 | `Remove-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager" -Name "PendingFileRenameOperations"` |
| upd-manifest-cache | 컴포넌트 매니페스트 캐시 | `Remove-Item "C:\Windows\WinSxS\ManifestCache\*" -Force` |
| upd-reboot-required | 시스템 재부팅 보류 플래그 | `Remove-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired" -Name "*"` |
| upd-wu-text-logs | Windows Update 텍스트 로그 | `Remove-Item "C:\Windows\WindowsUpdate.log" -Force` |
| upd-office-temp-cache | Office 배포 패키지 캐시 | `Remove-Item "C:\Windows\Temp\OfficeSetup\*" -Recurse -Force` |
| upd-sqm-data-all | SQM 품질 개선 통계 데이터 | `Remove-Item "C:\Windows\ServiceProfiles\LocalService\AppData\Local\Microsoft\Windows\WSQM\*" -Recurse -Force` |

---

## 6. 시스템 최적화 (System - 15 Modules)
네트워크와 시스템 가속 성능을 개선합니다.

| ID | 모듈명 | 실제 실행 명령어 (PowerShell/CMD) |
|:---|:---|:---|
| sys-dns-cache-flush | DNS 해석기 캐시 초기화 | `ipconfig /flushdns` |
| sys-icon-cache-reset | 시스템 아이콘 캐시 재생성 | `taskkill /F /IM explorer.exe; Remove-Item "$env:LOCALAPPDATA\Microsoft\Windows\Explorer\iconcache_*.db" -Force; start explorer.exe` |
| sys-shader-cache | DirectX 그래픽 셰이더 캐시 | `Get-ChildItem -Path "$env:LOCALAPPDATA\D3DSCache\*" -Recurse | Remove-Item -Force` |
| sys-net-stack-reset | 네트워크 스택/IP 리셋 | `netsh winsock reset; netsh int ip reset` |
| sys-spooler-reset | 프린터 인쇄 대기열 초기화 | `Stop-Service Spooler; Remove-Item "C:\Windows\System32\spool\PRINTERS\*" -Force; Start-Service Spooler` |
| sys-wsreset-cache | MS Store 서비스 캐시 리셋 | `wsreset.exe` |
| sys-timesync-all | 표준 시간 서버 동기화 | `w32tm /resync` |
| sys-wmi-salvage | WMI 관리 데이터베이스 리셋 | `winmgmt /salvagerepository` |
| sys-winhttp-proxy | 시스템 프록시 구성 초기화 | `netsh winhttp reset proxy` |
| sys-arp-cache-flush | ARP 테이블 캐시 초기화 | `netsh interface ip delete arpcache` |
| sys-perf-counters | 성능 측정 카운터 리셋 | `lodctr /r` |
| sys-com-catalog-reset | COM+ 구성 카탈로그 초기화 | `Remove-Item "C:\Windows\Registration\*.clb" -Force` |
| sys-winsock-reset | Winsock 소켓 카탈로그 리셋 | `netsh winsock reset` |
| sys-system32-temp | System32 루트 임시 파일 | `Remove-Item "C:\Windows\System32\Temp\*" -Recurse -Force` |
| sys-event-logs-clear | 윈도우 전체 이벤트 로그 | `wevtutil el | Foreach-Object {wevtutil cl "$_"}` |

---

## 7. 저장공간 (Storage - 25 Modules)
가장 큰 용량을 차지하는 임시 파일들을 정밀 제거합니다.

| ID | 모듈명 | 실제 실행 명령어 (PowerShell/CMD) |
|:---|:---|:---|
| stg-windows-temp | Windows 공용 임시 폴더 | `Remove-Item "C:\Windows\Temp\*" -Recurse -Force` |
| stg-user-temp-data | 사용자 프로필 임시 폴더 | `Remove-Item "$env:TEMP\*" -Recurse -Force` |
| stg-recycle-bin-all | 모든 드라이브 휴지통 | `Clear-RecycleBin -Force -ErrorAction SilentlyContinue` |
| stg-crash-memory-dump | 시스템 전체 메모리 덤프 | `Remove-Item "C:\Windows\memory.dmp" -Force` |
| stg-prefetch-all | Prefetch 가속 데이터 소거 | `Remove-Item "C:\Windows\Prefetch\*" -Recurse -Force` |
| stg-wer-reports | 사용자 계정 오류 보고서 | `Remove-Item "$env:LOCALAPPDATA\Microsoft\Windows\WER\*" -Recurse -Force` |
| stg-app-crash-dumps | 개별 앱 크래시 덤프 | `Remove-Item "$env:LOCALAPPDATA\CrashDumps\*" -Recurse -Force` |
| stg-font-render-cache | 시스템 글꼴 렌더링 캐시 | `Stop-Service FontCache; Remove-Item "C:\Windows\ServiceProfiles\LocalService\AppData\Local\FontCache\*" -Force; Start-Service FontCache` |
| stg-iis-log-files | IIS 웹 서버 로그 파일 | `Remove-Item "C:\inetpub\logs\LogFiles\*" -Recurse -Force` |
| stg-catroot2-cleanup | 암호화 카탈로그 저장소 캐시 | `Stop-Service CryptSvc; Remove-Item "C:\Windows\System32\catroot2\*" -Recurse -Force; Start-Service CryptSvc` |
| stg-nvidia-gl-cache | NVIDIA OpenGL 셰이더 캐시 | `Remove-Item "$env:LOCALAPPDATA\Local\NVIDIA\GLCache\*" -Recurse -Force` |
| stg-directx-shader | DirectX 범용 셰이더 캐시 | `Remove-Item "$env:LOCALAPPDATA\D3DSCache\*" -Recurse -Force` |
| stg-minidumps-all | 미니덤프 진단 요약본 | `Remove-Item "C:\Windows\Minidump\*" -Force` |
| stg-inf-install-logs | 드라이버 설치 이력 로그 | `Remove-Item "C:\Windows\inf\setupapi.dev.log" -Force` |
| stg-bits-cache | BITS 백그라운드 전송 캐시 | `Remove-Item "C:\ProgramData\Microsoft\Network\Downloader\*" -Recurse -Force` |
| stg-package-cache | 대형 앱 패키지 설치 캐시 | `Remove-Item "C:\ProgramData\Package Cache\*" -Recurse -Force` |
| stg-setupapi-breadcrumb| 장치 드라이버 로딩 로그 | `Remove-Item "C:\Windows\inf\setupapi.dev.log" -Force` |
| stg-win-logfiles-all | 윈도우 범용 로그 데이터 | `Remove-Item "C:\Windows\System32\LogFiles\*" -Recurse -Force` |
| stg-desktop-bridge | 데스크톱 브릿지 가상화 데이터 | `Remove-Item "C:\ProgramData\Microsoft\Windows\AppRepository\*" -Recurse -Force` |
| stg-dism-work-logs | DISM 이미지 복구 로그 | `Remove-Item "C:\Windows\Logs\DISM\*" -Force` |
| stg-local-temp-extra | 앱데이터 로컬 임시 잔해 | `Remove-Item "$env:LOCALAPPDATA\Temp\*" -Recurse -Force` |
| stg-was-activation | WAS 서비스 실행 로그 | `Remove-Item "C:\Windows\System32\LogFiles\WAS\*" -Recurse -Force` |
| stg-msi-installer-tmp | MSI 인스톨러 임시 데이터 | `Remove-Item "C:\Windows\Installer\*.tmp" -Force` |
| stg-sys-profile-temp | 시스템 프로필 전용 임시 파일 | `Remove-Item "C:\Windows\ServiceProfiles\LocalService\AppData\Local\Temp\*" -Recurse -Force` |
| stg-global-wer-clear | 전체 오류 보고 통합 저장소 | `Remove-Item "C:\ProgramData\Microsoft\Windows\WER\*" -Recurse -Force` |

---

### 🛡️ 실행 보안 정책 (Security Execution)
모든 명령어는 실행 전 **PowerShell EncodedCommand** 방식으로 변환되어 전달됩니다. 이는 한글 경로 및 특수 문자로 인한 스크립트 오류를 방지하고, 실행 정책(`Bypass`)을 우회하여 정확한 동작을 보장하기 위함입니다.

---

## 🧭 레지스트리 최적화 가이드 및 안전성 등급 분류 (Registry Optimization & Safety Classification)

윈도우 시스템 최적화에서 레지스트리 정리는 성능 조율과 프라이버시 보호에 있어 중요한 기둥이지만, 무분별한 소거는 시스템 불안정성을 유발할 수 있습니다. 아래 가이드는 WinPurify v3.0 관점에서 레지스트리 정리의 필요성과 안전(Safe)/위험(Risky) 항목의 명확한 경계를 규정합니다.

### 1. 레지스트리 정리 기능이 필요합니까? (Is it necessary?)
**결론: 예, 특정 조건과 엄격한 제어 하에 매우 효과적입니다.**
레지스트리는 운영체제와 애플리케이션의 핵심 구성 정보를 저장하는 거대 트리 데이터베이스입니다. 프로그램 설치/제거가 반복되면 존재하지 않는 DLL 참조, 잘못된 실행 파일 주소(App Paths), 껍데기만 남은 고립된 CLSID 등이 누적됩니다.
- **성능 이점**: 잘못 매핑된 시스템 호출 대기 시간 단축, 레지스트리 하이브 파일 크기 축소에 따른 메모리 점유율 개선 및 부팅 가속.
- **오류 예방**: 존재하지 않는 셸 익스텐션(Shell Extensions) 및 무효 앱 경로로 인한 탐색기 크래시 예방.
- **사생활 보호**: 파일 검색 기록, 최근 입력 경로, 실행 도구 이력 등의 레지스트리 기록을 제거하여 물리적 프라이버시 보존.

---

### 2. 안전한 레지스트리 정리 (Safe Clean) - "안전(Safe) 등급"
시스템 동작이나 타 애플리케이션의 뼈대 기능에 영향을 주지 않고, 찌꺼기 기록이나 단순 인터페이스 기록만 제거하는 안전한 항목들입니다.

* **MUI 인터페이스 캐시 (`reg-mui-cache-clean`)**:
  - 실행되었던 프로그램들의 다국어 셸 텍스트 캐시입니다. 소거 시 탐색기 갱신 가속에 기여하며 오작동 우려가 전혀 없습니다.
* **사용자 타이핑 및 실행 기록 (`sys-reg-mru`)**:
  - 윈도우 실행 창(RunMRU) 기록과 탐색기 상단에 다이렉트로 타이핑했던 경로(TypedPaths) 흔적을 소거합니다. 개인정보 누출을 막는 가장 기본적인 정리 작업입니다.
* **호환성 레이어 흔적 (`reg-compat-flags`)**:
  - 이전에 실행되었던 애플리케이션에 임시로 설정되었던 호환성 모드 정보를 리셋하여, 오작동 모드로 부당 고정된 설정을 해제합니다.
* **시작 메뉴 무효 바로가기 (`reg-start-menu-links`)**:
  - 프로그램이 지워졌음에도 시작 메뉴에 덩그러니 남은 빈 껍데기 LNK 링크들을 안전하게 추적하여 지웁니다.
* **무효 사운드 이벤트 (`reg-sound-events`)**:
  - 삭제된 프로그램이 레지스트리에 등록해둔 사운드 알림 이벤트를 제거하여 미디어 사운드 로딩 스레드의 낭비를 막습니다.

---

### 3. 위험한 레지스트리 정리 (Risky Clean) - "깊은(Deep) / 위험(Risky) 등급"
C++ 네이티브 모듈이나 고유 프레임워크와 하드웨어 드라이버가 사용하는 민감한 바인딩 정보들로, 과도하게 제거할 경우 타사 프로그램이 실행되지 않거나 정적 라이브러리 누락 에러(0xc000007b 등)가 발생할 수 있어 고도의 관리가 필요한 항목입니다.

* **부모 없는 클래스 식별자 (`reg-clsid-orphans`)**:
  - 연결할 실제 개체(Object)가 없는 CLSID 키입니다. 일부 보안 프로그램이나 액티브X, DRM 모듈이 레지스트리 청소 시 오감지되어 손상되면 은행 사이트나 기업망 접속 에러가 유발될 수 있습니다.
* **공유 DLL 잔여물 (`reg-shared-dlls`)**:
  - 여러 애플리케이션이 공유하는 동적 라이브러리 카운터입니다. 정리를 잘못하면 다른 프로그램에 사용 중인 DLL 파일이 레지스트리상에서 소멸 판정을 받아 프로그램이 실행되지 않는 치명적인 결과가 발생할 수 있습니다.
* **TypeLib 분리 파편 (`reg-typelib-orphans`)**:
  - 하위 구성 키나 구성 정의가 빈 껍데기인 라이브러리 정의로, 주로 OLE/COM 통신 도중 남겨진 데이터입니다. 닷넷(.NET) 계열 소프트웨어나 오피스 연동 컴포넌트 오류의 주원인이 될 수 있으므로 세심한 분석이 따르지 않으면 소거를 금해야 합니다.
* **인스톨러 분리 파편 (`reg-installer-fragments`)**:
  - MS 인스톨러(MSI)가 설치/업데이트 패치를 위해 보관하는 메타데이터입니다. 과도 소거 시 향후 해당 소프트웨어의 '수정', '추가 기능 설치' 또는 '안전한 제어판 제거'가 불가능해지는 부작용을 겪을 수 있습니다.

---

### 🛡️ WinPurify v3.0의 예방책 (Safety Mechanisms)
WinPurify v3.0은 레지스트리 최적화의 부작용을 원천 봉쇄하기 위해 이중 안전망을 전개합니다.
1. **사전 세이프포인트 수립**: 레지스트리 및 서비스 구성을 변경하기 직전, `Create Live Safepoint` 엔진을 통해 안전한 하이브(Hive) 백업본을 제작하고 필요시 **원클릭 복원(Undo)**을 즉각 허용합니다.
2. **엄격한 리스크 레이블링**: `Safe`, `Deep`, `Risky` 세 가지 등급으로 명확히 레이블을 차별화하고, 위험한 작업군은 사용자가 명시적으로 동의/선택하지 않는 한 기본 최적화 루틴에서 배제합니다.

