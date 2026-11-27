import React, { useState } from 'react';
import { Search, RefreshCw, KeyRound } from 'lucide-react';
import { generateHighEntropyClusterKey, generateStudioPairingPin, saveClusterKey } from '../securityVault';

interface CommanderSecurityStripProps {
  clusterKey: string;
  setClusterKey: (key: string) => void;
  isScanningNodes: boolean;
  onDiscoverNodes: () => void;
  onRefreshTelemetry: () => void;
  onAddLog: (msg: string) => void;
  onAddCommanderLog: (msg: string) => void;
}

export const CommanderSecurityStrip: React.FC<CommanderSecurityStripProps> = ({
  clusterKey,
  setClusterKey,
  isScanningNodes,
  onDiscoverNodes,
  onRefreshTelemetry,
  onAddLog,
  onAddCommanderLog
}) => {
  const [localPin, setLocalPin] = useState<string>('');
  const [remoteIp, setRemoteIp] = useState<string>('192.168.1.105');
  const [remotePinInput, setRemotePinInput] = useState<string>('');

  const handleGenerateKey = async () => {
    if (typeof window !== 'undefined' && window.electronAPI?.regenerateClusterKey) {
      try {
        const res = await window.electronAPI.regenerateClusterKey();
        if (res?.success && res.clusterKey) {
          setClusterKey(res.clusterKey);
          saveClusterKey(res.clusterKey);
          onAddLog('[보안 금고] Electron safeStorage 암호화 금고에 256-bit 클러스터 마스터 키가 갱신 저장되었습니다.');
          onAddCommanderLog('[보안 금고] 신규 256-bit 클러스터 마스터 키 갱신 완료 (safeStorage 암호화 활성)');
          return;
        }
      } catch (err: any) {
        onAddCommanderLog(`[보안 금고 경고] Electron 키 생성 실패, 로컬 키 생성 폴백: ${err?.message || err}`);
      }
    }
    const newKey = generateHighEntropyClusterKey();
    setClusterKey(newKey);
    saveClusterKey(newKey);
    onAddLog('[보안 금고] 새로운 256-bit 고엔트로피 클러스터 마스터 키가 생성되었습니다.');
    onAddCommanderLog('[보안 금고] 신규 256-bit 클러스터 마스터 키 갱신 완료 (평문 전송 차단 활성)');
  };

  const handleIssuePin = () => {
    const pin = generateStudioPairingPin();
    setLocalPin(pin);
    onAddCommanderLog(`[PIN 페어링] 6자리 일회용 작업실 페어링 PIN 발급됨: ${pin} (TLS 9874 전용, 3분 후 자동 만료)`);
  };

  const handlePairRemote = async () => {
    const pin = remotePinInput.trim();
    const targetIp = remoteIp.trim();
    if (pin.length !== 6) {
      onAddCommanderLog('[PIN 페어링 경고] 6자리 숫자 PIN을 입력하세요.');
      return;
    }
    if (!targetIp) {
      onAddCommanderLog('[PIN 페어링 경고] 대상 PC의 IP 주소를 입력하세요.');
      return;
    }

    if (typeof window !== 'undefined' && window.electronAPI?.pairNode) {
      onAddCommanderLog(`[TLS 페어링] ${targetIp}:9874 노드에 6자리 PIN 인증 요청 전송 중...`);
      try {
        const res = await window.electronAPI.pairNode({ ip: targetIp, tlsPort: 9874, pairingPin: pin });
        if (res?.success && res.clusterKey) {
          setClusterKey(res.clusterKey);
          saveClusterKey(res.clusterKey);
          setRemotePinInput('');
          onAddLog(`[TLS 페어링 완료] ${targetIp}:9874 TOFU 인증서 지문 고정 및 마스터 키 동기화 성공`);
          onAddCommanderLog(`[TLS 페어링 완료] ${targetIp}:9874 노드에 PIN 검증 및 TOFU 인증서 지문 고정 완료 (${res.elapsedMs || 0}ms).`);
        } else {
          onAddCommanderLog(`[TLS 페어링 실패] ${targetIp}:9874 -> ${res?.message || '인증 실패'}`);
        }
      } catch (err: any) {
        onAddCommanderLog(`[TLS 페어링 오류] ${targetIp}:9874 -> ${err?.message || err}`);
      }
      return;
    }

    onAddCommanderLog(`[웹 미리보기] ${targetIp}:9874 6자리 PIN(${pin}) 로컬 시뮬레이션 검증됨 (실제 TLS 핸드셰이크는 Electron/WPF 데스크톱에서 실행).`);
    setRemotePinInput('');
  };

  return (
    <div className="px-6 py-3 bg-slate-950/40 border-b border-slate-800 space-y-2.5 text-xs">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-3">
          <div className="flex items-center gap-2">
            <span className="text-slate-400 font-medium">🔒 클러스터 마스터 키:</span>
            <input
              type="text"
              value={clusterKey}
              onChange={e => {
                const nextKey = e.target.value;
                setClusterKey(nextKey);
                saveClusterKey(nextKey);
                if (typeof window !== 'undefined' && window.electronAPI?.saveClusterKey && nextKey.trim().length >= 8) {
                  window.electronAPI.saveClusterKey(nextKey.trim()).catch(() => {});
                }
              }}
              className="bg-slate-900 border border-slate-700 rounded px-2.5 py-1 text-sky-400 font-mono font-bold text-xs w-56 focus:outline-none focus:border-sky-500"
            />
            <button
              onClick={handleGenerateKey}
              className="px-2 py-1 rounded bg-slate-800 hover:bg-slate-700 text-sky-300 text-[11px] font-semibold border border-slate-700 cursor-pointer transition"
              title="암호학적 고유 난수 키 즉시 생성"
            >
              🎲 고유 난수 키 생성
            </button>
          </div>
          <div
            className="flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-emerald-500/10 border border-emerald-500/30 text-emerald-300 font-medium text-[11px]"
            title="TLS 1.2/1.3 (TCP 9874), ECDSA P-256 TOFU 인증서 피닝, HMAC-SHA256 무결성 검증, Zero Plaintext Secret 전송이 적용되어 있습니다."
          >
            <span className="text-emerald-400">🛡️</span>
            <span>보안 아키텍처: <strong>TLS 9874 (TOFU 피닝) • DPAPI 금고 • HMAC-SHA256 (평문 키 미전송)</strong></span>
          </div>
        </div>

        <div className="flex items-center gap-2">
          <button
            disabled={isScanningNodes}
            onClick={onDiscoverNodes}
            className="px-3 py-1.5 rounded-lg bg-blue-600 hover:bg-blue-500 text-white font-bold text-xs flex items-center gap-1.5 transition disabled:opacity-50 cursor-pointer"
          >
            <Search className="w-3.5 h-3.5" />
            <span>{isScanningNodes ? '탐색 중...' : '하이브리드 LAN 탐색 (UDP+/24)'}</span>
          </button>
          <button
            onClick={onRefreshTelemetry}
            className="px-3 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-200 font-semibold text-xs flex items-center gap-1.5 transition cursor-pointer"
          >
            <RefreshCw className="w-3.5 h-3.5" />
            <span>서명된 텔레메트리 갱신</span>
          </button>
        </div>
      </div>

      {/* Zero-Config 6-Digit PIN Studio Pairing Bar */}
      <div className="flex flex-wrap items-center justify-between gap-2 pt-2 border-t border-slate-800/70">
        <div className="flex flex-wrap items-center gap-2">
          <span className="text-amber-300 font-semibold flex items-center gap-1">
            <KeyRound className="w-3.5 h-3.5" />
            <span>작업실 1-Click 6자리 PIN 페어링:</span>
          </span>
          <button
            onClick={handleIssuePin}
            className="px-2.5 py-1 rounded bg-amber-600/20 hover:bg-amber-600/30 border border-amber-500/40 text-amber-300 font-bold text-[11px] cursor-pointer transition"
          >
            🔑 내 PC PIN 발급
          </button>
          {localPin && (
            <span className="px-2.5 py-1 rounded bg-amber-500/20 border border-amber-400 text-amber-200 font-mono font-extrabold tracking-widest">
              PIN: {localPin}
            </span>
          )}
        </div>

        <div className="flex items-center gap-2">
          <input
            type="text"
            value={remoteIp}
            onChange={e => setRemoteIp(e.target.value)}
            placeholder="대상 PC IP"
            className="bg-slate-900 border border-slate-700 rounded px-2 py-1 text-slate-200 font-mono text-[11px] w-28 focus:outline-none focus:border-amber-500"
          />
          <input
            type="text"
            maxLength={6}
            value={remotePinInput}
            onChange={e => setRemotePinInput(e.target.value)}
            placeholder="6자리 PIN"
            className="bg-slate-900 border border-slate-700 rounded px-2 py-1 text-amber-300 font-mono font-bold text-[11px] w-24 text-center focus:outline-none focus:border-amber-500"
          />
          <button
            onClick={handlePairRemote}
            className="px-2.5 py-1 rounded bg-emerald-600 hover:bg-emerald-500 text-white font-bold text-[11px] cursor-pointer transition"
          >
            TLS 페어링 연결
          </button>
        </div>
      </div>
    </div>
  );
};
