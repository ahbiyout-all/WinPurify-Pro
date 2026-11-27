# Security Summary

**Reviewed Files**: 22
**Issues Resolved**: 5 (plus 1 dev-only dependency advisory noted for follow-up)

### Issues Resolved
| Area | File | Line | What Was Happening | Impact | Status | How It Was Fixed |
|---|---|---|---|---|---|---|
| Static Path Confinement & Origin Checks | `dev-server.js` | 229, 427 | Static asset handler checked `!filePath.startsWith(rootDir)` without a trailing path separator, did not guard malformed URI encodings/null bytes or dotfiles/source folders, allowed wildcard `*` CORS, and had unbounded request body reads. | High | Fixed | Added `rootDir + path.sep` path boundary checks, safe `decodeURIComponent` + null-byte rejection, dotfile and source/log directory blocklists, extension allowlisting, 1MB body size cap, localhost/same-host origin checks, and cross-site API blocking. |
| Shell Quoting in Credential & Registry Cleanup | `Services/FreshHandoverService.cs` | 844, 876, 888, 1077 | Credential deletion passed target names into `cmd.exe /c cmdkey /delete:"..."`, and hive unload/CIM cleanup spawned `cmd.exe /c` command strings. | High | Fixed | Replaced `cmdkey` shell calls with direct Win32 `Advapi32.dll` (`CredEnumerateW`/`CredDeleteW`/`CredFree`) P/Invoke calls, enforced strict domain SID regex validation, and switched remaining `reg.exe`/`net.exe`/PowerShell calls to direct execution via `ProcessStartInfo.ArgumentList` with no shell. |
| Incomplete Handover Path & Symlink Guards | `electron-ipc-shared.js` | 673, 764 | `execute-handover-purge` did not block `os.homedir()`, drive roots, `userData`, `defaultuser0`, or symlinks, and deleting personal folders deleted the folder itself rather than emptying contents. | High | Fixed | Added exact/prefix protections for `os.homedir()`, OS roots, `SystemRoot`, `ProgramFiles`, `userData`, `defaultuser0`, and symlink skipping via `lstatSync`; separated personal folder cleanup into `emptyRootPaths`. |
| Missing Challenge-Response on Remote Handover | `electron-ipc-shared.js` | 572 | `FreshHandover` remote batch command was omitted from the sensitive command list and skipped the one-time Challenge-Response Nonce handshake. | Medium | Fixed | Added `'FreshHandover'` to `isSensitive` so remote Clean Handover commands enforce the IP-bound Challenge-Response handshake. |
| Handover Verification Reporting & DTO Mismatch | `Services/FreshHandoverService.cs`, `Services/RemoteAgentService.cs`, `index.tsx` | 916, 900, 4339 | Step 4 verification read optimistically zeroed counts rather than re-scanning disk/credentials, Electron UI replaced clean results with demo items and ignored IPC errors, and Commander sent `preset` instead of `presetName`. | Medium | Fixed | Implemented real 2nd-pass disk/credential re-scanning in C# and Electron, propagated verification errors accurately, preserved clean empty results, and aligned `PresetName`/`Preset` handling. |
| Build-Time Dev Dependencies | `package.json` | 48 | `bun audit` reported upstream transitive advisories inside dev-only packaging packages (`electron@28.2.0`, `electron-builder@24.13.3`, `@tailwindcss/cli`). Runtime UI packages (`react`, `react-dom`, `d3`, `recharts`, `lucide-react`, `esbuild`) have 0 findings. | Low | Needs Follow-Up | Left build-chain pins intact to preserve packaging compatibility; scheduled Electron 33+ / `electron-builder` major upgrade during dedicated Windows packaging validation. |

### Safeguards Verified
| Area | File | How It's Protected |
|---|---|---|
| Transport & Certificate Pinning | `Services/RemoteTlsServer.cs`, `electron-ipc-shared.js` | Embedded ECDSA P-256 TLS 1.2/1.3 listener on TCP 9874 with client-side Trust-On-First-Use (TOFU) SHA-256 certificate pinning. |
| Cluster Key Storage | `Services/ClusterSecurityVault.cs`, `electron-ipc-shared.js` | 256-bit random cluster keys stored via Windows DPAPI (`LocalMachine` + `SYSTEM`/`Administrators` ACL) and Electron `safeStorage`; hardcoded fallback keys and plaintext `secretKey` transmissions are rejected. |
| Replay & Cross-Host Auth Protection | `Services/RemoteAuthValidator.cs` | Enforces ±60s timestamp windows, single-use Nonce deduplication, HMAC-SHA256 signatures, and client IP binding on issued Challenge Nonces. |
| Self-Preservation & Account Safety | `Services/FreshHandoverService.cs` | Permanently excludes `WinPurifyPro` install/data directories, current active user SID/session, and Windows protected system profiles (`SYSTEM`, `Default`, `Public`, `defaultuser0`, `WDAGUtilityAccount`). |

---

## PoC Verification

### 1. Dev-Server Sibling Directory & Dotfile Traversal Attempt
#### Summary
| Field | Value |
|---|---|
| Issue Type | Path Traversal & Sensitive File Exposure |
| Impact | High |
| Affected File | `dev-server.js:465-499` |
| Test Scenario | `GET /.git/config`, `GET /Services/ClusterSecurityVault.cs`, or `GET /../applet-sibling/secret.txt` |

