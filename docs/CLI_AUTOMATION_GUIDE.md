# WinPurify Pro Command-Line Interface (CLI) Guide (`v4.71.1`)

WinPurify Pro provides comprehensive command-line switches for **unattended Fresh Handover sanitization**, scheduled system maintenance, Windows Service management, and Central Commander execution.

---

## 💻 CLI Switches Summary

| Switch | Syntax | Description |
|---|---|---|
| **Clean Handover (Auto)** | `--handover-auto`, `--zero-trace` | Headless execution: scans and purges previous Windows user profiles, multi-profile browser sessions, messenger/office tokens, and OS Credential Vault, then runs 2nd-pass integrity verification and exits (`0` = clean verified, `2` = residual items remain). |
| **Handover Preset: Sessions Only** | `--handover-auto --sessions-only` | Preserves local Windows user accounts while force-signing out and wiping all browser/app sessions and credentials. |
| **Handover Preset: Full Reset** | `--handover-auto --full-reset` | Purges previous user accounts, all app/browser sessions & credentials, AND empties the current user's personal folders (`Desktop`, `Documents`, `Downloads`, `Pictures`, `Videos`). |
| **Scheduled Maintenance Run** | `--schedule-run`, `--silent`, `/silent`, `--automated-run` | Runs the configured Task Scheduler optimization maintenance pass headlessly and exits cleanly. |
| **Service Mode** | `--service`, `-service`, `/service` | Runs the process as the `WinPurifyAgentService` background daemon (Session 0). |
| **Install Service** | `--install-service`, `-install-service`, `/install-service` | Registers and starts `WinPurifyAgentService` via Windows SCM. |
| **Uninstall Service** | `--uninstall-service`, `-uninstall-service`, `/uninstall-service` | Stops and removes `WinPurifyAgentService` from Windows SCM. |
| **Commander Mode** | `--commander`, `-commander`, `/commander` | Launches the dedicated Central Commander Fleet Management GUI (`CommanderMainWindow.xaml`). |
| **Deep Clean** | `--deep`, `-deep`, `/deep` | Enables Deep-risk optimization modules during automated runs. |
| **Auto-Exit** | `--auto-exit`, `-auto-exit` | Closes the application automatically upon finishing CLI-triggered work. |

---

## 🧹 Unattended Clean Handover Examples

### 1. Standard PC Handover (Previous Users + All App/Browser Sessions & Credentials)
```cmd
"C:\Program Files (x86)\WinPurify Pro\WinPurifyPro.exe" --handover-auto
```

### 2. Logout All Sessions & Credentials Only (Keep Accounts)
```cmd
"C:\Program Files (x86)\WinPurify Pro\WinPurifyPro.exe" --handover-auto --sessions-only
```

### 3. Full Machine Transfer Reset (Including Current User's Personal Folders)
```cmd
"C:\Program Files (x86)\WinPurify Pro\WinPurifyPro.exe" --zero-trace --full-reset
```

---

## ⏰ Automated Scheduled Task Example (Windows Task Scheduler)

Create a daily automated maintenance task via an elevated Command Prompt:

```cmd
schtasks /create /tn "WinPurifyPro_AutoMaintenance" /tr "\"C:\Program Files (x86)\WinPurify Pro\WinPurifyPro.exe\" --schedule-run" /sc weekly /d SUN /st 03:00 /rl highest
```
