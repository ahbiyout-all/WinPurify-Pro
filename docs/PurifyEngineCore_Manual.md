# 🛠️ PurifyEngineCore.dll 사용 설명서 및 기능 명세

> 📖 **심층 기술 문서 안내**:  
> 네이티브 엔진의 윈도우 커널 연동 원리, 세부 시퀀스 다이어그램, NT 권한 모델 및 벤치마크 분석은 **[PurifyEngineCore 동작 원리 및 심층 아키텍처 백서 (PURIFY_ENGINE_CORE_ARCHITECTURE_SPEC.md)](./PURIFY_ENGINE_CORE_ARCHITECTURE_SPEC.md)**에서 상세히 확인하실 수 있습니다.

`PurifyEngineCore.dll`은 **WinPurify Pro**의 핵심 성능 조율, 커널 스케줄링 및 무결성 시스템 복원, 파일 잠금 해제, 고속 I/O를 전담하는 순수 창작 고속 Native C++ 엔진 동적 링크 라이브러리(DLL)입니다. (제작자: AhBiYout)

---

## 1. 개요 (Overview)

`PurifyEngineCore.dll`은 C# 프로토콜과의 완벽한 상호운용성(Interoperability)을 지원하며, `__cdecl` 호출 규약(Calling Convention)과 표준 C-Linkage(`extern "C"`)를 제공합니다.
* **언어 규격**: Modern C++ (C++17) / MSVC 2026, MinGW-w64 GCC 13+, LLVM Clang 지원
* **아키텍처**: x64 최적화 (/O2 /MT /EHsc)
* **특징**: 커널 특권 획득(`SeDebugPrivilege`, `SeBackupPrivilege`, `SeRestorePrivilege`, `SeIncreaseBasePriorityPrivilege`), Windows Restart Manager 연동 잠금 해제, Native VSS/SRClient 복원 지점 생성, MMCSS/PowerThrottling 커널 튜닝, FindFirstFileExW 대용량 캐시 고속 탐색, Winsock 커널 TCP 파라미터 무재부팅 튜닝.

---

## 2. 5대 핵심 순수 창작 Native 엔진 명세

### [엔진 1] ⚡ 실시간 파일 잠금 해제 및 강제 소거 (Lock Hunter & Force Deleter)
프로세스가 점유(Lock)하고 있어 일반적인 파일 삭제가 차단되는 문제를 해결합니다.
* `UnlockAndForceDeleteFile(const wchar_t* filePath, int terminateLockingProcess)`:
  * 읽기 전용/숨김/시스템 특성을 즉각 해제하고 삭제를 시도합니다.
  * 잠금 발생 시 Windows Restart Manager API(`rstrtmgr.dll`)를 동적 호출하여 해당 파일을 점유 중인 프로세스 PID를 식별하고, 요청 시 강제 해제/종료 후 즉시 소거합니다.
  * 실패 시 다음 재부팅 삭제(`MoveFileExW(MOVEFILE_DELAY_UNTIL_REBOOT)`)로 자동 폴백합니다.
* `GetFileLockingProcesses(const wchar_t* filePath, DWORD* outPids, int maxPids, int* outCount)`:
  * 대상 파일을 잠그고 있는 프로세스 PID 목록을 조회합니다.

### [엔진 2] 🛡️ C++ VSS / Native 복원 지점 및 레지스트리 하이브 스냅샷 (Native Restore & Registry Snapshot)
PowerShell 프로세스 오버헤드 없이 순수 C++ 네이티브 레벨에서 윈도우 시스템 복원 지점 및 바이너리 하이브를 생성합니다.
* `CreateNativeRestorePoint(const wchar_t* description, int eventType, int restorePointType)`:
  * 레지스트리 `DisableSR = 0` 및 24시간 빈도 제한 `SystemRestorePointCreationFrequency = 0`을 네이티브로 해제.
  * SCM 서비스를 검사하여 `vss`, `swprv`, `srservice`를 시작.
  * Windows `srclient.dll`의 `SRSetRestorePointW`를 직접 호출하여 단 1초 만에 시스템 복원 지점을 원자적으로 생성.
* `BackupRegistryHiveNative(int hKeyRootType, const wchar_t* subKeyPath, const wchar_t* destFilePath)`:
  * `SeBackupPrivilege` 권한을 활용하여 `RegSaveKeyExW`로 레지스트리 하이브 바이너리를 0.05초 만에 덤프.

### [엔진 3] 🚀 초저지연 게임/작업 가속 커널 튜너 (Kernel MMCSS & Priority Tuner)
단순 프로세스 우선순위 상향을 넘어, 윈도우 커널 스케줄러와 전원 쓰로틀링을 제어합니다.
* `SetKernelGameBoost(int active)`:
  * 주요 게임 및 작업 프로세스에 대해 `HIGH_PRIORITY_CLASS` 적용.
  * Windows 10/11 `ProcessPowerThrottling` 해제 (E-코어 강제 이관 방지 및 클럭 극대화).
  * `ProcessMemoryPriority`를 `MEMORY_PRIORITY_VERY_HIGH`로 승격.
  * 백그라운드 텔레메트리/업데이터 프로세스를 `IDLE_PRIORITY_CLASS` 및 `MEMORY_PRIORITY_LOWEST`로 절전 격리.
