# Native C++ DLL Performance Optimization Plan (v4.53.0)

## Objectives
1.  **Fast File Scanning**: Implement native junk file scanning using `NtQueryDirectoryFile` to outperform managed `System.IO` API.
2.  **Memory Management**: Implement a custom memory pool allocator within the native C++ DLL to reduce Garbage Collection (GC) pressure in the WPF managed application.
3.  **Refactoring**: Audit existing `main.cpp` for any remaining potential memory leaks or synchronization issues.

## Tasks
1.  **Scan Engine Optimization**: Create `FastScanDirectoryNative` in `main.cpp`.
2.  **Memory Management**: Implement `NativeMemoryPool` in `main.cpp` and export allocation/deallocation methods.
3.  **Managed Integration**: Update `Services/NativeEngineService.cs` to import new native methods and provide managed wrappers.
4.  **Documentation**: Register new native features in `docs/NATIVE_PERFORMANCE_EXTENSIONS_SPEC.md`.

## Validation Plan
1.  **Unit Tests**: Verify `FastScanDirectoryNative` correctly returns file lists and sizes compared to managed IO.
2.  **Performance Benchmark**: Compare memory usage and scan time before and after implementation.
3.  **Stability**: Ensure zero memory leaks over long-running scan sessions.
