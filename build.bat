@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion
cd /d "%~dp0"

:: ---------------------------------------------------------------------
:: 0. Detect Application Version from docs/PATCH_NOTES.md or sync-version.js
:: ---------------------------------------------------------------------
set "APP_VERSION=4.41.0"
if exist "sync-version.js" (
    where node.exe >nul 2>&1
    if not errorlevel 1 (
        for /f "delims=" %%v in ('node sync-version.js') do (
            if not "%%v"=="" set "APP_VERSION=4.41.0"
        )
    )
)

if not defined APP_VERSION (
    if exist "docs\PATCH_NOTES.md" (
        for /f "tokens=2 delims=[]" %%a in ('findstr /r /c:"\[v[0-9]" docs\PATCH_NOTES.md') do (
            set "RAW_VER=%%a"
            set "APP_VERSION=4.41.0"
            goto :FOUND_VER
        )
    )
)
:FOUND_VER

title WinPurify Pro v!APP_VERSION! Unified Builder

echo =====================================================================
echo    WinPurify Pro v!APP_VERSION! Unified Build System
echo    Native C++ Core + .NET 8 LTS / .NET 10 WPF Suite
echo =====================================================================
echo.

:: 1. Check for .NET SDK (dotnet.exe)
where dotnet.exe >nul 2>&1
if errorlevel 1 goto :ERROR_NO_DOTNET

:: Detect .NET SDK Version and configure Multi-SDK (.NET 8 LTS & .NET 10 SDK)
set "DETECTED_DOTNET_VER="
set "DOTNET_MAJOR="

dotnet --version > "%TEMP%\winpurify_dotnet_ver.tmp" 2>nul
if exist "%TEMP%\winpurify_dotnet_ver.tmp" (
    set /p DETECTED_DOTNET_VER=<"%TEMP%\winpurify_dotnet_ver.tmp"
    del "%TEMP%\winpurify_dotnet_ver.tmp" >nul 2>&1
)

if defined DETECTED_DOTNET_VER (
    for /f "tokens=1 delims=." %%v in ("!DETECTED_DOTNET_VER!") do (
        set "DOTNET_MAJOR=%%v"
    )
)

:: Check CLI flags for TargetFramework override & Dual Sequential Build
set "TARGET_FRAMEWORK=net8.0-windows"
set "CLI_TFM_SET=0"
set "CLI_DUAL=0"
set "IS_SEQUENTIAL_DUAL=0"
set "DUAL_STAGE=0"

:ARG_PARSE_LOOP
if "%~1"=="" goto :ARG_PARSE_DONE
if /i "%~1"=="--net10" set "TARGET_FRAMEWORK=net10.0-windows" & set "CLI_TFM_SET=1"
if /i "%~1"=="-net10"  set "TARGET_FRAMEWORK=net10.0-windows" & set "CLI_TFM_SET=1"
if /i "%~1"=="/net10"  set "TARGET_FRAMEWORK=net10.0-windows" & set "CLI_TFM_SET=1"
if /i "%~1"=="net10"   set "TARGET_FRAMEWORK=net10.0-windows" & set "CLI_TFM_SET=1"
if /i "%~1"=="--net8"  set "TARGET_FRAMEWORK=net8.0-windows"  & set "CLI_TFM_SET=1"
if /i "%~1"=="-net8"   set "TARGET_FRAMEWORK=net8.0-windows"  & set "CLI_TFM_SET=1"
if /i "%~1"=="/net8"   set "TARGET_FRAMEWORK=net8.0-windows"  & set "CLI_TFM_SET=1"
if /i "%~1"=="net8"    set "TARGET_FRAMEWORK=net8.0-windows"  & set "CLI_TFM_SET=1"
if /i "%~1"=="--dual"       set "CLI_DUAL=1"
if /i "%~1"=="-dual"        set "CLI_DUAL=1"
if /i "%~1"=="dual"         set "CLI_DUAL=1"
if /i "%~1"=="sequential"   set "CLI_DUAL=1"
if /i "%~1"=="both"         set "CLI_DUAL=1"
if /i "%~1"=="multi"        set "CLI_DUAL=1"
if /i "%~1"=="d"            set "CLI_DUAL=1"
shift
goto :ARG_PARSE_LOOP

:ARG_PARSE_DONE
call :SET_TFM_VARS

if "%CLI_DUAL%"=="1" (
    echo [*] Sequential Dual-TFM Build mode activated via CLI.
) else if "%CLI_TFM_SET%"=="1" (
    echo [*] Target Framework explicitly set via CLI: !TARGET_FRAMEWORK!
) else (
    if "!DOTNET_MAJOR!"=="10" (
        echo [*] Detected .NET 10 SDK environment ^(!DETECTED_DOTNET_VER!^). Multi-TFM enabled.
    ) else if "!DOTNET_MAJOR!"=="8" (
        echo [*] Detected .NET 8 SDK environment ^(!DETECTED_DOTNET_VER!^).
    ) else (
        echo [*] Detected .NET SDK environment ^(!DETECTED_DOTNET_VER!^).
    )
)

:: 2. Check for pre-existing C++ Native DLL
if exist "PurifyEngineCore.dll" echo [*] Found existing PurifyEngineCore.dll.

:: 3. Check if MSVC compiler (cl.exe) is in PATH
where cl.exe >nul 2>&1
if not errorlevel 1 goto :DO_COMPILE_MSVC

:: 4. Locate Visual Studio vswhere.exe
echo [*] Searching for MSVC C++ compiler environment...
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" set "VSWHERE=%ProgramFiles%\Microsoft Visual Studio\Installer\vswhere.exe"

if exist "%VSWHERE%" (
    set "VS_PATH="
    "%VSWHERE%" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath > "%TEMP%\winpurify_vswhere.tmp" 2>nul
    if exist "%TEMP%\winpurify_vswhere.tmp" (
        set /p VS_PATH=<"%TEMP%\winpurify_vswhere.tmp"
        del "%TEMP%\winpurify_vswhere.tmp" >nul 2>&1
    )
    if defined VS_PATH (
        if exist "!VS_PATH!\VC\Auxiliary\Build\vcvars64.bat" (
            call "!VS_PATH!\VC\Auxiliary\Build\vcvars64.bat" >nul 2>&1
            cd /d "%~dp0"
            where cl.exe >nul 2>&1
            if not errorlevel 1 goto :DO_COMPILE_MSVC
        )
    )
)

