# 🛡️ WinPurify Pro - 종합 보안 점검 및 아키텍처 강화 보고서
# (Security Audit & Vulnerability Assessment Report)

> **문서 버전**: `v4.71.1`  
> **최종 점검 일자**: 2026-11-27  
> **점검 대상**: WinPurify Pro 코어 엔진(`FreshHandoverService`, `HandoverPolicyService`, `AccountCredentialPurgeService`), 원격 에이전트(`RemoteAgentService`, `RemoteTlsServer`, `RemoteAuthValidator`), 키 볼트(`ClusterSecurityVault`), Electron IPC(`electron-ipc-shared.js`), 웹 개발 서버(`dev-server.js`), C++ 네이티브 코어(`PurifyEngineCore.dll`)  
> **개발 총괄**: AhBiYout  
> **공식 링크**: [https://ahbiyoutvibe.blogspot.com/](https://ahbiyoutvibe.blogspot.com/)

---

## 📌 1. 개요 및 점검 목적 (Executive Summary)

WinPurify Pro(`v4.71.1`)는 PC 양도·초기화를 위한 **이전 사용자 프로필 및 로그인 세션 완전 소거(Clean Handover)** 기능과 사내/작업실 LAN 원격 관제 시스템(**TLS 9874 / HTTP 9870 / UDP 9871**)을 함께 제공합니다. 본 보고서는 인증·키 보관, 원격 명령 검증, 파일시스템 및 자격증명 소거 안전성, 개발 서버 경로 격리 전 영역에 대한 심층 점검 결과와 적용된 방어 체계를 기록합니다.

---

## 🔍 2. 핵심 보안 통제 및 조치 내역 (v4.69.0 ~ v4.71.1)

### ✅ 1. 클러스터 키 암호화 볼트 및 평문 비밀키 전송 전면 차단
- **적용 위치**: `Services/ClusterSecurityVault.cs`, `Services/RemoteAuthValidator.cs`, `electron-ipc-shared.js`, `src/securityVault.ts`
- **구현 내용**:
  - 하드코딩된 기본 클러스터 키를 완전 제거하고, 최초 구동 시 `RandomNumberGenerator`로 256-bit 암호학적 난수 키(`WPP-STD-...`)를 생성합니다.
  - C# 에이전트/커맨더는 `%ProgramData%\WinPurifyPro\Vault\cluster.key.dpapi`에 Windows **DPAPI (`DataProtectionScope.LocalMachine`) + `SYSTEM`/`BUILTIN\Administrators` 최소 권한 ACL**로 암호화 저장합니다.
  - Electron 데스크톱 콘솔은 OS 키체인/DPAPI 기반 **`safeStorage`**로 클러스터 키를 암호화 저장합니다.
  - 요청 본문에 평문 `secretKey` 필드가 포함된 경우 `RemoteAuthValidator`가 `[plaintext_secret_deprecated]`로 즉시 `401 Unauthorized` 차단합니다.

### ✅ 2. Zero-Config TLS 1.2/1.3 (TCP:9874) + TOFU 인증서 피닝 + 6자리 PIN 페어링
- **적용 위치**: `Services/RemoteTlsServer.cs`, `Services/RemoteAgentService.cs`, `electron-ipc-shared.js`
- **구현 내용**:
  - 에이전트 기동 시 ECDSA P-256 자체 서명 X.509 인증서를 생성하고 **TCP 9874 HTTPS(TLS 1.2/1.3)** 수신기를 기본 활성화합니다.
  - 최초 연결 시 서버 인증서의 SHA-256 지문을 로컬 `trusted_nodes.json` / `trusted-nodes.json`에 기록(**TOFU**)하며, 이후 지문이 변조될 경우 즉시 연결을 차단합니다.
  - 6자리 일회용 페어링 PIN(`POST /api/v1/auth/pair`)은 암호화된 TLS 채널 또는 루프백(`127.0.0.1`)을 통해서만 허용됩니다.

### ✅ 3. Challenge-Response Nonce + IP 바인딩 + Replay Attack 방어
- **적용 위치**: `Services/RemoteAuthValidator.cs`, `electron-ipc-shared.js`
- **구현 내용**:
  - 모든 원격 텔레메트리(`GET /api/v1/telemetry`) 및 배치 명령(`POST /api/v1/batch/*`)에 `X-WinPurify-Timestamp`(±60초 허용) + 1회용 `X-WinPurify-Nonce` + `X-WinPurify-Signature`(HMAC-SHA256) 검증을 강제합니다.
  - 고위험 명령(`Reboot`, `Shutdown`, `ZeroTraceReset`, `Rollback`, `ForensicsWipe`, `AccountPurge`, `FreshHandover`)은 `/api/v1/auth/challenge`에서 발급한 60초 유효 1회용 `ChallengeNonce` 및 **발급 클라이언트 IP 일치 검증(`challenge_ip_mismatch`)**을 통과해야만 실행됩니다.
  - 포터블 실행 모드(`AppEnvironment.IsPortable`)에서는 원격 명령 수신이 비활성화됩니다.

### ✅ 4. OS 자격증명 직접 P/Invoke 소거 & 무셸(Shell-Free) 실행 (v4.71.1 강화)
- **적용 위치**: `Services/FreshHandoverService.cs`, `Services/AccountCredentialPurgeService.cs`, `Services/RemoteAgentService.cs`
- **구현 내용**:
  - Windows Credential Vault 열거/삭제 시 `cmd.exe /c cmdkey /delete:"..."` 셸 문자열 조합을 제거하고, `Advapi32.dll`의 **`CredEnumerateW` / `CredDeleteW` / `CredFree` Win32 네이티브 API**를 직접 호출하여 자격증명 이름 내 특수문자(`&`, `"`, `|` 등)로 인한 셸 파싱 왜곡 가능성을 원천 차단했습니다.
  - 원격 `/browser-factory-reset` 및 `/zero-trace-reset`에서도 `RunHiddenCmd`(`cmd.exe /c`) 셸 실행을 완전히 제거하고 `Process.GetProcessesByName` + `FreshHandoverService.ForceDeleteDirectory` 및 `ProcessStartInfo.ArgumentList` 직접 호출로 전환했습니다.
  - 프로필 SID 언로드/CIM 정리 및 레지스트리 정리는 엄격한 도메인 SID 정규식(`^S-1-5-21-\d+(-\d+)+$`) 검증을 거칩니다.

### ✅ 5. 자기 보호(`IsPathProtected`), NTFS Junction 차단 및 재부팅 예약 가드 (v4.71.1 강화)
- **적용 위치**: `Services/FreshHandoverService.cs`, `Services/HandoverPolicyService.cs`, `electron-ipc-shared.js`
- **구현 내용**:
  - C# 및 Electron 소거 엔진 모두 현재 `WinPurify Pro` 실행 디렉터리, `%ProgramData%\WinPurifyPro`, `ProgramData`, `userData`, 드라이브 루트(`C:\`, `/`), `Windows`, `ProgramFiles`, `ProgramFiles(x86)`, 시스템 보호 프로필(`Public`, `Default`, `Default User`, `All Users`, `defaultuser0`, `WDAGUtilityAccount`) 및 현재 로그인 사용자 홈 루트 자체 삭제를 차단합니다.
  - C# `IsReparsePointPath` 및 Electron `fs.lstatSync().isSymbolicLink()` 검사로 NTFS Junction 및 심볼릭 링크 내부로 재귀 탐색/삭제하지 않도록 방어합니다.
  - Electron 선종료 옵션(`terminateProcesses`)은 사전 정의된 안전한 브라우저·메신저·개발도구 프로세스 허용 목록(`SAFE_HANDOVER_PROCESS_SET`)에 포함된 프로세스만 `execFileSync('taskkill.exe', ...)`로 종료하며, 잠금 파일은 하위 파일 먼저·상위 폴더 나중의 상향식 순서로 `PendingFileRenameOperations`에 등록합니다.

### ✅ 6. 웹 개발 서버(`dev-server.js`) 경로 격리, Origin/CSRF 및 페이로드 보호
- **적용 위치**: `dev-server.js`
- **구현 내용**:
  - 정적 파일 서빙 시 `decodeURIComponent` 예외 및 NULL 바이트 차단, `path.resolve` + `rootDir + path.sep` 경계 확인으로 형제 디렉터리 접두어 우회 및 경로 순회를 차단합니다.
  - 숨김 파일/도트파일(`.git`, `.env`), 백엔드 C#/C++ 소스 디렉터리, 로그 폴더(`WinPurify_Logs`) 및 Electron/패키지 설정 파일 정적 노출을 차단하고 허용된 웹 확장자만 제공합니다.
  - `readBody` 요청 본문 크기를 1MB로 제한하고, 와일드카드 `Access-Control-Allow-Origin: *` 대신 `localhost`/`127.0.0.1`/동일 호스트 오리진만 허용하며 `Sec-Fetch-Site: cross-site` API 호출을 거부합니다.

---

## 📦 3. 서드파티 패키지(Dev-Dependencies) 감사 현황

- **웹 프론트엔드 런타임 번들(`react`, `react-dom`, `d3`, `recharts`, `lucide-react`, `esbuild`)**: `bun audit` 기준 알려진 취약점 **0건**.
- **개발·빌드 전용 패키지(`devDependencies`)**: `electron@28.2.0` 및 `electron-builder@24.13.3` 하위 빌드 체인에서 빌드 타임 전용 권고가 보고되나, 해당 패키지는 Windows WPF 바이너리(`WinPurifyPro.exe`) 및 웹 프론트엔드 배포 번들에 포함되지 않습니다.

---

## 📋 4. 종합 평가 (Summary)

- **보안 및 무결성 상태**: **PASS (로컬 오프라인 완결형 Zero-Telemetry + TLS/DPAPI + Win32 직접 제어 아키텍처 확립)**
- 상세 검증 시나리오 및 릴리스 변경 내역은 [`docs/PATCH_NOTES.md`](./PATCH_NOTES.md)에서 확인할 수 있습니다.
