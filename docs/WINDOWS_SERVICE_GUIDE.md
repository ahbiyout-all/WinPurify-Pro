# WinPurifyAgentService Architecture & Management Guide (`v4.71.1`)

This document details the background **Windows Service (`WinPurifyAgentService`)** architecture in WinPurify Pro, enabling 24/7 autonomous fleet maintenance and Zero-Trust remote management from **WinPurify Central Commander** (WPF or Electron Desktop Console).

---

## 🏛️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                 Central Commander Console                   │
│         (WinPurifyCommander.exe / Electron Desktop)         │
└──────────────────────────────┬──────────────────────────────┘
                               │
            ┌──────────────────┴──────────────────────┐
            │  TLS 1.2/1.3 + HMAC-SHA256 + Nonce      │
            │  • TCP 9874: Primary HTTPS TLS Endpoint │
            │  • TCP 9870: Legacy HTTP Compatibility  │
            │  • UDP 9871: LAN Auto-Discovery Beacon  │
            └──────────────────┬──────────────────────┘
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                       Client Endpoint                       │
├──────────────────────────────┬──────────────────────────────┤
│  Mode 1: Windows Service     │  Mode 2: Interactive Desktop │
│  (WinPurifyAgentService)     │  (WinPurifyPro.exe GUI)      │
│  - Session 0 Background      │  - User Interactive Session  │
│  - Starts with OS Boot       │  - Clean Handover Workspace  │
│  - LocalSystem Privileges    │  - Real-time HUD & Tray      │
│  - Disabled in Portable Mode │  - DPAPI Cluster Key Vault   │
└──────────────────────────────┴──────────────────────────────┘
```

> **Note**: For security, `RemoteAgentService` does **not** listen on network ports when launched as a standalone Portable executable (`AppEnvironment.IsPortable == true`). Remote command reception runs in installed Desktop / `WinPurifyAgentService` environments.

---

## ⚙️ Service Specifications

- **Service Name**: `WinPurifyAgentService`
- **Display Name**: `WinPurify Pro Remote Agent Service`
- **Description**: `WinPurify Pro 중앙 관제(Central Commander) 원격 최적화 및 유지보수 명령 수신 백그라운드 서비스`
- **Binary Path**: `"C:\Program Files (x86)\WinPurify Pro\WinPurifyPro.exe" --service`
- **Startup Type**: Automatic (`SERVICE_AUTO_START`)
- **Account**: `NT AUTHORITY\LocalSystem`

---

## 🔧 Service Management Commands

### 1. Built-in Application CLI Commands
```cmd
:: Install and Start Service (automatically provisions TCP 9874/9870 & UDP 9871 firewall rules)
"C:\Program Files (x86)\WinPurify Pro\WinPurifyPro.exe" --install-service

:: Stop and Uninstall Service
"C:\Program Files (x86)\WinPurify Pro\WinPurifyPro.exe" --uninstall-service

:: Run Service Interactively (Console Debug Mode)
"C:\Program Files (x86)\WinPurify Pro\WinPurifyPro.exe" --service
```

### 2. Windows Native `sc.exe` Commands
```cmd
sc query WinPurifyAgentService
sc start WinPurifyAgentService
sc stop WinPurifyAgentService
sc delete WinPurifyAgentService
```

### 3. PowerShell Service Cmdlets
```powershell
Get-Service -Name WinPurifyAgentService
Start-Service -Name WinPurifyAgentService
Stop-Service -Name WinPurifyAgentService
Restart-Service -Name WinPurifyAgentService
```