:: 5. Search standard Visual Studio paths
for %%Y in (2022 2019 2017) do (
    for %%E in (Community Professional Enterprise BuildTools Preview) do (
        if exist "C:\Program Files\Microsoft Visual Studio\%%Y\%%E\VC\Auxiliary\Build\vcvars64.bat" (
            call "C:\Program Files\Microsoft Visual Studio\%%Y\%%E\VC\Auxiliary\Build\vcvars64.bat" >nul 2>&1
            cd /d "%~dp0"
            where cl.exe >nul 2>&1
            if not errorlevel 1 goto :DO_COMPILE_MSVC
        )
        if exist "C:\Program Files (x86)\Microsoft Visual Studio\%%Y\%%E\VC\Auxiliary\Build\vcvars64.bat" (
            call "C:\Program Files (x86)\Microsoft Visual Studio\%%Y\%%E\VC\Auxiliary\Build\vcvars64.bat" >nul 2>&1
            cd /d "%~dp0"
            where cl.exe >nul 2>&1
            if not errorlevel 1 goto :DO_COMPILE_MSVC
        )
        if exist "D:\Program Files\Microsoft Visual Studio\%%Y\%%E\VC\Auxiliary\Build\vcvars64.bat" (
            call "D:\Program Files\Microsoft Visual Studio\%%Y\%%E\VC\Auxiliary\Build\vcvars64.bat" >nul 2>&1
            cd /d "%~dp0"
            where cl.exe >nul 2>&1
            if not errorlevel 1 goto :DO_COMPILE_MSVC
        )
        if exist "D:\Program Files (x86)\Microsoft Visual Studio\%%Y\%%E\VC\Auxiliary\Build\vcvars64.bat" (
            call "D:\Program Files (x86)\Microsoft Visual Studio\%%Y\%%E\VC\Auxiliary\Build\vcvars64.bat" >nul 2>&1
            cd /d "%~dp0"
            where cl.exe >nul 2>&1
            if not errorlevel 1 goto :DO_COMPILE_MSVC
        )
    )
)

:: 6. Check for MinGW GCC (g++.exe) or Clang (clang++.exe)
where g++.exe >nul 2>&1
if not errorlevel 1 goto :DO_COMPILE_GCC

where clang++.exe >nul 2>&1
if not errorlevel 1 goto :DO_COMPILE_CLANG

