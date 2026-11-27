import React, { useEffect, useMemo, useState } from 'react';
import { CheckCircle2, Play, Radio, ShieldAlert, ShieldCheck, X } from 'lucide-react';
import { FleetHandoverNodeResult, HandoverPresetMode } from '../handoverData';
import type { ThemeId } from './HandoverWorkspace';

export interface FleetTargetNode {
  id: string;
  name: string;
  ip: string;
  port?: number;
  tlsPort?: number;
  group: string;
  online: boolean;
  selected: boolean;
}

interface HandoverFleetModalProps {
  theme: ThemeId;
  isOpen: boolean;
  onClose: () => void;
  nodes: FleetTargetNode[];
  defaultPreset: HandoverPresetMode;
  clusterKey: string;
  onOpenFullCommander: () => void;
  onLogAction: (msg: string) => void;
}

export const HandoverFleetModal: React.FC<HandoverFleetModalProps> = ({
  theme,
  isOpen,
  onClose,
  nodes,
  defaultPreset,
  clusterKey,
  onOpenFullCommander,
  onLogAction
}) => {
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set());
  const [preset, setPreset] = useState<HandoverPresetMode>(defaultPreset);
  const [terminateProcesses, setTerminateProcesses] = useState(true);
  const [schedulePendingReboot, setSchedulePendingReboot] = useState(true);
  const [isRunning, setIsRunning] = useState(false);
  const [results, setResults] = useState<FleetHandoverNodeResult[]>([]);

  useEffect(() => {
    if (isOpen) {
      setPreset(defaultPreset);
      setSelectedIds(new Set(nodes.filter(n => n.online && n.selected).map(n => n.id)));
      setResults([]);
    }
  }, [isOpen, defaultPreset, nodes]);

  const isLight = theme === 'white' || theme === 'beige';

  const tokens = useMemo(() => {
    if (theme === 'white') {
      return {
        surface: 'bg-white border-slate-200 text-slate-900',
        surfaceSub: 'bg-slate-50 border-slate-200 text-slate-800',
        rowBorder: 'border-slate-200',
        muted: 'text-slate-500',
        btnSecondary: 'bg-white hover:bg-slate-100 border-slate-300 text-slate-800',
        input: 'bg-white border-slate-300 text-slate-900',
        accentGreen: 'text-emerald-700',
        accentAmber: 'text-amber-700',
        accentRed: 'text-rose-700'
      };
    }
    if (theme === 'beige') {
      return {
        surface: 'bg-[#FCFBF8] border-[#E6DFD3] text-[#2D241E]',
        surfaceSub: 'bg-[#F5F0E6] border-[#E6DFD3] text-[#2D241E]',
        rowBorder: 'border-[#E6DFD3]',
        muted: 'text-[#6E6055]',
        btnSecondary: 'bg-[#FCFBF8] hover:bg-[#EFE9DC] border-[#D6CEC0] text-[#2D241E]',
        input: 'bg-[#FCFBF8] border-[#D6CEC0] text-[#2D241E]',
        accentGreen: 'text-[#15803D]',
        accentAmber: 'text-[#B45309]',
        accentRed: 'text-[#BE123C]'
      };
    }
    if (theme === 'gray') {
      return {
        surface: 'bg-[#282F3A] border-[#3D4756] text-slate-100',
        surfaceSub: 'bg-[#1E232B] border-[#3D4756] text-slate-200',
        rowBorder: 'border-[#3D4756]',
        muted: 'text-slate-400',
        btnSecondary: 'bg-[#1E232B] hover:bg-[#323B49] border-[#4B5563] text-slate-200',
        input: 'bg-[#1E232B] border-[#4B5563] text-slate-100',
        accentGreen: 'text-emerald-400',
        accentAmber: 'text-amber-400',
        accentRed: 'text-rose-400'
      };
    }
    return {
      surface: 'bg-[#0F172A] border-[#1E293B] text-slate-100',
      surfaceSub: 'bg-[#0B1120] border-[#1E293B] text-slate-200',
      rowBorder: 'border-[#1E293B]',
      muted: 'text-slate-400',
      btnSecondary: 'bg-[#1E293B] hover:bg-[#334155] border-[#334155] text-slate-200',
      input: 'bg-[#0B1120] border-[#334155] text-slate-100',
      accentGreen: 'text-emerald-400',
      accentAmber: 'text-amber-400',
      accentRed: 'text-rose-400'
    };
  }, [theme]);

  if (!isOpen) return null;

  const targetNodes = nodes.filter(n => selectedIds.has(n.id));

  const toggleNode = (id: string) => {
    setSelectedIds(prev => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const parseDetailsMetric = (details: string, regex: RegExp, fallback = 0): number => {
    const m = details.match(regex);
    return m ? Number(m[1]) || fallback : fallback;
  };

  const parseDetailsStr = (details: string, regex: RegExp, fallback = '-'): string => {
    const m = details.match(regex);
    return m ? m[1].trim() : fallback;
  };

  const handleDispatchFleetHandover = async () => {
    if (targetNodes.length === 0) return;
    setIsRunning(true);
    setResults([]);
    onLogAction(`[Fleet Handover] ${targetNodes.length}대 LAN PC에 '${preset}' 새 사용자 클린 초기화 원격 배포 시작`);

    const nextResults: FleetHandoverNodeResult[] = [];

    for (const node of targetNodes) {
      if (window.electronAPI?.sendBatchCommand) {
        try {
          const res = await window.electronAPI.sendBatchCommand({
            ip: node.ip,
            port: node.port || 9870,
            tlsPort: node.tlsPort || 9874,
            path: '/api/v1/batch/fresh-handover',
            payload: {
              commandType: 'FreshHandover',
              presetName: preset,
              preset,
              terminateProcessesBeforePurge: terminateProcesses,
              schedulePendingReboot,
              targetModules: [preset]
            },
            secretKey: clusterKey
          });
          const details = String(res.executionDetails || '');
          const pendingRebootCount = parseDetailsMetric(details, /PendingReboot:\s*(\d+)/i, 0);
          const remainingProfiles = parseDetailsMetric(details, /RemainingProfiles:\s*(\d+)/i, 0);
          const remainingSessions = parseDetailsMetric(details, /RemainingSessions:\s*(\d+)/i, 0);
          const fallbackRemaining = parseDetailsMetric(details, /Remaining:\s*(\d+)/i, 0);
          const remainingCount = (remainingProfiles + remainingSessions) || fallbackRemaining;
          const reclaimedText = parseDetailsStr(details, /Reclaimed:\s*([^,|]+)/i, '-');
          const verifiedMatch = details.match(/Verified:\s*(true|false)/i);
          const verifiedFlag = verifiedMatch ? verifiedMatch[1].toLowerCase() === 'true' : true;
          const isCleanVerified = Boolean(res.success && verifiedFlag && pendingRebootCount === 0 && remainingCount === 0);

          nextResults.push({
            nodeId: node.id,
            nodeName: node.name,
            ipAddress: node.ip,
            transportProtocol: res.transportProtocol || 'HTTPS-TLS',
            success: Boolean(res.success),
            isCleanVerified,
            pendingRebootCount,
            remainingCount,
            reclaimedText,
            executionTimeMs: Number(res.executionTimeMs) || 0,
            message: res.message || details || '완료'
          });
        } catch (err: any) {
          nextResults.push({
            nodeId: node.id,
            nodeName: node.name,
            ipAddress: node.ip,
            transportProtocol: 'TLS Error',
            success: false,
            isCleanVerified: false,
            pendingRebootCount: 0,
            remainingCount: 1,
            reclaimedText: '-',
            executionTimeMs: 0,
            message: err?.message || '연결 실패'
          });
        }
      } else {
        await new Promise(r => setTimeout(r, 120));
        nextResults.push({
          nodeId: node.id,
          nodeName: node.name,
          ipAddress: node.ip,
          transportProtocol: 'HTTPS-TLS',
          success: true,
          isCleanVerified: true,
          pendingRebootCount: 0,
          remainingCount: 0,
          reclaimedText: '2.4 GB',
          executionTimeMs: 410 + Math.floor(Math.random() * 240),
          message: `시뮬레이션 검증 완료 (${preset})`
        });
      }
      setResults([...nextResults]);
    }

    const okCount = nextResults.filter(r => r.success).length;
    onLogAction(`[Fleet Handover 완료] ${okCount}/${nextResults.length}대 PC 인수인계 검증 완료`);
    setIsRunning(false);
  };

  return (
    <div className="fixed inset-0 bg-black/75 z-50 flex items-center justify-center p-4">
      <div className={`w-full max-w-4xl border rounded-xl overflow-hidden flex flex-col max-h-[90vh] ${tokens.surface}`}>
        {/* Modal Header */}
        <div className={`px-5 py-4 border-b flex items-center justify-between gap-3 ${tokens.rowBorder} ${tokens.surfaceSub}`}>
          <div className="flex items-center gap-2.5">
            <Radio className="w-5 h-5 text-sky-500 shrink-0" />
            <div>
              <h3 className="text-base font-semibold">LAN 작업실 원격 일괄 인수인계 (Fleet Clean Handover)</h3>
              <p className={`text-xs ${tokens.muted}`}>
                Central Commander 프로토콜(TLS:9874 + Challenge Nonce)로 여러 PC의 이전 사용자·세션을 동시 초기화하고 검증 결과를 수집합니다.
              </p>
            </div>
          </div>
          <button
            type="button"
            onClick={onClose}
            className={`p-1.5 rounded border cursor-pointer ${tokens.btnSecondary}`}
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Body */}
        <div className="p-5 overflow-y-auto space-y-4">
          {/* Policy & Options */}
          <div className={`border rounded-lg p-4 space-y-3 ${tokens.surfaceSub}`}>
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-3">
              <div className="space-y-1">
                <div className="text-xs font-semibold">원격 배포 정책 (Handover Preset)</div>
                <select
                  value={preset}
                  disabled={isRunning}
                  onChange={e => setPreset(e.target.value as HandoverPresetMode)}
                  className={`px-3 py-1.5 rounded-md border text-xs font-semibold focus:outline-none ${tokens.input}`}
                >
                  <option value="PreviousUsersAndSessions">이전 사용자 프로필 + 모든 로그인 세션 초기화 (권장)</option>
                  <option value="SessionsOnly">앱·브라우저 로그인 및 OS 자격증명만 로그아웃</option>
                  <option value="FullMachineReset">현재 계정 개인폴더 포함 공장 수준 클린 초기화</option>
                </select>
              </div>

              <div className="flex flex-wrap items-center gap-4 text-xs">
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={terminateProcesses}
                    disabled={isRunning}
                    onChange={e => setTerminateProcesses(e.target.checked)}
                    className="w-3.5 h-3.5 rounded accent-rose-600"
                  />
                  <span>원격 잠금 프로세스 선종료</span>
                </label>
                <label className="flex items-center gap-1.5 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={schedulePendingReboot}
                    disabled={isRunning}
                    onChange={e => setSchedulePendingReboot(e.target.checked)}
                    className="w-3.5 h-3.5 rounded accent-amber-600"
                  />
                  <span>잠금 파일 재부팅 예약 허용</span>
                </label>
              </div>
            </div>
          </div>

          {/* Target Node Checklist */}
          <div className={`border rounded-lg overflow-hidden ${tokens.rowBorder}`}>
            <div className={`px-4 py-2 border-b flex items-center justify-between ${tokens.rowBorder} ${tokens.surfaceSub}`}>
              <span className="text-xs font-semibold">배포 대상 LAN PC ({selectedIds.size}/{nodes.length}대 선택)</span>
              <div className="flex items-center gap-1.5">
                <button
                  type="button"
                  disabled={isRunning}
                  onClick={() => setSelectedIds(new Set(nodes.filter(n => n.online).map(n => n.id)))}
                  className={`px-2.5 py-1 rounded border text-[11px] font-medium cursor-pointer ${tokens.btnSecondary}`}
                >
                  온라인 전체 선택
                </button>
                <button
                  type="button"
                  disabled={isRunning}
                  onClick={() => setSelectedIds(new Set())}
                  className={`px-2.5 py-1 rounded border text-[11px] font-medium cursor-pointer ${tokens.btnSecondary}`}
                >
                  선택 해제
                </button>
              </div>
            </div>

            <div className="max-h-48 overflow-y-auto divide-y divide-slate-800/30">
              {nodes.map(n => (
                <label
                  key={n.id}
                  className={`px-4 py-2 flex items-center justify-between gap-3 text-xs cursor-pointer border-b last:border-b-0 ${tokens.rowBorder}`}
                >
                  <div className="flex items-center gap-2.5 min-w-0">
                    <input
                      type="checkbox"
                      checked={selectedIds.has(n.id)}
                      disabled={isRunning || !n.online}
                      onChange={() => toggleNode(n.id)}
                      className="w-3.5 h-3.5 rounded accent-sky-600"
                    />
                    <span className="font-semibold truncate">{n.name}</span>
                    <span className={`font-mono ${tokens.muted}`}>{n.ip}</span>
                  </div>
                  <span className={`text-[11px] shrink-0 ${n.online ? tokens.accentGreen : tokens.muted}`}>
                    {n.group} · {n.online ? 'ONLINE (TLS:9874)' : 'OFFLINE'}
                  </span>
                </label>
              ))}
            </div>
          </div>

          {/* Fleet Verification Matrix Table */}
          {results.length > 0 && (
            <div className={`border rounded-lg overflow-hidden ${tokens.rowBorder}`}>
              <div className={`px-4 py-2 border-b text-xs font-semibold flex items-center justify-between ${tokens.rowBorder} ${tokens.surfaceSub}`}>
                <span>PC별 인수인계 실행 및 검증 매트릭스</span>
                <span className="font-mono tabular-nums">
                  통과 {results.filter(r => r.isCleanVerified).length} / 재부팅 예약 {results.filter(r => r.pendingRebootCount > 0).length}
                </span>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse text-xs">
                  <thead className={`text-[11px] border-b ${tokens.rowBorder} ${tokens.surfaceSub} ${tokens.muted}`}>
                    <tr>
                      <th className="py-2 px-3">PC / IP</th>
                      <th className="py-2 px-3">채널</th>
                      <th className="py-2 px-3 text-right">회수/소요</th>
                      <th className="py-2 px-3 text-right">재부팅 예약</th>
                      <th className="py-2 px-3">검증 상태</th>
                      <th className="py-2 px-3">결과 메시지</th>
                    </tr>
                  </thead>
                  <tbody>
                    {results.map(r => (
                      <tr key={r.nodeId} className={`border-b last:border-b-0 ${tokens.rowBorder}`}>
                        <td className="py-2 px-3">
                          <div className="font-semibold">{r.nodeName}</div>
                          <div className={`text-[11px] font-mono ${tokens.muted}`}>{r.ipAddress}</div>
                        </td>
                        <td className="py-2 px-3 font-mono text-[11px]">{r.transportProtocol}</td>
                        <td className="py-2 px-3 text-right font-mono tabular-nums">
                          {r.reclaimedText} · {r.executionTimeMs}ms
                        </td>
                        <td className="py-2 px-3 text-right font-mono tabular-nums">
                          {r.pendingRebootCount}건
                        </td>
                        <td className="py-2 px-3 font-semibold whitespace-nowrap">
                          {r.isCleanVerified ? (
                            <span className={`inline-flex items-center gap-1 ${tokens.accentGreen}`}>
                              <CheckCircle2 className="w-3.5 h-3.5" /> 검증 통과
                            </span>
                          ) : r.success && r.pendingRebootCount > 0 ? (
                            <span className={tokens.accentAmber}>재부팅 대기</span>
                          ) : (
                            <span className={`inline-flex items-center gap-1 ${tokens.accentRed}`}>
                              <ShieldAlert className="w-3.5 h-3.5" /> 확인 필요
                            </span>
                          )}
                        </td>
                        <td className={`py-2 px-3 text-[11px] max-w-[240px] truncate ${tokens.muted}`} title={r.message}>
                          {r.message}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className={`px-5 py-3.5 border-t flex flex-wrap items-center justify-between gap-2 ${tokens.rowBorder} ${tokens.surfaceSub}`}>
          <div className="flex items-center gap-2 text-[11px]">
            <ShieldCheck className={`w-4 h-4 ${isLight ? 'text-emerald-700' : 'text-emerald-400'}`} />
            <span className={tokens.muted}>HMAC-SHA256 + Challenge Nonce 일회성 서명 보호</span>
          </div>

          <div className="flex items-center gap-2">
            <button
              type="button"
              onClick={() => {
                onClose();
                onOpenFullCommander();
              }}
              className={`px-3 py-1.5 rounded-lg border text-xs font-semibold cursor-pointer ${tokens.btnSecondary}`}
            >
              전체 Central Commander 열기
            </button>
            <button
              type="button"
              disabled={isRunning || targetNodes.length === 0}
              onClick={() => void handleDispatchFleetHandover()}
              className="inline-flex items-center gap-1.5 px-4 py-1.5 rounded-lg bg-rose-600 hover:bg-rose-500 text-white text-xs font-semibold whitespace-nowrap cursor-pointer disabled:opacity-45"
            >
              <Play className="w-3.5 h-3.5" />
              <span>{isRunning ? '원격 배포 실행 중...' : `선택 ${targetNodes.length}대 PC 일괄 초기화 실행`}</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
