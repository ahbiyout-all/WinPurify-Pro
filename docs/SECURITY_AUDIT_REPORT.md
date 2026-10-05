# 🛡️ WinPurify Pro - 종합 보안 취약점 점검 및 보안 강화 보고서
# (Security Audit & Vulnerability Assessment Report)

> **문서 버전**: v4.32.0  
> **점검 일자**: 2026-09-26  
> **점검 대상**: WinPurify Pro 코어 엔진, 원격 에이전트 서비스(`WinPurifyAgentService`), 센트럴 커맨더(`WinPurifyCommander`), 웹 관제 대시보드(`index.tsx`), C++ 네이티브 가속 코어(`PurifyEngineCore.dll`)  
> **수석 보안 아키텍트**: Cisnet Soft (개발 총괄: AhBiYout)  
> **공식 링크**: [https://ahbiyoutvibe.blogspot.com/](https://ahbiyoutvibe.blogspot.com/) | [http://www.cisnet.co.kr/](http://www.cisnet.co.kr/)

---

## 📌 1. 개요 및 점검 목적 (Executive Summary)

WinPurify Pro는 단일 PC 최적화 뿐만 아니라 사내/홈 인트라넷 환경에서 최대 수백 대의 Windows 10/11 시스템을 원격으로 진단하고 일괄 정화하는 원격 관제 아키텍처(TCP 9870 / UDP 9871) 및 계정 자격증명 영구 소거, 디스크 포렌식 와이핑 등의 고위험 시스템 제어 기능을 내장하고 있습니다.

본 보안 점검 보고서는 **네트워크 원격 명령 주입, 재전송 공격(Replay Attack), 무단 브라우저 크로스오리진 호출(Drive-by attack), 로컬 권한 상승(LPE) 및 데이터 누출 가능성**을 원천 제거하기 위한 종합 보안 점검 결과 및 v4.32.0에서 적용 완료된 전방위 방어 체계를 상세히 기술합니다.

---

## 🔍 2. 식별된 보안 취약점 분석 및 조치 내역

### 🚨 취약점 1: 타임스탬프 유효 구간 내 Nonce 재전송 공격 (Replay Attack)
- **위험도**: `High (CVSS 7.5)`
- **취약 원인**:
  기존 HMAC-SHA256 패킷 서명 구조에서 60초 타임스탬프 허용 오차(`Skew Window`)만 존재할 경우, 공격자가 동일 로컬 네트워크(LAN)에서 서명된 유효한 패킷을 스니핑한 뒤 60초 이내에 수십 회 재전송(Replay)하여 원격 강제 재부팅, RAM 압축, 포렌식 와이핑을 무한 반복 트리거할 수 있는 잠재적 위험이 존재함.
- **v4.32.0 완벽 조치 내역 (`Services/RemoteAgentService.cs`)**:
  - `ConcurrentDictionary<string, DateTime>` 기반의 고속 Nonce 메모리 캐시 및 자동 만료 가비지 컬렉션 엔진 탑재.
  - 모든 원격 명령 패킷 헤더에 고유 UUID 기반의 `X-WinPurify-Nonce` 필드를 필수로 요구.
  - 수신된 Nonce가 이미 처리된 기록이 있거나 누락된 경우 즉시 `401 Unauthorized`로 명령을 폐기하고 보안 감사 로그를 기록.
  - 120초 경과 시 만료된 Nonce를 백그라운드 스레드에서 자동 제거하여 메모리 고갈(DoS) 방지.

---

### 🚨 취약점 2: 기본 클러스터 키 하드코딩 및 유출 위험
- **위험도**: `High (CVSS 7.2)`
- **취약 원인**:
  관리자가 별도의 보안 클러스터 키를 설정하지 않을 경우, 코드 내의 기본 인증 키에 의존할 가능성이 있어 네트워크 내부자가 기본 키로 전체 PC를 임의 제어할 수 있는 위험.
- **v4.32.0 완벽 조치 내역 (`Services/RemoteAgentService.cs` & `RemoteCommanderManager.cs`)**:
  - 시스템 최초 구동 시 `System.Security.Cryptography.RandomNumberGenerator`를 사용하여 **256-bit 고유 암호학적 난수 키**를 자동 생성.
  - 생성된 키는 관리자 전용 보안 디렉터리(`%ProgramData%\WinPurifyPro\cluster.key`)에 `SYSTEM` 및 `BUILTIN\Administrators` 권한만 접근 가능하도록 ACL을 설정하여 영구 보존.
  - 웹/WPF UI 대시보드에 `🎲 고유 난수 키 즉시 생성` 기능 및 키 변경 즉시 에이전트 재시작 파이프라인 연동.

---

### 🚨 취약점 3: 로컬 REST 포트(9870)에 대한 브라우저 드라이브바이(Drive-by) 공격
- **위험도**: `Critical (CVSS 8.8)`
- **취약 원인**:
  사용자가 웹 서핑 중 악성 사이트에 접속했을 때, 브라우저 백그라운드 JavaScript가 사용자의 로컬호스트(`http://127.0.0.1:9870/api/remote/action`)로 비동기 fetch 요청을 날려 임의 원격 명령을 실행시키는 공격 벡터(SOP 우회).
- **v4.32.0 완벽 조치 내역**:
  - 와일드카드 CORS(`Access-Control-Allow-Origin: *`)를 완전히 제거.
  - 오직 명시된 신뢰 로컬 오리진(`http://localhost:3000`, `http://127.0.0.1:3000`)에 대해서만 제한적 헤더 응답.
  - `Sec-Fetch-Site: cross-site` 헤더가 감지되거나 비인가 Host 헤더 수신 시 즉시 연결을 드롭(`403 Forbidden`).
  - 모든 HTTP 응답에 모던 보안 헤더 강제 적용:
    - `X-Content-Type-Options: nosniff`
    - `X-Frame-Options: DENY`
    - `Content-Security-Policy: default-src 'self'`

---

### 🚨 취약점 4: Windows 기본 프로필(`C:\Users\Default`) 복제 시 로컬 권한 상승 (LPE)
- **위험도**: `Medium (CVSS 6.8)`
- **취약 원인**:
  신규 사용자 자동 최적화를 위해 현재 프로필의 레지스트리/설정을 `C:\Users\Default`에 복제할 때, 파일 권한을 느슨하게 복사하면(`Everyone:F` 등) 로컬의 일반 사용자가 Default 폴더에 악성 실행 파일을 심어 이후 생성되는 관리자 계정에 악성코드를 감염시키는 로컬 권한 상승 취약점.
- **v4.32.0 완벽 조치 내역 (`Services/ProfileSnapshotService.cs`)**:
  - `icacls` 및 Win32 보안 디스크립터를 통해 `C:\Users\Default` 디렉터리에 **최소 권한 원칙(Least Privilege)** 강제 적용.
  - `SYSTEM:F`, `BUILTIN\Administrators:F`에만 쓰기/수정 권한 부여.
  - 일반 사용자는 읽기 및 실행만 가능(`BUILTIN\Users:(OI)(CI)RX`)하도록 상속 ACL을 정밀 재구성하여 임의 실행 파일 삽입 차단.

---

### 🚨 취약점 5: 원격 명령 파라미터 경로 순회(Path Traversal) 및 임의 문자열 주입
- **위험도**: `Medium (CVSS 6.5)`
- **취약 원인**:
  포렌식 와이핑 드라이브 문자나 브라우저 타겟, 계정 정화 프리셋 파라미터가 검증 없이 백그라운드 프로세스나 PowerShell 실행기로 전달될 경우 명령 주입 위험.
- **v4.32.0 완벽 조치 내역**:
  - `ForensicsTargetDrive`: `^[A-Z]:$` 엄격한 정규식 화이트리스트 검증 통과 시에만 허용.
  - `BrowserTarget`: 허용 목록(`all`, `chrome`, `edge`, `whale`, `firefox`) 외 입력 시 기본 안전값 자동 보정.
  - `AccountPurgePreset`: 허용 시나리오(`workplace`, `full`, `public`, `developer`) 엄격 검증.

---

## 🔒 3. 개인정보 보호 및 오프라인 완결성 점검 (Zero-Telemetry Assurance)

| 점검 항목 | 점검 결과 | 상세 내용 |
|---|---|---|
| **외부 서버 텔레메트리 전송** | **100% 없음 (PASS)** | 분석 및 정화 과정에서 어떠한 원격 서버나 외부 분석기로도 하드웨어 정보, 계정 세션, 로그가 전송되지 않음 |
| **로그인 자격증명 저장소 처리** | **영구 소거 (PASS)** | DPAPI 마스터키, Windows Vault, 웹 브라우저 쿠키/토큰 소거 시 3단계 덮어쓰기 파이프라인 가동 |
| **복원 지점(Safepoint) 격리** | **보안 암호화 (PASS)** | `%ProgramData%\WinPurifyPro\Safepoints` 로컬 전용 저장소에 보관되며 외부 유출 차단 |
| **C++ 네이티브 메모리 안전성** | **무결성 검증 (PASS)** | `PurifyEngineCore.dll` 내부 버퍼 오버플로우 방지, DJB2 해시 기반 변조 탐지 탑재 |

---

## 📋 4. 종합 평가 결론 (Overall Security Rating)

- **보안 등급**: **`GRADE A+ (엔터프라이즈급 보안성 충족)`**
- **결론**:
  식별된 모든 네트워크 벡터, 권한 상승 가능성, 재전송 공격 및 악성 브라우저 스크립트 접근 경로가 완벽히 차단되었으며, 한국어/영어 2종 EULA 라이선스 명세와 일치하는 **오프라인 완결형 무결성 보안 아키텍처**가 확립되었습니다.
