# ==============================================================================
# WinPurify Pro - GitHub Automated Sync & Push Script (PowerShell)
# Target: https://github.com/ahbiyout-all/WinPurify-Pro
# ==============================================================================

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$Host.UI.RawUI.WindowTitle = "WinPurify Pro - GitHub Automated Sync & Push Tool"

Set-Location $PSScriptRoot

Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host " WinPurify Pro - GitHub Automated Sync & Push Pipeline" -ForegroundColor Green
Write-Host " Account:    ahbiyout-all" -ForegroundColor Yellow
Write-Host " Repository: WinPurify-Pro" -ForegroundColor Yellow
Write-Host " Target URL: https://github.com/ahbiyout-all/WinPurify-Pro" -ForegroundColor Yellow
Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host ""

# 0. Check Git CLI Availability
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] Git command-line tool (git.exe) is not installed or not in PATH." -ForegroundColor Red
    Write-Host "Please install Git for Windows: https://git-scm.com/download/win" -ForegroundColor Yellow
    pause
    exit 1
}

# 1. Auto-initialize Git Repository if .git is missing
if (-not (Test-Path ".git")) {
    Write-Host "[*] Initializing local Git repository (.git)..." -ForegroundColor Cyan
    git init
    git branch -M main
    Write-Host "[+] Local Git repository initialized successfully." -ForegroundColor Green
    Write-Host ""
}

# 2. Version Synchronization
Write-Host "[*] Step 1: Synchronizing version numbers across all project files..." -ForegroundColor Cyan
if (Test-Path "sync-version.js") {
    node sync-version.js
}

# Extract current version
$appVersion = "4.42.0"
if (Test-Path "package.json") {
    $pkgJson = Get-Content -Raw -Path "package.json" | ConvertFrom-Json
    $appVersion = $pkgJson.version
}
Write-Host "[+] Current Synchronized Version: v$appVersion" -ForegroundColor Green
Write-Host ""

# 3. Check and configure Git remote
Write-Host "[*] Step 2: Checking Git remote origin repository..." -ForegroundColor Cyan
$expectedUrl = "https://github.com/ahbiyout-all/WinPurify-Pro.git"
$remoteUrl = git remote get-url origin 2>$null

if (-not $remoteUrl) {
    Write-Host "[!] Adding remote origin: $expectedUrl ..." -ForegroundColor Yellow
    git remote add origin $expectedUrl
} else {
    git remote set-url origin $expectedUrl
    Write-Host "[+] Remote origin configured: $expectedUrl" -ForegroundColor Green
}

# 4. Git status & staging
Write-Host ""
Write-Host "[*] Step 3: Staging all modified files (respecting .gitignore)..." -ForegroundColor Cyan
git add -A
git status --short

# 5. Commit message input
Write-Host ""
$customCommit = Read-Host "Enter commit description (Press Enter for default: 'Release v$appVersion')"
if ([string]::IsNullOrWhiteSpace($customCommit)) {
    $customCommit = "Release v$appVersion - WinPurify Pro Full Update (ahbiyout-all)"
}

# 6. Commit changes
Write-Host ""
Write-Host "[*] Step 4: Committing changes with message: '$customCommit'..." -ForegroundColor Cyan
git commit -m "$customCommit"

# 7. Tagging
Write-Host ""
Write-Host "[*] Step 5: Creating or updating Git tag v$appVersion..." -ForegroundColor Cyan
git tag -fa "v$appVersion" -m "WinPurify Pro v$appVersion Release"

# 8. Push to origin
Write-Host ""
Write-Host "[*] Step 6: Pushing branch and tags to GitHub (ahbiyout-all/WinPurify-Pro)..." -ForegroundColor Cyan
git branch -M main
git push -u origin main --force
if ($LASTEXITCODE -ne 0) {
    Write-Host "[!] Pushing to 'main' branch failed, trying 'master' branch..." -ForegroundColor Yellow
    git push -u origin master --force
}

git push origin "v$appVersion" --force

Write-Host ""
Write-Host "=====================================================================" -ForegroundColor Green
Write-Host " [SUCCESS] WinPurify Pro v$appVersion successfully pushed to GitHub!" -ForegroundColor Green
Write-Host " GitHub Actions is automatically building your release binaries!" -ForegroundColor Green
Write-Host " GitHub Repository: https://github.com/ahbiyout-all/WinPurify-Pro" -ForegroundColor Cyan
Write-Host " GitHub Actions:    https://github.com/ahbiyout-all/WinPurify-Pro/actions" -ForegroundColor Cyan
Write-Host " GitHub Releases:   https://github.com/ahbiyout-all/WinPurify-Pro/releases" -ForegroundColor Cyan
Write-Host "=====================================================================" -ForegroundColor Green
Write-Host ""
pause