#### How It Was Fixed
Decoded URL paths with malformed/null-byte rejection, blocked dotfiles and non-web backend directories/files, enforced an explicit web asset extension allowlist, and verified canonical path containment using `rootDir + path.sep`.

#### Step-by-Step Trace
| Step | What Happens in the Code | Outcome |
|---|---|---|
| 1 | Incoming URL enters static file handler at `dev-server.js:459` | Decoded via `decodeURIComponent` and checked for `\0` |
| 2 | Segment & file policy check at `dev-server.js:478-493` | `.git` starts with `.` and `Services` is in `blockedTopDirs` |
| 3 | Boundary check at `dev-server.js:495-499` | Any resolved path outside `rootDir + path.sep` fails containment |
| 4 | Final HTTP response | **403 Access Denied** |

#### Conclusion
**Fix Verified** - Repository metadata, environment files, C# source files, and sibling directories cannot be read through the local development server.

---

### 2. Special-Character Credential Target Name in Handover Cleanup
#### Summary
| Field | Value |
|---|---|
| Issue Type | Shell Quoting / Command Injection Risk |
| Impact | High |
| Affected File | `Services/FreshHandoverService.cs:1054-1102` |
| Test Scenario | A Windows Credential Vault entry whose `TargetName` contains quotes and shell metacharacters (e.g., `corp" & calc.exe & "`) |

#### How It Was Fixed
Replaced `cmd.exe /c cmdkey /delete:"..."` shell execution with direct Win32 `Advapi32.dll` API calls (`CredEnumerateW` and `CredDeleteW`), and switched registry/network cleanup rules to `ProcessStartInfo.ArgumentList` with no shell interpreter.

#### Step-by-Step Trace
| Step | What Happens in the Code | Outcome |
|---|---|---|
| 1 | `DeleteCredentialsByKeywords` runs at `Services/FreshHandoverService.cs:1054` | Enumerates credentials directly in memory via `CredEnumerateW` |
| 2 | Matches `TargetName` string against selected keywords | Stores `(target, cred.Type)` without building any shell string |
| 3 | Calls `CredDeleteW(item.Target, item.Type, 0)` | Passed directly as a Unicode string parameter to Windows LSASS |
| 4 | Final result | **Handled Safely (No Shell Invoked)** |

#### Conclusion
**Fix Verified** - Arbitrary characters in credential target names can never trigger shell command interpretation.

---

### 3. Electron Handover Home Root & Symlink Protection
#### Summary
| Field | Value |
|---|---|
| Issue Type | Arbitrary / Unsafe Local Path Deletion |
| Impact | High |
| Affected File | `electron-ipc-shared.js:759-838` |
| Test Scenario | IPC call `execute-handover-purge` with `targetPaths: [os.homedir(), "C:\\Windows", "<symlink-to-system>"]` |

#### How It Was Fixed
Added exact and prefix protection rules covering `os.homedir()`, OS roots, `SystemRoot`, `ProgramFiles`, `userData`, `defaultuser0`, and `fs.lstatSync().isSymbolicLink()` detection before calling `fs.rmSync`, while isolating personal folder cleanup into `emptyRootPaths`.

#### Step-by-Step Trace
| Step | What Happens in the Code | Outcome |
|---|---|---|
| 1 | IPC payload arrives at `execute-handover-purge` (`electron-ipc-shared.js:759`) | Each candidate is canonicalized via `path.resolve(rawPath)` |
| 2 | `checkProtectedReason(resolved)` runs at `electron-ipc-shared.js:800-817` | Matches `homeLower` / `protectedPrefixesLower` or `lstatSync().isSymbolicLink()` |
| 3 | Protected target is rejected and appended to `errors` | `fs.rmSync` is skipped |
| 4 | Final IPC result | **Handled Safely (`errors` recorded, OS/home untouched)** |

#### Conclusion
**Fix Verified** - Current user home roots, OS directories, WinPurify binaries, and symlinks are blocked from destructive removal.

---

### 4. Remote Clean Handover Challenge-Response Enforcement
#### Summary
| Field | Value |
|---|---|
| Issue Type | Missing Replay/Host-Binding Handshake on High-Impact Remote Command |
| Impact | Medium |
| Affected File | `electron-ipc-shared.js:572-626` |
| Test Scenario | Central Commander triggers `commandType: "FreshHandover"` against a managed LAN node |

#### How It Was Fixed
Added `'FreshHandover'` to `isSensitive` in `electron-ipc-shared.js` so the client automatically requests an IP-bound `ChallengeNonce` from `/api/v1/auth/challenge` and binds the HMAC-SHA256 signature to `${challengeNonce}:FreshHandover:${rawJson}`.

#### Step-by-Step Trace
| Step | What Happens in the Code | Outcome |
|---|---|---|
| 1 | `send-batch-command` inspects `safeInput.commandType` (`electron-ipc-shared.js:572`) | Matches `'FreshHandover'` -> `isSensitive = true` |
| 2 | Requests `/api/v1/auth/challenge` before sending command | Receives IP-bound `challengeNonce` |
| 3 | `RemoteAuthValidator.VerifySecurityToken` checks nonce + IP + HMAC | Executes only on valid, non-replayed signature |
| 4 | Final result | **Handled Safely** |

#### Conclusion
**Fix Verified** - Remote Clean Handover commands enforce the same single-use Challenge-Response protection as system power and forensic wipe actions.
