# Implementation Plan - Startup Sequence & Multi-Instance Architecture Refactoring (v4.63.0)

## Problem Analysis
1. **Global Mutex Termination**: Previously, `App.xaml.cs` and `CommanderApp.xaml.cs` acquired global mutexes (`Global\WinPurifyPro_App_Mutex_Global_6904B914` and `Global\WinPurifyPro_Commander_Mutex_Global_6904B914`). If an instance or commander was already running, subsequent attempts triggered `Application.Shutdown(0)` immediately.
2. **XAML StaticResource Crash in Earlier Builds**: Running older executables (e.g. `WinPurifyPro_v4.62.0.exe`) resulted in `System.Windows.StaticResourceExtension` error because `InverseBoolConverter` was unmapped in that version. Additionally, `WindowDropShadow` was referenced dynamically without fallback.
3. **Tray Icon Overwriting**: Shared static tray `uID` (1001) would overwrite Shell notify icons across concurrent instances.

## Implemented Solutions
1. **Removed Application-Level Mutex Restrictions**:
   - Eliminated `_appMutex` from `App.xaml.cs` and `_commanderMutex` from `CommanderApp.xaml.cs`.
   - Enabled unlimited concurrent instances of WinPurify Pro and Central Commander side-by-side on the same PC.
2. **Unique Process Instance Identification**:
   - Added `AppEnvironment.ProcessInstanceId` generating a distinct process identity (`WPP-{PID}-{GUID_8}`).
   - Bound instance identity to `MainViewModel.ProcessInstanceId` and dynamic window titles/logs.
   - Refactored `SingleInstanceService` to act in informative mode without forcing shutdowns.
3. **Isolated System Tray Notification IDs**:
   - `SystemTrayService` dynamically computes `_trayUid = (uint)(Process.GetCurrentProcess().Id & 0xFFFF)` ensuring each running process maintains its own independent icon in the Windows notification tray.
4. **Guaranteed Window Rendering & Complete XAML Resources**:
   - Explicit `Visibility = Visibility.Visible`, `Activate()`, and a post-initialization watchdog timer ensure windows render reliably on screen.
   - Added `WindowDropShadow` and all converter aliases (`InverseBoolConverter`, `InverseBool`, `InvBool`, `InverseBooleanConverter`) in `App.xaml` and `MainWindow.xaml`.
5. **Full System Version Sync (v4.63.0 / 4.63.0.0)**:
   - Synchronized all 14 project configurations, manifests, installer, scripts, and Blogger docs via `node sync-version.js`.
   - Validated cleanly with `compile_applet` and `lint_applet`.
