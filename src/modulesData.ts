import manifest from '../modules.manifest.json';

export interface ModuleItem {
  seq: number;
  id: string;
  name: string;
  category: string;
  subCategory?: "Account" | "FactoryReset" | "Profile" | "Forensics";
  risk: "Safe" | "Deep" | "Risky";
  estimatedKb: number;
  description: string;
  selected: boolean;
  status: "Pending" | "Scanning" | "Optimizing" | "Done";
  timeoutSeconds?: number;
  requiresAdmin?: boolean;
}

const CUSTOM_MODULES_STORAGE_KEY = 'winpurify_custom_modules_v1';

export function loadCustomModulesFromStorage(): ModuleItem[] {
  try {
    if (typeof localStorage === 'undefined') return [];
    const raw = localStorage.getItem(CUSTOM_MODULES_STORAGE_KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? parsed : [];
  } catch {
    return [];
  }
}

export function saveCustomModulesToStorage(customModules: ModuleItem[]): void {
  try {
    if (typeof localStorage === 'undefined') return;
    localStorage.setItem(CUSTOM_MODULES_STORAGE_KEY, JSON.stringify(customModules));
  } catch {
    // Ignore storage quota errors
  }
}

export const INITIAL_MODULES: ModuleItem[] = (manifest.modules || []).map((m: any) => ({
  seq: m.seq,
  id: m.id,
  name: m.name,
  description: m.description,
  category: m.category,
  ...(m.subCategory ? { subCategory: m.subCategory as ModuleItem['subCategory'] } : {}),
  risk: (m.risk || 'Safe') as ModuleItem['risk'],
  selected: Boolean(m.isSelected),
  status: 'Pending' as const,
  estimatedKb: m.estimatedKb || 0,
  timeoutSeconds: m.timeoutSeconds || 15,
  requiresAdmin: Boolean(m.requiresAdmin)
}));
