# WinPurify Pro - C# WPF MVVM 아키텍처 및 빌드 가이드 (WPF MVVM Architecture Guide)

본 문서는 **WinPurify Pro (v3.1.0)**의 C# WPF 기반 MVVM(Model-View-ViewModel) 엔터프라이즈 아키텍처 설계와 컴파일, 배포 방법을 상세히 설명합니다.

---

## 🏛️ 1. 아키텍처 개요 (Architecture Overview)

WinPurify Pro는 단일 모놀리식 구조에서 **관심사 분리(Separation of Concerns)**와 **높은 유지보수성**을 갖춘 계층형 MVVM 패턴으로 리팩토링되었습니다.

```
WinPurifyPro/
├── WinPurifyPro.sln                      # Visual Studio 2022 / 2026 솔루션 파일
├── WinPurifyPro.csproj                   # .NET 10.0-Windows (.NET 8.0 호환) WPF 프로젝트 명세
├── App.xaml / App.xaml.cs                # 전역 애플리케이션 수명주기 및 리소스
├── app.manifest                          # UAC 관리자 권한 승격 매니페스트
│
├── 📂 Models/                            # [Model Layer] 순수 데이터 및 도메인 엔터티
│   ├── OptimizationTask.cs               # 최적화 작업 항목 (ID, 위험도, PowerShell 커맨드)
│   ├── TaskCategory.cs                   # 카테고리 정의 (보안, 성능, 레지스트리 등)
│   ├── LogEntry.cs                       # 콘솔 로그 및 심각도(Severity) 모델
│   ├── Safepoint.cs                      # 시스템 스냅샷/복원 지점 엔터티
│   └── SystemStatusMetrics.cs            # CPU, RAM, Disk 모니터링 수치
│
├── 📂 ViewModels/                        # [ViewModel Layer] UI 상태 및 명령 오케스트레이션
│   ├── ViewModelBase.cs                  # INotifyPropertyChanged & UI Dispatcher 동기화 베이스
│   ├── RelayCommand.cs                   # ICommand 동기 명령 래퍼 (제네릭 지원)
│   ├── AsyncRelayCommand.cs              # 비동기 Task 실행 및 IsExecuting 상태 추적 래퍼
│   ├── TaskItemViewModel.cs              # 개별 작업 선택 상태 및 동적 색상 브러시 속성
│   ├── CategoryViewModel.cs              # 카테고리별 컬렉션 및 선택 카운터 집계
│   ├── SafepointViewModel.cs             # 복원 지점 UI 뷰모델
│   └── MainViewModel.cs                  # 메인 윈도우 마스터 오케스트레이션 뷰모델
│
├── 📂 Views/                             # [View Layer] XAML 기반 프레젠테이션 계층
│   ├── MainWindow.xaml                   # 데이터 바인딩 기반 고성능 벡터 UI
│   └── MainWindow.xaml.cs                # DataContext 바인딩 및 윈도우 수명주기 제어 Code-Behind
│
├── 📂 Services/                          # [Service Layer] 비즈니스 로직 & OS 엔진
│   ├── ISystemPurifyService.cs           # 시스템 정제 서비스 인터페이스
│   ├── SystemPurifyService.cs            # Base64 인코딩 PowerShell 비동기 실행 엔진
│   ├── ISystemMetricsService.cs          # 실시간 하드웨어 메트릭 수집 인터페이스
│   ├── SystemMetricsService.cs           # WorkingSet / Disk 모니터링 구현체
│   ├── ITaskSeedService.cs               # 작업 시드 데이터 프로바이더 인터페이스
│   └── TaskSeedService.cs                # 60+개 최적화 룰셋 데이터 생성기
│
├── 📂 Converters/                        # [Converters] XAML 데이터 바인딩 값 변환기
│   └── ValueConverters.cs                # ProgressToWidth, RiskToBrush, BoolToVisibility
│
└── PurifyEngine.cs                       # C++ 네이티브 DLL (PurifyEngineCore.dll) P/Invoke 브리지
```

---

## 🧩 2. 계층별 세부 설계 (Layer Details)

