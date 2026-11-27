# WinPurify Pro - C# WPF MVVM 아키텍처 및 빌드 가이드 (`v4.71.1`)

본 문서는 **WinPurify Pro (`v4.71.1` — Clean Handover & Fresh Environment Suite)**의 C# .NET 10.0 WPF Partial MVVM 아키텍처, 서비스 계층, 단일 진실 원천 JSON 카탈로그(`modules.manifest.json`) 및 컴파일/패키징 방법을 상세히 설명합니다.

---

## 🏛️ 1. 솔루션 및 파일 구조 (Verified Repository Layout)

```
WinPurifyPro/
├── WinPurifyPro.sln                      # Visual Studio 솔루션 파일
├── WinPurifyPro.csproj                   # .NET 10.0-Windows 메인 WPF 앱 명세
├── WinPurifyCommander.csproj             # .NET 10.0-Windows 독립 Central Commander 앱 명세
├── modules.manifest.json                 # 144개 모듈 단일 진실 원천(Single Source of Truth)
├── App.xaml / App.xaml.cs                # 전역 예외 처리, CLI 라우팅(--handover-auto, --schedule-run, --service)
├── MainWindow.xaml / MainWindow.xaml.cs  # Clean Handover 기본 워크스페이스 & 144모듈/트레이/모달 UI
├── CommanderMainWindow.xaml              # 중앙 관제 전용 WPF 대시보드
├── app.manifest                          # UAC requireAdministrator 승격 매니페스트
│
├── 📂 Models/                            # [Model Layer] 도메인 모델 및 DTO
│   ├── HandoverModels.cs                 # DetectedUserProfile, DetectedSessionArtifact, HandoverVerificationSummary
│   ├── OptimizationTask.cs               # OptimizationTask, TaskCategory, RiskLevel, SafepointRecord
│   └── RemoteCommanderModels.cs          # NodeTelemetryDto, BatchCommandRequest, StudioPair*Dto, AuthChallengeResponseDto
│
├── 📂 ViewModels/                        # [ViewModel Layer] Partial ViewModel 분할 구조
│   ├── IMainViewModel.cs                 # 메인 뷰모델 계약 인터페이스
│   ├── RelayCommand.cs                   # ICommand 구현체
│   ├── MainViewModel.cs                  # 핵심 상태, 테마, 로그, 트레이, 커맨더 이벤트 바인딩
│   ├── MainViewModel.Handover.cs         # Clean Handover 워크스페이스 스캔·프리셋·소거·검증 오케스트레이션
│   ├── MainViewModel.Optimization.cs     # 144개/커스텀 모듈 스캔·필터링·실행·세이프포인트 롤백
│   ├── MainViewModel.Diagnostics.cs      # 자가 진단, 하드웨어 텔레메트리, GitHub 실시간 업데이트
│   └── CommanderViewModel.cs             # Central Commander 다중 PC 플릿 관제 뷰모델
│
├── 📂 Services/                          # [Service Layer] OS·네트워크·보안·스케줄러 엔진
│   ├── FreshHandoverService.cs           # 이전 사용자 SID/폴더·다중 브라우저·AI/개발도구·자격증명 탐지/소거/2차 검증
│   ├── HandoverPolicyService.cs          # Whitelist 예외 영구 저장, PendingFileRenameOperations 예약, HTML/JSON 인증서 생성
│   ├── AccountCredentialPurgeService.cs  # Win32 CredEnumerateW/CredDeleteW & 15종 계정/세션 정화 엔진
│   ├── MultiEngineDispatcher.cs          # 4중 Failover (Native C++ → Fast CLI → Embedded PS → Fallback)
│   ├── NativeEngineService.cs            # PurifyEngineCore.dll P/Invoke 브리지 (RAM Trim, 파일 잠금 해제 등)
│   ├── RobocopyScannerService.cs         # Stutter-Free 비동기 폴더 용량 스캐너
│   ├── LiveSafepointService.cs           # 정화 직전 .reg 스냅샷 생성 및 1-Click 복구
│   ├── CustomModuleService.cs            # modules.manifest.json 로더 및 사용자 동적 모듈 저장소
│   ├── ClusterSecurityVault.cs           # Win32 DPAPI 클러스터 키 금고, ECDSA P-256 TLS 인증서, 6자리 PIN
│   ├── RemoteTlsServer.cs                # 내장 SslStream TLS 1.2/1.3 수신기 (TCP:9874)
│   ├── RemoteAgentService.cs             # TLS 9874 / HTTP 9870 / UDP 9871 원격 에이전트 & 배치 디스패처
│   ├── RemoteAuthValidator.cs            # HMAC-SHA256, 1회용 Nonce, Challenge IP 바인딩, 평문 Secret 거부
│   ├── HybridNodeDiscoveryService.cs     # 멀티 NIC UDP 브로드캐스트 + /24 서브넷 TCP 병렬 탐색
│   ├── RemoteCommanderManager.cs         # Commander HTTPS(TOFU) / HTTP 클라이언트 관리자
│   ├── RemoteExecutionAuditService.cs    # 원격 실행 감사 이력 영구 저장
│   ├── FirewallService.cs                # Windows 방화벽 TCP 9874/9870 · UDP 9871 자동 규칙 구성
│   ├── AutoSchedulerService.cs           # schtasks 기반 Startup/Interval/Daily/Weekly 무음 유지보수
│   ├── SystemTrayService.cs              # Win32 Shell_NotifyIconW 트레이 아이콘 & 실시간 진행률 툴팁
│   ├── SystemDiagnosticsService.cs       # CPU/RAM/Disk 실시간 메트릭 수집
│   ├── SystemPowerService.cs             # 4-Tier 강제 재부팅/종료 Failover
│   ├── WindowsServiceHost.cs             # WinPurifyAgentService (Session 0) 호스트 및 SCM 설치/삭제
│   ├── SingleInstanceService.cs          # 앱/커맨더 독립 뮤텍스 관리
│   ├── GitHubUpdateService.cs            # GitHub Releases 실시간 버전 조회 및 자동 업데이트
│   ├── ProfilePresetManager.cs           # Safe/Deep/Gaming/Privacy/ZeroTrace 프리셋 관리자
│   ├── SelfDiagnosticTester.cs           # 시스템/모듈 무결성 자가 진단
│   ├── LoggerService.cs                  # 로컬 디스크 감사 로그 저장
│   └── AppEnvironment.cs                 # 포터블/서비스/버전 실행 환경 판별
│
├── 📂 Converters/                        # [Converters] XAML 데이터 바인딩 변환기
│   └── ValueConverters.cs                # RiskToBrush, CategoryToActive*, HandoverFilterActive*, BoolToVis, InvBool 등
│
└── NativeEngine/                         # C++17 네이티브 가속 소스 (PurifyEngineCore.dll)
```

