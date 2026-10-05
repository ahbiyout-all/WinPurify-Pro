const http = require('http');
const fs = require('fs');
const path = require('path');
const { exec, execSync } = require('child_process');
const { promisify } = require('util');
const execPromise = promisify(exec);
const os = require('os');
const esbuild = require('esbuild');

// PORT must strictly be 3000 for the platform's nginx reverse proxy.
// Do NOT read or override process.env.PORT.
const PORT = 3000;

// Ensure dist-web assets exist on startup
function compileWebAssets() {
  const distDir = path.resolve('./dist-web');
  if (!fs.existsSync(distDir)) {
    fs.mkdirSync(distDir, { recursive: true });
  }

  const indexJsPath = path.resolve('./dist-web/index.js');
  const indexCssPath = path.resolve('./dist-web/index.css');

  if (fs.existsSync(indexJsPath) && fs.existsSync(indexCssPath)) {
    console.log('[Dev-Server] Pre-compiled web bundle assets found in dist-web.');
    return;
  }

  console.log('[Dev-Server] Compiling web bundle assets for initial launch...');

  try {
    esbuild.buildSync({
      entryPoints: ['index.tsx'],
      bundle: true,
      minify: false,
      sourcemap: 'inline',
      outfile: 'dist-web/index.js',
      platform: 'browser',
      target: ['chrome100'],
      define: {
        'process.env.NODE_ENV': '"development"'
      }
    });
    console.log('[Dev-Server] JS compilation finished successfully.');
  } catch (e) {
    console.error('[Dev-Server] JS compilation failed:', e.message);
  }

  try {
    execSync('npx @tailwindcss/cli -i input.css -o dist-web/index.css', { stdio: 'inherit' });
    console.log('[Dev-Server] Tailwind CSS compilation finished successfully.');
  } catch (e) {
    console.error('[Dev-Server] Tailwind CSS compilation failed:', e.message);
  }
}

// Compile assets once at dev server start
try {
  compileWebAssets();
} catch (err) {
  console.warn('[Dev-Server] Initial assets compilation failed:', err.message);
}

const MIME_TYPES = {
  '.html': 'text/html',
  '.css': 'text/css',
  '.js': 'application/javascript',
  '.tsx': 'application/javascript',
  '.ts': 'application/javascript',
  '.json': 'application/json',
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.gif': 'image/gif',
  '.svg': 'image/svg+xml',
  '.ico': 'image/x-icon'
};

// Simulated task size state (in bytes) to ensure lightning-fast 0ms responses without disk thrashing
const TASK_SIMULATED_SIZES = {
  'upd-system-files': 145000000,
  'upd-wu-cleanup': 820000000,
  'upd-delivery': 310000000,
  'upd-cbs-logs': 120000000,
  'upd-catroot': 45000000,
  'upd-old': 4500000000,
  'upd-store-cache': 35000000,
  'upd-lang-cleanup': 65000000,
  'stg-win-temp': 420000000,
  'stg-user-temp': 1250000000,
  'stg-recycle': 2100000000,
  'stg-crash-dumps': 850000000,
  'stg-thumbnails': 240000000,
  'stg-store-temp': 180000000,
  'stg-cryptnet-cache': 28000000,
  'stg-office-cache': 310000000,
  'stg-sandbox': 1500000000,
  'stg-gamebar': 450000000,
  'stg-wmp': 90000000,
  'stg-downloads': 1200000000,
  'app-discord': 380000000,
  'app-spotify': 620000000,
  'app-steam': 950000000,
  'app-vscode': 540000000,
  'app-npm': 430000000,
  'brw-chrome': 780000000,
  'brw-edge': 620000000,
  'brw-firefox': 410000000,
  'brw-brave': 350000000,
  'brw-whale': 290000000,
  'brw-opera': 210000000,
  'sys-driver-store': 1200000000,
  'sys-font-cache': 85000000,
  'sys-icon-cache': 45000000,
  'sys-shader-cache': 320000000,
  'sys-prefetch': 75000000,
  'sys-dns-flush': 5000000,
  'sys-print-spooler': 18000000,
  'sys-memory-dump': 650000000,
  'sys-setup-logs': 110000000,
  'sys-wmi-logs': 42000000,
  'sys-rdp-cache': 25000000,
  'sys-branchcache': 190000000,
  'sys-vss-old': 2800000000,
  'sys-perf-logs': 38000000,
  'sys-bt-logs': 12000000,
  'sys-net-usage': 24000000,
  'sys-clipboard': 8000000,
  'sys-actioncenter': 15000000,
  'brw-history': 35000000,
  'brw-storage': 140000000,
  'priv-telemetry': 95000000,
  'priv-activity': 48000000,
  'priv-recent': 12000000,
  'priv-app-compat': 65000000,
  'priv-speech': 22000000,
  'priv-mail': 110000000,
  'priv-ime': 32000000,
  'sec-defender': 180000000,
  'sec-event-logs': 240000000,
  'sec-defender-quar': 85000000,
  'inst-orphaned': 450000000
};

