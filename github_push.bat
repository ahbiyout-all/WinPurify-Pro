@echo off
chcp 65001 >nul
cd /d "%~dp0"

echo =====================================================================
echo  WinPurify Pro - GitHub Automated Sync ^& Push Launcher
echo  Account:    ahbiyout-all
echo  Repository: WinPurify-Pro
echo  Target URL: https://github.com/ahbiyout-all/WinPurify-Pro
echo =====================================================================
echo.

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0github_push.ps1"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [!] PowerShell execution completed or paused.
    pause
)
