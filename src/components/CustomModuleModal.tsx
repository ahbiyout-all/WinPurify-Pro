import React, { useState } from 'react';
import { X, Plus, ShieldAlert, Clock } from 'lucide-react';
import { ModuleItem } from '../modulesData';

interface CustomModuleModalProps {
  isOpen: boolean;
  onClose: () => void;
  onAddModule: (mod: ModuleItem, targetPath: string) => void;
  nextSeq: number;
}

export const CustomModuleModal: React.FC<CustomModuleModalProps> = ({
  isOpen,
  onClose,
  onAddModule,
  nextSeq
}) => {
  const [name, setName] = useState('');
  const [category, setCategory] = useState('Storage');
  const [risk, setRisk] = useState<'Safe' | 'Deep' | 'Risky'>('Safe');
  const [targetPath, setTargetPath] = useState('D:\\StudioRenderCache');
  const [estimatedMb, setEstimatedMb] = useState(250);
  const [timeoutSeconds, setTimeoutSeconds] = useState(30);
  const [requiresAdmin, setRequiresAdmin] = useState(false);
  const [description, setDescription] = useState('');

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

    const newMod: ModuleItem = {
      seq: nextSeq,
      id: `custom-${Date.now().toString(36)}`,
      name: name.trim(),
      category,
      risk,
      estimatedKb: Math.max(64, estimatedMb * 1024),
      description: description.trim() || `사용자 정의 작업실 정화 모듈 (${targetPath.trim() || '직접 명령'})`,
      selected: true,
      status: 'Pending',
      timeoutSeconds: Math.min(300, Math.max(10, timeoutSeconds)),
      requiresAdmin
    };

    onAddModule(newMod, targetPath.trim());
    setName('');
    setDescription('');
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4">
      <div className="w-full max-w-lg bg-slate-900 border border-sky-500/40 rounded-2xl shadow-2xl overflow-hidden text-slate-100">
        <div className="px-5 py-4 bg-slate-950 border-b border-slate-800 flex items-center justify-between">
          <div>
            <h3 className="font-bold text-sm text-sky-300 flex items-center gap-2">
              <Plus className="w-4 h-4" />
              <span>작업실 동적 커스텀 정화 모듈 등록 (JSON Manifest Extensible)</span>
            </h3>
            <p className="text-[11px] text-slate-400 mt-0.5">
              코드 재컴파일 없이 렌더 캐시·스크래치 디스크 등 작업실 전용 정화 규칙을 즉시 추가합니다.
            </p>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition cursor-pointer"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="p-5 space-y-3.5 text-xs">
          <div>
            <label className="block text-slate-300 font-semibold mb-1">모듈 이름 *</label>
            <input
              type="text"
              required
              value={name}
              onChange={e => setName(e.target.value)}
              placeholder="예: 프리미어/블렌더 임시 렌더 캐시 소거"
              className="w-full bg-slate-950 border border-slate-700 rounded-lg px-3 py-2 text-slate-100 focus:outline-none focus:border-sky-500"
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-slate-300 font-semibold mb-1">카테고리</label>
              <select
                value={category}
                onChange={e => setCategory(e.target.value)}
                className="w-full bg-slate-950 border border-slate-700 rounded-lg px-3 py-2 text-slate-100 focus:outline-none focus:border-sky-500"
              >
                <option value="Storage">Storage (저장공간)</option>
                <option value="Browser">Browser (브라우저)</option>
                <option value="System">System (시스템 가속)</option>
                <option value="Privacy">Privacy (개인정보)</option>
                <option value="Security">Security (보안)</option>
              </select>
            </div>
            <div>
              <label className="block text-slate-300 font-semibold mb-1">위험 등급</label>
              <select
                value={risk}
                onChange={e => setRisk(e.target.value as 'Safe' | 'Deep' | 'Risky')}
                className="w-full bg-slate-950 border border-slate-700 rounded-lg px-3 py-2 text-slate-100 focus:outline-none focus:border-sky-500"
              >
                <option value="Safe">Safe (안전 기본)</option>
                <option value="Deep">Deep (심층 정화)</option>
                <option value="Risky">Risky (주의 필요)</option>
              </select>
            </div>
          </div>

          <div>
            <label className="block text-slate-300 font-semibold mb-1">대상 폴더 경로 (Target Folder)</label>
            <input
              type="text"
              value={targetPath}
              onChange={e => setTargetPath(e.target.value)}
              placeholder="예: %LocalAppData%\Adobe\Common\Media Cache Files"
              className="w-full bg-slate-950 border border-slate-700 rounded-lg px-3 py-2 text-sky-300 font-mono focus:outline-none focus:border-sky-500"
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-slate-300 font-semibold mb-1 flex items-center gap-1">
                <Clock className="w-3.5 h-3.5 text-sky-400" />
                <span>동적 타임아웃 (초)</span>
              </label>
              <input
                type="number"
                min={10}
                max={300}
                value={timeoutSeconds}
                onChange={e => setTimeoutSeconds(Number(e.target.value) || 30)}
                className="w-full bg-slate-950 border border-slate-700 rounded-lg px-3 py-2 text-slate-100 font-mono focus:outline-none focus:border-sky-500"
              />
            </div>
            <div>
              <label className="block text-slate-300 font-semibold mb-1">예상 정리 크기 (MB)</label>
              <input
                type="number"
                min={1}
                max={50000}
                value={estimatedMb}
                onChange={e => setEstimatedMb(Number(e.target.value) || 100)}
                className="w-full bg-slate-950 border border-slate-700 rounded-lg px-3 py-2 text-slate-100 font-mono focus:outline-none focus:border-sky-500"
              />
            </div>
          </div>

          <div>
            <label className="block text-slate-300 font-semibold mb-1">설명 (선택)</label>
            <input
              type="text"
              value={description}
              onChange={e => setDescription(e.target.value)}
              placeholder="모듈 동작 설명"
              className="w-full bg-slate-950 border border-slate-700 rounded-lg px-3 py-2 text-slate-100 focus:outline-none focus:border-sky-500"
            />
          </div>

          <label className="flex items-center gap-2 p-2.5 rounded-lg bg-slate-950 border border-slate-800 cursor-pointer">
            <input
              type="checkbox"
              checked={requiresAdmin}
              onChange={e => setRequiresAdmin(e.target.checked)}
              className="rounded bg-slate-900 border-slate-700 text-amber-500"
            />
            <ShieldAlert className="w-4 h-4 text-amber-400 shrink-0" />
            <span className="text-[11px] text-slate-300">
              UAC 관리자 권한 사전 검증 활성화 (HKLM·시스템 디렉터리 대상인 경우 권장)
            </span>
          </label>

          <div className="pt-2 flex items-center justify-end gap-2">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-200 font-semibold cursor-pointer"
            >
              취소
            </button>
            <button
              type="submit"
              className="px-4 py-2 rounded-lg bg-sky-600 hover:bg-sky-500 text-white font-bold cursor-pointer"
            >
              모듈 즉시 등록
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
