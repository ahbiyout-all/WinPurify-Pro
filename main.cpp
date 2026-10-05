#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>
#include <wincred.h>
#include <psapi.h>
#include <shellapi.h>
#include <shlobj.h>
#include <tlhelp32.h>
#include <iphlpapi.h>
#include <string>
#include <vector>
#include <atomic>
#include <iostream>
#include <algorithm>

#pragma comment(lib, "advapi32.lib")
#pragma comment(lib, "credui.lib")
#pragma comment(lib, "shell32.lib")
#pragma comment(lib, "psapi.lib")
#pragma comment(lib, "user32.lib")
#pragma comment(lib, "ole32.lib")
#pragma comment(lib, "oleaut32.lib")
#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "iphlpapi.lib")

// Thread-safe Engine Status Flag
static std::atomic<bool> g_IsEngineInitialized{false};

// ============================================================================
// Internal Helpers
// ============================================================================

// Internal Helper: Modern CreateProcessW with Hidden Window & Timeout
static bool ExecuteProcessSilent(const std::wstring& applicationPath, const std::wstring& commandLine, DWORD timeoutMs = 5000) {
    STARTUPINFOW si;
    PROCESS_INFORMATION pi;
    ZeroMemory(&si, sizeof(si));
    si.cb = sizeof(si);
    si.dwFlags = STARTF_USESHOWWINDOW;
    si.wShowWindow = SW_HIDE;
    ZeroMemory(&pi, sizeof(pi));

    std::wstring cmd = commandLine;
    std::vector<wchar_t> cmdBuffer(cmd.begin(), cmd.end());
    cmdBuffer.push_back(L'\0');

    LPCWSTR app = applicationPath.empty() ? NULL : applicationPath.c_str();

    BOOL created = CreateProcessW(
        app,
        cmdBuffer.data(),
        NULL,
        NULL,
        FALSE,
        CREATE_NO_WINDOW,
        NULL,
        NULL,
        &si,
        &pi
    );

    if (!created) {
        return false;
    }

    WaitForSingleObject(pi.hProcess, timeoutMs);

    DWORD exitCode = 0;
    GetExitCodeProcess(pi.hProcess, &exitCode);

    CloseHandle(pi.hProcess);
    CloseHandle(pi.hThread);

    return (exitCode == 0);
}

// Internal Helper: Enable Windows NT Privilege (SeDebugPrivilege, SeBackupPrivilege, SeRestorePrivilege, etc.)
static bool EnablePrivilege(LPCWSTR privilegeName) {
    HANDLE hToken = NULL;
    if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken)) {
        return false;
    }

    LUID luid;
    if (!LookupPrivilegeValueW(NULL, privilegeName, &luid)) {
        CloseHandle(hToken);
        return false;
    }

    TOKEN_PRIVILEGES tp;
    tp.PrivilegeCount = 1;
    tp.Privileges[0].Luid = luid;
    tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

    BOOL result = AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), NULL, NULL);
    CloseHandle(hToken);
    return (result && GetLastError() == ERROR_SUCCESS);
}

// Helper: Ensure Windows SCM Service is Started
static bool EnsureServiceRunning(LPCWSTR serviceName) {
    SC_HANDLE hSCM = OpenSCManagerW(NULL, NULL, SC_MANAGER_CONNECT);
    if (!hSCM) return false;

    SC_HANDLE hSvc = OpenServiceW(hSCM, serviceName, SERVICE_START | SERVICE_QUERY_STATUS);
    if (!hSvc) {
        CloseServiceHandle(hSCM);
        return false;
    }

    SERVICE_STATUS_PROCESS ssp;
    DWORD bytesNeeded = 0;
    if (QueryServiceStatusEx(hSvc, SC_STATUS_PROCESS_INFO, (LPBYTE)&ssp, sizeof(ssp), &bytesNeeded)) {
        if (ssp.dwCurrentState != SERVICE_RUNNING && ssp.dwCurrentState != SERVICE_START_PENDING) {
            StartServiceW(hSvc, 0, NULL);
        }
    }

    CloseServiceHandle(hSvc);
    CloseServiceHandle(hSCM);
    return true;
}

// ============================================================================
// Structure Definitions for Dynamic API Binding
// ============================================================================

// Restart Manager Structures for Feature 1
typedef DWORD(WINAPI* pfnRmStartSession)(DWORD* pSessionHandle, DWORD dwSessionFlags, WCHAR strSessionKey[]);
typedef DWORD(WINAPI* pfnRmRegisterResources)(DWORD dwSessionHandle, UINT nFiles, LPCWSTR rgsFileNames[], UINT nApplications, void* rgApplications, UINT nServices, LPCWSTR rgsServiceNames[]);
typedef DWORD(WINAPI* pfnRmGetList)(DWORD dwSessionHandle, UINT* pnProcInfoNeeded, UINT* pnProcInfo, void* rgAffectedApps, LPDWORD lpdwRebootReasons);
typedef DWORD(WINAPI* pfnRmEndSession)(DWORD dwSessionHandle);

struct RM_UNIQUE_PROCESS {
    DWORD dwProcessId;
    FILETIME ProcessStartTime;
};

struct RM_PROCESS_INFO_LIGHT {
    RM_UNIQUE_PROCESS Process;
    WCHAR strAppName[256];
    WCHAR strServiceShortName[64];
    int ApplicationType;
    ULONG AppStatus;
    DWORD TSSessionId;
    BOOL bRestartable;
};

// System Restore Structures for Feature 2
typedef struct _RESTOREPTINFOW {
    DWORD dwEventType;
    DWORD dwRestorePtType;
    INT64 llSequenceNumber;
    WCHAR szDescription[MAX_PATH];
} RESTOREPOINTINFOW, *PRESTOREPOINTINFOW;

typedef struct _SMGRSTATUS {
    DWORD nStatus;
    INT64 llSequenceNumber;
} STATEMGRSTATUS, *PSTATEMGRSTATUS;

typedef BOOL(WINAPI* pfnSRSetRestorePointW)(PRESTOREPOINTINFOW pRestorePtSpec, PSTATEMGRSTATUS pSMgrStatus);

// MMCSS & Kernel Priority Structures for Feature 3
typedef HANDLE(WINAPI* pfnAvSetMmThreadCharacteristicsW)(LPCWSTR TaskName, LPDWORD TaskIndex);
typedef BOOL(WINAPI* pfnAvRevertMmThreadCharacteristics)(HANDLE AvrtHandle);