* `TuneProcessKernelPriority(DWORD pid, int profileMode)`:
  * 개별 PID에 대해 1(최고 성능/실시간), 2(절전/백그라운드), 0(정상) 커널 튜닝 적용.

### [엔진 4] 🧹 딥 MFT / 커널 캐시 고속 디렉터리 분석기 (Fast Native Scanner)
파일 수십만 개를 탐색할 때 C#의 `DirectoryInfo` 대비 10배 이상 빠른 커널 레벨 파일 탐색을 수행합니다.
* `FastScanDirectoryNative(const wchar_t* rootPath, const wchar_t* filterExtension, __int64* outTotalBytes, int* outCount)`:
  * Win32 `FindFirstFileExW`의 `FindExInfoBasic` (8.3 짧은 파일명 버퍼 할당 생략)과 `FIND_FIRST_EX_LARGE_FETCH` (커널 메모리 대용량 디렉터리 배치 프리페치)를 적용.
  * 재귀 탐색을 네이티브 포인터 속도로 수행하여 바이트 용량과 파일 수를 즉시 집계.

### [엔진 5] 🌐 커널 소켓 TCP/IP 네트워크 스택 즉시 최적화기 (Low-Level Winsock Optimizer)
네트워크 지연율(Ping)을 단축하고 패킷 버퍼링을 제거합니다.
* `OptimizeNetworkStackNative(int profileType)`:
  * 프로필: 1(저지연 게이밍), 2(고대역폭 스트리밍/다운로드), 0(윈도우 기본값 환원).
  * `HKLM\...\Tcpip\Parameters` 및 활성 인터페이스의 `TcpAckFrequency=1`, `TCPNoDelay=1`, `TcpDelAckTicks=0` 즉시 적용.
  * `netsh int tcp set global autotuninglevel=normal`, `ecncapability=enabled` 무재부팅 즉시 튜닝.
  * DNS 리졸버 캐시 일괄 플러시.

---

## 3. 기본 레거시 및 관리 API (Legacy Compatible)

* `InitializeEngine()`: 커널 디버그/백업/복원 특권 획득 및 전역 엔진 초기화.
* `ShutdownEngine()`: 엔진 종료 및 플래그 초기화.
* `CompressPhysicalRAM()`: 전체 프로세스 Working Set 커널 일괄 트림 (`EmptyWorkingSet`).
* `SetProcessPriorityTuning(int active)`: 커널 게이밍 부스트 위임 호출.
* `ExecuteQuickMaintenance(int type)`: 1(휴지통 비우기), 2(DNS 플러시), 3(임시파일 정리).
* `VerifyModuleRegistryKey(const char* moduleId)`: DJB2 변형 무결성 해시 검증.

---

## 4. C# P/Invoke 연동 인터페이스

```csharp
using System.Runtime.InteropServices;

namespace WinPurifyPro.Services
{
    public static class NativeEngineService
    {
        private const string DllName = "PurifyEngineCore.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int InitializeEngine();

        // 1. 파일 잠금 해제 및 강제 소거
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int UnlockAndForceDeleteFile(string filePath, int terminateLockingProcess);

        // 2. Native 복원 지점 생성
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int CreateNativeRestorePoint(string description, int eventType, int restorePointType);

        // 3. 커널 게이밍 부스트
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetKernelGameBoost(int active);

        // 4. 커널 캐시 고속 디렉터리 스캔
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int FastScanDirectoryNative(string rootPath, string filterExtension, out long outTotalBytes, out int outCount);

        // 5. 커널 TCP/IP 소켓 최적화
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int OptimizeNetworkStackNative(int profileType);

        // 고속 물리 RAM 압축
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CompressPhysicalRAM();
    }
}
```

---

## 5. 컴파일 및 빌드 가이드

### MSVC (Developer Command Prompt for VS x64)
```cmd
cl.exe /LD /O2 /MT /EHsc /std:c++17 /utf-8 /DUNICODE /D_UNICODE main.cpp /Fe"PurifyEngineCore.dll" advapi32.lib shell32.lib psapi.lib user32.lib ole32.lib oleaut32.lib ws2_32.lib iphlpapi.lib /link /OUT:"PurifyEngineCore.dll" /NOLOGO
```

### MinGW-w64 GCC
```cmd
g++ -shared -O3 -std=c++17 -DUNICODE -D_UNICODE main.cpp -o PurifyEngineCore.dll -ladvapi32 -lshell32 -lpsapi -luser32 -lole32 -loleaut32 -lws2_32 -liphlpapi -static -static-libgcc -static-libstdc++
```

---
*WinPurify Pro Native Architecture (제작자: AhBiYout) — All Rights Reserved.*