echo [INFO] C++ compiler not detected. Native DLL compilation skipped (Using C# built-in engine).
goto :SELECT_BUILD_PROFILE

:DO_COMPILE_MSVC
if not exist "main.cpp" goto :SELECT_BUILD_PROFILE
echo [+] Compiling main.cpp with MSVC (x64 Release /O2 /MT)...
if exist "version.rc" rc.exe /nologo version.rc >nul 2>&1
if exist "version.res" (
    cl.exe /LD /O2 /MT /EHsc /std:c++17 /utf-8 /DUNICODE /D_UNICODE main.cpp version.res /Fe"PurifyEngineCore.dll" advapi32.lib shell32.lib psapi.lib user32.lib ole32.lib oleaut32.lib ws2_32.lib iphlpapi.lib /link /OUT:"PurifyEngineCore.dll" /NOLOGO
    del /f /q "version.res" >nul 2>&1
) else (
    cl.exe /LD /O2 /MT /EHsc /std:c++17 /utf-8 /DUNICODE /D_UNICODE main.cpp /Fe"PurifyEngineCore.dll" advapi32.lib shell32.lib psapi.lib user32.lib ole32.lib oleaut32.lib ws2_32.lib iphlpapi.lib /link /OUT:"PurifyEngineCore.dll" /NOLOGO
)
if exist "main.obj" del /f /q "main.obj" >nul 2>&1
if exist "main.exp" del /f /q "main.exp" >nul 2>&1
if exist "main.lib" del /f /q "main.lib" >nul 2>&1
if exist "PurifyEngineCore.exp" del /f /q "PurifyEngineCore.exp" >nul 2>&1
if exist "PurifyEngineCore.lib" del /f /q "PurifyEngineCore.lib" >nul 2>&1
if exist "PurifyEngineCore.dll" (
    echo [SUCCESS] PurifyEngineCore.dll compiled successfully!
) else (
    echo [WARNING] MSVC compilation did not produce PurifyEngineCore.dll.
)
goto :SELECT_BUILD_PROFILE

:DO_COMPILE_GCC
if not exist "main.cpp" goto :SELECT_BUILD_PROFILE
echo [+] Compiling main.cpp with MinGW GCC (x64 Release -O3)...
if exist "version.rc" windres version.rc -O coff -o version.res >nul 2>&1
if exist "version.res" (
    g++ -shared -O3 -std=c++17 -DUNICODE -D_UNICODE main.cpp version.res -o PurifyEngineCore.dll -ladvapi32 -lshell32 -lpsapi -luser32 -lole32 -loleaut32 -lws2_32 -liphlpapi -static -static-libgcc -static-libstdc++
    del /f /q "version.res" >nul 2>&1
) else (
    g++ -shared -O3 -std=c++17 -DUNICODE -D_UNICODE main.cpp -o PurifyEngineCore.dll -ladvapi32 -lshell32 -lpsapi -luser32 -lole32 -loleaut32 -lws2_32 -liphlpapi -static -static-libgcc -static-libstdc++
)
if exist "PurifyEngineCore.dll" (
    echo [SUCCESS] PurifyEngineCore.dll compiled successfully with MinGW!
) else (
    echo [WARNING] MinGW compilation did not produce PurifyEngineCore.dll.
)
goto :SELECT_BUILD_PROFILE

:DO_COMPILE_CLANG
if not exist "main.cpp" goto :SELECT_BUILD_PROFILE
echo [+] Compiling main.cpp with LLVM Clang (x64 Release -O3)...
clang++ -shared -O3 -std=c++17 -DUNICODE -D_UNICODE main.cpp -o PurifyEngineCore.dll -ladvapi32 -lshell32 -lpsapi -luser32 -lole32 -loleaut32 -lws2_32 -liphlpapi
if exist "PurifyEngineCore.dll" (
    echo [SUCCESS] PurifyEngineCore.dll compiled successfully with Clang!
) else (
    echo [WARNING] Clang compilation did not produce PurifyEngineCore.dll.
)
goto :SELECT_BUILD_PROFILE

:SELECT_BUILD_PROFILE
if not exist "WinPurifyPro.csproj" goto :ERROR_NO_CSPROJ
call :SET_TFM_VARS

:: Check CLI Arguments
if "%CLI_DUAL%"=="1" goto :BUILD_DUAL_SEQUENTIAL
if /i "%~1"=="dual" goto :BUILD_DUAL_SEQUENTIAL
if /i "%~1"=="sequential" goto :BUILD_DUAL_SEQUENTIAL
if /i "%~1"=="both" goto :BUILD_DUAL_SEQUENTIAL
if /i "%~1"=="multi" goto :BUILD_DUAL_SEQUENTIAL
if /i "%~1"=="d" goto :BUILD_DUAL_SEQUENTIAL
if "%~1"=="0" goto :BUILD_DUAL_SEQUENTIAL

if /i "%~1"=="single" goto :BUILD_SINGLE_FILE
if /i "%~1"=="singlefile" goto :BUILD_SINGLE_FILE
if /i "%~1"=="installer" goto :BUILD_INSTALLER_ONLY
if /i "%~1"=="setup" goto :BUILD_INSTALLER_ONLY
if /i "%~1"=="unpacked" goto :BUILD_UNPACKED_ONLY
if /i "%~1"=="standalone" goto :BUILD_STANDALONE
if /i "%~1"=="standard" goto :BUILD_STANDARD
if /i "%~1"=="commander" goto :BUILD_COMMANDER_ONLY
if /i "%~1"=="commander-fx" goto :BUILD_COMMANDER_FX_ONLY
if /i "%~1"=="commander-framework" goto :BUILD_COMMANDER_FX_ONLY
if /i "%~1"=="commanderfx" goto :BUILD_COMMANDER_FX_ONLY
if /i "%~1"=="all" goto :BUILD_ALL
if "%~1"=="1" goto :BUILD_SINGLE_FILE
if "%~1"=="2" goto :BUILD_INSTALLER_ONLY
if "%~1"=="3" goto :BUILD_STANDALONE
if "%~1"=="4" goto :BUILD_STANDARD
if "%~1"=="5" goto :BUILD_ALL
if "%~1"=="6" goto :BUILD_COMMANDER_ONLY
if "%~1"=="7" goto :BUILD_UNPACKED_ONLY
if "%~1"=="9" goto :BUILD_COMMANDER_FX_ONLY

echo.
echo ---------------------------------------------------------------------
echo    Select Build Profile:
echo    [Current Target Framework: !TARGET_FRAMEWORK! (Output: !PUBLISH_DIR!\)]
echo ---------------------------------------------------------------------
echo  [1] Single-File Portable Publish (Slim single-file exe + Commander)
echo  [2] Inno Setup 6 Installer .exe (Recommended Production Setup)
echo  [3] Standalone Single-File (.NET Runtime Embedded Self-Contained)
echo  [4] Standard Framework-Dependent Build (WinPurifyPro + Commander)
echo  [5] Build All Profiles for current framework (!TARGET_FRAMEWORK!)
echo  [6] Central Commander Standalone Executable (WinPurifyCommander.exe)
echo  [7] Unpacked App Directory Only (Extracted multi-file binaries)
echo  [8] Toggle Target Framework (.NET 8.0 ^<--^> .NET 10.0)
echo  [9] Central Commander Framework-Dependent (~3MB)
echo  [D] Sequential Dual-TFM Build (.NET 10.0 ^& .NET 8.0 side-by-side)
echo ---------------------------------------------------------------------
set /p "TARGET_CHOICE=Enter choice number (Default: 2): "
if "%TARGET_CHOICE%"=="" set "TARGET_CHOICE=2"

if /i "%TARGET_CHOICE%"=="d" goto :BUILD_DUAL_SEQUENTIAL
if "%TARGET_CHOICE%"=="0" goto :BUILD_DUAL_SEQUENTIAL
if "%TARGET_CHOICE%"=="1" goto :BUILD_SINGLE_FILE
if "%TARGET_CHOICE%"=="2" goto :BUILD_INSTALLER_ONLY
if "%TARGET_CHOICE%"=="3" goto :BUILD_STANDALONE
if "%TARGET_CHOICE%"=="4" goto :BUILD_STANDARD
if "%TARGET_CHOICE%"=="5" goto :BUILD_ALL
if "%TARGET_CHOICE%"=="6" goto :BUILD_COMMANDER_ONLY
if "%TARGET_CHOICE%"=="7" goto :BUILD_UNPACKED_ONLY
if "%TARGET_CHOICE%"=="8" goto :TOGGLE_TFM
if "%TARGET_CHOICE%"=="9" goto :BUILD_COMMANDER_FX_ONLY
goto :BUILD_INSTALLER_ONLY

:TOGGLE_TFM
if "!TARGET_FRAMEWORK!"=="net8.0-windows" (
    set "TARGET_FRAMEWORK=net10.0-windows"
    echo [*] Switched Target Framework to: net10.0-windows
) else (
    set "TARGET_FRAMEWORK=net8.0-windows"
    echo [*] Switched Target Framework to: net8.0-windows
)
call :SET_TFM_VARS
goto :SELECT_BUILD_PROFILE

:SET_TFM_VARS
if "!TARGET_FRAMEWORK!"=="net10.0-windows" (
    set "TFM_NAME=net10.0"
    set "PUBLISH_DIR=publish\net10.0"
) else (
    set "TARGET_FRAMEWORK=net8.0-windows"
    set "TFM_NAME=net8.0"
    set "PUBLISH_DIR=publish\net8.0"
)
exit /b 0

:RESTORE_PROJECT_ASSETS
echo [+] Restoring project dependencies for !TARGET_FRAMEWORK!...
dotnet restore WinPurifyPro.csproj -p:TargetFramework=!TARGET_FRAMEWORK! >nul 2>&1
if exist "WinPurifyCommander.csproj" dotnet restore WinPurifyCommander.csproj -p:TargetFramework=!TARGET_FRAMEWORK! >nul 2>&1
exit /b 0

:: ---------------------------------------------------------------------
:: DUAL SEQUENTIAL BUILD (.NET 10 & .NET 8 side-by-side)
:: ---------------------------------------------------------------------
:BUILD_DUAL_SEQUENTIAL
set "IS_SEQUENTIAL_DUAL=1"
set "DUAL_STAGE=1"
echo.
echo =====================================================================
echo [*] Starting Sequential Dual-TFM Build Pipeline
echo  Stage 1: .NET 10.0 (publish\net10.0\)
echo  Stage 2: .NET 8.0 LTS (publish\net8.0\)
echo =====================================================================

set "TARGET_FRAMEWORK=net10.0-windows"
set "TARGET_CHOICE=5"
goto :BUILD_UNPACKED

:DUAL_STAGE_2
set "DUAL_STAGE=2"
echo.
echo =====================================================================
echo [*] Stage 1 (.NET 10) Complete! Starting Stage 2: .NET 8.0 LTS...
echo =====================================================================
set "TARGET_FRAMEWORK=net8.0-windows"
set "TARGET_CHOICE=5"
goto :BUILD_UNPACKED

:DUAL_STAGE_FINISH
echo.
echo =====================================================================
echo  [SUCCESS] Sequential Dual-TFM Build Completed Successfully!
echo  - .NET 10.0 Output: publish\net10.0\
echo  - .NET 8.0 Output:  publish\net8.0\
echo =====================================================================
goto :BUILD_END

:: ---------------------------------------------------------------------
:: BUILD PROFILES
:: ---------------------------------------------------------------------
:BUILD_SINGLE_FILE
call :SET_TFM_VARS
call :RESTORE_PROJECT_ASSETS
echo.
echo =====================================================================
echo [*] Building Single-File Portable Executables for !TFM_NAME!...
echo =====================================================================
if not exist "!PUBLISH_DIR!\SingleFile" md "!PUBLISH_DIR!\SingleFile" >nul 2>&1

echo [+] Publishing Single-File WinPurifyPro.exe...
dotnet publish WinPurifyPro.csproj -c Release -r win-x64 --self-contained false -f !TARGET_FRAMEWORK! -p:TargetFramework=!TARGET_FRAMEWORK! -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "!PUBLISH_DIR!\SingleFile"
if errorlevel 1 goto :ERROR_BUILD_FAILED

:: Versioned output artifact copies
if exist "!PUBLISH_DIR!\SingleFile\WinPurifyPro.exe" (
    copy /y "!PUBLISH_DIR!\SingleFile\WinPurifyPro.exe" "!PUBLISH_DIR!\SingleFile\WinPurifyPro_v!APP_VERSION!.exe" >nul 2>&1
)

echo [+] Publishing Single-File WinPurifyCommander.exe...
if exist "publish\TempSingleCommander" rd /s /q "publish\TempSingleCommander" >nul 2>&1
dotnet publish WinPurifyCommander.csproj -c Release -r win-x64 --self-contained false -f !TARGET_FRAMEWORK! -p:TargetFramework=!TARGET_FRAMEWORK! -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "publish\TempSingleCommander"
if errorlevel 1 goto :ERROR_BUILD_FAILED

if exist "publish\TempSingleCommander\WinPurifyCommander.exe" (
    copy /y "publish\TempSingleCommander\WinPurifyCommander.exe" "!PUBLISH_DIR!\SingleFile\WinPurifyCommander.exe" >nul 2>&1
    copy /y "publish\TempSingleCommander\WinPurifyCommander.exe" "!PUBLISH_DIR!\SingleFile\WinPurifyCommander_v!APP_VERSION!.exe" >nul 2>&1
)
rd /s /q "publish\TempSingleCommander" >nul 2>&1

if exist "PurifyEngineCore.dll" copy /y "PurifyEngineCore.dll" "!PUBLISH_DIR!\SingleFile\PurifyEngineCore.dll" >nul 2>&1
if exist "WinPurify_logo.png" copy /y "WinPurify_logo.png" "!PUBLISH_DIR!\SingleFile\WinPurify_logo.png" >nul 2>&1
if exist "WinPurify_icon.png" copy /y "WinPurify_icon.png" "!PUBLISH_DIR!\SingleFile\WinPurify_icon.png" >nul 2>&1
if exist "WinPurify.ico" copy /y "WinPurify.ico" "!PUBLISH_DIR!\SingleFile\WinPurify.ico" >nul 2>&1
if exist "LICENSE_KO.txt" copy /y "LICENSE_KO.txt" "!PUBLISH_DIR!\SingleFile\LICENSE_KO.txt" >nul 2>&1
if exist "LICENSE_EN.txt" copy /y "LICENSE_EN.txt" "!PUBLISH_DIR!\SingleFile\LICENSE_EN.txt" >nul 2>&1

echo.
echo =====================================================================
echo  [SUCCESS] Single-File Portable Build Completed!
echo  - Target Framework: !TARGET_FRAMEWORK!
echo  - Main Client: !PUBLISH_DIR!\SingleFile\WinPurifyPro_v!APP_VERSION!.exe
echo  - Commander:   !PUBLISH_DIR!\SingleFile\WinPurifyCommander_v!APP_VERSION!.exe
echo =====================================================================
if "%TARGET_CHOICE%"=="5" goto :BUILD_ALL_NEXT_2_SUB
goto :BUILD_END

:BUILD_UNPACKED_ONLY
set "TARGET_CHOICE=7"
goto :BUILD_UNPACKED

:BUILD_UNPACKED
call :SET_TFM_VARS
call :RESTORE_PROJECT_ASSETS
echo.
echo =====================================================================
echo [*] Building Unpacked Multi-File Production Directory for !TFM_NAME!...
echo =====================================================================
if not exist "!PUBLISH_DIR!\App" md "!PUBLISH_DIR!\App" >nul 2>&1

echo [+] Publishing WinPurifyPro.csproj (Multi-File Unpacked)...
dotnet publish WinPurifyPro.csproj -c Release -r win-x64 --self-contained false -f !TARGET_FRAMEWORK! -p:TargetFramework=!TARGET_FRAMEWORK! -p:PublishSingleFile=false -p:DebugType=none -p:DebugSymbols=false -o "!PUBLISH_DIR!\App"
if errorlevel 1 goto :ERROR_BUILD_FAILED

echo [+] Publishing WinPurifyCommander.csproj (Multi-File Unpacked)...
if exist "publish\TempCommander" rd /s /q "publish\TempCommander" >nul 2>&1
dotnet publish WinPurifyCommander.csproj -c Release -r win-x64 --self-contained false -f !TARGET_FRAMEWORK! -p:TargetFramework=!TARGET_FRAMEWORK! -p:PublishSingleFile=false -p:DebugType=none -p:DebugSymbols=false -o "publish\TempCommander"
if errorlevel 1 goto :ERROR_BUILD_FAILED

if exist "publish\TempCommander\WinPurifyCommander.exe" copy /y "publish\TempCommander\WinPurifyCommander.exe" "!PUBLISH_DIR!\App\WinPurifyCommander.exe" >nul 2>&1
if exist "publish\TempCommander\WinPurifyCommander.dll" copy /y "publish\TempCommander\WinPurifyCommander.dll" "!PUBLISH_DIR!\App\WinPurifyCommander.dll" >nul 2>&1
rd /s /q "publish\TempCommander" >nul 2>&1

:: Copy versioned binaries
if exist "!PUBLISH_DIR!\App\WinPurifyPro.exe" (
    copy /y "!PUBLISH_DIR!\App\WinPurifyPro.exe" "!PUBLISH_DIR!\App\WinPurifyPro_v!APP_VERSION!.exe" >nul 2>&1
)
if exist "!PUBLISH_DIR!\App\WinPurifyCommander.exe" (
    copy /y "!PUBLISH_DIR!\App\WinPurifyCommander.exe" "!PUBLISH_DIR!\App\WinPurifyCommander_v!APP_VERSION!.exe" >nul 2>&1
)

if exist "PurifyEngineCore.dll" copy /y "PurifyEngineCore.dll" "!PUBLISH_DIR!\App\PurifyEngineCore.dll" >nul 2>&1
if exist "WinPurify_logo.png" copy /y "WinPurify_logo.png" "!PUBLISH_DIR!\App\WinPurify_logo.png" >nul 2>&1
if exist "WinPurify_icon.png" copy /y "WinPurify_icon.png" "!PUBLISH_DIR!\App\WinPurify_icon.png" >nul 2>&1
if exist "WinPurify.ico" copy /y "WinPurify.ico" "!PUBLISH_DIR!\App\WinPurify.ico" >nul 2>&1
if exist "LICENSE_KO.txt" copy /y "LICENSE_KO.txt" "!PUBLISH_DIR!\App\LICENSE_KO.txt" >nul 2>&1
if exist "LICENSE_EN.txt" copy /y "LICENSE_EN.txt" "!PUBLISH_DIR!\App\LICENSE_EN.txt" >nul 2>&1
echo 1 > "!PUBLISH_DIR!\App\installed.tag"

echo.
echo =====================================================================
echo  [SUCCESS] Unpacked Directory Built: !PUBLISH_DIR!\App
echo =====================================================================
if "%TARGET_CHOICE%"=="2" goto :COMPILE_INNO_SETUP
if "%TARGET_CHOICE%"=="5" goto :BUILD_ALL_NEXT_1
goto :BUILD_END

:BUILD_COMMANDER_ONLY
call :SET_TFM_VARS
call :RESTORE_PROJECT_ASSETS
echo.
echo =====================================================================
echo [*] Building Central Commander Console Suite for !TFM_NAME!...
echo =====================================================================
goto :BUILD_COMMANDER_STANDALONE_INTERNAL

:BUILD_COMMANDER_FX_ONLY
call :SET_TFM_VARS
call :RESTORE_PROJECT_ASSETS
echo.
echo =====================================================================
echo [*] Building Central Commander Console (Framework-Dependent ~3MB)...
echo =====================================================================
if not exist "!PUBLISH_DIR!\Commander" md "!PUBLISH_DIR!\Commander" >nul 2>&1
echo [+] Building WinPurifyCommander.exe (Framework-Dependent)...
if exist "publish\TempFxCommander" rd /s /q "publish\TempFxCommander" >nul 2>&1
dotnet publish WinPurifyCommander.csproj -c Release -r win-x64 --self-contained false -f !TARGET_FRAMEWORK! -p:TargetFramework=!TARGET_FRAMEWORK! -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "publish\TempFxCommander"
if errorlevel 1 goto :ERROR_BUILD_FAILED
if exist "publish\TempFxCommander\WinPurifyCommander.exe" (
    copy /y "publish\TempFxCommander\WinPurifyCommander.exe" "!PUBLISH_DIR!\Commander\WinPurifyCommander.exe" >nul 2>&1
    copy /y "publish\TempFxCommander\WinPurifyCommander.exe" "!PUBLISH_DIR!\Commander\WinPurifyCommander_v!APP_VERSION!.exe" >nul 2>&1
)
rd /s /q "publish\TempFxCommander" >nul 2>&1

if exist "PurifyEngineCore.dll" copy /y "PurifyEngineCore.dll" "!PUBLISH_DIR!\Commander\PurifyEngineCore.dll" >nul 2>&1
if exist "WinPurify_logo.png" copy /y "WinPurify_logo.png" "!PUBLISH_DIR!\Commander\WinPurify_logo.png" >nul 2>&1
if exist "WinPurify_icon.png" copy /y "WinPurify_icon.png" "!PUBLISH_DIR!\Commander\WinPurify_icon.png" >nul 2>&1
if exist "WinPurify.ico" copy /y "WinPurify.ico" "!PUBLISH_DIR!\Commander\WinPurify.ico" >nul 2>&1

echo.
echo =====================================================================
echo  [SUCCESS] WinPurifyCommander Framework-Dependent Build Completed!
echo  - Output: !PUBLISH_DIR!\Commander\WinPurifyCommander_v!APP_VERSION!.exe
echo =====================================================================
if "%TARGET_CHOICE%"=="5" goto :BUILD_ALL_FINISH
goto :BUILD_END

:BUILD_COMMANDER_STANDALONE_INTERNAL
call :SET_TFM_VARS
call :RESTORE_PROJECT_ASSETS
echo.
echo =====================================================================
echo [*] Building Central Commander Console (Self-Contained Standalone, ~70MB)...
echo =====================================================================
if not exist "!PUBLISH_DIR!\Commander" md "!PUBLISH_DIR!\Commander" >nul 2>&1
echo [+] Building WinPurifyCommander.exe (Self-Contained Standalone Slim)...
if exist "publish\TempStandaloneCommander" rd /s /q "publish\TempStandaloneCommander" >nul 2>&1
dotnet publish WinPurifyCommander.csproj -c Release -r win-x64 --self-contained true -f !TARGET_FRAMEWORK! -p:TargetFramework=!TARGET_FRAMEWORK! -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "publish\TempStandaloneCommander"
if errorlevel 1 goto :ERROR_BUILD_FAILED
if exist "publish\TempStandaloneCommander\WinPurifyCommander.exe" (
    copy /y "publish\TempStandaloneCommander\WinPurifyCommander.exe" "!PUBLISH_DIR!\Commander\WinPurifyCommander.exe" >nul 2>&1
    copy /y "publish\TempStandaloneCommander\WinPurifyCommander.exe" "!PUBLISH_DIR!\Commander\WinPurifyCommander_v!APP_VERSION!_Standalone.exe" >nul 2>&1
)
rd /s /q "publish\TempStandaloneCommander" >nul 2>&1

if exist "PurifyEngineCore.dll" copy /y "PurifyEngineCore.dll" "!PUBLISH_DIR!\Commander\PurifyEngineCore.dll" >nul 2>&1
if exist "WinPurify_logo.png" copy /y "WinPurify_logo.png" "!PUBLISH_DIR!\Commander\WinPurify_logo.png" >nul 2>&1
if exist "WinPurify_icon.png" copy /y "WinPurify_icon.png" "!PUBLISH_DIR!\Commander\WinPurify_icon.png" >nul 2>&1
if exist "WinPurify.ico" copy /y "WinPurify.ico" "!PUBLISH_DIR!\Commander\WinPurify.ico" >nul 2>&1

echo.
echo =====================================================================
echo  [SUCCESS] WinPurifyCommander Standalone Build Completed!
echo  - Output: !PUBLISH_DIR!\Commander\WinPurifyCommander_v!APP_VERSION!_Standalone.exe
echo =====================================================================
if "%TARGET_CHOICE%"=="5" goto :BUILD_ALL_FINISH
goto :BUILD_END

:BUILD_INSTALLER_ONLY
set "TARGET_CHOICE=2"
goto :BUILD_UNPACKED

:COMPILE_INNO_SETUP
call :SET_TFM_VARS
echo.
echo =====================================================================
echo [*] Compiling Inno Setup 6 Installer for !TARGET_FRAMEWORK! (!TFM_NAME!)...
echo =====================================================================

:: Search standard Inno Setup installation paths
set "ISCC_BIN="
where iscc.exe >nul 2>&1
if not errorlevel 1 set "ISCC_BIN=iscc.exe"

if not defined ISCC_BIN if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" set "ISCC_BIN=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if not defined ISCC_BIN if exist "C:\Program Files\Inno Setup 6\ISCC.exe" set "ISCC_BIN=C:\Program Files\Inno Setup 6\ISCC.exe"
if not defined ISCC_BIN if exist "D:\Program Files (x86)\Inno Setup 6\ISCC.exe" set "ISCC_BIN=D:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if not defined ISCC_BIN if exist "D:\Program Files\Inno Setup 6\ISCC.exe" set "ISCC_BIN=D:\Program Files\Inno Setup 6\ISCC.exe"
if not defined ISCC_BIN if exist "C:\Inno Setup 6\ISCC.exe" set "ISCC_BIN=C:\Inno Setup 6\ISCC.exe"
if not defined ISCC_BIN if exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" set "ISCC_BIN=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if not defined ISCC_BIN if exist "%ProgramFiles%\Inno Setup 6\ISCC.exe" set "ISCC_BIN=%ProgramFiles%\Inno Setup 6\ISCC.exe"
if not defined ISCC_BIN if exist "%LocalAppData%\Programs\Inno Setup 6\ISCC.exe" set "ISCC_BIN=%LocalAppData%\Programs\Inno Setup 6\ISCC.exe"

if not defined ISCC_BIN goto :WARN_NO_INNO_SETUP
if not exist "installer.iss" goto :ERROR_NO_ISS

if not exist "!PUBLISH_DIR!\Installer" md "!PUBLISH_DIR!\Installer" >nul 2>&1
set "SETUP_OUT_FILENAME=WinPurifyPro_v!APP_VERSION!_!TFM_NAME!_Setup"

echo [+] Executing Inno Setup Compiler: "!ISCC_BIN!"
"!ISCC_BIN!" /DMyAppVersion="!APP_VERSION!" /DAppSourceDir="!PUBLISH_DIR!\App" /DOutputDir="!PUBLISH_DIR!\Installer" /DOutputBaseFilename="!SETUP_OUT_FILENAME!" "installer.iss"
if errorlevel 1 (
    echo [ERROR] Inno Setup compilation failed.
    if "%TARGET_CHOICE%"=="5" goto :BUILD_ALL_NEXT_2
    goto :BUILD_END
)

echo.
echo =====================================================================
echo  [SUCCESS] Inno Setup 6 Installer Created Successfully!
echo  - Target Framework: !TARGET_FRAMEWORK! (!TFM_NAME!)
echo  - Output Installer: !PUBLISH_DIR!\Installer\!SETUP_OUT_FILENAME!.exe
echo =====================================================================
if "%TARGET_CHOICE%"=="5" goto :BUILD_ALL_NEXT_2
goto :BUILD_END

:WARN_NO_INNO_SETUP
echo.
echo ---------------------------------------------------------------------
echo  [NOTE] Inno Setup 6 Compiler (ISCC.exe) was not found.
echo ---------------------------------------------------------------------
echo  * Download Inno Setup 6: https://jrsoftware.org/isdl.php
echo  * After installing, rerun 'build.bat 2' or open 'installer.iss' in Inno Setup.
echo ---------------------------------------------------------------------
if "%TARGET_CHOICE%"=="5" goto :BUILD_ALL_NEXT_2
goto :BUILD_END

:ERROR_NO_ISS
echo [ERROR] installer.iss script file not found.
if "%TARGET_CHOICE%"=="5" goto :BUILD_ALL_NEXT_2
goto :BUILD_END

:BUILD_STANDALONE
call :SET_TFM_VARS
call :RESTORE_PROJECT_ASSETS
echo.
echo =====================================================================
echo [*] Building Standalone Single-File (.NET Runtime Embedded) for !TFM_NAME!...
echo =====================================================================
if not exist "!PUBLISH_DIR!\Standalone" md "!PUBLISH_DIR!\Standalone" >nul 2>&1

echo [+] Publishing Standalone WinPurifyPro.exe...
dotnet publish WinPurifyPro.csproj -c Release -r win-x64 --self-contained true -f !TARGET_FRAMEWORK! -p:TargetFramework=!TARGET_FRAMEWORK! -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "!PUBLISH_DIR!\Standalone"
if errorlevel 1 goto :ERROR_BUILD_FAILED

if exist "!PUBLISH_DIR!\Standalone\WinPurifyPro.exe" (
    copy /y "!PUBLISH_DIR!\Standalone\WinPurifyPro.exe" "!PUBLISH_DIR!\Standalone\WinPurifyPro_v!APP_VERSION!_Standalone.exe" >nul 2>&1
)

echo [+] Publishing Standalone WinPurifyCommander.exe...
if exist "publish\TempStandaloneCommander" rd /s /q "publish\TempStandaloneCommander" >nul 2>&1
dotnet publish WinPurifyCommander.csproj -c Release -r win-x64 --self-contained true -f !TARGET_FRAMEWORK! -p:TargetFramework=!TARGET_FRAMEWORK! -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "publish\TempStandaloneCommander"
if errorlevel 1 goto :ERROR_BUILD_FAILED

if exist "publish\TempStandaloneCommander\WinPurifyCommander.exe" (
    copy /y "publish\TempStandaloneCommander\WinPurifyCommander.exe" "!PUBLISH_DIR!\Standalone\WinPurifyCommander.exe" >nul 2>&1
    copy /y "publish\TempStandaloneCommander\WinPurifyCommander.exe" "!PUBLISH_DIR!\Standalone\WinPurifyCommander_v!APP_VERSION!_Standalone.exe" >nul 2>&1
)
rd /s /q "publish\TempStandaloneCommander" >nul 2>&1

if exist "PurifyEngineCore.dll" copy /y "PurifyEngineCore.dll" "!PUBLISH_DIR!\Standalone\PurifyEngineCore.dll" >nul 2>&1
if exist "WinPurify_logo.png" copy /y "WinPurify_logo.png" "!PUBLISH_DIR!\Standalone\WinPurify_logo.png" >nul 2>&1
if exist "WinPurify_icon.png" copy /y "WinPurify_icon.png" "!PUBLISH_DIR!\Standalone\WinPurify_icon.png" >nul 2>&1
if exist "WinPurify.ico" copy /y "WinPurify.ico" "!PUBLISH_DIR!\Standalone\WinPurify.ico" >nul 2>&1

echo.
echo =====================================================================
echo  [SUCCESS] Standalone Single-File Build Completed!
echo  - Main Client: !PUBLISH_DIR!\Standalone\WinPurifyPro_v!APP_VERSION!_Standalone.exe (~70MB)
echo  - Commander:   !PUBLISH_DIR!\Standalone\WinPurifyCommander_v!APP_VERSION!_Standalone.exe (~70MB)
echo =====================================================================
if "%TARGET_CHOICE%"=="5" goto :BUILD_ALL_NEXT_3
goto :BUILD_END

:BUILD_STANDARD
call :SET_TFM_VARS
call :RESTORE_PROJECT_ASSETS
echo.
echo =====================================================================
echo [*] Building Standard Framework-Dependent WPF Suite for !TFM_NAME!...
echo =====================================================================
if not exist "!PUBLISH_DIR!\Standard" md "!PUBLISH_DIR!\Standard" >nul 2>&1

echo [+] Building WinPurifyPro.exe (Framework-Dependent)...
dotnet build WinPurifyPro.csproj -c Release -r win-x64 --self-contained false -f !TARGET_FRAMEWORK! -p:TargetFramework=!TARGET_FRAMEWORK! -o "!PUBLISH_DIR!\Standard"
if errorlevel 1 goto :ERROR_BUILD_FAILED

if exist "!PUBLISH_DIR!\Standard\WinPurifyPro.exe" (
    copy /y "!PUBLISH_DIR!\Standard\WinPurifyPro.exe" "!PUBLISH_DIR!\Standard\WinPurifyPro_v!APP_VERSION!.exe" >nul 2>&1
)

echo [+] Building WinPurifyCommander.exe (Framework-Dependent)...
dotnet build WinPurifyCommander.csproj -c Release -r win-x64 --self-contained false -f !TARGET_FRAMEWORK! -p:TargetFramework=!TARGET_FRAMEWORK! -o "!PUBLISH_DIR!\Standard"
if errorlevel 1 goto :ERROR_BUILD_FAILED

if exist "!PUBLISH_DIR!\Standard\WinPurifyCommander.exe" (
    copy /y "!PUBLISH_DIR!\Standard\WinPurifyCommander.exe" "!PUBLISH_DIR!\Standard\WinPurifyCommander_v!APP_VERSION!.exe" >nul 2>&1
)

if exist "PurifyEngineCore.dll" copy /y "PurifyEngineCore.dll" "!PUBLISH_DIR!\Standard\PurifyEngineCore.dll" >nul 2>&1
if exist "WinPurify_logo.png" copy /y "WinPurify_logo.png" "!PUBLISH_DIR!\Standard\WinPurify_logo.png" >nul 2>&1
if exist "WinPurify_icon.png" copy /y "WinPurify_icon.png" "!PUBLISH_DIR!\Standard\WinPurify_icon.png" >nul 2>&1
if exist "WinPurify.ico" copy /y "WinPurify.ico" "!PUBLISH_DIR!\Standard\WinPurify.ico" >nul 2>&1

echo.
echo =====================================================================
echo  [SUCCESS] Standard Framework-Dependent Build Completed!
echo  - Main Client: !PUBLISH_DIR!\Standard\WinPurifyPro_v!APP_VERSION!.exe
echo  - Commander:   !PUBLISH_DIR!\Standard\WinPurifyCommander_v!APP_VERSION!.exe
echo =====================================================================
if "%TARGET_CHOICE%"=="5" goto :BUILD_ALL_NEXT_4
goto :BUILD_END

:BUILD_ALL
set "TARGET_CHOICE=5"
goto :BUILD_UNPACKED

:BUILD_ALL_NEXT_1
goto :COMPILE_INNO_SETUP

:BUILD_ALL_NEXT_2
goto :BUILD_SINGLE_FILE

:BUILD_ALL_NEXT_2_SUB
goto :BUILD_STANDALONE

:BUILD_ALL_NEXT_3
goto :BUILD_STANDARD

:BUILD_ALL_NEXT_4
call :SET_TFM_VARS
call :RESTORE_PROJECT_ASSETS
echo.
echo =====================================================================
echo [*] Building Central Commander Suite for !TFM_NAME!...
echo =====================================================================
if not exist "!PUBLISH_DIR!\Commander" md "!PUBLISH_DIR!\Commander" >nul 2>&1

echo [+] Building WinPurifyCommander.exe (Framework-Dependent Slim, ~3MB)...
if exist "publish\TempFxCommander" rd /s /q "publish\TempFxCommander" >nul 2>&1
dotnet publish WinPurifyCommander.csproj -c Release -r win-x64 --self-contained false -f !TARGET_FRAMEWORK! -p:TargetFramework=!TARGET_FRAMEWORK! -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "publish\TempFxCommander"
if errorlevel 1 goto :ERROR_BUILD_FAILED
if exist "publish\TempFxCommander\WinPurifyCommander.exe" (
    copy /y "publish\TempFxCommander\WinPurifyCommander.exe" "!PUBLISH_DIR!\Commander\WinPurifyCommander.exe" >nul 2>&1
    copy /y "publish\TempFxCommander\WinPurifyCommander.exe" "!PUBLISH_DIR!\Commander\WinPurifyCommander_v!APP_VERSION!_Framework.exe" >nul 2>&1
)
rd /s /q "publish\TempFxCommander" >nul 2>&1

echo [+] Building WinPurifyCommander_Standalone.exe (Self-Contained Standalone, ~70MB)...
if exist "publish\TempStandaloneCommander" rd /s /q "publish\TempStandaloneCommander" >nul 2>&1
dotnet publish WinPurifyCommander.csproj -c Release -r win-x64 --self-contained true -f !TARGET_FRAMEWORK! -p:TargetFramework=!TARGET_FRAMEWORK! -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=none -p:DebugSymbols=false -o "publish\TempStandaloneCommander"
if errorlevel 1 goto :ERROR_BUILD_FAILED
if exist "publish\TempStandaloneCommander\WinPurifyCommander.exe" (
    copy /y "publish\TempStandaloneCommander\WinPurifyCommander.exe" "!PUBLISH_DIR!\Commander\WinPurifyCommander_v!APP_VERSION!_Standalone.exe" >nul 2>&1
)
rd /s /q "publish\TempStandaloneCommander" >nul 2>&1

if exist "PurifyEngineCore.dll" copy /y "PurifyEngineCore.dll" "!PUBLISH_DIR!\Commander\PurifyEngineCore.dll" >nul 2>&1
if exist "WinPurify_logo.png" copy /y "WinPurify_logo.png" "!PUBLISH_DIR!\Commander\WinPurify_logo.png" >nul 2>&1
if exist "WinPurify_icon.png" copy /y "WinPurify_icon.png" "!PUBLISH_DIR!\Commander\WinPurify_icon.png" >nul 2>&1
if exist "WinPurify.ico" copy /y "WinPurify.ico" "!PUBLISH_DIR!\Commander\WinPurify.ico" >nul 2>&1

:BUILD_ALL_FINISH
if "%IS_SEQUENTIAL_DUAL%"=="1" (
    if "%DUAL_STAGE%"=="1" goto :DUAL_STAGE_2
    if "%DUAL_STAGE%"=="2" goto :DUAL_STAGE_FINISH
)

echo.
echo =====================================================================
echo  [SUCCESS] All Build Profiles Completed for !TARGET_FRAMEWORK! (v!APP_VERSION!)!
echo  - Output Directory: !PUBLISH_DIR!\
echo =====================================================================
goto :BUILD_END

:ERROR_NO_DOTNET
echo [ERROR] .NET SDK (dotnet.exe) was not found in system PATH.
echo Please install .NET 8 SDK or .NET 10 SDK: https://dotnet.microsoft.com/download
goto :BUILD_END

:ERROR_NO_CSPROJ
echo [ERROR] WinPurifyPro.csproj not found in current directory (%~dp0).
goto :BUILD_END

:ERROR_BUILD_FAILED
echo.
echo [ERROR] Build step failed with error code %ERRORLEVEL%.
echo Please inspect the compiler output above for details.
goto :BUILD_END

:BUILD_END
echo.
echo Exiting builder script...
endlocal
exit /b 0
