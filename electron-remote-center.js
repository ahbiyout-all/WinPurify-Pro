/**
 * WinPurify Pro - Master Central Commander Standalone Desktop App (Electron)
 * Version: v4.71.1
 *
 * Security & Networking Architecture:
 *  - TLS 1.2/1.3 Transport (TCP:9874) with TOFU Certificate Fingerprint Pinning
 *  - Electron safeStorage Encrypted Local Cluster Master Key Vault (Zero Hardcoded Keys)
 *  - HMAC-SHA256 Request Signing + Challenge-Response Nonces (Zero Plaintext Secret Transmission)
 *  - 6-Digit Single-Use PIN Zero-Config Studio Pairing (POST /api/v1/auth/pair)
 *  - Hybrid LAN Discovery: Multi-NIC UDP:9871 Broadcast + Throttled /24 TCP:9874/9870 Fallback
 */

const { app, BrowserWindow, ipcMain, shell, safeStorage } = require('electron');
const path = require('path');
const fs = require('fs');
const { createVaultAndIpcHelpers } = require('./electron-ipc-shared');

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

// =========================================================================
// Window Creation
// =========================================================================
function createWindow() {
  const iconPath = path.join(__dirname, 'WinPurify_icon.png');

  mainWindow = new BrowserWindow({
    title: 'WinPurify Pro - Master Central Commander Console v4.71.1',
    width: 1320,
    height: 860,
    minWidth: 1060,
    minHeight: 620,
    backgroundColor: '#0B0F19',
    frame: false,
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

// =========================================================================
// IPC Handlers
// =========================================================================
ipcMain.on('window-minimize', () => {
  if (mainWindow) mainWindow.minimize();
});

ipcMain.on('window-maximize', () => {
  if (mainWindow) {
    if (mainWindow.isMaximized()) mainWindow.unmaximize();
    else mainWindow.maximize();
  }
});

ipcMain.on('window-close', () => {
  if (mainWindow) mainWindow.close();
});

const { registerClusterIpcHandlers } = createVaultAndIpcHelpers({ app, shell, safeStorage });
registerClusterIpcHandlers(ipcMain);

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
