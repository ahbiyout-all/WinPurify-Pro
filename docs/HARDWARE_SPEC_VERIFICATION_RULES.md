# 🛡️ WinPurify Pro - 달라진 하드웨어 스펙 검증을 위한 규칙 선언서
# (Hardware Specification Drift & Verification Rules Specification)

- **규정 코드**: `WPP-RULE-HW-2026-V1`
- **최초 제정일**: 2026-10-09
- **적용 대상**: WinPurify Pro Desktop (C# WPF), Native C++ Core (`PurifyEngineCore.dll`), Background Service (`WinPurifyAgentService`), Central Commander 및 Web UI
- **문서 책임자**: AhBiYout (WinPurify Pro)

---

## 📌 제1장 총칙 (General Principles)

### 제1조 (목적)
본 선언서는 **WinPurify Pro**가 구동되는 Windows 호스트 시스템의 CPU, RAM, 스토리지, GPU, 펌웨어/보안 서브시스템의 물리적·논리적 스펙 변경(Hardware Drift / Hardware Upgrade / Slot Dropout / Hot-plug)을 신속하고 정확하게 감지 및 검증하고, 시스템 성능 최적화 파이프라인의 안전성과 정합성을 유지하기 위한 **5대 하드웨어 스펙 검증 규칙**을 공식 선언함을 목적으로 한다.

### 제2조 (배경 및 필요성)
1. **최적화 프리셋의 하드웨어 종속성**:
   - WinPurify Pro의 144개 최적화 모듈(물리 RAM Working Set 트림, MMCSS 멀티미디어/게임 스레드 우선순위 격상, 가상 메모리 Paging File 크기, SSD TRIM 강제 명령, Volume Shadow Copy 스토리지 할당 한도 등)은 호스트의 구체적인 하드웨어 스펙에 정밀하게 동기화되어 동작합니다.
2. **스펙 변동 시의 잠재적 리스크**:
   - **RAM 증설(예: 16GB → 32GB/64GB)** 시: 기존 16GB 기준의 과도하게 잦은 Working Set 강제 압축은 불필요한 페이징 I/O 오버헤드를 유발함.
   - **RAM 탈락(예: DIMM 슬롯 접촉 불량으로 32GB → 16GB)** 시: 가상 메모리 부족 및 OS 크래시 방지를 위해 즉시 보수적 캐시 정책 및 경고가 발령되어야 함.
   - **스토리지 교체(예: SATA SSD → PCIe Gen4/5 NVMe SSD)** 시: 디프래그 스케줄링 전면 배제, TRIM 가속(`DisableDeleteNotify=0`) 강제 적용 및 VSS 섀도우 복사본 최대 공간 자동 재계산이 필수적임.
   - **CPU 교체(예: 구형 멀티코어 → Intel 12~14세대/Core Ultra P+E 하이브리드 코어 또는 AMD 3D V-Cache)** 시: 이종 스케줄링(Heterogeneous Thread Scheduling) 및 MMCSS 오디오/렌더러 코어 선호도(Core Affinity)를 전면 재교정해야 함.

---

## 📌 제2장 5대 하드웨어 스펙 검증 규칙 (The 5 Verification Rules)

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                 WinPurify Pro 5대 하드웨어 스펙 검증 규칙 체계                 │
├─────────────────┬──────────────────────────────────┬────────────────────────────┤
│ 규칙 번호       │ 핵심 대상 및 검증 영역           │ 주요 집행 정책             │
├─────────────────┼──────────────────────────────────┼────────────────────────────┤
│ Rule 1          │ 기준 지문 및 불변 무결성 해시    │ SHA-256 System Hash 대조   │
│ Rule 2 (CPU)    │ 코어 토폴로지 및 명령어 세트     │ 스레드 풀 / 전원 계획 갱신 │
│ Rule 3 (RAM)    │ 물리 메모리 용량 및 채널 대역폭  │ Working Set / 페이징 조율  │
│ Rule 4 (Disk)   │ 스토리지 미디어 티어 및 TRIM     │ NVMe/SSD TRIM & VSS 재조정 │
│ Rule 5 (GPU/Sec)│ 그래픽 어댑터 / UEFI Secure Boot │ HAGS / VRAM / 보안 격리    │
└─────────────────┴──────────────────────────────────┴────────────────────────────┘
```

---

### 🏛️ Rule 1: 기준 하드웨어 지문 및 불변 무결성 해시 검증 규칙 (Baseline Fingerprint & Immutability Hash)

1. **지문 생성 원칙**:
   - 시스템 최초 최적화 실행 시점 또는 관리자 승인 기준점 갱신 시, 아래의 불변 하드웨어 식별자를 결합하여 `SHA-256` 기반의 **System Hardware Fingerprint**를 생성한다.
     - `CPUID`: 프로세서 시그니처 및 패밀리/모델/스테핑
     - `SMBIOS UUID`: 마더보드 고유 시스템 UUID
     - `Baseboard Serial`: 메인보드 하드웨어 일련번호
     - `Primary Disk Serial`: 물리 시스템 드라이브 하드웨어 일련번호
     - `Installed Physical RAM Capacity`: 물리 메모리 바이트 합계
2. **저장 및 검증**:
   - 생성된 지문 해시는 Windows 레지스트리 `HKLM\SOFTWARE\WinPurifyPro\HardwareBaseline` 키 아래 `BaselineFingerprintHash` (REG_SZ) 및 암호화된 타임스탬프로 보존한다.
   - WinPurify Pro 기동 시 실시간 계산된 현재 하드웨어 해시와 저장된 기준 해시의 불일치 여부를 0.5초 이내에 대조 판별한다.

---

### 🏛️ Rule 2: CPU 토폴로지 및 명령어 세트 변경 검증 규칙 (CPU Topology & Instruction Set Verification)

1. **검증 지표**:
   - 물리 코어 수(`NumberOfCores`) 및 논리 프로세서 수(`NumberOfLogicalProcessors`)
   - 하이브리드 아키텍처 여부(Performance 코어 vs Efficient 코어 비율)
   - 최신 확장 명령어 세트 가용성 (`AVX-512`, `AVX2`, `SSE4.2`, `BMI2`)
   - 기본 및 터보 최대 클럭 주파수, L2/L3 캐시 용량
2. **판정 및 조율 정책**:
   - **코어 증가 감지 시**: Native C++ 엔진(`PurifyEngineCore.dll`)의 비동기 작업자 스레드 풀(`ThreadPoolCapacity = HardwareConcurrency * 2`)을 즉시 확장.
   - **하이브리드(P+E) 코어 전환 감지 시**: 백그라운드 정화 작업은 E-Core로 격리하고, 실시간 반응성(UI 렌더링, Robocopy I/O)은 P-Core에 고정하도록 MMCSS 스케줄러를 자동 분기.
   - **AVX2 / AVX-512 활성화 감지 시**: 메모리 블록 비교 및 데이터 와이핑 알고리즘을 256/512비트 SIMD 가속 명령으로 동적 승격.

---

### 🏛️ Rule 3: 물리 RAM 용량 및 대역폭 검증 규칙 (Physical RAM Capacity & Working Set Verification)

1. **검증 지표**:
   - 물리 메모리 총용량 (`GlobalMemoryStatusEx.ullTotalPhys`)
   - 메모리 채널 구성 (Dual-Channel vs Single-Channel vs Quad-Channel)
   - 메모리 속도 (MT/s, DDR4/DDR5 판별)
   - 커밋 한도(`CommitLimit`) 및 시스템 페이징 파일 크기
2. **용량 변동별 집행 정책**:
   - **RAM 증설 감지 (예: 16GB 이하 → 32GB 이상)**:
     - Working Set 메모리 트림 주기 완화 (과도한 캐시 소거 방지, 파일 시스템 캐시 적중률 보존).
     - 네이티브 디렉터리 스캐너 및 Robocopy 버퍼 크기를 16MB → 64MB로 자동 확장.
     - 가상 메모리 페이징 파일 고정 크기 상향 권고.
   - **RAM 축소 감지 (예: 32GB → 16GB, 슬롯 접촉 불량 의심)**:
     - ⚠️ **긴급 하드웨어 드리프트 경고 발령** (작업 표시줄 트레이 토스트 및 진단 모달 알림).
     - 긴급 Working Set 메모리 트림 임계치를 75% 수준으로 강화하여 OOM(Out of Memory) 사전 방지.

---

### 🏛️ Rule 4: 스토리지 미디어 티어 및 볼륨 구성 검증 규칙 (Storage Media Tier & TRIM Verification)

1. **검증 지표**:
   - 시스템 드라이브의 버스 타입 및 미디어 특성 (`IOCTL_STORAGE_QUERY_PROPERTY`)
     - `NVMe PCIe (Gen3 / Gen4 / Gen5)` vs `SATA SSD` vs `Rotational HDD`
   - TRIM 지원 상태 (`fsutil behavior query DisableDeleteNotify`)
   - 볼륨 섀도우 복사본(VSS) 저장소 할당 한도 (`vssadmin list shadowstorage`)
   - 4K 섹터 정렬 상태 (4Kn vs 512e)
2. **조율 및 강제 집행 정책**:
   - **SSD/NVMe 감지 시**:
     - 기존의 기계식 디스크 디프래그(Defrag) 관련 스케줄 전면 비활성화 유지.
     - TRIM 활성화(`DisableDeleteNotify = 0`) 규칙 강제 적용.
     - 윈도우 프리패치(Prefetcher) 및 수퍼패치(SysMain) 최적화 프로필을 NVMe 전용 초경량 모드로 전환.
   - **디스크 용량 확장 감지 시**:
     - VSS 섀도우 복사본 안전 스냅샷 최대 한도를 신규 볼륨 총용량의 **5%~10% (최대 32GB)**로 동적 재산정.

---

### 🏛️ Rule 5: GPU 디스플레이 어댑터 및 보안 가상화 검증 규칙 (GPU Adapter & Security/Virtualization)

1. **검증 지표**:
   - 주 디스플레이 어댑터 벤더 및 모델 (`PCI\VEN_xxxx&DEV_xxxx`, NVIDIA / AMD / Intel)
   - 전용 비디오 메모리(Dedicated VRAM) 용량
   - 하드웨어 가속 GPU 일정 예약 (HAGS, `HwSchMode`) 레지스트리 상태
   - DirectX 12 Feature Level (12_1 / 12_2 Ultimate) 및 Vulkan 1.3 가용성
   - UEFI Secure Boot 및 TPM 2.0 보안 상태
2. **조율 정책**:
   - **외장 GPU 장착 또는 세대 교체 감지 시**:
     - 구형 GPU의 잔여 셰이더 캐시(`DirectX Shader Cache`, `NVIDIA DXCache`, `AMD DxcCache`) 일괄 소거 권고.
     - Windows DWM 컴포지터 가속 및 게이밍 부스트 프로필 재적용.
   - **가상머신(Hyper-V / VMware / KVM) 전이 감지 시**:
     - 네이티브 하드웨어 직접 제어 모듈을 가상화 안전 모드로 격리하여 VM 호스트 충돌 방지.

---

## 📌 제3장 하드웨어 변경 감지 트리거 및 라이프사이클

```
[System Boot / Service Start] ──> WinPurifyAgentService (Background Baseline Diff)
                                               │
[User Launches WinPurify Pro] ──> PurifyEngineCore.dll (0.1ms Hardware Query)
                                               │
[Runtime Device Hot-Plug]    ──> WM_DEVICECHANGE Listener (Dynamic Storage/RAM Event)
                                               │
                                 ┌─────────────┴─────────────┐
                                 ▼                           ▼
                        [스펙 변동 없음 (Match)]    [스펙 변동 감지 (Drift Detected)]
                                 │                           │
                            정상 서비스 유지          🚨 3단계 검증 프로세스 가동
                                                       1) 변동 내역 비교 리포트 생성
                                                       2) 기존 종속 캐시 무효화 (Invalidate)
                                                       3) 세이프포인트 백업 후 신규 기준치 갱신
