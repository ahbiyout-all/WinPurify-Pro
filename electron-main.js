/**
 * WinPurify Pro - Main Desktop Packaged App (Electron)
 * Version: v4.67.0
 */

const { app, BrowserWindow, ipcMain, shell } = require('electron');
const path = require('path');
const fs = require('fs');
const { spawn } = require('child_process');

let mainWindow = null;
let commanderWindow = null;

// Support --commander CLI flag to directly boot Master Central Commander desktop
const isCommanderMode = process.argv.includes('--commander');

function createCommanderWindow() {
  if (commanderWindow) {
    if (commanderWindow.isMinimized()) commanderWindow.restore();
    commanderWindow.focus();
    return commanderWindow;
  }

  const iconPath = path.join(__dirname, 'WinPurify_icon.png');

  commanderWindow = new BrowserWindow({
    title: 'WinPurify Pro - Master Central Commander Console v4.67.0',
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

  commanderWindow.loadFile(path.join(__dirname, 'remote-center.html'));

  commanderWindow.once('ready-to-show', () => {
    commanderWindow.show();
    commanderWindow.focus();
  });

  commanderWindow.on('closed', () => {
    commanderWindow = null;
  });

  return commanderWindow;
}

function createMainWindow() {
  const iconPath = path.join(__dirname, 'WinPurify_icon.png');

  mainWindow = new BrowserWindow({
    title: 'WinPurify Pro v4.67.0 - Enterprise System Optimization Suite',
    width: 1360,
    height: 890,
    minWidth: 1080,
    minHeight: 650,
    backgroundColor: '#0B0F19',
    show: false,
    icon: fs.existsSync(iconPath) ? iconPath : undefined,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      nodeIntegration: false,
      contextIsolation: true,
      sandbox: false
    }
  });

  mainWindow.loadFile(path.join(__dirname, 'index.html'));

  mainWindow.once('ready-to-show', () => {
    mainWindow.show();
    mainWindow.focus();
  });

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

// IPC Handlers
ipcMain.on('window-minimize', (event) => {
  const win = BrowserWindow.fromWebContents(event.sender);
  if (win) win.minimize();
});

ipcMain.on('window-maximize', (event) => {
  const win = BrowserWindow.fromWebContents(event.sender);
  if (win) {
    if (win.isMaximized()) win.unmaximize();
    else win.maximize();
  }
});

ipcMain.on('window-close', (event) => {
  const win = BrowserWindow.fromWebContents(event.sender);
  if (win) win.close();
});

// IPC Handler to launch Master Commander directly as an independent desktop window
ipcMain.handle('launch-commander', () => {
  createCommanderWindow();
  return { success: true, mode: 'standalone_desktop' };
});

app.whenReady().then(() => {
  if (isCommanderMode) {
    createCommanderWindow();
  } else {
    createMainWindow();
  }

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      if (isCommanderMode) createCommanderWindow();
      else createMainWindow();
    }
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});
