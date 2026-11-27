# 🛡️ WinPurify Pro Central Commander - Architecture & Security Specification (`v4.71.1`)

**Version**: `v4.71.1`  
**Target Platform**: Windows 10 / 11 (x64) — Dedicated Desktop Controllers (`.NET 10 WPF` `WinPurifyCommander.exe` & `Electron` Desktop Commander)  
**Classification**: Small Studio & Enterprise LAN Fleet Management Architecture (Zero-Trust TLS 9874)

---

## 1. 🌐 시스템 개요 (System Overview)

WinPurify Pro Central Commander는 동일 로컬 네트워크(LAN), 작업실 단일 공유기, 사내망 및 테스트 랩의 다중 Windows PC를 중앙에서 실시간 모니터링하고 원격으로 **클린 환경 인수인계(`FreshHandover`), 통합 계정 정화(`AccountPurge`), RAM 압축, 최적화 프리셋 배포 및 무음 스케줄러 설정**을 수행하는 독립형 데스크톱 관제 시스템입니다.

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│              [중앙 관제 콘솔] WinPurify Pro Central Commander                   │
│       (WPF WinPurifyCommander.exe 또는 Electron 독립 데스크톱 콘솔)              │
│                                                                                 │
│  • 6자리 일회용 PIN 기반 TLS 페어링 (POST /api/v1/auth/pair)                    │
│  • 하이브리드 LAN 노드 탐색 (멀티 NIC UDP:9871 + /24 서브넷 TCP:9874/9870)       │
│  • TOFU (Trust On First Use) ECDSA P-256 인증서 SHA-256 지문 피닝               │
│  • Win32 DPAPI / Electron safeStorage 암호화 키 볼트                            │
└───────────────────────────────────────┬─────────────────────────────────────────┘
                                        │
             ┌──────────────────────────┼──────────────────────────┐
             │ HTTPS TLS 1.2/1.3 (:9874)│ (HMAC-SHA256 + Nonce)    │
             ▼                          ▼                          ▼
   ┌───────────────────┐      ┌───────────────────┐      ┌───────────────────┐
   │  [Target Node 1]  │      │  [Target Node 2]  │      │  [Target Node 3]  │
   │  WinPurifyAgent   │      │  WinPurifyAgent   │      │  WinPurifyAgent   │
   │  TLS :9874 (우선) │      │  TLS :9874 (우선) │      │  TLS :9874 (우선) │
   │  HTTP:9870 (호환) │      │  HTTP:9870 (호환) │      │  HTTP:9870 (호환) │
   │  UDP :9871 (탐색) │      │  UDP :9871 (탐색) │      │  UDP :9871 (탐색) │
   └───────────────────┘      └───────────────────┘      └───────────────────┘
```

---

## 2. 🔒 Zero-Trust 보안 모델 (Security Architecture)

| 보안 계층 | 기술 사양 | 상세 동작 원리 |
| :--- | :--- | :--- |
| **Layer 1: 암호화 전송 채널** | ECDSA P-256 TLS 1.2/1.3 (:9874) | `RemoteTlsServer.cs` 내장 `SslStream` 서버가 기본 암호화 채널을 제공하며, Commander는 최초 접속 지문을 고정(TOFU)합니다. |
| **Layer 2: 암호화 키 금고** | DPAPI / `safeStorage` | 256-bit 난수 마스터 키(`WPP-STD-...`)를 DPAPI(LocalMachine) 또는 Electron `safeStorage`에 암호화 보관하며 평문 `secretKey` 전송 시 `401` 거부합니다. |
| **Layer 3: 1-Click 6자리 PIN 페어링** | `POST /api/v1/auth/pair` | TLS 채널(또는 루프백)에서만 6자리 일회용 PIN을 검증하여 안전하게 클러스터 키와 TLS 지문을 동기화합니다. |
| **Layer 4: Challenge + Nonce + IP 바인딩** | HMAC-SHA256 + IP 검증 | 모든 요청에 ±60초 타임스탬프·1회용 Nonce 서명을 검증하며, 고위험 명령(`FreshHandover`, `AccountPurge`, `ZeroTraceReset`, `Reboot`, `Shutdown`, `Rollback`, `ForensicsWipe`)은 발급 IP와 일치하는 1회용 `ChallengeNonce`를 필수로 요구합니다. |

---

## 3. 📡 통신 프로토콜 및 포트 정의 (Ports & Headers)

- **TCP 9874 (Primary TLS 1.2/1.3)**: HTTPS REST & 텔레메트리 및 PIN 페어링 기본 채널
- **TCP 9870 (Legacy HTTP Compatibility)**: 루프백/호환 챌린지 및 탐색 응답 채널
- **UDP 9871 (Discovery Beacon)** + **`/24` 서브넷 병렬 TCP 스캔**: 공유기 브로드캐스트 차단 환경에서도 자동 노드 탐지(`HybridNodeDiscoveryService.cs`, `electron-ipc-shared.js`)

---

## 4. 🛠️ 핵심 API 엔드포인트 사양 (`RemoteAgentService.cs`)

### 4.1 인증 및 상태 조회
- **`GET /api/v1/telemetry`**: HMAC-SHA256 서명 인증을 거쳐 CPU/RAM/Disk, 적용 모듈 수, TLS 포트 및 인증서 지문 반환
- **`GET /api/v1/auth/challenge`**: 클라이언트 IP에 바인딩된 60초 유효 일회용 `ChallengeNonce` 발급
- **`POST /api/v1/auth/pair`**: TLS 채널에서 6자리 PIN 검증 후 클러스터 키 및 TLS 지문 동기화

### 4.2 일괄 제어 명령 (`POST /api/v1/batch/*`)
- **`/api/v1/batch/fresh-handover`** (`FreshHandover`, Challenge 필수): `presetName`/`preset`(`PreviousUsersAndSessions`, `SessionsOnly`, `FullMachineReset`), `terminateProcessesBeforePurge`, `schedulePendingReboot` 기반으로 원격 PC의 이전 사용자·브라우저 다중 프로필·AI/개발도구·앱 세션·자격 증명을 소거하고 `PendingReboot`/`RemainingProfiles`/`RemainingSessions`/`Verified` 상세를 반환 (`HandoverFleetModal.tsx` 및 `remote-center.html`에서 매트릭스/로그 추적)
- **`/api/v1/batch/account-purge`** (`AccountPurge`, Challenge 필수): 15종 계정 및 세션 타깃 정화
- **`/api/v1/batch/zero-trace-reset`** (`ZeroTraceReset`, Challenge 필수): `ArgumentList` 기반 직접 실행으로 휴지통/DNS/Temp/RAM 및 클립보드·히스토리 초기화
- **`/api/v1/batch/ram-clean`** (`RamTrim`): C++ Native 엔진 물리 RAM 워킹셋 정화
- **`/api/v1/batch/apply-preset`** (`ApplyPreset`): `Safe`, `Deep`, `Gaming` 프리셋 배포
- **`/api/v1/batch/schedule`** / **`/unschedule`**: 자동 유지보수 작업 스케줄러 등록/해제
- **`/api/v1/batch/restore-point`** / **`/rollback`**: Live Safepoint 생성 및 롤백
- **`/api/v1/batch/browser-factory-reset`** / **`/forensics-wipe`** / **`/profile-sync`**: 셸 호출 없는 직접 프로세스 종료 및 정션 안전 삭제(`ForceDeleteDirectory`)로 브라우저 초기화 및 특수 정화 제어
- **`/api/v1/batch/reboot`** / **`/shutdown`** (Challenge 필수): 4-Tier 강제 재부팅/종료
