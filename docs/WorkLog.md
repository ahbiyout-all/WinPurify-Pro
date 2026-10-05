# 📝 WinPurify Pro - 작업 로그 (Work Log)

> **프로젝트**: WinPurify Pro (Remix WinPurify3-windowform-net)  
> **총괄 개발자**: AhBiYout (Cisnet Soft)  
> **현재 공식 버전**: `v4.34.0`  
> **공식 블로그**: [https://ahbiyoutvibe.blogspot.com/](https://ahbiyoutvibe.blogspot.com/)  
> **공식 홈페이지**: [http://www.cisnet.co.kr/](http://www.cisnet.co.kr/)

---

## 📅 작업 내역 요약 (Recent Milestones)

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
