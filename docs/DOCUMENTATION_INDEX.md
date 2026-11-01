# WinPurify Pro Technical Documentation Index

Welcome to the **WinPurify Pro** technical documentation repository. Below is the complete catalogue of technical specifications, architecture blueprints, installation manuals, and release notes.

---

## 📚 Core Documentation Directory

| Document | Topic & Focus Area |
|---|---|
| **[`HARDWARE_SPEC_VERIFICATION_RULES.md`](./HARDWARE_SPEC_VERIFICATION_RULES.md)** | **달라진 하드웨어 스펙 검증을 위한 5대 규칙 선언서 (CPU, RAM, NVMe/SSD, GPU, 보안 드리프트)** |
| **[`GITHUB_RULES_AND_DEPLOYMENT_GUIDE.md`](./GITHUB_RULES_AND_DEPLOYMENT_GUIDE.md)** | **GitHub 공식 운영 규칙, 배포 파이프라인, Actions 및 자동 업데이트 명세서** |
| **[`PURE_CREATION_COMMANDER_DLL_SPEC.md`](./PURE_CREATION_COMMANDER_DLL_SPEC.md)** | **커맨더 순수 창작 DLL 4대 기능 동작 원리, 세부 기술 명세 및 무오류 검증 보고서** |
| **[`NATIVE_PURIFY_ENGINE_CORE_ORIGINAL_SPEC.md`](./NATIVE_PURIFY_ENGINE_CORE_ORIGINAL_SPEC.md)** | **순수 창작 Native C++ 코어 DLL 동작 원리, 커널 메커니즘 및 정밀 명세서** |
| **[`NATIVE_PERFORMANCE_EXTENSIONS_SPEC.md`](./NATIVE_PERFORMANCE_EXTENSIONS_SPEC.md)** | **네이티브 성능 확장 (Memory Pool 및 Fast Directory Scanner) 기술 명세서** |
| **[`MODULES_PRECISION_DIAGNOSTICS_REPORT.md`](./MODULES_PRECISION_DIAGNOSTICS_REPORT.md)** | **카테고리별 143/144개 전수 모듈 정밀 진단 및 성능/보안 감사 보고서** |
| **[`COMMERCIAL_PRO_TIER_SPEC.md`](./COMMERCIAL_PRO_TIER_SPEC.md)** | **WinPurify Pro 상용(Commercial) 라이선스 티어 및 기능 차별화 명세서** |
| **[`PATCH_NOTES.md`](./PATCH_NOTES.md)** | **Primary Release History & Semantic Version Changelog** |
| **[`SECURITY_AUDIT_REPORT.md`](./SECURITY_AUDIT_REPORT.md)** | **종합 보안 취약점 점검 보고서 (4-Tier 방어 아키텍처 & 취약점 조치 내역)** |
| **[`WorkLog.md`](./WorkLog.md)** | **프로젝트 마일스톤 및 작업 로그 (Work Log)** |
| **[`LICENSE_KO.md`](./LICENSE_KO.md)** | **WinPurify Pro 최종 사용자 사용권 계약서 (EULA - 한국어 정식판)** |
| **[`LICENSE_EN.md`](./LICENSE_EN.md)** | **WinPurify Pro End User License Agreement (EULA - English Standard)** |
| **[`UNATTENDED_INSTALLATION_GUIDE.md`](./UNATTENDED_INSTALLATION_GUIDE.md)** | Enterprise Silent Setup, Intune/SCCM, & Inno Setup switches |
| **[`WINDOWS_SERVICE_GUIDE.md`](./WINDOWS_SERVICE_GUIDE.md)** | `WinPurifyAgentService` Architecture, 24/7 Remote Receiver, & SCM Management |
| **[`CLI_AUTOMATION_GUIDE.md`](./CLI_AUTOMATION_GUIDE.md)** | Command-Line Interface parameters, automation switches, & Task Scheduler |
| **[`REMOTE_COMMANDER_SPEC.md`](./REMOTE_COMMANDER_SPEC.md)** | WinPurify Central Commander Fleet Management Architecture & Protocol |
| **[`WPF_MVVM_ARCHITECTURE_GUIDE.md`](./WPF_MVVM_ARCHITECTURE_GUIDE.md)** | WPF UI Layer, MVVM Pattern, Reactive Data Binding, & HUD Toasts |
| **[`README.md`](./README.md)** | Project Overview, Features, and Developer Quickstart |

---

## 📌 Versioning & Build Policy (Semantic Versioning)
- **Major (X.0.0)**: Major architectural overhaul, platform migration, framework transition.
- **Minor (X.Y.0)**: New functional module, independent tool isolation, fleet management extension.
- **Patch (X.Y.Z)**: Bug fixes, UI/UX polish, installer improvements, CLI bindings, service stabilization.
