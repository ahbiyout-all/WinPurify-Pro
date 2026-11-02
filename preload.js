const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('electronAPI', {
  isElectron: true,
  platform: process.platform,
  version: '4.67.0',
  minimize: () => ipcRenderer.send('window-minimize'),
  maximize: () => ipcRenderer.send('window-maximize'),
  close: () => ipcRenderer.send('window-close'),
  launchCommander: () => ipcRenderer.invoke('launch-commander'),
  discoverNodes: (timeoutMs) => ipcRenderer.invoke('discover-nodes', timeoutMs),
  sendBatchCommand: (options) => ipcRenderer.invoke('send-batch-command', options),
  fetchTelemetry: (ip, port) => ipcRenderer.invoke('fetch-telemetry', { ip, port }),
  fetchChallenge: (ip, port) => ipcRenderer.invoke('fetch-challenge', { ip, port }),
  openLogFolder: () => ipcRenderer.invoke('open-log-folder'),
  saveLogs: (content, filename) => ipcRenderer.invoke('save-logs', { content, filename }),
  onTelemetryUpdate: (callback) => {
    ipcRenderer.on('telemetry-update', (_, data) => callback(data));
  }
});