// Calculate initial total
let initialSimulatedSize = Object.values(TASK_SIMULATED_SIZES).reduce((a, b) => a + b, 0);

function findTaskIdFromCommand(command) {
  const cmd = String(command);
  
  if (cmd.includes('C:\\Windows\\Temp')) return 'stg-win-temp';
  if (cmd.includes('$env:TEMP') || cmd.includes('%TEMP%')) return 'stg-user-temp';
  if (cmd.includes('Clear-RecycleBin') || cmd.includes('$RECYCLE.BIN')) return 'stg-recycle';
  if (cmd.includes('C:\\Windows\\SoftwareDistribution') || cmd.includes('wuauserv')) return 'upd-system-files';
  if (cmd.includes('chrome')) return 'brw-chrome';
  if (cmd.includes('msedge')) return 'brw-edge';
  if (cmd.includes('firefox')) return 'brw-firefox';
  if (cmd.includes('brave')) return 'brw-brave';
  if (cmd.includes('whale')) return 'brw-whale';
  if (cmd.includes('opera')) return 'brw-opera';
  if (cmd.includes('discord')) return 'app-discord';
  if (cmd.includes('Spotify')) return 'app-spotify';
  if (cmd.includes('Steam')) return 'app-steam';
  if (cmd.includes('Code')) return 'app-vscode';
  if (cmd.includes('npm-cache')) return 'app-npm';
  if (cmd.includes('C:\\Windows\\Logs\\CBS')) return 'upd-cbs-logs';
  if (cmd.includes('C:\\Windows\\System32\\catroot2')) return 'upd-catroot';
  if (cmd.includes('C:\\Windows.old')) return 'upd-old';
  if (cmd.includes('WerSvc') && cmd.includes('memory.dmp')) return 'stg-crash-dumps';
  if (cmd.includes('thumbcache')) return 'stg-thumbnails';
  if (cmd.includes('certutil -urlcache')) return 'stg-cryptnet-cache';
  if (cmd.includes('OfficeFileCache')) return 'stg-office-cache';
  if (cmd.includes('Containers')) return 'stg-sandbox';
  if (cmd.includes('Highlights')) return 'stg-gamebar';
  if (cmd.includes('Media Player')) return 'stg-wmp';
  if (cmd.includes('powercfg -h')) return 'stg-hiberfil';
  if (cmd.includes('Downloads')) return 'stg-downloads';
  if (cmd.includes('pnputil')) return 'sys-driver-store';
  if (cmd.includes('FontCache')) return 'sys-font-cache';
  if (cmd.includes('IconCache.db')) return 'sys-icon-cache';
  if (cmd.includes('D3DSCache')) return 'sys-shader-cache';
  if (cmd.includes('RunMRU')) return 'sys-reg-mru';
  if (cmd.includes('Prefetch')) return 'sys-prefetch';
  if (cmd.includes('Clear-DnsClientCache')) return 'sys-dns-flush';
  if (cmd.includes('spooler')) return 'sys-print-spooler';
  if (cmd.includes('WerSvc') && cmd.includes('CrashDumps')) return 'sys-memory-dump';
  if (cmd.includes('Panther')) return 'sys-setup-logs';
  if (cmd.includes('wbem\\Logs')) return 'sys-wmi-logs';
  if (cmd.includes('Terminal Server Client')) return 'sys-rdp-cache';
  if (cmd.includes('BranchCache') || cmd.includes('PeerDistSvc')) return 'sys-branchcache';
  if (cmd.includes('ShadowCopy')) return 'sys-vss-old';
  if (cmd.includes('WMI')) return 'sys-perf-logs';
  if (cmd.includes('BthTelemetry')) return 'sys-bt-logs';
  if (cmd.includes('sru')) return 'sys-net-usage';
  if (cmd.includes('Clipboard')) return 'sys-clipboard';
  if (cmd.includes('Notifications')) return 'sys-actioncenter';
  if (cmd.includes('History')) return 'brw-history';
  if (cmd.includes('IndexedDB')) return 'brw-storage';
  if (cmd.includes('DiagTrack')) return 'priv-telemetry';
  if (cmd.includes('ConnectedDevicesPlatform')) return 'priv-activity';
  if (cmd.includes('Recent')) return 'priv-recent';
  if (cmd.includes('AppCompat')) return 'priv-app-compat';
  if (cmd.includes('InputPersonalization')) return 'priv-speech';
  if (cmd.includes('communicationsapps')) return 'priv-mail';
  if (cmd.includes('InputMethod\\Shared')) return 'priv-ime';
  if (cmd.includes('Scans\\History')) return 'sec-defender';
  if (cmd.includes('wevtutil')) return 'sec-event-logs';
  if (cmd.includes('Quarantine')) return 'sec-defender-quar';
  if (cmd.includes('LocalPackage')) return 'inst-orphaned';

  return null;
}

