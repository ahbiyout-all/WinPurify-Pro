import React, { useMemo, useState } from 'react';
import {
  CheckCircle2,
  Download,
  Eye,
  KeyRound,
  Plus,
  Power,
  Radio,
  RefreshCw,
  Search,
  Shield,
  ShieldAlert,
  ShieldCheck,
  Trash2,
  UserX,
  X
} from 'lucide-react';
import {
  DetectedSessionArtifact,
  DetectedUserProfile,
  HandoverExclusionRule,
  HandoverPresetMode,
  HandoverProgressState,
  HandoverScanPhase,
  HandoverVerificationSummary,
  formatBytesReadable
} from '../handoverData';

export type ThemeId = 'dark' | 'gray' | 'white' | 'beige';

interface HandoverWorkspaceProps {
  theme: ThemeId;
  phase: HandoverScanPhase;
  statusBanner: string;
  preset: HandoverPresetMode;
  profiles: DetectedUserProfile[];
  artifacts: DetectedSessionArtifact[];
  exclusions: HandoverExclusionRule[];
  progressState: HandoverProgressState | null;
  verification: HandoverVerificationSummary | null;
  isConfirmModalOpen: boolean;
  onRescan: () => void;
  onApplyPreset: (preset: HandoverPresetMode) => void;
  onToggleProfile: (sid: string) => void;
  onToggleArtifact: (artifactId: string) => void;
  onSelectAllFiltered: (artifactIds: string[]) => void;
  onDeselectAllFiltered: (artifactIds: string[]) => void;
  onSaveExclusions: (rules: HandoverExclusionRule[]) => void;
  onExportReport: (format: 'html' | 'json') => void;
  onTriggerReboot: () => void;
  onOpenConfirmModal: () => void;
  onCloseConfirmModal: () => void;
  onExecutePurge: (opts: {
    terminateProcesses: boolean;
    createSafepoint: boolean;
    schedulePendingReboot: boolean;
  }) => void;
  onDeployToLanFleet: () => void;
}

