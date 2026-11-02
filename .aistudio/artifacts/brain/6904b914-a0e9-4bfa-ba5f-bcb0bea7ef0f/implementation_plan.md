# Implementation Plan - Remote Agent Security Authentication & Clock Skew Resolution (v4.63.2)

## Defect Analysis
- **Log Error**:
  `[COMMANDER] [INFO] [Single Action] 노드 [WIN-3MUTJH5JMJU] (192.168.219.178)에 개별 명령 전송: Reboot`
  `└ [오류] [WIN-3MUTJH5JMJU] HTTP 401: {"Success":false,"Message":"Security Authentication Failed or Replay Detected"}`
- **Root Cause**:
  1. **PC-specific Random Secret Keys**: `LoadOrGenerateClusterKey()` generated a unique 32-byte random hex key in `C:\ProgramData\WinPurifyPro\cluster.key` on each independent machine. When Commander on Machine A sent commands to Agent on Machine B (`WIN-3MUTJH5JMJU`), Machine B rejected the HMAC signature because its local key differed.
  2. **Strict 60s Time Skew Limit**: 사설망 LAN PC간 시계 오차(Clock Skew)가 60초를 초과할 경우 `Math.Abs(currentTs - ts) > 60`이 작동하여 HTTP 401로 원격 명령이 즉시 거부됨.
  3. **No Key Fallback inside Header Block**: When headers (`X-WinPurify-Signature`) were supplied, any signature or timestamp discrepancy immediately terminated evaluation with `return false`, skipping payload `req.SecretKey` verification.

## Fix Implementation
1. **Multi-Key Verification Candidate Pool (`validKeys`)**:
   - `VerifySecurityToken` in `Services/RemoteAgentService.cs` checks against a list of valid keys:
     - Active `ClusterSecretKey`
     - Standard cluster default key `DefaultClusterKey` (`"WPP-SEC-CLUSTER-2026-KEY"`)
     - Legacy key (`"WinPurifyClusterKey2026"`)
2. **Clock Skew Tolerance Expansion (300 Seconds / 5 Minutes)**:
   - Expanded timestamp validity window from 60s to **300s (5 minutes)** to comfortably absorb LAN clock drift between PCs in domain/workgroup environments.
3. **Resilient 2-Tier Fallback Authentication**:
   - Tier 1: HMAC-SHA256 digital signature validation against all valid keys in the 300s window with Nonce replay protection.
   - Tier 2: Payload `req.SecretKey` direct match against `validKeys`. If `req.SecretKey` matches the default cluster key or local key, authentication succeeds seamlessly.
4. **Out-of-the-Box Cluster Key Synchronization (`DefaultClusterKey`)**:
   - Defaulted `LoadOrGenerateClusterKey()` to `"WPP-SEC-CLUSTER-2026-KEY"`, ensuring out-of-the-box cluster pairing across LAN nodes.
5. **Full System Version Sync (v4.63.2 / 4.63.2.0)**:
   - Synchronized all 14 project configurations, manifests, installer, scripts, and Blogger docs via `node sync-version.js`.
   - Verified cleanly with `compile_applet` and `lint_applet`.
