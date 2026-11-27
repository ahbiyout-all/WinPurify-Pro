const { contextBridge, ipcRenderer } = require('electron');

contextBridge.exposeInMainWorld('electronAPI', {
  isElectron: true,
  platform: process.platform,
  version: '4.71.1',
  minimize: () => ipcRenderer.send('window-minimize'),
  maximize: () => ipcRenderer.send('window-maximize'),
  close: () => ipcRenderer.send('window-close'),
  launchCommander: () => ipcRenderer.invoke('launch-commander'),
  discoverNodes: (timeoutMs) => ipcRenderer.invoke('discover-nodes', timeoutMs),
  sendBatchCommand: (options) => ipcRenderer.invoke('send-batch-command', options),
  fetchTelemetry: (options, port) => ipcRenderer.invoke('fetch-telemetry', typeof options === 'string' ? { ip: options, port } : options),
  fetchChallenge: (options, port) => ipcRenderer.invoke('fetch-challenge', typeof options === 'string' ? { ip: options, port } : options),
  getClusterKey: () => ipcRenderer.invoke('get-cluster-key'),
  saveClusterKey: (newKey) => ipcRenderer.invoke('save-cluster-key', newKey),
  regenerateClusterKey: () => ipcRenderer.invoke('regenerate-cluster-key'),
  pairNode: (options) => ipcRenderer.invoke('pair-node', options),
  resetNodeTrust: (ip) => ipcRenderer.invoke('reset-node-trust', { ip }),
  openLogFolder: () => ipcRenderer.invoke('open-log-folder'),
  saveLogs: (content, filename) => ipcRenderer.invoke('save-logs', { content, filename }),
  scanHandover: () => ipcRenderer.invoke('scan-handover'),
  executeHandoverPurge: (options) => ipcRenderer.invoke('execute-handover-purge', options),
  getHandoverExclusions: () => ipcRenderer.invoke('get-handover-exclusions'),
  saveHandoverExclusions: (rules) => ipcRenderer.invoke('save-handover-exclusions', rules),
  exportHandoverReport: (options) => ipcRenderer.invoke('export-handover-report', options),
  triggerSafeReboot: () => ipcRenderer.invoke('trigger-safe-reboot'),
  onHandoverProgress: (callback) => {
    const handler = (_, data) => callback(data);
    ipcRenderer.on('handover-progress', handler);
    return () => ipcRenderer.removeListener('handover-progress', handler);
  },
  onTelemetryUpdate: (callback) => {
    ipcRenderer.on('telemetry-update', (_, data) => callback(data));
  }
});
