# WinPurify Pro Agent Guidelines & Rules

## 📌 자동 버전 관리 및 패치노트 기록 규칙 (Mandatory Rule)

사용자의 지침에 따라, **코드 수정에 특이점(새로운 기능 구현, 아키텍처 변경, 버그 수정, UI/UX 개선 등)이 발생할 때마다 자동으로 패치노트를 작성하고 버전을 관리**합니다.

### 1. 버전 번호 부여 원칙 (Semantic Versioning)
- **Major (X.0.0)**: 대규모 시스템 개편, 플랫폼 마이그레이션
- **Minor (X.Y.0)**: 신규 기능 추가, 독립 앱/모듈 분리, 핵심 아키텍처 개선
- **Patch (X.Y.Z)**: 기능 개선, UI/UX 수정, 버그 픽스, 커맨드/이벤트 바인딩 보완

### 2. 특이점 발생 시 자동 수행 절차
1. **패치노트 문서 기록**:
   - `docs/PATCH_NOTES.md` 최상단에 새 버전 태그(예: `## 📌 [v4.4.4] - YYYY-MM-DD (주요 내용)`)와 함께 변경/개선 상세 내역 기록.
2. **앱 화면 및 자동빌드 스크립트 버전 완전 동기화 (Mandatory)**:
   - **앱 화면(WPF 데스크톱 & 웹 UI)**:
     - `MainWindow.xaml`: `WindowTitle` 및 사이드바 헤더 서브타이틀(`AppHeaderSubtitle`) 연동
     - `ViewModels/MainViewModel.cs`: 어셈블리 버전 리플렉션 및 `AppVersion` 동기화
     - `src/version.ts` & `index.tsx`: 웹 UI 버전 뱃지(`v{APP_VERSION}`) 동기화
   - **자동빌드 스크립트 및 결과물 바이너리**:
     - `build.bat` 및 `build.ps1`: 빌더 헤더 및 산출물 버전 정보 갱신
     - `version.rc`: Native C++ `PurifyEngineCore.dll`의 `FILEVERSION` 및 `PRODUCTVERSION` 리소스 갱신
     - `WinPurifyPro.csproj`: `<Version>`, `<AssemblyVersion>`, `<FileVersion>` 갱신
   - **프로젝트 메타데이터**:
     - `metadata.json`의 `version` 및 `description` 갱신
     - `package.json`의 `version` 및 `description` 갱신
     - `node sync-version.js`를 실행하여 모든 파일 정합성 일괄 검증 및 동기화 자동화
3. **컴파일 및 빌드 검증**:
   - `compile_applet`을 실행하여 모든 코드가 오류 없이 빌드되는지 최종 확인.
