@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

echo =====================================================================
echo  WinPurify Pro - GitHub Automated Sync & Push Tool
echo  Account:    ahbiyout-all
echo  Repository: WinPurify-Pro
echo  Target URL: https://github.com/ahbiyout-all/WinPurify-Pro
echo =====================================================================
echo.

:: Step 1: Execute version synchronization across all 14 files
echo [*] Step 1: Synchronizing version numbers across all project files...
call node sync-version.js
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] sync-version.js failed with error code %ERRORLEVEL%.
    pause
    exit /b %ERRORLEVEL%
)

:: Step 2: Extract current version from package.json
for /f "tokens=2 delims=:, " %%a in ('findstr "\"version\":" package.json') do (
    set RAW_VER=%%~a
)
set APP_VER=%RAW_VER:"=%
echo [+] Current Synchronized Version: v%APP_VER%
echo.

:: Step 3: Check and set Git Remote Origin
echo [*] Step 2: Configuring Git remote origin repository...
git remote get-url origin >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [!] Remote origin not found. Adding origin https://github.com/ahbiyout-all/WinPurify-Pro.git ...
    git remote add origin https://github.com/ahbiyout-all/WinPurify-Pro.git
) else (
    git remote set-url origin https://github.com/ahbiyout-all/WinPurify-Pro.git
    echo [+] Remote origin is set to https://github.com/ahbiyout-all/WinPurify-Pro.git
)

:: Step 4: Git Status check & staging
echo.
echo [*] Step 3: Staging all changed files (excluding .gitignore rules)...
git add -A
git status --short

echo.
set /p COMMIT_MSG="Enter commit description (Press Enter for default: 'Release v%APP_VER%'): "
if "%COMMIT_MSG%"=="" (
    set COMMIT_MSG=Release v%APP_VER% - WinPurify Pro Full Update (ahbiyout-all)
)

:: Step 5: Commit changes
echo.
echo [*] Step 4: Committing changes with message: "%COMMIT_MSG%"...
git commit -m "%COMMIT_MSG%"
if %ERRORLEVEL% NEQ 0 (
    echo [!] No new changes to commit or commit already up to date.
)

:: Step 6: Create or update version tag
echo.
echo [*] Step 5: Creating Git tag v%APP_VER%...
git tag -fa "v%APP_VER%" -m "WinPurify Pro v%APP_VER% Release"

:: Step 7: Push to GitHub
echo.
echo [*] Step 6: Pushing commits and tags to GitHub (ahbiyout-all/WinPurify-Pro)...
echo [+] Target: https://github.com/ahbiyout-all/WinPurify-Pro
echo.

git branch -M main
git push -u origin main
if %ERRORLEVEL% NEQ 0 (
    echo [!] Pushing to 'main' branch failed, trying 'master' branch...
    git push -u origin master
)

git push origin "v%APP_VER%" --force

echo.
echo =====================================================================
echo  [SUCCESS] All files and version tag v%APP_VER% successfully pushed!
echo  GitHub Actions will now automatically build and publish releases!
echo.
echo  GitHub Repository: https://github.com/ahbiyout-all/WinPurify-Pro
echo  GitHub Actions:    https://github.com/ahbiyout-all/WinPurify-Pro/actions
echo  GitHub Releases:   https://github.com/ahbiyout-all/WinPurify-Pro/releases
echo =====================================================================
echo.
pause