```

1. **Cold-Boot 트리거**:
   - Windows 부팅 시 `WinPurifyAgentService`가 기동되면서 로컬 하드웨어 핑거프린트를 읽어 기준 레지스트리와 비교.
2. **App Launch 트리거**:
   - WinPurify Pro 실행 시 즉시 실시간 진단 파이프라인에서 하드웨어 스펙 정밀 검증을 수행.
3. **Hot-Plug 이벤트 트리거**:
   - 외부 드라이브 파티션 변경, RAM 핫플러그(가상화 환경), 디스플레이 어댑터 변경 시 `WM_DEVICECHANGE`를 가로채어 실시간 반응.

---

## 📌 제4장 자동 무효화 및 안전 롤백 보장 규정

1. **종속 캐시 즉각 무효화**:
   - 하드웨어 변경이 감지되면 이전 하드웨어 기준으로 산정되었던 임시 최적화 캐시, 스레드 선호도 설정 및 예약 스케줄 변수를 즉시 무효화(Invalidate)한다.
2. **Live Safepoint 선행 백업**:
   - 새로운 하드웨어 스펙에 맞추어 레지스트리, 전원 옵션, 가상 메모리 설정을 재조율하기 전에, **반드시 직전 시스템 상태를 Live Safepoint로 자동 캡처**하여 1-Click 복원을 보장한다.
3. **관리자 승인 절차**:
   - 대화형 GUI 환경에서는 하드웨어 변경 검증 모달을 띄워 관리자가 변경 내역을 확인하고 승인하도록 한다.
   - 무인 엔터프라이즈 CLI 환경에서는 `/AcceptHardwareDrift` 스위치가 제공된 경우에 한해 무음으로 신규 기준치를 승인 및 갱신한다.

---

## 📌 제5장 감사 로그 및 텔레메트리 리포팅 규정

1. **로컬 감사 로그**:
   - 모든 하드웨어 검증 판정 결과는 `%ProgramData%\WinPurifyPro\Logs\HardwareVerification.log` 파일에 영구 기록된다.
   - 로그 포맷 예시:
     ```
     [2026-10-09 14:32:01] [HW-VERIFY] Hardware drift detected!
       - Baseline: Intel Core i7-13700K (16C/24T), 16GB DDR5, SATA SSD (500GB)
       - Current:  Intel Core i7-13700K (16C/24T), 32GB DDR5 (+16GB Expansion), Samsung 990 PRO NVMe (2TB)
       - Actions:  Invalidated obsolete TRIM/WorkingSet cache. Auto-created Live Safepoint #104.
       - Result:   Baseline renewed successfully. All 144 modules re-calibrated.
     ```
2. **Central Commander 원격 관제 패킷 연동**:
   - 다중 PC 환경에서 하드웨어 스펙이 변경된 노드는 센트럴 커맨더로 `HardwareDriftAuditPacket`을 암호화(HMAC-SHA256) 전송하여 사내 자산 관리팀에 실시간 알림을 전달한다.

---

## 📌 부칙 (Addendum)

1. **시행일**: 본 규칙 선언서는 2026년 10월 09일 배포되는 WinPurify Pro `v4.55.0`부터 즉시 효력을 발생한다.
2. **상위성**: 본 규칙 선언서는 WinPurify Pro의 모든 최적화 모듈, 스케줄러, CLI 및 원격 관제 엔진의 동작에 최우선적으로 적용된다.
