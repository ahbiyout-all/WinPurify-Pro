# WinPurify Pro Technical Documentation Index

Welcome to the **WinPurify Pro (`v4.71.1`)** technical documentation repository. Below is the complete catalogue of architecture blueprints, Clean Handover & security audits, installation manuals, and release notes.

---

## 📚 Core Documentation Directory

| Document | Topic & Focus Area |
|---|---|
| **[`README.md`](./README.md)** | **Clean Handover & Fresh Environment Suite 개요 및 전체 문서 허브 (`v4.71.1`)** |
| **[`WPF_MVVM_ARCHITECTURE_GUIDE.md`](./WPF_MVVM_ARCHITECTURE_GUIDE.md)** | **C# .NET 10 WPF Partial MVVM 아키텍처, `FreshHandoverService`, `HandoverPolicyService`, `modules.manifest.json` 가이드** |
| **[`REMOTE_COMMANDER_SPEC.md`](./REMOTE_COMMANDER_SPEC.md)** | **Central Commander (WPF/Electron) 규격 — TLS 9874 + TOFU, 6자리 PIN 페어링, Challenge Nonce & `FreshHandover` 원격 배치** |
| **[`SECURITY_AUDIT_REPORT.md`](./SECURITY_AUDIT_REPORT.md)** | **종합 보안 감사 보고서 — DPAPI/`safeStorage` 금고, Win32 자격증명 직접 소거, 경로 보호 가드 및 Dev-Server 격리** |
| **[`MODULES_PRECISION_DIAGNOSTICS_REPORT.md`](./MODULES_PRECISION_DIAGNOSTICS_REPORT.md)** | **144개 모듈(`modules.manifest.json`) + 다중 브라우저/앱 세션 정밀 진단 및 감사 보고서** |
| **[`CLI_AUTOMATION_GUIDE.md`](./CLI_AUTOMATION_GUIDE.md)** | **CLI 자동화 스위치 (`--handover-auto`, `--zero-trace`, `--sessions-only`, `--full-reset`, `--schedule-run`, `--service`)** |
| **[`WINDOWS_SERVICE_GUIDE.md`](./WINDOWS_SERVICE_GUIDE.md)** | **`WinPurifyAgentService` 백그라운드 데몬 아키텍처 및 방화벽 포트 (`TCP 9874`, `TCP 9870`, `UDP 9871`)** |
| **[`UNATTENDED_INSTALLATION_GUIDE.md`](./UNATTENDED_INSTALLATION_GUIDE.md)** | **Enterprise Silent Setup (Intune/SCCM/GPO), Inno Setup 스위치 및 자동 방화벽/서비스 검증** |
| **[`HARDWARE_SPEC_VERIFICATION_RULES.md`](./HARDWARE_SPEC_VERIFICATION_RULES.md)** | **달라진 하드웨어 스펙 검증을 위한 5대 규칙 선언서 (CPU, RAM, NVMe/SSD, GPU, 보안 드리프트)** |
| **[`GITHUB_RULES_AND_DEPLOYMENT_GUIDE.md`](./GITHUB_RULES_AND_DEPLOYMENT_GUIDE.md)** | **GitHub 공식 운영 규칙, CI/CD 배포 파이프라인, Actions(Node 22 LTS) 및 자동 업데이트 명세서** |
| **[`PURE_CREATION_COMMANDER_DLL_SPEC.md`](./PURE_CREATION_COMMANDER_DLL_SPEC.md)** | **커맨더 순수 창작 DLL 기능 동작 원리, HMAC-SHA256 네이티브 서명 가속 및 무오류 검증 보고서** |
| **[`NATIVE_PURIFY_ENGINE_CORE_ORIGINAL_SPEC.md`](./NATIVE_PURIFY_ENGINE_CORE_ORIGINAL_SPEC.md)** | **순수 창작 Native C++ 코어 DLL (`PurifyEngineCore.dll`) 동작 원리 및 커널 메커니즘 명세서** |
| **[`NATIVE_PERFORMANCE_EXTENSIONS_SPEC.md`](./NATIVE_PERFORMANCE_EXTENSIONS_SPEC.md)** | **네이티브 성능 확장 (Memory Pool 및 Fast Directory Scanner) 기술 명세서** |
| **[`COMMERCIAL_PRO_TIER_SPEC.md`](./COMMERCIAL_PRO_TIER_SPEC.md)** | **WinPurify Pro 상용(Commercial) 라이선스 티어 및 기능 차별화 명세서** |
| **[`PATCH_NOTES.md`](./PATCH_NOTES.md)** | **Primary Release History & Semantic Version Changelog** |
| **[`WorkLog.md`](./WorkLog.md)** | **프로젝트 마일스톤 및 작업 로그 (`v4.71.1`)** |
| **[`LICENSE_KO.md`](./LICENSE_KO.md)** | **WinPurify Pro 최종 사용자 사용권 계약서 (EULA - 한국어 정식판)** |
| **[`LICENSE_EN.md`](./LICENSE_EN.md)** | **WinPurify Pro End User License Agreement (EULA - English Standard)** |

---

## 📌 Versioning & Build Policy (Semantic Versioning)
- **Major (X.0.0)**: Major architectural overhaul, platform migration, framework transition.
- **Minor (X.Y.0)**: New functional module, independent tool isolation, fleet management extension.
- **Patch (X.Y.Z)**: Bug fixes, security hardening, UI/UX polish, installer improvements, CLI bindings, service stabilization.
