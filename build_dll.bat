@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion
cd /d "%~dp0"
title PurifyEngineCore.dll Native Builder

echo =====================================================================
echo    PurifyEngineCore.dll Native C++ Compiler ^& Diagnostics
echo    Target: x64 Release Optimization /O2 /MT (or MinGW -O3)
echo =====================================================================
echo.

:: 1. Check if cl.exe is already available in current PATH
where cl.exe >nul 2>&1
if not errorlevel 1 (
    echo [*] Detected MSVC compiler (cl.exe) in current environment.
    goto :DO_MSVC_COMPILE
)

:: 2. Locate Visual Studio using vswhere.exe (Official Locator)
echo [*] Searching for Visual Studio installation via vswhere...
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" set "VSWHERE=%ProgramFiles%\Microsoft Visual Studio\Installer\vswhere.exe"

if exist "%VSWHERE%" (
    for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath 2^>nul`) do (
        set "VS_PATH=%%i"
    )
    if defined VS_PATH (
        if exist "!VS_PATH!\VC\Auxiliary\Build\vcvars64.bat" (
            echo [+] Found Visual Studio at: !VS_PATH!
            call "!VS_PATH!\VC\Auxiliary\Build\vcvars64.bat" >nul 2>&1
            where cl.exe >nul 2>&1
            if not errorlevel 1 goto :DO_MSVC_COMPILE
        )
    )
)

:: 3. Exhaustive fallback path scanning for VS 2022 / 2019 / 2017 / BuildTools
echo [*] Scanning standard Visual Studio directories...
for %%D in ("%ProgramFiles%" "%ProgramFiles(x86)%" "C:\Program Files" "C:\Program Files (x86)" "D:\Program Files" "D:\Program Files (x86)") do (
    for %%Y in (2022 2019 2017) do (
        for %%E in (Community Professional Enterprise BuildTools Preview) do (
            if exist "%%~D\Microsoft Visual Studio\%%Y\%%E\VC\Auxiliary\Build\vcvars64.bat" (
                echo [+] Found Visual Studio vcvars64: %%~D\Microsoft Visual Studio\%%Y\%%E
                call "%%~D\Microsoft Visual Studio\%%Y\%%E\VC\Auxiliary\Build\vcvars64.bat" >nul 2>&1
                where cl.exe >nul 2>&1
                if not errorlevel 1 goto :DO_MSVC_COMPILE
            )
        )
    )
)

:: 4. Check for MinGW-w64 (g++.exe) or Clang (clang++.exe)
where g++.exe >nul 2>&1
if not errorlevel 1 (
    echo [*] MSVC not found, but GCC (g++.exe) detected!
    goto :DO_GCC_COMPILE
)

where clang++.exe >nul 2>&1
if not errorlevel 1 (
    echo [*] MSVC not found, but LLVM Clang (clang++.exe) detected!
    goto :DO_CLANG_COMPILE
)

:: 5. No C++ Compiler Found - Show Clear Guidance
goto :ERROR_NO_COMPILER

:DO_MSVC_COMPILE
if not exist "main.cpp" goto :ERROR_NO_SOURCE

echo [+] Compiling main.cpp with MSVC cl.exe (x64 Release /O2 /MT)...
if exist "version.rc" rc.exe /nologo version.rc >nul 2>&1
if exist "version.res" (
    cl.exe /LD /O2 /MT /EHsc /std:c++17 /utf-8 /DUNICODE /D_UNICODE main.cpp version.res /Fe"PurifyEngineCore.dll" advapi32.lib shell32.lib psapi.lib user32.lib ole32.lib oleaut32.lib ws2_32.lib iphlpapi.lib /link /OUT:"PurifyEngineCore.dll" /NOLOGO
    del /f /q "version.res" >nul 2>&1
) else (
    cl.exe /LD /O2 /MT /EHsc /std:c++17 /utf-8 /DUNICODE /D_UNICODE main.cpp /Fe"PurifyEngineCore.dll" advapi32.lib shell32.lib psapi.lib user32.lib ole32.lib oleaut32.lib ws2_32.lib iphlpapi.lib /link /OUT:"PurifyEngineCore.dll" /NOLOGO
)
set "BUILD_EXIT_CODE=%errorlevel%"
if exist "main.obj" del /f /q "main.obj" >nul 2>&1
if exist "main.exp" del /f /q "main.exp" >nul 2>&1
if exist "main.lib" del /f /q "main.lib" >nul 2>&1
if exist "PurifyEngineCore.exp" del /f /q "PurifyEngineCore.exp" >nul 2>&1
if exist "PurifyEngineCore.lib" del /f /q "PurifyEngineCore.lib" >nul 2>&1

if exist "PurifyEngineCore.dll" goto :SUCCESS_DLL
goto :ERROR_BUILD_FAIL

:DO_GCC_COMPILE
if not exist "main.cpp" goto :ERROR_NO_SOURCE

echo [+] Compiling main.cpp with MinGW-w64 g++ (x64 Release -O3)...
if exist "version.rc" windres version.rc -O coff -o version.res >nul 2>&1
if exist "version.res" (
    g++ -shared -O3 -std=c++17 -DUNICODE -D_UNICODE main.cpp version.res -o PurifyEngineCore.dll -ladvapi32 -lshell32 -lpsapi -luser32 -lole32 -loleaut32 -lws2_32 -liphlpapi -static -static-libgcc -static-libstdc++
    del /f /q "version.res" >nul 2>&1
) else (
    g++ -shared -O3 -std=c++17 -DUNICODE -D_UNICODE main.cpp -o PurifyEngineCore.dll -ladvapi32 -lshell32 -lpsapi -luser32 -lole32 -loleaut32 -lws2_32 -liphlpapi -static -static-libgcc -static-libstdc++
)
set "BUILD_EXIT_CODE=%errorlevel%"

if exist "PurifyEngineCore.dll" goto :SUCCESS_DLL
goto :ERROR_BUILD_FAIL

:DO_CLANG_COMPILE
if not exist "main.cpp" goto :ERROR_NO_SOURCE

echo [+] Compiling main.cpp with LLVM clang++ (x64 Release -O3)...
clang++ -shared -O3 -std=c++17 -DUNICODE -D_UNICODE main.cpp -o PurifyEngineCore.dll -ladvapi32 -lshell32 -lpsapi -luser32 -lole32 -loleaut32 -lws2_32 -liphlpapi
set "BUILD_EXIT_CODE=%errorlevel%"

if exist "PurifyEngineCore.dll" goto :SUCCESS_DLL
goto :ERROR_BUILD_FAIL

:SUCCESS_DLL
echo.
echo =====================================================================
echo    [SUCCESS] PurifyEngineCore.dll 생성 완료!
echo    - 위치: %~dp0PurifyEngineCore.dll
echo =====================================================================
goto :FINISH_DLL

:ERROR_NO_SOURCE
echo.
echo [ERROR] main.cpp 소스 파일이 현재 디렉터리에 존재하지 않습니다.
goto :FINISH_DLL

:ERROR_BUILD_FAIL
echo.
echo [ERROR] 컴파일러 실행 중 오류가 발생하여 DLL 생성에 실패했습니다 (Exit Code: %BUILD_EXIT_CODE%).
echo 상단에 출력된 컴파일러 에러 메시지를 확인해주세요.
goto :FINISH_DLL

:ERROR_NO_COMPILER
echo.
echo ---------------------------------------------------------------------
echo  [안내] C++ 컴파일러(MSVC, GCC, Clang) 환경을 찾을 수 없습니다.
echo ---------------------------------------------------------------------
echo  * PurifyEngineCore.dll은 시스템 가속을 위한 C++ Native 보조 모듈입니다.
echo  * 본 모듈이 없어도 WinPurify Pro C# WPF 앱은 100%% 순수 C# 폴백 엔진으로
echo    모든 115개 최적화 및 정화 기능을 정상적으로 수행합니다.
echo.
echo  [C++ DLL 직접 빌드 방법]:
echo   1. Visual Studio 설치 관리자(Visual Studio Installer) 실행
echo      - 'C++를 사용한 데스크톱 개발' 워크로드 설치
echo   2. 시작 메뉴에서 'x64 Native Tools Command Prompt for VS 2022' 실행 후
echo      본 폴더로 이동하여 'build_dll.bat' 실행
echo   3. 또는 MinGW-w64 (g++) / LLVM Clang 설치 후 실행
echo ---------------------------------------------------------------------
goto :FINISH_DLL

:FINISH_DLL
echo.
if /i "%~1"=="nopause" goto :EOF
if /i "%~2"=="nopause" goto :EOF
echo Press any key to continue...
pause