---

## 🧩 2. 핵심 설계 원칙 (Key Architecture Patterns)

1. **Partial ViewModel 분할 패턴**:
   - 비대해지기 쉬운 `MainViewModel`을 핵심 상태(`MainViewModel.cs`), 클린 인수인계(`MainViewModel.Handover.cs`), 모듈 실행(`MainViewModel.Optimization.cs`), 진단·업데이트(`MainViewModel.Diagnostics.cs`) 4개 파일로 분리하여 유지보수성과 XAML 바인딩 일관성을 확보했습니다.
2. **Clean Handover 기본 워크스페이스 & 비파괴 자동 스캔 (`v4.71.1` 강화)**:
   - `MainViewModel` 생성 시 UI가 먼저 렌더링된 직후 백그라운드에서 `FreshHandoverService.ScanUserProfilesAsync()` 및 `ScanSessionArtifactsAsync()`를 비파괴 실행합니다.
   - 실제 삭제는 사용자의 1-Click 확인 모달 승인 후에만 진행되며, `Advapi32.dll` P/Invoke(`CredEnumerateW`/`CredDeleteW`) 및 `userenv.dll`(`DeleteProfileW`)를 직접 호출합니다. 또한 `IsReparsePointPath` 검사로 심볼릭 링크·정션을 따라 외부 폴더가 삭제되는 것을 차단하고, 사용 중인 항목은 **하위 파일 → 상위 디렉터리 순(Bottom-Up)**으로 `PendingFileRenameOperations`에 예약한 뒤 Step 4에서 **실제 디스크/자격증명 2차 검증 재스캔**을 수행합니다.
3. **단일 진실 원천 (`modules.manifest.json`)**:
   - 144개 표준·특수 모듈 정의를 JSON 매니페스트로 통합하고 `CustomModuleService.cs`와 프론트엔드 TypeScript가 동일 소스를 참조합니다.

---

## 💻 3. 빌드 및 배포 방법 (Build & Publish)

### A. 프론트엔드 자산 컴파일 & XAML 구조 검증 (Linux/Windows 공통)
```bash
npm run build           # sync-version.js + esbuild + Tailwind CSS -> dist-web/
npm run validate:xaml   # MainWindow.xaml / CommanderMainWindow.xaml / App.xaml 정적 무결성 검증
```

### B. .NET 10 WPF 빌드 (Windows)
```powershell
dotnet build WinPurifyPro.csproj -c Release
dotnet build WinPurifyCommander.csproj -c Release
```

### C. 원클릭 통합 빌드 스크립트 (`build.bat` / `build.ps1`)
루트 디렉터리의 `build.bat` 또는 `build.ps1`을 실행하면 `node sync-version.js` 버전 동기화, C++ Native DLL 빌드, WPF Slim/Self-Contained 단일 실행 파일 및 Inno Setup 설치 프로그램 패키징이 자동 실행됩니다.
