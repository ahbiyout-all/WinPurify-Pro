/**
 * WinPurify Pro - Master Central Commander Standalone Desktop App (Electron)
 * Version: v4.67.0
 * 
 * Runs the Master Central Commander as a standalone desktop packaged app,
 * completely independent of any web browser, with direct UDP broadcast discovery,
 * HMAC-SHA256 cryptography, Challenge-Response handshake, and real-time fleet analytics.
 */

const { app, BrowserWindow, ipcMain, shell } = require('electron');
const path = require('path');
const dgram = require('dgram');
const http = require('http');
const crypto = require('crypto');
const os = require('os');
const fs = require('fs');

let mainWindow = null;

// Enforce single instance for Master Central Commander
const gotTheLock = app.requestSingleInstanceLock();
if (!gotTheLock) {
  app.quit();
} else {
  app.on('second-instance', () => {
    if (mainWindow) {
      if (mainWindow.isMinimized()) mainWindow.restore();
      mainWindow.focus();
    }
  });
}

function getLocalIpAddresses() {
  const interfaces = os.networkInterfaces();
  const addresses = [];
  for (const name of Object.keys(interfaces)) {
    for (const iface of interfaces[name] || []) {
      if (iface.family === 'IPv4' && !iface.internal) {
        addresses.push(iface.address);
      }
    }
  }
  return addresses.length > 0 ? addresses : ['127.0.0.1'];
}

function computeHmacSha256(data, key) {
  return crypto.createHmac('sha256', key).update(data).digest('hex').toUpperCase();
}

function createWindow() {
  const iconPath = path.join(__dirname, 'WinPurify_icon.png');

  mainWindow = new BrowserWindow({
    title: 'WinPurify Pro - Master Central Commander Console v4.67.0',
    width: 1320,
    height: 860,
    minWidth: 1060,
    minHeight: 620,
    backgroundColor: '#0B0F19',
    frame: false, // Custom sleek dark industrial title bar
    show: false,
    icon: fs.existsSync(iconPath) ? iconPath : undefined,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      nodeIntegration: false,
      contextIsolation: true,
      sandbox: false
    }
  });

  mainWindow.loadFile(path.join(__dirname, 'remote-center.html'));

  mainWindow.once('ready-to-show', () => {
    mainWindow.show();
    mainWindow.focus();
  });

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

// IPC Handlers
ipcMain.on('window-minimize', () => {
  if (mainWindow) mainWindow.minimize();
});

ipcMain.on('window-maximize', () => {
  if (mainWindow) {
    if (mainWindow.isMaximized()) {
      mainWindow.unmaximize();
    } else {
      mainWindow.maximize();
    }
  }
});

ipcMain.on('window-close', () => {
  if (mainWindow) mainWindow.close();
});

ipcMain.handle('open-log-folder', () => {
  const logDir = path.join(os.homedir(), 'AppData', 'Roaming', 'WinPurifyPro', 'Logs');
  if (!fs.existsSync(logDir)) {
    fs.mkdirSync(logDir, { recursive: true });
  }
  shell.openPath(logDir);
  return { success: true, path: logDir };
});

ipcMain.handle('save-logs', async (_, { content, filename }) => {
  try {
    const desktopDir = path.join(os.homedir(), 'Desktop');
    const targetFile = path.join(desktopDir, filename || `WinPurify_Commander_${Date.now()}.txt`);
    fs.writeFileSync(targetFile, content, 'utf8');
    return { success: true, path: targetFile };
  } catch (err) {
    return { success: false, error: err.message };
  }
});

// UDP Discovery on port 9871
ipcMain.handle('discover-nodes', async (_, timeoutMs = 2500) => {
  return new Promise((resolve) => {
    const discoveredNodes = [];
    const client = dgram.createSocket('udp4');

    client.on('error', (err) => {
      console.warn('[UDP Discovery Error]', err.message);
      try { client.close(); } catch (e) {}
      resolve(discoveredNodes);
    });

    client.on('message', (msg, rinfo) => {
      const message = msg.toString('utf8');
      if (message.startsWith('WINPURIFY_DISCOVER_PONG')) {
        const parts = message.split('|');
        const nodeName = parts[1] || rinfo.address;
        const port = parseInt(parts[2], 10) || 9870;
        const osVer = parts[3] || 'Windows 11 x64';
        const reportedIp = parts[5] || rinfo.address;

        let finalIp = reportedIp;
        if (!finalIp || finalIp === '127.0.0.1') {
          finalIp = rinfo.address;
        }

        if (!discoveredNodes.some(n => n.ipAddress === finalIp && n.port === port)) {
          discoveredNodes.push({
            name: nodeName,
            ipAddress: finalIp,
            port: port,
            osVersion: osVer,
            isOnline: true,
            lastSeen: '방금 전'
          });
        }
      }
    });

    client.bind(0, () => {
      client.setBroadcast(true);
      const pingPacket = Buffer.from('WINPURIFY_DISCOVER_PING');
      
      // Broadcast to standard port 9871
      client.send(pingPacket, 0, pingPacket.length, 9871, '255.255.255.255', (err) => {
        if (err) console.warn('[UDP Broadcast Send Warning]', err.message);
      });
      
      // Also send to localhost
      client.send(pingPacket, 0, pingPacket.length, 9871, '127.0.0.1');
    });

    setTimeout(() => {
      try { client.close(); } catch (e) {}
      
      // Always ensure local host node is present
      const localIps = getLocalIpAddresses();
      if (!discoveredNodes.some(n => n.ipAddress === '127.0.0.1' || localIps.includes(n.ipAddress))) {
        discoveredNodes.unshift({
          name: `${os.hostname()} (현재 PC)`,
          ipAddress: localIps[0] || '127.0.0.1',
          port: 9870,
          osVersion: `${os.type()} ${os.release()}`,
          isOnline: true,
          lastSeen: '로컬 실행 중'
        });
      }
      resolve(discoveredNodes);
    }, timeoutMs);
  });
});

