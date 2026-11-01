@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo =====================================================================
echo  WinPurify Pro - GitHub Automated Sync ^& Push Launcher
echo  Developer:   AhBiYout
echo  Account:     AhBiYout-all
echo  Repository:  WinPurifyPro
echo  Target URL:  https://github.com/AhBiYout-all/WinPurifyPro
echo  Official Blog: https://ahbiyoutvibe.blogspot.com/
echo =====================================================================
echo.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0github_push.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [!] PowerShell execution completed or paused.
    pause
)
