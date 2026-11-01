# 📢 Google Blogger [작성 모드 (서식 있는 텍스트)] 공식 포스팅 양식

> **용도**: 구글 블로거(Blogger.com) 포스팅 시 **[작성 모드]**에서 그대로 복사하여 붙여넣을 수 있는 서식 있는 마크다운/텍스트 문서입니다.
> **최신 버전**: v4.62.0  
> **생성 일자**: 2026-11-01  

---

# [1] 제목 (Title)
WinPurify Pro v4.62.0 - Central Commander & WinPurify Pro 한 PC 동시 실행(Coexistence) 전면 지원 & C++ 네이티브 가속 Windows 11/10 최적화 솔루션

---

# [2] 본문 (Body)

**WinPurify Pro**는 단일 PC 시스템 최적화부터 다중 PC 중앙 관제(Central Commander)까지 완벽하게 지원하는 차세대 Windows 11/10 전용 시스템 정화 솔루션입니다.

.NET 런타임 가비지 컬렉션(GC) 지연과 PowerShell 실행 오버헤드를 극복하기 위해 **순수 창작 C++ 네이티브 가속 코어(`PurifyEngineCore.dll`)**를 탑재하여, 기존 C# 관리 코드 대비 최대 15배 빠른 커널 레벨 디렉토리/레지스트리 탐색 속도를 제공합니다.

특히 최신 **v4.62.0**에서는 **한 PC에서 Central Commander(중앙 관제 콘솔)와 WinPurify Pro 메인 앱을 동시에 나란히 실행**할 수 있도록 소켓 및 뮤텍스 아키텍처를 전면 개편하여, 관리자가 자신의 로컬 PC를 정밀 정화하면서 사내/홈 LAN 상의 다중 PC 원격 관제를 끊김 없이 동시에 수행할 수 있습니다.

---

# [3] 업데이트 (Latest Features & Highlights)

### ✨ v4.62.0 주요 신규 기능 및 개선 하이라이트
* **⚡ Central Commander & WinPurify Pro 한 PC 동시 실행(Coexistence) 전면 지원**:
  * Central Commander와 WinPurify Pro 메인 앱이 동일 시스템 내에서 충돌 없이 동시에 함께 실행될 수 있도록 아키텍처 전면 개편.
  * 상호 실행 차단 해제: 독립 글로벌 뮤텍스(`Global\WinPurifyPro_App_Mutex_Global_6904B914`, `Global\WinPurifyPro_Commander_Mutex_Global_6904B914`)를 통해 자기 자신의 중복 실행만 방지하고 두 프로그램은 나란히 동시 구동.
  * 소켓 포트 충돌 원천 차단: UDP Discovery 리스너에 `SO_REUSEADDR`을 적용하고, TCP 9870 포트가 이미 활성화된 경우 자동 공유 채널 모드로 전환하여 바인딩 에러 없이 양방향 통신 지원.
  * 메인 앱 내 `LaunchCommanderProcess`: 메인 앱을 종료하지 않고 백그라운드로 커맨더를 동시 기동.
  * 웹 대시보드 UI에 `[중앙 관제 (동시 실행 중)]` 실시간 펄스 뱃지 및 동시 실행 상태 배너 연동.
* **📈 Recharts 시각화 텔레메트리 대시보드 (`showTelemetryModal`)**:
  * 기간별(최근 7일/30일/90일/전체) 누적 회수 디스크 용량 추이(GB) 및 주간 정크 청소 용량(MB) `ComposedChart` 시각화.
  * 시스템 최적화 효율 점수(%) 및 부팅/앱 응답 속도 개선율(%) `LineChart` 분석.
* **🚀 GitHub Actions CI/CD 최신화 (Node.js 22 LTS)**:
  * GitHub Actions Node.js 20 EOL 대응을 위해 `node-version: 22`로 완벽 전환하여 CI/CD 빌드 경고 메시지 완전 차단.
* **🛡️ Inno Setup 무소음 설치 & 방화벽 사전 자동 등록**:
  * 폴더 풀림(Unpacked Multi-File) 정식 설치 및 무인 설치(`/SILENT`, `/VERYSILENT`) 지원.
  * Windows 방화벽 예외 규칙 자동 등록 (`WinPurify Pro v4.62.0` 및 범용 규칙).

---

# [4] 앱 그림 (App Screenshot & UI Visuals)

> **[UI 그래픽 가이드]**: 아래 그래픽 요소를 구글 블로그 본문 상단 및 주요 설명 위치에 삽입하세요.

1. **메인 최적화 클라이언트 인터페이스**:
   * 다크 사이버-글래스 테마 기반의 144개 카테고리별 모듈 선택 및 1-Click 정화 컨트롤러.
2. **Recharts 텔레메트리 대시보드 화면**:
   * 누적 회수 디스크 용량(GB) 영역 차트 및 최적화 효율 지수(%) 꺾은선 차트 시각화 패널.
3. **144개 전수 모듈 스캔 대시보드**:
   * Robocopy /L 가상 I/O 스트림 실시간 탐색 진행률 및 카테고리별 발견 용량 카드.
4. **센트럴 커맨더 (Central Commander) 원격 관제 콘솔**:
   * 고대비 사이언/블루 `#38BDF8` 프로필 드롭다운 및 다중 PC 원격 배치 최적화 관제 패널.