// Telemetry query HTTP GET /api/v1/telemetry
ipcMain.handle('fetch-telemetry', async (_, { ip, port }) => {
  return new Promise((resolve) => {
    const req = http.request({
      hostname: ip,
      port: port || 9870,
      path: '/api/v1/telemetry',
      method: 'GET',
      timeout: 3000
    }, (res) => {
      let data = '';
      res.on('data', chunk => { data += chunk; });
      res.on('end', () => {
        try {
          resolve(JSON.parse(data));
        } catch {
          resolve(null);
        }
      });
    });

    req.on('error', () => resolve(null));
    req.on('timeout', () => { req.destroy(); resolve(null); });
    req.end();
  });
});

// Challenge Nonce request HTTP GET /api/v1/auth/challenge
ipcMain.handle('fetch-challenge', async (_, { ip, port }) => {
  return new Promise((resolve) => {
    const req = http.request({
      hostname: ip,
      port: port || 9870,
      path: '/api/v1/auth/challenge',
      method: 'GET',
      timeout: 4000
    }, (res) => {
      let data = '';
      res.on('data', chunk => { data += chunk; });
      res.on('end', () => {
        try {
          const json = JSON.parse(data);
          resolve(json.challengeNonce || null);
        } catch {
          resolve(null);
        }
      });
    });

    req.on('error', () => resolve(null));
    req.on('timeout', () => { req.destroy(); resolve(null); });
    req.end();
  });
});

// Send signed batch command with Challenge-Response Handshake
ipcMain.handle('send-batch-command', async (_, { ip, port, path: reqPath, payload, secretKey }) => {
  const key = secretKey || 'WPP-SEC-CLUSTER-2026-KEY';
  const startTime = Date.now();
  let handshakeLatency = 0;

  const isSensitive = ['Reboot', 'Shutdown', 'ZeroTraceReset', 'Rollback', 'ForensicsWipe', 'AccountPurge']
    .some(cmd => payload.commandType && payload.commandType.toLowerCase() === cmd.toLowerCase());

  let challengeNonce = null;
  if (isSensitive) {
    const hsStart = Date.now();
    try {
      challengeNonce = await new Promise((resolve) => {
        const cReq = http.request({
          hostname: ip,
          port: port || 9870,
          path: '/api/v1/auth/challenge',
          method: 'GET',
          timeout: 4000
        }, (res) => {
          let b = '';
          res.on('data', chunk => { b += chunk; });
          res.on('end', () => {
            try { resolve(JSON.parse(b).challengeNonce); } catch { resolve(null); }
          });
        });
        cReq.on('error', () => resolve(null));
        cReq.on('timeout', () => { cReq.destroy(); resolve(null); });
        cReq.end();
      });
    } catch {}
    handshakeLatency = Date.now() - hsStart;
  }

  const timestamp = Math.floor(Date.now() / 1000);
  const nonce = crypto.randomBytes(16).toString('hex');
  const bodyObj = {
    ...payload,
    timestamp,
    nonce,
    secretKey: key,
    challengeNonce: challengeNonce || undefined
  };

  const rawJson = JSON.stringify(bodyObj);
  let signature;
  if (challengeNonce) {
    const challengePayload = `${challengeNonce}:${payload.commandType || ''}:${rawJson}`;
    signature = computeHmacSha256(challengePayload, key);
  } else {
    signature = computeHmacSha256(rawJson, key);
  }

  return new Promise((resolve) => {
    const req = http.request({
      hostname: ip,
      port: port || 9870,
      path: reqPath,
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
      timeout: 90000 // 90s for OS level restore points & VSS snapshots
    }, (res) => {
      let respData = '';
      res.on('data', chunk => { respData += chunk; });
      res.on('end', () => {
        const elapsed = Date.now() - startTime;
        const errPoint = res.headers['x-winpurify-auth-error-point'] || res.headers['x-winpurify-error-point'] || null;
        const authCat = res.headers['x-winpurify-authcategory'] || null;

        try {
          const parsed = JSON.parse(respData);
          resolve({
            success: res.statusCode >= 200 && res.statusCode < 300 && (parsed.success !== false),
            statusCode: res.statusCode,
            message: parsed.message || (res.statusCode === 200 ? '성공' : `HTTP ${res.statusCode}`),
            executionDetails: parsed.executionDetails || '',
            executionTimeMs: elapsed,
            handshakeLatencyMs: handshakeLatency,
            authErrorPoint: errPoint,
            authCategory: authCat
          });
        } catch {
          resolve({
            success: res.statusCode >= 200 && res.statusCode < 300,
            statusCode: res.statusCode,
            message: `HTTP ${res.statusCode}: ${respData.slice(0, 100)}`,
            executionTimeMs: elapsed,
            handshakeLatencyMs: handshakeLatency,
            authErrorPoint: errPoint,
            authCategory: authCat
          });
        }
      });
    });

    req.on('error', (err) => {
      resolve({
        success: false,
        message: `통신 오류: ${err.message}`,
        executionTimeMs: Date.now() - startTime,
        handshakeLatencyMs: handshakeLatency,
        authErrorPoint: 'connection_refused'
      });
    });

    req.on('timeout', () => {
      req.destroy();
      resolve({
        success: false,
        message: '응답 시간 초과 (Timeout)',
        executionTimeMs: Date.now() - startTime,
        handshakeLatencyMs: handshakeLatency,
        authErrorPoint: 'timeout'
      });
    });

    req.write(rawJson);
    req.end();
  });
});

app.whenReady().then(() => {
  createWindow();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});