### 1. Model 계층 (`/Models`)
- UI나 프레임워크에 의존하지 않는 순수 POCO(Plain Old CLR Object) 클래스로 구성.
- `OptimizationTask`: 최적화 대상의 고유 ID, 영문/한글 설명, 위험도 수준(`Safe`, `Deep`, `Risky`), 예상 확보 용량 및 실행할 PowerShell 명령어를 캡슐화.
- `LogEntry`: 타임스탬프, 심각도(`Info`, `Success`, `Warning`, `Error`, `Command`), 카테고리를 관리하며 UI 브러시 색상을 매핑.

### 2. ViewModel 계층 (`/ViewModels`)
- `ViewModelBase`: 모든 뷰모델의 기반 클래스로, `SetProperty<T>`를 통한 간결한 프로퍼티 변경 알림과 `RunOnUIThread(Action)`를 통한 스레드 안전 UI 업데이트를 제공.
- `AsyncRelayCommand`: 비동기 작업(`async/await`) 실행 중 버튼 중복 클릭을 방지(`IsExecuting`)하며, 작업 완료 시 커맨드 실행 가능 상태(`CanExecute`)를 자동 갱신.
- `MainViewModel`: 전체 최적화 시퀀스 실행, 프로그레스 백분율 계산, 실시간 콘솔 스트림 로깅, 언어 전환(한국어/영어), 다크/라이트 테마 변경을 총괄.

### 3. Service 계층 (`/Services`)
- UI와 분리된 비즈니스 로직 계층으로, 단위 테스트(Unit Test) 및 목(Mock) 객체 교체가 용이한 인터페이스 기반 설계.
- `SystemPurifyService`:
  - PowerShell 인젝션 방지를 위한 `-EncodedCommand (Unicode Base64)` 실행 방식 적용.
  - `CancellationToken`을 통한 사용자 강제 중단(Cancel) 지원 및 활성 프로세스 트리 즉시 종료(`Process.Kill(true)`).
  - 윈도우 시스템 복원 지점(`Checkpoint-Computer`) 자동 생성.

### 4. Native Interop 계층 (`PurifyEngine.cs`)
- C++ 고성능 코어 라이브러리(`PurifyEngineCore.dll`)와의 `[DllImport]` 인터페이스:
  - `CompressPhysicalRAM()`: 네이티브 Working Set Trimming 및 메모리 압축.
  - `DeepScanDirectoryW()`: 커널 레벨 디렉터리 고속 분석.

---

## 💻 3. 빌드 및 배포 방법 (Build & Publish)

### A. Visual Studio 2022 / 2026에서 빌드
1. `WinPurifyPro.sln` 파일을 실행합니다.
2. 솔루션 구성(Configuration)을 `Release` | `x64` 또는 `Any CPU`로 설정합니다.
3. `F5` 키(디버깅 시작) 또는 `Ctrl + F5`(디버깅하지 않고 시작)를 누릅니다.

### B. .NET CLI 명령어를 통한 빌드
```bash
# 1. 의존성 복원 및 일반 빌드
dotnet build WinPurifyPro.csproj -c Release

# 2. 독립 실행형(Single-File Portable) EXE 단일 파일 배포
dotnet publish WinPurifyPro.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishReadyToRun=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o ./publish/
```

### C. 제공된 배치 스크립트 활용
루트 디렉터리의 `build.bat`을 실행하면 환경 감지부터 C++ DLL 컴파일, WPF 프로젝트 빌드, 단일 실행 파일 패키징까지 원클릭으로 자동 진행됩니다.

```cmd
build.bat
```

---

## 🔒 4. 보안 및 권한 설정 (UAC & Security)

WinPurify Pro는 레지스트리 수정, 윈도우 서비스 제어, 임시 시스템 캐시 삭제를 수행하므로 관리자 권한이 필수적입니다.

- `app.manifest` 내에 `<requestedExecutionLevel level="requireAdministrator" uiAccess="false" />`가 구성되어 있어 프로그램 실행 시 자동으로 UAC 승인 대화상자가 표시됩니다.
- 실행 중인 프로세스가 관리자 권한이 아닌 경우 `ISystemPurifyService.IsAdministrator()`가 감지하여 UI에 경고 배지를 표시하고 권한 상승 재실행을 유도합니다.
