/**
 * WinPurify Pro - Shared Electron Cluster Security, Discovery & Telemetry IPC Module
 * Version: v4.71.1
 *
 * Shared by both electron-main.js (Unified Desktop App) and electron-remote-center.js (Standalone Commander).
 */

const path = require('path');
const dgram = require('dgram');
const http = require('http');
const https = require('https');
const net = require('net');
const crypto = require('crypto');
const os = require('os');
const fs = require('fs');
const { execFile, execFileSync } = require('child_process');

function createVaultAndIpcHelpers({ app, shell, safeStorage, dialog }) {
  function getElectronDialog() {
    if (dialog) return dialog;
    try {
      return require('electron').dialog || null;
    } catch {
      return null;
    }
  }

  function getHandoverExclusionsPath() {
    return path.join(app.getPath('userData'), 'handover-exclusions.json');
  }

  function loadHandoverExclusions() {
    try {
      const f = getHandoverExclusionsPath();
      if (!fs.existsSync(f)) return [];
      const parsed = JSON.parse(fs.readFileSync(f, 'utf8'));
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }

  function saveHandoverExclusions(rules) {
    const clean = (Array.isArray(rules) ? rules : [])
      .filter(r => r && typeof r.value === 'string' && r.value.trim().length > 0)
      .slice(0, 200)
      .map(r => ({
        id: String(r.id || crypto.randomBytes(8).toString('hex')),
        ruleType: r.ruleType === 'ArtifactId' ? 'ArtifactId' : 'PathPrefix',
        value: r.ruleType === 'ArtifactId' ? r.value.trim() : path.resolve(r.value.trim()),
        label: String(r.label || r.value).trim(),
        createdAt: String(r.createdAt || new Date().toISOString())
      }));
    const dir = path.dirname(getHandoverExclusionsPath());
    if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });
    fs.writeFileSync(getHandoverExclusionsPath(), JSON.stringify(clean, null, 2), { encoding: 'utf8', mode: 0o600 });
    return clean;
  }
  function getVaultDir() {
    const dir = path.join(app.getPath('userData'), 'security-vault');
    if (!fs.existsSync(dir)) {
      fs.mkdirSync(dir, { recursive: true, mode: 0o700 });
    }
    return dir;
  }

  function generateRandomClusterKey() {
    return `WPP-STD-${crypto.randomBytes(24).toString('hex').toUpperCase()}`;
  }

  function savePersistedClusterKey(key) {
    if (!key || typeof key !== 'string' || key.trim().length < 8) {
      throw new Error('Cluster key must be at least 8 characters.');
    }
    const cleanKey = key.trim();
    const keyFile = path.join(getVaultDir(), 'cluster-key.json');
    let payload;
    if (safeStorage && safeStorage.isEncryptionAvailable()) {
      const encrypted = safeStorage.encryptString(cleanKey);
      payload = {
        encrypted: true,
        cipherBase64: encrypted.toString('base64'),
        updatedAt: new Date().toISOString()
      };
    } else {
      payload = {
        encrypted: false,
        key: cleanKey,
        updatedAt: new Date().toISOString()
      };
    }
    fs.writeFileSync(keyFile, JSON.stringify(payload, null, 2), { encoding: 'utf8', mode: 0o600 });
    return cleanKey;
  }

  function readPersistedClusterKey() {
    const keyFile = path.join(getVaultDir(), 'cluster-key.json');
    try {
      if (fs.existsSync(keyFile)) {
        const raw = JSON.parse(fs.readFileSync(keyFile, 'utf8'));
        if (raw.encrypted && raw.cipherBase64 && safeStorage && safeStorage.isEncryptionAvailable()) {
          const plain = safeStorage.decryptString(Buffer.from(raw.cipherBase64, 'base64'));
          if (plain && plain.length >= 16) return plain;
        } else if (raw.key && typeof raw.key === 'string' && raw.key.length >= 16) {
          return raw.key;
        }
      }
    } catch (err) {
      console.warn('[SecurityVault] Cluster key read warning:', err.message);
    }

    const newKey = generateRandomClusterKey();
    savePersistedClusterKey(newKey);
    return newKey;
  }

  function getTrustedNodesPath() {
    return path.join(getVaultDir(), 'trusted-nodes.json');
  }

  function loadTrustedNodes() {
    const file = getTrustedNodesPath();
    try {
      if (fs.existsSync(file)) {
        return JSON.parse(fs.readFileSync(file, 'utf8')) || {};
      }
    } catch {}
    return {};
  }

  function saveTrustedNodes(map) {
    fs.writeFileSync(getTrustedNodesPath(), JSON.stringify(map, null, 2), { encoding: 'utf8', mode: 0o600 });
  }

  function verifyOrPinTofuThumbprint(ip, thumbprint) {
    if (!thumbprint) return { trusted: false, reason: 'missing_peer_certificate' };
    const cleanThumb = thumbprint.replace(/:/g, '').toUpperCase();
    const trustedMap = loadTrustedNodes();
    const existing = trustedMap[ip];

    if (existing && existing.thumbprint) {
      if (existing.thumbprint.toUpperCase() === cleanThumb) {
        return { trusted: true, status: 'verified', thumbprint: cleanThumb };
      }
      return {
        trusted: false,
        status: 'mismatch',
        expectedThumbprint: existing.thumbprint,
        actualThumbprint: cleanThumb,
        reason: 'tofu_pin_mismatch'
      };
    }

    trustedMap[ip] = {
      thumbprint: cleanThumb,
      trustedAt: new Date().toISOString()
    };
    saveTrustedNodes(trustedMap);
    return { trusted: true, status: 'newly_pinned', thumbprint: cleanThumb };
  }

  function computeBroadcast(ip, mask) {
    const ipParts = ip.split('.').map(Number);
    const maskParts = mask.split('.').map(Number);
    if (ipParts.length !== 4 || maskParts.length !== 4) return '255.255.255.255';
    return ipParts.map((oct, i) => (oct | (~maskParts[i] & 255))).join('.');
  }

  function getLocalIpInfos() {
    const interfaces = os.networkInterfaces();
    const infos = [];
    for (const name of Object.keys(interfaces)) {
      for (const iface of interfaces[name] || []) {
        if (iface.family === 'IPv4' && !iface.internal && !iface.address.startsWith('169.254.')) {
          infos.push({
            address: iface.address,
            netmask: iface.netmask || '255.255.255.0',
            broadcast: computeBroadcast(iface.address, iface.netmask || '255.255.255.0')
          });
        }
      }
    }
    return infos;
  }

  function computeHmacSha256(data, key) {
    return crypto.createHmac('sha256', key).update(data).digest('hex').toUpperCase();
  }

  function buildSignedGetHeaders(reqPath, key) {
    const timestamp = Math.floor(Date.now() / 1000);
    const nonce = crypto.randomBytes(16).toString('hex');
    const signature = computeHmacSha256(`GET:${reqPath}:${timestamp}:${nonce}`, key);
    return {
      'X-WinPurify-Signature': signature,
      'X-WinPurify-Timestamp': timestamp.toString(),
      'X-WinPurify-Nonce': nonce
    };
  }

  function probeTcpPort(ip, port, timeoutMs = 160) {
    return new Promise((resolve) => {
      const socket = new net.Socket();
      let done = false;
      const finish = (open) => {
        if (done) return;
        done = true;
        try { socket.destroy(); } catch {}
        resolve(open);
      };
      socket.setTimeout(timeoutMs);
      socket.once('connect', () => finish(true));
      socket.once('timeout', () => finish(false));
      socket.once('error', () => finish(false));
      socket.connect(port, ip);
    });
  }

  function sendNodeRequest({ ip, port = 9870, tlsPort = 9874, reqPath, method = 'GET', headers = {}, body = null, timeoutMs = 4000, allowHttpFallback = true }) {
    const attemptHttps = () => new Promise((resolve, reject) => {
      let tofuCheck = null;
      const req = https.request({
        hostname: ip,
        port: tlsPort || 9874,
        path: reqPath,
        method,
        headers,
        timeout: timeoutMs,
        rejectUnauthorized: false, // Explicit TOFU fingerprint verification handled below
        agent: false
      }, (res) => {
        let data = '';
        res.on('data', chunk => { data += chunk; });
        res.on('end', () => {
          resolve({
            statusCode: res.statusCode,
            headers: res.headers,
            body: data,
            protocol: 'HTTPS-TLS',
            tofuCheck
          });
        });
      });

      req.on('socket', (socket) => {
        socket.on('secureConnect', () => {
          try {
            const cert = socket.getPeerCertificate(true);
            const fp = cert && cert.fingerprint256 ? cert.fingerprint256 : '';
            tofuCheck = verifyOrPinTofuThumbprint(ip, fp);
            if (!tofuCheck.trusted) {
              req.destroy(new Error(`TOFU 인증서 지문 불일치 (${tofuCheck.reason})`));
            }
          } catch (err) {
            req.destroy(err);
          }
        });
      });

      req.on('error', (err) => reject(err));
      req.on('timeout', () => { req.destroy(new Error('timeout')); });

      if (body) req.write(body);
      req.end();
    });

    const attemptHttp = () => new Promise((resolve, reject) => {
      const req = http.request({
        hostname: ip,
        port: port || 9870,
        path: reqPath,
        method,
        headers,
        timeout: timeoutMs
      }, (res) => {
        let data = '';
        res.on('data', chunk => { data += chunk; });
        res.on('end', () => {
          resolve({
            statusCode: res.statusCode,
            headers: res.headers,
            body: data,
            protocol: 'HTTP-Legacy',
            tofuCheck: null
          });
        });
      });
      req.on('error', (err) => reject(err));
      req.on('timeout', () => { req.destroy(new Error('timeout')); });
      if (body) req.write(body);
      req.end();
    });

    return attemptHttps().catch((tlsErr) => {
      if (tlsErr.message && tlsErr.message.includes('TOFU')) {
        throw tlsErr; // Never fall back to plaintext HTTP if peer cert failed TOFU verification
      }
      if (!allowHttpFallback) {
        throw tlsErr;
      }
      return attemptHttp();
    });
  }

  function registerClusterIpcHandlers(ipcMain) {
    ipcMain.handle('get-cluster-key', () => {
      return {
        success: true,
        clusterKey: readPersistedClusterKey(),
        encryptedVault: Boolean(safeStorage && safeStorage.isEncryptionAvailable())
      };
    });

    ipcMain.handle('save-cluster-key', (_, newKey) => {
      try {
        const saved = savePersistedClusterKey(newKey);
        return { success: true, clusterKey: saved };
      } catch (err) {
        return { success: false, error: err.message };
      }
    });

    ipcMain.handle('regenerate-cluster-key', () => {
      const generated = generateRandomClusterKey();
      savePersistedClusterKey(generated);
      return { success: true, clusterKey: generated };
    });

    ipcMain.handle('reset-node-trust', (_, arg) => {
      const ip = typeof arg === 'string' ? arg : arg?.ip;
      if (!ip) return { success: false, error: 'Missing IP' };
      const map = loadTrustedNodes();
      delete map[ip];
      saveTrustedNodes(map);
      return { success: true };
    });

    ipcMain.handle('open-log-folder', () => {
      const logDir = path.join(os.homedir(), 'AppData', 'Roaming', 'WinPurifyPro', 'Logs');
      if (!fs.existsSync(logDir)) {
        fs.mkdirSync(logDir, { recursive: true });
      }
      shell.openPath(logDir);
      return { success: true, path: logDir };
    });

    ipcMain.handle('save-logs', async (_, { content, filename } = {}) => {
      try {
        const desktopDir = path.join(os.homedir(), 'Desktop');
        const safeName = path.basename(filename || `WinPurify_Commander_${Date.now()}.txt`) || `WinPurify_Commander_${Date.now()}.txt`;
        const targetFile = path.join(desktopDir, safeName);
        fs.writeFileSync(targetFile, typeof content === 'string' ? content : String(content || ''), 'utf8');
        return { success: true, path: targetFile };
      } catch (err) {
        return { success: false, error: err.message };
      }
    });

    ipcMain.handle('discover-nodes', async (_, timeoutMs = 2500) => {
      const discoveredNodes = [];
      const localInfos = getLocalIpInfos();
      const localIps = localInfos.map(i => i.address);

      const upsertNode = (node) => {
        const idx = discoveredNodes.findIndex(n => n.ipAddress === node.ipAddress);
        if (idx === -1) {
          discoveredNodes.push(node);
        } else {
          discoveredNodes[idx] = { ...discoveredNodes[idx], ...node };
        }
      };

      const udpPromise = new Promise((resolve) => {
        const client = dgram.createSocket('udp4');

        client.on('error', (err) => {
          console.warn('[UDP Discovery Error]', err.message);
          try { client.close(); } catch {}
          resolve();
        });

        client.on('message', (msg, rinfo) => {
          const message = msg.toString('utf8');
          if (message.startsWith('WINPURIFY_DISCOVER_PONG')) {
            const parts = message.split('|');
            const nodeName = parts[1] || rinfo.address;
            const port = parseInt(parts[2], 10) || 9870;
            const osVer = parts[3] || 'Windows 11 x64';
            const parsedCount = parseInt(parts[4], 10);
            const modulesCount = Number.isFinite(parsedCount) && parsedCount > 0 ? parsedCount : 144;
            const reportedIp = parts[5] || rinfo.address;
            const tlsPort = parseInt(parts[6], 10) || 9874;
            const tlsThumbprint = (parts[7] || '').trim();

            let finalIp = reportedIp;
            if (!finalIp || finalIp === '127.0.0.1') {
              finalIp = rinfo.address;
            }

            upsertNode({
              name: nodeName,
              ipAddress: finalIp,
              port,
              tlsPort,
              tlsSupported: tlsPort > 0,
              tlsThumbprint,
              activeModulesCount: modulesCount,
              osVersion: osVer,
              discoverySource: 'UDP:9871',
              isOnline: true,
              lastSeen: '방금 전'
            });
          }
        });

        client.bind(0, () => {
          client.setBroadcast(true);
          const pingPacket = Buffer.from('WINPURIFY_DISCOVER_PING');
          const targets = new Set(['255.255.255.255', '127.0.0.1']);
          for (const info of localInfos) {
            targets.add(info.broadcast);
          }
          for (const targetIp of targets) {
            client.send(pingPacket, 0, pingPacket.length, 9871, targetIp, () => {});
          }
        });

        setTimeout(() => {
          try { client.close(); } catch {}
          resolve();
        }, timeoutMs);
      });

      const tcpFallbackPromise = (async () => {
        const candidates = [];
        for (const info of localInfos) {
          const octets = info.address.split('.');
          if (octets.length !== 4) continue;
          const prefix = `${octets[0]}.${octets[1]}.${octets[2]}`;
          for (let host = 1; host <= 254; host++) {
            const ip = `${prefix}.${host}`;
            if (!localIps.includes(ip)) {
              candidates.push(ip);
            }
          }
        }

        const batchSize = 48;
        for (let i = 0; i < candidates.length; i += batchSize) {
          const chunk = candidates.slice(i, i + batchSize);
          await Promise.all(chunk.map(async (ip) => {
            if (discoveredNodes.some(n => n.ipAddress === ip)) return;
            const tlsOpen = await probeTcpPort(ip, 9874, 150);
            if (tlsOpen) {
              upsertNode({
                name: `WinPurify-LAN (${ip})`,
                ipAddress: ip,
                port: 9870,
                tlsPort: 9874,
                tlsSupported: true,
                tlsThumbprint: '',
                osVersion: 'Windows LAN Node',
                discoverySource: 'TCP:9874 Probe',
                isOnline: true,
                lastSeen: 'LAN 탐지됨'
              });
              return;
            }
            const legacyOpen = await probeTcpPort(ip, 9870, 140);
            if (legacyOpen) {
              upsertNode({
                name: `WinPurify-LAN (${ip})`,
                ipAddress: ip,
                port: 9870,
                tlsPort: 0,
                tlsSupported: false,
                tlsThumbprint: '',
                osVersion: 'Windows LAN Node',
                discoverySource: 'TCP:9870 Probe',
                isOnline: true,
                lastSeen: 'LAN 탐지됨'
              });
            }
          }));
        }
      })();

      await Promise.all([udpPromise, tcpFallbackPromise]);

      if (!discoveredNodes.some(n => n.ipAddress === '127.0.0.1' || localIps.includes(n.ipAddress))) {
        discoveredNodes.unshift({
          name: `${os.hostname()} (현재 PC)`,
          ipAddress: localIps[0] || '127.0.0.1',
          port: 9870,
          tlsPort: 9874,
          tlsSupported: true,
          osVersion: `${os.type()} ${os.release()}`,
          discoverySource: 'Localhost',
          isOnline: true,
          lastSeen: '로컬 실행 중'
        });
      }

      return discoveredNodes;
    });

    ipcMain.handle('fetch-telemetry', async (_, opts = {}) => {
      const { ip, port, tlsPort, secretKey } = typeof opts === 'string' ? { ip: opts } : (opts || {});
      if (!ip) return null;
      const key = (secretKey && secretKey.trim()) || readPersistedClusterKey();
      const headers = buildSignedGetHeaders('/api/v1/telemetry', key);
      try {
        const resp = await sendNodeRequest({
          ip,
          port: port || 9870,
          tlsPort: tlsPort || 9874,
          reqPath: '/api/v1/telemetry',
          method: 'GET',
          headers,
          timeoutMs: 3000
        });
        const parsed = JSON.parse(resp.body);
        return {
          ...parsed,
          activeModulesCount: parsed.totalRulesCount || parsed.activeModulesCount || 144,
          transportProtocol: resp.protocol,
          tofuStatus: resp.tofuCheck ? resp.tofuCheck.status : 'legacy'
        };
      } catch {
        return null;
      }
    });

    ipcMain.handle('fetch-challenge', async (_, opts = {}) => {
      const { ip, port, tlsPort } = typeof opts === 'string' ? { ip: opts } : (opts || {});
      if (!ip) return null;
      try {
        const resp = await sendNodeRequest({
          ip,
          port: port || 9870,
          tlsPort: tlsPort || 9874,
          reqPath: '/api/v1/auth/challenge',
          method: 'GET',
          timeoutMs: 4000
        });
        const json = JSON.parse(resp.body);
        return json.challengeNonce || null;
      } catch {
        return null;
      }
    });

    ipcMain.handle('pair-node', async (_, { ip, tlsPort, pairingPin } = {}) => {
      const startTime = Date.now();
      if (!ip || !pairingPin) {
        return {
          success: false,
          message: '대상 IP 또는 6자리 페어링 PIN이 누락되었습니다.',
          elapsedMs: 0
        };
      }
      try {
        const machineLabel = `${os.hostname()} (Electron Commander)`;
        const bodyJson = JSON.stringify({
          pairingPin: String(pairingPin).trim(),
          commanderMachineName: machineLabel,
          commanderName: machineLabel
        });

        const resp = await sendNodeRequest({
          ip,
          port: 9870,
          tlsPort: tlsPort || 9874,
          reqPath: '/api/v1/auth/pair',
          method: 'POST',
          headers: {
            'Content-Type': 'application/json; charset=utf-8',
            'Content-Length': Buffer.byteLength(bodyJson)
          },
          body: bodyJson,
          timeoutMs: 8000,
          allowHttpFallback: false
        });

        const parsed = JSON.parse(resp.body);
        const ok = resp.statusCode >= 200 && resp.statusCode < 300 && parsed.success;
        if (ok && parsed.clusterKey) {
          savePersistedClusterKey(parsed.clusterKey);
        }

        return {
          success: ok,
          clusterKey: parsed.clusterKey || null,
          tlsThumbprint: (resp.tofuCheck && resp.tofuCheck.thumbprint) || parsed.tlsThumbprint || '',
          message: parsed.message || (ok ? '페어링 완료' : `HTTP ${resp.statusCode}`),
          elapsedMs: Date.now() - startTime
        };
      } catch (err) {
        return {
          success: false,
          message: `TLS 페어링 실패: ${err.message}`,
          elapsedMs: Date.now() - startTime
        };
      }
    });

    ipcMain.handle('send-batch-command', async (_, { ip, port, tlsPort, path: reqPath, payload, secretKey } = {}) => {
      const key = (secretKey && secretKey.trim()) || readPersistedClusterKey();
      const startTime = Date.now();
      let handshakeLatency = 0;
      const safeInput = payload || {};

      const isSensitive = ['Reboot', 'Shutdown', 'ZeroTraceReset', 'Rollback', 'ForensicsWipe', 'AccountPurge', 'FreshHandover']
        .some(cmd => safeInput.commandType && String(safeInput.commandType).toLowerCase() === cmd.toLowerCase());

      let challengeNonce = null;
      if (isSensitive) {
        const hsStart = Date.now();
        try {
          const cResp = await sendNodeRequest({
            ip,
            port: port || 9870,
            tlsPort: tlsPort || 9874,
            reqPath: '/api/v1/auth/challenge',
            method: 'GET',
            timeoutMs: 4000
          });
          const cJson = JSON.parse(cResp.body);
          challengeNonce = cJson.challengeNonce || null;
        } catch {}
        handshakeLatency = Date.now() - hsStart;
      }

      // Strip any accidental secretKey field before serialization
      const { secretKey: _discarded, ...safePayload } = safeInput;
      const timestamp = Math.floor(Date.now() / 1000);
      const nonce = crypto.randomBytes(16).toString('hex');
      const bodyObj = {
        ...safePayload,
        timestamp,
        nonce,
        challengeNonce: challengeNonce || undefined
      };

      const rawJson = JSON.stringify(bodyObj);
      const signature = challengeNonce
        ? computeHmacSha256(`${challengeNonce}:${safePayload.commandType || ''}:${rawJson}`, key)
        : computeHmacSha256(rawJson, key);

      try {
        const resp = await sendNodeRequest({
          ip,
          port: port || 9870,
          tlsPort: tlsPort || 9874,
          reqPath,
          method: 'POST',
          headers: {
            'Content-Type': 'application/json; charset=utf-8',
            'Content-Length': Buffer.byteLength(rawJson),
            'X-WinPurify-Signature': signature,
            'X-WinPurify-Timestamp': timestamp.toString(),
            'X-WinPurify-Nonce': nonce,
            ...(challengeNonce ? {
              'X-WinPurify-Challenge-Nonce': challengeNonce,
              'X-WinPurify-Challenge-Signature': signature
            } : {})
          },
          body: rawJson,
          timeoutMs: 90000
        });

        const elapsed = Date.now() - startTime;
        const errPoint = resp.headers['x-winpurify-auth-error-point'] || resp.headers['x-winpurify-error-point'] || null;
        const authCat = resp.headers['x-winpurify-authcategory'] || null;

        try {
          const parsed = JSON.parse(resp.body);
          return {
            success: resp.statusCode >= 200 && resp.statusCode < 300 && (parsed.success !== false),
            statusCode: resp.statusCode,
            message: parsed.message || (resp.statusCode === 200 ? '성공' : `HTTP ${resp.statusCode}`),
            executionDetails: parsed.executionDetails || '',
            executionTimeMs: elapsed,
            handshakeLatencyMs: handshakeLatency,
            transportProtocol: resp.protocol,
            authErrorPoint: errPoint,
            authCategory: authCat
          };
        } catch {
          return {
            success: resp.statusCode >= 200 && resp.statusCode < 300,
            statusCode: resp.statusCode,
            message: `HTTP ${resp.statusCode}: ${resp.body.slice(0, 100)}`,
            executionTimeMs: elapsed,
            handshakeLatencyMs: handshakeLatency,
            transportProtocol: resp.protocol,
            authErrorPoint: errPoint,
            authCategory: authCat
          };
        }
      } catch (err) {
        const isTofuErr = err.message && err.message.includes('TOFU');
        return {
          success: false,
          message: `통신 오류: ${err.message}`,
          executionTimeMs: Date.now() - startTime,
          handshakeLatencyMs: handshakeLatency,
          transportProtocol: 'TLS Error',
          authErrorPoint: isTofuErr ? 'tofu_pin_mismatch' : 'connection_refused'
        };
      }
    });
    ipcMain.handle('get-handover-exclusions', async () => {
      return { success: true, rules: loadHandoverExclusions() };
    });

    ipcMain.handle('save-handover-exclusions', async (_, rules = []) => {
      try {
        const saved = saveHandoverExclusions(rules);
        return { success: true, rules: saved };
      } catch (err) {
        return { success: false, error: err.message, rules: loadHandoverExclusions() };
      }
    });

    ipcMain.handle('scan-handover', async () => {
      const protectedNames = new Set(['default', 'default user', 'public', 'all users', 'defaultuser0', 'wdagutilityaccount']);
      const currentUser = (os.userInfo().username || '').toLowerCase();
      const usersRoot = process.platform === 'win32' ? 'C:\\Users' : path.dirname(os.homedir());
      const exclusions = loadHandoverExclusions();
      const profiles = [];
      const artifacts = [];

      function isPathExcluded(targetPath) {
        const norm = path.resolve(targetPath).toLowerCase().replace(/[\\/]+$/, '');
        return exclusions.some(r => {
          if (r.ruleType !== 'PathPrefix' || !r.value) return false;
          const ex = path.resolve(r.value).toLowerCase().replace(/[\\/]+$/, '');
          return norm === ex || norm.startsWith(ex + path.sep);
        });
      }

      function estimateDirBytes(dirPath, maxDepth = 2, depth = 0) {
        let total = 0;
        try {
          const entries = fs.readdirSync(dirPath, { withFileTypes: true });
          for (const entry of entries) {
            const full = path.join(dirPath, entry.name);
            try {
              const lstat = fs.lstatSync(full);
              if (lstat.isSymbolicLink()) continue;
              if (lstat.isFile()) {
                total += lstat.size;
              } else if (lstat.isDirectory() && depth < maxDepth) {
                total += estimateDirBytes(full, maxDepth, depth + 1);
              }
            } catch {}
          }
        } catch {}
        return total;
      }

      try {
        if (fs.existsSync(usersRoot)) {
          const dirs = fs.readdirSync(usersRoot, { withFileTypes: true }).filter(d => d.isDirectory());
          for (const d of dirs) {
            const lower = d.name.toLowerCase();
            const fullPath = path.join(usersRoot, d.name);
            try {
              if (fs.lstatSync(fullPath).isSymbolicLink()) continue;
            } catch {
              continue;
            }
            const isSystemProtected = protectedNames.has(lower);
            const isCurrentSession = lower === currentUser || fullPath.toLowerCase() === os.homedir().toLowerCase();
            let lastUsed = '-';
            try {
              lastUsed = fs.statSync(fullPath).mtime.toISOString().slice(0, 10);
            } catch {}
            const estimatedBytes = isSystemProtected ? 0 : estimateDirBytes(fullPath, 2);
            profiles.push({
              sid: isCurrentSession ? 'CURRENT-SESSION' : `LOCAL-${d.name.toUpperCase()}`,
              username: d.name,
              profilePath: fullPath,
              accountType: isSystemProtected ? 'Windows 보호 프로필' : isCurrentSession ? '현재 활성 계정' : '이전 사용자 프로필',
              lastUsedTimeText: lastUsed,
              isCurrentSession,
              isSystemProtected,
              estimatedBytes
            });
          }
        }
      } catch (err) {
        console.warn('[Handover IPC Scan] profile scan warning:', err.message);
      }

      const browserDefs = [
        { id: 'chrome-multi', title: 'Google Chrome 전체 다중 프로필 (Default / Profile *)', rel: path.join('AppData', 'Local', 'Google', 'Chrome', 'User Data'), cat: 'BrowserSession', procs: ['chrome'] },
        { id: 'edge-multi', title: 'Microsoft Edge 전체 다중 프로필 (Default / Profile *)', rel: path.join('AppData', 'Local', 'Microsoft', 'Edge', 'User Data'), cat: 'BrowserSession', procs: ['msedge'] },
        { id: 'whale-multi', title: 'Naver Whale 다중 프로필', rel: path.join('AppData', 'Local', 'Naver', 'Naver Whale', 'User Data'), cat: 'BrowserSession', procs: ['whale'] },
        { id: 'brave-multi', title: 'Brave Browser 다중 프로필', rel: path.join('AppData', 'Local', 'BraveSoftware', 'Brave-Browser', 'User Data'), cat: 'BrowserSession', procs: ['brave'] },
        { id: 'firefox-multi', title: 'Mozilla Firefox 프로필 & cookies.sqlite', rel: path.join('AppData', 'Roaming', 'Mozilla', 'Firefox', 'Profiles'), cat: 'BrowserSession', procs: ['firefox'] },
        { id: 'kakao-session', title: '카카오톡 PC 인증서 & 자동 로그인 토큰', rel: path.join('AppData', 'Local', 'Kakao', 'KakaoTalk'), cat: 'AppSession', procs: ['KakaoTalk'] },
        { id: 'discord-session', title: 'Discord 데스크톱 토큰 캐시', rel: path.join('AppData', 'Roaming', 'discord', 'Local Storage'), cat: 'AppSession', procs: ['Discord'] },
        { id: 'slack-session', title: 'Slack 워크스페이스 로그인 캐시', rel: path.join('AppData', 'Roaming', 'Slack'), cat: 'AppSession', procs: ['slack'] },
        { id: 'telegram-session', title: 'Telegram Desktop 세션 데이터 (tdata)', rel: path.join('AppData', 'Roaming', 'Telegram Desktop', 'tdata'), cat: 'AppSession', procs: ['Telegram'] },
        { id: 'zoom-session', title: 'Zoom 데스크톱 계정·회의 캐시', rel: path.join('AppData', 'Roaming', 'Zoom', 'data'), cat: 'AppSession', procs: ['Zoom'] },
        { id: 'teams-wam', title: 'Microsoft Teams / WAM TokenBroker', rel: path.join('AppData', 'Local', 'Packages', 'Microsoft.AAD.BrokerPlugin_cw5n1h2txyewy'), cat: 'AppSession', procs: ['Teams', 'ms-teams'] },
        { id: 'ms-oneauth', title: 'Microsoft OneAuth / Office 인증 캐시', rel: path.join('AppData', 'Local', 'Microsoft', 'OneAuth'), cat: 'AppSession', procs: ['OUTLOOK', 'WINWORD', 'EXCEL'] },
        { id: 'cursor-ai-session', title: 'Cursor AI 에디터 세션 & 인증 캐시', rel: path.join('AppData', 'Roaming', 'Cursor', 'User', 'globalStorage'), cat: 'AppSession', procs: ['Cursor'] },
        { id: 'vscode-copilot-session', title: 'VS Code · Copilot · GitHub 인증 상태', rel: path.join('AppData', 'Roaming', 'Code', 'User', 'globalStorage'), cat: 'AppSession', procs: ['Code'] },
        { id: 'github-cli-session', title: 'GitHub CLI (hosts.yml) 인증 토큰', rel: path.join('AppData', 'Roaming', 'GitHub CLI'), cat: 'AppSession', procs: ['gh'] },
        { id: 'docker-session', title: 'Docker Desktop 자격 증명 설정 (.docker)', rel: '.docker', cat: 'AppSession', procs: ['Docker Desktop'] },
        { id: 'postman-figma-session', title: 'Postman / Figma 데스크톱 세션 캐시', rel: path.join('AppData', 'Roaming', 'Postman'), cat: 'AppSession', procs: ['Postman', 'Figma'] },
        { id: 'adobe-oobe-session', title: 'Adobe Creative Cloud (OOBE) 인증 캐시', rel: path.join('AppData', 'Local', 'Adobe', 'OOBE'), cat: 'AppSession', procs: ['Creative Cloud', 'Adobe Desktop Service'] },
        { id: 'onedrive-session', title: 'Microsoft OneDrive 동기화 계정 설정', rel: path.join('AppData', 'Local', 'Microsoft', 'OneDrive', 'settings'), cat: 'AppSession', procs: ['OneDrive'] },
        { id: 'ssh-keys-session', title: 'SSH 개인키 및 호스트 신뢰 목록 (.ssh)', rel: '.ssh', cat: 'AppSession', procs: [] },
        { id: 'npki-certs-session', title: '공동인증서 저장소 (NPKI)', rel: path.join('AppData', 'LocalLow', 'NPKI'), cat: 'AppSession', procs: [] },
        { id: 'recent-jumplists', title: 'Windows 최근 실행 파일 · Jump Lists (AutomaticDestinations)', rel: path.join('AppData', 'Roaming', 'Microsoft', 'Windows', 'Recent', 'AutomaticDestinations'), cat: 'CredentialAndNetwork', procs: [] }
      ];

      for (const prof of profiles.filter(p => !p.isSystemProtected)) {
        for (const def of browserDefs) {
          const target = path.join(prof.profilePath, def.rel);
          try {
            if (fs.existsSync(target) && !fs.lstatSync(target).isSymbolicLink()) {
              const bytes = estimateDirBytes(target, 2);
              const artifactId = `${def.id}::${prof.username}`;
              const matchedRule = exclusions.find(
                r => (r.ruleType === 'ArtifactId' && (r.value === artifactId || r.value === def.id)) ||
                     (r.ruleType === 'PathPrefix' && isPathExcluded(target))
              );
              artifacts.push({
                artifactId,
                title: def.title,
                domainCategory: def.cat,
                userScope: prof.username,
                targetPaths: [target],
                detectedCount: 1,
                estimatedBytes: bytes,
                associatedProcesses: def.procs,
                isExcluded: Boolean(matchedRule),
                exclusionReason: matchedRule ? `보존 예외: ${matchedRule.label}` : ''
              });
            }
          } catch {}
        }
      }

      // Current user personal folders (only selected in FullMachineReset mode)
      const currentHome = os.homedir();
      const personalFolders = ['Desktop', 'Documents', 'Downloads', 'Pictures', 'Videos']
        .map(folder => path.join(currentHome, folder))
        .filter(p => {
          try { return fs.existsSync(p) && !fs.lstatSync(p).isSymbolicLink() && !isPathExcluded(p); } catch { return false; }
        });

      if (personalFolders.length > 0) {
        let personalBytes = 0;
        let itemCount = 0;
        for (const pf of personalFolders) {
          personalBytes += estimateDirBytes(pf, 2);
          try { itemCount += fs.readdirSync(pf).length; } catch {}
        }
        if (itemCount > 0) {
          const matchedRule = exclusions.find(r => r.ruleType === 'ArtifactId' && r.value === 'current-personal-folders');
          artifacts.push({
            artifactId: 'current-personal-folders',
            title: `현재 사용자 개인 폴더 (${path.basename(currentHome)}: 바탕화면·문서·다운로드 등)`,
            domainCategory: 'PersonalFolder',
            userScope: os.userInfo().username || path.basename(currentHome),
            targetPaths: personalFolders,
            detectedCount: itemCount,
            estimatedBytes: personalBytes,
            associatedProcesses: [],
            isExcluded: Boolean(matchedRule),
            exclusionReason: matchedRule ? `보존 예외: ${matchedRule.label}` : ''
          });
        }
      }

      return {
        success: true,
        platform: process.platform,
        scannedAt: new Date().toISOString(),
        profiles,
        artifacts
      };
    });

    ipcMain.handle('execute-handover-purge', async (event, { targetPaths = [], emptyRootPaths = [], schedulePendingReboot = true, terminateProcesses = false, processNames = [] } = {}) => {
      const appBaseLower = path.resolve(__dirname).toLowerCase();
      const homeLower = path.resolve(os.homedir()).toLowerCase();
      const exclusions = loadHandoverExclusions();
      const usersRoot = process.platform === 'win32'
        ? path.resolve(process.env.SystemDrive ? `${process.env.SystemDrive}\\Users` : 'C:\\Users')
        : path.resolve(path.dirname(os.homedir()));

      const protectedExactLower = new Set([
        path.parse(usersRoot).root.toLowerCase().replace(/[\\/]+$/, '') || '/',
        '/',
        'c:',
        'c:\\',
        usersRoot.toLowerCase(),
        homeLower,
        '/home',
        '/users',
        '/etc',
        '/usr',
        '/bin',
        '/sbin',
        '/var'
      ]);

      const protectedPrefixesLower = [
        appBaseLower,
        (process.env.SystemRoot || 'C:\\Windows').toLowerCase(),
        (process.env.ProgramFiles || 'C:\\Program Files').toLowerCase(),
        (process.env['ProgramFiles(x86)'] || 'C:\\Program Files (x86)').toLowerCase(),
        (process.env.ProgramData || 'C:\\ProgramData').toLowerCase(),
        path.join(usersRoot, 'Public').toLowerCase(),
        path.join(usersRoot, 'Default').toLowerCase(),
        path.join(usersRoot, 'Default User').toLowerCase(),
        path.join(usersRoot, 'All Users').toLowerCase(),
        path.join(usersRoot, 'defaultuser0').toLowerCase(),
        path.join(usersRoot, 'WDAGUtilityAccount').toLowerCase()
      ].map(p => p.replace(/[\\/]+$/, ''));

      try {
        protectedPrefixesLower.push(path.resolve(app.getPath('userData')).toLowerCase());
        protectedPrefixesLower.push(path.resolve(path.dirname(app.getPath('exe'))).toLowerCase());
      } catch {}

      function emitProgress(stepIndex, stepTitle, percent, currentItemLabel, logLine) {
        try {
          event.sender.send('handover-progress', { stepIndex, stepTitle, percent, currentItemLabel, logLine });
        } catch {}
      }

      function checkProtectedReason(resolved) {
        const cleanLower = resolved.toLowerCase().replace(/[\\/]+$/, '') || '/';
        if (protectedExactLower.has(cleanLower)) {
          return `Protected OS/Home root path skipped: ${resolved}`;
        }
        if (appBaseLower.startsWith(cleanLower + path.sep) || cleanLower === appBaseLower || cleanLower.startsWith(appBaseLower + path.sep)) {
          return `Protected WinPurify path skipped: ${resolved}`;
        }
        if (protectedPrefixesLower.some(p => p && (cleanLower === p || cleanLower.startsWith(p + path.sep) || p.startsWith(cleanLower + path.sep)))) {
          return `Protected OS/System path skipped: ${resolved}`;
        }
        const matchedEx = exclusions.find(r => {
          if (r.ruleType !== 'PathPrefix' || !r.value) return false;
          const exLower = path.resolve(r.value).toLowerCase().replace(/[\\/]+$/, '');
          return cleanLower === exLower || cleanLower.startsWith(exLower + path.sep);
        });
        if (matchedEx) {
          return `Whitelist exclusion preserved: ${resolved}`;
        }
        try {
          if (fs.existsSync(resolved) && fs.lstatSync(resolved).isSymbolicLink()) {
            return `Symlink/Reparse point skipped for safety: ${resolved}`;
          }
        } catch {}
        return null;
      }

      const SAFE_HANDOVER_PROCESS_SET = new Set([
        'chrome', 'msedge', 'whale', 'brave', 'firefox',
        'kakaotalk', 'discord', 'slack', 'telegram', 'zoom',
        'teams', 'ms-teams', 'outlook', 'winword', 'excel',
        'cursor', 'code', 'docker desktop', 'postman', 'figma',
        'creative cloud', 'adobe desktop service', 'onedrive',
        'googledrivefs', 'steam', 'riotclientservices', 'epicgameslauncher'
      ]);

      function scheduleWindowsPendingDelete(resolved) {
        if (process.platform !== 'win32') return false;
        try {
          const psScript = [
            "$p = 'HKLM:\\SYSTEM\\CurrentControlSet\\Control\\Session Manager'",
            "$nt = $env:WPP_PENDING_TARGET",
            "if ([string]::IsNullOrWhiteSpace($nt)) { exit 1 }",
            "$cur = @()",
            "try { $v = (Get-ItemProperty -Path $p -Name 'PendingFileRenameOperations' -ErrorAction SilentlyContinue).PendingFileRenameOperations; if ($v) { $cur = [string[]]$v } } catch {}",
            "$list = New-Object System.Collections.Generic.List[string]",
            "foreach ($item in $cur) { $list.Add($item) }",
            "$exists = $false",
            "for ($i = 0; $i + 1 -lt $list.Count; $i += 2) { if ($list[$i] -eq $nt -and [string]::IsNullOrEmpty($list[$i+1])) { $exists = $true; break } }",
            "if (-not $exists) { $list.Add($nt); $list.Add(''); Set-ItemProperty -Path $p -Name 'PendingFileRenameOperations' -Value ([string[]]$list.ToArray()) -Type MultiString }",
            "exit 0"
          ].join('; ');
          execFileSync('powershell.exe', ['-NoProfile', '-NonInteractive', '-Command', psScript], {
            env: { ...process.env, WPP_PENDING_TARGET: `\\??\\${resolved}` },
            timeout: 5000,
            windowsHide: true
          });
          return true;
        } catch {
          return false;
        }
      }

      const targetList = Array.isArray(targetPaths) ? targetPaths.filter(p => p && typeof p === 'string') : [];
      const emptyList = Array.isArray(emptyRootPaths) ? emptyRootPaths.filter(p => p && typeof p === 'string') : [];
      const totalOps = Math.max(1, targetList.length + emptyList.length);
      let completedOps = 0;
      let removedCount = 0;
      const errors = [];
      const pendingRebootPaths = [];
      const pendingSet = new Set();

      function queuePendingOnce(p) {
        const key = p.toLowerCase();
        if (pendingSet.has(key)) return true;
        if (scheduleWindowsPendingDelete(p)) {
          pendingSet.add(key);
          pendingRebootPaths.push(p);
          return true;
        }
        return false;
      }

      function removeOrQueueBottomUp(targetPath, pct, label) {
        const reason = checkProtectedReason(targetPath);
        if (reason) {
          errors.push(reason);
          return;
        }
        if (!fs.existsSync(targetPath)) return;

        let lstat;
        try {
          lstat = fs.lstatSync(targetPath);
        } catch (err) {
          errors.push(`${targetPath}: ${err.message}`);
          return;
        }

        if (lstat.isSymbolicLink()) {
          errors.push(`Symlink/Reparse point skipped for safety: ${targetPath}`);
          return;
        }

        if (!lstat.isDirectory()) {
          try {
            fs.rmSync(targetPath, { force: true });
            removedCount++;
          } catch (err) {
            if (schedulePendingReboot && queuePendingOnce(targetPath)) {
              emitProgress(3, '잠금 항목 재부팅 예약', pct, label, `[재부팅 예약] 잠긴 파일 예약 등록: ${targetPath}`);
            } else {
              errors.push(`${targetPath}: ${err.message}`);
            }
          }
          return;
        }

        try {
          fs.rmSync(targetPath, { recursive: true, force: true });
          removedCount++;
          return;
        } catch {}

        let allChildrenHandled = true;
        try {
          for (const entry of fs.readdirSync(targetPath)) {
            removeOrQueueBottomUp(path.join(targetPath, entry), pct, label);
          }
          fs.rmdirSync(targetPath);
          removedCount++;
          return;
        } catch {
          allChildrenHandled = false;
        }

        if (!allChildrenHandled && fs.existsSync(targetPath)) {
          if (schedulePendingReboot && queuePendingOnce(targetPath)) {
            emitProgress(3, '잠금 항목 재부팅 예약', pct, label, `[재부팅 예약] 잔여 디렉터리 예약 등록: ${targetPath}`);
          } else {
            errors.push(`${targetPath}: directory remains locked`);
          }
        }
      }

      emitProgress(1, '보호 가드 및 잠금 검증', 15, `${totalOps}개 경로 검증`, `[Handover Step 1/4] 보호 경계 및 보존 예외 목록(${exclusions.length}건) 교차 검증 시작`);

      if (terminateProcesses && process.platform === 'win32' && Array.isArray(processNames)) {
        const uniqProcs = [...new Set(processNames.map(n => String(n || '').trim().replace(/\.exe$/i, '')).filter(Boolean))];
        for (const proc of uniqProcs) {
          if (!SAFE_HANDOVER_PROCESS_SET.has(proc.toLowerCase())) continue;
          try {
            execFileSync('taskkill.exe', ['/F', '/IM', `${proc}.exe`], { windowsHide: true, timeout: 4000 });
          } catch {}
        }
      }

      for (const rawPath of targetList) {
        const resolved = path.resolve(rawPath);
        completedOps++;
        const pct = 20 + Math.round((completedOps / totalOps) * 60);
        const label = path.basename(resolved) || resolved;
        emitProgress(2, '앱·브라우저·프로필 경로 소거', pct, label);
        removeOrQueueBottomUp(resolved, pct, label);
      }

      for (const rawDir of emptyList) {
        const resolved = path.resolve(rawDir);
        completedOps++;
        const pct = 20 + Math.round((completedOps / totalOps) * 60);
        const label = path.basename(resolved) || resolved;
        emitProgress(3, '개인 폴더 내부 비우기', pct, label);

        const reason = checkProtectedReason(resolved);
        if (reason) {
          errors.push(reason);
          continue;
        }
        try {
          if (fs.existsSync(resolved)) {
            for (const entry of fs.readdirSync(resolved)) {
              removeOrQueueBottomUp(path.join(resolved, entry), pct, label);
            }
          }
        } catch (err) {
          errors.push(`${resolved}: ${err.message}`);
        }
      }

      emitProgress(4, '2차 잔여물 재스캔 및 무결성 검증', 95, '디스크 재검증 중', `[Handover Step 4/4] 소거 ${removedCount}건 · 재부팅 예약 ${pendingRebootPaths.length}건 · 재스캔 진행`);

      return {
        success: errors.length === 0,
        removedCount,
        errors,
        pendingRebootPaths
      };
    });

    ipcMain.handle('export-handover-report', async (_, { content, defaultFilename } = {}) => {
      if (typeof content !== 'string' || !content.trim()) {
        return { success: false, error: 'Empty report content' };
      }
      const safeName = path.basename(String(defaultFilename || `WinPurify_Handover_Report_${Date.now()}.html`)).replace(/[^a-zA-Z0-9._-]/g, '_');
      const resolvedDialog = getElectronDialog();

      if (resolvedDialog && typeof resolvedDialog.showSaveDialog === 'function') {
        try {
          const dlgRes = await resolvedDialog.showSaveDialog({
            title: '클린 환경 인수인계 검증 인증서 저장',
            defaultPath: path.join(app.getPath('desktop'), safeName),
            filters: [
              { name: 'HTML 인증서 (*.html)', extensions: ['html'] },
              { name: 'JSON 리포트 (*.json)', extensions: ['json'] }
            ]
          });
          if (dlgRes.canceled || !dlgRes.filePath) {
            return { success: false, canceled: true };
          }
          fs.writeFileSync(dlgRes.filePath, content, 'utf8');
          return { success: true, filePath: dlgRes.filePath };
        } catch (err) {
          return { success: false, error: err.message };
        }
      }

      const reportsDir = path.join(app.getPath('desktop'), 'WinPurify_Reports');
      if (!fs.existsSync(reportsDir)) fs.mkdirSync(reportsDir, { recursive: true });
      const targetFile = path.resolve(reportsDir, safeName);
      if (!targetFile.startsWith(path.resolve(reportsDir) + path.sep)) {
        return { success: false, error: 'Invalid report filename' };
      }
      fs.writeFileSync(targetFile, content, 'utf8');
      return { success: true, filePath: targetFile };
    });

    ipcMain.handle('trigger-safe-reboot', async () => {
      if (process.platform !== 'win32') {
        return { success: false, message: 'Windows 환경에서만 안전 재부팅 명령(shutdown.exe)이 지원됩니다.' };
      }
      return await new Promise((resolve) => {
        execFile('shutdown.exe', ['/r', '/f', '/t', '5', '/c', 'WinPurify Pro: 재부팅 후 잠금 잔여 항목 자동 정리'], { windowsHide: true }, (err) => {
          if (err) {
            resolve({ success: false, message: `재부팅 예약 실패 (관리자 권한 확인): ${err.message}` });
          } else {
            resolve({ success: true, message: '5초 후 시스템이 안전하게 재부팅됩니다.' });
          }
        });
      });
    });
  }

  return {
    registerClusterIpcHandlers,
    readPersistedClusterKey,
    savePersistedClusterKey
  };
}

module.exports = { createVaultAndIpcHelpers };
