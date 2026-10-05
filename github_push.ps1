# ==============================================================================
# WinPurify Pro - GitHub Automated Sync & Push Script (PowerShell)
# Author: AhBiYout (Repository: AhBiYout/AhBiYout-all)
# ==============================================================================

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$Host.UI.RawUI.WindowTitle = "WinPurify Pro - GitHub Automated Sync & Push Tool"

Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host " WinPurify Pro - GitHub Automated Sync & Push Pipeline" -ForegroundColor Green
Write-Host " Account:    AhBiYout" -ForegroundColor Yellow
Write-Host " Repository: AhBiYout-all" -ForegroundColor Yellow
Write-Host "=====================================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Version Synchronization
Write-Host "[*] Step 1: Synchronizing version numbers across all 14 project files..." -ForegroundColor Cyan
node sync-version.js
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] sync-version.js failed with exit code $LASTEXITCODE." -ForegroundColor Red
    exit $LASTEXITCODE
}

# 2. Extract current version
$pkgJson = Get-Content -Raw -Path "package.json" | ConvertFrom-Json
$appVersion = $pkgJson.version
Write-Host "[+] Current Synchronized Version: v$appVersion" -ForegroundColor Green
Write-Host ""

# 3. Check and configure Git remote
Write-Host "[*] Step 2: Checking Git remote origin repository..." -ForegroundColor Cyan
$remoteUrl = git remote get-url origin 2>$null
$expectedUrl = "https://github.com/AhBiYout/AhBiYout-all.git"

if (-not $remoteUrl) {
    Write-Host "[!] Remote origin not found. Adding origin $expectedUrl ..." -ForegroundColor Yellow
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
    $customCommit = "Release v$appVersion - WinPurify Pro Full Update (AhBiYout)"
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
Write-Host "[*] Step 6: Pushing branch and tags to GitHub (AhBiYout/AhBiYout-all)..." -ForegroundColor Cyan
git branch -M main
git push -u origin main
if ($LASTEXITCODE -ne 0) {
    Write-Host "[!] Pushing to 'main' branch failed, trying 'master' branch..." -ForegroundColor Yellow
    git push -u origin master
}

git push origin "v$appVersion" --force

Write-Host ""
Write-Host "=====================================================================" -ForegroundColor Green
Write-Host " [SUCCESS] WinPurify Pro v$appVersion successfully pushed to GitHub!" -ForegroundColor Green
Write-Host " GitHub Repository: https://github.com/AhBiYout/AhBiYout-all" -ForegroundColor Cyan
Write-Host " GitHub Actions:    https://github.com/AhBiYout/AhBiYout-all/actions" -ForegroundColor Cyan
Write-Host " GitHub Releases:   https://github.com/AhBiYout/AhBiYout-all/releases" -ForegroundColor Cyan
Write-Host "=====================================================================" -ForegroundColor Green
Write-Host ""
pause