export const HandoverWorkspace: React.FC<HandoverWorkspaceProps> = ({
  theme,
  phase,
  statusBanner,
  preset,
  profiles,
  artifacts,
  exclusions,
  progressState,
  verification,
  isConfirmModalOpen,
  onRescan,
  onApplyPreset,
  onToggleProfile,
  onToggleArtifact,
  onSelectAllFiltered,
  onDeselectAllFiltered,
  onSaveExclusions,
  onExportReport,
  onTriggerReboot,
  onOpenConfirmModal,
  onCloseConfirmModal,
  onExecutePurge,
  onDeployToLanFleet
}) => {
  const [searchQuery, setSearchQuery] = useState('');
  const [domainFilter, setDomainFilter] = useState<
    'all' | 'BrowserSession' | 'AppSession' | 'CredentialAndNetwork' | 'PersonalFolder'
  >('all');
  const [terminateProcesses, setTerminateProcesses] = useState(true);
  const [createSafepoint, setCreateSafepoint] = useState(true);
  const [schedulePendingReboot, setSchedulePendingReboot] = useState(true);
  const [inspectedArtifactId, setInspectedArtifactId] = useState<string | null>(null);
  const [isExclusionModalOpen, setIsExclusionModalOpen] = useState(false);
  const [newExPath, setNewExPath] = useState('');
  const [newExLabel, setNewExLabel] = useState('');
  const [confirmRebootInline, setConfirmRebootInline] = useState(false);

  const isLight = theme === 'white' || theme === 'beige';

  const tokens = useMemo(() => {
    if (theme === 'white') {
      return {
        surface: 'bg-white border-slate-200 text-slate-900',
        surfaceSub: 'bg-slate-50 border-slate-200 text-slate-800',
        rowHover: 'hover:bg-slate-50',
        rowSelected: 'bg-sky-50/70',
        rowBorder: 'border-slate-200',
        muted: 'text-slate-500',
        secondary: 'text-slate-700',
        input: 'bg-white border-slate-300 text-slate-900 placeholder:text-slate-400',
        btnSecondary: 'bg-white hover:bg-slate-100 border-slate-300 text-slate-800',
        progressTrack: 'bg-slate-200',
        logBox: 'bg-slate-900 text-slate-200 border-slate-300',
        accentBlue: 'text-blue-700',
        accentGreen: 'text-emerald-700',
        accentAmber: 'text-amber-700',
        accentRed: 'text-rose-700'
      };
    }
    if (theme === 'beige') {
      return {
        surface: 'bg-[#FCFBF8] border-[#E6DFD3] text-[#2D241E]',
        surfaceSub: 'bg-[#F5F0E6] border-[#E6DFD3] text-[#2D241E]',
        rowHover: 'hover:bg-[#F5F0E6]/60',
        rowSelected: 'bg-[#EFE9DC]',
        rowBorder: 'border-[#E6DFD3]',
        muted: 'text-[#6E6055]',
        secondary: 'text-[#4A3F37]',
        input: 'bg-[#FCFBF8] border-[#D6CEC0] text-[#2D241E] placeholder:text-[#8C7E72]',
        btnSecondary: 'bg-[#FCFBF8] hover:bg-[#EFE9DC] border-[#D6CEC0] text-[#2D241E]',
        progressTrack: 'bg-[#E6DFD3]',
        logBox: 'bg-[#2D241E] text-[#F5F0E6] border-[#D6CEC0]',
        accentBlue: 'text-[#0369A1]',
        accentGreen: 'text-[#15803D]',
        accentAmber: 'text-[#B45309]',
        accentRed: 'text-[#BE123C]'
      };
    }
    if (theme === 'gray') {
      return {
        surface: 'bg-[#282F3A] border-[#3D4756] text-slate-100',
        surfaceSub: 'bg-[#1E232B] border-[#3D4756] text-slate-200',
        rowHover: 'hover:bg-[#323B49]',
        rowSelected: 'bg-[#323B49]/80',
        rowBorder: 'border-[#3D4756]',
        muted: 'text-slate-400',
        secondary: 'text-slate-300',
        input: 'bg-[#1E232B] border-[#4B5563] text-slate-100 placeholder:text-slate-400',
        btnSecondary: 'bg-[#1E232B] hover:bg-[#323B49] border-[#4B5563] text-slate-200',
        progressTrack: 'bg-[#151920]',
        logBox: 'bg-[#151920] text-slate-300 border-[#3D4756]',
        accentBlue: 'text-sky-400',
        accentGreen: 'text-emerald-400',
        accentAmber: 'text-amber-400',
        accentRed: 'text-rose-400'
      };
    }
    return {
      surface: 'bg-[#0F172A] border-[#1E293B] text-slate-100',
      surfaceSub: 'bg-[#0B1120] border-[#1E293B] text-slate-200',
      rowHover: 'hover:bg-[#1E293B]/60',
      rowSelected: 'bg-[#1E293B]/75',
      rowBorder: 'border-[#1E293B]',
      muted: 'text-slate-400',
      secondary: 'text-slate-300',
      input: 'bg-[#0B1120] border-[#334155] text-slate-100 placeholder:text-slate-500',
      btnSecondary: 'bg-[#1E293B] hover:bg-[#334155] border-[#334155] text-slate-200',
      progressTrack: 'bg-slate-800',
      logBox: 'bg-[#070B14] text-slate-300 border-[#1E293B]',
      accentBlue: 'text-sky-400',
      accentGreen: 'text-emerald-400',
      accentAmber: 'text-amber-400',
      accentRed: 'text-rose-400'
    };
  }, [theme]);

  const previousProfilesCount = profiles.filter(p => !p.isSystemProtected && !p.isCurrentSession).length;
  const selectedProfilesCount = profiles.filter(p => p.selected).length;
  const activeArtifactsCount = artifacts.filter(a => a.detectedCount > 0 && !a.isExcluded).length;
  const selectedArtifactsCount = artifacts.filter(a => a.selected).length;

  const selectedReclaimBytes = useMemo(() => {
    const pBytes = profiles.filter(p => p.selected).reduce((acc, p) => acc + p.estimatedBytes, 0);
    const aBytes = artifacts.filter(a => a.selected).reduce((acc, a) => acc + a.estimatedBytes, 0);
    return pBytes + aBytes;
  }, [profiles, artifacts]);

  const filteredArtifacts = useMemo(() => {
    const q = searchQuery.trim().toLowerCase();
    return artifacts.filter(a => {
      if (domainFilter !== 'all' && a.domainCategory !== domainFilter) return false;
      if (!q) return true;
      return (
        a.title.toLowerCase().includes(q) ||
        a.categoryLabel.toLowerCase().includes(q) ||
        a.targetPathsSummary.toLowerCase().includes(q) ||
        a.userScope.toLowerCase().includes(q)
      );
    });
  }, [artifacts, domainFilter, searchQuery]);

  const inspectedArtifact = useMemo(
    () => artifacts.find(a => a.artifactId === inspectedArtifactId) || null,
    [artifacts, inspectedArtifactId]
  );

  const isBusy = phase === 'scanning' || phase === 'purging';

  const handleAddArtifactExclusion = (item: DetectedSessionArtifact) => {
    const baseId = item.artifactId.split('::')[0];
    if (exclusions.some(e => e.ruleType === 'ArtifactId' && (e.value === baseId || e.value === item.artifactId))) {
      return;
    }
    const nextRule: HandoverExclusionRule = {
      id: `ex_${Date.now()}_${Math.random().toString(36).slice(2, 6)}`,
      ruleType: 'ArtifactId',
      value: item.artifactId,
      label: item.title,
      createdAt: new Date().toISOString().slice(0, 10)
    };
    onSaveExclusions([...exclusions, nextRule]);
  };

  const handleAddPathExclusion = () => {
    const trimmedPath = newExPath.trim();
    if (!trimmedPath) return;
    const label = newExLabel.trim() || trimmedPath;
    if (exclusions.some(e => e.ruleType === 'PathPrefix' && e.value.toLowerCase() === trimmedPath.toLowerCase())) {
      return;
    }
    const nextRule: HandoverExclusionRule = {
      id: `ex_${Date.now()}_${Math.random().toString(36).slice(2, 6)}`,
      ruleType: 'PathPrefix',
      value: trimmedPath,
      label,
      createdAt: new Date().toISOString().slice(0, 10)
    };
    onSaveExclusions([...exclusions, nextRule]);
    setNewExPath('');
    setNewExLabel('');
  };

  const handleRemoveExclusion = (id: string) => {
    onSaveExclusions(exclusions.filter(r => r.id !== id));
  };

  const filteredIds = filteredArtifacts
    .filter(a => !a.isProtected && !a.isExcluded)
    .map(a => a.artifactId);

  const hasPendingReboot = Boolean(verification && verification.pendingRebootPaths.length > 0);

  return (
    <div className="space-y-4">
      {/* Top Action & Metric Strip (Single-Elevation) */}
      <div className={`border rounded-lg p-5 ${tokens.surface}`}>
        <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4">
          <div className="space-y-1">
            <h2 className="text-[18px] font-semibold tracking-tight">
              새 사용자 클린 환경 인수인계 (Clean Handover Workspace)
            </h2>
            <p className={`text-[13px] ${tokens.muted}`}>
              {statusBanner}
            </p>
          </div>

          <div className="flex flex-wrap items-center gap-2">
            <button
              type="button"
              disabled={isBusy}
              onClick={onRescan}
              className={`inline-flex items-center gap-1.5 px-3 py-2 rounded-lg border text-xs font-semibold whitespace-nowrap transition cursor-pointer disabled:opacity-50 ${tokens.btnSecondary}`}
            >
              <RefreshCw className={`w-3.5 h-3.5 ${phase === 'scanning' ? 'animate-spin' : ''}`} />
              <span>환경 다시 스캔</span>
            </button>

            <button
              type="button"
              disabled={isBusy}
              onClick={() => setIsExclusionModalOpen(true)}
              className={`inline-flex items-center gap-1.5 px-3 py-2 rounded-lg border text-xs font-semibold whitespace-nowrap transition cursor-pointer disabled:opacity-50 ${tokens.btnSecondary}`}
              title="초기화 대상에서 영구 보존할 앱 또는 폴더 경로 예외 규칙을 관리합니다."
            >
              <Shield className="w-3.5 h-3.5 text-sky-500" />
              <span>예외 관리 ({exclusions.length})</span>
            </button>

            <button
              type="button"
              disabled={isBusy}
              onClick={onDeployToLanFleet}
              className={`inline-flex items-center gap-1.5 px-3 py-2 rounded-lg border text-xs font-semibold whitespace-nowrap transition cursor-pointer disabled:opacity-50 ${tokens.btnSecondary}`}
              title="작업실 LAN PC 전체에 새 사용자 클린 초기화 명령을 배포하고 PC별 검증 결과를 확인합니다."
            >
              <Radio className="w-3.5 h-3.5" />
              <span>LAN PC 일괄 배포</span>
            </button>

            <button
              type="button"
              disabled={isBusy || (selectedProfilesCount === 0 && selectedArtifactsCount === 0)}
              onClick={onOpenConfirmModal}
              className="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg bg-rose-600 hover:bg-rose-500 text-white text-xs font-semibold whitespace-nowrap transition cursor-pointer disabled:opacity-45"
            >
              <Trash2 className="w-3.5 h-3.5" />
              <span>새 환경으로 완전 초기화</span>
            </button>
          </div>
        </div>

        {/* Minimal 3-Metric Operational Strip (Zero Pills, Tabular Nums) */}
        <div className={`grid grid-cols-1 sm:grid-cols-3 gap-4 mt-4 pt-4 border-t ${tokens.rowBorder}`}>
          <div className="flex items-baseline justify-between sm:block">
            <span className={`text-[12px] font-medium ${tokens.muted}`}>탐지된 이전 사용자 프로필</span>
            <div className={`text-[22px] font-semibold tabular-nums font-mono mt-0.5 ${previousProfilesCount > 0 ? tokens.accentRed : tokens.accentGreen}`}>
              {previousProfilesCount} <span className={`text-xs font-sans font-normal ${tokens.muted}`}>계정 (선택 {selectedProfilesCount})</span>
            </div>
          </div>
          <div className="flex items-baseline justify-between sm:block">
            <span className={`text-[12px] font-medium ${tokens.muted}`}>탐지된 로그인·앱·OS 자격 흔적</span>
            <div className={`text-[22px] font-semibold tabular-nums font-mono mt-0.5 ${activeArtifactsCount > 0 ? tokens.accentAmber : tokens.accentGreen}`}>
              {activeArtifactsCount} <span className={`text-xs font-sans font-normal ${tokens.muted}`}>영역 (선택 {selectedArtifactsCount} · 보존 예외 {exclusions.length})</span>
            </div>
          </div>
          <div className="flex items-baseline justify-between sm:block">
            <span className={`text-[12px] font-medium ${tokens.muted}`}>회수 예정 공간 (선택 합계)</span>
            <div className={`text-[22px] font-semibold tabular-nums font-mono mt-0.5 ${tokens.accentGreen}`}>
              {formatBytesReadable(selectedReclaimBytes)}
            </div>
          </div>
        </div>
      </div>

      {/* Verification Proof Banner (Post-Purge) */}
      {verification && (
        <div
          className={`border rounded-lg p-4 space-y-3 ${
            verification.isCleanHandoverVerified
              ? isLight
                ? 'bg-emerald-50 border-emerald-300 text-emerald-950'
                : 'bg-emerald-950/35 border-emerald-700/70 text-emerald-100'
              : hasPendingReboot
                ? isLight
                  ? 'bg-amber-50 border-amber-300 text-amber-950'
                  : 'bg-amber-950/35 border-amber-700/70 text-amber-100'
                : isLight
                  ? 'bg-rose-50 border-rose-300 text-rose-950'
                  : 'bg-rose-950/35 border-rose-700/70 text-rose-100'
          }`}
        >
          <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-3">
            <div className="flex items-start gap-2.5">
              <CheckCircle2
                className={`w-4 h-4 mt-0.5 shrink-0 ${
                  verification.isCleanHandoverVerified
                    ? isLight ? 'text-emerald-700' : 'text-emerald-400'
                    : hasPendingReboot
                      ? isLight ? 'text-amber-700' : 'text-amber-400'
                      : isLight ? 'text-rose-700' : 'text-rose-400'
                }`}
              />
              <div className="text-[13px]">
                <div className="font-semibold">
                  {verification.isCleanHandoverVerified
                    ? '클린 환경 검증 완료 — 새 사용자 인수인계 준비됨'
                    : hasPendingReboot
                      ? `사용 중 파일 ${verification.pendingRebootPaths.length}건 재부팅 자동 삭제 예약 완료`
                      : '부분 초기화 완료 (일부 잠금 항목 확인 필요)'}
                </div>
                <div className="text-xs mt-0.5 font-mono tabular-nums opacity-90">
                  {verification.summaryMessage} · 검증 시각 {verification.completedAt}
                </div>
              </div>
            </div>

            <div className="flex flex-wrap items-center gap-2 shrink-0">
              <button
                type="button"
                onClick={() => onExportReport('html')}
                className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-md border text-xs font-semibold whitespace-nowrap cursor-pointer ${tokens.btnSecondary}`}
              >
                <Download className="w-3.5 h-3.5" />
                <span>HTML 인증서</span>
              </button>
              <button
                type="button"
                onClick={() => onExportReport('json')}
                className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-md border text-xs font-semibold whitespace-nowrap cursor-pointer ${tokens.btnSecondary}`}
              >
                <Download className="w-3.5 h-3.5" />
                <span>JSON 리포트</span>
              </button>
              {hasPendingReboot && !confirmRebootInline && (
                <button
                  type="button"
                  onClick={() => setConfirmRebootInline(true)}
                  className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-md bg-amber-600 hover:bg-amber-500 text-white text-xs font-semibold whitespace-nowrap cursor-pointer"
                >
                  <Power className="w-3.5 h-3.5" />
                  <span>지금 안전 재부팅</span>
                </button>
              )}
            </div>
          </div>

          {hasPendingReboot && (
            <div className={`border-t pt-2.5 text-xs space-y-1.5 ${isLight ? 'border-amber-200' : 'border-amber-800/60'}`}>
              <div className="font-medium">
                Windows Session Manager(PendingFileRenameOperations) 예약 경로:
              </div>
              <div className="font-mono text-[11px] max-h-20 overflow-y-auto space-y-0.5 opacity-90">
                {verification.pendingRebootPaths.map((p, idx) => (
                  <div key={idx} className="truncate">• {p}</div>
                ))}
              </div>
              {confirmRebootInline && (
                <div className="flex flex-wrap items-center justify-between gap-2 pt-1.5">
                  <span className="font-semibold">
                    재부팅 시 예약된 잠금 파일이 자동 삭제됩니다. 지금 시스템을 재부팅할까요?
                  </span>
                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      onClick={() => setConfirmRebootInline(false)}
                      className={`px-2.5 py-1 rounded border text-xs font-medium cursor-pointer ${tokens.btnSecondary}`}
                    >
                      취소
                    </button>
                    <button
                      type="button"
                      onClick={() => {
                        setConfirmRebootInline(false);
                        onTriggerReboot();
                      }}
                      className="px-3 py-1 rounded bg-rose-600 hover:bg-rose-500 text-white text-xs font-semibold cursor-pointer"
                    >
                      5초 후 재부팅 실행
                    </button>
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      )}

      {/* Purging Real-Time 4-Step Stepper & Live Log State */}
      {phase === 'purging' && (
        <div className={`border rounded-lg p-4 space-y-3 ${tokens.surface}`}>
          <div className="flex items-center justify-between text-xs font-semibold gap-2">
            <span>
              [단계 {progressState?.stepIndex || 1}/4] {progressState?.stepTitle || '잠금 프로세스 해제 및 파일/자격증명 소거 중...'}
            </span>
            <span className="font-mono tabular-nums text-rose-500 shrink-0">
              {progressState?.percent ?? 45}%
            </span>
          </div>

          <div className="grid grid-cols-4 gap-2 text-[11px]">
            {[
              { step: 1, label: '1. 잠금/예외 검증' },
              { step: 2, label: '2. 프로필/앱 소거' },
              { step: 3, label: '3. 재부팅 예약/정리' },
              { step: 4, label: '4. 2차 재스캔 검증' }
            ].map(s => {
              const current = progressState?.stepIndex || 1;
              const active = current >= s.step;
              return (
                <div
                  key={s.step}
                  className={`px-2.5 py-1.5 rounded border text-center font-medium truncate ${
                    active
                      ? isLight
                        ? 'bg-slate-900 text-white border-slate-900'
                        : 'bg-rose-950/60 text-rose-200 border-rose-700/70'
                      : `${tokens.surfaceSub} ${tokens.muted}`
                  }`}
                >
                  {s.label}
                </div>
              );
            })}
          </div>

          <div className={`w-full h-2 rounded overflow-hidden ${tokens.progressTrack}`}>
            <div
              className="h-full bg-rose-500 transition-all duration-300"
              style={{ width: `${Math.min(100, Math.max(8, progressState?.percent ?? 35))}%` }}
            />
          </div>

          {progressState && (
            <div className={`rounded border p-2.5 font-mono text-[11px] space-y-1 max-h-24 overflow-y-auto ${tokens.logBox}`}>
              {progressState.currentItemLabel && (
                <div className="text-sky-400">▶ 현재 처리 대상: {progressState.currentItemLabel}</div>
              )}
              {progressState.recentLogs.slice(0, 4).map((line, i) => (
                <div key={i} className="truncate opacity-85">{line}</div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Preset Policy Tabs + Search */}
      <div className={`border rounded-lg p-3.5 flex flex-col md:flex-row md:items-center justify-between gap-3 ${tokens.surface}`}>
        <div className="flex flex-wrap items-center gap-1.5">
          {(
            [
              { id: 'PreviousUsersAndSessions', label: '이전 사용자 + 모든 로그인 (권장)' },
              { id: 'SessionsOnly', label: '로그인·자격증명만 로그아웃' },
              { id: 'FullMachineReset', label: '현재 계정 개인폴더 포함 전체' }
            ] as const
          ).map(item => {
            const active = preset === item.id;
            return (
              <button
                key={item.id}
                type="button"
                disabled={isBusy}
                onClick={() => onApplyPreset(item.id)}
                className={`px-3 py-1.5 rounded-md text-xs font-semibold border whitespace-nowrap transition cursor-pointer disabled:opacity-50 ${
                  active
                    ? isLight
                      ? 'bg-slate-900 text-white border-slate-900'
                      : 'bg-sky-600 text-white border-sky-500'
                    : tokens.btnSecondary
                }`}
              >
                {item.label}
              </button>
            );
          })}
        </div>

        <div className="relative w-full md:w-72">
          <Search className={`w-3.5 h-3.5 absolute left-3 top-1/2 -translate-y-1/2 ${tokens.muted}`} />
          <input
            type="text"
            value={searchQuery}
            onChange={e => setSearchQuery(e.target.value)}
            placeholder="브라우저, AI/개발도구, 앱, 경로 검색..."
            className={`w-full pl-8 pr-3 py-1.5 rounded-md border text-xs focus:outline-none ${tokens.input}`}
          />
        </div>
      </div>

      {/* Main Discovery Data Grid: 12-Col Bento Split (5 : 7) */}
      <div className="grid grid-cols-1 xl:grid-cols-12 gap-4">
        {/* Left: Windows User Profiles & SID Table (5 cols) */}
        <section className={`xl:col-span-5 border rounded-lg overflow-hidden flex flex-col ${tokens.surface}`}>
          <div className={`px-4 py-2.5 border-b flex items-center justify-between ${tokens.rowBorder} ${tokens.surfaceSub}`}>
            <div className="flex items-center gap-2 text-[13px] font-semibold">
              <UserX className="w-4 h-4 text-rose-400" />
              <span>Windows 사용자 프로필 (ProfileList · C:\Users)</span>
            </div>
            <span className={`text-[11px] font-mono tabular-nums ${tokens.muted}`}>{profiles.length}건</span>
          </div>

          <div className="max-h-[420px] overflow-y-auto">
            {phase === 'scanning' ? (
              <div className="p-4 space-y-2.5">
                {[1, 2, 3].map(idx => (
                  <div key={idx} className={`h-10 rounded border animate-pulse ${tokens.surfaceSub}`} />
                ))}
              </div>
            ) : profiles.length === 0 ? (
              <div className={`p-8 text-center text-xs ${tokens.muted}`}>
                삭제 가능한 이전 사용자 프로필이 발견되지 않았습니다.
              </div>
            ) : (
              profiles.map(p => {
                const locked = p.isSystemProtected || p.isCurrentSession;
                return (
                  <div
                    key={p.sid}
                    onClick={() => !locked && !isBusy && onToggleProfile(p.sid)}
                    className={`px-4 py-2.5 flex items-center gap-3 border-b last:border-b-0 ${tokens.rowBorder} ${
                      locked ? 'opacity-65 cursor-not-allowed' : `cursor-pointer ${tokens.rowHover}`
                    }`}
                  >
                    <input
                      type="checkbox"
                      checked={p.selected}
                      disabled={locked || isBusy}
                      onChange={() => {}}
                      className="w-3.5 h-3.5 rounded accent-rose-600 shrink-0"
                    />
                    <div className="min-w-0 flex-1">
                      <div className="flex items-center justify-between gap-2">
                        <span className="text-[13px] font-semibold truncate">{p.username}</span>
                        <span className="text-[12px] font-mono tabular-nums font-medium shrink-0">
                          {p.isSystemProtected ? '보호됨' : formatBytesReadable(p.estimatedBytes)}
                        </span>
                      </div>
                      <div className={`flex items-center justify-between gap-2 text-[11px] mt-0.5 ${tokens.muted}`}>
                        <span className="font-mono truncate">{p.profilePath}</span>
                        <span className="font-mono tabular-nums shrink-0">{p.lastUsedTimeText}</span>
                      </div>
                      <div className={`text-[11px] mt-0.5 ${p.isSystemProtected ? tokens.accentGreen : tokens.muted}`}>
                        {p.accountType} · {p.statusText}
                      </div>
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </section>

        {/* Right: App / Browser / AI Dev Tools / Credential / Folder Table (7 cols) */}
        <section className={`xl:col-span-7 border rounded-lg overflow-hidden flex flex-col ${tokens.surface}`}>
          <div className={`px-4 py-2 border-b flex flex-wrap items-center justify-between gap-2 ${tokens.rowBorder} ${tokens.surfaceSub}`}>
            <div className="flex items-center gap-2 text-[13px] font-semibold">
              <KeyRound className="w-4 h-4 text-sky-400" />
              <span>앱 · 브라우저 · AI/개발도구 · 자격증명 · 개인폴더</span>
            </div>

            <div className="flex flex-wrap items-center gap-2">
              <div className="flex items-center gap-1 text-[11px]">
                {(
                  [
                    { id: 'all', label: '전체' },
                    { id: 'BrowserSession', label: '브라우저' },
                    { id: 'AppSession', label: '앱/AI/메신저' },
                    { id: 'CredentialAndNetwork', label: '자격증명' },
                    { id: 'PersonalFolder', label: '개인폴더' }
                  ] as const
                ).map(tab => (
                  <button
                    key={tab.id}
                    type="button"
                    onClick={() => setDomainFilter(tab.id)}
                    className={`px-2 py-1 rounded text-[11px] font-medium whitespace-nowrap cursor-pointer transition ${
                      domainFilter === tab.id
                        ? isLight
                          ? 'bg-slate-900 text-white'
                          : 'bg-slate-700 text-white'
                        : `${tokens.muted} hover:opacity-80`
                    }`}
                  >
                    {tab.label}
                  </button>
                ))}
              </div>

              <div className={`flex items-center gap-1 pl-2 border-l ${tokens.rowBorder}`}>
                <button
                  type="button"
                  disabled={isBusy || filteredIds.length === 0}
                  onClick={() => onSelectAllFiltered(filteredIds)}
                  className={`px-2 py-1 rounded border text-[11px] font-medium whitespace-nowrap cursor-pointer disabled:opacity-40 ${tokens.btnSecondary}`}
                >
                  전체 선택
                </button>
                <button
                  type="button"
                  disabled={isBusy || filteredIds.length === 0}
                  onClick={() => onDeselectAllFiltered(filteredIds)}
                  className={`px-2 py-1 rounded border text-[11px] font-medium whitespace-nowrap cursor-pointer disabled:opacity-40 ${tokens.btnSecondary}`}
                >
                  선택 해제
                </button>
              </div>
            </div>
          </div>

          <div className="max-h-[420px] overflow-y-auto">
            {phase === 'scanning' ? (
              <div className="p-4 space-y-2">
                {[1, 2, 3, 4, 5].map(idx => (
                  <div key={idx} className={`h-10 rounded border animate-pulse ${tokens.surfaceSub}`} />
                ))}
              </div>
            ) : filteredArtifacts.length === 0 ? (
              <div className={`p-10 text-center text-xs ${tokens.muted}`}>
                {searchQuery
                  ? `'${searchQuery}' 조건과 일치하는 탐지 항목이 없습니다.`
                  : '남아 있는 앱/브라우저 세션 또는 자격 증명 흔적이 없습니다.'}
              </div>
            ) : (
              <table className="w-full text-left border-collapse">
                <thead className={`text-[11px] border-b ${tokens.rowBorder} ${tokens.surfaceSub} ${tokens.muted}`}>
                  <tr>
                    <th className="py-2 pl-4 pr-2 w-8">선택</th>
                    <th className="py-2 px-2">대상 영역 및 경로 (행 클릭 시 상세·잠금 확인)</th>
                    <th className="py-2 px-2 w-16 text-right">탐지</th>
                    <th className="py-2 px-2 w-20 text-right">용량</th>
                    <th className="py-2 pl-1 pr-3 w-12 text-right">상세</th>
                  </tr>
                </thead>
                <tbody className="text-[12.5px]">
                  {filteredArtifacts.map(item => {
                    const isInspected = inspectedArtifactId === item.artifactId;
                    const disabled = Boolean(item.isProtected || item.isExcluded || isBusy);
                    return (
                      <tr
                        key={item.artifactId}
                        onClick={() => setInspectedArtifactId(isInspected ? null : item.artifactId)}
                        className={`border-b last:border-b-0 ${tokens.rowBorder} ${
                          isInspected
                            ? tokens.rowSelected
                            : item.isProtected || item.isExcluded
                              ? 'opacity-65 cursor-pointer'
                              : `cursor-pointer ${tokens.rowHover}`
                        }`}
                      >
                        <td
                          className="py-2.5 pl-4 pr-2 align-middle"
                          onClick={e => {
                            e.stopPropagation();
                            if (!disabled) onToggleArtifact(item.artifactId);
                          }}
                        >
                          <input
                            type="checkbox"
                            checked={item.selected}
                            disabled={disabled}
                            onChange={() => {}}
                            className="w-3.5 h-3.5 rounded accent-sky-600 cursor-pointer"
                          />
                        </td>
                        <td className="py-2.5 px-2">
                          <div className="font-medium leading-snug">
                            {item.title}
                          </div>
                          <div className={`text-[11px] font-mono truncate max-w-[390px] ${item.isExcluded ? tokens.accentBlue : tokens.muted}`}>
                            {item.categoryLabel} · {item.userScope} · {item.isExcluded ? item.exclusionReason || '보존 예외 적용됨' : item.targetPathsSummary}
                          </div>
                        </td>
                        <td className={`py-2.5 px-2 text-right font-mono tabular-nums text-[12px] ${item.detectedCount > 0 ? tokens.accentAmber : tokens.muted}`}>
                          {item.detectedCount}건
                        </td>
                        <td className={`py-2.5 px-2 text-right font-mono tabular-nums font-semibold text-[12px] ${tokens.accentGreen}`}>
                          {formatBytesReadable(item.estimatedBytes)}
                        </td>
                        <td className="py-2.5 pl-1 pr-3 text-right">
                          <button
                            type="button"
                            onClick={e => {
                              e.stopPropagation();
                              setInspectedArtifactId(isInspected ? null : item.artifactId);
                            }}
                            className={`inline-flex items-center gap-1 px-2 py-1 rounded border text-[11px] whitespace-nowrap cursor-pointer ${tokens.btnSecondary}`}
                            title="실제 파일 경로 및 잠금 프로세스 힌트 상세 보기"
                          >
                            <Eye className="w-3 h-3" />
                          </button>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            )}
          </div>
        </section>
      </div>

      {/* Detail & Lock Inspector Panel (Opens when inspecting an artifact) */}
      {inspectedArtifact && (
        <div className={`border rounded-lg p-4 space-y-3 ${tokens.surface}`}>
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <div className="text-[13px] font-semibold">{inspectedArtifact.title}</div>
              <div className={`text-xs mt-0.5 ${tokens.muted}`}>
                {inspectedArtifact.categoryLabel} · 범위: {inspectedArtifact.userScope} · {inspectedArtifact.description}
              </div>
            </div>

            <div className="flex items-center gap-2 shrink-0">
              {!inspectedArtifact.isExcluded && !inspectedArtifact.isProtected && (
                <button
                  type="button"
                  onClick={() => handleAddArtifactExclusion(inspectedArtifact)}
                  className={`inline-flex items-center gap-1 px-2.5 py-1.5 rounded border text-xs font-semibold whitespace-nowrap cursor-pointer ${tokens.btnSecondary}`}
                >
                  <Shield className="w-3.5 h-3.5 text-sky-500" />
                  <span>이 항목 보존 예외 추가</span>
                </button>
              )}
              <button
                type="button"
                onClick={() => setInspectedArtifactId(null)}
                className={`p-1.5 rounded border cursor-pointer ${tokens.btnSecondary}`}
                title="상세 패널 닫기"
              >
                <X className="w-3.5 h-3.5" />
              </button>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-3 text-xs">
            <div className={`border rounded-md p-3 ${tokens.surfaceSub}`}>
              <div className={`font-semibold mb-1.5 ${tokens.secondary}`}>탐지된 로컬 / 레지스트리 대상 경로</div>
              <div className="font-mono text-[11px] space-y-1 max-h-28 overflow-y-auto">
                {inspectedArtifact.targetPaths.length > 0 ? (
                  inspectedArtifact.targetPaths.map((p, idx) => (
                    <div key={idx} className="break-all">• {p}</div>
                  ))
                ) : (
                  <div className={tokens.muted}>경로 정보 없음</div>
                )}
              </div>
            </div>

            <div className={`border rounded-md p-3 ${tokens.surfaceSub}`}>
              <div className={`font-semibold mb-1.5 ${tokens.secondary}`}>잠금 해제 및 연관 프로세스 힌트</div>
              <div className="text-[11px] space-y-1.5">
                <div>
                  <span className={tokens.muted}>선종료 대상 프로세스: </span>
                  <span className="font-mono">
                    {inspectedArtifact.associatedProcesses && inspectedArtifact.associatedProcesses.length > 0
                      ? inspectedArtifact.associatedProcesses.join(', ')
                      : '연관 백그라운드 프로세스 없음'}
                  </span>
                </div>
                {inspectedArtifact.lockedHints && (
                  <div className={tokens.accentAmber}>잠금 안내: {inspectedArtifact.lockedHints}</div>
                )}
                {inspectedArtifact.isExcluded && (
                  <div className={tokens.accentBlue}>
                    예외 상태: {inspectedArtifact.exclusionReason || '보존 예외 목록에 의해 초기화에서 제외됨'}
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Explicit Confirmation Modal */}
      {isConfirmModalOpen && (
        <div className="fixed inset-0 bg-black/75 z-50 flex items-center justify-center p-4">
          <div className={`w-full max-w-lg border rounded-xl p-6 space-y-4 ${tokens.surface}`}>
            <div className="flex items-start gap-3">
              <div className="p-2 rounded-lg bg-rose-500/15 text-rose-500 border border-rose-500/30 shrink-0">
                <ShieldAlert className="w-5 h-5" />
              </div>
              <div>
                <h3 className="text-base font-semibold">새 사용자 클린 환경 초기화 실행 확인</h3>
                <p className={`text-xs mt-1 leading-relaxed ${tokens.muted}`}>
                  선택한 이전 Windows 사용자 프로필, 브라우저 다중 프로필 쿠키, AI/앱 로그인 토큰 및 OS 자격 증명을 영구적으로 제거합니다.{' '}
                  Windows 시스템 계정(SYSTEM, Public, Default)과 보존 예외 목록({exclusions.length}건)은 안전 가드에 의해 차단되어 보존됩니다.
                </p>
              </div>
            </div>

            <div className={`border rounded-lg p-3.5 space-y-1.5 text-xs ${tokens.surfaceSub}`}>
              <div className="flex justify-between">
                <span className={tokens.muted}>제거 대상 이전 사용자 프로필</span>
                <span className={`font-mono font-semibold tabular-nums ${tokens.accentRed}`}>{selectedProfilesCount}개 계정</span>
              </div>
              <div className="flex justify-between">
                <span className={tokens.muted}>초기화 대상 세션 · 자격증명 · 폴더</span>
                <span className={`font-mono font-semibold tabular-nums ${tokens.accentBlue}`}>{selectedArtifactsCount}개 영역</span>
              </div>
              <div className="flex justify-between">
                <span className={tokens.muted}>예상 회수 디스크 공간</span>
                <span className={`font-mono font-semibold tabular-nums ${tokens.accentGreen}`}>{formatBytesReadable(selectedReclaimBytes)}</span>
              </div>
            </div>

            <div className="space-y-2 text-xs">
              <label className="flex items-center gap-2 cursor-pointer select-none">
                <input
                  type="checkbox"
                  checked={terminateProcesses}
                  onChange={e => setTerminateProcesses(e.target.checked)}
                  className="w-3.5 h-3.5 rounded accent-rose-600"
                />
                <span>잠금 프로세스 선종료 (Chrome, Edge, Cursor, VS Code, 카카오톡, Office 프로세스 종료)</span>
              </label>
              <label className="flex items-center gap-2 cursor-pointer select-none">
                <input
                  type="checkbox"
                  checked={createSafepoint}
                  onChange={e => setCreateSafepoint(e.target.checked)}
                  className="w-3.5 h-3.5 rounded accent-sky-600"
                />
                <span>실행 직전 Live Safepoint 레지스트리 스냅샷 자동 백업 (권장)</span>
              </label>
              <label className="flex items-center gap-2 cursor-pointer select-none">
                <input
                  type="checkbox"
                  checked={schedulePendingReboot}
                  onChange={e => setSchedulePendingReboot(e.target.checked)}
                  className="w-3.5 h-3.5 rounded accent-amber-600"
                />
                <span>사용 중인 잠금 파일이 남으면 재부팅 시 자동 삭제 예약 (PendingFileRenameOperations)</span>
              </label>
            </div>

            <div className={`flex items-center justify-between pt-3 border-t ${tokens.rowBorder}`}>
              <span className={`inline-flex items-center gap-1.5 text-[11px] ${tokens.muted}`}>
                <ShieldCheck className="w-3.5 h-3.5 text-emerald-500" />
                <span>비파괴 스캔 후 명시적 1-Click 승인</span>
              </span>
              <div className="flex items-center gap-2">
                <button
                  type="button"
                  onClick={onCloseConfirmModal}
                  className={`px-3.5 py-1.5 rounded-lg border text-xs font-semibold cursor-pointer ${tokens.btnSecondary}`}
                >
                  취소
                </button>
                <button
                  type="button"
                  onClick={() => onExecutePurge({ terminateProcesses, createSafepoint, schedulePendingReboot })}
                  className="px-4 py-1.5 rounded-lg bg-rose-600 hover:bg-rose-500 text-white text-xs font-semibold cursor-pointer"
                >
                  확인 후 완전 초기화 실행
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Whitelist / Exclusion Manager Modal */}
      {isExclusionModalOpen && (
        <div className="fixed inset-0 bg-black/75 z-50 flex items-center justify-center p-4">
          <div className={`w-full max-w-xl border rounded-xl p-6 space-y-4 ${tokens.surface}`}>
            <div className="flex items-start justify-between gap-3">
              <div className="flex items-center gap-2.5">
                <Shield className="w-5 h-5 text-sky-500 shrink-0" />
                <div>
                  <h3 className="text-base font-semibold">인수인계 보존 예외 목록 (Whitelist Manager)</h3>
                  <p className={`text-xs mt-0.5 ${tokens.muted}`}>
                    여기에 등록된 앱 세션 또는 폴더 경로는 초기화 스캔 및 삭제에서 자동으로 보호됩니다.
                  </p>
                </div>
              </div>
              <button
                type="button"
                onClick={() => setIsExclusionModalOpen(false)}
                className={`p-1.5 rounded border cursor-pointer ${tokens.btnSecondary}`}
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            <div className={`border rounded-lg p-3.5 space-y-2.5 ${tokens.surfaceSub}`}>
              <div className="text-xs font-semibold">새 경로 예외 규칙 추가</div>
              <div className="grid grid-cols-1 sm:grid-cols-12 gap-2">
                <input
                  type="text"
                  value={newExPath}
                  onChange={e => setNewExPath(e.target.value)}
                  placeholder="예: C:\WorkData\SharedTemplates"
                  className={`sm:col-span-7 px-3 py-1.5 rounded border text-xs font-mono focus:outline-none ${tokens.input}`}
                />
                <input
                  type="text"
                  value={newExLabel}
                  onChange={e => setNewExLabel(e.target.value)}
                  placeholder="설명 라벨 (선택)"
                  className={`sm:col-span-3 px-3 py-1.5 rounded border text-xs focus:outline-none ${tokens.input}`}
                />
                <button
                  type="button"
                  onClick={handleAddPathExclusion}
                  disabled={!newExPath.trim()}
                  className="sm:col-span-2 inline-flex items-center justify-center gap-1 px-3 py-1.5 rounded bg-sky-600 hover:bg-sky-500 text-white text-xs font-semibold whitespace-nowrap cursor-pointer disabled:opacity-45"
                >
                  <Plus className="w-3.5 h-3.5" />
                  <span>추가</span>
                </button>
              </div>
            </div>

            <div className={`border rounded-lg overflow-hidden ${tokens.rowBorder}`}>
              <div className={`px-3.5 py-2 border-b text-xs font-semibold flex justify-between ${tokens.rowBorder} ${tokens.surfaceSub}`}>
                <span>등록된 보존 예외 규칙</span>
                <span className="font-mono tabular-nums">{exclusions.length}건</span>
              </div>
              <div className="max-h-60 overflow-y-auto divide-y divide-slate-800/30">
                {exclusions.length === 0 ? (
                  <div className={`p-6 text-center text-xs ${tokens.muted}`}>
                    등록된 보존 예외가 없습니다. 항목 상세 패널이나 위 입력창에서 추가할 수 있습니다.
                  </div>
                ) : (
                  exclusions.map(rule => (
                    <div key={rule.id} className={`px-3.5 py-2.5 flex items-center justify-between gap-3 border-b last:border-b-0 ${tokens.rowBorder}`}>
                      <div className="min-w-0">
                        <div className="text-xs font-semibold truncate">{rule.label}</div>
                        <div className={`text-[11px] font-mono truncate ${tokens.muted}`}>
                          [{rule.ruleType}] {rule.value} · 등록일 {rule.createdAt}
                        </div>
                      </div>
                      <button
                        type="button"
                        onClick={() => handleRemoveExclusion(rule.id)}
                        className={`px-2.5 py-1 rounded border text-[11px] font-semibold whitespace-nowrap cursor-pointer ${tokens.btnSecondary}`}
                      >
                        해제
                      </button>
                    </div>
                  ))
                )}
              </div>
            </div>

            <div className="flex justify-end">
              <button
                type="button"
                onClick={() => setIsExclusionModalOpen(false)}
                className={`px-4 py-1.5 rounded-lg border text-xs font-semibold cursor-pointer ${tokens.btnSecondary}`}
              >
                닫기
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
