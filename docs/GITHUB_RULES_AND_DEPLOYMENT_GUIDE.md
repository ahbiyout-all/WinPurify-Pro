# 🚀 WinPurify Pro - GitHub 공식 운영 규칙, 배포 파이프라인 및 자동 업데이트 명세서
# (GitHub Rules, CI/CD Actions, Inno Setup Installer & Auto-Updater Specification)

> **문서 버전**: v4.62.1  
> **최종 갱신일**: 2026-11-01  
> **공식 개발자**: AhBiYout  
> **GitHub 조직/사용자명**: `AhBiYout-all`  
> **공식 저장소**: `https://github.com/AhBiYout-all/WinPurifyPro`  
> **공식 블로그**: `https://ahbiyoutvibe.blogspot.com/`

---

## 📌 1. 제작자 및 공식 채널 안내 (Official Creator & Channels)

WinPurify Pro의 모든 공식 배포 및 소스 코드는 아래 지정된 공인 채널을 통해서만 운영됩니다.

| 항목 | 공식 정보 | 설명 |
| :--- | :--- | :--- |
| **개발자 명칭** | **AhBiYout** | 프로젝트 총괄 설계 및 수석 엔지니어 |
| **GitHub 계정** | **AhBiYout-all** | 공식 GitHub 사용자/조직 식별자 |
| **GitHub 저장소** | **[`https://github.com/AhBiYout-all/WinPurifyPro`](https://github.com/AhBiYout-all/WinPurifyPro)** | 기본 앱 이름을 기반으로 생성된 공식 리포지토리 |
| **공식 구글 블로그** | **[`https://ahbiyoutvibe.blogspot.com/`](https://ahbiyoutvibe.blogspot.com/)** | 기술 블로그 및 공식 업데이트 소식지 |

---

## 🔒 2. 개인정보 보호 및 깃허브 업로드 보안 규칙

1. **개인정보 및 개인 이메일 업로드 원천 금지**:
   - 개발자의 개인 이메일(`ahbiyout@gmail.com` 등)과 민감 식별 정보는 소스 코드, 문서, 빌드 스크립트 어디에도 기록되지 않습니다.
   - `github_push.ps1` 스크립트 실행 시 정적 `git grep` 보안 스캐너가 자동 구동되어, 개인 이메일이나 민감 문자열이 발견될 경우 커밋을 즉시 중단합니다.
2. **불필요한 파일 동반 업로드 방지 (`.gitignore`)**:
   - 빌드 산출물(`bin/`, `obj/`, `publish/`, `Output/`), 임시 파일(`*.tmp`, `*.bak`, `*.log`), IDE 로컬 설정(`.vs/`, `.idea/`, `*.user`), Node.js 의존성(`node_modules/`)은 완벽히 제외됩니다.

---

## 🔄 3. GitHub Releases 기반 실시간 자동 업데이트 시스템

### 1) 동작 아키텍처 및 API 연동
- **API 엔드포인트**: `https://api.github.com/repos/AhBiYout-all/WinPurifyPro/releases/latest`
- **동작 주기**:
  - 앱 기동 시 백그라운드에서 2.5초 지연 후 자동 검사 수행 (`CheckForUpdatesAsync(isManual: false)`).
  - 사이드바 빠른 실행 버튼 ("🚀 GitHub 실시간 업데이트") 또는 웹 헤더 버튼 클릭 시 수동 검사.
- **버전 비교 (Semantic Versioning)**:
  - 현재 로컬 버전(`AppEnvironment.AppVersion`)과 GitHub 릴리스 태그(`tag_name`)를 SemVer 규칙으로 정밀 비교.

### 2) 업데이트 알림 팝업창 자동 실행 로직
- 신규 버전(`isNewer = true`) 발견 시 **사용자의 별도 클릭 없이 업데이트 알림 팝업창이 화면 중앙에 자동 실행**됩니다.
- **팝업창 3대 필수 구성 요소**:
  1. **업데이트 버튼**: 최신 인스톨러(`WinPurifyPro-Setup.exe`)를 1-Click으로 다운로드 및 실행하는 직관적 액션 버튼.
  2. **공식 GitHub 경로**:
     - 메인 리포지토리 경로: `https://github.com/AhBiYout-all/WinPurifyPro`
     - 릴리스 배포 경로: `https://github.com/AhBiYout-all/WinPurifyPro/releases`
  3. **버전 정보**:
     - 현재 설치된 버전 (예: `v4.53.0`)
     - GitHub 최신 배포 버전 (예: `v4.54.0`)
     - 출시 일자 및 마크다운 패치노트 요약

---

## ⚙️ 4. GitHub Actions CI/CD 워크플로 및 Node.js 20 EOL 대응

### 1) Node.js 20 공식 지원 종료(EOL) 대응
- 2026년 4월 Node.js 20 EOL 및 가을 GitHub Actions 공식 중단에 따라, 모든 워크플로 파일(`.github/workflows/*.yml`)에서 `node-version: 20`을 완전히 배제하고 **`node-version: 22` (최신 LTS) 및 `node-version: 24`**를 전면 채택하였습니다.
- 노란색 경고(Warning) 없이 깨끗한 최신 러너 환경에서 구동됩니다.

### 2) Windows PC 전용 빌드 및 인스톨러 배포 파이프라인 (`.github/workflows/build-and-release.yml`)
1. **순수 Windows 64비트 엔터프라이즈 플랫폼 집중**:
   - WinPurify Pro는 Windows NT 커널 API, 레지스트리 엔진, 볼륨 섀도우 복사본(VSS), MMCSS, C++ 네이티브 DLL(`PurifyEngineCore.dll`), Inno Setup 등을 기반으로 동작하는 전문 Windows 최적화 도구입니다.
   - 따라서 **이 시스템 구조가 존재하지 않고 동작하지 않는 모바일 APK(Android) 및 iOS 관련 빌드/자산은 전면 배제 및 제거**되었습니다.
2. **Windows PC 공식 인스톨러 빌드**:
   - .NET 8.0 SDK 및 Inno Setup 6 연동.
   - 초경량 언팩 개별 어셈블리 빌드 후 `Output/WinPurifyPro-Setup.exe` 생성.
3. **GitHub Release 배포 규칙 (인스톨 파일만 첨부)**:
   - **릴리스 자산에는 오직 공식 Windows 인스톨러 파일만 생성 및 첨부**:
     - `WinPurifyPro-Setup.exe` (Windows PC 공식 Inno Setup 정식 인스톨러)
   - 소스 코드 zip, 디버그 심볼, 포터블 파일, 불필요한 빌드 잔여물은 일절 첨부하지 않습니다.

---

## 📦 5. Inno Setup 설치 프로그램 표준 (`installer.iss`)

### 1) 폴더 풀림(Unpacked Multi-File) 개별 파일 설치
- 단일 번들 파일 대신 `{app}` 설치 경로에 모든 실행 파일(`WinPurifyPro.exe`, `WinPurifyCommander.exe`), 네이티브 C++ 코어 DLL(`PurifyEngineCore.dll`), 설정 및 종속 DLL들이 독립 개별 파일로 명확히 풀립니다.
- 불필요한 마크다운 문서 및 포터블 압축 파일의 중복 포함이 엄격히 금지됩니다.

### 2) 기존 버전 설치 감지 및 삭제/덮어쓰기 질의 대화상자
- 시스템 레지스트리 및 디렉터리를 스캔하여 이전 버전 감지 시:
  - **[예]**: 기존 언인스톨러를 자동 호출하여 완전 삭제(Clean Install) 후 새로 설치.
  - **[아니오]**: 기존 설정을 보존하며 새 파일로 덮어쓰기(Update Install).
  - **[취소]**: 설치 중단.

### 3) 무인 설치(Silent Install) 완벽 지원
- 기업 배포, SCCM/Intune 자동화 시 `/VERYSILENT /SUPPRESSMSGBOXES /NORESTART` 스위치를 지원합니다.
- 무인 모드에서는 사용자 질의 팝업이 자동 통과되어 즉시 무소음 설치가 완료됩니다.

### 4) 방화벽(Windows Firewall) 사전 자동 등록
- 설치 프로그램 실행 시 별도 명령 프롬프트 창 없이 백그라운드(`runhidden`)에서 방화벽 규칙을 조용히 등록합니다:
  - **기본 이름 규칙**: `WinPurify Pro` (Inbound & Outbound)
  - **버전 표기 규칙**: `WinPurify Pro v{#MyAppVersion}` (Inbound & Outbound)
  - **Central Commander 기본/버전 규칙**: `WinPurify Central Commander` 및 `WinPurify Central Commander v{#MyAppVersion}`
  - 언인스톨 시 등록된 모든 방화벽 규칙이 자동으로 일괄 삭제됩니다.

---

## 🛠️ 6. 깃허브용 원클릭 자동 푸시 스크립트 (`github_push.bat`)

로컬 개발 환경에서 더블 클릭 한 번으로 모든 동기화와 배포가 완료되는 스크립트를 제공합니다.

- **스크립트 파일**: `github_push.bat` (호출: `github_push.ps1`)
- **수행 절차**:
  1. `node sync-version.js`를 실행하여 15개 프로젝트 파일의 Semantic Version 일괄 검증.
  2. 개인정보 스크리닝 (민감 이메일 검출 시 즉시 중단).
  3. Git remote origin을 `https://github.com/AhBiYout-all/WinPurifyPro.git`으로 자동 설정.
  4. 변경된 파일 일괄 스테이징 (`git add -A`).
  5. 버전 번호가 자동 결합된 커밋 메시지 작성 (`Release vX.Y.Z`).
  6. Git Release 태그 생성 (`vX.Y.Z`).
  7. GitHub 저장소로 안전 푸시 (`git push origin main --tags`).
