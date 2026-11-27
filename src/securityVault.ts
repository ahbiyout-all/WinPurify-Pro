const CLUSTER_KEY_STORAGE_KEY = 'winpurify_cluster_key_v2';

export function generateHighEntropyClusterKey(): string {
  const bytes = new Uint8Array(24);
  if (typeof window !== 'undefined' && window.crypto?.getRandomValues) {
    window.crypto.getRandomValues(bytes);
  } else {
    for (let i = 0; i < bytes.length; i++) {
      bytes[i] = Math.floor(Math.random() * 256);
    }
  }
  const hex = Array.from(bytes, b => b.toString(16).padStart(2, '0')).join('').toUpperCase();
  return `WPP-STD-${hex}`;
}

export function getOrCreateClusterKey(): string {
  try {
    if (typeof localStorage !== 'undefined') {
      const existing = localStorage.getItem(CLUSTER_KEY_STORAGE_KEY);
      if (existing && existing.length >= 16) {
        return existing;
      }
      const generated = generateHighEntropyClusterKey();
      localStorage.setItem(CLUSTER_KEY_STORAGE_KEY, generated);
      return generated;
    }
  } catch {
    // Ignore storage access restriction
  }
  return generateHighEntropyClusterKey();
}

export function saveClusterKey(key: string): void {
  try {
    if (typeof localStorage !== 'undefined' && key.trim().length >= 8) {
      localStorage.setItem(CLUSTER_KEY_STORAGE_KEY, key.trim());
    }
  } catch {
    // Ignore storage access restriction
  }
}

export function generateStudioPairingPin(): string {
  const arr = new Uint32Array(1);
  if (typeof window !== 'undefined' && window.crypto?.getRandomValues) {
    window.crypto.getRandomValues(arr);
  } else {
    arr[0] = Math.floor(Math.random() * 1000000);
  }
  return String(100000 + (arr[0] % 900000));
}
