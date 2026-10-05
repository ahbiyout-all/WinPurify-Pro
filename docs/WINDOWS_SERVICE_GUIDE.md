# WinPurifyAgentService Architecture & Management Guide

This document details the background **Windows Service (`WinPurifyAgentService`)** architecture in WinPurify Pro, enabling 24/7 autonomous fleet maintenance and real-time remote commands from **WinPurify Central Commander**.

---

## 🏛️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                 Central Commander Console                   │
│              (WinPurifyCommander.exe / WPF)                 │
└──────────────────────────────┬──────────────────────────────┘
                               │
            ┌──────────────────┴──────────────────┐
            │  HMAC-SHA256 Encrypted Protocol      │
            │  • TCP 9870: REST & Action Endpoint │
            │  • UDP 9871: Discovery Broadcast    │
            └──────────────────┬──────────────────┘
                               ▼
┌─────────────────────────────────────────────────────────────┐
│                       Client Endpoint                       │
├──────────────────────────────┬──────────────────────────────┤
│  Mode 1: Windows Service     │  Mode 2: Interactive Desktop │
│  (WinPurifyAgentService)     │  (WinPurifyPro.exe GUI)      │
│  - Session 0 Background      │  - User Interactive Session  │
│  - Starts with OS Boot       │  - Real-time HUD Toast popup │
│  - LocalSystem Privileges    │  - Visual Progress Indicator │
└──────────────────────────────┴──────────────────────────────┘
```

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
:: Install and Start Service
"C:\Program Files (x86)\WinPurify Pro\WinPurifyPro.exe" --install-service

:: Stop and Uninstall Service
"C:\Program Files (x86)\WinPurify Pro\WinPurifyPro.exe" --uninstall-service

:: Run Service Interactively (Console Debug Mode)
"C:\Program Files (x86)\WinPurify Pro\WinPurifyPro.exe" --service
```

### 2. Windows Native `sc.exe` Commands
```cmd
:: Query Service Status
sc query WinPurifyAgentService

:: Start Service
sc start WinPurifyAgentService

:: Stop Service
sc stop WinPurifyAgentService

:: Delete Service
sc delete WinPurifyAgentService
```

### 3. PowerShell Service Cmdlets
```powershell
Get-Service -Name WinPurifyAgentService
Start-Service -Name WinPurifyAgentService
Stop-Service -Name WinPurifyAgentService
Restart-Service -Name WinPurifyAgentService
```
