# WinPurify Pro Documentation Hub (문서 센터)

WinPurify Pro 프로젝트의 주요 아키텍처, 기능 가이드 및 개발 문서 모음입니다.

> **현재 공식 버전**: `v4.62.0`  
> **공식 GitHub 저장소**: [https://github.com/AhBiYout-all/WinPurifyPro](https://github.com/AhBiYout-all/WinPurifyPro)  
> **공식 개발자 블로그**: [https://ahbiyoutvibe.blogspot.com/](https://ahbiyoutvibe.blogspot.com/)  

---

## 📚 문서 목차 (Document Index)

1. [GitHub 공식 운영 규칙 및 배포 파이프라인 가이드 (GITHUB_RULES_AND_DEPLOYMENT_GUIDE.md)](./GITHUB_RULES_AND_DEPLOYMENT_GUIDE.md)
   - GitHub Actions CI/CD (Node.js 22 LTS), Inno Setup 무소음 설치, `.gitignore` 스크립트 업로드 제외 규칙 명세.
2. [구글 블로그 포스팅 마크다운 양식 [작성 모드] (GOOGLE_BLOGGER_POST_RICH_TEXT.md)](./GOOGLE_BLOGGER_POST_RICH_TEXT.md)
   - Blogger.com [작성 모드 (서식 있는 텍스트)] 바로 붙여넣기용 공식 홍보 및 업데이트 배포 포스팅 문서.
3. [구글 블로그 포스팅 HTML 코드 [HTML 편집 모드] (GOOGLE_BLOGGER_POST_HTML.html)](./GOOGLE_BLOGGER_POST_HTML.html)
   - Blogger.com [HTML 편집 모드] 바로 붙여넣기용 3D 다크 테마 HTML/CSS 디자인 포스팅 코드.
4. [상용(Commercial) Pro 라이선스 티어 명세서 (COMMERCIAL_PRO_TIER_SPEC.md)](./COMMERCIAL_PRO_TIER_SPEC.md)
   - Standard(무료) 대 Pro(상용) 대 Enterprise(원격관제) 티어별 기능 차별화, HWID 오프라인 라이선스 검증 메커니즘 및 수익화 모델 명세.
5. [Native C++ PurifyEngineCore 동작 원리 및 심층 아키텍처 백서 (PURIFY_ENGINE_CORE_ARCHITECTURE_SPEC.md)](./PURIFY_ENGINE_CORE_ARCHITECTURE_SPEC.md)
   - 순수 창작 C++17 x64 코어 엔진 5대 핵심 파이프라인 동작 원리 및 내부 메커니즘 심층 분석.
6. [Native C++ 엔진 매뉴얼 & P/Invoke 가이드 (PurifyEngineCore_Manual.md)](./PurifyEngineCore_Manual.md)
   - C# <-> C++ P/Invoke 연동 함수 목록, MSVC/MinGW 빌드 명령어 및 빠른 참조 가이드.
7. [C# WPF MVVM 아키텍처 & 빌드 가이드 (WPF_MVVM_ARCHITECTURE_GUIDE.md)](./WPF_MVVM_ARCHITECTURE_GUIDE.md)
   - .NET 8.0 / .NET 10.0 WPF 솔루션 구조, 계층별 MVVM 패턴 설계, 비동기 커맨드 처리 및 단일 파일 배포 가이드.
8. [중앙 관제 콘솔 원격 제어 명세서 (REMOTE_COMMANDER_SPEC.md)](./REMOTE_COMMANDER_SPEC.md)
   - WinPurify Central Commander 아키텍처, UDP 브로드캐스트 에이전트 자동 탐색 및 암호화 HTTP 원격 배치 최적화 프로토콜.
9. [종합 보안 취약점 점검 보고서 (SECURITY_AUDIT_REPORT.md)](./SECURITY_AUDIT_REPORT.md)
   - 4-Tier 원격 관제 보안, Replay Attack 방어, 256-bit 클러스터 키, CORS 제한 및 최소 권한 ACL 점검 내역.
10. [소프트웨어 사용권 계약서 2종 (LICENSE_KO.md / LICENSE_EN.md)](./LICENSE_KO.md)
    - WinPurify Pro 정식 한국어/영어 EULA 라이선스 계약서.
11. [패치 노트 & 변경 이력 (PATCH_NOTES.md)](./PATCH_NOTES.md)
    - 버전별 상세 변경 및 최적화 이력 기록.

---

## 🛠️ 주요 하이라이트 기능 (Key Features)

- **C# .NET 8.0 / .NET 10.0 WPF MVVM 엔터프라이즈 구조**: 강력한 데이터 바인딩과 스레드 분리 설계.
- **5MB대 초경량 슬림(Ultra-Slim) 단일 실행 바이너리**: `WinPurifyPro-Slim.exe` (~7.9MB) 및 `WinPurifyCommander-Slim.exe` (~4.5MB) 즉시 실행 무설치 파일.
- **Zero-Duplicate GitHub Releases 매트릭스**: 중복 파일 0% 단권화 배포 파이프라인.
- **Node.js 22 LTS CI/CD 파이프라인**: GitHub Actions 최신 런타임 마이그레이션.
- **클래식 Win95/98 스타일 UI & 모던 인터랙션**: 레트로 감성과 현대적 윈도우 성능 엔진의 결합.
- **실시간 비동기 텔레메트리 콘솔**: 백그라운드 PowerShell 작업 진행률 및 스트림 로그 실시간 동기화.
- **Quick Optimize 빠른 제어판**: RAM 압축, 임시 파일, 휴지통 원클릭 즉시 정제.
- **오프라인 실행 스크립트 생성**: `.PS1` 및 `.BAT` 포맷의 독립 실행형 최적화 스크립트 빌드.
- **LAN 원격 제어 센터**: 동일 네트워크 내 PC 자동 탐색 및 원격 일괄 정제 지원.
