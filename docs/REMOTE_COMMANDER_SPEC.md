# 🛡️ WinPurify Pro Central Commander - Architecture & Security Specification

**Version**: 1.0 (for WinPurify Pro v4.9.0+)  
**Target Platform**: Windows 10 / 11 (x64) (.NET 8 WPF + Native C++ Core)  
**Classification**: Enterprise & Multi-PC Fleet Management Architecture (Option B: Dedicated Desktop Controller)

---

## 1. 🌐 시스템 개요 (System Overview)

WinPurify Pro Central Commander는 단일 로컬 PC 제어를 넘어, **동일 로컬 네트워크(LAN), 사내망, PC방, 연구실 또는 가족 네트워크 내의 다중 Windows PC를 중앙에서 일괄 모니터링하고 원격 최적화하는 독립형 데스크톱 관제 시스템**입니다.

웹 브라우저를 통한 비인가 접근 위험을 원천 차단하기 위해 **Option B (독립형 데스크톱 WPF 관리자 전용 프로그램)** 방식을 채택하며, 암호화된 바이너리/REST 패킷과 4중 보안 검증 체계를 통해 인가된 관리자 PC에서만 클러스터 노드를 제어할 수 있도록 설계되었습니다.

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│              [중앙 관제 콘솔] WinPurify Pro Central Commander                   │
│              (관리자 전용 독립 데스크톱 WPF 애플리케이션)                       │
│                                                                                 │
│  - 다중 PC 실시간 상태 그리드 모니터링 (CPU, RAM, 최적화 점수, 프로세스)       │
│  - 원클릭 일괄 명령: 전체 RAM 정화, 프리셋 일괄 배포, 복원지점 생성             │
│  - UDP 브로드캐스트 기반 원터치 자동 노드 탐색 (Zero-Configuration)             │
│  - HMAC-SHA256 디지털 서명 및 마스터 클러스터 키 보안 엔진                      │
└───────────────────────────────────────┬─────────────────────────────────────────┘
                                        │
             ┌──────────────────────────┼──────────────────────────┐
             │ (보안 REST / WebSocket)   │ (보안 REST / WebSocket)   │ (보안 REST / WebSocket)
             ▼                          ▼                          ▼
   ┌───────────────────┐      ┌───────────────────┐      ┌───────────────────┐
   │  [Target Node 1]  │      │  [Target Node 2]  │      │  [Target Node 3]  │
   │  WinPurify Pro    │      │  WinPurify Pro    │      │  WinPurify Pro    │
   │  Agent Service    │      │  Agent Service    │      │  Agent Service    │
   │  포트: TCP 9870   │      │  포트: TCP 9870   │      │  포트: TCP 9870   │
   │  C++ Native 엔진  │      │  C++ Native 엔진  │      │  C++ Native 엔진  │
   └───────────────────┘      └───────────────────┘      └───────────────────┘
