# WinPurify Pro Documentation Hub (문서 센터)

WinPurify Pro 프로젝트의 주요 아키텍처, 클린 환경 인수인계 파이프라인, 원격 관제 규격 및 보안 감사 문서 모음입니다.

> **현재 공식 버전**: `v4.71.1`  
> **앱 정체성**: **Clean Handover & Fresh Environment Suite** (이전 사용자 프로필·앱 데이터·로그인 세션 완전 소거 및 새 사용자 클린 환경 초기화)  
> **공식 GitHub 저장소**: [https://github.com/AhBiYout-all/WinPurifyPro](https://github.com/AhBiYout-all/WinPurifyPro)  
> **공식 개발자 블로그**: [https://ahbiyoutvibe.blogspot.com/](https://ahbiyoutvibe.blogspot.com/)

---

## 📚 문서 목차 (Document Index)

1. [GitHub 공식 운영 규칙 및 배포 파이프라인 가이드 (GITHUB_RULES_AND_DEPLOYMENT_GUIDE.md)](./GITHUB_RULES_AND_DEPLOYMENT_GUIDE.md)
   - GitHub Actions CI/CD (Node.js 22 LTS), Inno Setup 무소음 설치, `.gitignore` 스크립트 업로드 제외 규칙 명세.
2. [C# WPF MVVM 아키텍처 & 빌드 가이드 (WPF_MVVM_ARCHITECTURE_GUIDE.md)](./WPF_MVVM_ARCHITECTURE_GUIDE.md)
   - .NET 10.0 WPF 솔루션 구조, Partial ViewModel 분할(`Handover`/`Optimization`/`Diagnostics`), DPAPI 보안 금고, `modules.manifest.json` 단일 진실 원천 및 단일 파일 배포 가이드.
3. [중앙 관제 콘솔 원격 제어 명세서 (REMOTE_COMMANDER_SPEC.md)](./REMOTE_COMMANDER_SPEC.md)
   - WinPurify Central Commander(WPF / Electron 데스크톱), TLS 1.3(TCP:9874) + TOFU 인증서 피닝, 6자리 일회용 PIN 페어링, Challenge-Response Nonce, 하이브리드 LAN 탐색(UDP:9871 + `/24` 스캔) 및 `/api/v1/batch/fresh-handover`.
4. [종합 보안 취약점 점검 보고서 (SECURITY_AUDIT_REPORT.md)](./SECURITY_AUDIT_REPORT.md)
   - DPAPI/`safeStorage` 키 금고, Zero Plaintext Secret, Challenge IP 바인딩, `FreshHandoverService` 자기 보호 및 Win32 P/Invoke 직접 소거, 개발 서버 경로 격리 점검 내역.
5. [CLI 자동화 & 무인 인수인계 가이드 (CLI_AUTOMATION_GUIDE.md)](./CLI_AUTOMATION_GUIDE.md)
   - `--handover-auto`, `--zero-trace`, `--sessions-only`, `--full-reset`, `--schedule-run`, `--service` 자동화 스위치 및 작업 스케줄러 연동.
6. [Windows 백그라운드 서비스 가이드 (WINDOWS_SERVICE_GUIDE.md)](./WINDOWS_SERVICE_GUIDE.md)
   - `WinPurifyAgentService` SCM 서비스 아키텍처 및 방화벽 포트(`TCP 9874`, `TCP 9870`, `UDP 9871`).
7. [엔터프라이즈 무인 설치 가이드 (UNATTENDED_INSTALLATION_GUIDE.md)](./UNATTENDED_INSTALLATION_GUIDE.md)
   - Intune / SCCM / GPO 무소음 설치 스위치 및 방화벽/서비스 검증 스크립트.
8. [144개 모듈 정밀 진단 & 감사 보고서 (MODULES_PRECISION_DIAGNOSTICS_REPORT.md)](./MODULES_PRECISION_DIAGNOSTICS_REPORT.md)
   - 8대 카테고리 144개 모듈 + `modules.manifest.json` 동적 커스텀 모듈 + 다중 브라우저 프로필 정밀 진단.
9. [Native C++ PurifyEngineCore 설계 명세서 (NATIVE_PURIFY_ENGINE_CORE_ORIGINAL_SPEC.md)](./NATIVE_PURIFY_ENGINE_CORE_ORIGINAL_SPEC.md)
   - C++17 `PurifyEngineCore.dll` 5대 핵심 파이프라인 및 Win32 커널 P/Invoke 규격.
10. [Native 성능 확장 & Commander DLL 명세서 (NATIVE_PERFORMANCE_EXTENSIONS_SPEC.md / PURE_CREATION_COMMANDER_DLL_SPEC.md)](./NATIVE_PERFORMANCE_EXTENSIONS_SPEC.md)
    - 메모리 풀링, 고속 디렉터리 스캐너 및 HMAC-SHA256 네이티브 서명 가속 명세.
11. [하드웨어 스펙 검증 5대 규칙 (HARDWARE_SPEC_VERIFICATION_RULES.md)](./HARDWARE_SPEC_VERIFICATION_RULES.md)
    - CPU/RAM/스토리지/GPU/보안 드리프트 탐지 규칙.
12. [상용(Commercial) Pro 라이선스 티어 명세서 (COMMERCIAL_PRO_TIER_SPEC.md)](./COMMERCIAL_PRO_TIER_SPEC.md)
    - Standard / Pro / Enterprise 티어별 기능 차별화 및 오프라인 HWID 검증.
13. [구글 블로그 홍보 양식 (GOOGLE_BLOGGER_POST_RICH_TEXT.md / GOOGLE_BLOGGER_POST_HTML.html)](./GOOGLE_BLOGGER_POST_RICH_TEXT.md)
    - Blogger.com 작성 모드 및 HTML 편집 모드 공식 게시 양식.
14. [소프트웨어 사용권 계약서 2종 (LICENSE_KO.md / LICENSE_EN.md)](./LICENSE_KO.md)
    - WinPurify Pro 정식 한국어/영어 EULA 라이선스 계약서.
15. [패치 노트 & 변경 이력 (PATCH_NOTES.md / WorkLog.md)](./PATCH_NOTES.md)
    - 시맨틱 버전별 상세 변경 이력 및 마일스톤 기록.

---

## 🛠️ 주요 하이라이트 기능 (Key Features)

- **새 사용자 클린 환경 인수인계 (`Clean Handover`, 기본 워크스페이스)**: 앱 실행 즉시 이전 Windows 사용자 프로필(`ProfileList` SID + 고립 폴더), 다중 브라우저 프로필, 카카오톡/Discord/Teams/Adobe/Git/NPKI 및 **Cursor·VS Code·GitHub CLI·Docker·Postman·Figma** 세션을 비파괴 자동 탐지하고, **Whitelist 보존 예외 관리**, **잠금 파일 재부팅 예약(`PendingFileRenameOperations`)**, **실시간 4단계 진행률**, **HTML/JSON 검증 인증서 발급**까지 제공.
- **LAN 일괄 인수인계 오케스트레이션 & 검증 매트릭스 (`HandoverFleetModal`, `Central Commander`)**: ECDSA P-256 TLS 9874 + TOFU 피닝, DPAPI/`safeStorage` 키 금고, 6자리 PIN 페어링, Challenge Nonce 서명으로 작업실 다중 PC 클린 인수인계를 동시 배포하고 PC별 검증 상태(`통과` / `재부팅 대기`)를 수집.
- **단일 진실 원천(`modules.manifest.json`) 기반 144개 + 사용자 동적 모듈**: C# WPF / Electron / React UI가 동일한 JSON 매니페스트를 공유하며 4중 실행 Failover(Native DLL ➡️ Fast CLI ➡️ Embedded PS ➡️ Fallback) 제공.
- **실시간 Live Safepoint & 시스템 트레이**: 소거 직전 `.reg` 스냅샷 백업 및 1-Click 롤백, `Shell_NotifyIconW` 기반 진행률 툴팁 및 토스트 알림.
