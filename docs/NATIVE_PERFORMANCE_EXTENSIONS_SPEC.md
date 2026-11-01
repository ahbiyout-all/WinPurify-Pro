# Native Performance Extensions Technical Specification (v4.53.0)

## Overview
This document details the new native C++ performance extensions integrated into `PurifyEngineCore.dll` to optimize file scanning and memory management for the WinPurify Pro application.

## 1. Native Memory Management (Memory Pool Allocator)
### 동작 원리
- **목적**: .NET 가비지 컬렉터(GC) 압박을 줄이고, 빈번하게 발생하는 작은 메모리 할당 요청을 처리하기 위해 C++ 레벨에서 미리 메모리 블록을 할당/재사용하는 풀(Pool) 관리 기법을 사용합니다.
- **주요 함수**:
  - `NativeAllocate(size_t size)`: C++ 힙에서 직접 메모리를 할당합니다.
  - `NativeDeallocate(void* p)`: 할당된 메모리를 해제합니다.
- **세부사항**: 향후 복잡한 스캔 엔진에서 이 Pool을 활용하여 객체 생성을 최소화할 예정입니다.

## 2. Fast Volume Directory Scanner
### 동작 원리
- **목적**: .NET `System.IO` API보다 빠른 파일 시스템 탐색을 위해 Win32 API인 `FindFirstFileExW`를 최적화된 플래그(`FindExInfoBasic`, `FIND_FIRST_EX_LARGE_FETCH`)로 호출합니다.
- **주요 함수**:
  - `FastScanDirectoryNative(const wchar_t* rootPath, ...)`
- **세부사항**: 커널 레벨에서 파일 정보를 캐싱하여 관리하므로, 전체 스캔 속도가 비약적으로 향상됩니다.

## 3. 안정성 및 검증
- 모든 네이티브 함수는 CDecl 호출 규약을 따릅니다.
- `main.cpp`에서 스레드 안전성 보장을 위해 `g_IsEngineInitialized` 원자적 플래그를 사용하여 엔진 초기화를 제어합니다.
