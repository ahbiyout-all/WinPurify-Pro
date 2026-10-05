# WinPurify Pro - 기술 명세서 (Technical Specification)

본 문서는 WinPurify Pro의 내부 시스템 구조, C# WPF MVVM 아키텍처, C++ Native Interop, PowerShell 실행 엔진 및 컴파일 파이프라인 명세를 다룹니다.

---

## 🏗️ 1. C# WPF MVVM 시스템 아키텍처

```
┌─────────────────────────────────────────────────────────────┐
│                 View Layer (XAML / WPF)                     │
│  MainWindow.xaml (DataBinding, ValueConverters, Styles)     │
└──────────────────────────────┬──────────────────────────────┘
                               │ DataBinding / ICommand
┌──────────────────────────────▼──────────────────────────────┐
│                    ViewModel Layer                          │
│  MainViewModel, TaskItemViewModel, CategoryViewModel        │
│  (ViewModelBase, RelayCommand, AsyncRelayCommand)           │
└──────────────┬──────────────────────────────┬───────────────┘
               │                              │
┌──────────────▼──────────────┐┌──────────────▼───────────────┐
│        Model Layer          ││       Service Layer          │
│  OptimizationTask, Category ││  ISystemPurifyService        │
│  LogEntry, Safepoint, Status││  ISystemMetricsService       │
└─────────────────────────────┘└──────────────┬───────────────┘
                                              │
                    ┌─────────────────────────┴───────────────┐
                    │                                         │
┌───────────────────▼───────────┐         ┌───────────────────▼───────────┐
│     PowerShell Execution      │         │     Native C++ Interop        │
│  (Base64 Encoded, CancellationToken)   │  (PurifyEngineCore.dll P/Invoke)│
└───────────────────────────────┘         └───────────────────────────────┘
```

### 계층별 책임 및 원칙
1. **Model Layer (`/Models`)**:
   - UI 및 외부 프레임워크 종속성이 없는 순수 데이터 구조체 (POCO).
   - 상태 및 데이터 무결성 보장.
2. **ViewModel Layer (`/ViewModels`)**:
   - `INotifyPropertyChanged` 알림을 통한 UI 상태 변경 전파.
   - `ICommand` / `AsyncRelayCommand`를 통한 사용자 인터랙션 처리 및 `IsExecuting` 상태 추적.
   - 백그라운드 스레드에서 UI 스레드로의 안전한 디스패칭 (`RunOnUIThread`).
3. **Service Layer (`/Services`)**:
   - 비동기 PowerShell 실행 엔진, 복원 지점 생성, 시스템 리소스 모니터링.
   - 인터페이스 기반 설계로 테스트 및 기능 확장성 확보.
4. **Presentation / View Layer (`/Views`, `/Converters`)**:
   - 선언적 XAML 기반 뷰 레이아웃.
   - Value Converter를 통한 데이터 가공 및 UI 표현 분리.

---

## 🔌 2. Interop & CLI Compatibility

### A. PowerShell Execution Engine
- **Base64 Unicode Encoding**:
  - 명령어 실행 시 파라미터 이스케이핑 및 인젝션 문제를 방지하기 위해 UTF-16LE 바이트를 Base64로 인코딩하여 `-EncodedCommand`로 실행.
- **Process Cancellation & Cleanup**:
  - 사용자 작업 취소 요청 시 `CancellationToken`을 트리거하여 하위 프로세스 트리(`Process.Kill(true)`)를 즉시 종료.

### B. Native C++ Core Engine (`PurifyEngineCore.dll`)
- **P/Invoke API 바인딩**:
  - `CompressPhysicalRAM()`: 커널 레벨 Working Set 정리 및 메모리 압축.
  - `DeepScanDirectoryW(LPCWSTR path)`: 고속 폴더 용량 계산 및 정크 파일 스캔.

---

## 📦 3. 빌드 및 배포 파이프라인 (Build Targets)

- **Target Framework**: `.NET 10.0-Windows` (.NET 8.0-Windows 호환)
- **Deployment Mode**: `Self-Contained Single-File (win-x64)`
- **Configuration**: `Release` / `ReadyToRun` 활성화

