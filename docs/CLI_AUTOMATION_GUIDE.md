# WinPurify Pro Command-Line Interface (CLI) Guide

WinPurify Pro provides comprehensive command-line switches for automated system maintenance, CI/CD runners, scheduled tasks, and headless operations.

---

## 💻 CLI Switches Summary

| Switch | Syntax | Description |
|---|---|---|
| **Service Mode** | `--service`, `-service`, `/service` | Runs the process as a background service daemon. |
| **Install Service** | `--install-service` | Automatically registers and starts `WinPurifyAgentService` via SCM. |
| **Uninstall Service** | `--uninstall-service` | Stops and deletes `WinPurifyAgentService` from SCM. |
| **Commander Mode** | `--commander`, `-commander`, `/commander` | Launches the dedicated Central Commander Fleet Management GUI. |
| **Silent Quick Purge** | `--silent`, `-silent`, `/silent` | Runs a standard system purge in the background and terminates upon completion. |
| **Deep Clean** | `--deep`, `-deep`, `/deep` | Executes deep junk purge including Windows Component Store (`WinSxS`) analysis. |
| **Auto-Exit** | `--auto-exit`, `-auto-exit` | Automatically closes the desktop window after completing an optimization pass. |

---

## ⏰ Automated Scheduled Task Example (Windows Task Scheduler)

Create a daily automated maintenance task via administrative Command Prompt:

```cmd
schtasks /create /tn "WinPurifyDailyClean" /tr "\"C:\Program Files (x86)\WinPurify Pro\WinPurifyPro.exe\" --silent --deep" /sc daily /st 03:00 /rl highest
```