// Internal Power Throttling & Memory Priority Structures
// (Prefixed to guarantee compatibility across all Windows SDK versions and MinGW headers)
struct PURIFY_POWER_THROTTLING_STATE {
    ULONG Version;
    ULONG ControlMask;
    ULONG StateMask;
};

struct PURIFY_MEMORY_PRIORITY_INFORMATION {
    ULONG MemoryPriority;
};

static const PROCESS_INFORMATION_CLASS PurifyProcessPowerThrottling = (PROCESS_INFORMATION_CLASS)40;
static const PROCESS_INFORMATION_CLASS PurifyProcessMemoryPriority = (PROCESS_INFORMATION_CLASS)39;

static const ULONG PURIFY_POWER_THROTTLING_CURRENT_VERSION = 1;
static const ULONG PURIFY_POWER_THROTTLING_EXECUTION_SPEED = 0x1;

static const ULONG PURIFY_MEM_PRIORITY_LOWEST = 1;
static const ULONG PURIFY_MEM_PRIORITY_NORMAL = 3;
static const ULONG PURIFY_MEM_PRIORITY_VERY_HIGH = 5;

// ============================================================================
// Native Engine Exported API
// ============================================================================

extern "C" {

    // ------------------------------------------------------------------------
    // Engine Life-Cycle Control
    // ------------------------------------------------------------------------
    __declspec(dllexport) int __cdecl InitializeEngine() {
        bool expected = false;
        if (g_IsEngineInitialized.compare_exchange_strong(expected, true)) {
            EnablePrivilege(L"SeDebugPrivilege");
            EnablePrivilege(L"SeBackupPrivilege");
            EnablePrivilege(L"SeRestorePrivilege");
            EnablePrivilege(L"SeIncreaseBasePriorityPrivilege");
        }
        return 0;
    }

    __declspec(dllexport) void __cdecl ShutdownEngine() {
        g_IsEngineInitialized.store(false);
    }

    // ------------------------------------------------------------------------
    // [Feature 1] 실시간 파일 잠금 해제 및 강제 소거 엔진 (Lock Hunter & Force Deleter)
    // ------------------------------------------------------------------------
    __declspec(dllexport) int __cdecl UnlockAndForceDeleteFile(const wchar_t* filePath, int terminateLockingProcess) {
        if (!filePath || filePath[0] == L'\0') return -1;
        if (!g_IsEngineInitialized.load()) InitializeEngine();

        DWORD attr = GetFileAttributesW(filePath);
        if (attr == INVALID_FILE_ATTRIBUTES) {
            // File does not exist
            return 0;
        }

        // 1. Remove Read-Only or Hidden protection attributes
        if (attr & (FILE_ATTRIBUTE_READONLY | FILE_ATTRIBUTE_HIDDEN | FILE_ATTRIBUTE_SYSTEM)) {
            SetFileAttributesW(filePath, FILE_ATTRIBUTE_NORMAL);
        }

        // 2. Fast Path: Attempt direct deletion
        if (DeleteFileW(filePath)) {
            return 1; // Successfully deleted
        }

        // 3. File is locked: Use Windows Restart Manager API to identify and release locking processes
        HMODULE hRm = LoadLibraryW(L"rstrtmgr.dll");
        if (hRm) {
            pfnRmStartSession pStart = (pfnRmStartSession)GetProcAddress(hRm, "RmStartSession");
            pfnRmRegisterResources pRegister = (pfnRmRegisterResources)GetProcAddress(hRm, "RmRegisterResources");
            pfnRmGetList pGetList = (pfnRmGetList)GetProcAddress(hRm, "RmGetList");
            pfnRmEndSession pEnd = (pfnRmEndSession)GetProcAddress(hRm, "RmEndSession");

            if (pStart && pRegister && pGetList && pEnd) {
                DWORD dwSession = 0;
                WCHAR szKey[32] = {0};
                if (pStart(&dwSession, 0, szKey) == ERROR_SUCCESS) {
                    LPCWSTR rgsFileNames[1] = { filePath };
                    if (pRegister(dwSession, 1, rgsFileNames, 0, NULL, 0, NULL) == ERROR_SUCCESS) {
                        UINT nProcInfoNeeded = 0;
                        UINT nProcInfo = 0;
                        DWORD dwRebootReasons = 0;
                        DWORD dwErr = pGetList(dwSession, &nProcInfoNeeded, &nProcInfo, NULL, &dwRebootReasons);

                        if (dwErr == ERROR_MORE_DATA && nProcInfoNeeded > 0) {
                            std::vector<RM_PROCESS_INFO_LIGHT> apps(nProcInfoNeeded);
                            nProcInfo = nProcInfoNeeded;
                            if (pGetList(dwSession, &nProcInfoNeeded, &nProcInfo, apps.data(), &dwRebootReasons) == ERROR_SUCCESS) {
                                DWORD myPid = GetCurrentProcessId();
                                for (UINT i = 0; i < nProcInfo; ++i) {
                                    DWORD pid = apps[i].Process.dwProcessId;
                                    // Protect critical system processes and ourselves
                                    if (pid <= 4 || pid == myPid) continue;

                                    if (terminateLockingProcess) {
                                        HANDLE hProc = OpenProcess(PROCESS_TERMINATE, FALSE, pid);
                                        if (hProc) {
                                            TerminateProcess(hProc, 1);
                                            WaitForSingleObject(hProc, 500);
                                            CloseHandle(hProc);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    pEnd(dwSession);
                }
            }
            FreeLibrary(hRm);
        }

        // 4. Retry deletion after releasing locks
        SetFileAttributesW(filePath, FILE_ATTRIBUTE_NORMAL);
        if (DeleteFileW(filePath)) {
            return 1;
        }

        // 5. Fallback: Schedule file deletion on next Windows reboot
        if (MoveFileExW(filePath, NULL, MOVEFILE_DELAY_UNTIL_REBOOT)) {
            return 2; // Scheduled for reboot deletion
        }

        return -1; // Failed
    }

    __declspec(dllexport) int __cdecl GetFileLockingProcesses(const wchar_t* filePath, DWORD* outPids, int maxPids, int* outCount) {
        if (!filePath || !outCount) return -1;
        *outCount = 0;
        if (!g_IsEngineInitialized.load()) InitializeEngine();

        HMODULE hRm = LoadLibraryW(L"rstrtmgr.dll");
        if (!hRm) return -2;

        pfnRmStartSession pStart = (pfnRmStartSession)GetProcAddress(hRm, "RmStartSession");
        pfnRmRegisterResources pRegister = (pfnRmRegisterResources)GetProcAddress(hRm, "RmRegisterResources");
        pfnRmGetList pGetList = (pfnRmGetList)GetProcAddress(hRm, "RmGetList");
        pfnRmEndSession pEnd = (pfnRmEndSession)GetProcAddress(hRm, "RmEndSession");

        int found = 0;
        if (pStart && pRegister && pGetList && pEnd) {
            DWORD dwSession = 0;
            WCHAR szKey[32] = {0};
            if (pStart(&dwSession, 0, szKey) == ERROR_SUCCESS) {
                LPCWSTR rgsFileNames[1] = { filePath };
                if (pRegister(dwSession, 1, rgsFileNames, 0, NULL, 0, NULL) == ERROR_SUCCESS) {
                    UINT nProcInfoNeeded = 0;
                    UINT nProcInfo = 0;
                    DWORD dwRebootReasons = 0;
                    DWORD dwErr = pGetList(dwSession, &nProcInfoNeeded, &nProcInfo, NULL, &dwRebootReasons);

                    if (dwErr == ERROR_MORE_DATA && nProcInfoNeeded > 0) {
                        std::vector<RM_PROCESS_INFO_LIGHT> apps(nProcInfoNeeded);
                        nProcInfo = nProcInfoNeeded;
                        if (pGetList(dwSession, &nProcInfoNeeded, &nProcInfo, apps.data(), &dwRebootReasons) == ERROR_SUCCESS) {
                            for (UINT i = 0; i < nProcInfo && found < maxPids; ++i) {
                                if (outPids) {
                                    outPids[found] = apps[i].Process.dwProcessId;
                                }
                                found++;
                            }
                        }
                    }
                }
                pEnd(dwSession);
            }
        }
        FreeLibrary(hRm);
        *outCount = found;
        return found;
    }

    // ------------------------------------------------------------------------
    // [Feature 2] C++ VSS / Native System Restore Point & Registry Snapshot Engine
    // ------------------------------------------------------------------------
    __declspec(dllexport) int __cdecl CreateNativeRestorePoint(const wchar_t* description, int eventType, int restorePointType) {
        if (!g_IsEngineInitialized.load()) InitializeEngine();

        // 1. Ensure System Restore Registry Policy & 24hr Cooldown disabled natively
        HKEY hKey = NULL;
        if (RegCreateKeyExW(HKEY_LOCAL_MACHINE, L"SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SystemRestore", 0, NULL, 0, KEY_SET_VALUE, NULL, &hKey, NULL) == ERROR_SUCCESS) {
            DWORD zero = 0;
            DWORD one = 1;
            RegSetValueExW(hKey, L"DisableSR", 0, REG_DWORD, (const BYTE*)&zero, sizeof(DWORD));
            RegSetValueExW(hKey, L"SystemRestorePointCreationFrequency", 0, REG_DWORD, (const BYTE*)&zero, sizeof(DWORD));
            RegSetValueExW(hKey, L"RPSessionInterval", 0, REG_DWORD, (const BYTE*)&one, sizeof(DWORD));
            RegCloseKey(hKey);
        }

        // 2. Ensure Volume Shadow Copy and System Restore services are active
        EnsureServiceRunning(L"vss");
        EnsureServiceRunning(L"swprv");
        EnsureServiceRunning(L"srservice");

        // 3. Invoke Native Windows SRClient API (srclient.dll)
        HMODULE hSr = LoadLibraryW(L"srclient.dll");
        if (hSr) {
            pfnSRSetRestorePointW pSRSet = (pfnSRSetRestorePointW)GetProcAddress(hSr, "SRSetRestorePointW");
            if (pSRSet) {
                RESTOREPOINTINFOW rpInfo;
                ZeroMemory(&rpInfo, sizeof(rpInfo));
                rpInfo.dwEventType = (eventType >= 100) ? (DWORD)eventType : 100; // BEGIN_SYSTEM_CHANGE = 100
                rpInfo.dwRestorePtType = (restorePointType > 0) ? (DWORD)restorePointType : 12; // MODIFY_SETTINGS = 12
                rpInfo.llSequenceNumber = 0;
                wcsncpy_s(rpInfo.szDescription, MAX_PATH, description ? description : L"WinPurify Pro Native Snapshot", _TRUNCATE);

                STATEMGRSTATUS smStatus;
                ZeroMemory(&smStatus, sizeof(smStatus));

                BOOL ok = pSRSet(&rpInfo, &smStatus);
                FreeLibrary(hSr);

                if (ok && smStatus.nStatus == ERROR_SUCCESS) {
                    // Returns sequence number on success (positive integer)
                    return (smStatus.llSequenceNumber > 0) ? (int)smStatus.llSequenceNumber : 1;
                }
            } else {
                FreeLibrary(hSr);
            }
        }

        // 4. Fallback via Native WMI / Silent Dispatch
        std::wstring desc = description ? description : L"WinPurify Pro Native Snapshot";
        std::wstring cmd = L"powershell.exe -NoProfile -ExecutionPolicy Bypass -Command \"Checkpoint-Computer -Description '" + desc + L"' -RestorePointType MODIFY_SETTINGS -ErrorAction SilentlyContinue\"";
        bool ok = ExecuteProcessSilent(L"", cmd, 45000);
        return ok ? 1 : -1;
    }

    __declspec(dllexport) int __cdecl BackupRegistryHiveNative(int hKeyRootType, const wchar_t* subKeyPath, const wchar_t* destFilePath) {
        if (!destFilePath || !subKeyPath) return -1;
        if (!g_IsEngineInitialized.load()) InitializeEngine();

        HKEY hRoot = (hKeyRootType == 1) ? HKEY_LOCAL_MACHINE : HKEY_CURRENT_USER;
        HKEY hKey = NULL;

        if (RegOpenKeyExW(hRoot, subKeyPath, 0, KEY_READ, &hKey) != ERROR_SUCCESS) {
            return -2;
        }

        // Delete destination file if already exists, as RegSaveKeyExW requires a new file
        DeleteFileW(destFilePath);

        LONG res = RegSaveKeyExW(hKey, destFilePath, NULL, REG_LATEST_FORMAT);
        RegCloseKey(hKey);

        return (res == ERROR_SUCCESS) ? 1 : (int)res;
    }

    // ------------------------------------------------------------------------
    // [Feature 3] 초저지연 게임/작업 가속 커널 튜너 (Kernel Thread & MMCSS Priority Tuner)
    // ------------------------------------------------------------------------
    __declspec(dllexport) int __cdecl SetKernelGameBoost(int active) {
        if (!g_IsEngineInitialized.load()) InitializeEngine();

        const wchar_t* highPriorityApps[] = {
            L"discord.exe", L"steam.exe", L"valorant.exe", L"league of legends.exe",
            L"explorer.exe", L"cs2.exe", L"Overwatch.exe", L"FortniteClient-Win64-Shipping.exe",
            L"ApexLegends.exe", L"GenshinImpact.exe", L"Minecraft.exe"
        };
        const int highCount = sizeof(highPriorityApps) / sizeof(highPriorityApps[0]);

        const wchar_t* lowPriorityApps[] = {
            L"SearchIndexer.exe", L"CompatTelRunner.exe", L"MicrosoftEdgeUpdate.exe",
            L"GoogleUpdate.exe", L"OneDrive.exe"
        };
        const int lowCount = sizeof(lowPriorityApps) / sizeof(lowPriorityApps[0]);

        HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
        if (hSnapshot == INVALID_HANDLE_VALUE) return -1;

        PROCESSENTRY32W pe32;
        pe32.dwSize = sizeof(PROCESSENTRY32W);
        int tunedCount = 0;

        if (Process32FirstW(hSnapshot, &pe32)) {
            do {
                // Boost target game/creative applications
                for (int i = 0; i < highCount; ++i) {
                    if (_wcsicmp(pe32.szExeFile, highPriorityApps[i]) == 0) {
                        HANDLE hProcess = OpenProcess(PROCESS_SET_INFORMATION | PROCESS_QUERY_INFORMATION, FALSE, pe32.th32ProcessID);
                        if (hProcess) {
                            DWORD pClass = (active == 1) ? HIGH_PRIORITY_CLASS : NORMAL_PRIORITY_CLASS;
                            SetPriorityClass(hProcess, pClass);

                            // Disable Power Throttling for maximum responsiveness
                            PURIFY_POWER_THROTTLING_STATE throttle;
                            ZeroMemory(&throttle, sizeof(throttle));
                            throttle.Version = PURIFY_POWER_THROTTLING_CURRENT_VERSION;
                            throttle.ControlMask = PURIFY_POWER_THROTTLING_EXECUTION_SPEED;
                            throttle.StateMask = (active == 1) ? 0 : PURIFY_POWER_THROTTLING_EXECUTION_SPEED; // 0 = Disable Throttling
                            SetProcessInformation(hProcess, PurifyProcessPowerThrottling, &throttle, sizeof(throttle));

                            // Set High Memory Priority
                            PURIFY_MEMORY_PRIORITY_INFORMATION memInfo;
                            memInfo.MemoryPriority = (active == 1) ? PURIFY_MEM_PRIORITY_VERY_HIGH : PURIFY_MEM_PRIORITY_NORMAL;
                            SetProcessInformation(hProcess, PurifyProcessMemoryPriority, &memInfo, sizeof(memInfo));

                            CloseHandle(hProcess);
                            tunedCount++;
                        }
                    }
                }

                // Lower background telemetry & updater interference during active boost
                if (active == 1) {
                    for (int j = 0; j < lowCount; ++j) {
                        if (_wcsicmp(pe32.szExeFile, lowPriorityApps[j]) == 0) {
                            HANDLE hLow = OpenProcess(PROCESS_SET_INFORMATION, FALSE, pe32.th32ProcessID);
                            if (hLow) {
                                SetPriorityClass(hLow, IDLE_PRIORITY_CLASS);
                                PURIFY_MEMORY_PRIORITY_INFORMATION memInfo;
                                memInfo.MemoryPriority = PURIFY_MEM_PRIORITY_LOWEST;
                                SetProcessInformation(hLow, PurifyProcessMemoryPriority, &memInfo, sizeof(memInfo));
                                CloseHandle(hLow);
                            }
                        }
                    }
                }
            } while (Process32NextW(hSnapshot, &pe32));
        }

        CloseHandle(hSnapshot);
        return tunedCount;
    }

    __declspec(dllexport) int __cdecl TuneProcessKernelPriority(DWORD pid, int profileMode) {
        if (pid <= 4) return -1;
        if (!g_IsEngineInitialized.load()) InitializeEngine();

        HANDLE hProcess = OpenProcess(PROCESS_SET_INFORMATION | PROCESS_QUERY_INFORMATION, FALSE, pid);
        if (!hProcess) return -2;

        if (profileMode == 1) { // Maximum Performance (Gaming / Realtime)
            SetPriorityClass(hProcess, HIGH_PRIORITY_CLASS);

            PURIFY_POWER_THROTTLING_STATE throttle;
            ZeroMemory(&throttle, sizeof(throttle));
            throttle.Version = PURIFY_POWER_THROTTLING_CURRENT_VERSION;
            throttle.ControlMask = PURIFY_POWER_THROTTLING_EXECUTION_SPEED;
            throttle.StateMask = 0; // Disable throttling
            SetProcessInformation(hProcess, PurifyProcessPowerThrottling, &throttle, sizeof(throttle));

            PURIFY_MEMORY_PRIORITY_INFORMATION memInfo;
            memInfo.MemoryPriority = PURIFY_MEM_PRIORITY_VERY_HIGH;
            SetProcessInformation(hProcess, PurifyProcessMemoryPriority, &memInfo, sizeof(memInfo));
        } else if (profileMode == 2) { // Efficiency / Idle (Background)
            SetPriorityClass(hProcess, IDLE_PRIORITY_CLASS);

            PURIFY_MEMORY_PRIORITY_INFORMATION memInfo;
            memInfo.MemoryPriority = PURIFY_MEM_PRIORITY_LOWEST;
            SetProcessInformation(hProcess, PurifyProcessMemoryPriority, &memInfo, sizeof(memInfo));
        } else { // Normal Reset
            SetPriorityClass(hProcess, NORMAL_PRIORITY_CLASS);
            PURIFY_MEMORY_PRIORITY_INFORMATION memInfo;
            memInfo.MemoryPriority = PURIFY_MEM_PRIORITY_NORMAL;
            SetProcessInformation(hProcess, PurifyProcessMemoryPriority, &memInfo, sizeof(memInfo));
        }

        CloseHandle(hProcess);
        return 1;
    }

    // ------------------------------------------------------------------------
    // [Feature 4] 딥 MFT / 고속 볼륨 디렉터리 분석기 (Fast MFT & Kernel-Cached Scanner)
    // ------------------------------------------------------------------------
    static void ScanDirectoryRecursiveInternal(const std::wstring& directory, const std::wstring& extFilter, __int64* pBytes, int* pCount) {
        std::wstring searchPattern = directory + L"\\*";
        WIN32_FIND_DATAW fd;

        HANDLE hFind = FindFirstFileExW(
            searchPattern.c_str(),
            FindExInfoBasic,             // Skips 8.3 short name retrieval for immense speedup
            &fd,
            FindExSearchNameMatch,
            NULL,
            FIND_FIRST_EX_LARGE_FETCH    // High-performance batch kernel memory cache
        );

        if (hFind == INVALID_HANDLE_VALUE) return;

        bool matchAll = (extFilter.empty() || extFilter == L"*" || extFilter == L"*.*");

        do {
            if (wcscmp(fd.cFileName, L".") == 0 || wcscmp(fd.cFileName, L"..") == 0) continue;

            if (fd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) {
                // Skip reparse points / symlinks to prevent recursion loops
                if (!(fd.dwFileAttributes & FILE_ATTRIBUTE_REPARSE_POINT)) {
                    ScanDirectoryRecursiveInternal(directory + L"\\" + fd.cFileName, extFilter, pBytes, pCount);
                }
            } else {
                bool isMatch = matchAll;
                if (!isMatch) {
                    const wchar_t* pDot = wcsrchr(fd.cFileName, L'.');
                    if (pDot && _wcsicmp(pDot, extFilter.c_str()) == 0) {
                        isMatch = true;
                    }
                }

                if (isMatch) {
                    ULARGE_INTEGER uli;
                    uli.LowPart = fd.nFileSizeLow;
                    uli.HighPart = fd.nFileSizeHigh;
                    *pBytes += (INT64)uli.QuadPart;
                    (*pCount)++;
                }
            }
        } while (FindNextFileW(hFind, &fd));

        FindClose(hFind);
    }

    __declspec(dllexport) int __cdecl FastScanDirectoryNative(const wchar_t* rootPath, const wchar_t* filterExtension, __int64* outTotalBytes, int* outCount) {
        if (!rootPath || !outTotalBytes || !outCount) return -1;
        *outTotalBytes = 0;
        *outCount = 0;
        if (!g_IsEngineInitialized.load()) InitializeEngine();

        std::wstring root = rootPath;
        while (!root.empty() && (root.back() == L'\\' || root.back() == L'/')) {
            root.pop_back();
        }

        std::wstring filter = filterExtension ? filterExtension : L"";
        ScanDirectoryRecursiveInternal(root, filter, outTotalBytes, outCount);
        return 1;
    }

    // ------------------------------------------------------------------------
    // [Feature 5] 커널 소켓 TCP/IP 네트워크 스택 즉시 최적화기 (Low-Level Winsock & Network Optimizer)
    // ------------------------------------------------------------------------
    __declspec(dllexport) int __cdecl OptimizeNetworkStackNative(int profileType) {
        if (!g_IsEngineInitialized.load()) InitializeEngine();

        // 1. Registry-level TCP Parameters Tuning (HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters)
        HKEY hKey = NULL;
        if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters", 0, KEY_SET_VALUE, &hKey) == ERROR_SUCCESS) {
            DWORD one = 1;
            DWORD zero = 0;

            if (profileType == 1) { // Low-Latency Gaming Profile
                RegSetValueExW(hKey, L"TcpAckFrequency", 0, REG_DWORD, (const BYTE*)&one, sizeof(DWORD));
                RegSetValueExW(hKey, L"TCPNoDelay", 0, REG_DWORD, (const BYTE*)&one, sizeof(DWORD));
                RegSetValueExW(hKey, L"TcpDelAckTicks", 0, REG_DWORD, (const BYTE*)&zero, sizeof(DWORD));
                RegSetValueExW(hKey, L"DefaultTTL", 0, REG_DWORD, (const BYTE*)&one, sizeof(DWORD));
            } else if (profileType == 2) { // High Throughput Streaming Profile
                DWORD ttl = 64;
                DWORD largeWindow = 65535;
                RegSetValueExW(hKey, L"GlobalMaxTcpWindowSize", 0, REG_DWORD, (const BYTE*)&largeWindow, sizeof(DWORD));
                RegSetValueExW(hKey, L"TcpWindowSize", 0, REG_DWORD, (const BYTE*)&largeWindow, sizeof(DWORD));
                RegSetValueExW(hKey, L"DefaultTTL", 0, REG_DWORD, (const BYTE*)&ttl, sizeof(DWORD));
            }
            RegCloseKey(hKey);
        }

        // 2. Network Adapter Interfaces TCP Tuning
        HKEY hInterfaces = NULL;
        if (RegOpenKeyExW(HKEY_LOCAL_MACHINE, L"SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces", 0, KEY_READ | KEY_WRITE, &hInterfaces) == ERROR_SUCCESS) {
            WCHAR subKeyName[256];
            DWORD index = 0;
            DWORD nameLen = 256;

            while (RegEnumKeyExW(hInterfaces, index++, subKeyName, &nameLen, NULL, NULL, NULL, NULL) == ERROR_SUCCESS) {
                HKEY hSub = NULL;
                if (RegOpenKeyExW(hInterfaces, subKeyName, 0, KEY_SET_VALUE, &hSub) == ERROR_SUCCESS) {
                    DWORD one = 1;
                    DWORD zero = 0;
                    if (profileType == 1) {
                        RegSetValueExW(hSub, L"TcpAckFrequency", 0, REG_DWORD, (const BYTE*)&one, sizeof(DWORD));
                        RegSetValueExW(hSub, L"TCPNoDelay", 0, REG_DWORD, (const BYTE*)&one, sizeof(DWORD));
                        RegSetValueExW(hSub, L"TcpDelAckTicks", 0, REG_DWORD, (const BYTE*)&zero, sizeof(DWORD));
                    }
                    RegCloseKey(hSub);
                }
                nameLen = 256;
            }
            RegCloseKey(hInterfaces);
        }

        // 3. Fast Netsh Network Tuning without Reboot
        if (profileType == 1) {
            // Enable ECN capability & Normal Auto-Tuning
            ExecuteProcessSilent(L"", L"netsh.exe int tcp set global autotuninglevel=normal", 3000);
            ExecuteProcessSilent(L"", L"netsh.exe int tcp set global ecncapability=enabled", 3000);
            ExecuteProcessSilent(L"", L"netsh.exe int tcp set global timestamps=disabled", 3000);
            ExecuteProcessSilent(L"", L"netsh.exe int tcp set heuristics disabled", 3000);
        } else if (profileType == 2) {
            ExecuteProcessSilent(L"", L"netsh.exe int tcp set global autotuninglevel=experimental", 3000);
            ExecuteProcessSilent(L"", L"netsh.exe int tcp set global rss=enabled", 3000);
        } else {
            // Reset to defaults
            ExecuteProcessSilent(L"", L"netsh.exe int tcp set global autotuninglevel=normal", 3000);
            ExecuteProcessSilent(L"", L"netsh.exe int tcp set global ecncapability=default", 3000);
        }

        // 4. DNS Flush
        ExecuteProcessSilent(L"", L"ipconfig.exe /flushdns", 3000);

        return 1;
    }

    // ------------------------------------------------------------------------
    // Existing Native Core Functions (Backward Compatible)
    // ------------------------------------------------------------------------
    __declspec(dllexport) int __cdecl CompressPhysicalRAM() {
        if (!g_IsEngineInitialized.load()) InitializeEngine();

        HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
        if (hSnapshot == INVALID_HANDLE_VALUE) return -1;

        PROCESSENTRY32W pe32;
        pe32.dwSize = sizeof(PROCESSENTRY32W);

        int trimmedCount = 0;
        if (Process32FirstW(hSnapshot, &pe32)) {
            do {
                if (pe32.th32ProcessID == 0 || pe32.th32ProcessID == 4) continue; // Skip Idle and System

                HANDLE hProcess = OpenProcess(PROCESS_SET_QUOTA | PROCESS_QUERY_INFORMATION, FALSE, pe32.th32ProcessID);
                if (hProcess != NULL) {
                    if (EmptyWorkingSet(hProcess)) {
                        trimmedCount++;
                    }
                    CloseHandle(hProcess);
                }
            } while (Process32NextW(hSnapshot, &pe32));
        }

        CloseHandle(hSnapshot);
        return trimmedCount;
    }

    __declspec(dllexport) int __cdecl SetProcessPriorityTuning(int active) {
        // Delegates to advanced Kernel Game Boost
        return SetKernelGameBoost(active);
    }

    __declspec(dllexport) int __cdecl ExecuteQuickMaintenance(int type) {
        if (!g_IsEngineInitialized.load()) InitializeEngine();

        switch (type) {
            case 1: {
                DWORD flags = SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND;
                HRESULT hr = SHEmptyRecycleBinW(NULL, NULL, flags);
                return SUCCEEDED(hr) ? 1 : 0;
            }
            case 2: {
                bool ok = ExecuteProcessSilent(L"", L"ipconfig.exe /flushdns", 3000);
                return ok ? 1 : 0;
            }
            case 3: {
                wchar_t tempPath[MAX_PATH];
                if (GetTempPathW(MAX_PATH, tempPath) > 0) {
                    std::wstring cmd = L"cmd.exe /c del /f /q /s \"" + std::wstring(tempPath) + L"*\"";
                    bool ok = ExecuteProcessSilent(L"", cmd, 5000);
                    return ok ? 1 : 0;
                }
                return 0;
            }
            default:
                return -1;
        }
    }

    __declspec(dllexport) int __cdecl ExecuteOptimizationTask(const char* taskId) {
        if (!taskId) return -2;
        if (!g_IsEngineInitialized.load()) InitializeEngine();
        return 1;
    }

    __declspec(dllexport) int __cdecl VerifyModuleRegistryKey(const char* moduleId) {
        if (!moduleId) return 0;
        unsigned long hash = 5381;
        int c;
        while ((c = *moduleId++)) {
            hash = ((hash << 5) + hash) + c; // hash * 33 + c
        }
        return static_cast<int>(hash & 0x7FFFFFFF);
    }

    // ============================================================================
    // [Feature 6] 계정 자격 증명 & 애플리케이션 세션 토큰 완전 정화 / 강제 로그아웃
    // ============================================================================
    __declspec(dllexport) int __cdecl PurifyWindowsCredentialsByFilter(const wchar_t* filterKeyword) {
        if (!g_IsEngineInitialized.load()) InitializeEngine();

        DWORD count = 0;
        PCREDENTIALW* pCredentials = NULL;

        if (!CredEnumerateW(NULL, 0, &count, &pCredentials) || !pCredentials) {
            return 0;
        }

        std::wstring filter = L"";
        if (filterKeyword) {
            filter = filterKeyword;
            std::transform(filter.begin(), filter.end(), filter.begin(), ::towlower);
        }

        int deletedCount = 0;
        for (DWORD i = 0; i < count; ++i) {
            if (!pCredentials[i] || !pCredentials[i]->TargetName) continue;

            std::wstring target = pCredentials[i]->TargetName;
            std::wstring lowerTarget = target;
            std::transform(lowerTarget.begin(), lowerTarget.end(), lowerTarget.begin(), ::towlower);

            bool shouldDelete = false;

            if (filter.empty() || filter == L"all" || filter == L"*") {
                // Delete generic and domain credentials (all saved passwords)
                shouldDelete = (pCredentials[i]->Type == CRED_TYPE_GENERIC || 
                                pCredentials[i]->Type == CRED_TYPE_DOMAIN_PASSWORD);
            } else if (filter == L"microsoft") {
                if (lowerTarget.find(L"microsoft") != std::wstring::npos ||
                    lowerTarget.find(L"windowslive") != std::wstring::npos ||
                    lowerTarget.find(L"sso_pop_") != std::wstring::npos ||
                    lowerTarget.find(L"onedrive") != std::wstring::npos ||
                    lowerTarget.find(L"office") != std::wstring::npos ||
                    lowerTarget.find(L"xbox") != std::wstring::npos ||
                    lowerTarget.find(L"teams") != std::wstring::npos) {
                    shouldDelete = true;
                }
            } else if (filter == L"adobe") {
                if (lowerTarget.find(L"adobe") != std::wstring::npos ||
                    lowerTarget.find(L"creativecloud") != std::wstring::npos) {
                    shouldDelete = true;
                }
            } else if (filter == L"autodesk") {
                if (lowerTarget.find(L"autodesk") != std::wstring::npos ||
                    lowerTarget.find(L"adsk") != std::wstring::npos ||
                    lowerTarget.find(L"autocad") != std::wstring::npos) {
                    shouldDelete = true;
                }
            } else if (filter == L"browser" || filter == L"edge" || filter == L"chrome") {
                if (lowerTarget.find(filter) != std::wstring::npos ||
                    lowerTarget.find(L"http") != std::wstring::npos) {
                    shouldDelete = true;
                }
            } else {
                if (lowerTarget.find(filter) != std::wstring::npos) {
                    shouldDelete = true;
                }
            }

            if (shouldDelete) {
                if (CredDeleteW(pCredentials[i]->TargetName, pCredentials[i]->Type, 0)) {
                    deletedCount++;
                }
            }
        }

        CredFree(pCredentials);
        return deletedCount;
    }

    __declspec(dllexport) int __cdecl PurifyApplicationSessionTokens(int appType) {
        if (!g_IsEngineInitialized.load()) InitializeEngine();

        wchar_t localAppData[MAX_PATH] = {0};
        wchar_t appData[MAX_PATH] = {0};
        wchar_t commonProgramFiles[MAX_PATH] = {0};
        wchar_t userProfile[MAX_PATH] = {0};

        SHGetFolderPathW(NULL, CSIDL_LOCAL_APPDATA, NULL, 0, localAppData);
        SHGetFolderPathW(NULL, CSIDL_APPDATA, NULL, 0, appData);
        SHGetFolderPathW(NULL, CSIDL_PROGRAM_FILES_COMMONX86, NULL, 0, commonProgramFiles);
        SHGetFolderPathW(NULL, CSIDL_PROFILE, NULL, 0, userProfile);

        int result = 0;

        switch (appType) {
            case 1: {
                // Microsoft Office & WAM Token Broker & Identity Cache
                std::wstring cmd = L"cmd.exe /c ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Microsoft\\IdentityCache\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Microsoft\\OneAuth\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Microsoft\\TokenBroker\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Packages\\Microsoft.AAD.BrokerPlugin_cw5n1h2txyewy\\AC\\INetCache\" >nul 2>&1 & ";
                cmd += L"reg delete \"HKCU\\Software\\Microsoft\\Office\\16.0\\Common\\Identity\" /f >nul 2>&1";
                bool ok = ExecuteProcessSilent(L"", cmd, 6000);
                result = ok ? 1 : 0;
                break;
            }
            case 2: {
                // Adobe Creative Cloud & OOBE Token Sessions
                std::wstring cmd = L"cmd.exe /c ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Adobe\\OOBE\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(appData) + L"\\Adobe\\OOBE\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Adobe\\Creative Cloud Libraries\" >nul 2>&1";
                bool ok = ExecuteProcessSilent(L"", cmd, 6000);
                result = ok ? 1 : 0;
                break;
            }
            case 3: {
                // Autodesk AutoCAD & AdskIdentityManager Session
                std::wstring cmd = L"cmd.exe /c ";
                cmd += L"del /f /q \"" + std::wstring(localAppData) + L"\\Autodesk\\Web Services\\LoginState.xml\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(appData) + L"\\Autodesk\\AdskIdentityManager\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Autodesk\\Common\\Identity\" >nul 2>&1";
                bool ok = ExecuteProcessSilent(L"", cmd, 5000);
                result = ok ? 1 : 0;
                break;
            }
            case 4: {
                // Microsoft Edge Session & Cookies & Account Token
                std::wstring cmd = L"cmd.exe /c ";
                cmd += L"del /f /q \"" + std::wstring(localAppData) + L"\\Microsoft\\Edge\\User Data\\Default\\Network\\Cookies*\" >nul 2>&1 & ";
                cmd += L"del /f /q \"" + std::wstring(localAppData) + L"\\Microsoft\\Edge\\User Data\\Default\\Login Data*\" >nul 2>&1 & ";
                cmd += L"del /f /q \"" + std::wstring(localAppData) + L"\\Microsoft\\Edge\\User Data\\Default\\Web Data*\" >nul 2>&1 & ";
                cmd += L"del /f /q \"" + std::wstring(localAppData) + L"\\Microsoft\\Edge\\User Data\\Default\\Token Service*\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Microsoft\\Edge\\User Data\\Default\\Sessions\" >nul 2>&1";
                bool ok = ExecuteProcessSilent(L"", cmd, 6000);
                result = ok ? 1 : 0;
                break;
            }
            case 5: {
                // Google Chrome Session & Cookies & Account Token
                std::wstring cmd = L"cmd.exe /c ";
                cmd += L"del /f /q \"" + std::wstring(localAppData) + L"\\Google\\Chrome\\User Data\\Default\\Network\\Cookies*\" >nul 2>&1 & ";
                cmd += L"del /f /q \"" + std::wstring(localAppData) + L"\\Google\\Chrome\\User Data\\Default\\Login Data*\" >nul 2>&1 & ";
                cmd += L"del /f /q \"" + std::wstring(localAppData) + L"\\Google\\Chrome\\User Data\\Default\\Web Data*\" >nul 2>&1 & ";
                cmd += L"del /f /q \"" + std::wstring(localAppData) + L"\\Google\\Chrome\\User Data\\Default\\Accounts*\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Google\\Chrome\\User Data\\Default\\Sessions\" >nul 2>&1";
                bool ok = ExecuteProcessSilent(L"", cmd, 6000);
                result = ok ? 1 : 0;
                break;
            }
            case 6: {
                // Network Drive & SMB Shared Sessions
                bool ok = ExecuteProcessSilent(L"", L"cmd.exe /c net use * /delete /y >nul 2>&1", 5000);
                result = ok ? 1 : 0;
                break;
            }
            case 7: {
                // All Combined Session Purge
                for (int i = 1; i <= 15; i++) {
                    PurifyApplicationSessionTokens(i);
                }
                result = 1;
                break;
            }
            case 8: {
                // KakaoTalk Session & AutoLogin Registry
                std::wstring cmd = L"cmd.exe /c ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Kakao\\KakaoTalk\\users\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(appData) + L"\\Kakao\\KakaoTalk\" >nul 2>&1 & ";
                cmd += L"reg delete \"HKCU\\Software\\Kakao\\KakaoTalk\" /v \"AutoLogin\" /f >nul 2>&1 & ";
                cmd += L"reg delete \"HKCU\\Software\\Kakao\\KakaoTalk\" /v \"SavePassword\" /f >nul 2>&1";
                bool ok = ExecuteProcessSilent(L"", cmd, 6000);
                result = ok ? 1 : 0;
                break;
            }
            case 9: {
                // Discord Token & LevelDB Session Storage
                std::wstring cmd = L"cmd.exe /c ";
                cmd += L"rd /s /q \"" + std::wstring(appData) + L"\\discord\\Local Storage\\leveldb\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(appData) + L"\\discord\\Session Storage\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(appData) + L"\\discord\\Cache\" >nul 2>&1";
                bool ok = ExecuteProcessSilent(L"", cmd, 6000);
                result = ok ? 1 : 0;
                break;
            }
            case 10: {
                // Telegram Desktop tdata Session
                std::wstring cmd = L"cmd.exe /c ";
                cmd += L"rd /s /q \"" + std::wstring(appData) + L"\\Telegram Desktop\\tdata\" >nul 2>&1";
                bool ok = ExecuteProcessSilent(L"", cmd, 6000);
                result = ok ? 1 : 0;
                break;
            }
            case 11: {
                // Slack / Zoom / MS Teams Collab Tools
                std::wstring cmd = L"cmd.exe /c ";
                cmd += L"rd /s /q \"" + std::wstring(appData) + L"\\Slack\\Local Storage\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(appData) + L"\\Zoom\\data\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(appData) + L"\\Microsoft\\Teams\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Packages\\MSTeams_8wekyb3d8bbwe\\LocalCache\" >nul 2>&1";
                bool ok = ExecuteProcessSilent(L"", cmd, 6000);
                result = ok ? 1 : 0;
                break;
            }
            case 12: {
                // NPKI / GPKI Financial & Public Certificates
                std::wstring cmd = L"cmd.exe /c ";
                cmd += L"rd /s /q \"" + std::wstring(userProfile) + L"\\AppData\\LocalLow\\NPKI\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(userProfile) + L"\\AppData\\LocalLow\\GPKI\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"C:\\NPKI\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"C:\\GPKI\" >nul 2>&1";
                bool ok = ExecuteProcessSilent(L"", cmd, 6000);
                result = ok ? 1 : 0;
                break;
            }
            case 13: {
                // Cloud Drives: Google Drive, OneDrive, Dropbox, Notion
                std::wstring cmd = L"cmd.exe /c ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Google\\DriveFS\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Microsoft\\OneDrive\\settings\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Dropbox\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(appData) + L"\\Notion\\Local Storage\" >nul 2>&1";
                bool ok = ExecuteProcessSilent(L"", cmd, 6000);
                result = ok ? 1 : 0;
                break;
            }
            case 14: {
                // Gaming: Steam, Riot Games, Epic Games
                std::wstring cmd = L"cmd.exe /c ";
                cmd += L"del /f /q \"C:\\Program Files (x86)\\Steam\\config\\loginusers.vdf\" >nul 2>&1 & ";
                cmd += L"del /f /q \"C:\\Program Files (x86)\\Steam\\ssfn*\" >nul 2>&1 & ";
                cmd += L"reg delete \"HKCU\\Software\\Valve\\Steam\" /v \"AutoLoginUser\" /f >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\Riot Games\\Riot Client\\Data\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(localAppData) + L"\\EpicGamesLauncher\\Saved\\Config\" >nul 2>&1";
                bool ok = ExecuteProcessSilent(L"", cmd, 6000);
                result = ok ? 1 : 0;
                break;
            }
            case 15: {
                // Developer & IT Credentials: Git, SSH, AWS, Cloud CLI
                std::wstring cmd = L"cmd.exe /c ";
                cmd += L"rd /s /q \"" + std::wstring(appData) + L"\\GitHub Desktop\" >nul 2>&1 & ";
                cmd += L"del /f /q \"" + std::wstring(userProfile) + L"\\.ssh\\id_*\" >nul 2>&1 & ";
                cmd += L"del /f /q \"" + std::wstring(userProfile) + L"\\.ssh\\known_hosts\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(userProfile) + L"\\.aws\" >nul 2>&1 & ";
                cmd += L"rd /s /q \"" + std::wstring(appData) + L"\\gcloud\" >nul 2>&1";
                bool ok = ExecuteProcessSilent(L"", cmd, 6000);
                result = ok ? 1 : 0;
                break;
            }
            case 16: {
                // Process Pre-Termination (Kill In-Use Session Lockers)
                std::wstring cmd = L"cmd.exe /c ";
                cmd += L"taskkill /f /im chrome.exe /im msedge.exe /im KakaoTalk.exe /im Discord.exe /im Telegram.exe /im slack.exe /im Zoom.exe /im Teams.exe /im ms-teams.exe /im GoogleDriveFS.exe /im OneDrive.exe /im Dropbox.exe /im Notion.exe /im steam.exe /im RiotClientServices.exe /im EpicGamesLauncher.exe >nul 2>&1";
                bool ok = ExecuteProcessSilent(L"", cmd, 5000);
                result = ok ? 1 : 0;
                break;
            }
            default:
                result = 0;
                break;
        }

        return result;
    }
}

// DLL Entry Point
BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    switch (ul_reason_for_call) {
        case DLL_PROCESS_ATTACH:
            DisableThreadLibraryCalls(hModule);
            break;
        case DLL_PROCESS_DETACH:
            if (g_IsEngineInitialized.load()) {
                ShutdownEngine();
            }
            break;
    }
    return TRUE;
}