function purgeSimulatedTask(taskId) {
  if (taskId && TASK_SIMULATED_SIZES[taskId] !== undefined) {
    TASK_SIMULATED_SIZES[taskId] = 0;
  }
}

function readBody(req) {
  return new Promise((resolve, reject) => {
    let body = '';
    req.on('data', chunk => { body += chunk.toString(); });
    req.on('end', () => { resolve(body); });
    req.on('error', err => { reject(err); });
  });
}

const server = http.createServer(async (req, res) => {
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type');

  if (req.method === 'OPTIONS') {
    res.writeHead(200);
    res.end();
    return;
  }

  // --- API ROUTE HANDLERS ---

  // 1) Diagnostic check for .NET 10.0 SDK
  if (req.url === '/api/dotnet-status') {
    res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
    let hasNet10 = false;
    let dotnetPath = '';
    let sdks = '';
    let activePath = process.env.PATH || '';

    try {
      const pathResult = await execPromise(process.platform === 'win32' ? 'where dotnet' : 'which dotnet');
      dotnetPath = pathResult.stdout.trim();
    } catch (e) {
      dotnetPath = 'NOT_FOUND_IN_PATH';
    }

    try {
      const listResult = await execPromise('dotnet --list-sdks');
      sdks = listResult.stdout.trim();
      if (/10\./.test(sdks)) {
        hasNet10 = true;
      }
    } catch (e) {
      sdks = 'DOTNET_EXEC_ERROR_OR_NOT_INSTALLED';
    }

    res.end(JSON.stringify({
      success: true,
      hasNet10,
      dotnetPath,
      sdks,
      activePath,
      platform: process.platform
    }));
    return;
  }

  // 2) Run system command fallback
  if (req.url === '/api/run-system-command' && req.method === 'POST') {
    const bodyText = await readBody(req);
    res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
    try {
      const { command } = JSON.parse(bodyText);
      const cmdStr = typeof command === 'object' && command !== null ? command.command : String(command);
      
      console.log('[Dev-Server API] Simulating runCommand:', cmdStr);
      
      const taskId = findTaskIdFromCommand(cmdStr);
      if (taskId) {
        console.log(`[Dev-Server API] Matched taskId: ${taskId}. Purging simulated task size...`);
        purgeSimulatedTask(taskId);
      }

      let output = "Command execution simulated successfully on server sandbox.";
      if (cmdStr.includes('tasklist')) {
        output = ""; // return empty to denote no browser process running
      } else if (cmdStr.includes('$dotnetInstalled') || cmdStr.includes('dotnet')) {
        let hasNet10 = false;
        let dotnetPath = 'NOT_FOUND_IN_PATH';
        let sdksArr = [];
        try {
          const pathResult = await execPromise(process.platform === 'win32' ? 'where dotnet' : 'which dotnet');
          dotnetPath = pathResult.stdout.trim();
        } catch (e) {}
        try {
          const listResult = await execPromise('dotnet --list-sdks');
          const sdksText = listResult.stdout.trim();
          sdksArr = sdksText ? sdksText.split('\n') : [];
          if (/10\./.test(sdksText)) hasNet10 = true;
        } catch (e) {}
        output = JSON.stringify({
          installed: dotnetPath !== 'NOT_FOUND_IN_PATH',
          sdks: sdksArr,
          path: process.env.PATH || ''
        });
      }

      res.end(JSON.stringify({ success: true, output, error: null }));
    } catch (err) {
      res.end(JSON.stringify({ success: false, output: null, error: err.message }));
    }
    return;
  }

  // 3) Scan Folders Sizes Native
  if (req.url === '/api/scan-folders-sizes-native' && req.method === 'POST') {
    const bodyText = await readBody(req);
    res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
    try {
      const { taskIds } = JSON.parse(bodyText);
      const resultObj = {};
      if (Array.isArray(taskIds)) {
        for (const id of taskIds) {
          resultObj[id] = TASK_SIMULATED_SIZES[id] || 0;
        }
      }
      res.end(JSON.stringify(resultObj));
    } catch (err) {
      res.writeHead(400, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify({ error: err.message }));
    }
    return;
  }

  // 4) Get Disk Free Space
  if (req.url === '/api/get-disk-free-space') {
    res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
    const baselineFreeSpace = 1024 * 1024 * 1024 * 150; // 150 GB baseline
    const currentSimulatedSize = Object.values(TASK_SIMULATED_SIZES).reduce((a, b) => a + b, 0);
    const delta = Math.max(0, initialSimulatedSize - currentSimulatedSize);
    res.end(JSON.stringify({ freeSpace: baselineFreeSpace + delta }));
    return;
  }

  // 5) Check Admin
  if (req.url === '/api/check-admin') {
    res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
    res.end(JSON.stringify({ isAdmin: true }));
    return;
  }

  // 6) Get Process Metrics & System Memory
  if (req.url === '/api/get-system-memory') {
    res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
    const total = os.totalmem();
    const free = os.freemem();
    const used = total - free;
    const totalMB = Math.round(total / (1024 * 1024));
    const usedMB = Math.round(used / (1024 * 1024));
    const usagePercent = Math.round((used / total) * 100);
    res.end(JSON.stringify({ totalMB, usedMB, freeMB: Math.round(free / (1024 * 1024)), usagePercent }));
    return;
  }

  if (req.url === '/api/get-process-metrics') {
    res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
    const total = os.totalmem();
    const free = os.freemem();
    const usedMB = Math.round((total - free) / 1024 / 1024);
    const cpu = Math.min(100, Math.max(1, Math.round(os.loadavg()[0] * 10) || 4));
    res.end(JSON.stringify({ cpu, memory: usedMB }));
    return;
  }

  // 7) Save and Open Log
  if (req.url === '/api/save-and-open-log' && req.method === 'POST') {
    const bodyText = await readBody(req);
    res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
    try {
      const { content } = JSON.parse(bodyText);
      const logDir = path.resolve('./WinPurify_Logs');
      if (!fs.existsSync(logDir)) {
        fs.mkdirSync(logDir, { recursive: true });
      }
      const timestamp = new Date().toISOString().replace(/[:.]/g, '-').split('T');
      const fileName = `WinPurify_Log_${timestamp[0]}_${timestamp[1].slice(0, 8)}.txt`;
      const filePath = path.join(logDir, fileName);
      fs.writeFileSync(filePath, content, 'utf8');
      
      console.log(`[Dev-Server] Saved log file: ${filePath}`);
      res.end(JSON.stringify({ success: true, path: filePath, fileName }));
    } catch (err) {
      res.end(JSON.stringify({ success: false, error: err.message }));
    }
    return;
  }

  // 8) Open Log Folder (Stub)
  if (req.url === '/api/open-log-folder' && req.method === 'POST') {
    res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
    res.end(JSON.stringify({ success: true }));
    return;
  }

  // 9) Force Kill Command (Stub)
  if (req.url === '/api/force-kill-command' && req.method === 'POST') {
    res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
    res.end(JSON.stringify({ success: true }));
    return;
  }

  // Serve static files
  let filePath = req.url === '/' ? './index.html' : '.' + req.url.split('?')[0];
  filePath = path.resolve(filePath);

  // Prevent directory traversal
  const rootDir = path.resolve('.');
  if (!filePath.startsWith(rootDir)) {
    res.writeHead(403, { 'Content-Type': 'text/plain' });
    res.end('Access Denied');
    return;
  }

  fs.stat(filePath, (err, stats) => {
    if (err || !stats.isFile()) {
      res.writeHead(404, { 'Content-Type': 'text/plain' });
      res.end('404 Not Found');
      return;
    }

    const ext = path.extname(filePath).toLowerCase();
    const contentType = MIME_TYPES[ext] || 'application/octet-stream';

    res.writeHead(200, { 'Content-Type': contentType });
    fs.createReadStream(filePath).pipe(res);
  });
});

server.listen(PORT, '0.0.0.0', () => {
  console.log(`Cisnet Dev Diagnostics Server listening on http://0.0.0.0:${PORT}`);
});
