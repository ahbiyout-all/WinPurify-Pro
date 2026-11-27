export type HandoverPresetMode = 'PreviousUsersAndSessions' | 'SessionsOnly' | 'FullMachineReset';

export type HandoverScanPhase = 'idle' | 'scanning' | 'ready' | 'purging' | 'verified';

export type HandoverDomainCategory =
  | 'BrowserSession'
  | 'AppSession'
  | 'CredentialAndNetwork'
  | 'PersonalFolder';

export interface HandoverExclusionRule {
  id: string;
  ruleType: 'ArtifactId' | 'PathPrefix';
  value: string;
  label: string;
  createdAt: string;
}

export interface HandoverProgressState {
  stepIndex: number;
  stepTitle: string;
  percent: number;
  currentItemLabel: string;
  recentLogs: string[];
}

export interface HandoverPurgedItemSnapshot {
  title: string;
  category: string;
  estimatedBytes: number;
  status: string;
  pathsSummary: string;
}

export interface FleetHandoverNodeResult {
  nodeId: string;
  nodeName: string;
  ipAddress: string;
  transportProtocol: string;
  success: boolean;
  isCleanVerified: boolean;
  pendingRebootCount: number;
  remainingCount: number;
  reclaimedText: string;
  executionTimeMs: number;
  message: string;
}

export interface DetectedUserProfile {
  sid: string;
  username: string;
  profilePath: string;
  accountType: string;
  lastUsedTimeText: string;
  isCurrentSession: boolean;
  isSystemProtected: boolean;
  estimatedBytes: number;
  selected: boolean;
  statusText: string;
  pendingRebootScheduled?: boolean;
}

export interface DetectedSessionArtifact {
  artifactId: string;
  title: string;
  domainCategory: HandoverDomainCategory;
  categoryLabel: string;
  userScope: string;
  targetPaths: string[];
  targetPathsSummary: string;
  description: string;
  detectedCount: number;
  estimatedBytes: number;
  isProtected: boolean;
  isExcluded?: boolean;
  exclusionReason?: string;
  associatedProcesses?: string[];
  lockedHints?: string;
  selected: boolean;
  statusText: string;
}

export interface HandoverVerificationSummary {
  completedAt: string;
  machineName: string;
  operatorUser: string;
  presetUsed: string;
  purgedProfilesCount: number;
  purgedSessionsCount: number;
  reclaimedBytes: number;
  remainingPreviousProfiles: number;
  remainingActiveSessions: number;
  excludedItemsCount: number;
  pendingRebootPaths: string[];
  isCleanHandoverVerified: boolean;
  summaryMessage: string;
  purgedItemsSnapshot: HandoverPurgedItemSnapshot[];
}

export function formatBytesReadable(bytes: number): string {
  if (!bytes || bytes <= 0) return '0 MB';
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  return `${(bytes / (1024 * 1024 * 1024)).toFixed(2)} GB`;
}