```

---

## 2. 🔒 4중 보안 아키텍처 (4-Layer Security Model)

비인가자나 네트워크 내 악의적 사용자가 명령을 위조하거나 PC를 조작할 수 없도록 **4중 보안 장벽**을 구성합니다.

| 보안 계층 | 기술 사양 | 상세 동작 원리 |
| :--- | :--- | :--- |
| **Layer 1: 파일 및 실행 격리** | 데스크톱 .exe 배포 분리 | 일반 사용자 PC에는 에이전트만 실행되며, 관제 프로그램(`WinPurifyCommander.exe`)은 관리자 본인 PC에만 물리적으로 보관. |
| **Layer 2: 클러스터 시크릿 키** | Pre-Shared Key (PSK) | 관리자 앱과 각 에이전트 노드 간 최초 1회 공유 비밀키(Secret Key) 설정. 키가 일치하지 않는 모든 패킷은 즉시 드롭(Drop). |
| **Layer 3: HMAC-SHA256 디지털 서명** | RFC 2104 규격 서명 | 모든 요청 헤더에 `X-WinPurify-Signature: HMAC_SHA256(Body + Timestamp + Nonce, SecretKey)`를 부착하여 위변조 원천 방지. |
| **Layer 4: 재전송 공격 방지 & IP 필터링** | Nonce + Timestamp + Whitelist | 30초 이상 경과된 패킷이나 중복 Nonce는 즉시 폐기하며, 각 노드 설정에서 인가된 관리자 IP(예: `192.168.1.100`)만 허용. |

---

## 3. 📡 통신 프로토콜 및 포트 정의 (Communication Protocol)

### 3.1 포트 할당
- **UDP 9871 (Discovery Beacon)**: 중앙 관제 콘솔이 LAN 상의 노드를 자동 탐색(Ping/Pong)하기 위한 브로드캐스트 포트
- **TCP 9870 (Secure Agent REST & Telemetry Stream)**: 실시간 텔레메트리 전송 및 일괄 명령 수신용 포트

### 3.2 인증 헤더 규격
모든 HTTP 제어 요청에는 다음 헤더가 필수적으로 포함됩니다:
```http
POST /api/v1/batch/ram-clean HTTP/1.1
Host: 192.168.1.50:9870
Content-Type: application/json
X-WinPurify-NodeId: node-desktop-01
X-WinPurify-Timestamp: 1787994800
X-WinPurify-Nonce: e8f9c2d1-4b6a-49e2-8d7b-9c3f1e5a7b8c
X-WinPurify-Signature: a3f89e81b67f4c90d56c87a12b... (HMAC-SHA256)
```

---

## 4. 🛠️ 핵심 API 엔드포인트 사양 (Agent Node API)

### 4.1 상태 조회 및 텔레메트리 (Telemetry)
- **`GET /api/v1/telemetry`**
  - **응답 본문**:
    ```json
    {
      "nodeName": "DESKTOP-GAMING-01",
      "osVersion": "Windows 11 Pro 23H2 (22631.3007)",
      "uptimeSeconds": 86400,
      "cpuUsagePercent": 14.5,
      "ramTotalMb": 32768,
      "ramUsedMb": 10240,
      "ramUsagePercent": 31.25,
      "optimizationScore": 96,
      "activePreset": "HighPerformanceGaming",
      "nativeEngineAvailable": true,
      "appliedRulesCount": 112,
      "totalRulesCount": 115
    }
    ```

### 4.2 일괄 제어 명령 (Batch Commands)
- **`POST /api/v1/batch/ram-clean`**:
  - 내장 C++ Native 엔진(`PurifyEngineCore.dll`)을 통해 시스템 워킹셋 및 대기 메모리 즉시 일괄 정화
- **`POST /api/v1/batch/apply-preset`**:
  - 최적화 프리셋(`Gaming`, `PrivacyWork`, `Balanced`, `DeepClean`)을 전달하여 115개 룰 일괄 원격 적용
- **`POST /api/v1/batch/restore-point`**:
  - 시스템 변경 전 Windows 시스템 복원 지점(`WinPurify Pro Batch Restore Point`) 자동 생성
- **`POST /api/v1/batch/benchmark`**:
  - C++ CPU/Memory/Disk 벤치마크를 원격 트리거하고 결과를 관제 센터로 전송

---

## 5. 🖥️ 중앙 관제 콘솔 UI/UX 기능 명세 (Commander Dashboard)

1. **클러스터 플릿 뷰 (Fleet Grid View)**:
   - 등록된 모든 PC를 카드 형태로 나열.
   - 온라인/오프라인 상태 배지, 실시간 CPU/RAM 부하 게이지, 최적화 점수(0~100) 표시.
   - 그룹 태그별 필터링 (예: `전체`, `사무실`, `게이밍룸`, `테스트 랩`).
2. **글로벌 일괄 액션 툴바 (Global Command Bar)**:
   - `[⚡ 전체 PC RAM 일괄 정화]` : 클릭 한 번으로 모든 온라인 노드의 RAM을 동시 정리.
   - `[🎯 일괄 프리셋 배포]` : 선택한 프리셋을 전체 또는 선택한 PC 그룹에 1초 만에 배포.
   - `[🛡️ 전체 복원 지점 생성]` : 대규모 윈도우 업데이트 전 모든 PC의 스냅샷 생성.
3. **노드 상세 제어 모달 (Individual Node Control)**:
   - 특정 PC 카드를 클릭하여 115개 최적화 룰의 세부 On/Off 상태를 원격으로 토글.
   - 실시간 상위 프로세스 점유율 및 하드웨어 사양 상세 확인.

---

## 6. 📦 빌드 파이프라인 확장 설계 (`build.bat`)

기존 4가지 빌드 타겟에 관리자 전용 타겟을 추가하여 체계적으로 패키징합니다:
- **`publish\SingleFile\WinPurifyPro.exe`**: 클라이언트 PC용 단일 파일 포터블 노드
- **`publish\Installer\WinPurifyPro_Setup.exe`**: 클라이언트 PC용 정식 설치본
- **`publish\Commander\WinPurifyCommander.exe`**: 관리자 전용 독립형 중앙 관제 콘솔
- **`publish\All\`**: 전체 패키지 일괄 빌드

---

본 명세서는 WinPurify Pro 다중 PC 원격 관제 아키텍처의 공식 기술 규격서로 사용됩니다.