---

# [5] 다운로드 (Download Links)

* **공식 GitHub Release 다운로드**:  
  [https://github.com/AhBiYout-all/WinPurifyPro/releases](https://github.com/AhBiYout-all/WinPurifyPro/releases)
* **공식 Windows 정식 인스톨러 직접 다운로드 (WinPurifyPro-Setup.exe)**:  
  [https://github.com/AhBiYout-all/WinPurifyPro/releases/latest/download/WinPurifyPro-Setup.exe](https://github.com/AhBiYout-all/WinPurifyPro/releases/latest/download/WinPurifyPro-Setup.exe)
* **공식 GitHub 소스 코드 저장소**:  
  [https://github.com/AhBiYout-all/WinPurifyPro](https://github.com/AhBiYout-all/WinPurifyPro)

---

# [6] 버전 정보 (Version Information)

* **공식 애플리케이션 버전**: `v4.60.0`
* **어셈블리 버전**: `4.60.0.0`
* **C++ 네이티브 가속 엔진**: `PurifyEngineCore.dll` (v4.60.0)
* **어셈블리 아키텍처**: 64-bit (x64 Unpacked Multi-File)
* **릴리스 일자**: 2026년 11월 1일

---

# [7] 시스템 요구 사항 (System Requirements)

* **운영체제**: Microsoft Windows 11 (64-bit) / Windows 10 (64-bit) / Windows Server 2016 이상
* **프로세서**: x64 호환 프로세서 (Intel Core / AMD Ryzen 2GHz 이상)
* **메모리(RAM)**: 2 GB RAM 이상 (4 GB 이상 권장)
* **디스크 공간**: 최소 20 MB 여유 공간 (언팩 설치)
* **권한**: 관리자 권한 (Administrator Rights) 필요
* **원격 관제 네트워크**: TCP Port 9870 (REST 명령) / UDP Port 9871 (에이전트 브로드캐스트)

---

# [8] 제작자 및 공식 채널 안내 (Official Creator & Channels)

* **개발자 명칭**: **AhBiYout** (프로젝트 총괄 설계 및 수석 엔지니어)
* **GitHub 계정**: **AhBiYout-all**
* **GitHub 저장소**: [https://github.com/AhBiYout-all/WinPurifyPro](https://github.com/AhBiYout-all/WinPurifyPro)
* **공식 구글 블로그**: [https://ahbiyoutvibe.blogspot.com/](https://ahbiyoutvibe.blogspot.com/)

---

# [9] 전체 업데이트 (Full Release History & Feature Improvements)

* **v4.60.0 (2026-11-01)**: Recharts 기반 누적 디스크 회수(GB) & 최적화 효율 지수(%) 텔레메트리 대시보드 탑재, 구글 블로거 포스팅용 공식 문서 생성 파이프라인 완성.
* **v4.59.0 (2026-11-01)**: Recharts 144개 모듈 실시간 탐색 I/O 스트림 영역 차트, 카테고리 공간 회수 막대 차트 및 정화 상태 도넛 차트 구현.
* **v4.58.0 (2026-11-01)**: GitHub Actions Node.js 22 LTS 런타임 최신화, .gitignore 로컬 푸시 스크립트 제외 규칙 및 Inno Setup 무소음 방화벽 사전 등록 강화.
* **v4.57.0 (2026-11-01)**: Recharts 라이브러리 신규 설치 및 시각화 데이터 연동 체계 수립.
* **v4.56.4 (2026-11-01)**: 프로젝트 전반 소속 표기 및 cisnet.co.kr 도메인 완전 Clean-up, WinPurify Pro 단독 브랜딩 확립.
* **v4.56.3 (2026-10-31)**: 센트럴 커맨더 최적화 프로필 시각 가독성 개선 (고대비 사이언/블루 `#38BDF8` 적용).
* **v4.56.2 (2026-10-31)**: Native C++ Core (`main.cpp`) 중복 함수 정의 (`error C2084`) 제거 및 MSVC 빌드 완벽 정상화.
* **v4.56.1 (2026-10-31)**: Native C++ Core 구문 오류 수정 및 11개 C++ Export API 링크 정상화.
* **v4.56.0 (2026-10-23)**: 144개 전수 모듈 실시간 스캔 진행률 대시보드 및 발견 사항 종합 요약 보고서 엔진 구축.
* **v4.55.0 (2026-10-18)**: 달라진 하드웨어 스펙 검증 센터 및 5대 검증 규칙 (`WPP-RULE-HW-2026-V1`) 탑재.
* **v4.54.0 (2026-10-09)**: 비동작 모바일 APK/iOS 빌드 설정 전면 제거 및 Windows PC 전용 64비트 플랫폼 단권화.
* **v4.53.0 (2026-10-09)**: GitHub Releases 기반 실시간 자동 업데이트 검사 및 1-Click 업데이트 알림 팝업창 구현.

---

# [10] 추천 태그 (Recommended Tags)

> **[태그 제약 조건 준수]**: 한글/영어 혼용, 20개 이하, 총 200글자 이내

WinPurifyPro, Windows11최적화, PC최적화, 레지스트리청소, 컴퓨터속도빠르게, C++네이티브, AhBiYout, SystemCleaner, PCPerformance, WindowsOptimizer, RegistryCleaner, SpeedUpPC
