# 📝 WinPurify Pro - 작업 로그 (Work Log)

> **프로젝트**: WinPurify Pro (Remix WinPurify3-windowform-net)  
> **총괄 개발자**: AhBiYout (WinPurify Pro)  
> **현재 공식 버전**: `v4.71.1`  
> **공식 저장소**: [https://github.com/AhBiYout-all/WinPurifyPro](https://github.com/AhBiYout-all/WinPurifyPro)  
> **공식 블로그**: [https://ahbiyoutvibe.blogspot.com/](https://ahbiyoutvibe.blogspot.com/)  

---

## 📅 작업 내역 요약 (Recent Milestones)

### [2026-11-27] v4.71.1 전수 자아성찰(Self-Reflection) 및 보안·재부팅 예약·원격 매트릭스·UI 수정
1. **보호 경로 & Junction 차단 & 재부팅 예약 순서 교정 (`FreshHandoverService.cs`, `HandoverPolicyService.cs`, `electron-ipc-shared.js`)**:
   - `ProgramFiles`, `ProgramFiles(x86)`, `CommonApplicationData`, 현재 `UserProfile` 루트 보호를 추가하고 NTFS `ReparsePoint` 재귀 진입을 차단.
   - `PendingFileRenameOperations` 등록 시 잠금 하위 파일을 먼저 예약하고 상위 디렉터리를 후순위 예약하는 상향식(Bottom-Up) 소거로 교정.
   - 부분 예외(`PathPrefix`) 적용 시 `DetectedCount`/`EstimatedBytes` 재계산 및 오류 시 회수 용량 과다 계상 방지.
2. **원격 제어 무셸 전환 & Fleet 검증 매트릭스 수정 (`RemoteAgentService.cs`, `HandoverFleetModal.tsx`, `index.tsx`)**:
   - `/browser-factory-reset`에서 `RunHiddenCmd`를 제거해 직접 프로세스 종료 + 안전 디렉터리 소거로 전환.
   - `HandoverFleetModal`의 `RemainingProfiles`/`RemainingSessions`/`Reclaimed`/`Verified` 파싱 정규식 교정 및 Commander `clusterKey` 전달 누락 수정.
3. **Electron 스캐너 확장 & WPF UI 보완 (`electron-ipc-shared.js`, `ValueConverters.cs`, `MainWindow.xaml`, `MainViewModel.Handover.cs`)**:
   - Slack/Telegram/Zoom/Adobe OOBE/OneDrive/.ssh/NPKI/JumpLists 스캔 및 안전 프로세스 허용 목록 선종료 추가, WPF 필터 활성 버튼 하이라이트 및 예약 건수 표시 반영.

### [2026-11-27] v4.71.0 Clean Handover 4대 기능 고도화 (WPF & React/Electron 통합 구현)
1. **클린 인수인계 완결성 · 잠금 파일 재부팅 예약 · 인증서 발급 (`HandoverPolicyService.cs`, `FreshHandoverService.cs`, `electron-ipc-shared.js`, `HandoverWorkspace.tsx`)**:
   - 잠긴 파일/폴더를 `HKLM\...\Session Manager\PendingFileRenameOperations`에 등록하고, 정화 배너에서 예약 목록 확인·안전 재부팅 실행·HTML/JSON 오프라인 인증서 내보내기 구현.
2. **AI/개발도구 탐지 확장 · 영구 보존 예외(Whitelist) 관리자**:
   - `Cursor`, `VS Code Auth/Copilot`, `GitHub CLI`, `Docker Desktop`, `Postman`, `Figma` 세션을 탐지 대상으로 추가하고 `ArtifactId`/`PathPrefix` 예외 규칙 영구 저장(`handover_exclusions.json` / `handover-exclusions.json`) 구현.
3. **실시간 4단계 진행률 · 필터별 일괄 선택 · 상세/잠금 드로어 UX**:
   - WPF(`IProgress`) 및 Electron(`handover-progress`) 실시간 4단계 진행 바, 필터 범위 `전체 선택/선택 해제` 및 경로·잠금 프로세스 힌트 상세 패널 구축.
4. **LAN 작업실 일괄 인수인계 검증 매트릭스 (`HandoverFleetModal.tsx`, `RemoteAgentService.cs`, `remote-center.html`)**:
   - 다중 PC 대상 일괄 배포 모달과 PC별 검증 상태(`검증 통과`/`재부팅 대기`) 매트릭스 및 `remote-center.html` 프리셋 선택기 구현.

### [2026-11-27] v4.70.1 전수 자아성찰(Self-Reflection) 및 보안·기능·문서 일괄 수정
1. **보안 및 무결성 강화 (`FreshHandoverService.cs`, `electron-ipc-shared.js`, `dev-server.js`)**:
   - Windows 자격증명 삭제를 `cmdkey` 셸 호출에서 Win32 `Advapi32.dll` (`CredEnumerateW`/`CredDeleteW`) 직접 P/Invoke로 전환하고, `reg.exe`/`net.exe`/CIM 정리를 `ArgumentList` 무셸 실행 및 엄격한 SID 정규식 검증으로 강화.
   - `FreshHandoverService` Step 4 2차 검증에서 실제 디스크/자격증명 재스캔을 수행하도록 교정하고 `Process` 핸들 누수 및 부분 실패 상태 보고를 정확히 반영.
   - Electron `execute-handover-purge`에 `os.homedir()`, 드라이브 루트, `SystemRoot`, `userData`, `defaultuser0` 및 심볼릭 링크 차단 가드와 개인 폴더 보존 비우기(`emptyRootPaths`)를 추가하고 `FreshHandover` 원격 명령에 Challenge-Response 핸드셰이크를 강제.
   - `dev-server.js` 정적 경로 경계 확인(`+ path.sep`), 도트파일/소스 차단, 1MB 페이로드 상한 및 `cross-site` 차단 적용.
2. **WPF / Electron / Commander UI 연동 교정 (`MainWindow.xaml`, `MainViewModel.Handover.cs`, `RemoteAgentService.cs`, `index.tsx`, `remote-center.html`)**:
   - 사이드바 워크스페이스/카테고리 활성 하이라이트 충돌 해결, 보호 프로필/항목 체크박스 비활성화 바인딩, `FreshHandover` DTO(`presetName`/`preset`) 일관화 및 `remote-center.html` 일괄 초기화 버튼 추가.
3. **`/docs/` 문서 현행화 & 검증 기록 (`walkthrough.md`)**:
   - `README.md`, `SECURITY_AUDIT_REPORT.md`, `WPF_MVVM_ARCHITECTURE_GUIDE.md`, `REMOTE_COMMANDER_SPEC.md`, `CLI_AUTOMATION_GUIDE.md`, `WINDOWS_SERVICE_GUIDE.md`, `UNATTENDED_INSTALLATION_GUIDE.md`, `MODULES_PRECISION_DIAGNOSTICS_REPORT.md`를 `v4.70.1` 기준으로 전면 동기화.

### [2026-11-22] v4.70.0 Clean Handover & Fresh Environment Suite 전면 개편
1. **앱 정체성 재확립**: 실행 즉시 이전 사용자 프로필(`ProfileList` SID + 고립 폴더), 다중 브라우저 프로필, 앱/메신저 세션, OS 자격 증명을 비파괴 자동 탐지하여 새 사용자에게 깨끗한 PC 환경을 제공하는 워크스페이스를 기본 화면으로 구축.

### [2026-11-15 ~ 11-22] v4.69.0 ~ v4.69.2 TLS 9874 · DPAPI 금고 · JSON 단일 카탈로그 · XAML 검증 정비
1. ECDSA P-256 TLS(9874) + TOFU 피닝 + DPAPI/`safeStorage` 키 금고 + 6자리 PIN 페어링 + `modules.manifest.json` 단일화 + `validate-xaml.ps1` 보간 구문(`${var}`) 교정.

### [2026-11-01] v4.62.0 Central Commander & WinPurify Pro 한 PC 동시 실행(Coexistence) 전면 지원
1. **사용자 요청 사항 완벽 반영**: "커맨더와 WinPurifyPro가 한 pc에서 동시에 실행 되게 하고 싶다."
2. **동시 실행 아키텍처 구축**:
   - 상호 차단 로직 전면 해제: Central Commander와 WinPurify Pro 메인 앱이 한 머신에서 나란히 동시 실행 가능.
   - 독립 뮤텍스 분리: `AppMutexName`, `CommanderMutexName` 각자 자기 자신의 중복만 방지.
   - 소켓 포트 충돌 방지: UDP Discovery에 `SO_REUSEADDR` 적용, TCP 에이전트 포트 이미 가동 시 공유 채널 자동 전환.
   - `LaunchCommanderProcess`: 메인 앱 종료 없이 Central Commander 프로세스를 동시 기동.
   - 웹 UI: 동시 실행 상태 뱃지 `[중앙 관제 (동시 실행 중)]`, 상단 동시 실행 안내 배너 및 검사/최적화 병행 지원.
3. **버전 자동 동기화 및 패치노트 기록**:
   - `v4.62.0` 패치노트 작성 및 `node sync-version.js`로 전체 파일 동기화 완료.

### [2026-11-01] v4.61.0 Central Commander 구동 시 WinPurify Pro 상호 배타적 단일 인스턴스 보호 시스템 도입
1. **사용자 요청 사항 완벽 반영**: "커멘더가 실행되고 있을때 WinPurify Pro는 실행 안됨"
2. **상호 배타적 단일 인스턴스 보호 시스템 구축 (`SingleInstanceService.cs`)**:
   - Central Commander(중앙 관제 콘솔)가 실행 중인 상태에서 WinPurify Pro 기동 시, 포트(TCP 9870/UDP 9871), 서비스 및 C++ 커널 가속 엔진 충돌 방지 안내 팝업을 표시하고 안전하게 종료(`Shutdown`).
   - 이미 켜져 있는 Central Commander 윈도우를 자동 감지하여 포그라운드 최상위로 활성화(`SetForegroundWindow`).
   - 글로벌 뮤텍스 분리: `Global\WinPurifyPro_App_Mutex_Global_6904B914` 및 `Global\WinPurifyPro_Commander_Mutex_Global_6904B914`.
   - `Inno Setup` 설치 프로그램(`installer.iss`)의 `AppMutex`에 두 뮤텍스를 동시 등록하여 설치/업데이트 중 프로세스 실행 상태 감지 보강.
   - 웹 UI(`index.tsx`)에 상호 배타성 알림 배너, 헤더 펄스 뱃지 및 충돌 감지 대화상자 탑재.
3. **버전 자동 동기화 및 패치노트 기록**:
   - `docs/PATCH_NOTES.md` 및 `DOC/PATCH_NOTES.md`에 v4.61.0 작성 및 `node sync-version.js`를 통해 15개 파일 일괄 동기화 완료.

### [2026-10-04] v4.43.0 GitHub Releases 중복 바이너리 배포 전면 정리 (Zero Duplicates)
1. **사용자 요청 사항 완벽 반영**: "중복해서 빌드 올리는 것들 정리"
2. **배포 파이프라인 정돈 (`.github/workflows/release.yml`)**:
   - 기존 별칭(Alias)으로 2중 생성되던 중복 바이너리 복사본 제거.
   - 5대 핵심 바이너리(`WinPurifyPro-Slim.exe`, `WinPurifyCommander-Slim.exe`, `WinPurifyPro-Slim-Portable.zip`, `WinPurifyPro-Setup.exe`, `WinPurifyPro-Portable-x64.zip`)로 고유 단권화.

---

### [2026-10-04] v4.42.0 GitHub Actions Node.js 22 LTS 런타임 전환 & SxS 매니페스트 4자리 규격 보강
1. **GitHub 공식 EOL 정책 반영**:
   - `build-windows` 및 `publish-release` 잡을 `node-version: 22` (LTS)로 업그레이드하여 노란색 경고 완전 제거.
2. **Win32 SxS Side-by-Side 실행 오류 해결**:
   - `app.manifest`에 `version="4.43.0.0"` 4자리 어셈블리 포맷 적용 및 `sync-version.js` 매니페스트 동기화 정규식 보강.
   - `github_push.bat`을 PowerShell 무인 실행 기반으로 이관하여 폴더명 괄호 `(2)` 파싱 예외 완전 해결.

---

### [2026-10-04] v4.41.0 5MB대 초경량 슬림(Ultra-Slim) 단일 실행 바이너리 및 포터블 자동 빌드 파이프라인
1. **5MB대 초경량 단일 실행 바이너리 추가**:
   - `WinPurifyPro-Slim.exe` (~7.9MB) 및 `WinPurifyCommander-Slim.exe` (~4.5MB) Framework-Dependent 단일 파일 패키징.
2. **웹 UI 다운로드 모달 라인업 확장 (`index.tsx`)**:
   - 실시간 업데이트 모달 다이얼로그에 초경량 단일 파일 바로가기 뱃지 추가.

---

### [2026-10-04] v4.40.0 상단 헤더 서브타이틀 텍스트 간소화 및 레이아웃 정리
1. **웹 클라이언트 (`index.tsx`) 상단 브랜딩 레이아웃 정제**:
   - 부제목 문구(`Decoupled Native C++...`)를 제거하고 `블로그 • 라이선스` 미니멀 링크 구성.

---

### [2026-09-27] v4.34.0 시스템 트레이 작업 진행사항 실시간 툴팁 및 Windows 액션 센터 알림 기능 탑재
1. **사용자 요청 사항 완벽 구현**:
   - "시스템 트레이에서 작업 진행사항 및 알림 기능."
2. **Win32 P/Invoke 기반 네이티브 시스템 트레이 서비스 구축 (`Services/SystemTrayService.cs`)**:
   - `Shell_NotifyIconW` 기반 초저지연 트레이 아이콘 등록/갱신/삭제 엔진 탑재.
   - 트레이 아이콘 더블 클릭 시 메인 윈도우 즉각 최상위 복원 및 활성화(`SetForegroundWindow`).
   - 우클릭 시 빠른 컨텍스트 메뉴(빠른 정화, 전체 스캔, RAM 압축, 스케줄러, 트레이 설정, 로그 폴더, 종료) 제공.
3. **작업 진행사항(Progress) 실시간 툴팁 동적 갱신**:
   - 최적화 작업 중 마우스 오버 시 실시간 처리 퍼센트(%), 현재 처리 모듈명, 처리 건수를 툴팁으로 실시간 표출.
   - 대기 상태에서는 CPU/RAM 실시간 텔레메트리 및 시스템 상태 정보 제공.
4. **Windows 시스템 트레이 토스트/풍선 알림 파이프라인 탑재**:
   - 최적화 시작 알림, 25%/50%/75% 마일스톤 경과 알림, 최종 성공 및 공간 회수 완료 알림.
   - RAM 압축 완료 알림, Robocopy 스캔 완료 알림, 스케줄러 자동 실행 알림, 세이프포인트 복원 알림 완비.
   - Web UI 브라우저 환경에서 OS 네이티브 `Notification API` 지원.
5. **트레이 상주 정책 & UI/UX 전면 연동**:
   - 창 최소화 시 트레이 축소(Minimize to Tray) 및 창 닫기 시 트레이 상주(Close to Tray) 옵션 제공.
   - `MainWindow.xaml`: 사이드바 트레이 설정 버튼 및 전용 모달 다이얼로그 탑재.
   - `index.tsx`: Windows 11 작업 표시줄 트레이 시뮬레이터 HUD 및 플로팅 토스트 알림 센터 구현.

---

### [2026-09-27] v4.33.0 자동 최적화 태스크 스케줄러 인터페이스 구축 및 부팅/주기별 무음 정화 파이프라인 탑재
1. **사용자 요청 사항 완벽 구현**:
   - "Implement a task scheduler interface that allows users to configure automated runs for the optimization modules at specific intervals or on system startup."
2. **트리거 유형 4종 전면 지원**:
   - **🚀 시스템 부팅 시 (Startup/Logon)**: `/SC ONSTART` (부팅 즉시) 및 `/SC ONLOGON` (사용자 로그인 시) 트리거.
   - **부팅 안정화 대기 지연(Startup Delay)**: 0~15분 (기본 1분 지연) 설정으로 부팅 초기 과부하 방지.
   - **⏱️ 특정 주기 간격 반복 (Interval)**: 매 1, 2, 4, 6, 12, 24시간 간격 반복 실행 (`/SC HOURLY /MO N`).
   - **🔄 매일 정시 (Daily)** 및 **📅 매주 지정 요일 (Weekly)** 정밀 시각(시:분) 스케줄링.
3. **정화 대상 모듈 범위(Scope) 유연화**:
   - **프로필 프리셋 모드**: 안전 권장(Safe), 딥클린(Deep), 게이밍(Gaming), 개인정보(Privacy), 전체(All 115개).
   - **사용자 개별 모듈 지정 모드(Custom)**: 카테고리 필터링(개인정보/시스템/브라우저/스토리지/보안 등), 실시간 검색, 전체/안전 선택 토글을 통한 특정 모듈 타겟팅 정화.
4. **무음 헤드리스(Silent Headless) 백그라운드 런타임 구축**:
   - `App.xaml.cs` 및 `Services/AutoSchedulerService.cs`: `--schedule-run`, `--silent` 수신 시 GUI 창 표시 없이 백그라운드에서 지정 모듈 순차 실행 후 정상 종료.
   - 실행 결과 및 감사 로그를 `%ProgramData%\WinPurifyPro\Logs\AutoScheduler.log`에 실시간 기록.
   - **실시간 세이프포인트(Live Safepoint) 자동 백업** 및 **RAM Working Set 즉시 압축** 연동.
   - AC 전원 전용 옵션 및 절전 모드 자동 해제(Wake-to-run) 지원.
5. **UI/UX 대대적 고도화 및 즉시 테스트(Run Now) 인터랙션 탑재**:
   - `index.tsx`: 4개 탭(`실행 트리거`, `정화 모듈 범위`, `안전/전원 옵션`, `CLI 명령어 및 상태`) 구성.
   - 실시간 Windows `schtasks.exe` 명령어 미리보기 및 원클릭 복사 버튼.
   - **⚡ 지금 즉시 테스트 실행**: 백그라운드 자동 정화 시뮬레이션 및 실시간 프로그레스 바/단계별 피드백 제공.
   - `MainWindow.xaml` 및 `ViewModels/MainViewModel.cs`: 부팅 지연, 간격 시간, 모듈 스코프, 즉시 테스트 커맨드(`RunScheduleNowCommand`) 전면 바인딩.

---

### [2026-09-26] v4.32.0 보안 취약점 전면 보완 및 앱 정보 정합성 전수 동기화
1. **보안 헛점 정밀 점검 및 4-Tier 방어 아키텍처 구축**:
   - `X-WinPurify-Nonce` 120초 만료 캐시 기반 재전송 공격(Replay Attack) 방어 실드 탑재.
   - `RandomNumberGenerator` 기반 256-bit 고유 암호학적 클러스터 키 생성 및 `%ProgramData%\WinPurifyPro\cluster.key` 안전 보관.
   - CORS 와일드카드 제거, `localhost`/`127.0.0.1` 신뢰 도메인 한정 및 브라우저 드라이브바이(Drive-by) 차단 보안 헤더 적용.
   - Windows 기본 프로필(`C:\Users\Default`) 복제 최소 권한(Least Privilege) ACL 강화 (`SYSTEM:F`, `Administrators:F`, `Users:(OI)(CI)RX`).
   - 원격 실행 명령 입력값 정규식 화이트리스트 검증(`ForensicsTargetDrive: ^[A-Z]:$` 등).
   - 종합 보안 점검 보고서(`docs/SECURITY_AUDIT_REPORT.md`) 신설.

2. **현재 앱 정보 정합성 전수 검사 및 불일치 수정**:
   - `build.bat`: 하드코딩된 구버전 `APP_VERSION=4.23.7`을 최신 버전 `v4.32.0`으로 전면 보정.
   - `build.ps1`: Inno Setup 인스톨러 생성 파일명의 하드코딩 구버전 `v4.23.5`를 최신 버전 `v4.32.0`으로 전면 보정.
   - `WinPurifyCommander.csproj`: `<InformationalVersion>4.10.0</InformationalVersion>`을 `4.32.0 (AhBiYout)`으로 일치화.
   - `WinPurifyPro.csproj`: `<InformationalVersion>4.4.0</InformationalVersion>`을 `4.32.0 (AhBiYout)`으로 일치화.
   - `CommanderMainWindow.xaml`: 타이틀 및 헤더 배지 `v4.27.0`을 `v4.32.0`으로 일치화.
   - `app.manifest`: `assemblyIdentity version="4.0.0.0"`을 `4.32.0.0`으로 갱신.
   - `ViewModels/MainViewModel.cs`: 하드코딩된 `"115개 모듈"` 스캔/스케줄 로그를 실제 적재된 동적 모듈 수치(`{AllTasks.Count}개 모듈`)로 전환.
   - `sync-version.js`: 14개 전 파일(C#, C++, XAML, Inno Setup, PowerShell, Batch, Web, Manifest, Metadata) 일괄 자동 동기화 엔진으로 전면 고도화.

---

### [2026-09-26] v4.31.0 소프트웨어 라이선스 정의 한국어/영어 2종 체계 탑재
1. **정식 EULA 라이선스 계약서 2종 신설**:
   - `LICENSE_KO.txt` & `docs/LICENSE_KO.md`: 한국어 최종 사용자 사용권 계약서.
   - `LICENSE_EN.txt` & `docs/LICENSE_EN.md`: 영문 글로벌 표준 End User License Agreement.
2. **Inno Setup 6 다국어 설치 마법사 연동**:
   - 한국어 설치 시 `LICENSE_KO.txt`, 영어 설치 시 `LICENSE_EN.txt` 자동 출력 및 동의 필수 절차 연동.
   - 시작 메뉴 및 프로그램 폴더에 2종 라이선스 바로가기 자동 배치.
3. **WPF 데스크톱 및 웹 관제 UI 라이선스 뷰어 탑재**:
   - WPF: 사이드바 '📜 라이선스 정의 (KO / EN)' 버튼 및 탭 전환 모달 연동.
   - Web: 헤더 브랜드 배지 및 모달 팝업, 클립보드 복사 및 다운로드 지원.

---

## 📊 현재 프로젝트 상태 요약
- **UI/UX 계층**: WPF (C# .NET 8 / .NET 10) + React 18 웹 관제 대시보드
- **엔진 코어**: C++17 x64 Native DLL (`PurifyEngineCore.dll`) + C# P/Invoke 계층
- **모듈 수**: 144개 전수 세부 최적화/정화 모듈
- **통신 포트**: TCP 9870 (REST 관제), UDP 9871 (LAN 탐색)
- **라이선스 체계**: 한국어 및 영어 2종 정식 EULA 완비
- **보안 등급**: Grade A+ (Nonce 재전송 방어, 256-bit 클러스터 키, 최소 권한 ACL 적용)