export function escapeHtml(raw: string): string {
  return String(raw || '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

export function buildVerificationCertificateJson(summary: HandoverVerificationSummary, appVersion: string): string {
  return JSON.stringify(
    {
      product: 'WinPurify Pro',
      version: appVersion,
      documentType: 'CleanHandoverVerificationCertificate',
      generatedAt: new Date().toISOString(),
      ...summary
    },
    null,
    2
  );
}

export function buildVerificationCertificateHtml(summary: HandoverVerificationSummary, appVersion: string): string {
  const hasPending = summary.pendingRebootPaths.length > 0;
  const statusColor = summary.isCleanHandoverVerified ? '#15803D' : hasPending ? '#B45309' : '#BE123C';
  const statusBg = summary.isCleanHandoverVerified ? '#ECFDF5' : hasPending ? '#FFFBEB' : '#FFF1F2';
  const statusLabel = summary.isCleanHandoverVerified
    ? '클린 인수인계 2차 무결성 검증 통과'
    : hasPending
      ? `재부팅 자동 삭제 예약 완료 (${summary.pendingRebootPaths.length}건)`
      : '일부 잠금 항목 확인 필요';

  const rowsHtml =
    summary.purgedItemsSnapshot.length > 0
      ? summary.purgedItemsSnapshot
          .map(
            item =>
              `<tr><td>${escapeHtml(item.title)}</td><td>${escapeHtml(item.category)}</td><td class="mono num">${escapeHtml(formatBytesReadable(item.estimatedBytes))}</td><td>${escapeHtml(item.status)}</td><td class="mono">${escapeHtml(item.pathsSummary)}</td></tr>`
          )
          .join('\n')
      : '<tr><td colspan="5" style="text-align:center;color:#64748B;">소거 상세 내역이 없습니다.</td></tr>';

  const pendingSection = hasPending
    ? `<h3>재부팅 시 자동 삭제 예약 경로 (PendingFileRenameOperations)</h3><ul>${summary.pendingRebootPaths
        .map(p => `<li class="mono">${escapeHtml(p)}</li>`)
        .join('')}</ul>`
    : '';

  return `<!DOCTYPE html>
<html lang="ko">
<head>
<meta charset="utf-8" />
<title>WinPurify Pro - Clean Handover Verification Certificate</title>
<style>
  body { font-family: 'Segoe UI', system-ui, sans-serif; background:#F8FAFC; color:#0F172A; margin:32px; line-height:1.5; }
  .sheet { max-width:960px; margin:0 auto; background:#FFFFFF; border:1px solid #CBD5E1; border-radius:10px; padding:32px; }
  .header { display:flex; justify-content:space-between; align-items:flex-start; border-bottom:1px solid #E2E8F0; padding-bottom:18px; margin-bottom:20px; gap:16px; }
  .title { font-size:21px; font-weight:700; margin:0; }
  .sub { font-size:13px; color:#475569; margin-top:4px; }
  .status { display:inline-block; padding:8px 14px; border-radius:6px; border:1px solid ${statusColor}; background:${statusBg}; color:${statusColor}; font-weight:700; font-size:13px; white-space:nowrap; }
  .grid { display:grid; grid-template-columns:repeat(4,1fr); gap:12px; margin:20px 0; }
  .metric { border:1px solid #E2E8F0; border-radius:8px; padding:12px 14px; background:#F8FAFC; }
  .metric-label { font-size:11.5px; color:#64748B; }
  .metric-val { font-size:18px; font-weight:700; margin-top:4px; font-variant-numeric:tabular-nums; }
  table { width:100%; border-collapse:collapse; margin-top:14px; font-size:12.5px; }
  th, td { border-bottom:1px solid #E2E8F0; padding:9px 10px; text-align:left; vertical-align:top; }
  th { background:#F1F5F9; color:#334155; font-weight:600; }
  .mono { font-family: Consolas, 'JetBrains Mono', monospace; font-size:11.5px; word-break:break-all; }
  .num { text-align:right; white-space:nowrap; font-variant-numeric:tabular-nums; }
  .footer { margin-top:24px; padding-top:14px; border-top:1px solid #E2E8F0; font-size:11.5px; color:#64748B; display:flex; justify-content:space-between; }
</style>
</head>
<body>
  <div class="sheet">
    <div class="header">
      <div>
        <h1 class="title">WinPurify Pro · 클린 환경 인수인계 검증 인증서</h1>
        <div class="sub">PC: ${escapeHtml(summary.machineName)} · 실행 계정: ${escapeHtml(summary.operatorUser)} · 정책: ${escapeHtml(summary.presetUsed)} · 검증 시각: ${escapeHtml(summary.completedAt)}</div>
      </div>
      <div class="status">${escapeHtml(statusLabel)}</div>
    </div>
    <p style="font-size:13.5px;margin:0 0 12px 0;">${escapeHtml(summary.summaryMessage)}</p>
    <div class="grid">
      <div class="metric"><div class="metric-label">제거된 이전 프로필</div><div class="metric-val">${summary.purgedProfilesCount}개</div></div>
      <div class="metric"><div class="metric-label">초기화된 앱/세션</div><div class="metric-val">${summary.purgedSessionsCount}영역</div></div>
      <div class="metric"><div class="metric-label">회수된 디스크 공간</div><div class="metric-val">${escapeHtml(formatBytesReadable(summary.reclaimedBytes))}</div></div>
      <div class="metric"><div class="metric-label">재부팅 예약 / 보존 예외</div><div class="metric-val">${summary.pendingRebootPaths.length}건 / ${summary.excludedItemsCount}건</div></div>
    </div>
    <h3>소거 및 검증 상세 내역</h3>
    <table>
      <thead><tr><th>대상 영역</th><th>분류</th><th class="num">회수 크기</th><th>상태</th><th>경로 요약</th></tr></thead>
      <tbody>${rowsHtml}</tbody>
    </table>
    ${pendingSection}
    <div class="footer">
      <span>Generated offline by WinPurify Pro v${escapeHtml(appVersion)} (Zero-Telemetry Verified)</span>
      <span>2차 잔여 프로필: ${summary.remainingPreviousProfiles}개 · 잔여 활성 세션: ${summary.remainingActiveSessions}건</span>
    </div>
  </div>
</body>
</html>`;
}

export const DEFAULT_DISCOVERED_PROFILES: DetectedUserProfile[] = [
  {
    sid: 'S-1-5-21-3891042211-1002',
    username: '이전사용자_Kim',
    profilePath: 'C:\\Users\\이전사용자_Kim',
    accountType: '이전 로컬 프로필 (ProfileList)',
    lastUsedTimeText: '2026-11-18',
    isCurrentSession: false,
    isSystemProtected: false,
    estimatedBytes: 4_820_000_000,
    selected: true,
    statusText: '이전 사용자 데이터 및 AppData 잔여물 탐지'
  },
  {
    sid: 'S-1-5-21-3891042211-1005',
    username: 'Staff_Lee_Temp',
    profilePath: 'C:\\Users\\Staff_Lee_Temp',
    accountType: '고립 프로필 폴더 (Orphaned)',
    lastUsedTimeText: '2026-10-29',
    isCurrentSession: false,
    isSystemProtected: false,
    estimatedBytes: 1_940_000_000,
    selected: true,
    statusText: '레지스트리 분리 잔여 디렉터리 탐지'
  },
  {
    sid: 'S-1-5-21-3891042211-1001',
    username: 'CurrentOperator',
    profilePath: 'C:\\Users\\CurrentOperator',
    accountType: '현재 활성 세션',
    lastUsedTimeText: '현재 로그인 중',
    isCurrentSession: true,
    isSystemProtected: false,
    estimatedBytes: 860_000_000,
    selected: false,
    statusText: '현재 로그인 계정 (계정 삭제 불가 · 세션 정화 가능)'
  },
  {
    sid: 'PROTECTED-PUBLIC',
    username: 'Public / Default / SYSTEM',
    profilePath: 'C:\\Users\\Public',
    accountType: 'Windows 시스템 보호 대상',
    lastUsedTimeText: 'OS 고정',
    isCurrentSession: false,
    isSystemProtected: true,
    estimatedBytes: 0,
    selected: false,
    statusText: '안전 가드에 의해 영구 보호됨'
  }
];

export const DEFAULT_DISCOVERED_ARTIFACTS: DetectedSessionArtifact[] = [
  {
    artifactId: 'chrome-multi',
    title: 'Google Chrome 전체 다중 프로필 (Default / Profile 1 / Profile 2)',
    domainCategory: 'BrowserSession',
    categoryLabel: '브라우저 세션',
    userScope: '전체 사용자 (3개 계정/프로필)',
    targetPaths: [
      'C:\\Users\\*\\AppData\\Local\\Google\\Chrome\\User Data\\Default',
      'C:\\Users\\*\\AppData\\Local\\Google\\Chrome\\User Data\\Profile *'
    ],
    targetPathsSummary: 'AppData\\Local\\Google\\Chrome\\User Data\\{Default, Profile *, Guest}',
    description: 'Chrome 다중 프로필 쿠키, Login Data, Web Data 및 자동 로그인 토큰 전수 소거.',
    associatedProcesses: ['chrome'],
    detectedCount: 3,
    estimatedBytes: 428_000_000,
    isProtected: false,
    selected: true,
    statusText: '활성 로그인 쿠키·프로필 3건 탐지'
  },
  {
    artifactId: 'edge-multi',
    title: 'Microsoft Edge 전체 다중 프로필 & Work 프로필',
    domainCategory: 'BrowserSession',
    categoryLabel: '브라우저 세션',
    userScope: '전체 사용자 (2개 프로필)',
    targetPaths: ['C:\\Users\\*\\AppData\\Local\\Microsoft\\Edge\\User Data'],
    targetPathsSummary: 'AppData\\Local\\Microsoft\\Edge\\User Data\\{Default, Profile *}',
    description: 'Edge 동기화 프로필 및 M365 자동 인증 세션 쿠키 소거.',
    associatedProcesses: ['msedge'],
    detectedCount: 2,
    estimatedBytes: 312_000_000,
    isProtected: false,
    selected: true,
    statusText: '활성 세션 2건 탐지'
  },
  {
    artifactId: 'whale-brave-firefox',
    title: 'Naver Whale · Brave · Mozilla Firefox 전체 프로필',
    domainCategory: 'BrowserSession',
    categoryLabel: '브라우저 세션',
    userScope: '전체 사용자',
    targetPaths: [
      'C:\\Users\\*\\AppData\\Local\\Naver\\Naver Whale\\User Data',
      'C:\\Users\\*\\AppData\\Roaming\\Mozilla\\Firefox\\Profiles'
    ],
    targetPathsSummary: 'Naver Whale / Brave / Firefox Profiles (cookies.sqlite, key4.db)',
    description: '국내외 보조 웹 브라우저 세션·북마크·자동 로그인 인증 소거.',
    associatedProcesses: ['whale', 'brave', 'firefox'],
    detectedCount: 2,
    estimatedBytes: 185_000_000,
    isProtected: false,
    selected: true,
    statusText: '활성 프로필 2건 탐지'
  },
  {
    artifactId: 'ms-office-wam',
    title: 'Microsoft Live / M365 Office / Teams / OneAuth (WAM)',
    domainCategory: 'AppSession',
    categoryLabel: '앱 로그인 세션',
    userScope: '현재·이전 사용자',
    targetPaths: [
      'AppData\\Local\\Microsoft\\OneAuth',
      'AppData\\Local\\Packages\\Microsoft.AAD.BrokerPlugin_cw5n1h2txyewy',
      'HKCU\\Software\\Microsoft\\Office\\16.0\\Common\\Identity'
    ],
    targetPathsSummary: 'OneAuth · TokenBroker · Office Identity Registry',
    description: 'Windows 웹 계정 관리자(WAM) 및 Office 365 연동 계정 강제 로그아웃.',
    associatedProcesses: ['Teams', 'ms-teams', 'OUTLOOK', 'WINWORD', 'EXCEL'],
    detectedCount: 4,
    estimatedBytes: 164_000_000,
    isProtected: false,
    selected: true,
    statusText: 'WAM / Office 인증 토큰 4건 탐지'
  },
  {
    artifactId: 'messengers-korea',
    title: '카카오톡 PC · Discord · Slack · Telegram · Zoom',
    domainCategory: 'AppSession',
    categoryLabel: '앱 로그인 세션',
    userScope: '전체 사용자',
    targetPaths: [
      'AppData\\Local\\Kakao\\KakaoTalk',
      'AppData\\Roaming\\discord\\Local Storage',
      'AppData\\Roaming\\Slack'
    ],
    targetPathsSummary: 'KakaoTalk · Discord · Slack · Telegram · Zoom 토큰',
    description: '메신저 자동 로그인 인증서 및 대화방 로컬 캐시 세션 초기화.',
    associatedProcesses: ['KakaoTalk', 'Discord', 'slack', 'Telegram', 'Zoom'],
    detectedCount: 3,
    estimatedBytes: 294_000_000,
    isProtected: false,
    selected: true,
    statusText: '메신저 자동로그인 3개 앱 탐지'
  },
  {
    artifactId: 'ai-dev-workstation',
    title: 'AI · 개발 도구 세션 (Cursor · VS Code Copilot · GitHub CLI · Docker · Postman · Figma)',
    domainCategory: 'AppSession',
    categoryLabel: 'AI & 개발 도구',
    userScope: '전체 사용자',
    targetPaths: [
      'AppData\\Roaming\\Cursor\\User\\globalStorage',
      'AppData\\Roaming\\Code\\User\\globalStorage',
      'AppData\\Roaming\\GitHub CLI\\hosts.yml',
      '.docker\\config.json'
    ],
    targetPathsSummary: 'Cursor · VS Code Auth · GitHub CLI · Docker · Postman · Figma',
    description: '개발용 PC 양도 시 남기 쉬운 AI 에디터, Copilot, GitHub CLI 및 컨테이너 레지스트리 토큰을 소거합니다.',
    associatedProcesses: ['Cursor', 'Code', 'Docker Desktop', 'Postman', 'Figma'],
    detectedCount: 4,
    estimatedBytes: 218_000_000,
    isProtected: false,
    selected: true,
    statusText: '개발·AI 도구 인증 캐시 4건 탐지'
  },
  {
    artifactId: 'creative-cloud-dev',
    title: 'Adobe Creative Cloud · Autodesk · OneDrive · .ssh / Git',
    domainCategory: 'AppSession',
    categoryLabel: '앱 로그인 세션',
    userScope: '전체 사용자',
    targetPaths: [
      'AppData\\Local\\Adobe\\OOBE',
      'AppData\\Local\\Microsoft\\OneDrive\\settings',
      '.ssh / .gitconfig'
    ],
    targetPathsSummary: 'Adobe OOBE · Autodesk WebServices · OneDrive · .ssh · .git-credentials',
    description: '창작/개발 도구 라이선스 세션 및 SSH/Git 자격 증명 파일 소거.',
    associatedProcesses: ['Creative Cloud', 'OneDrive', 'GoogleDriveFS'],
    detectedCount: 3,
    estimatedBytes: 142_000_000,
    isProtected: false,
    selected: true,
    statusText: '작업·개발 자격 파일 3건 탐지'
  },
  {
    artifactId: 'npki-launchers',
    title: 'NPKI / GPKI 공동인증서 · Steam · Riot · Epic 런처',
    domainCategory: 'AppSession',
    categoryLabel: '앱 로그인 세션',
    userScope: '전체 사용자',
    targetPaths: ['AppData\\LocalLow\\NPKI', 'C:\\Program Files (x86)\\Steam\\config\\loginusers.vdf'],
    targetPathsSummary: 'NPKI/GPKI 공동인증서 · Steam loginusers.vdf · Riot/Epic',
    description: '공용 PC 양도 시 치명적인 공동인증서 및 게임 런처 자동 로그인 해제.',
    associatedProcesses: ['steam', 'RiotClientServices', 'EpicGamesLauncher'],
    detectedCount: 2,
    estimatedBytes: 68_000_000,
    isProtected: false,
    selected: true,
    statusText: '공동인증서 및 런처 세션 2건 탐지'
  },
  {
    artifactId: 'os-cred-vault',
    title: 'Windows 자격 증명 관리자 (Credential Vault) · RDP · SMB',
    domainCategory: 'CredentialAndNetwork',
    categoryLabel: '자격 증명/OS 이력',
    userScope: '로컬 시스템 전체',
    targetPaths: ['Advapi32 CredEnumerateW/CredDeleteW', 'HKCU\\Software\\Microsoft\\Terminal Server Client\\Servers'],
    targetPathsSummary: 'Windows Credential Vault · RDP Mstsc · SMB Net Use',
    description: '저장된 웹사이트/네트워크 공유 암호, 원격 데스크톱 접속 기록 완전 소거.',
    associatedProcesses: ['mstsc'],
    detectedCount: 5,
    estimatedBytes: 12_000_000,
    isProtected: false,
    selected: true,
    statusText: '저장된 자격 증명 5건 탐지'
  },
  {
    artifactId: 'shell-mru-jumplists',
    title: '최근 실행 파일(Quick Access) · Jump Lists · RunMRU',
    domainCategory: 'CredentialAndNetwork',
    categoryLabel: '자격 증명/OS 이력',
    userScope: '전체 사용자',
    targetPaths: [
      'AppData\\Roaming\\Microsoft\\Windows\\Recent\\AutomaticDestinations',
      'HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\RunMRU'
    ],
    targetPathsSummary: 'Recent Documents · AutomaticDestinations · RunMRU',
    description: '탐색기 최근 열어본 파일, 고정 목록, 실행 대화상자 이력 소거.',
    associatedProcesses: [],
    detectedCount: 42,
    estimatedBytes: 24_000_000,
    isProtected: false,
    selected: true,
    statusText: '최근 사용 흔적 42건 탐지'
  },
  {
    artifactId: 'current-user-personal-folders',
    title: '현재 계정 개인 폴더 (바탕화면 · 문서 · 다운로드 · 사진 · 동영상)',
    domainCategory: 'PersonalFolder',
    categoryLabel: '개인 폴더',
    userScope: '현재 로그인 사용자',
    targetPaths: [
      'Desktop',
      'Documents',
      'Downloads',
      'Pictures',
      'Videos'
    ],
    targetPathsSummary: '%USERPROFILE%\\{Desktop, Documents, Downloads, Pictures, Videos}',
    description: '동일 계정을 물려주며 새 PC처럼 비울 때 선택합니다. (기본 해제)',
    associatedProcesses: [],
    detectedCount: 18,
    estimatedBytes: 620_000_000,
    isProtected: false,
    selected: false,
    statusText: '선택 시 현재 계정 개인 문서 비우기 실행'
  }
];

export function isArtifactExcludedByRules(artifact: DetectedSessionArtifact, rules: HandoverExclusionRule[]): HandoverExclusionRule | undefined {
  if (!rules || rules.length === 0) return undefined;
  return rules.find(r => {
    if (r.ruleType === 'ArtifactId') {
      return r.value === artifact.artifactId || artifact.artifactId.startsWith(`${r.value}::`);
    }
    const cleanEx = r.value.trim().toLowerCase().replace(/[\\/]+$/, '');
    if (!cleanEx) return false;
    return artifact.targetPaths.some(p => {
      const cleanP = p.trim().toLowerCase().replace(/[\\/]+$/, '');
      return cleanP === cleanEx || cleanP.startsWith(`${cleanEx}\\`) || cleanP.startsWith(`${cleanEx}/`);
    });
  });
}

export function applyPresetToHandoverItems(
  profiles: DetectedUserProfile[],
  artifacts: DetectedSessionArtifact[],
  preset: HandoverPresetMode,
  exclusions: HandoverExclusionRule[] = []
): { profiles: DetectedUserProfile[]; artifacts: DetectedSessionArtifact[] } {
  const nextProfiles = profiles.map(p => ({
    ...p,
    selected:
      preset === 'SessionsOnly'
        ? false
        : !p.isSystemProtected && !p.isCurrentSession
  }));

  const nextArtifacts = artifacts.map(a => {
    const matchedEx = isArtifactExcludedByRules(a, exclusions);
    const isExcluded = Boolean(a.isExcluded || matchedEx);
    const exclusionReason = a.exclusionReason || (matchedEx ? `보존 예외: ${matchedEx.label}` : '');
    if (a.isProtected || isExcluded) {
      return {
        ...a,
        isExcluded,
        exclusionReason,
        selected: false,
        statusText: exclusionReason || a.statusText
      };
    }

    let selected = false;
    if (preset === 'PreviousUsersAndSessions') {
      selected = a.domainCategory !== 'PersonalFolder';
    } else if (preset === 'SessionsOnly') {
      selected =
        a.domainCategory === 'BrowserSession' ||
        a.domainCategory === 'AppSession' ||
        a.domainCategory === 'CredentialAndNetwork';
    } else if (preset === 'FullMachineReset') {
      selected = true;
    }
    return { ...a, isExcluded: false, exclusionReason: '', selected };
  });

  return { profiles: nextProfiles, artifacts: nextArtifacts };
}
