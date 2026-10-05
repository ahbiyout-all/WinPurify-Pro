# WinPurify Pro Enterprise Unattended Installation Guide

This guide covers silent, non-interactive (unattended) deployment strategies for **WinPurify Pro** across corporate and fleet environments via IT automation tools such as Microsoft Intune, SCCM (MECM), Group Policy (GPO), PDQ Deploy, and Ansible.

---

## 🚀 Quick Command Reference

| Deployment Scenario | Command Line | Description |
|---|---|---|
| **Standard Silent Install (Recommended)** | `WinPurifyPro_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-` | Installs completely in the background with zero UI or user prompts. Automatically registers and starts `WinPurifyAgentService`. |
| **Progress-Bar Only Silent Install** | `WinPurifyPro_Setup.exe /SILENT /SUPPRESSMSGBOXES /NORESTART /SP-` | Shows a minimal progress bar window without requiring user confirmation. |
| **Custom Installation Directory** | `WinPurifyPro_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /DIR="D:\Apps\WinPurifyPro"` | Overrides the default install location (`C:\Program Files (x86)\WinPurify Pro`). |
| **Silent Logged Installation** | `WinPurifyPro_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /LOG="C:\Logs\WinPurify_Install.log"` | Emits a complete installation log file for deployment verification. |
| **Clean Reinstall (Auto-Uninstall Previous)** | `WinPurifyPro_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /CLEAN=1` | Silently removes any existing legacy version prior to installing the current version. |
| **Portable / No-Service Install** | `WinPurifyPro_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /NOSERVICE=1` | Bypasses background Windows Service registration for isolated or kiosk endpoints. |
| **Auto-Launch Post-Install** | `WinPurifyPro_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /AUTORUN=1` | Automatically launches `WinPurifyPro.exe` immediately after installation. |
| **No Desktop Icon** | `WinPurifyPro_Setup.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /TASKS=""` | Suppresses creation of the desktop shortcut icon. |
| **Silent Uninstallation** | `"C:\Program Files (x86)\WinPurify Pro\unins000.exe" /VERYSILENT /SUPPRESSMSGBOXES /NORESTART` | Removes the software and unregisters all services silently. |

---

## 🛠️ Automated Enterprise Deployment Scripts

### 1. PowerShell Script (`deploy-winpurify.ps1`)
```powershell
<#
.SYNOPSIS
    Automated Enterprise Installer for WinPurify Pro
.DESCRIPTION
    Installs WinPurify Pro silently, verifies Windows Service registration, and opens necessary firewall ports.
#>
param (
    [string]$SetupPath = ".\WinPurifyPro_Setup.exe",
    [string]$InstallDir = "$env:ProgramFiles (x86)\WinPurify Pro",
    [switch]$NoService,
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $SetupPath)) {
    Write-Error "[WinPurify Deploy] Installer not found at: $SetupPath"
    exit 1
}

$Arguments = @("/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART", "/SP-")
if ($InstallDir) { $Arguments += "/DIR=`"$InstallDir`"" }
if ($Clean) { $Arguments += "/CLEAN=1" }
if ($NoService) { $Arguments += "/NOSERVICE=1" }

Write-Host "[WinPurify Deploy] Executing silent installation..." -ForegroundColor Cyan
$Process = Start-Process -FilePath $SetupPath -ArgumentList $Arguments -Wait -PassThru

if ($Process.ExitCode -ne 0) {
    Write-Error "[WinPurify Deploy] Installer failed with exit code: $($Process.ExitCode)"
    exit $Process.ExitCode
}

Write-Host "[WinPurify Deploy] Installation completed successfully." -ForegroundColor Green

# Verify Windows Service state
if (-not $NoService) {
    $Service = Get-Service -Name "WinPurifyAgentService" -ErrorAction SilentlyContinue
    if ($Service) {
        Write-Host "[WinPurify Deploy] Service 'WinPurifyAgentService' Status: $($Service.Status)" -ForegroundColor Green
    } else {
        Write-Warning "[WinPurify Deploy] Service not found. Attempting self-healing registration..."
        $AppExe = Join-Path $InstallDir "WinPurifyPro.exe"
        if (Test-Path $AppExe) {
            Start-Process -FilePath $AppExe -ArgumentList "--install-service" -Wait
        }
    }
}
```

---

## 🛡️ Firewall & Network Configuration for Central Commander

WinPurify Pro automatically provisions inbound and outbound Windows Firewall rules during setup. For corporate network administrators configuring hardware firewalls or domain GPOs:

- **TCP Port 9870**: REST API & Command Receiver (Inbound on Client PC)
- **UDP Port 9871**: LAN Auto-Discovery Broadcast & Beacon
- **Security**: HMAC-SHA256 Token Authentication & TLS Encryption
