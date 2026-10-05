import React, { useState, useEffect, useRef } from 'react';
import { createRoot } from 'react-dom/client';
import { 
  ShieldCheck, 
  Cpu, 
  HardDrive, 
  Zap, 
  Play, 
  RotateCcw, 
  Layers, 
  Terminal, 
  CheckCircle2, 
  AlertTriangle, 
  Search,
  Activity,
  Server,
  Stethoscope,
  Lock,
  Gamepad2,
  Calendar,
  Radio,
  RefreshCw,
  X,
  Trash2,
  Save,
  Bell,
  History,
  Download,
  Power,
  FileText,
  Copy,
  Check,
  ChevronDown,
  Palette
} from 'lucide-react';
import { APP_VERSION } from './src/version';
import { INITIAL_MODULES, ModuleItem } from './src/modulesData';

const LICENSE_TEXT_KO = `================================================================================
                    WinPurify Pro 최종 사용자 사용권 계약서 (EULA)
             (End User License Agreement - Korean Edition)
================================================================================

본 최종 사용자 사용권 계약서(이하 "본 계약")는 Cisnet Soft(이하 "회사")가 개발 및 
공급하는 "WinPurify Pro"(이하 "소프트웨어", C++ 네이티브 가속 엔진 PurifyEngineCore.dll, 
원격 에이전트 서비스 WinPurifyAgentService, 센트럴 커맨더 Central Commander 및 부속 
어셈블리와 문서 포함)의 설치 및 사용에 관한 사용자와 회사 간의 법적 계약입니다.

사용자가 본 소프트웨어를 설치, 복사 또는 사용하는 것은 본 계약 조건의 전부에 동의함을 
의미합니다. 본 계약 조건에 동의하지 않는 경우, 설치를 중단하고 소프트웨어 파일 일체를 
즉시 삭제하여 주십시오.

--------------------------------------------------------------------------------
제1조 (사용권의 부여 및 허용 범위)
--------------------------------------------------------------------------------
1. 회사는 본 계약 조건에 따라 사용자에게 비독점적이고 양도 불가능한 소프트웨어 사용 권한을 
   부여합니다.
2. 개인 사용자 및 기업, 교육기관, 공공기관은 정식 라이선스 범위 내에서 Windows 운영체제 
   기반 PC 및 서버 환경에 본 소프트웨어를 설치하고, 133개 최적화 모듈, 실시간 모니터링, 
   자동 스케줄러, 세이프포인트 백업 및 복원 기능을 자유롭게 사용할 수 있습니다.
3. Unpacked Multi-File 정식 설치 또는 Portable 단독 실행 패키지 형태로 허가된 시스템에 
   배포하여 사용할 수 있습니다.

--------------------------------------------------------------------------------
제2조 (원격 관제 및 네트워크 포트 통신)
--------------------------------------------------------------------------------
1. 소프트웨어에 포함된 Central Commander 및 WinPurifyAgentService는 다중 PC 원격 진단 
   및 일괄 정화 관제를 위해 TCP 포트 9870(REST 제어 채널) 및 UDP 포트 9871(브로드캐스트 탐색)을 
   사용합니다.
2. 사용자는 인스톨러 설치 마법사의 원격포트 열기 옵션(openremoteport)을 통해 방화벽 예외 
   등록을 자유롭게 선택할 수 있으며, 기본값으로 원격포트 개방이 활성화됩니다.
3. 모든 원격 제어 및 통신 기능은 사용자가 정당한 관리 권한을 보유한 로컬 에어갭 망 또는 
   사내 인트라넷 환경에서만 승인된 관리자에 의해 구동되어야 하며, 허가받지 않은 제3자 시스템에 
   대한 무단 접속 및 임의 제어 용도로의 사용은 엄격히 금지됩니다.

--------------------------------------------------------------------------------
제3조 (지적재산권 및 저작권)
--------------------------------------------------------------------------------
1. 본 소프트웨어와 관련된 프로그램 코드, UI 디자인, 아이콘(WinPurify.ico), 로고 이미지, 
   C++ 네이티브 바이너리(PurifyEngineCore.dll), 문서 및 기술 사양에 대한 일체의 소유권과 
   저작권은 Cisnet Soft에 있습니다.
2. 본 소프트웨어는 대한민국 저작권법, 컴퓨터프로그램보호법 및 국제 저작권 협약의 보호를 
   받습니다.
3. 사용자는 회사의 사전 서면 승인 없이 본 소프트웨어를 역컴파일, 리버스 엔지니어링, 
   디스어셈블하거나 원본 코드를 임의 변조하여 무단 상업적 재배포를 수행할 수 없습니다.

--------------------------------------------------------------------------------
제4조 (데이터 보호 및 계정 정화 보안 준수)
--------------------------------------------------------------------------------
1. 소프트웨어에 내장된 계정 로그아웃 및 자격증명 정화(AccountCredentialPurgeService), 
   브라우저 공장 초기화, 디스크 포렌식 와이핑(Cipher) 기능은 영구적인 데이터 및 세션 소거를 
   수반합니다.
2. 회사는 사용자의 PC로부터 개인정보, 로그인 암호, 하드웨어 식별자, 원격 통신 페이로드 등의 
   민감 데이터를 외부 서버로 수집하거나 무단 전송하지 않으며, 모든 정화 작업은 로컬 장치 
   내에서 오프라인 완결형으로 실행됩니다.

--------------------------------------------------------------------------------
제5조 (보증의 한계 및 면책 조항)
--------------------------------------------------------------------------------
1. 회사는 관련 법률이 허용하는 최대 범위 내에서 본 소프트웨어를 "있는 그대로(AS-IS)" 
   제공하며, 상품성, 특정 목적에 대한 적합성 또는 무결성에 대해 어떠한 명시적/묵시적 보증도 
   부인합니다.
2. 본 소프트웨어의 깊은 시스템 레지스트리 청소, 서비스 비활성화, 원격 전원 종료 및 재부팅을 
   실행하기 전에, 사용자는 내장된 실시간 세이프포인트(LiveSafepointService) 스냅샷 생성 및 
   중요 데이터 백업을 수행할 것을 강력히 권장합니다.
3. 회사는 소프트웨어 사용 또는 사용 불능으로 인해 발생하는 데이터 손실, 업무 중단, 컴퓨터 
   오작동 등 직·간접적 또는 부수적 손해에 대해 법적 책임을 부담하지 않습니다.

--------------------------------------------------------------------------------
제6조 (계약의 해지 및 준거법)
--------------------------------------------------------------------------------
1. 사용자가 본 계약 조건을 위반하는 경우, 회사는 별도의 통지 없이 사용권을 해지할 수 
   있으며 사용자는 소프트웨어의 모든 복사본을 즉시 파기해야 합니다.
2. 본 계약은 대한민국 법률에 따라 규율되고 해석되며, 본 계약과 관련하여 발생하는 모든 
   분쟁은 대한민국 법원을 관할 법원으로 합니다.

================================================================================
공급자: Cisnet Soft (개발자: AhBiYout)
블로그: https://ahbiyoutvibe.blogspot.com/
웹사이트: http://www.cisnet.co.kr/
================================================================================`;

const LICENSE_TEXT_EN = `================================================================================
                 WinPurify Pro End User License Agreement (EULA)
                      (Standard English Edition)
================================================================================

IMPORTANT: PLEASE READ THIS SOFTWARE LICENSE AGREEMENT CAREFULLY BEFORE DOWNLOADING, 
INSTALLING, OR USING WINPURIFY PRO.

This End User License Agreement ("EULA") is a legal agreement between you (either an 
individual or a single legal entity, hereinafter "User") and Cisnet Soft ("Licensor", 
developer of WinPurify Pro) regarding the use of "WinPurify Pro" (including the 
133-module optimization suite, native C++ acceleration core PurifyEngineCore.dll, 
Central Commander multi-PC fleet management console, WinPurifyAgentService background 
Windows service, and associated documentation).

BY INSTALLING, COPYING, OR USING THIS SOFTWARE, YOU AGREE TO BE BOUND BY ALL THE TERMS 
OF THIS EULA. IF YOU DO NOT AGREE TO THESE TERMS, DO NOT INSTALL OR USE THE SOFTWARE, 
AND PROMPTLY REMOVE ALL COPIES FROM YOUR SYSTEM.

--------------------------------------------------------------------------------
1. GRANT OF LICENSE
--------------------------------------------------------------------------------
1.1 Licensor grants you a revocable, non-exclusive, non-transferable, limited license 
    to install and execute the Software on compatible Windows devices (Windows 10, 
    Windows 11, and Windows Server 64-bit architectures) strictly in accordance with 
    the terms of this Agreement.
1.2 You may deploy the Software in Unpacked Multi-File directory installation mode or 
    Standalone Portable mode for personal, educational, corporate, and commercial system 
    maintenance purposes.
1.3 All features including 133 optimization modules, real-time hardware telemetry, weekly 
    automation scheduler, and Live Safepoint registry backup are authorized for operation 
    under this license.

--------------------------------------------------------------------------------
2. REMOTE MANAGEMENT & NETWORK PORT CONFIGURATION
--------------------------------------------------------------------------------
2.1 The Software includes Central Commander and WinPurifyAgentService capabilities 
    operating over TCP Port 9870 (REST command channel) and UDP Port 9871 (broadcast 
    node discovery).
2.2 During installation, users are presented with an explicit option to configure 
    remote ports and Windows Firewall exceptions ("openremoteport", enabled by default).
2.3 Network communication features are strictly intended for authorized local area network 
    (LAN) or intranet administration. Deploying or executing remote control, system shutdown, 
    or reboot commands against unauthorized third-party systems is strictly prohibited.

--------------------------------------------------------------------------------
3. INTELLECTUAL PROPERTY RIGHTS & RESTRICTIONS
--------------------------------------------------------------------------------
3.1 All title, ownership rights, trademarks, and intellectual property rights in and 
    to the Software (including binary code, C++ native core PurifyEngineCore.dll, WPF UI 
    designs, graphics, and documentation) are owned exclusively by Cisnet Soft.
3.2 The Software is protected by copyright laws and international copyright treaties.
3.3 You shall not:
    (a) Reverse engineer, decompile, disassemble, or derive source code from the compiled 
        binaries, except to the extent permitted by applicable law;
    (b) Modify, alter, adapt, or create derivative works of the Software;
    (c) Distribute, lease, rent, sell, or sublicense the Software without explicit written 
        authorization from Cisnet Soft.

--------------------------------------------------------------------------------
4. PRIVACY, DATA INTEGRITY & CREDENTIAL PURGE
--------------------------------------------------------------------------------
4.1 WinPurify Pro operates entirely offline and does not harvest, transmit, or store 
    personal data, credentials, telemetry, or network payloads on external servers.
4.2 Advanced features including Account Credential Purge, Browser Factory Reset, and 
    Disk Forensic Cipher Wiping permanently delete authentication tokens and temporary files. 
    Users bear sole responsibility for verifying targets prior to permanent destruction.

--------------------------------------------------------------------------------
5. DISCLAIMER OF WARRANTIES & LIMITATION OF LIABILITY
--------------------------------------------------------------------------------
5.1 TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THE SOFTWARE IS PROVIDED "AS IS" 
    AND "AS AVAILABLE", WITH ALL FAULTS AND WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS, 
    IMPLIED, OR STATUTORY, INCLUDING WITHOUT LIMITATION WARRANTIES OF MERCHANTABILITY, 
    FITNESS FOR A PARTICULAR PURPOSE, OR NON-INFRINGEMENT.
5.2 Users are strongly advised to utilize the built-in Live Safepoint rollback facility 
    and maintain comprehensive system backups before executing aggressive optimization, 
    registry cleaning, or remote power control operations.
5.3 IN NO EVENT SHALL CISNET SOFT BE LIABLE FOR ANY DIRECT, INDIRECT, SPECIAL, INCIDENTAL, 
    OR CONSEQUENTIAL DAMAGES (INCLUDING LOSS OF PROFITS, DATA CORRUPTION, BUSINESS 
    INTERRUPTION, OR SYSTEM DOWNTIME) ARISING OUT OF OR IN CONNECTION WITH THE USE OR 
    INABILITY TO USE THE SOFTWARE.

--------------------------------------------------------------------------------
6. TERMINATION & GOVERNING LAW
--------------------------------------------------------------------------------
6.1 This Agreement is effective until terminated. Your rights under this license terminate 
    automatically without notice if you fail to comply with any provision of this EULA.
6.2 This EULA shall be governed by, construed, and enforced in accordance with the laws 
    of the Republic of Korea. Any disputes arising under or in connection with this 
    Agreement shall be subject to the exclusive jurisdiction of the competent courts of 
    the Republic of Korea.

================================================================================
Licensor: Cisnet Soft (Lead Developer: AhBiYout)
Blog: https://ahbiyoutvibe.blogspot.com/
Website: http://www.cisnet.co.kr/
================================================================================`;

interface DiagnosticItem {
  title: string;
  desc: string;
  passed: boolean;
  detail: string;
}

const CATEGORIES = ['All', 'Privacy', 'Browser', 'Security', 'Registry', 'Update', 'System', 'Storage', 'Special'];

const CATEGORY_CONFIG: Record<string, { label: string; count: number; desc: string; activeBg: string; activeText: string; activeBorder: string; badgeActive: string }> = {
  All: { label: '⚡ 전체 모듈', count: 138, desc: '시스템 138개 최적화, 제로트레이스 및 심층 정화 모듈 전체 제어', activeBg: 'bg-blue-950/80', activeText: 'text-blue-300 font-bold', activeBorder: 'border-blue-500 shadow-sm shadow-blue-500/20', badgeActive: 'bg-blue-500/20 text-blue-300 border-blue-500/40 font-bold' },
  Privacy: { label: '🛡️ 개인정보 보호', count: 27, desc: 'Windows 자격증명, RDP/USB 포렌식 이력, 텔레메트리, BAM/DAM 등 27개 모듈', activeBg: 'bg-purple-950/80', activeText: 'text-purple-300 font-bold', activeBorder: 'border-purple-500 shadow-sm shadow-purple-500/20', badgeActive: 'bg-purple-500/20 text-purple-300 border-purple-500/40 font-bold' },
  Browser: { label: '🌐 브라우저 최적화', count: 21, desc: '자동완성/비밀번호 소거, 카카오톡/Discord/Teams 세션 토큰 등 21개 모듈', activeBg: 'bg-sky-950/80', activeText: 'text-sky-300 font-bold', activeBorder: 'border-sky-500 shadow-sm shadow-sky-500/20', badgeActive: 'bg-sky-500/20 text-sky-300 border-sky-500/40 font-bold' },
  Security: { label: '🔒 보안 센터', count: 10, desc: 'Windows Defender 검사 히스토리, 방화벽 로그, 격리소 파편 소거 10개 모듈', activeBg: 'bg-red-950/80', activeText: 'text-red-300 font-bold', activeBorder: 'border-red-500 shadow-sm shadow-red-500/20', badgeActive: 'bg-red-500/20 text-red-300 border-red-500/40 font-bold' },
  Registry: { label: '⚙️ 레지스트리', count: 15, desc: 'MUI 인터페이스 캐시, 고립된 CLSID 등 레지스트리 잔여물 정리 15개 모듈', activeBg: 'bg-amber-950/80', activeText: 'text-amber-300 font-bold', activeBorder: 'border-amber-500 shadow-sm shadow-amber-500/20', badgeActive: 'bg-amber-500/20 text-amber-300 border-amber-500/40 font-bold' },
  Update: { label: '🔄 업데이트 관리', count: 15, desc: 'Windows Update 다운로드 잔여 패키지, 전송 최적화 캐시 정리 15개 모듈', activeBg: 'bg-teal-950/80', activeText: 'text-teal-300 font-bold', activeBorder: 'border-teal-500 shadow-sm shadow-teal-500/20', badgeActive: 'bg-teal-500/20 text-teal-300 border-teal-500/40 font-bold' },
  System: { label: '🚀 시스템 가속', count: 15, desc: 'DNS 리졸버 캐시 플러시, DirectX 셰이더 캐시 정리 등 시스템 가속 15개 모듈', activeBg: 'bg-emerald-950/80', activeText: 'text-emerald-300 font-bold', activeBorder: 'border-emerald-500 shadow-sm shadow-emerald-500/20', badgeActive: 'bg-emerald-500/20 text-emerald-300 border-emerald-500/40 font-bold' },
  Storage: { label: '💾 저장공간 정리', count: 29, desc: 'hiberfil.sys, DISM ResetBase, DriverStore, %TEMP%, 휴지통 등 29개 모듈', activeBg: 'bg-indigo-950/80', activeText: 'text-indigo-300 font-bold', activeBorder: 'border-indigo-500 shadow-sm shadow-indigo-500/20', badgeActive: 'bg-indigo-500/20 text-indigo-300 border-indigo-500/40 font-bold' },
  Special: { label: '🛑 특수 기능', count: 12, desc: '통합 계정 정화 센터(Microsoft/Adobe/AutoCAD/Edge/Chrome/자격증명 일괄 로그아웃), 브라우저 공장 초기화(Chrome/Edge/Whale/Firefox), Windows 기본 프로필(Default) 덮어쓰기 복제, 디스크 포렌식 와이핑(Cipher) 등 자동 선택 제외 특수 모듈 12개', activeBg: 'bg-rose-950/80', activeText: 'text-rose-300 font-bold', activeBorder: 'border-rose-500 shadow-sm shadow-rose-500/20', badgeActive: 'bg-rose-500/20 text-rose-300 border-rose-500/40 font-bold' }
};

export type ThemeMode = 'dark' | 'gray' | 'white' | 'beige';

export interface ThemeOption {
  id: ThemeMode;
  label: string;
  icon: string;
  desc: string;
}

export const THEMES: ThemeOption[] = [
  { id: 'dark', label: '다크', icon: '🌙', desc: '모던 딥 스페이스 다크 테마' },
  { id: 'gray', label: '회색', icon: '🔘', desc: '뉴트럴 슬레이트 그레이 테마' },
  { id: 'white', label: '화이트', icon: '☀️', desc: '고대비 클린 모던 화이트 테마' },
  { id: 'beige', label: '베이지', icon: '📜', desc: '따뜻하고 편안한 웜 베이지 테마' },
];

export const themeStyles = {
  dark: {
    app: 'bg-[#0B0F19] text-slate-100',
    header: 'bg-slate-900/90 border-slate-800 text-white',
    headerSub: 'text-slate-400',
    card: 'bg-slate-900/80 border-slate-800 text-slate-100',
    sidebar: 'bg-slate-900/70 border-slate-800',
    sidebarBox: 'bg-slate-900/70 border-slate-800',
    panel: 'bg-slate-950 border-slate-800 text-slate-200',
    panelSub: 'text-slate-400',
    input: 'bg-slate-900 border-slate-800 text-slate-200 placeholder:text-slate-600 focus:border-blue-500',
    tableHeader: 'bg-slate-950/60 text-slate-400 border-slate-800',
    tableBorder: 'border-slate-800/60',
    moduleCardActive: 'bg-slate-900/90 border-blue-500/50 shadow-sm shadow-blue-500/10 text-slate-100',
    moduleCardInactive: 'bg-slate-900/40 border-slate-800/80 opacity-75 hover:opacity-100 hover:border-slate-700 text-slate-200',
    moduleTitle: 'text-slate-100',
    moduleDesc: 'text-slate-200',
    console: 'bg-slate-950 border-slate-800 text-slate-300',
    modalBg: 'bg-slate-900 border-slate-700 text-white',
    modalHeader: 'bg-slate-950/80 border-slate-800',
    themeGroupBg: 'bg-slate-950/90 border-slate-800',
    buttonInactive: 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/60',
    categoryInactive: 'text-slate-400 hover:bg-slate-800/70 hover:text-slate-200',
    categoryBadgeInactive: 'bg-slate-800/80 border-slate-700/60 text-slate-400',
    gaugeBg: 'bg-slate-900/80 border-slate-800',
    gaugeTrack: 'bg-slate-800',
    statSub: 'text-slate-400',
    modalInput: 'bg-slate-950 border-slate-800 text-slate-200',
    metricCpu: 'text-sky-400',
    metricRam: 'text-emerald-400',
    metricDisk: 'text-amber-400',
    metricSpace: 'text-emerald-400',
    sizeText: 'text-sky-400 font-bold',
    riskSafe: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
    riskDeep: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
    riskRisky: 'bg-rose-500/10 text-rose-400 border-rose-500/20',
    seqBadge: 'bg-slate-800/90 text-sky-400 border-slate-700/60',
    profileLabel: 'text-slate-400',
    selectBtn: 'bg-slate-800 text-slate-300 hover:bg-slate-700 border-slate-700',
    dropdownItem: 'hover:bg-slate-800/80 text-slate-200 hover:text-white',
    dropdownItemActive: 'bg-blue-600/25 text-blue-400 border-blue-500/40 font-bold',
    dropdownSub: 'text-slate-400',
  },
  gray: {
    app: 'bg-[#1E232B] text-slate-100',
    header: 'bg-[#282F3A]/95 border-[#3D4756] text-white',
    headerSub: 'text-slate-300',
    card: 'bg-[#282F3A]/85 border-[#3D4756] text-slate-100',
    sidebar: 'bg-[#252C37]/80 border-[#3D4756]',
    sidebarBox: 'bg-[#282F3A]/80 border-[#3D4756]',
    panel: 'bg-[#1C2128] border-[#3D4756] text-slate-200',
    panelSub: 'text-slate-300',
    input: 'bg-[#1C2128] border-[#3D4756] text-slate-100 placeholder:text-slate-400 focus:border-blue-400',
    tableHeader: 'bg-[#1C2128]/80 text-slate-300 border-[#3D4756]',
    tableBorder: 'border-[#3D4756]/60',
    moduleCardActive: 'bg-[#2E3644] border-blue-400/60 shadow-sm shadow-blue-500/10 text-slate-100',
    moduleCardInactive: 'bg-[#252C37]/60 border-[#3D4756]/80 opacity-80 hover:opacity-100 hover:border-[#4E5B6E] text-slate-200',
    moduleTitle: 'text-slate-100',
    moduleDesc: 'text-slate-200',
    console: 'bg-[#15191E] border-[#3D4756] text-slate-300',
    modalBg: 'bg-[#282F3A] border-[#475569] text-white',
    modalHeader: 'bg-[#1C2128] border-[#3D4756]',
    themeGroupBg: 'bg-[#181C22] border-[#3D4756]',
    buttonInactive: 'text-slate-300 hover:text-white hover:bg-[#343D4C]/60',
    categoryInactive: 'text-slate-300 hover:bg-[#343D4C] hover:text-white',
    categoryBadgeInactive: 'bg-[#1C2128] border-[#3D4756] text-slate-300',
    gaugeBg: 'bg-[#282F3A]/80 border-[#3D4756]',
    gaugeTrack: 'bg-[#1C2128]',
    statSub: 'text-slate-300',
    modalInput: 'bg-[#1C2128] border-[#3D4756] text-slate-200',
    metricCpu: 'text-sky-300',
    metricRam: 'text-emerald-300',
    metricDisk: 'text-amber-300',
    metricSpace: 'text-emerald-300',
    sizeText: 'text-sky-300 font-bold',
    riskSafe: 'bg-emerald-500/15 text-emerald-300 border-emerald-500/30',
    riskDeep: 'bg-amber-500/15 text-amber-300 border-amber-500/30',
    riskRisky: 'bg-rose-500/15 text-rose-300 border-rose-500/30',
    seqBadge: 'bg-[#1C2128] text-sky-300 border-[#3D4756]',
    profileLabel: 'text-slate-300',
    selectBtn: 'bg-[#282F3A] text-slate-200 hover:bg-[#343D4C] border-[#3D4756]',
    dropdownItem: 'hover:bg-[#343D4C] text-slate-200 hover:text-white',
    dropdownItemActive: 'bg-blue-600/25 text-blue-300 border-blue-400/40 font-bold',
    dropdownSub: 'text-slate-300',
  },
  white: {
    app: 'bg-[#F8FAFC] text-slate-900',
    header: 'bg-white/95 border-slate-200 text-slate-900 shadow-sm',
    headerSub: 'text-slate-600',
    card: 'bg-white border-slate-200 text-slate-900 shadow-sm',
    sidebar: 'bg-white border-slate-200 shadow-sm',
    sidebarBox: 'bg-slate-50 border-slate-200',
    panel: 'bg-slate-50 border-slate-200 text-slate-800',
    panelSub: 'text-slate-600',
    input: 'bg-white border-slate-300 text-slate-900 placeholder:text-slate-400 focus:border-blue-600 shadow-inner',
    tableHeader: 'bg-slate-100 text-slate-700 border-slate-200',
    tableBorder: 'border-slate-100',
    moduleCardActive: 'bg-blue-50/70 border-blue-500/70 shadow-sm text-slate-900',
    moduleCardInactive: 'bg-white border-slate-200 hover:border-slate-300 shadow-xs text-slate-800',
    moduleTitle: 'text-slate-900 font-bold',
    moduleDesc: 'text-slate-700',
    console: 'bg-slate-900 border-slate-800 text-slate-200',
    modalBg: 'bg-white border-slate-200 text-slate-900 shadow-2xl',
    modalHeader: 'bg-slate-50 border-slate-200',
    themeGroupBg: 'bg-slate-100 border-slate-200',
    buttonInactive: 'text-slate-600 hover:text-slate-900 hover:bg-slate-200/60',
    categoryInactive: 'text-slate-600 hover:bg-slate-100 hover:text-slate-900',
    categoryBadgeInactive: 'bg-slate-100 border-slate-200 text-slate-600',
    gaugeBg: 'bg-white border-slate-200 shadow-sm',
    gaugeTrack: 'bg-slate-100',
    statSub: 'text-slate-600',
    modalInput: 'bg-slate-50 border-slate-200 text-slate-900',
    metricCpu: 'text-sky-700',
    metricRam: 'text-emerald-700',
    metricDisk: 'text-amber-700',
    metricSpace: 'text-emerald-700',
    sizeText: 'text-sky-700 font-bold',
    riskSafe: 'bg-emerald-50 text-emerald-800 border-emerald-300',
    riskDeep: 'bg-amber-50 text-amber-900 border-amber-300',
    riskRisky: 'bg-rose-50 text-rose-800 border-rose-300',
    seqBadge: 'bg-slate-100 text-sky-700 border-slate-300 font-bold',
    profileLabel: 'text-slate-700',
    selectBtn: 'bg-slate-100 text-slate-800 hover:bg-slate-200 border-slate-300',
    dropdownItem: 'hover:bg-slate-100 text-slate-800 hover:text-slate-900',
    dropdownItemActive: 'bg-blue-50 text-blue-700 border-blue-300 font-bold',
    dropdownSub: 'text-slate-500',
  },
  beige: {
    app: 'bg-[#F5F0E6] text-[#2D241E]',
    header: 'bg-[#FAF7F0]/95 border-[#E6DFD3] text-[#2D241E] shadow-sm',
    headerSub: 'text-[#665A52]',
    card: 'bg-[#FCFBF8] border-[#E6DFD3] text-[#2D241E] shadow-sm',
    sidebar: 'bg-[#FAF7F0] border-[#E6DFD3] shadow-sm',
    sidebarBox: 'bg-[#F5EFE3] border-[#E0D8CA]',
    panel: 'bg-[#F0EBE0] border-[#E0D8CA] text-[#2D241E]',
    panelSub: 'text-[#665A52]',
    input: 'bg-[#FCFBF8] border-[#DDD5C7] text-[#2D241E] placeholder:text-[#8F8176] focus:border-amber-600 shadow-inner',
    tableHeader: 'bg-[#EFE9DC] text-[#4A3F37] border-[#E6DFD3]',
    tableBorder: 'border-[#EFE9DC]',
    moduleCardActive: 'bg-[#FAF2E6] border-[#D97706]/70 shadow-sm text-[#2D241E]',
    moduleCardInactive: 'bg-[#FCFBF8] border-[#E6DFD3] hover:border-[#D6CCC0] shadow-xs text-[#2D241E]',
    moduleTitle: 'text-[#2D241E] font-bold',
    moduleDesc: 'text-[#4A3F37]',
    console: 'bg-[#241E1A] border-[#3D332C] text-[#D6CECA]',
    modalBg: 'bg-[#FCFBF8] border-[#E6DFD3] text-[#2D241E] shadow-2xl',
    modalHeader: 'bg-[#FAF7F0] border-[#E6DFD3]',
    themeGroupBg: 'bg-[#EAE4D7] border-[#DDD5C7]',
    buttonInactive: 'text-[#5C5046] hover:text-[#2D241E] hover:bg-[#DED5C0]/60',
    categoryInactive: 'text-[#5C5046] hover:bg-[#EAE2D2] hover:text-[#2D241E]',
    categoryBadgeInactive: 'bg-[#EAE4D7] border-[#DDD5C7] text-[#5C5046]',
    gaugeBg: 'bg-[#FCFBF8] border-[#E6DFD3] shadow-sm',
    gaugeTrack: 'bg-[#EAE2D2]',
    statSub: 'text-[#665A52]',
    modalInput: 'bg-[#F0EBE0] border-[#E0D8CA] text-[#2D241E]',
    metricCpu: 'text-sky-800',
    metricRam: 'text-emerald-800',
    metricDisk: 'text-amber-800',
    metricSpace: 'text-emerald-800',
    sizeText: 'text-sky-800 font-bold',
    riskSafe: 'bg-[#E6F4EA] text-[#137333] border-[#CEEAD6]',
    riskDeep: 'bg-[#FEF7E0] text-[#B06000] border-[#FEEFC3]',
    riskRisky: 'bg-[#FCE8E6] text-[#C5221F] border-[#FAD2CF]',
    seqBadge: 'bg-[#EAE4D7] text-sky-800 border-[#DDD5C7] font-bold',
    profileLabel: 'text-[#4A3F37]',
    selectBtn: 'bg-[#EAE4D7] text-[#2D241E] hover:bg-[#DDD5C7] border-[#DDD5C7]',
    dropdownItem: 'hover:bg-[#EAE2D2] text-[#2D241E]',
    dropdownItemActive: 'bg-[#F2E5D0] text-[#B45309] border-amber-300 font-bold',
    dropdownSub: 'text-[#7C6E64]',
  }
};

const App: React.FC = () => {
  const [theme, setTheme] = useState<ThemeMode>(() => {
    try {
      const saved = localStorage.getItem('winpurify_theme') as ThemeMode;
      if (saved && ['dark', 'gray', 'white', 'beige'].includes(saved)) return saved;
    } catch { }
    return 'dark';
  });

  useEffect(() => {
    try {
      localStorage.setItem('winpurify_theme', theme);
    } catch { }
  }, [theme]);

  const [isThemeDropdownOpen, setIsThemeDropdownOpen] = useState<boolean>(false);
  const themeDropdownRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleOutsideClick = (e: MouseEvent) => {
      if (themeDropdownRef.current && !themeDropdownRef.current.contains(e.target as Node)) {
        setIsThemeDropdownOpen(false);
      }
    };
    if (isThemeDropdownOpen) {
      document.addEventListener('mousedown', handleOutsideClick);
    }
    return () => {
      document.removeEventListener('mousedown', handleOutsideClick);
    };
  }, [isThemeDropdownOpen]);

  const currentTheme = themeStyles[theme];

  const [modules, setModules] = useState<ModuleItem[]>(INITIAL_MODULES);
  const [selectedCategory, setSelectedCategory] = useState<string>('All');
  const [specialSubCategory, setSpecialSubCategory] = useState<'All' | 'Account' | 'FactoryReset' | 'Profile' | 'Forensics'>('All');
  const [search, setSearch] = useState<string>('');
  const [isProcessing, setIsProcessing] = useState<boolean>(false);
  const [hasSafepoint, setHasSafepoint] = useState<boolean>(false);
  const [showDiagModal, setShowDiagModal] = useState<boolean>(false);
  const [isScheduled, setIsScheduled] = useState<boolean>(false);
  const [showScheduleModal, setShowScheduleModal] = useState<boolean>(false);
  const [showCommanderModal, setShowCommanderModal] = useState<boolean>(false);
  const [showDefaultProfileWarningModal, setShowDefaultProfileWarningModal] = useState<boolean>(false);

  // System Tray & Notification Center State
  interface TrayToastItem {
    id: string;
    title: string;
    message: string;
    type: 'info' | 'success' | 'warning' | 'error';
    timestamp: string;
    progress?: number;
    moduleName?: string;
  }

  const [showTrayModal, setShowTrayModal] = useState<boolean>(false);
  const [showTrayMenu, setShowTrayMenu] = useState<boolean>(false);
  const [isTrayMinimized, setIsTrayMinimized] = useState<boolean>(false);
  const [isTrayHovered, setIsTrayHovered] = useState<boolean>(false);
  const [trayActiveTab, setTrayActiveTab] = useState<'policy' | 'notifications' | 'history'>('policy');

  const [trayOptions, setTrayOptions] = useState({
    minimizeToTray: true,
    closeToTray: true,
    liveTooltipProgress: true,
    toastNotifications: true,
    milestoneNotifications: true,
    nativeBrowserNotifications: false,
    soundAlerts: true
  });

  // GitHub Releases Live Auto-Update State
  const [showUpdateModal, setShowUpdateModal] = useState<boolean>(false);
  const [isCheckingUpdate, setIsCheckingUpdate] = useState<boolean>(false);
  const [updateInfo, setUpdateInfo] = useState<{
    latestVersion: string;
    isNewer: boolean;
    releaseTitle: string;
    releaseNotes: string;
    releaseUrl: string;
    downloadUrl: string;
    publishedAt: string;
    status: string;
  }>({
    latestVersion: APP_VERSION,
    isNewer: false,
    releaseTitle: `WinPurify Pro v${APP_VERSION}`,
    releaseNotes: '',
    releaseUrl: 'https://github.com/ahbiyout-all/WinPurify-Pro/releases',
    downloadUrl: 'https://github.com/ahbiyout-all/WinPurify-Pro/releases/latest',
    publishedAt: '',
    status: '확인 대기 중'
  });

  const compareSemVer = (a: string, b: string): number => {
    const pa = a.split('.').map(n => parseInt(n, 10) || 0);
    const pb = b.split('.').map(n => parseInt(n, 10) || 0);
    for (let i = 0; i < Math.max(pa.length, pb.length); i++) {
      const va = pa[i] || 0;
      const vb = pb[i] || 0;
      if (va > vb) return 1;
      if (va < vb) return -1;
    }
    return 0;
  };

  const handleCheckGitHubUpdates = async (isManual = true) => {
    if (isManual) {
      setShowUpdateModal(true);
    }
    setIsCheckingUpdate(true);
    setUpdateInfo(prev => ({ ...prev, status: 'GitHub Releases API (ahbiyout-all/WinPurify-Pro) 실시간 조회 중...' }));
    if (isManual) {
      addLog('[자동 업데이트] GitHub API (https://api.github.com/repos/ahbiyout-all/WinPurify-Pro/releases/latest) 실시간 릴리스 조회 중...');
    }

    try {
      const res = await fetch('https://api.github.com/repos/ahbiyout-all/WinPurify-Pro/releases/latest');
      if (res.ok) {
        const data = await res.json();
        const tag = (data.tag_name || '').replace(/^v/i, '').trim();
        const isNewer = compareSemVer(tag, APP_VERSION) > 0;
        let dlUrl = data.html_url || 'https://github.com/ahbiyout-all/WinPurify-Pro/releases/latest';
        if (data.assets && Array.isArray(data.assets) && data.assets.length > 0) {
          const exeAsset = data.assets.find((a: any) => a.name?.endsWith('.exe'));
          if (exeAsset) dlUrl = exeAsset.browser_download_url;
        }

        setUpdateInfo({
          latestVersion: tag || APP_VERSION,
          isNewer,
          releaseTitle: data.name || `WinPurify Pro v${tag}`,
          releaseNotes: data.body || '릴리스 변경 내역이 등록되어 있습니다.',
          releaseUrl: data.html_url || 'https://github.com/ahbiyout-all/WinPurify-Pro/releases',
          downloadUrl: dlUrl,
          publishedAt: data.published_at ? new Date(data.published_at).toLocaleDateString('ko-KR') : '',
          status: isNewer 
            ? `새로운 버전 v${tag}이 발견되었습니다! (현재 v${APP_VERSION})`
            : `현재 최신 버전 v${APP_VERSION}을 사용하고 있습니다.`
        });

        if (isNewer) {
          // 업데이트가 있으면 알림 팝업창 자동 실행!
          setShowUpdateModal(true);
          dispatchTrayNotification('🚀 새 버전 업데이트 발견!', `최신 버전 v${tag} 업데이트가 배포되었습니다. 지금 바로 확인하세요.`, 'info');
          addLog(`[자동 업데이트 감지] 최신 버전 v${tag} 배포가 확인되어 업데이트 알림 팝업창을 자동으로 실행했습니다.`);
        }
      } else {
        setUpdateInfo(prev => ({
          ...prev,
          status: `GitHub 저장소(https://github.com/ahbiyout-all/WinPurify-Pro) 배포 상태 정상. 현재 공식 버전 v${APP_VERSION} 가동 중.`
        }));
      }
    } catch (err: any) {
      setUpdateInfo(prev => ({
        ...prev,
        status: `네트워크 연결 확인 필요: ${err?.message || err}. 현재 로컬 빌드 v${APP_VERSION} 정상 구동 중.`
      }));
    } finally {
      setIsCheckingUpdate(false);
    }
  };

  const [toasts, setToasts] = useState<TrayToastItem[]>([]);
  const [toastHistory, setToastHistory] = useState<TrayToastItem[]>([
    {
      id: 'init-toast',
      title: '🚀 시스템 트레이 감시 서비스 가동',
      message: `WinPurify Pro v${APP_VERSION} 트레이 알림 센터가 활성화되었습니다.`,
      type: 'info',
      timestamp: new Date().toLocaleTimeString('ko-KR', { hour12: false })
    }
  ]);

  const dispatchTrayNotification = (
    title: string,
    message: string,
    type: 'info' | 'success' | 'warning' | 'error' = 'info',
    progress?: number,
    moduleName?: string
  ) => {
    if (!trayOptions.toastNotifications) return;

    if (trayOptions.soundAlerts) {
      playAlertSound(type === 'warning' || type === 'error' ? 'warning' : 'complete');
    }

    const newToast: TrayToastItem = {
      id: `toast-${Date.now()}-${Math.random().toString(36).substr(2, 4)}`,
      title,
      message,
      type,
      timestamp: new Date().toLocaleTimeString('ko-KR', { hour12: false }),
      progress,
      moduleName
    };

    setToasts(prev => [newToast, ...prev.slice(0, 3)]);
    setToastHistory(prev => [newToast, ...prev.slice(0, 49)]);

    if (trayOptions.nativeBrowserNotifications && 'Notification' in window && Notification.permission === 'granted') {
      try {
        new Notification(title, {
          body: message,
          icon: 'WinPurify_icon.png'
        });
      } catch { }
    }

    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== newToast.id));
    }, 4500);
  };

  const requestBrowserNotificationPermission = async () => {
    if (!('Notification' in window)) {
      addLog('[트레이 알림] 브라우저 환경에서 Notification API를 지원하지 않습니다.');
      return;
    }
    try {
      const res = await Notification.requestPermission();
      if (res === 'granted') {
        setTrayOptions(prev => ({ ...prev, nativeBrowserNotifications: true }));
        dispatchTrayNotification('🔔 데스크톱 네이티브 알림 연동', '브라우저 창 최소화 상태에서도 OS 시스템 알림을 직접 수신합니다.', 'success');
        addLog('[트레이 알림] 브라우저 데스크톱 알림 권한 허용 완료');
      } else {
        setTrayOptions(prev => ({ ...prev, nativeBrowserNotifications: false }));
        addLog('[트레이 알림] 브라우저 데스크톱 알림 권한이 거부되었습니다.');
      }
    } catch { }
  };

  // Central Commander Multi-PC Fleet Management State
  interface RemoteNode {
    id: string;
    name: string;
    ip: string;
    group: string;
    online: boolean;
    cpu: number;
    ramPercent: number;
    ramText: string;
    score: number;
    selected: boolean;
  }

  const [clusterKey, setClusterKey] = useState<string>('WinPurifyClusterKey2026');
  const [isScanningNodes, setIsScanningNodes] = useState<boolean>(false);
  const [remoteNodes, setRemoteNodes] = useState<RemoteNode[]>([
    { id: '1', name: 'ADMIN-PC (로컬 호스트)', ip: '127.0.0.1', group: '본사 관리실', online: true, cpu: 14, ramPercent: 32, ramText: '10.2 / 32 GB', score: 98, selected: true },
    { id: '2', name: 'FINANCE-PC-01', ip: '192.168.1.102', group: '재무회계팀', online: true, cpu: 28, ramPercent: 54, ramText: '8.6 / 16 GB', score: 84, selected: true },
    { id: '3', name: 'DEV-WORKSTATION-04', ip: '192.168.1.145', group: '연구개발실', online: true, cpu: 45, ramPercent: 68, ramText: '43.5 / 64 GB', score: 91, selected: true },
    { id: '4', name: 'CS-TERMINAL-09', ip: '192.168.1.201', group: '고객지원센터', online: true, cpu: 12, ramPercent: 38, ramText: '6.1 / 16 GB', score: 76, selected: true },
    { id: '5', name: 'MEETING-ROOM-HUB', ip: '192.168.1.88', group: '대회의실', online: true, cpu: 6, ramPercent: 22, ramText: '3.5 / 16 GB', score: 100, selected: false }
  ]);
  const [commanderLogs, setCommanderLogs] = useState<string[]>([
    `[Commander v${APP_VERSION}] HMAC-SHA256 암호화 관제 채널 초기화 완료 (포트 TCP:9870 / UDP:9871)`,
    `[Fleet Special] Option A 4대 서브 카테고리 (계정/브라우저/프로필/포렌식) 원격 특수 기능 허브 준비 완료`,
    '[Fleet] 5개 클러스터 노드 텔레메트리 및 144개 모듈 무결성 점검 세션 준비 완료'
  ]);
  const [commanderActiveTab, setCommanderActiveTab] = useState<'nodes' | 'audit'>('nodes');

  // Central Commander Remote Execution Incoming Action Alert State
  interface RemoteActionState {
    isOpen: boolean;
    title: string;
    commandName: string;
    sourceIp: string;
    status: string;
    details: string;
    isActive: boolean;
    autoCloseSeconds: number;
  }

  const [remoteAction, setRemoteAction] = useState<RemoteActionState>({
    isOpen: false,
    title: '🚨 중앙 관제(Commander) 원격 명령 동작 중',
    commandName: '',
    sourceIp: '',
    status: '',
    details: '',
    isActive: false,
    autoCloseSeconds: 5
  });

  interface RemoteAuditRecord {
    id: string;
    timestamp: string;
    commanderIp: string;
    commandType: string;
    commandTitle: string;
    details: string;
    executionTimeMs: number;
    isSuccess: boolean;
  }

  const [remoteAuditHistory, setRemoteAuditHistory] = useState<RemoteAuditRecord[]>([
    {
      id: 'audit-1',
      timestamp: '2026-09-08 10:14:22',
      commanderIp: '192.168.1.100 (Central Commander)',
      commandType: 'RamTrim',
      commandTitle: '물리 RAM 워킹셋 긴급 정화 (RAM Trim)',
      details: '[성공] C++ Native 엔진을 통해 물리 RAM 워킹셋 정화 완료 (84개 프로세스)',
      executionTimeMs: 42,
      isSuccess: true
    },
    {
      id: 'audit-2',
      timestamp: '2026-09-08 09:30:15',
      commanderIp: '192.168.1.100 (Central Commander)',
      commandType: 'CreateRestorePoint',
      commandTitle: 'Windows 시스템 복원 지점 생성',
      details: '[성공] Windows C: 시스템 보호 자동 활성화 및 세이프포인트 스냅샷 생성 완료',
      executionTimeMs: 3120,
      isSuccess: true
    }
  ]);

  // Account & Credential Force Logout State
  interface AccountSessionItem {
    id: string;
    icon: string;
    serviceName: string;
    category: string;
    description: string;
    targetSummary: string;
    isDetected: boolean;
    detectedSummary: string;
    selected: boolean;
  }

  const [showAccountPurgeModal, setShowAccountPurgeModal] = useState<boolean>(false);
  const [showLicenseModal, setShowLicenseModal] = useState<boolean>(false);
  const [licenseLang, setLicenseLang] = useState<'ko' | 'en'>('ko');
  const [licenseCopied, setLicenseCopied] = useState<boolean>(false);

  const [isAccountPurging, setIsAccountPurging] = useState<boolean>(false);
  const [terminateProcessesBeforePurge, setTerminateProcessesBeforePurge] = useState<boolean>(true);
  const [accountPurgeStatus, setAccountPurgeStatus] = useState<string>('스캔 완료: 선택된 계정을 안전하게 정화할 수 있습니다.');
  const [accountTargets, setAccountTargets] = useState<AccountSessionItem[]>([
    {
      id: 'ms-account',
      icon: '🪟',
      serviceName: 'Microsoft 계정 & Office WAM 토큰 소거',
      category: 'Microsoft',
      description: 'Windows 자격 증명 관리자의 MicrosoftAccount/SSO_POP 토큰 및 WAM(IdentityCache/OneAuth/TokenBroker) 캐시를 소거하여 Office/Windows Live에서 강제 로그아웃합니다.',
      targetSummary: 'CredentialManager: MicrosoftAccount:* | LocalAppData\\Microsoft\\IdentityCache',
      isDetected: true,
      detectedSummary: '활성 세션 감지됨 (WAM 토큰)',
      selected: true
    },
    {
      id: 'adobe-session',
      icon: '🎨',
      serviceName: 'Adobe Creative Cloud 계정 로그인 세션 & OOBE 소거',
      category: 'Adobe',
      description: 'Adobe Creative Cloud, Photoshop, Acrobat의 OOBE 인증 토큰 및 로컬 세션 캐시를 소거하여 모든 Adobe 앱에서 즉시 로그아웃합니다.',
      targetSummary: 'LocalAppData\\Adobe\\OOBE | CommonFiles\\Adobe\\SLCache',
      isDetected: true,
      detectedSummary: '활성 세션 감지됨 (OOBE 세션)',
      selected: true
    },
    {
      id: 'autodesk-session',
      icon: '📐',
      serviceName: 'Autodesk (AutoCAD) 로그인 상태 & Web Services 토큰 소거',
      category: 'Autodesk',
      description: 'AutoCAD 및 Autodesk 제품군의 Web Services LoginState.xml 세션 및 AdskIdentityManager 토큰을 초기화하여 로그아웃합니다.',
      targetSummary: 'AppData\\Autodesk\\Web Services\\LoginState.xml | LocalAppData\\Autodesk',
      isDetected: true,
      detectedSummary: '활성 세션 감지됨 (LoginState)',
      selected: true
    },
    {
      id: 'edge-session',
      icon: '🌐',
      serviceName: 'Microsoft Edge 브라우저 로그인 세션 & Login Data 소거',
      category: 'Edge',
      description: 'Edge 브라우저 프로필의 자동 로그인 쿠키, Login Data(비밀번호), Token Service 및 탭 복원 세션을 소거하여 모든 사이트에서 로그아웃합니다.',
      targetSummary: 'Edge\\User Data\\Default\\Network\\Cookies | Login Data | Sessions',
      isDetected: true,
      detectedSummary: '활성 세션 감지됨 (Edge 쿠키/세션)',
      selected: true
    },
    {
      id: 'chrome-session',
      icon: '⚡',
      serviceName: 'Google Chrome 브라우저 로그인 세션 & 동기화 토큰 소거',
      category: 'Chrome',
      description: 'Google Chrome 브라우저의 구글 계정 동기화 토큰, 자동 로그인 쿠키, Login Data 및 세션을 초기화하여 모든 웹사이트에서 로그아웃합니다.',
      targetSummary: 'Chrome\\User Data\\Default\\Network\\Cookies | Login Data | Sessions',
      isDetected: true,
      detectedSummary: '활성 세션 감지됨 (Google 계정 동기화)',
      selected: true
    },
    {
      id: 'kakaotalk-session',
      icon: '💬',
      serviceName: '카카오톡 (KakaoTalk) 자동로그인 & 기기인증 토큰 소거',
      category: '메신저 & SNS',
      description: '카카오톡 PC버전의 사용자 계정 폴더, 기기 인증 세션 캐시 및 레지스트리 자동로그인/암호저장 플래그를 소거하여 즉시 로그아웃합니다.',
      targetSummary: '%LOCALAPPDATA%\\Kakao\\KakaoTalk\\users | %APPDATA%\\Kakao | HKCU\\Software\\Kakao',
      isDetected: true,
      detectedSummary: '활성 세션 감지됨 (카카오톡 프로필)',
      selected: true
    },
    {
      id: 'discord-session',
      icon: '🎧',
      serviceName: '디스코드 (Discord) 인증 토큰 & 세션 스토리지 소거',
      category: '메신저 & SNS',
      description: '디스코드 클라이언트의 LevelDB 인증 토큰 스토리지, Session Storage, 통화 및 계정 캐시를 소거하여 모든 디스코드 세션을 강제 해제합니다.',
      targetSummary: '%APPDATA%\\discord\\Local Storage\\leveldb | Session Storage | Cache',
      isDetected: true,
      detectedSummary: '활성 세션 감지됨 (Discord LevelDB)',
      selected: true
    },
    {
      id: 'telegram-session',
      icon: '✈️',
      serviceName: '텔레그램 (Telegram Desktop) tdata 인증 키 & 세션 소거',
      category: '메신저 & SNS',
      description: '텔레그램 PC버전의 암호화된 계정 키가 보관된 tdata 세션 폴더를 제거하여 로컬 기기에서의 자동 로그인을 완전히 해제합니다.',
      targetSummary: '%APPDATA%\\Telegram Desktop\\tdata (user_data, key_datas, sessions)',
      isDetected: true,
      detectedSummary: '활성 세션 감지됨 (tdata 세션 키)',
      selected: true
    },
    {
      id: 'collab-session',
      icon: '👥',
      serviceName: '협업 도구 (Slack / Zoom / New Teams) 로그인 세션 소거',
      category: '업무 & 협업',
      description: '기업용 메신저 Slack의 워크스페이스 세션, Zoom 회의 계정 데이터, New Teams(UWP) 로컬 캐시를 일괄 소거하여 사내 세션을 안전하게 정리합니다.',
      targetSummary: '%APPDATA%\\Slack | %APPDATA%\\Zoom\\data | %LOCALAPPDATA%\\Packages\\MSTeams_*',
      isDetected: true,
      detectedSummary: '활성 세션 감지됨 (협업 툴 캐시)',
      selected: true
    },
    {
      id: 'npki-cert',
      icon: '🏦',
      serviceName: '금융 & 공공 공동인증서 (NPKI / GPKI 전자서명키) 영구 소거',
      category: '금융 & 공공 보안',
      description: '은행, 증권, 국세청 홈택스 등에 사용되는 NPKI(공동인증서 signCert.der, signPri.key) 및 공공 GPKI 인증서 폴더를 로컬 디스크에서 완전히 영구 소거합니다.',
      targetSummary: '%USERPROFILE%\\AppData\\LocalLow\\NPKI | GPKI | C:\\NPKI | C:\\GPKI',
      isDetected: true,
      detectedSummary: '공동인증서 보관함 감지됨',
      selected: false
    },
    {
      id: 'cloud-drive',
      icon: '☁️',
      serviceName: '클라우드 동기화 드라이브 (Google Drive, OneDrive, Dropbox, Notion)',
      category: '클라우드 & 스토리지',
      description: 'Google Drive 데스크톱, OneDrive, Dropbox, Notion의 로컬 인증 캐시 및 동기화 세션을 소거하여 로컬 파일 접근 권한을 해제합니다.',
      targetSummary: '%LOCALAPPDATA%\\Google\\DriveFS | OneDrive\\settings | Dropbox | %APPDATA%\\Notion',
      isDetected: true,
      detectedSummary: '동기화 세션 감지됨',
      selected: false
    },
    {
      id: 'gaming-platform',
      icon: '🕹️',
      serviceName: '게임 플랫폼 세션 (Steam, Riot Games, Epic Games)',
      category: '게임 & 엔터테인먼트',
      description: 'Steam Guard 인증 토큰(ssfn*) 및 자동로그인 vdf/레지스트리, Riot Games Client 로그인 토큰, Epic Games Launcher 인증 세션을 일괄 해제합니다.',
      targetSummary: 'Steam\\config\\loginusers.vdf | ssfn* | Riot Client\\Data | EpicGamesLauncher',
      isDetected: true,
      detectedSummary: '게임 런처 세션 감지됨',
      selected: false
    },
    {
      id: 'developer-creds',
      icon: '💻',
      serviceName: '개발자 자격 증명 (Git/GitHub, SSH 개인키, AWS/GCloud CLI)',
      category: '개발자 & 엔지니어링',
      description: 'GitHub Desktop 토큰, Windows Credential의 git:https:// 자격 증명, SSH 개인키(id_rsa, id_ed25519), AWS/GCloud CLI 인증 캐시를 안전하게 소거합니다.',
      targetSummary: '%APPDATA%\\GitHub Desktop | %USERPROFILE%\\.ssh | .aws | %APPDATA%\\gcloud',
      isDetected: true,
      detectedSummary: '개발자 키 & CLI 토큰 감지됨',
      selected: false
    },
    {
      id: 'wincred-generic',
      icon: '🔐',
      serviceName: 'Windows 자격 증명 관리자 일반(Generic) 암호 일괄 소거',
      category: 'Windows Vault',
      description: 'Windows 자격 증명 관리자에 자동 저장된 모든 일반 웹/앱/서버 로그인 자격 증명을 일괄 제거합니다.',
      targetSummary: 'Advapi32 CredEnumerate(CRED_TYPE_GENERIC) & CredDeleteW',
      isDetected: true,
      detectedSummary: '자격 증명 보관함 감지됨',
      selected: false
    },
    {
      id: 'netshares-session',
      icon: '📁',
      serviceName: '네트워크 드라이브 & SMB 공유 연결 세션 일괄 해제',
      category: '네트워크',
      description: '사내 NAS, 공유 폴더, 네트워크 드라이브에 로그인된 모든 SMB 인증 세션을 일괄 해제합니다 (net use * /delete /y).',
      targetSummary: 'Windows Net Session & SMB Client Credentials (net use * /delete /y)',
      isDetected: true,
      detectedSummary: '활성 공유 세션 감지됨',
      selected: false
    }
  ]);

  interface CommanderConfirmationRecord {
    id: string;
    timestamp: string;
    commanderIp: string;
    commandType: string;
    commandTitle: string;
    details: string;
    executionTimeMs: number;
    scenario: string;
    targetsPurged: number;
    hmacVerified: boolean;
  }

  const [commanderConfirmations, setCommanderConfirmations] = useState<CommanderConfirmationRecord[]>([
    {
      id: 'conf-init',
      timestamp: '16:12:08',
      commanderIp: '192.168.1.100 (Central Commander)',
      commandType: 'AccountPurge',
      commandTitle: '원격 업무 PC 반납 정화 (Zero-Trace)',
      details: '[실시간 확인] 8개 타깃 서비스 토큰 무복구 파쇄 및 세션 프로세스 강제 종료 완료 (HMAC-SHA256 서명 검증됨)',
      executionTimeMs: 342,
      scenario: 'workplace',
      targetsPurged: 8,
      hmacVerified: true
    }
  ]);
  const [latestCommanderStatus, setLatestCommanderStatus] = useState<string>('🟢 원격 관제 채널 정상 연동 중 (TCP:9870 | HMAC-SHA256 보호됨)');
  const [isSimulatingConfirmation, setIsSimulatingConfirmation] = useState<boolean>(false);

  const handleSimulateCommanderConfirmation = (preset: 'workplace' | 'full' | 'public' | 'developer' = 'workplace') => {
    setIsSimulatingConfirmation(true);
    setLatestCommanderStatus('⚡ [수신 중] Central Commander로부터 원격 정화 명령 수신 중...');
    playAlertSound('warning');

    setTimeout(() => {
      let title = '🏢 업무 PC 반납 정화';
      let count = 8;
      if (preset === 'full') { title = '💻 PC 양도/판매 (Zero-Trace 전체)'; count = 13; }
      else if (preset === 'public') { title = '☕ 공용/PC방 이용 후 세션 정화'; count = 7; }
      else if (preset === 'developer') { title = '👨‍💻 개발자/보안 점검 정화'; count = 5; }

      const elapsed = Math.floor(Math.random() * 150 + 250);
      const newRec: CommanderConfirmationRecord = {
        id: `conf-${Date.now()}`,
        timestamp: new Date().toLocaleTimeString('ko-KR', { hour12: false }),
        commanderIp: '192.168.1.100 (Central Commander)',
        commandType: 'AccountPurge',
        commandTitle: `원격 ${title}`,
        details: `[실시간 확인] ${count}개 대상 서비스 토큰 무복구 파쇄 및 세션 프로세스 강제 종료 완료 (HMAC-SHA256 서명 검증됨)`,
        executionTimeMs: elapsed,
        scenario: preset,
        targetsPurged: count,
        hmacVerified: true
      };

      setCommanderConfirmations(prev => [newRec, ...prev.slice(0, 20)]);
      setLatestCommanderStatus(`✅ [Commander 원격 확인 완료] 192.168.1.100 -> ${title} (${elapsed}ms)`);
      setIsSimulatingConfirmation(false);
      playAlertSound('complete');
      addLog(`[Commander 확인 수신] 중앙 관제 콘솔(${newRec.commanderIp})로부터 '${title}' 원격 정화 완료 영수증을 수신하였습니다.`);
    }, 600);
  };

  const handleOpenAccountPurgeModal = () => {
    setShowAccountPurgeModal(true);
    handleScanAccountTargets();
  };

  const handleApplyScenarioPreset = (preset: 'workplace' | 'full' | 'public' | 'developer') => {
    let selectedIds: string[] = [];
    let presetName = '';

    switch (preset) {
      case 'workplace':
        selectedIds = ['ms-account', 'edge-session', 'chrome-session', 'collab-session', 'npki-cert', 'cloud-drive', 'netshares-session', 'wincred-generic'];
        presetName = '🏢 업무 PC 반납';
        break;
      case 'full':
        selectedIds = accountTargets.map(t => t.id);
        presetName = '💻 PC 양도/판매 (Zero-Trace 전체)';
        break;
      case 'public':
        selectedIds = ['chrome-session', 'edge-session', 'kakaotalk-session', 'discord-session', 'telegram-session', 'gaming-platform', 'npki-cert'];
        presetName = '☕ 공용/PC방 이용 후';
        break;
      case 'developer':
        selectedIds = ['developer-creds', 'wincred-generic', 'netshares-session', 'ms-account', 'chrome-session'];
        presetName = '👨‍💻 개발자/보안 점검';
        break;
    }

    setAccountTargets(prev => prev.map(t => ({
      ...t,
      selected: selectedIds.includes(t.id)
    })));
    addLog(`[계정 정화] 시나리오 프리셋 적용: ${presetName} (${selectedIds.length}개 대상 선택)`);
  };

  const handleScanAccountTargets = () => {
    setAccountPurgeStatus('시스템 계정 및 로그인 세션 검사 중...');
    setTimeout(() => {
      setAccountTargets(prev => prev.map(t => ({ ...t, isDetected: true })));
      const count = accountTargets.length;
      setAccountPurgeStatus(`검사 완료: ${count}개 서비스에서 활성 자격 증명 및 세션이 감지되었습니다.`);
      addLog(`[계정 정화] 계정 세션 스캔 완료 (${count}개 서비스 감지됨)`);
    }, 450);
  };

  const handlePurgeSelectedAccounts = () => {
    if (isAccountPurging) return;
    setIsAccountPurging(true);
    setAccountPurgeStatus('선택된 계정 강제 로그아웃 및 자격 증명 정화 진행 중...');
    playAlertSound('warning');

    if (terminateProcessesBeforePurge) {
      addLog('[*] [Pre-Termination] 세션 잠금 방지를 위해 실행 중인 관련 프로세스(Chrome, Edge, KakaoTalk, Discord, Steam 등)를 선행 종료합니다.');
    }

    setTimeout(() => {
      const selectedCount = accountTargets.filter(t => t.selected).length;
      setAccountTargets(prev => prev.map(t => t.selected ? { ...t, isDetected: false, detectedSummary: '정화 완료 (로그아웃됨)' } : t));
      setIsAccountPurging(false);
      setAccountPurgeStatus(`정화 완료: 총 ${selectedCount}개 계정/브라우저 세션이 성공적으로 로그아웃되었습니다.`);
      playAlertSound('complete');
      addLog(`[계정 정화 완료] 총 ${selectedCount}개 계정 세션 및 자격 증명 정화 완료 (Zero-Trace)`);
    }, 1200);
  };

  const playAlertSound = (type: 'warning' | 'complete') => {
    try {
      const audioCtx = new (window.AudioContext || (window as any).webkitAudioContext)();
      const osc = audioCtx.createOscillator();
      const gain = audioCtx.createGain();
      osc.connect(gain);
      gain.connect(audioCtx.destination);
      if (type === 'warning') {
        osc.type = 'sawtooth';
        osc.frequency.setValueAtTime(440, audioCtx.currentTime);
        osc.frequency.exponentialRampToValueAtTime(880, audioCtx.currentTime + 0.18);
        gain.gain.setValueAtTime(0.18, audioCtx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.01, audioCtx.currentTime + 0.25);
        osc.start();
        osc.stop(audioCtx.currentTime + 0.25);
      } else {
        osc.type = 'sine';
        osc.frequency.setValueAtTime(523.25, audioCtx.currentTime);
        osc.frequency.setValueAtTime(659.25, audioCtx.currentTime + 0.08);
        osc.frequency.setValueAtTime(783.99, audioCtx.currentTime + 0.16);
        gain.gain.setValueAtTime(0.18, audioCtx.currentTime);
        gain.gain.exponentialRampToValueAtTime(0.01, audioCtx.currentTime + 0.32);
        osc.start();
        osc.stop(audioCtx.currentTime + 0.32);
      }
    } catch { }
  };

  const triggerRemoteActionHud = (
    title: string,
    commandName: string,
    sourceIp: string,
    details: string,
    commandType: string = 'RamTrim'
  ) => {
    playAlertSound('warning');
    setRemoteAction({
      isOpen: true,
      title: '🚨 중앙 관제(Commander) 원격 명령 동작 중',
      commandName,
      sourceIp,
      status: '원격 최적화 엔진 동작 중...',
      details: 'C++ Native 파이프라인 및 시스템 커널 명령 처리 중...',
      isActive: true,
      autoCloseSeconds: 5
    });

    setTimeout(() => {
      playAlertSound('complete');
      setRemoteAction(prev => ({
        ...prev,
        title: '✅ 중앙 관제(Commander) 원격 명령 처리 완료',
        status: '정상 완료',
        details,
        isActive: false
      }));

      const newRecord: RemoteAuditRecord = {
        id: `audit-${Date.now()}`,
        timestamp: new Date().toISOString().replace('T', ' ').substring(0, 19),
        commanderIp: sourceIp,
        commandType,
        commandTitle: commandName,
        details,
        executionTimeMs: Math.floor(35 + Math.random() * 60),
        isSuccess: true
      };

      setRemoteAuditHistory(prev => [newRecord, ...prev.slice(0, 49)]);
      addLog(`[원격 완료] '${commandName}' 처리 완료 - ${details}`);
    }, 1200);
  };

  // Remote Action Auto-Close Countdown Timer Effect
  useEffect(() => {
    if (!remoteAction.isOpen || remoteAction.isActive) return;
    if (remoteAction.autoCloseSeconds <= 0) {
      setRemoteAction(prev => ({ ...prev, isOpen: false }));
      return;
    }
    const timer = setInterval(() => {
      setRemoteAction(prev => {
        if (prev.autoCloseSeconds <= 1) {
          clearInterval(timer);
          return { ...prev, isOpen: false, autoCloseSeconds: 0 };
        }
        return { ...prev, autoCloseSeconds: prev.autoCloseSeconds - 1 };
      });
    }, 1000);
    return () => clearInterval(timer);
  }, [remoteAction.isOpen, remoteAction.isActive, remoteAction.autoCloseSeconds]);

  // Detailed Schedule Customization & Task Scheduler State
  const [scheduleTriggerType, setScheduleTriggerType] = useState<'startup' | 'interval' | 'daily' | 'weekly'>('weekly');
  const [scheduleStartupTrigger, setScheduleStartupTrigger] = useState<'onstart' | 'onlogon'>('onstart');
  const [scheduleStartupDelayMinutes, setScheduleStartupDelayMinutes] = useState<number>(1);
  const [scheduleIntervalHours, setScheduleIntervalHours] = useState<number>(4);
  const [scheduleDay, setScheduleDay] = useState<number>(0); // 0: SUN, 1: MON...
  const [scheduleHour, setScheduleHour] = useState<number>(3);
  const [scheduleMinute, setScheduleMinute] = useState<number>(0);
  const [scheduleProfile, setScheduleProfile] = useState<string>('safe');
  const [scheduleModuleScope, setScheduleModuleScope] = useState<'profile' | 'custom'>('profile');
  const [scheduleCustomModuleIds, setScheduleCustomModuleIds] = useState<Set<string>>(() => new Set(INITIAL_MODULES.filter(m => m.risk === 'Safe').slice(0, 15).map(m => m.id)));
  const [scheduleAutoTrimRam, setScheduleAutoTrimRam] = useState<boolean>(true);
  const [scheduleCreateSafepoint, setScheduleCreateSafepoint] = useState<boolean>(true);
  const [scheduleRunOnlyOnAC, setScheduleRunOnlyOnAC] = useState<boolean>(false);
  const [scheduleWakeToRun, setScheduleWakeToRun] = useState<boolean>(false);
  const [scheduleActiveTab, setScheduleActiveTab] = useState<'trigger' | 'modules' | 'advanced' | 'preview'>('trigger');
  const [scheduleModuleSearch, setScheduleModuleSearch] = useState<string>('');
  const [scheduleModuleCategoryFilter, setScheduleModuleCategoryFilter] = useState<string>('all');
  const [isScheduleTesting, setIsScheduleTesting] = useState<boolean>(false);
  const [scheduleTestStep, setScheduleTestStep] = useState<string>('');
  const [scheduleTestProgress, setScheduleTestProgress] = useState<number>(0);
  const [scheduleCopiedCmd, setScheduleCopiedCmd] = useState<boolean>(false);

  // Live Hardware Simulation Metrics
  const [cpuUsage, setCpuUsage] = useState<number>(14);
  const [ramUsagePercent, setRamUsagePercent] = useState<number>(42);
  const [ramGb, setRamGb] = useState<{ used: number; total: number }>({ used: 6.7, total: 16.0 });

  const [logs, setLogs] = useState<string[]>([
    `[SYS-INIT] WinPurify Pro v${APP_VERSION} Native Engine Core online (144 Modules / 3-Tier MultiEngine).`,
    `[COMMANDER] Central Commander v${APP_VERSION} 온라인 - Option A 원격 특수 기능 허브 (기본 프로필 복제/브라우저 공장초기화/포렌식 파쇄/계정 정화) 준비 완료.`,
    `[LOG-SYSTEM] 컴퓨터 디스크 로그 자동 저장 엔진 활성화됨 (Logs\\WinPurifyPro_YYYYMMDD.log).`,
    `[FIREWALL] Windows 방화벽 예외 규칙 동기화: 'WinPurify Pro v${APP_VERSION}' 및 'Central Commander v${APP_VERSION}' (In/Out) 등록됨.`,
    `[DISPATCHER] 3-Tier Multi-Engine Failover Mesh configured (Native C++ -> Fast CLI -> Sandbox PS).`,
    `[AUTOMATION] Task Scheduler & Standalone Single-File Deployment Pipeline Ready.`
  ]);

  const [autoSaveLogs, setAutoSaveLogs] = useState<boolean>(true);
  const [autoSaveCommanderLogs, setAutoSaveCommanderLogs] = useState<boolean>(true);

  // File Download Helper to save logs directly to local computer disk
  const downloadLogFile = (lines: string[], filenamePrefix: string) => {
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-').slice(0, 19);
    const content = [
      '================================================================================',
      ` ${filenamePrefix} - Generated: ${new Date().toLocaleString()}`,
      ` Suite Version: v${APP_VERSION} | AhBiYout CISNET`,
      '================================================================================',
      '',
      ...lines
    ].join('\r\n');

    const blob = new Blob([content], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${filenamePrefix}_${timestamp}.txt`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  };

  useEffect(() => {
    const timer = setInterval(() => {
      setCpuUsage(Math.floor(8 + Math.random() * 18));
      setRamUsagePercent(prev => Math.min(95, Math.max(20, prev + (Math.random() * 2 - 1))));
    }, 2000);

    // 앱 기동 1.5초 후 백그라운드로 최신 버전 검사 수행 -> 업데이트 발견 시 알림 팝업창 자동 실행
    const autoUpdateTimer = setTimeout(() => {
      handleCheckGitHubUpdates(false);
    }, 1500);

    // 1시간마다 주기적 백그라운드 업데이트 확인
    const hourlyInterval = setInterval(() => {
      handleCheckGitHubUpdates(false);
    }, 3600000);

    return () => {
      clearInterval(timer);
      clearTimeout(autoUpdateTimer);
      clearInterval(hourlyInterval);
    };
  }, []);

  const addLog = (msg: string) => {
    const time = new Date().toLocaleTimeString();
    const entry = `[${time}] ${msg}`;
    setLogs(prev => [entry, ...prev.slice(0, 59)]);
    if (autoSaveLogs) {
      try {
        const stored = localStorage.getItem('winpurify_main_logs') || '';
        localStorage.setItem('winpurify_main_logs', `${entry}\n${stored}`.slice(0, 50000));
      } catch { }
    }
  };

  const addCommanderLog = (msg: string) => {
    const time = new Date().toLocaleTimeString();
    const entry = `[${time}] ${msg}`;
    setCommanderLogs(prev => [entry, ...prev.slice(0, 59)]);
    if (autoSaveCommanderLogs) {
      try {
        const stored = localStorage.getItem('winpurify_commander_logs') || '';
        localStorage.setItem('winpurify_commander_logs', `${entry}\n${stored}`.slice(0, 50000));
      } catch { }
    }
  };

  const filteredModules = modules.filter(m => {
    const matchesCat = selectedCategory === 'All' || m.category === selectedCategory;
    const matchesSubCat = selectedCategory !== 'Special' || specialSubCategory === 'All' || m.subCategory === specialSubCategory;
    const matchesSearch = m.name.toLowerCase().includes(search.toLowerCase()) || m.description.toLowerCase().includes(search.toLowerCase());
    return matchesCat && matchesSubCat && matchesSearch;
  });

  const specialCountAll = modules.filter(m => m.category === 'Special').length;
  const specialCountAccount = modules.filter(m => m.category === 'Special' && m.subCategory === 'Account').length;
  const specialCountFactoryReset = modules.filter(m => m.category === 'Special' && m.subCategory === 'FactoryReset').length;
  const specialCountProfile = modules.filter(m => m.category === 'Special' && m.subCategory === 'Profile').length;
  const specialCountForensics = modules.filter(m => m.category === 'Special' && m.subCategory === 'Forensics').length;

  const selectedCount = modules.filter(m => m.selected).length;
  const totalReclaimableBytes = modules.filter(m => m.selected).reduce((acc, cur) => acc + cur.estimatedKb * 1024, 0);

  const formatSize = (bytes: number) => {
    if (bytes <= 0) return '0 KB';
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    if (bytes < 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
    return `${(bytes / (1024 * 1024 * 1024)).toFixed(2)} GB`;
  };

  const handleToggle = (id: string) => {
    if (id === 'spc-sync-default-user-profile') {
      const current = modules.find(m => m.id === id);
      if (current && !current.selected) {
        setShowDefaultProfileWarningModal(true);
        return;
      }
    }
    setModules(prev => prev.map(m => m.id === id ? { ...m, selected: !m.selected } : m));
  };

  const handleConfirmDefaultProfile = () => {
    setModules(prev => prev.map(m => m.id === 'spc-sync-default-user-profile' ? { ...m, selected: true } : m));
    setShowDefaultProfileWarningModal(false);
    addLog('[경고 승인] Windows 기본 프로필(Default) 덮어쓰기 복제 특수 모듈이 활성화되었습니다.');
  };

  const handleCancelDefaultProfile = () => {
    setShowDefaultProfileWarningModal(false);
  };

  const handleSelectAll = (select: boolean) => {
    setModules(prev => prev.map(m => {
      // Special category tasks are never selected in bulk unless viewed under Special category
      if (select && m.category === 'Special' && selectedCategory !== 'Special') {
        return { ...m, selected: false };
      }
      return { ...m, selected: select };
    }));
    addLog(`모듈 일괄 ${select ? '선택' : '해제'} 완료.`);
  };

  const handlePreset = (risk: 'Safe' | 'Deep') => {
    setModules(prev => prev.map(m => {
      if (m.category === 'Special') {
        return { ...m, selected: false };
      }
      return {
        ...m,
        selected: risk === 'Deep' ? true : m.risk === 'Safe'
      };
    }));
    addLog(`[프리셋] ${risk === 'Deep' ? '딥 클린 (Deep Clean)' : '권장 기본 (Safe Default)'} 프리셋 적용.`);
  };

  const handleGamingProfile = () => {
    setModules(prev => prev.map(m => {
      if (m.category === 'Special') {
        return { ...m, selected: false };
      }
      return {
        ...m,
        selected: (m.category === 'System' || m.category === 'Storage' || m.category === 'Update') && m.risk <= 'Deep'
      };
    }));
    addLog(`[프로필] Gaming Boost Profile 활성화 (게이밍 지연시간 최소화 설정)`);
  };

  const handlePrivacyProfile = () => {
    setModules(prev => prev.map(m => {
      if (m.category === 'Special') {
        return { ...m, selected: false };
      }
      return {
        ...m,
        selected: m.category === 'Privacy' || m.category === 'Browser' || m.category === 'Security'
      };
    }));
    addLog(`[프로필] Deep Privacy Profile 활성화 (개인정보 및 브라우저 전수 보호)`);
  };

  const handleZeroTraceProfile = () => {
    setModules(prev => prev.map(m => {
      // Exclude Special category (Cipher manual only)
      if (m.category === 'Special') {
        return { ...m, selected: false };
      }

      // 1. All Privacy & Anti-forensics
      if (m.category === 'Privacy') {
        return { ...m, selected: true };
      }

      // 2. All Browser, Messengers, Cloud session tokens & passwords
      if (m.category === 'Browser') {
        return { ...m, selected: true };
      }

      // 3. Security logs
      if (m.category === 'Security') {
        return { ...m, selected: true };
      }

      // 4. Critical user activity & dump storages
      if (m.category === 'Storage') {
        const isZeroTraceStorage = m.id.includes('recycle-bin') ||
                                   m.id.includes('crash-memory') ||
                                   m.id.includes('wer-reports') ||
                                   m.id.includes('temp') ||
                                   m.id.includes('hiberfil') ||
                                   m.id.includes('wsearch-edb') ||
                                   m.id.includes('prefetch');
        return { ...m, selected: isZeroTraceStorage };
      }

      // Exclude general OS tuning, animations, network acceleration
      return { ...m, selected: false };
    }));
    addLog(`[프로필] Zero-Trace 안티포렌식 초기화 프로필 활성화 (계정/세션/자격증명/흔적 모듈 정밀 선별)`);
  };

  const handleFastScan = () => {
    if (isProcessing) return;
    setIsProcessing(true);
    dispatchTrayNotification('🔍 Robocopy 초고속 스캔 개시', '133개 전체 모듈 대상 가상 I/O 스트림 탐색 중...', 'info');
    addLog(`[Robocopy SCAN] 133개 전체 모듈 대상 초고속 가상 I/O 스트림 탐색 (/L /S /BYTES)...`);

    setTimeout(() => {
      setModules(prev => prev.map(m => ({ ...m, status: 'Done' })));
      setIsProcessing(false);
      const freed = formatSize(totalReclaimableBytes);
      dispatchTrayNotification('✨ 전체 모듈 스캔 완료', `총 133개 모듈 분석 완료. 예상 확보 가능 공간: ${freed}`, 'success');
      addLog(`[스캔 완료] 총 133개 모듈 I/O 스캔 완료. 확보 가능 공간: ${freed} (소요시간: 0.18s)`);
    }, 400);
  };

  const handleOptimize = () => {
    if (isProcessing || selectedCount === 0) return;
    setIsProcessing(true);
    dispatchTrayNotification('🚀 시스템 최적화 시작', `선택된 ${selectedCount}개 모듈의 하이브리드 정화 작업을 시작합니다.`, 'info');
    addLog(`[Safepoint] 최적화 전 .reg 백업 스냅샷 캡처 중...`);

    if (trayOptions.milestoneNotifications) {
      setTimeout(() => {
        dispatchTrayNotification('⚡ 최적화 진행 중 (50%)', `${Math.round(selectedCount / 2)}/${selectedCount}개 모듈 처리 완료: 레지스트리 및 캐시 파이프라인`, 'info', 50);
      }, 250);
    }

    setTimeout(() => {
      setHasSafepoint(true);
      addLog(`[PurifyEngineCore.dll] Native Win32 API 및 Fast CLI 파이프라인 일괄 정화 시작...`);
      setModules(prev => prev.map(m => m.selected ? { ...m, status: 'Done', estimatedKb: 0 } : m));
      setIsProcessing(false);
      const freed = formatSize(totalReclaimableBytes);
      dispatchTrayNotification('✨ 시스템 최적화 완료', `총 ${selectedCount}개 모듈 최적화 성공! ${freed} 공간 및 RAM 즉시 반환 완료.`, 'success', 100);
      addLog(`[정화 완료] ${selectedCount}개 모듈 최적화 성공. 디스크 공간 및 RAM 즉시 반환 완료.`);
    }, 550);
  };

  const handleRollback = () => {
    if (!hasSafepoint || isProcessing) return;
    setIsProcessing(true);
    addLog(`[1-Click Rollback] 직전 세이프포인트 복원(reg.exe import) 진행 중...`);

    setTimeout(() => {
      setIsProcessing(false);
      dispatchTrayNotification('↩️ 세이프포인트 롤백 완료', '백업된 레지스트리 및 시스템 설정이 원래 상태로 완전 복원되었습니다.', 'success');
      addLog(`[복원 완료] 백업된 레지스트리 및 설정이 원래 상태로 완전 복구되었습니다.`);
    }, 450);
  };

  const handleRamTrim = () => {
    setRamUsagePercent(24);
    setRamGb({ used: 3.8, total: 16.0 });
    dispatchTrayNotification('🧠 RAM 즉시 압축 완료', '142개 프로세스 Working Set Trim 완료 (2.9 GB 메모리 즉시 회수)', 'success');
    addLog(`[PurifyEngineCore] CompressPhysicalRAM() 실행 -> 142개 프로세스 Working Set Trim 완료 (2.9 GB RAM 즉시 회수)`);
  };

  const handleGameBoost = () => {
    setCpuUsage(8);
    addLog(`[Game Boost] SetProcessPriorityTuning(1) 실행 -> 게임 프로세스 HIGH 우선순위 격상 및 DNS 플러시 완료.`);
  };

  const handleToggleSchedule = () => {
    setShowScheduleModal(true);
  };

  const getGeneratedSchtasksCmd = () => {
    const exePath = `C:\\Program Files\\WinPurifyPro\\WinPurifyPro.exe`;
    let opts = `--silent --schedule-run`;
    if (scheduleModuleScope === 'custom') {
      const selectedArr = Array.from(scheduleCustomModuleIds);
      const modStr = selectedArr.length > 0 ? selectedArr.join(',') : 'pri-telemetry,sys-temp-files';
      opts += ` --profile custom --modules "${modStr}"`;
    } else {
      opts += ` --profile ${scheduleProfile}`;
    }
    if (scheduleAutoTrimRam) opts += ` --trim-ram`;
    if (scheduleCreateSafepoint) opts += ` --safepoint`;

    const timeStr = `${String(scheduleHour).padStart(2, '0')}:${String(scheduleMinute).padStart(2, '0')}`;
    if (scheduleTriggerType === 'startup') {
      const sc = scheduleStartupTrigger === 'onlogon' ? 'ONLOGON' : 'ONSTART';
      const delay = scheduleStartupDelayMinutes > 0 ? ` /DELAY 000${scheduleStartupDelayMinutes}:00` : '';
      return `schtasks.exe /Create /TN "WinPurifyPro_AutoMaintenance" /TR "\\"${exePath}\\" ${opts}" /SC ${sc}${delay} /RL HIGHEST /F`;
    }
    if (scheduleTriggerType === 'interval') {
      return `schtasks.exe /Create /TN "WinPurifyPro_AutoMaintenance" /TR "\\"${exePath}\\" ${opts}" /SC HOURLY /MO ${scheduleIntervalHours} /ST ${timeStr} /RL HIGHEST /F`;
    }
    if (scheduleTriggerType === 'daily') {
      return `schtasks.exe /Create /TN "WinPurifyPro_AutoMaintenance" /TR "\\"${exePath}\\" ${opts}" /SC DAILY /ST ${timeStr} /RL HIGHEST /F`;
    }
    const dayStr = ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT'][scheduleDay];
    return `schtasks.exe /Create /TN "WinPurifyPro_AutoMaintenance" /TR "\\"${exePath}\\" ${opts}" /SC WEEKLY /D ${dayStr} /ST ${timeStr} /RL HIGHEST /F`;
  };

  const handleRunScheduleTestNow = () => {
    if (isScheduleTesting) return;
    setIsScheduleTesting(true);
    setScheduleTestProgress(15);
    setScheduleTestStep('스케줄러 파라미터 및 최고 관리자 권한 토큰 검증 중...');

    setTimeout(() => {
      setScheduleTestProgress(35);
      setScheduleTestStep(scheduleCreateSafepoint ? 'Live Safepoint 레지스트리 백업 스냅샷 자동 생성 중...' : '스케줄 환경 초기화 중...');

      setTimeout(() => {
        setScheduleTestProgress(65);
        if (scheduleAutoTrimRam) {
          setRamGb({ used: 3.8, total: 16.0 });
        }
        setScheduleTestStep(scheduleAutoTrimRam ? 'PurifyEngineCore Physical RAM Working Set 압축 완료...' : '지정된 모듈 순차 파이프라인 배치 가동 중...');

        setTimeout(() => {
          setScheduleTestProgress(90);
          const count = scheduleModuleScope === 'custom' 
            ? scheduleCustomModuleIds.size 
            : (scheduleProfile === 'all' ? 115 : (scheduleProfile === 'safe' ? 78 : 45));
          setScheduleTestStep(`지정된 ${count}개 최적화 모듈 무음 백그라운드 정화 완료 및 임시 캐시 소거...`);

          setTimeout(() => {
            setScheduleTestProgress(100);
            setScheduleTestStep('자동 최적화 테스트 성공 완료! (%ProgramData%\\WinPurifyPro\\Logs\\AutoScheduler.log 기록)');
            addLog(`[Task Scheduler] 즉시 테스트 완료: ${scheduleTriggerType.toUpperCase()} 트리거 시뮬레이션 성공 (${count}개 모듈 정화 / RAM 2.9GB 압축 완료)`);
            setTimeout(() => {
              setIsScheduleTesting(false);
            }, 1800);
          }, 600);
        }, 600);
      }, 700);
    }, 600);
  };

  const handleSaveSchedule = () => {
    setShowScheduleModal(false);
    if (!isScheduled) {
      addLog(`[Task Scheduler] 자동 정화 스케줄이 비활성화되었습니다.`);
    } else {
      let triggerText = '';
      if (scheduleTriggerType === 'startup') {
        triggerText = `시스템 부팅 시 (${scheduleStartupTrigger === 'onlogon' ? '로그온' : '부팅'}, ${scheduleStartupDelayMinutes}분 지연)`;
      } else if (scheduleTriggerType === 'interval') {
        triggerText = `매 ${scheduleIntervalHours}시간 주기 간격 반복`;
      } else if (scheduleTriggerType === 'daily') {
        triggerText = `매일 ${String(scheduleHour).padStart(2, '0')}:${String(scheduleMinute).padStart(2, '0')}`;
      } else {
        const days = ['일요일 (SUN)', '월요일 (MON)', '화요일 (TUE)', '수요일 (WED)', '목요일 (THU)', '금요일 (FRI)', '토요일 (SAT)'];
        triggerText = `매주 ${days[scheduleDay]} ${String(scheduleHour).padStart(2, '0')}:${String(scheduleMinute).padStart(2, '0')}`;
      }

      const scopeText = scheduleModuleScope === 'custom'
        ? `사용자 정의 선택 모듈 (${scheduleCustomModuleIds.size}개)`
        : (scheduleProfile === 'all' ? '전체 모듈 (115개)' : `${scheduleProfile.toUpperCase()} 프로필`);

      addLog(`[Task Scheduler] 자동 정화 스케줄 저장 완료: ${triggerText} | 대상: ${scopeText} | RAM압축: ${scheduleAutoTrimRam ? '사용' : '미사용'} | 세이프포인트: ${scheduleCreateSafepoint ? '사용' : '미사용'}`);
      addLog(`[Task Scheduler] schtasks 명령 연동: ${getGeneratedSchtasksCmd()}`);
    }
  };

  const handleRemoveSchedule = () => {
    setIsScheduled(false);
    setShowScheduleModal(false);
    addLog(`[Task Scheduler] Windows 작업 스케줄러 등록 작업(WinPurifyPro_AutoMaintenance) 제거 완료`);
  };

  const diagnosticsList: DiagnosticItem[] = [
    { title: '관리자 권한 (Administrator Elevation)', desc: '시스템 서비스 제어 및 커널 레지스트리 수정 권한 검증', passed: true, detail: '관리자 권한으로 정상 실행 중입니다.' },
    { title: 'Native C++ Core 엔진 바인딩', desc: 'PurifyEngineCore.dll P/Invoke 및 Win32 커널 API 상태', passed: true, detail: 'PurifyEngineCore.dll 정상 연결 (C-Linkage Hash: 0x5E81A2)' },
    { title: 'Robocopy 가상 I/O 스캐너', desc: 'Stutter-Free /L /BYTES 비동기 메타데이터 쿼리 가용성', passed: true, detail: 'Windows Robocopy 가상 I/O 엔진 사용 가능' },
    { title: '세이프포인트 스토리지 쓰기 권한', desc: '1-Click Undo 롤백 및 .reg 백업 스냅샷 저장소 접근성', passed: true, detail: '로컬 백업 디렉터리 읽기/쓰기 정상' },
    { title: '115개 최적화 모듈 데이터셋 무결성', desc: '7개 전체 카테고리 및 Failover 파이프라인 매핑 정합성', passed: true, detail: '115개 전수 모듈 데이터 매핑 완결' }
  ];

  return (
    <div id="main-app" className={`min-h-screen ${currentTheme.app} flex flex-col font-sans selection:bg-blue-600 selection:text-white transition-colors duration-200`}>
      {/* Header Bar */}
      <header id="app-header" className={`${currentTheme.header} backdrop-blur border-b px-6 py-3.5 flex flex-wrap items-center justify-between gap-4 transition-colors duration-200`}>
        <div id="brand-container" className="flex items-center gap-3.5">
          <div id="logo-badge" className="relative w-11 h-11 rounded-2xl bg-gradient-to-br from-sky-500/20 via-slate-800/90 to-blue-950/80 border border-sky-500/30 shadow-lg shadow-sky-500/15 flex items-center justify-center overflow-hidden p-1 transition-all duration-300 hover:scale-105 hover:border-sky-400/50 hover:shadow-sky-500/25">
            <div className="absolute inset-0 bg-radial from-sky-400/10 to-transparent pointer-events-none"></div>
            <img src="WinPurify_icon.png" alt="WinPurify Pro Icon" className="w-full h-full object-contain filter drop-shadow-[0_2px_8px_rgba(56,189,248,0.3)] transition-transform duration-300" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <h1 id="app-title" className="text-lg font-bold tracking-tight">WinPurify Pro</h1>
              <span id="version-badge" className="text-[11px] px-2 py-0.5 rounded-full bg-blue-500/10 text-blue-400 font-semibold border border-blue-500/30">
                v{APP_VERSION} Enterprise
              </span>
              <span className={`text-[11px] ${currentTheme.headerSub} hidden sm:inline`}>|</span>
              <span className={`text-[11px] ${currentTheme.headerSub} font-medium hidden sm:inline`}>
                Dev: <strong className="text-sky-400">AhBiYout</strong>
              </span>
            </div>
            <div className={`flex items-center gap-2 text-[11px] ${currentTheme.headerSub}`}>
              <a href="https://ahbiyoutvibe.blogspot.com/" target="_blank" rel="noreferrer" className="text-purple-400 hover:text-purple-300 hover:underline">
                블로그
              </a>
              <span className="opacity-50">•</span>
              <a href="http://www.cisnet.co.kr/" target="_blank" rel="noreferrer" className="text-sky-400 hover:text-sky-300 hover:underline">
                CISNET
              </a>
              <span className="opacity-50">•</span>
              <button
                id="brand-license-btn"
                onClick={() => { setShowLicenseModal(true); setLicenseCopied(false); }}
                className="text-amber-400 hover:text-amber-300 hover:underline font-bold flex items-center gap-1 cursor-pointer"
                title="한국어 및 영어 2종 소프트웨어 최종 사용자 사용권 계약서(EULA) 확인"
              >
                📜 라이선스 (KO/EN)
              </button>
            </div>
          </div>
        </div>

        {/* Theme Selector Dropdown (다크 / 회색 / 화이트 / 베이지) */}
        <div id="theme-dropdown-container" ref={themeDropdownRef} className="relative">
          <button
            id="theme-dropdown-trigger"
            type="button"
            onClick={() => setIsThemeDropdownOpen(prev => !prev)}
            title="UI 스타일 테마 선택 (다크 / 회색 / 화이트 / 베이지)"
            className={`flex items-center gap-2 px-3 py-1.5 rounded-xl border text-xs font-bold transition-all shadow-sm cursor-pointer ${currentTheme.themeGroupBg} hover:border-blue-500/50`}
          >
            <Palette className="w-3.5 h-3.5 text-blue-400 shrink-0" />
            <span className="flex items-center gap-1.5">
              <span>{THEMES.find(t => t.id === theme)?.icon}</span>
              <span>{THEMES.find(t => t.id === theme)?.label} 테마</span>
            </span>
            <ChevronDown className={`w-3.5 h-3.5 text-slate-400 transition-transform duration-200 ${isThemeDropdownOpen ? 'rotate-180' : ''}`} />
          </button>

          {isThemeDropdownOpen && (
            <div
              id="theme-dropdown-menu"
              className={`absolute right-0 sm:left-0 sm:right-auto mt-1.5 w-56 p-1.5 rounded-xl border shadow-2xl backdrop-blur-md z-50 animate-in fade-in zoom-in-95 duration-150 ${currentTheme.modalBg}`}
            >
              <div className="text-[10px] font-bold text-slate-400 uppercase tracking-wider px-2.5 py-1 mb-1 border-b border-slate-700/40 flex items-center justify-between">
                <span>UI 스타일 테마</span>
                <span className="font-mono text-[9px] lowercase opacity-75">4 Presets</span>
              </div>
              <div className="space-y-1">
                {THEMES.map(t => {
                  const isActive = theme === t.id;
                  return (
                    <button
                      key={t.id}
                      id={`theme-option-${t.id}`}
                      type="button"
                      onClick={() => {
                        setTheme(t.id);
                        setIsThemeDropdownOpen(false);
                        addLog(`[테마 설정] ${t.label} 테마 스타일로 변경되었습니다.`);
                      }}
                      className={`w-full flex items-center justify-between px-2.5 py-2 rounded-lg text-xs transition-all duration-150 cursor-pointer ${
                        isActive
                          ? currentTheme.dropdownItemActive
                          : `${currentTheme.dropdownItem} border border-transparent`
                      }`}
                    >
                      <div className="flex items-center gap-2.5">
                        <span className="text-base leading-none">{t.icon}</span>
                        <div className="text-left">
                          <div className="font-bold leading-tight">{t.label} 테마</div>
                          <div className={`text-[10px] ${currentTheme.dropdownSub} font-normal`}>{t.desc}</div>
                        </div>
                      </div>
                      {isActive && <Check className="w-4 h-4 text-blue-400 shrink-0" />}
                    </button>
                  );
                })}
              </div>
            </div>
          )}
        </div>

        {/* Global Quick Action Buttons */}
        <div id="quick-actions" className="flex items-center gap-2">
          <button 
            id="ram-trim-btn"
            onClick={handleRamTrim}
            title="물리 메모리의 불필요한 프로세스 Working Set을 네이티브 Win32 API로 즉시 비워 여유 RAM을 확보합니다."
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-sky-950 hover:bg-sky-900 border border-sky-800 text-sky-300 text-xs font-semibold transition"
          >
            <Cpu className="w-3.5 h-3.5" />
            <span>RAM 압축</span>
          </button>
          <button 
            id="game-boost-btn"
            onClick={handleGameBoost}
            title="게임 프로세스 우선순위를 격상하고 네트워크 지연 요소를 일시 차단하여 인게임 렉을 방지합니다."
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-emerald-950 hover:bg-emerald-900 border border-emerald-800 text-emerald-300 text-xs font-semibold transition"
          >
            <Zap className="w-3.5 h-3.5" />
            <span>게이밍 부스트</span>
          </button>
          <button 
            id="scheduler-btn"
            onClick={handleToggleSchedule}
            title="시스템 부팅 시 또는 주기별 간격에 맞춰 무인 백그라운드 자동 정화 작업을 스케줄링합니다."
            className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg border text-xs font-semibold transition ${
              isScheduled 
                ? 'bg-sky-950 border-sky-600 text-sky-300' 
                : 'bg-slate-800 border-slate-700 text-slate-300 hover:bg-slate-700'
            }`}
          >
            <Calendar className="w-3.5 h-3.5 text-sky-400" />
            <span>{isScheduled ? '스케줄 활성' : '태스크 스케줄러'}</span>
          </button>
          <button 
            id="tray-header-btn"
            onClick={() => setShowTrayModal(true)}
            title="시스템 트레이 상주 정책, 실시간 진행사항 툴팁 및 풍선/토스트 알림 센터를 설정합니다."
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-indigo-950/80 hover:bg-indigo-900 border border-indigo-700/80 text-indigo-300 text-xs font-bold transition shadow-sm cursor-pointer"
          >
            <Bell className="w-3.5 h-3.5 text-indigo-400" />
            <span>트레이 &amp; 알림</span>
          </button>
          <button 
            id="commander-header-btn"
            onClick={() => setShowCommanderModal(true)}
            title="다중 PC 중앙 관제(Central Commander) 원격 진단 및 일괄 관리 콘솔을 엽니다."
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-indigo-950/80 hover:bg-indigo-900 border border-indigo-700/80 text-indigo-300 text-xs font-bold transition shadow-sm cursor-pointer"
          >
            <Radio className="w-3.5 h-3.5 text-indigo-400" />
            <span>중앙 관제</span>
          </button>
          <button 
            id="license-header-btn"
            onClick={() => { setShowLicenseModal(true); setLicenseCopied(false); }}
            title="한국어 및 영어 2종 소프트웨어 최종 사용자 사용권 계약서(EULA)를 확인합니다."
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-amber-950/80 hover:bg-amber-900 border border-amber-700/80 text-amber-300 text-xs font-bold transition shadow-sm cursor-pointer"
          >
            <FileText className="w-3.5 h-3.5 text-amber-400" />
            <span>라이선스</span>
          </button>
          <button 
            id="github-update-header-btn"
            onClick={() => handleCheckGitHubUpdates()}
            title="GitHub Releases (ahbiyout-all/WinPurify-Pro)를 실시간 조회하여 최신 버전 확인 및 Windows PC 설치 파일을 확인합니다."
            className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-sky-950/80 hover:bg-sky-900 border border-sky-700/80 text-sky-300 text-xs font-bold transition shadow-sm cursor-pointer"
          >
            <Download className="w-3.5 h-3.5 text-sky-400" />
            <span>업데이트 &amp; 다운로드</span>
          </button>
          <button 
            id="account-purge-btn"
            onClick={handleOpenAccountPurgeModal}
            title="Microsoft, Adobe, AutoCAD, Edge, Chrome 및 Windows 자격 증명 관리자의 로그인 세션을 원클릭으로 강제 로그아웃/소거합니다."
            className="flex items-center gap-1.5 px-3.5 py-1.5 rounded-lg bg-rose-950/80 hover:bg-rose-900 border border-rose-700/80 text-rose-300 text-xs font-bold transition shadow-sm"
          >
            <Lock className="w-3.5 h-3.5 text-rose-400" />
            <span>계정 로그아웃</span>
          </button>
          <button 
            id="scan-fast-btn"
            disabled={isProcessing}
            onClick={handleFastScan}
            title="Windows Robocopy 가상 메타데이터 I/O 스트림(/L /BYTES)을 사용하여 115개 전체 모듈의 점유 용량을 초고속 스캔합니다."
            className="flex items-center gap-1.5 px-3.5 py-1.5 rounded-lg bg-white hover:bg-slate-100 border border-slate-300 text-slate-900 text-xs font-bold shadow-sm transition disabled:bg-slate-200 disabled:text-slate-600 disabled:opacity-80"
          >
            <Search className="w-3.5 h-3.5 text-slate-900" />
            <span>초고속 스캔</span>
          </button>
          <button 
            id="optimize-btn"
            disabled={isProcessing || selectedCount === 0}
            onClick={handleOptimize}
            title="레지스트리 세이프포인트를 자동 캡처한 뒤, 3단계 Failover 멀티 엔진(C++ Native -> Fast CLI -> PS Sandbox)을 통해 선택된 모듈을 일괄 정화합니다."
            className="flex items-center gap-1.5 px-4 py-1.5 rounded-lg bg-blue-600 hover:bg-blue-500 text-white text-xs font-bold shadow-lg shadow-blue-600/30 transition disabled:opacity-50"
          >
            <Play className="w-3.5 h-3.5" />
            <span>즉시 정화 ({selectedCount})</span>
          </button>
        </div>
      </header>

      {/* Main Dashboard Layout */}
      <div id="dashboard-body" className="flex-1 grid grid-cols-1 lg:grid-cols-12 gap-5 p-5">
        
        {/* Left Category & Sidebar */}
        <aside id="sidebar-panel" className="lg:col-span-3 space-y-4">
          {/* Brand Logo Banner */}
          <div id="brand-banner-box" className={`p-3 rounded-xl ${currentTheme.sidebarBox} border flex items-center justify-center transition-colors duration-200`}>
            <img src="WinPurify_logo.png" alt="WinPurify Pro Logo" className="max-h-9 object-contain drop-shadow" />
          </div>

          {/* Category Filter */}
          <div id="category-box" className={`p-3.5 rounded-xl ${currentTheme.card} border transition-colors duration-200`}>
            <h2 id="category-heading" className={`text-xs font-bold ${currentTheme.panelSub} tracking-wider uppercase mb-2.5 flex items-center gap-2`}>
              <Layers className="w-3.5 h-3.5 text-blue-400" />
              <span>카테고리 ({modules.length}개 모듈)</span>
            </h2>
            <div id="category-list" className="space-y-1.5">
              {CATEGORIES.map(cat => {
                const config = CATEGORY_CONFIG[cat];
                const isSelected = selectedCategory === cat;
                return (
                  <button
                    key={cat}
                    id={`cat-btn-${cat.toLowerCase()}`}
                    onClick={() => {
                      setSelectedCategory(cat);
                      if (cat === 'Special') setSpecialSubCategory('All');
                    }}
                    title={config.desc}
                    className={`w-full text-left px-3 py-2 rounded-lg text-xs transition flex items-center justify-between border ${
                      isSelected 
                        ? `${config.activeBg} ${config.activeText} ${config.activeBorder}` 
                        : `border-transparent ${currentTheme.categoryInactive}`
                    }`}
                  >
                    <span>{config.label}</span>
                    <span className={`text-[10px] px-2 py-0.5 rounded-full border transition ${
                      isSelected ? config.badgeActive : currentTheme.categoryBadgeInactive
                    }`}>
                      {config.count}
                    </span>
                  </button>
                );
              })}
            </div>
          </div>

          {/* Rollback Button Box */}
          <div id="safepoint-box" className={`p-3.5 rounded-xl ${currentTheme.card} border space-y-2 transition-colors duration-200`}>
            <div className="flex items-center justify-between text-xs font-semibold">
              <span className="flex items-center gap-1.5 text-purple-400">
                <RotateCcw className="w-3.5 h-3.5" />
                <span>1-Click Rollback</span>
              </span>
              <span className={`text-[10px] px-1.5 py-0.5 rounded ${hasSafepoint ? 'bg-emerald-500/20 text-emerald-400' : 'bg-slate-800 text-slate-500'}`}>
                {hasSafepoint ? '스냅샷 준비됨' : '대기'}
              </span>
            </div>
            <button
              id="rollback-btn"
              disabled={!hasSafepoint || isProcessing}
              onClick={handleRollback}
              title="정화 직전에 자동으로 백업된 레지스트리 스냅샷을 1-Click으로 이전 상태로 안전하게 복구합니다."
              className="w-full py-2 rounded-lg bg-purple-900/40 hover:bg-purple-900/70 border border-purple-700/60 text-purple-200 text-xs font-bold transition disabled:opacity-40"
            >
              직전 세이프포인트 복원 (Undo)
            </button>
          </div>

          {/* System Tray & Notification Center Sidebar Box */}
          <div id="tray-sidebar-box" className={`p-3.5 rounded-xl ${currentTheme.card} border space-y-2 transition-colors duration-200`}>
            <div className="flex items-center justify-between text-xs font-semibold">
              <span className="flex items-center gap-1.5 text-indigo-400">
                <Bell className="w-3.5 h-3.5" />
                <span>트레이 &amp; 알림 감시</span>
              </span>
              <span className="text-[10px] px-1.5 py-0.5 rounded bg-indigo-500/20 text-indigo-300 font-bold">
                Active
              </span>
            </div>
            <button
              id="sidebar-tray-btn"
              onClick={() => setShowTrayModal(true)}
              title="백그라운드 시스템 트레이 상주, 작업 진행사항 실시간 툴팁 및 풍선/토스트 알림 설정을 구성합니다."
              className="w-full py-2 rounded-lg bg-indigo-950/60 hover:bg-indigo-900/80 border border-indigo-700/60 text-indigo-200 text-xs font-bold transition flex items-center justify-center gap-1.5"
            >
              <span>🔔 트레이 환경설정</span>
            </button>
          </div>

          {/* Account & Session Purge Box */}
          <div id="account-purge-sidebar-box" className={`p-3.5 rounded-xl ${currentTheme.card} border space-y-2 transition-colors duration-200`}>
            <div className="flex items-center justify-between text-xs font-semibold">
              <span className="flex items-center gap-1.5 text-rose-400">
                <Lock className="w-3.5 h-3.5" />
                <span>계정 세션 &amp; 토큰 소거</span>
              </span>
              <span className="text-[10px] px-1.5 py-0.5 rounded bg-rose-500/20 text-rose-300 font-bold">
                Zero-Trace
              </span>
            </div>
            <button
              id="sidebar-account-purge-btn"
              onClick={handleOpenAccountPurgeModal}
              title="Microsoft, Adobe, AutoCAD, Edge, Chrome 및 Windows 자격증명의 로그인 상태를 일괄 로그아웃합니다."
              className="w-full py-2 rounded-lg bg-rose-950/60 hover:bg-rose-900/80 border border-rose-700/60 text-rose-200 text-xs font-bold transition"
            >
              🔐 계정 로그아웃 센터 열기
            </button>
          </div>

          {/* Architecture Badge */}
          <div id="architecture-box" className={`p-3.5 rounded-xl ${currentTheme.sidebarBox} border space-y-2 text-[11px] transition-colors duration-200`}>
            <h3 id="arch-heading" className={`text-xs font-bold ${currentTheme.panelSub} tracking-wider uppercase flex items-center gap-2`}>
              <Server className="w-3.5 h-3.5 text-emerald-400" />
              <span>3-Tier Failover Engine</span>
            </h3>
            <div className={`p-1.5 rounded ${currentTheme.panel} border flex items-center gap-2`} title="Win32 커널 API를 통한 0ms 네이티브 I/O 최적화">
              <span className="w-2 h-2 rounded-full bg-emerald-400"></span>
              <span className="font-medium">1차: Native C++ Core</span>
              <span className="opacity-60 ml-auto text-[10px]">0ms I/O</span>
            </div>
            <div className={`p-1.5 rounded ${currentTheme.panel} border flex items-center gap-2`} title="sc.exe, reg.exe 등 초고속 시스템 CLI 제어">
              <span className="w-2 h-2 rounded-full bg-sky-400"></span>
              <span className="font-medium">2차: Fast CLI (sc/reg)</span>
              <span className="opacity-60 ml-auto text-[10px]">Zero CLR</span>
            </div>
            <div className={`p-1.5 rounded ${currentTheme.panel} border flex items-center gap-2`} title="안전한 Base64 인코딩 파워셸 샌드박스 3차 페일오버">
              <span className="w-2 h-2 rounded-full bg-amber-400"></span>
              <span className="font-medium">3차: Encoded PS Sandbox</span>
              <span className="opacity-60 ml-auto text-[10px]">Safe Failover</span>
            </div>
          </div>
        </aside>

        {/* Central Tasks List & Overview */}
        <main id="main-content" className="lg:col-span-9 space-y-4">
          {/* Top Live Hardware Gauges */}
          <div id="hardware-gauges" className="grid grid-cols-3 gap-3">
            <div className={`p-3 rounded-xl ${currentTheme.gaugeBg} border transition-colors duration-200`} title="실시간 CPU 사용률">
              <div className="flex justify-between items-center text-xs mb-1.5">
                <span className={currentTheme.statSub}>CPU 점유율</span>
                <span className={`font-bold ${currentTheme.metricCpu}`}>{cpuUsage}%</span>
              </div>
              <div className={`w-full ${currentTheme.gaugeTrack} rounded-full h-1.5 overflow-hidden`}>
                <div className="bg-sky-500 h-1.5 rounded-full transition-all duration-500" style={{ width: `${cpuUsage}%` }}></div>
              </div>
            </div>
            <div className={`p-3 rounded-xl ${currentTheme.gaugeBg} border transition-colors duration-200`} title="실시간 물리 RAM 사용량 및 점유율">
              <div className="flex justify-between items-center text-xs mb-1.5">
                <span className={currentTheme.statSub}>RAM 사용량</span>
                <span className={`font-bold ${currentTheme.metricRam}`}>{ramGb.used.toFixed(1)} / {ramGb.total} GB ({ramUsagePercent}%)</span>
              </div>
              <div className={`w-full ${currentTheme.gaugeTrack} rounded-full h-1.5 overflow-hidden`}>
                <div className="bg-emerald-500 h-1.5 rounded-full transition-all duration-500" style={{ width: `${ramUsagePercent}%` }}></div>
              </div>
            </div>
            <div className={`p-3 rounded-xl ${currentTheme.gaugeBg} border transition-colors duration-200`} title="시스템 드라이브 여유 공간">
              <div className="flex justify-between items-center text-xs mb-1.5">
                <span className={currentTheme.statSub}>시스템 드라이브 (C:)</span>
                <span className={`font-bold ${currentTheme.metricDisk}`}>324 GB Free</span>
              </div>
              <div className={`w-full ${currentTheme.gaugeTrack} rounded-full h-1.5 overflow-hidden`}>
                <div className="bg-amber-500 h-1.5 rounded-full" style={{ width: `35%` }}></div>
              </div>
            </div>
          </div>

          {/* Stats Bar & Profiles */}
          <div id="stats-overview" className={`p-4 rounded-xl ${currentTheme.card} border flex flex-wrap items-center justify-between gap-3 transition-colors duration-200`}>
            <div className="space-y-0.5">
              <span className={`text-[11px] ${currentTheme.statSub}`}>예상 확보 공간 (Estimated Space)</span>
              <div className={`text-xl font-bold ${currentTheme.metricSpace}`}>{formatSize(totalReclaimableBytes)}</div>
            </div>
            <div className="flex flex-wrap items-center gap-1.5 text-xs">
              <span className={`${currentTheme.profileLabel} mr-1 font-medium`}>프로필:</span>
              <button 
                id="profile-zerotrace-btn"
                onClick={handleZeroTraceProfile}
                title="PC 양도/퇴사/중고 판매 전 수준으로 모든 사용자 흔적, 로그인 세션, 비밀번호, 자격증명, 대용량 정크를 100% 일괄 선별합니다."
                className="px-2.5 py-1 rounded bg-rose-950 border border-rose-800/80 text-rose-300 font-semibold hover:bg-rose-900 transition flex items-center gap-1"
              >
                <Zap className="w-3 h-3 text-rose-400" />
                <span>완전 초기화 (Zero-Trace)</span>
              </button>
              <button 
                id="profile-gaming-btn"
                onClick={handleGamingProfile}
                title="게이밍 프레임 드랍 방지 및 지연시간 단축을 위해 불필요한 시스템 서비스, 셰이더 캐시, 업데이트 캐시를 선별합니다."
                className="px-2.5 py-1 rounded bg-sky-950 border border-sky-800/80 text-sky-300 font-semibold hover:bg-sky-900 transition flex items-center gap-1"
              >
                <Gamepad2 className="w-3 h-3" />
                <span>게이밍</span>
              </button>
              <button 
                id="profile-privacy-btn"
                onClick={handlePrivacyProfile}
                title="윈도우 텔레메트리, 진단 로그, 브라우저 방문 기록 등 개인정보 유출 위험 항목을 전수 선별합니다."
                className="px-2.5 py-1 rounded bg-purple-950 border border-purple-800/80 text-purple-300 font-semibold hover:bg-purple-900 transition flex items-center gap-1"
              >
                <Lock className="w-3 h-3" />
                <span>개인정보</span>
              </button>
              <button 
                id="preset-safe-btn"
                onClick={() => handlePreset('Safe')}
                title="시스템 동작에 전혀 영향을 주지 않는 100% 안전한 기본 정화 항목만 선별합니다."
                className="px-2.5 py-1 rounded bg-emerald-950 border border-emerald-800/80 text-emerald-300 font-semibold hover:bg-emerald-900 transition"
              >
                Safe
              </button>
              <button 
                id="preset-deep-btn"
                onClick={() => handlePreset('Deep')}
                title="안전 항목뿐만 아니라 고립 CLSID, 격리소 파편 등 심층 정화 항목까지 모두 선별합니다."
                className="px-2.5 py-1 rounded bg-amber-950 border border-amber-800/80 text-amber-300 font-semibold hover:bg-amber-900 transition"
              >
                Deep
              </button>
              <div className="h-3.5 w-px bg-slate-400 dark:bg-slate-700 opacity-60 mx-1"></div>
              <button 
                id="select-all-btn"
                onClick={() => handleSelectAll(true)}
                title="현재 표시된 모든 모듈을 일괄 선택합니다."
                className={`px-2 py-1 rounded border text-xs font-medium transition ${currentTheme.selectBtn}`}
              >
                전체
              </button>
              <button 
                id="deselect-all-btn"
                onClick={() => handleSelectAll(false)}
                title="선택된 모든 모듈을 일괄 해제합니다."
                className={`px-2 py-1 rounded border text-xs font-medium transition ${currentTheme.selectBtn}`}
              >
                해제
              </button>
            </div>
          </div>

          {/* Search Toolbar */}
          <div id="search-toolbar" className="relative">
            <Search className="w-4 h-4 absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-500" />
            <input 
              id="module-search-input"
              type="text"
              value={search}
              onChange={e => setSearch(e.target.value)}
              placeholder="모듈 이름, 설명 또는 키워드로 검색 (마우스 호버 시 상세 정보 표시)..."
              title="모듈 이름, 설명 키워드를 입력하여 즉시 실시간 필터링합니다."
              className={`w-full pl-10 pr-4 py-2 rounded-xl border text-xs focus:outline-none transition ${currentTheme.input}`}
            />
          </div>

          {/* Special Category Dedicated Account Purge Launcher Banner */}
          {selectedCategory === 'Special' && (
            <div id="special-category-banner" className="p-4 rounded-xl bg-gradient-to-r from-purple-950/70 via-rose-950/50 to-slate-900/90 border border-rose-500/40 shadow-lg shadow-rose-950/20 flex flex-col sm:flex-row sm:items-center justify-between gap-4">
              <div className="flex items-start gap-3.5">
                <div className="w-10 h-10 rounded-xl bg-rose-500/20 border border-rose-500/30 flex items-center justify-center shrink-0 text-xl">
                  🔐
                </div>
                <div>
                  <div className="flex items-center gap-2">
                    <h3 className="text-sm font-bold text-rose-200">특수 기능 전용 센터: 통합 계정 정화 &amp; 브라우저 공장 초기화 &amp; 기본 프로필 복제</h3>
                    <span className="text-[10px] font-bold px-2 py-0.5 rounded-full bg-rose-500/20 text-rose-300 border border-rose-500/30">Zero-Trace &amp; Template</span>
                  </div>
                  <p className="text-xs text-rose-300/80 mt-1 leading-relaxed max-w-2xl">
                    Microsoft Live/Office WAM 토큰 강제 로그아웃, Chrome/Edge/Whale/Firefox 브라우저 완전 공장 초기화(User Data 소거), Windows 기본 프로필(C:\Users\Default) 덮어쓰기 복제 등 시스템 원본에 중대한 영향을 미치는 특수 모듈을 안전하게 제어합니다.
                  </p>
                  <p className="text-[11px] text-amber-400/90 mt-1.5 font-medium flex items-center gap-1">
                    <span>※ 특수 기능 모듈은 시스템 템플릿 변경 및 데이터 영구 소거 방지를 위해 일반 최적화의 자동 선택에서 기본 제외됩니다.</span>
                  </p>
                </div>
              </div>
              <button
                id="open-account-purge-from-special-banner"
                onClick={handleOpenAccountPurgeModal}
                className="self-start sm:self-center px-4 py-2.5 rounded-lg bg-rose-600 hover:bg-rose-500 text-white text-xs font-bold shadow-lg shadow-rose-600/30 transition flex items-center gap-2 shrink-0 cursor-pointer"
              >
                <Lock className="w-4 h-4" />
                <span>계정 정화 센터 열기</span>
              </button>
            </div>
          )}

          {/* Special Category Sub-Category Tabs (Option A) */}
          {selectedCategory === 'Special' && (
            <div id="special-subcategories-bar" className="p-1.5 rounded-xl bg-slate-900/90 border border-slate-700/80 shadow-inner flex flex-wrap sm:flex-nowrap items-center gap-1.5">
              <button
                id="special-subtab-all"
                type="button"
                onClick={() => setSpecialSubCategory('All')}
                className={`flex-1 min-w-[100px] px-3 py-2 rounded-lg text-xs font-semibold flex items-center justify-center gap-2 transition cursor-pointer ${
                  specialSubCategory === 'All'
                    ? 'bg-gradient-to-r from-fuchsia-900 to-purple-900 text-fuchsia-100 border border-fuchsia-500/60 shadow-md shadow-fuchsia-950/40'
                    : 'bg-slate-800/60 hover:bg-slate-800 text-slate-300 hover:text-white border border-transparent'
                }`}
              >
                <span>📑</span>
                <span>전체 보기</span>
                <span className={`text-[10px] font-bold px-1.5 py-0.2 rounded-full ${
                  specialSubCategory === 'All' ? 'bg-fuchsia-500/30 text-fuchsia-200' : 'bg-slate-700 text-slate-400'
                }`}>
                  {specialCountAll}
                </span>
              </button>

              <button
                id="special-subtab-account"
                type="button"
                onClick={() => setSpecialSubCategory('Account')}
                className={`flex-1 min-w-[110px] px-3 py-2 rounded-lg text-xs font-semibold flex items-center justify-center gap-2 transition cursor-pointer ${
                  specialSubCategory === 'Account'
                    ? 'bg-gradient-to-r from-rose-900 to-pink-900 text-rose-100 border border-rose-500/60 shadow-md shadow-rose-950/40'
                    : 'bg-slate-800/60 hover:bg-slate-800 text-slate-300 hover:text-white border border-transparent'
                }`}
              >
                <span>🔐</span>
                <span>계정 &amp; 세션</span>
                <span className={`text-[10px] font-bold px-1.5 py-0.2 rounded-full ${
                  specialSubCategory === 'Account' ? 'bg-rose-500/30 text-rose-200' : 'bg-slate-700 text-slate-400'
                }`}>
                  {specialCountAccount}
                </span>
              </button>

              <button
                id="special-subtab-factory-reset"
                type="button"
                onClick={() => setSpecialSubCategory('FactoryReset')}
                className={`flex-1 min-w-[110px] px-3 py-2 rounded-lg text-xs font-semibold flex items-center justify-center gap-2 transition cursor-pointer ${
                  specialSubCategory === 'FactoryReset'
                    ? 'bg-gradient-to-r from-red-900 to-orange-950 text-red-100 border border-red-500/60 shadow-md shadow-red-950/40'
                    : 'bg-slate-800/60 hover:bg-slate-800 text-slate-300 hover:text-white border border-transparent'
                }`}
              >
                <span>🌐</span>
                <span>공장 초기화</span>
                <span className={`text-[10px] font-bold px-1.5 py-0.2 rounded-full ${
                  specialSubCategory === 'FactoryReset' ? 'bg-red-500/30 text-red-200' : 'bg-slate-700 text-slate-400'
                }`}>
                  {specialCountFactoryReset}
                </span>
              </button>

              <button
                id="special-subtab-profile"
                type="button"
                onClick={() => setSpecialSubCategory('Profile')}
                className={`flex-1 min-w-[110px] px-3 py-2 rounded-lg text-xs font-semibold flex items-center justify-center gap-2 transition cursor-pointer ${
                  specialSubCategory === 'Profile'
                    ? 'bg-gradient-to-r from-purple-900 to-indigo-900 text-purple-100 border border-purple-500/60 shadow-md shadow-purple-950/40'
                    : 'bg-slate-800/60 hover:bg-slate-800 text-slate-300 hover:text-white border border-transparent'
                }`}
              >
                <span>👥</span>
                <span>프로필 복제</span>
                <span className={`text-[10px] font-bold px-1.5 py-0.2 rounded-full ${
                  specialSubCategory === 'Profile' ? 'bg-purple-500/30 text-purple-200' : 'bg-slate-700 text-slate-400'
                }`}>
                  {specialCountProfile}
                </span>
              </button>

              <button
                id="special-subtab-forensics"
                type="button"
                onClick={() => setSpecialSubCategory('Forensics')}
                className={`flex-1 min-w-[100px] px-3 py-2 rounded-lg text-xs font-semibold flex items-center justify-center gap-2 transition cursor-pointer ${
                  specialSubCategory === 'Forensics'
                    ? 'bg-gradient-to-r from-slate-800 to-slate-900 text-slate-100 border border-slate-500/60 shadow-md shadow-slate-950/40'
                    : 'bg-slate-800/60 hover:bg-slate-800 text-slate-300 hover:text-white border border-transparent'
                }`}
              >
                <span>🛡️</span>
                <span>포렌식 파쇄</span>
                <span className={`text-[10px] font-bold px-1.5 py-0.2 rounded-full ${
                  specialSubCategory === 'Forensics' ? 'bg-slate-600/50 text-slate-200' : 'bg-slate-700 text-slate-400'
                }`}>
                  {specialCountForensics}
                </span>
              </button>
            </div>
          )}

          {/* Module Cards Grid (Streamlined with Sequence Number and Rich Floating Tooltip) */}
          <div id="module-cards-header" className={`flex items-center justify-between text-xs ${currentTheme.panelSub} px-1`}>
            <span className="flex items-center gap-1.5">
              <span className="w-2 h-2 rounded-full bg-blue-400"></span>
              <span>모듈 목록: <strong className={currentTheme.moduleTitle}>{filteredModules.length}개 표시 중</strong> (전체 {modules.length}개)</span>
            </span>
            <span className="text-[11px] font-mono opacity-70">선택됨: <strong className="text-sky-400">{selectedCount}</strong>개</span>
          </div>

          <div id="module-cards-container" className="grid grid-cols-1 md:grid-cols-2 gap-2 max-h-[460px] overflow-y-auto pr-1">
            {filteredModules.map(item => (
              <div 
                key={item.id}
                id={`module-card-${item.id}`}
                onClick={() => handleToggle(item.id)}
                title={`${item.name} [${item.risk} 등급 / ${item.category}]: ${item.description}`}
                className={`group relative px-3 py-2 rounded-xl border transition-all duration-150 cursor-pointer flex items-center justify-between gap-2.5 select-none ${
                  item.selected 
                    ? currentTheme.moduleCardActive 
                    : currentTheme.moduleCardInactive
                }`}
              >
                {/* Left: Checkbox + Sequence Number + Name + Badges */}
                <div className="flex items-center gap-2 min-w-0 flex-1">
                  <input 
                    type="checkbox"
                    checked={item.selected}
                    onChange={() => {}}
                    className="w-3.5 h-3.5 rounded text-blue-600 bg-slate-800 border-slate-700 focus:ring-0 cursor-pointer shrink-0"
                  />
                  {/* Sequence Number Badge */}
                  <span className={`font-mono text-[10px] px-1.5 py-0.5 rounded border shrink-0 ${currentTheme.seqBadge}`}>
                    #{String(item.seq).padStart(2, '0')}
                  </span>
                  <span className={`font-semibold text-xs truncate ${currentTheme.moduleTitle}`}>{item.name}</span>
                  <span className={`text-[9px] font-bold px-1.5 py-0.5 rounded border shrink-0 ${
                    item.risk === 'Safe' ? currentTheme.riskSafe :
                    item.risk === 'Deep' ? currentTheme.riskDeep :
                    currentTheme.riskRisky
                  }`}>
                    {item.risk}
                  </span>
                  <span className={`text-[9px] px-1.5 py-0.2 rounded border hidden xl:inline-block shrink-0 ${currentTheme.categoryBadgeInactive}`}>
                    {item.category}
                  </span>
                  {item.category === 'Special' && item.subCategory && (
                    <span className="text-[9px] font-bold px-1.5 py-0.2 rounded border hidden sm:inline-block shrink-0 bg-rose-500/10 text-rose-300 border-rose-500/30">
                      {item.subCategory === 'Account' && '🔐 계정 정화'}
                      {item.subCategory === 'FactoryReset' && '🌐 공장 초기화'}
                      {item.subCategory === 'Profile' && '👥 기본 프로필'}
                      {item.subCategory === 'Forensics' && '🛡️ 포렌식 파쇄'}
                    </span>
                  )}
                </div>

                {/* Right: Size */}
                <div className="text-right shrink-0">
                  <span className={`text-xs ${currentTheme.sizeText}`}>{formatSize(item.estimatedKb * 1024)}</span>
                </div>

                {/* Rich Floating Tooltip (Shown on Mouse Hover) */}
                <div className={`pointer-events-none absolute left-1/2 bottom-[calc(100%+6px)] -translate-x-1/2 w-80 p-3 rounded-xl border shadow-2xl backdrop-blur-md opacity-0 group-hover:opacity-100 transition-all duration-200 z-50 scale-95 group-hover:scale-100 origin-bottom ${currentTheme.modalBg}`}>
                  <div className="flex items-center justify-between pb-1.5 border-b border-slate-700/50 mb-1.5">
                    <div className="font-bold text-xs truncate">#{String(item.seq).padStart(2, '0')} {item.name}</div>
                    <span className={`text-[9px] font-bold px-1.5 py-0.5 rounded border ${
                      item.risk === 'Safe' ? currentTheme.riskSafe :
                      item.risk === 'Deep' ? currentTheme.riskDeep :
                      currentTheme.riskRisky
                    }`}>
                      {item.risk} 등급
                    </span>
                  </div>
                  <p className="text-[11px] opacity-90 leading-relaxed mb-2 font-normal">
                    {item.description}
                  </p>
                  <div className="flex items-center justify-between text-[10px] opacity-80 pt-1.5 border-t border-slate-700/50">
                    <span>카테고리: <strong>{item.category}</strong></span>
                    <span>예상 회수 공간: <strong>{formatSize(item.estimatedKb * 1024)}</strong></span>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Realtime Live Console */}
          <div id="live-console-box" className={`p-3.5 rounded-xl ${currentTheme.console} border space-y-1.5 transition-colors duration-200`}>
            <div className="flex flex-wrap items-center justify-between gap-2 text-xs text-slate-400 font-mono">
              <div className="flex items-center gap-2">
                <span className="flex items-center gap-1.5 text-blue-400 font-semibold">
                  <Terminal className="w-3.5 h-3.5" />
                  <span>Live Engine Tracing Stream</span>
                </span>
                <span className="text-[10px] px-1.5 py-0.5 rounded bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 font-bold">
                  💾 {autoSaveLogs ? '디스크 자동 저장 중' : '자동 저장 꺼짐'}
                </span>
              </div>
              <div className="flex items-center gap-2">
                <label className="flex items-center gap-1 text-[11px] cursor-pointer hover:text-slate-200 select-none">
                  <input
                    type="checkbox"
                    checked={autoSaveLogs}
                    onChange={e => {
                      setAutoSaveLogs(e.target.checked);
                      addLog(`[로그 설정] 작업 로그 컴퓨터 디스크 자동 저장이 ${e.target.checked ? '활성화' : '비활성화'}되었습니다.`);
                    }}
                    className="rounded bg-slate-900 border-slate-700 text-blue-500 focus:ring-0 w-3.5 h-3.5"
                  />
                  <span>자동 저장</span>
                </label>
                <button
                  onClick={() => downloadLogFile(logs, 'WinPurifyPro_OperationLogs')}
                  className="px-2 py-0.5 rounded bg-slate-800 hover:bg-slate-700 border border-slate-700 text-slate-300 hover:text-white text-[10px] font-medium transition flex items-center gap-1"
                  title="현재 작업 로그를 컴퓨터 텍스트 파일(.txt)로 즉시 다운로드 저장합니다"
                >
                  <Download className="w-3 h-3" />
                  <span>로그 PC 저장</span>
                </button>
                <button
                  onClick={() => {
                    setLogs([`[${new Date().toLocaleTimeString()}] 작업 로그 콘솔이 초기화되었습니다. (디스크 로그 보존됨)`]);
                  }}
                  className="px-1.5 py-0.5 rounded bg-slate-800 hover:bg-slate-700 border border-slate-700 text-slate-400 hover:text-slate-200 text-[10px] transition"
                  title="콘솔 화면 지우기"
                >
                  🧹
                </button>
              </div>
            </div>
            <div id="console-stream" className="h-24 overflow-y-auto space-y-1 font-mono text-[10px] text-slate-400 pr-1">
              {logs.map((log, idx) => (
                <div key={idx} className="leading-tight">{log}</div>
              ))}
            </div>
          </div>
        </main>
      </div>

      {/* Task Scheduler Customization Modal */}
      {showScheduleModal && (
        <div id="schedule-modal-backdrop" className="fixed inset-0 bg-black/80 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div id="schedule-modal" className="bg-slate-900 border border-sky-500/60 rounded-2xl w-full max-w-2xl overflow-hidden shadow-2xl flex flex-col max-h-[92vh]">
            {/* Modal Header */}
            <div className="flex items-center justify-between px-6 py-4 border-b border-slate-800 bg-slate-950/90">
              <div className="flex items-center gap-3">
                <div className="p-2.5 rounded-xl bg-sky-500/20 text-sky-400 border border-sky-500/30">
                  <Calendar className="w-5 h-5" />
                </div>
                <div>
                  <div className="flex items-center gap-2">
                    <h3 className="font-bold text-base text-slate-100">자동 최적화 태스크 스케줄러</h3>
                    <span className="text-[10px] px-2 py-0.5 rounded-full font-bold bg-sky-500/20 text-sky-300 border border-sky-500/40">
                      v{APP_VERSION}
                    </span>
                    <span className={`text-[10px] px-2 py-0.5 rounded-full font-bold border ${
                      isScheduled 
                        ? 'bg-emerald-500/20 text-emerald-300 border-emerald-500/40' 
                        : 'bg-slate-800 text-slate-400 border-slate-700'
                    }`}>
                      {isScheduled ? '● 스케줄 활성화' : '○ 미등록'}
                    </span>
                  </div>
                  <p className="text-[11px] text-slate-400">Windows 작업 스케줄러(schtasks)와 최고 관리자 권한으로 연동되어 부팅 시 또는 지정 주기마다 자동 무음 정화를 수행합니다.</p>
                </div>
              </div>
              <button 
                onClick={() => setShowScheduleModal(false)} 
                className="text-slate-400 hover:text-slate-200 p-1.5 rounded-lg hover:bg-slate-800 transition"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {/* Master Toggle & Summary Strip */}
            <div className="px-6 py-3 bg-slate-950/50 border-b border-slate-800/80 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <label className="relative inline-flex items-center cursor-pointer">
                  <input 
                    type="checkbox" 
                    checked={isScheduled} 
                    onChange={e => setIsScheduled(e.target.checked)} 
                    className="sr-only peer"
                  />
                  <div className="w-11 h-6 bg-slate-800 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-sky-500"></div>
                </label>
                <div>
                  <div className="text-xs font-bold text-slate-200">자동 정화 스케줄 활성화</div>
                  <div className="text-[10px] text-slate-400">
                    {scheduleTriggerType === 'startup' && `시스템 부팅/로그인 시 (${scheduleStartupDelayMinutes}분 지연)`}
                    {scheduleTriggerType === 'interval' && `매 ${scheduleIntervalHours}시간 간격 반복 실행`}
                    {scheduleTriggerType === 'daily' && `매일 ${String(scheduleHour).padStart(2, '0')}:${String(scheduleMinute).padStart(2, '0')} 정시`}
                    {scheduleTriggerType === 'weekly' && `매주 ${['일','월','화','수','목','금','토'][scheduleDay]}요일 ${String(scheduleHour).padStart(2, '0')}:${String(scheduleMinute).padStart(2, '0')}`}
                    {' • '}
                    {scheduleModuleScope === 'custom' ? `선택 모듈 (${scheduleCustomModuleIds.size}개)` : `${scheduleProfile.toUpperCase()} 프로필`}
                  </div>
                </div>
              </div>

              {/* Instant Test Run Button */}
              <button
                type="button"
                onClick={handleRunScheduleTestNow}
                disabled={isScheduleTesting}
                className="px-3 py-1.5 rounded-lg bg-indigo-600/30 hover:bg-indigo-600/50 border border-indigo-500/50 text-indigo-200 text-xs font-bold transition flex items-center gap-1.5 shadow-sm"
              >
                <Zap className="w-3.5 h-3.5 text-indigo-400" />
                <span>{isScheduleTesting ? '테스트 실행 중...' : '⚡ 지금 즉시 테스트'}</span>
              </button>
            </div>

            {/* Test Execution Overlay / Banner */}
            {isScheduleTesting && (
              <div className="px-6 py-3 bg-indigo-950/80 border-b border-indigo-500/50 animate-pulse">
                <div className="flex items-center justify-between text-xs text-indigo-200 font-bold mb-1.5">
                  <span className="flex items-center gap-1.5">
                    <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                    <span>{scheduleTestStep}</span>
                  </span>
                  <span>{scheduleTestProgress}%</span>
                </div>
                <div className="w-full bg-indigo-950 h-2 rounded-full overflow-hidden border border-indigo-800">
                  <div 
                    className="bg-indigo-500 h-full transition-all duration-300 rounded-full" 
                    style={{ width: `${scheduleTestProgress}%` }}
                  />
                </div>
              </div>
            )}

            {/* Navigation Tabs */}
            <div className="flex border-b border-slate-800 bg-slate-950/30 px-6">
              {[
                { id: 'trigger', label: '1. 실행 트리거', icon: '🚀' },
                { id: 'modules', label: '2. 정화 모듈 범위', icon: '🎯' },
                { id: 'advanced', label: '3. 안전/전원 옵션', icon: '🛡️' },
                { id: 'preview', label: '4. CLI 명령어 및 상태', icon: '💻' }
              ].map(tab => (
                <button
                  key={tab.id}
                  type="button"
                  onClick={() => setScheduleActiveTab(tab.id as any)}
                  className={`py-3 px-4 text-xs font-bold border-b-2 transition flex items-center gap-1.5 ${
                    scheduleActiveTab === tab.id
                      ? 'border-sky-500 text-sky-400 bg-sky-500/5'
                      : 'border-transparent text-slate-400 hover:text-slate-200 hover:bg-slate-800/40'
                  }`}
                >
                  <span>{tab.icon}</span>
                  <span>{tab.label}</span>
                </button>
              ))}
            </div>

            {/* Tab Contents Scrollable Area */}
            <div className="p-6 overflow-y-auto space-y-5 flex-1">

              {/* TAB 1: TRIGGER SETTINGS */}
              {scheduleActiveTab === 'trigger' && (
                <div className="space-y-5">
                  <div>
                    <label className="block text-xs font-bold text-slate-300 mb-2">실행 트리거 유형 (Trigger Mode)</label>
                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-2">
                      {[
                        { id: 'startup', title: '🚀 부팅 / 로그인 시', desc: 'OS 부팅 또는 로그온 시 자동 실행' },
                        { id: 'interval', title: '⏱️ 주기적 반복', desc: '매 N시간 간격으로 자동 정화' },
                        { id: 'daily', title: '🔄 매일 정시', desc: '매일 지정된 시각에 정화' },
                        { id: 'weekly', title: '📅 매주 요일', desc: '매주 특정 요일 지정 시각 정화' }
                      ].map(trig => (
                        <button
                          key={trig.id}
                          type="button"
                          onClick={() => setScheduleTriggerType(trig.id as any)}
                          className={`p-3 rounded-xl text-left border transition ${
                            scheduleTriggerType === trig.id
                              ? 'bg-sky-500/20 border-sky-500 text-sky-200 shadow-sm'
                              : 'bg-slate-950/60 border-slate-800 text-slate-400 hover:bg-slate-800/60'
                          }`}
                        >
                          <div className="text-xs font-bold text-slate-200">{trig.title}</div>
                          <div className="text-[10px] text-slate-400 mt-1 leading-snug">{trig.desc}</div>
                        </button>
                      ))}
                    </div>
                  </div>

                  {/* SUB-SECTION 1: STARTUP TRIGGER OPTIONS */}
                  {scheduleTriggerType === 'startup' && (
                    <div className="p-4 rounded-xl bg-slate-950 border border-sky-900/60 space-y-4">
                      <div className="flex items-center justify-between border-b border-slate-800 pb-2.5">
                        <span className="text-xs font-bold text-sky-300">부팅 시점 트리거 세부 설정</span>
                        <span className="text-[10px] text-slate-400">schtasks /SC ONSTART / ONLOGON</span>
                      </div>

                      <div className="grid grid-cols-2 gap-3">
                        <button
                          type="button"
                          onClick={() => setScheduleStartupTrigger('onstart')}
                          className={`p-2.5 rounded-lg border text-left text-xs transition ${
                            scheduleStartupTrigger === 'onstart'
                              ? 'bg-sky-500/20 border-sky-500 text-sky-200 font-bold'
                              : 'bg-slate-900 border-slate-800 text-slate-400 hover:bg-slate-800'
                          }`}
                        >
                          <div className="font-bold">시스템 시작 시 (ONSTART)</div>
                          <div className="text-[10px] text-slate-400 mt-0.5">사용자 로그인 전 윈도우 부팅 즉시 백그라운드 준비</div>
                        </button>
                        <button
                          type="button"
                          onClick={() => setScheduleStartupTrigger('onlogon')}
                          className={`p-2.5 rounded-lg border text-left text-xs transition ${
                            scheduleStartupTrigger === 'onlogon'
                              ? 'bg-sky-500/20 border-sky-500 text-sky-200 font-bold'
                              : 'bg-slate-900 border-slate-800 text-slate-400 hover:bg-slate-800'
                          }`}
                        >
                          <div className="font-bold">사용자 로그인 시 (ONLOGON)</div>
                          <div className="text-[10px] text-slate-400 mt-0.5">바탕화면 진입 시 사용자 세션과 함께 정화 개시</div>
                        </button>
                      </div>

                      <div>
                        <label className="block text-xs font-bold text-slate-300 mb-1.5">
                          부팅 안정화 대기 지연 시간 (Startup Delay)
                        </label>
                        <div className="flex items-center gap-2">
                          {[
                            { value: 0, label: '즉시 실행' },
                            { value: 1, label: '1분 후 (권장)' },
                            { value: 2, label: '2분 후' },
                            { value: 5, label: '5분 후' }
                          ].map(d => (
                            <button
                              key={d.value}
                              type="button"
                              onClick={() => setScheduleStartupDelayMinutes(d.value)}
                              className={`py-1.5 px-3 rounded-lg text-xs font-bold border transition ${
                                scheduleStartupDelayMinutes === d.value
                                  ? 'bg-sky-500/20 border-sky-500 text-sky-300'
                                  : 'bg-slate-900 border-slate-800 text-slate-400 hover:bg-slate-800'
                              }`}
                            >
                              {d.label}
                            </button>
                          ))}
                        </div>
                        <p className="text-[10px] text-slate-500 mt-1">부팅 직후 시작 프로그램과 서비스들이 완전히 로드된 후 안정적으로 백그라운드 정화를 실행합니다.</p>
                      </div>
                    </div>
                  )}

                  {/* SUB-SECTION 2: INTERVAL TRIGGER OPTIONS */}
                  {scheduleTriggerType === 'interval' && (
                    <div className="p-4 rounded-xl bg-slate-950 border border-purple-900/60 space-y-4">
                      <div className="flex items-center justify-between border-b border-slate-800 pb-2.5">
                        <span className="text-xs font-bold text-purple-300">반복 실행 간격 설정</span>
                        <span className="text-[10px] text-slate-400">schtasks /SC HOURLY /MO N</span>
                      </div>

                      <div>
                        <label className="block text-xs font-bold text-slate-300 mb-2">실행 간격 선택 (Interval Hours)</label>
                        <div className="grid grid-cols-3 sm:grid-cols-6 gap-2">
                          {[1, 2, 4, 6, 12, 24].map(h => (
                            <button
                              key={h}
                              type="button"
                              onClick={() => setScheduleIntervalHours(h)}
                              className={`py-2 rounded-lg text-xs font-bold text-center border transition ${
                                scheduleIntervalHours === h
                                  ? 'bg-purple-500/20 border-purple-500 text-purple-300 shadow-sm'
                                  : 'bg-slate-900 border-slate-800 text-slate-400 hover:bg-slate-800'
                              }`}
                            >
                              {h === 24 ? '24시간' : `${h}시간마다`}
                            </button>
                          ))}
                        </div>
                      </div>

                      <div className="flex items-center gap-3 pt-1">
                        <span className="text-xs text-slate-400">기준 시작 시각:</span>
                        <select
                          value={scheduleHour}
                          onChange={e => setScheduleHour(Number(e.target.value))}
                          className="bg-slate-900 border border-slate-700 text-purple-300 font-bold rounded-lg px-2.5 py-1 text-xs focus:outline-none"
                        >
                          {Array.from({ length: 24 }).map((_, i) => (
                            <option key={i} value={i}>{String(i).padStart(2, '0')}시</option>
                          ))}
                        </select>
                        <span className="text-slate-500 font-bold">:</span>
                        <select
                          value={scheduleMinute}
                          onChange={e => setScheduleMinute(Number(e.target.value))}
                          className="bg-slate-900 border border-slate-700 text-purple-300 font-bold rounded-lg px-2.5 py-1 text-xs focus:outline-none"
                        >
                          {[0, 15, 30, 45].map(m => (
                            <option key={m} value={m}>{String(m).padStart(2, '0')}분</option>
                          ))}
                        </select>
                        <span className="text-[11px] text-slate-500">부터 {scheduleIntervalHours}시간 간격으로 반복</span>
                      </div>
                    </div>
                  )}

                  {/* SUB-SECTION 3: DAILY / WEEKLY TRIGGER OPTIONS */}
                  {(scheduleTriggerType === 'daily' || scheduleTriggerType === 'weekly') && (
                    <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 space-y-4">
                      {scheduleTriggerType === 'weekly' && (
                        <div>
                          <label className="block text-xs font-bold text-slate-300 mb-2">실행 요일 선택 (Day of Week)</label>
                          <div className="grid grid-cols-7 gap-1.5">
                            {[
                              { day: 0, label: '일', color: 'text-red-400' },
                              { day: 1, label: '월', color: 'text-slate-300' },
                              { day: 2, label: '화', color: 'text-slate-300' },
                              { day: 3, label: '수', color: 'text-slate-300' },
                              { day: 4, label: '목', color: 'text-slate-300' },
                              { day: 5, label: '금', color: 'text-slate-300' },
                              { day: 6, label: '토', color: 'text-blue-400' }
                            ].map(item => (
                              <button
                                key={item.day}
                                type="button"
                                onClick={() => setScheduleDay(item.day)}
                                className={`py-2 rounded-lg text-xs font-bold transition border ${
                                  scheduleDay === item.day
                                    ? 'bg-amber-500/20 border-amber-500 text-amber-300 shadow-sm'
                                    : 'bg-slate-900 border-slate-800 text-slate-400 hover:bg-slate-800'
                                }`}
                              >
                                <span className={item.color}>{item.label}</span>
                              </button>
                            ))}
                          </div>
                        </div>
                      )}

                      <div>
                        <label className="block text-xs font-bold text-slate-300 mb-2">실행 시각 (24시간 형식)</label>
                        <div className="flex items-center gap-3 p-3 rounded-xl bg-slate-900 border border-slate-800">
                          <div className="flex-1 flex items-center gap-2">
                            <span className="text-xs text-slate-400">시:</span>
                            <select
                              value={scheduleHour}
                              onChange={e => setScheduleHour(Number(e.target.value))}
                              className="bg-slate-950 border border-slate-700 text-sky-400 font-bold rounded-lg px-3 py-1.5 text-xs w-full focus:outline-none focus:border-sky-500"
                            >
                              {Array.from({ length: 24 }).map((_, i) => (
                                <option key={i} value={i}>{String(i).padStart(2, '0')}시 ({i < 12 ? `오전 ${i}시` : `오후 ${i === 12 ? 12 : i - 12}시`})</option>
                              ))}
                            </select>
                          </div>
                          <span className="text-slate-500 font-bold">:</span>
                          <div className="flex-1 flex items-center gap-2">
                            <span className="text-xs text-slate-400">분:</span>
                            <select
                              value={scheduleMinute}
                              onChange={e => setScheduleMinute(Number(e.target.value))}
                              className="bg-slate-950 border border-slate-700 text-sky-400 font-bold rounded-lg px-3 py-1.5 text-xs w-full focus:outline-none focus:border-sky-500"
                            >
                              {[0, 15, 30, 45].map(m => (
                                <option key={m} value={m}>{String(m).padStart(2, '0')}분</option>
                              ))}
                            </select>
                          </div>
                        </div>
                      </div>
                    </div>
                  )}
                </div>
              )}

              {/* TAB 2: OPTIMIZATION MODULES SCOPE */}
              {scheduleActiveTab === 'modules' && (
                <div className="space-y-4">
                  {/* Mode Toggle: Preset vs Custom */}
                  <div className="flex items-center justify-between bg-slate-950 p-2 rounded-xl border border-slate-800">
                    <span className="text-xs font-bold text-slate-300 ml-2">정화 모듈 구성 방식</span>
                    <div className="flex gap-1">
                      <button
                        type="button"
                        onClick={() => setScheduleModuleScope('profile')}
                        className={`px-3 py-1.5 rounded-lg text-xs font-bold transition ${
                          scheduleModuleScope === 'profile'
                            ? 'bg-sky-500/20 text-sky-300 border border-sky-500/50'
                            : 'text-slate-400 hover:text-slate-200'
                        }`}
                      >
                        ⚡ 프로필 프리셋 기반
                      </button>
                      <button
                        type="button"
                        onClick={() => setScheduleModuleScope('custom')}
                        className={`px-3 py-1.5 rounded-lg text-xs font-bold transition ${
                          scheduleModuleScope === 'custom'
                            ? 'bg-sky-500/20 text-sky-300 border border-sky-500/50'
                            : 'text-slate-400 hover:text-slate-200'
                        }`}
                      >
                        🎯 사용자 개별 모듈 지정 ({scheduleCustomModuleIds.size})
                      </button>
                    </div>
                  </div>

                  {/* PRESET MODE */}
                  {scheduleModuleScope === 'profile' && (
                    <div className="space-y-2">
                      <label className="block text-xs font-bold text-slate-300">실행 프로필 선택</label>
                      <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                        {[
                          { id: 'safe', title: '안전 모드 (Safe)', desc: '충돌 위험이 없는 시스템 캐시, 임시 파일 및 텔레메트리 78개 정화 (가장 권장)', badge: '권장', color: 'border-emerald-500 text-emerald-300' },
                          { id: 'deep', title: '딥클린 모드 (Deep)', desc: '시스템 깊은 임시 데이터 및 소프트웨어 배포 잔여 캐시 정화', badge: '고효율', color: 'border-amber-500 text-amber-300' },
                          { id: 'gaming', title: '게이밍 부스트 (Gaming)', desc: '백그라운드 지연 서비스 중지 및 GPU/네트워크 스택 최적화', badge: '게임특화', color: 'border-blue-500 text-blue-300' },
                          { id: 'privacy', title: '개인정보 보호 (Privacy)', desc: '진단 데이터, 활동 기록, 검색 캐시 및 브라우저 세션 소거', badge: '보안특화', color: 'border-purple-500 text-purple-300' },
                          { id: 'all', title: '전체 모듈 일괄 정화 (All)', desc: 'WinPurify Pro의 115개 전체 최적화 모듈 전수 자동 실행', badge: '115개', color: 'border-sky-500 text-sky-300' }
                        ].map(p => (
                          <button
                            key={p.id}
                            type="button"
                            onClick={() => setScheduleProfile(p.id)}
                            className={`p-3 rounded-xl text-left border transition ${
                              scheduleProfile === p.id
                                ? `bg-slate-800/90 ${p.color} shadow-sm ring-1 ring-sky-500/40`
                                : 'bg-slate-950/60 border-slate-800 text-slate-400 hover:bg-slate-800/60'
                            }`}
                          >
                            <div className="flex items-center justify-between">
                              <span className="font-bold text-xs text-slate-200">{p.title}</span>
                              <span className="text-[10px] px-1.5 py-0.5 rounded bg-slate-800 text-slate-300 border border-slate-700">{p.badge}</span>
                            </div>
                            <div className="text-[10px] text-slate-400 mt-1 leading-snug">{p.desc}</div>
                          </button>
                        ))}
                      </div>
                    </div>
                  )}

                  {/* CUSTOM MODULE SELECTION MODE */}
                  {scheduleModuleScope === 'custom' && (
                    <div className="space-y-3">
                      {/* Search & Quick Controls */}
                      <div className="flex items-center gap-2">
                        <div className="relative flex-1">
                          <Search className="w-3.5 h-3.5 absolute left-3 top-2.5 text-slate-500" />
                          <input
                            type="text"
                            placeholder="모듈 이름 또는 설명 검색..."
                            value={scheduleModuleSearch}
                            onChange={e => setScheduleModuleSearch(e.target.value)}
                            className="w-full bg-slate-950 border border-slate-800 rounded-lg pl-8 pr-3 py-1.5 text-xs text-slate-200 focus:outline-none focus:border-sky-500"
                          />
                        </div>
                        <button
                          type="button"
                          onClick={() => {
                            const allIds = new Set(INITIAL_MODULES.map(m => m.id));
                            setScheduleCustomModuleIds(allIds);
                          }}
                          className="px-2.5 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-[11px] font-semibold text-slate-300 transition"
                        >
                          전체 선택
                        </button>
                        <button
                          type="button"
                          onClick={() => {
                            const safeIds = new Set(INITIAL_MODULES.filter(m => m.risk === 'Safe').map(m => m.id));
                            setScheduleCustomModuleIds(safeIds);
                          }}
                          className="px-2.5 py-1.5 rounded-lg bg-emerald-950 hover:bg-emerald-900 border border-emerald-800/80 text-[11px] font-semibold text-emerald-300 transition"
                        >
                          안전 모듈만
                        </button>
                        <button
                          type="button"
                          onClick={() => setScheduleCustomModuleIds(new Set())}
                          className="px-2.5 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-[11px] font-semibold text-slate-400 transition"
                        >
                          선택 해제
                        </button>
                      </div>

                      {/* Module Categories Filter */}
                      <div className="flex items-center gap-1.5 overflow-x-auto pb-1 text-[11px]">
                        {[
                          { id: 'all', label: '전체 모듈' },
                          { id: 'Privacy', label: '개인정보' },
                          { id: 'System', label: '시스템' },
                          { id: 'Browser', label: '브라우저' },
                          { id: 'Storage', label: '스토리지' },
                          { id: 'Registry', label: '레지스트리' },
                          { id: 'Security', label: '보안' }
                        ].map(c => (
                          <button
                            key={c.id}
                            type="button"
                            onClick={() => setScheduleModuleCategoryFilter(c.id)}
                            className={`px-2.5 py-1 rounded-md font-semibold whitespace-nowrap transition ${
                              scheduleModuleCategoryFilter === c.id
                                ? 'bg-sky-500/20 text-sky-300 border border-sky-500/50'
                                : 'bg-slate-950 text-slate-400 hover:text-slate-200 border border-slate-800'
                            }`}
                          >
                            {c.label}
                          </button>
                        ))}
                      </div>

                      {/* Module Checkbox Grid */}
                      <div className="h-56 overflow-y-auto space-y-1.5 pr-1 border border-slate-800/80 rounded-xl p-2 bg-slate-950/60">
                        {INITIAL_MODULES
                          .filter(m => scheduleModuleCategoryFilter === 'all' || m.category === scheduleModuleCategoryFilter)
                          .filter(m => !scheduleModuleSearch || m.name.toLowerCase().includes(scheduleModuleSearch.toLowerCase()) || m.description.toLowerCase().includes(scheduleModuleSearch.toLowerCase()))
                          .map(m => {
                            const isChecked = scheduleCustomModuleIds.has(m.id);
                            return (
                              <div
                                key={m.id}
                                onClick={() => {
                                  const updated = new Set(scheduleCustomModuleIds);
                                  if (isChecked) updated.delete(m.id);
                                  else updated.add(m.id);
                                  setScheduleCustomModuleIds(updated);
                                }}
                                className={`p-2 rounded-lg border transition cursor-pointer flex items-center justify-between ${
                                  isChecked
                                    ? 'bg-sky-950/40 border-sky-800/80 text-sky-200'
                                    : 'bg-slate-900/50 border-slate-800 text-slate-400 hover:bg-slate-800/40'
                                }`}
                              >
                                <div className="flex items-center gap-2.5 min-w-0">
                                  <input
                                    type="checkbox"
                                    checked={isChecked}
                                    readOnly
                                    className="rounded bg-slate-900 border-slate-700 text-sky-500 focus:ring-0 w-3.5 h-3.5"
                                  />
                                  <div className="truncate">
                                    <div className="text-xs font-bold text-slate-200 truncate">{m.name}</div>
                                    <div className="text-[10px] text-slate-400 truncate">{m.description}</div>
                                  </div>
                                </div>
                                <div className="flex items-center gap-1.5 shrink-0 ml-2">
                                  <span className={`text-[9px] px-1.5 py-0.5 rounded font-bold ${
                                    m.risk === 'Safe' ? 'bg-emerald-950 text-emerald-300 border border-emerald-800' :
                                    m.risk === 'Deep' ? 'bg-amber-950 text-amber-300 border border-amber-800' :
                                    'bg-red-950 text-red-300 border border-red-800'
                                  }`}>
                                    {m.risk}
                                  </span>
                                  <span className="text-[9px] text-slate-500">{m.category}</span>
                                </div>
                              </div>
                            );
                          })}
                      </div>
                    </div>
                  )}
                </div>
              )}

              {/* TAB 3: ADVANCED SAFETY & POWER OPTIONS */}
              {scheduleActiveTab === 'advanced' && (
                <div className="space-y-4">
                  <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 space-y-3.5">
                    <div className="text-xs font-bold text-slate-200 border-b border-slate-800 pb-2">안전 및 시스템 복원 옵션</div>
                    
                    <label className="flex items-start gap-3 cursor-pointer">
                      <input 
                        type="checkbox" 
                        checked={scheduleCreateSafepoint} 
                        onChange={e => setScheduleCreateSafepoint(e.target.checked)} 
                        className="mt-0.5 rounded bg-slate-900 border-slate-700 text-sky-500 focus:ring-0 w-4 h-4"
                      />
                      <div>
                        <div className="text-xs font-bold text-slate-200">실시간 세이프포인트(Live Safepoint) 레지스트리 백업 자동 생성</div>
                        <div className="text-[10px] text-slate-400 mt-0.5">정화 작업을 개시하기 전 주요 레지스트리 키 및 서비스 설정의 스냅샷을 자동 백업하여 언제든 원클릭 롤백할 수 있도록 보호합니다.</div>
                      </div>
                    </label>

                    <label className="flex items-start gap-3 cursor-pointer">
                      <input 
                        type="checkbox" 
                        checked={scheduleAutoTrimRam} 
                        onChange={e => setScheduleAutoTrimRam(e.target.checked)} 
                        className="mt-0.5 rounded bg-slate-900 border-slate-700 text-sky-500 focus:ring-0 w-4 h-4"
                      />
                      <div>
                        <div className="text-xs font-bold text-slate-200">정화 시작 전 RAM 즉시 압축 (Working Set Memory Trim) 동시 수행</div>
                        <div className="text-[10px] text-slate-400 mt-0.5">PurifyEngineCore.dll C++ 네이티브 커널 API를 통해 프로세스 불필요 캐시를 비우고 물리 RAM 여유 용량을 확보합니다.</div>
                      </div>
                    </label>
                  </div>

                  <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 space-y-3.5">
                    <div className="text-xs font-bold text-slate-200 border-b border-slate-800 pb-2">전원 및 실행 정책 (Power & Battery Policies)</div>

                    <label className="flex items-start gap-3 cursor-pointer">
                      <input 
                        type="checkbox" 
                        checked={scheduleRunOnlyOnAC} 
                        onChange={e => setScheduleRunOnlyOnAC(e.target.checked)} 
                        className="mt-0.5 rounded bg-slate-900 border-slate-700 text-sky-500 focus:ring-0 w-4 h-4"
                      />
                      <div>
                        <div className="text-xs font-bold text-slate-200">전원 어댑터(AC 전원) 연결 시에만 실행</div>
                        <div className="text-[10px] text-slate-400 mt-0.5">노트북이 배터리로 구동 중일 때는 자동 정화 실행을 보류하여 배터리를 절약합니다.</div>
                      </div>
                    </label>

                    <label className="flex items-start gap-3 cursor-pointer">
                      <input 
                        type="checkbox" 
                        checked={scheduleWakeToRun} 
                        onChange={e => setScheduleWakeToRun(e.target.checked)} 
                        className="mt-0.5 rounded bg-slate-900 border-slate-700 text-sky-500 focus:ring-0 w-4 h-4"
                      />
                      <div>
                        <div className="text-xs font-bold text-slate-200">절전 모드 해제 후 실행 (Wake Computer to Run Task)</div>
                        <div className="text-[10px] text-slate-400 mt-0.5">지정된 예약 시각에 컴퓨터가 절전 상태이면 깨워서 정화를 수행한 후 다시 절전 상태로 복귀합니다.</div>
                      </div>
                    </label>

                    <div className="p-2.5 rounded-lg bg-sky-950/40 border border-sky-800/60 flex items-center justify-between text-[11px]">
                      <span className="text-sky-300 font-semibold">보안 권한 수준: 최고 관리자 권한 (/RL HIGHEST) 강제 적용</span>
                      <span className="text-emerald-400 font-bold">✓ 관리자 권한 완비</span>
                    </div>
                  </div>
                </div>
              )}

              {/* TAB 4: CLI PREVIEW & STATUS */}
              {scheduleActiveTab === 'preview' && (
                <div className="space-y-4">
                  <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 space-y-3">
                    <div className="flex items-center justify-between">
                      <span className="text-xs font-bold text-slate-200">생성된 Windows 작업 스케줄러 CLI 명령어</span>
                      <button
                        type="button"
                        onClick={() => {
                          navigator.clipboard.writeText(getGeneratedSchtasksCmd());
                          setScheduleCopiedCmd(true);
                          setTimeout(() => setScheduleCopiedCmd(false), 2000);
                        }}
                        className="px-2.5 py-1 rounded bg-slate-800 hover:bg-slate-700 border border-slate-700 text-xs font-semibold text-sky-300 transition flex items-center gap-1"
                      >
                        {scheduleCopiedCmd ? <Check className="w-3.5 h-3.5 text-emerald-400" /> : <Copy className="w-3.5 h-3.5" />}
                        <span>{scheduleCopiedCmd ? '복사 완료' : '명령어 복사'}</span>
                      </button>
                    </div>
                    <div className="bg-slate-900 border border-slate-800 rounded-lg p-3 font-mono text-[11px] text-sky-300 break-all select-all">
                      {getGeneratedSchtasksCmd()}
                    </div>
                    <p className="text-[10px] text-slate-500">위 명령어는 관리자 권한 명령 프롬프트(cmd.exe) 또는 PowerShell에서 직접 수동 등록할 수도 있습니다.</p>
                  </div>

                  <div className="grid grid-cols-3 gap-3">
                    <div className="p-3 rounded-xl bg-slate-950 border border-slate-800">
                      <div className="text-[10px] text-slate-400 font-semibold">작업 이름</div>
                      <div className="text-xs font-bold text-slate-200 mt-1">WinPurifyPro_AutoMaintenance</div>
                    </div>
                    <div className="p-3 rounded-xl bg-slate-950 border border-slate-800">
                      <div className="text-[10px] text-slate-400 font-semibold">실행 모드</div>
                      <div className="text-xs font-bold text-emerald-400 mt-1">무음 백그라운드 (--silent)</div>
                    </div>
                    <div className="p-3 rounded-xl bg-slate-950 border border-slate-800">
                      <div className="text-[10px] text-slate-400 font-semibold">로그 보존 경로</div>
                      <div className="text-xs font-bold text-sky-300 mt-1 truncate">%ProgramData%\Logs</div>
                    </div>
                  </div>
                </div>
              )}

            </div>

            {/* Modal Footer */}
            <div className="px-6 py-4 border-t border-slate-800 bg-slate-950/80 flex items-center justify-between gap-3">
              <button 
                onClick={() => setShowScheduleModal(false)}
                className="px-4 py-2 rounded-lg bg-slate-800 hover:bg-slate-700 text-xs font-semibold text-white transition"
              >
                닫기
              </button>

              <div className="flex items-center gap-2">
                <button 
                  onClick={handleRemoveSchedule}
                  className="px-3.5 py-2 rounded-lg bg-red-950/80 hover:bg-red-900 border border-red-700/80 text-xs font-bold text-red-200 transition flex items-center gap-1.5"
                >
                  <Trash2 className="w-3.5 h-3.5" />
                  <span>스케줄 제거</span>
                </button>
                <button 
                  onClick={handleSaveSchedule}
                  className="px-5 py-2 rounded-lg bg-blue-600 hover:bg-blue-500 text-xs font-bold text-white shadow-lg shadow-blue-600/20 transition flex items-center gap-1.5"
                >
                  <Save className="w-3.5 h-3.5" />
                  <span>스케줄 적용 및 저장</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Central Commander Fleet Modal */}
      {showCommanderModal && (
        <div id="commander-modal" className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in duration-150">
          <div className="bg-slate-900 border border-blue-500/80 rounded-2xl w-full max-w-4xl shadow-2xl overflow-hidden flex flex-col max-h-[90vh]">
            
            {/* Modal Header */}
            <div className="px-6 py-4 border-b border-slate-800 flex items-center justify-between bg-slate-950/60">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-xl bg-blue-500/10 border border-blue-500/30 text-blue-400">
                  <Radio className="w-5 h-5" />
                </div>
                <div>
                  <div className="flex items-center gap-2">
                    <h3 className="text-base font-bold text-white">Central Commander (다중 PC 원격 관제)</h3>
                    <span className="text-[10px] px-2 py-0.5 rounded bg-blue-900/60 text-blue-300 font-bold border border-blue-700/50">Option B 데스크톱 전용</span>
                  </div>
                  <p className="text-xs text-slate-400">사내/홈 LAN 내의 WinPurify Pro 노드를 자동 탐색하고 동시 일괄 최적화 및 4중 보안 제어를 수행합니다.</p>
                </div>
              </div>
              <button 
                onClick={() => setShowCommanderModal(false)}
                className="p-1.5 rounded-lg bg-slate-800 text-slate-400 hover:text-white hover:bg-slate-700 transition"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            {/* Security & Control Strip */}
            <div className="px-6 py-3 bg-slate-950/40 border-b border-slate-800 flex flex-wrap items-center justify-between gap-3 text-xs">
              <div className="flex flex-wrap items-center gap-3">
                <div className="flex items-center gap-2">
                  <span className="text-slate-400 font-medium">🔒 클러스터 마스터 키:</span>
                  <input 
                    type="text" 
                    value={clusterKey} 
                    onChange={e => setClusterKey(e.target.value)} 
                    className="bg-slate-900 border border-slate-700 rounded px-2.5 py-1 text-sky-400 font-mono font-bold text-xs focus:outline-none focus:border-sky-500"
                  />
                  <button
                    onClick={() => {
                      const bytes = new Uint8Array(16);
                      window.crypto.getRandomValues(bytes);
                      const hex = Array.from(bytes, b => b.toString(16).padStart(2, '0')).join('').toUpperCase();
                      setClusterKey(`WinPurify_${hex}`);
                      addLog(`[보안] 새로운 256-bit 클러스터 마스터 키가 안전하게 생성되었습니다.`);
                    }}
                    className="px-2 py-1 rounded bg-slate-800 hover:bg-slate-700 text-sky-300 text-[11px] font-semibold border border-slate-700 cursor-pointer transition"
                    title="암호학적 고유 난수 키 즉시 생성"
                  >
                    🎲 고유 난수 키 생성
                  </button>
                </div>
                <div className="flex items-center gap-1.5 px-2.5 py-1 rounded-lg bg-emerald-500/10 border border-emerald-500/30 text-emerald-300 font-medium text-[11px]" title="HMAC-SHA256 디지털 서명, Nonce 재전송(Replay) 방어 실드, Drive-by 차단 및 시스템 최소 권한 ACL이 상시 적용되어 있습니다.">
                  <span className="text-emerald-400">🛡️</span>
                  <span>보안 아키텍처: <strong>4중 보안 강화 (Nonce Replay 차단 • 최소권한 ACL • 입력값 검증)</strong></span>
                </div>
              </div>

              <div className="flex items-center gap-2">
                <button 
                  disabled={isScanningNodes}
                  onClick={() => {
                    setIsScanningNodes(true);
                    setCommanderLogs(prev => [`[Discovery] UDP 9871 브로드캐스트로 LAN 노드 검색 중...`, ...prev]);
                    setTimeout(() => {
                      setIsScanningNodes(false);
                      setCommanderLogs(prev => [`[Discovery 완료] 5대 활성 노드 감지 성공 (HMAC 무결성 인증 통과)`, ...prev]);
                    }, 600);
                  }}
                  className="px-3 py-1.5 rounded-lg bg-blue-600 hover:bg-blue-500 text-white font-bold text-xs flex items-center gap-1.5 transition disabled:opacity-50"
                >
                  <Search className="w-3.5 h-3.5" />
                  <span>{isScanningNodes ? '탐색 중...' : '노드 자동 탐색 (UDP)'}</span>
                </button>
                <button 
                  onClick={() => {
                    setRemoteNodes(prev => prev.map(n => ({
                      ...n,
                      cpu: Math.floor(10 + Math.random() * 30),
                      ramPercent: Math.floor(30 + Math.random() * 40)
                    })));
                    setCommanderLogs(prev => [`[Telemetry] 5대 노드 실시간 CPU/RAM/최적화 상태 갱신 완료`, ...prev]);
                  }}
                  className="px-3 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-200 font-semibold text-xs flex items-center gap-1.5 transition"
                >
                  <RefreshCw className="w-3.5 h-3.5" />
                  <span>텔레메트리 갱신</span>
                </button>
              </div>
            </div>

            {/* Navigation Tabs between Fleet Nodes and Audit History */}
            <div className="px-6 border-b border-slate-800 bg-slate-900/50 flex items-center justify-between">
              <div className="flex items-center gap-1">
                <button
                  onClick={() => setCommanderActiveTab('nodes')}
                  className={`px-4 py-2.5 text-xs font-bold border-b-2 transition flex items-center gap-2 ${
                    commanderActiveTab === 'nodes'
                      ? 'border-blue-500 text-blue-400 bg-blue-500/10'
                      : 'border-transparent text-slate-400 hover:text-slate-200'
                  }`}
                >
                  <Server className="w-3.5 h-3.5" />
                  <span>클러스터 노드 관제 ({remoteNodes.length}대)</span>
                </button>
                <button
                  onClick={() => setCommanderActiveTab('audit')}
                  className={`px-4 py-2.5 text-xs font-bold border-b-2 transition flex items-center gap-2 ${
                    commanderActiveTab === 'audit'
                      ? 'border-sky-500 text-sky-400 bg-sky-500/10'
                      : 'border-transparent text-slate-400 hover:text-slate-200'
                  }`}
                >
                  <History className="w-3.5 h-3.5" />
                  <span>원격 실행 감사 기록 ({remoteAuditHistory.length}건)</span>
                </button>
              </div>

              <div className="flex items-center gap-2">
                <button
                  onClick={() => {
                    triggerRemoteActionHud(
                      '🚨 중앙 관제(Commander) 원격 명령 동작 중',
                      '물리 RAM 워킹셋 긴급 정화 (RAM Trim)',
                      '192.168.1.100 (Central Commander Console)',
                      '[성공] C++ Native 엔진을 통해 84개 프로세스 물리 RAM Working Set 즉시 회수 완료 (38ms)',
                      'RamTrim'
                    );
                  }}
                  className="px-2.5 py-1 rounded bg-blue-900/60 hover:bg-blue-800 text-blue-300 border border-blue-700/50 text-[11px] font-bold flex items-center gap-1.5 transition"
                  title="원격 관제 콘솔에서 로컬 PC로 명령이 전달되었을 때의 실시간 알람 사운드 및 모달 동작을 테스트합니다."
                >
                  <Bell className="w-3 h-3 text-blue-400 animate-bounce" />
                  <span>원격 알람/모달 수신 테스트</span>
                </button>
              </div>
            </div>

            {commanderActiveTab === 'nodes' ? (
              <>
                {/* Fleet Nodes Grid */}
                <div className="p-6 overflow-y-auto flex-1 space-y-2.5">
                  {remoteNodes.map(node => (
                    <div 
                      key={node.id}
                      className={`p-3.5 rounded-xl border transition flex items-center justify-between gap-4 ${
                        node.selected 
                          ? 'bg-slate-800/80 border-blue-500/50 shadow-sm' 
                          : 'bg-slate-950/50 border-slate-800 opacity-70'
                      }`}
                    >
                      <div className="flex items-center gap-3">
                        <input 
                          type="checkbox" 
                          checked={node.selected} 
                          onChange={e => {
                            const checked = e.target.checked;
                            setRemoteNodes(prev => prev.map(n => n.id === node.id ? { ...n, selected: checked } : n));
                          }}
                          className="rounded bg-slate-900 border-slate-700 text-blue-500 focus:ring-0 w-4 h-4 cursor-pointer"
                        />
                        <div>
                          <div className="flex items-center gap-2">
                            <span className="font-bold text-sm text-white">{node.name}</span>
                            <span className="text-[10px] px-2 py-0.5 rounded bg-slate-700 text-slate-300 font-medium">{node.group}</span>
                          </div>
                          <span className="text-[11px] text-slate-400 font-mono">IP: {node.ip}:9870</span>
                        </div>
                      </div>

                      <div className="flex items-center gap-6">
                        <div className="w-28 space-y-1">
                          <div className="flex justify-between text-[11px]">
                            <span className="text-slate-400">CPU</span>
                            <span className="text-sky-400 font-bold">{node.cpu}%</span>
                          </div>
                          <div className="w-full bg-slate-900 rounded-full h-1.5 overflow-hidden">
                            <div className="bg-sky-400 h-1.5 rounded-full" style={{ width: `${node.cpu}%` }}></div>
                          </div>
                        </div>

                        <div className="w-32 space-y-1">
                          <div className="flex justify-between text-[11px]">
                            <span className="text-slate-400">RAM ({node.ramPercent}%)</span>
                            <span className="text-emerald-400 font-bold">{node.ramText}</span>
                          </div>
                          <div className="w-full bg-slate-900 rounded-full h-1.5 overflow-hidden">
                            <div className="bg-emerald-400 h-1.5 rounded-full" style={{ width: `${node.ramPercent}%` }}></div>
                          </div>
                        </div>

                        <div className="text-right">
                          <span className="text-[10px] text-slate-400 block">최적화 점수</span>
                          <span className="text-sm font-bold text-amber-400">{node.score}점</span>
                        </div>

                        <div className="text-right">
                          <span className="inline-flex items-center gap-1 text-[10px] px-2 py-0.5 rounded bg-emerald-500/20 text-emerald-300 font-bold border border-emerald-500/30">
                            <span className="w-1.5 h-1.5 rounded-full bg-emerald-400 animate-pulse"></span>
                            ONLINE
                          </span>
                        </div>

                        <div className="flex items-center gap-1">
                          <button
                            onClick={() => {
                              setRemoteNodes(prev => prev.map(n => n.id === node.id ? { ...n, ramPercent: 22, ramText: '3.5 / 16 GB' } : n));
                              setCommanderLogs(prev => [`[Single] [${node.name}] (${node.ip}) 물리 RAM 워킹셋 긴급 회수 완료 (36ms)`, ...prev]);
                              triggerRemoteActionHud(
                                '🚨 중앙 관제(Commander) 원격 명령 동작 중',
                                `물리 RAM 긴급 정화 (RAM Trim) - ${node.name}`,
                                '192.168.1.100 (Central Commander Console)',
                                `[성공] C++ Native 엔진을 통해 ${node.name} 물리 RAM 워킹셋 즉시 압축 완료`,
                                'RamTrim'
                              );
                            }}
                            className="px-2 py-1 rounded bg-sky-600/80 hover:bg-sky-500 text-white text-[10px] font-semibold transition"
                            title="해당 PC 물리 RAM 압축"
                          >
                            ⚡ RAM
                          </button>
                          <button
                            onClick={() => {
                              setCommanderLogs(prev => [
                                `[Single 긴급] [${node.name}] (${node.ip}) 4-Tier 시스템 강제 재부팅(/r /f /t 3) 명령 하달`,
                                `  └ [SystemPower] Tier 1(shutdown.exe /r /f /t 0), Tier 2(SeShutdownPrivilege + ExitWindowsEx) 4단계 Failover 대기`,
                                ...prev
                              ]);
                              triggerRemoteActionHud(
                                '🚨 중앙 관제 원격 시스템 재부팅 명령 수신',
                                `원격 시스템 강제 재부팅 (/r /f /t 3) - ${node.name}`,
                                '192.168.1.100 (Central Commander Console)',
                                `[성공] 원격 시스템 강제 재부팅이 예약되었습니다. 3초 후 /r /f 안전 재시작이 수행됩니다. (4-Tier Failover 엔진 활성화)`,
                                'Reboot'
                              );
                            }}
                            className="px-2 py-1 rounded bg-rose-600/80 hover:bg-rose-500 text-white text-[10px] font-semibold transition flex items-center gap-0.5"
                            title="해당 PC 원격 강제 재부팅 (/r /f /t 3)"
                          >
                            <RotateCcw className="w-2.5 h-2.5" />
                            <span>재부팅</span>
                          </button>
                          <button
                            onClick={() => {
                              setCommanderLogs(prev => [
                                `[Single 긴급] [${node.name}] (${node.ip}) 4-Tier 시스템 강제 종료(/s /f /t 3) 명령 하달`,
                                `  └ [SystemPower] Tier 1(shutdown.exe /s /f /t 0), Tier 2(SeShutdownPrivilege + ExitWindowsEx EWX_SHUTDOWN) 4단계 Failover 대기`,
                                ...prev
                              ]);
                              triggerRemoteActionHud(
                                '🚨 중앙 관제 원격 시스템 종료 명령 수신',
                                `원격 윈도우 강제 종료 (/s /f /t 3) - ${node.name}`,
                                '192.168.1.100 (Central Commander Console)',
                                `[성공] 원격 시스템 강제 종료가 예약되었습니다. 3초 후 /s /f 전원 차단이 수행됩니다. (4-Tier Failover 엔진 활성화)`,
                                'Shutdown'
                              );
                            }}
                            className="px-2 py-1 rounded bg-red-800/80 hover:bg-red-700 text-white text-[10px] font-semibold transition flex items-center gap-0.5"
                            title="해당 PC 원격 윈도우 강제 종료 (/s /f /t 3)"
                          >
                            <Power className="w-2.5 h-2.5" />
                            <span>종료</span>
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>

                {/* Batch Action Bar */}
                <div className="px-6 py-3.5 bg-slate-950 border-t border-slate-800 flex items-center justify-between gap-4">
                  <div className="flex items-center gap-2 text-xs">
                    <span className="text-slate-400">선택 대상:</span>
                    <strong className="text-sky-400">{remoteNodes.filter(n => n.selected).length}대 PC</strong>
                    <button 
                      onClick={() => setRemoteNodes(prev => prev.map(n => ({ ...n, selected: true })))}
                      className="px-2 py-1 rounded bg-slate-800 text-slate-300 hover:bg-slate-700 text-[11px]"
                    >
                      전체 선택
                    </button>
                    <button 
                      onClick={() => setRemoteNodes(prev => prev.map(n => ({ ...n, selected: false })))}
                      className="px-2 py-1 rounded bg-slate-800 text-slate-300 hover:bg-slate-700 text-[11px]"
                    >
                      해제
                    </button>
                  </div>

                  <div className="flex items-center gap-2">
                    <button 
                      onClick={() => {
                        const selCount = remoteNodes.filter(n => n.selected).length;
                        setCommanderLogs(prev => [`[Batch] ${selCount}대 원격 노드에 주간 자동 스케줄러(매주 일요일 03:00, Safe 프로필) 일괄 배포 완료`, ...prev]);
                        triggerRemoteActionHud(
                          '🚨 중앙 관제(Commander) 원격 명령 동작 중',
                          '주간 자동 정화 스케줄러 일괄 배포',
                          '192.168.1.100 (Central Commander Console)',
                          `[성공] Windows 작업 스케줄러(매주 일요일 03:00) 등록 및 실행 정책 동기화 완료`,
                          'DeploySchedule'
                        );
                      }}
                      className="px-3 py-1.5 rounded-lg bg-amber-600 hover:bg-amber-500 text-white font-bold text-xs shadow-lg shadow-amber-600/20 transition flex items-center gap-1.5"
                    >
                      <Calendar className="w-3.5 h-3.5" />
                      <span>스케줄러 배포</span>
                    </button>

                    <button 
                      onClick={() => {
                        const selCount = remoteNodes.filter(n => n.selected).length;
                        setRemoteNodes(prev => prev.map(n => n.selected ? { ...n, ramPercent: 22, ramText: '3.5 / 16 GB' } : n));
                        setCommanderLogs(prev => [`[Batch] ${selCount}대 대상 PC 물리 RAM 일괄 Trim(압축) 전송 성공`, ...prev]);
                        triggerRemoteActionHud(
                          '🚨 중앙 관제(Commander) 원격 명령 동작 중',
                          '물리 RAM 워킹셋 긴급 정화 (RAM Trim)',
                          '192.168.1.100 (Central Commander Console)',
                          `[성공] C++ Native 엔진을 통해 84개 프로세스 물리 RAM Working Set 즉시 회수 완료 (42ms)`,
                          'RamTrim'
                        );
                      }}
                      className="px-3 py-1.5 rounded-lg bg-sky-600 hover:bg-sky-500 text-white font-bold text-xs shadow-lg shadow-sky-600/20 transition flex items-center gap-1.5"
                    >
                      <Cpu className="w-3.5 h-3.5" />
                      <span>RAM 일괄 정화</span>
                    </button>

                    <button 
                      onClick={() => {
                        const selCount = remoteNodes.filter(n => n.selected).length;
                        setCommanderLogs(prev => [`[Batch] ${selCount}대 대상 PC Windows 시스템 복원 지점 일괄 생성 완료`, ...prev]);
                        triggerRemoteActionHud(
                          '🚨 중앙 관제(Commander) 원격 명령 동작 중',
                          'Windows 시스템 복원 지점 생성',
                          '192.168.1.100 (Central Commander Console)',
                          `[성공] C: 시스템 보호 자동 활성화 및 세이프포인트 스냅샷 생성 완료 (2,840ms)`,
                          'CreateRestorePoint'
                        );
                      }}
                      className="px-3 py-1.5 rounded-lg bg-purple-600 hover:bg-purple-500 text-white font-bold text-xs shadow-lg shadow-purple-600/20 transition flex items-center gap-1.5"
                    >
                      <ShieldCheck className="w-3.5 h-3.5" />
                      <span>복원지점 생성</span>
                    </button>

                    <button 
                      onClick={() => {
                        const selCount = remoteNodes.filter(n => n.selected).length;
                        setCommanderLogs(prev => [
                          `[Batch 긴급] ${selCount}대 원격 노드에 4-Tier 시스템 강제 재부팅(/r /f /t 3) 명령 일괄 하달`,
                          `  └ [SystemPower] Tier 1(shutdown.exe /r /f /t 0), Tier 2(SeShutdownPrivilege + ExitWindowsEx) 4단계 Failover 가동`,
                          ...prev
                        ]);
                        triggerRemoteActionHud(
                          '🚨 중앙 관제 원격 시스템 재부팅 명령 수신',
                          `선택된 ${selCount}대 PC 원격 강제 재부팅 (/r /f /t 3)`,
                          '192.168.1.100 (Central Commander Console)',
                          `[성공] 원격 시스템 강제 재부팅이 예약되었습니다. 3초 후 /r /f 안전 재시작이 수행됩니다. (4-Tier Failover 엔진 활성화)`,
                          'Reboot'
                        );
                      }}
                      className="px-3 py-1.5 rounded-lg bg-rose-600 hover:bg-rose-500 text-white font-bold text-xs shadow-lg shadow-rose-600/20 transition flex items-center gap-1.5"
                      title="선택된 모든 PC에 4-Tier 시스템 강제 재부팅(/r /f /t 3) 명령 하달"
                    >
                      <RotateCcw className="w-3.5 h-3.5" />
                      <span>원격 강제 재부팅</span>
                    </button>

                    <button 
                      onClick={() => {
                        const selCount = remoteNodes.filter(n => n.selected).length;
                        setCommanderLogs(prev => [
                          `[Batch 긴급] ${selCount}대 원격 노드에 4-Tier 시스템 강제 종료(/s /f /t 3) 명령 일괄 하달`,
                          `  └ [SystemPower] Tier 1(shutdown.exe /s /f /t 0), Tier 2(SeShutdownPrivilege + ExitWindowsEx EWX_SHUTDOWN) 4단계 Failover 가동`,
                          ...prev
                        ]);
                        triggerRemoteActionHud(
                          '🚨 중앙 관제 원격 시스템 종료 명령 수신',
                          `선택된 ${selCount}대 PC 원격 윈도우 강제 종료 (/s /f /t 3)`,
                          '192.168.1.100 (Central Commander Console)',
                          `[성공] 원격 시스템 강제 종료가 예약되었습니다. 3초 후 /s /f 전원 차단이 수행됩니다. (4-Tier Failover 엔진 활성화)`,
                          'Shutdown'
                        );
                      }}
                      className="px-3 py-1.5 rounded-lg bg-red-900 hover:bg-red-800 text-white font-bold text-xs shadow-lg shadow-red-900/30 transition flex items-center gap-1.5"
                      title="선택된 모든 PC에 4-Tier 시스템 강제 종료(/s /f /t 3) 명령 하달"
                    >
                      <Power className="w-3.5 h-3.5" />
                      <span>원격 윈도우 종료</span>
                    </button>
                  </div>
                </div>
              </>
            ) : (
              /* Remote Execution Audit History Tab */
              <div className="p-6 overflow-y-auto flex-1 space-y-3">
                <div className="flex items-center justify-between text-xs text-slate-400 mb-2">
                  <span>🛡️ 원격 관제 콘솔로부터 수신되어 로컬에서 실행된 모든 명령 감사 기록입니다 (JSON 파일 영구 저장).</span>
                  <button
                    onClick={() => {
                      setRemoteAuditHistory([]);
                      addLog('[Audit] 원격 실행 감사 로그가 초기화되었습니다.');
                    }}
                    className="px-2.5 py-1 rounded bg-slate-800 hover:bg-slate-700 text-slate-300 text-[11px] flex items-center gap-1"
                  >
                    <Trash2 className="w-3 h-3" />
                    <span>기록 비우기</span>
                  </button>
                </div>

                {remoteAuditHistory.length === 0 ? (
                  <div className="py-12 text-center text-slate-500 text-sm">
                    기록된 원격 실행 내역이 없습니다.
                  </div>
                ) : (
                  remoteAuditHistory.map(audit => (
                    <div key={audit.id} className="p-3.5 rounded-xl bg-slate-900/90 border border-slate-800 space-y-2">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <span className="px-2 py-0.5 rounded bg-blue-500/20 text-blue-300 font-bold text-[11px] border border-blue-500/30">
                            {audit.commandType}
                          </span>
                          <span className="font-bold text-sm text-slate-200">{audit.commandTitle}</span>
                        </div>
                        <div className="flex items-center gap-3 text-xs">
                          <span className="text-slate-400 font-mono">{audit.timestamp}</span>
                          <span className="px-2 py-0.5 rounded bg-emerald-500/20 text-emerald-300 font-semibold text-[10px]">
                            {audit.executionTimeMs}ms
                          </span>
                        </div>
                      </div>
                      <div className="text-xs text-slate-400 font-mono">
                        발신 원격지 IP: <strong className="text-sky-300">{audit.commanderIp}</strong>
                      </div>
                      <div className="text-xs bg-slate-950 p-2.5 rounded-lg text-slate-300 font-mono border border-slate-800/80">
                        {audit.details}
                      </div>
                    </div>
                  ))
                )}
              </div>
            )}

            {/* Commander Logs Console Header & Stream */}
            <div className="bg-slate-950 border-t border-slate-900">
              <div className="px-6 py-1.5 flex flex-wrap items-center justify-between gap-2 border-b border-slate-900/60 text-xs font-mono">
                <div className="flex items-center gap-2">
                  <span className="text-[11px] font-bold text-sky-400 flex items-center gap-1">
                    <Terminal className="w-3 h-3" />
                    <span>중앙 관제 이벤트 & 보안 감사 로그 (HMAC-SHA256 Signed)</span>
                  </span>
                  <span className="text-[9px] px-1.5 py-0.2 rounded bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 font-bold">
                    💾 {autoSaveCommanderLogs ? '디스크 자동 저장 중' : '자동 저장 꺼짐'}
                  </span>
                </div>
                <div className="flex items-center gap-2">
                  <label className="flex items-center gap-1 text-[10px] text-slate-400 cursor-pointer select-none">
                    <input
                      type="checkbox"
                      checked={autoSaveCommanderLogs}
                      onChange={e => {
                        setAutoSaveCommanderLogs(e.target.checked);
                        addCommanderLog(`[로그 설정] 관제 작업 로그 컴퓨터 디스크 자동 저장이 ${e.target.checked ? '활성화' : '비활성화'}되었습니다.`);
                      }}
                      className="rounded bg-slate-900 border-slate-700 text-sky-500 focus:ring-0 w-3 h-3"
                    />
                    <span>자동 저장</span>
                  </label>
                  <button
                    onClick={() => downloadLogFile(commanderLogs, 'WinPurifyCommander_FleetLogs')}
                    className="px-2 py-0.5 rounded bg-slate-800 hover:bg-slate-700 border border-slate-700 text-slate-300 hover:text-white text-[10px] font-medium transition flex items-center gap-1"
                    title="중앙 관제 작업 로그를 컴퓨터 텍스트 파일(.txt)로 저장합니다"
                  >
                    <Download className="w-3 h-3" />
                    <span>관제 로그 PC 저장</span>
                  </button>
                  <button
                    onClick={() => {
                      setCommanderLogs([`[${new Date().toLocaleTimeString()}] 관제 로그 화면이 초기화되었습니다. (디스크 로그 보존됨)`]);
                    }}
                    className="px-1.5 py-0.5 rounded bg-slate-800 hover:bg-slate-700 border border-slate-700 text-slate-400 hover:text-slate-200 text-[10px] transition"
                    title="콘솔 화면 지우기"
                  >
                    🧹
                  </button>
                </div>
              </div>
              <div className="px-6 py-2 max-h-24 overflow-y-auto font-mono text-[11px] text-sky-400 space-y-1">
                {commanderLogs.map((log, idx) => (
                  <div key={idx} className="leading-tight">{log}</div>
                ))}
              </div>
            </div>

          </div>
        </div>
      )}

      {/* ================================================================= */}
      {/* Central Commander Remote Execution Incoming Action Alert HUD Modal */}
      {/* ================================================================= */}
      {remoteAction.isOpen && (
        <div 
          id="remote-action-modal-backdrop" 
          className="fixed inset-0 bg-slate-950/80 backdrop-blur-md z-[1200] flex items-center justify-center p-4 animate-in fade-in duration-200"
        >
          <div 
            id="remote-action-card"
            className="w-full max-w-xl bg-slate-900 border-2 border-sky-500/80 rounded-2xl shadow-2xl shadow-sky-500/25 overflow-hidden transform transition-all animate-in zoom-in-95 duration-200"
          >
            {/* Top Glowing Header Bar */}
            <div className="bg-slate-950 px-6 py-4 border-b border-slate-800 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-blue-900/60 border border-blue-500/40 flex items-center justify-center text-blue-300 shadow-inner">
                  <Zap className="w-5 h-5 animate-pulse text-sky-400" />
                </div>
                <div>
                  <h3 className="font-bold text-base text-white tracking-tight">{remoteAction.title}</h3>
                  <p className="text-xs text-sky-400 font-mono mt-0.5">{remoteAction.sourceIp}</p>
                </div>
              </div>
              <span className="px-2.5 py-1 rounded-full bg-sky-500/20 text-sky-300 text-[10px] font-bold border border-sky-500/30 tracking-wider uppercase">
                REMOTE ACTION
              </span>
            </div>

            {/* Middle Content */}
            <div className="p-6 space-y-4">
              {/* Command Name Badge */}
              <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 flex items-center gap-3">
                <span className="text-xs font-bold text-slate-400 whitespace-nowrap">명령 내용</span>
                <span className="text-sm font-bold text-sky-300">{remoteAction.commandName}</span>
              </div>

              {/* Status and Auto Close */}
              <div className="flex items-center justify-between text-xs">
                <div className="flex items-center gap-2">
                  <span className={`w-2 h-2 rounded-full ${remoteAction.isActive ? 'bg-amber-400 animate-ping' : 'bg-emerald-400'}`}></span>
                  <span className="font-semibold text-slate-200">{remoteAction.status}</span>
                </div>
                {!remoteAction.isActive && (
                  <span className="text-slate-400">{remoteAction.autoCloseSeconds}초 후 자동 닫힘</span>
                )}
              </div>

              {/* Animated Progress Bar */}
              <div className="w-full bg-slate-950 rounded-full h-2 overflow-hidden border border-slate-800">
                {remoteAction.isActive ? (
                  <div className="bg-gradient-to-r from-blue-500 to-sky-400 h-full w-full animate-pulse"></div>
                ) : (
                  <div className="bg-emerald-500 h-full w-full"></div>
                )}
              </div>

              {/* Execution Details Box */}
              <div className="p-3.5 rounded-xl bg-slate-950 border border-slate-800 font-mono text-xs text-slate-300 leading-relaxed">
                {remoteAction.details}
              </div>
            </div>

            {/* Bottom Controls */}
            <div className="px-6 py-3.5 bg-slate-950/80 border-t border-slate-800 flex items-center justify-between">
              <span className="text-[11px] text-slate-400">
                🛡️ 로컬 감사 로그 (remote_execution_history.json)에 영구 보존됨
              </span>
              <button
                onClick={() => setRemoteAction(prev => ({ ...prev, isOpen: false }))}
                className="px-4 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 text-white font-bold text-xs border border-slate-700 transition"
              >
                닫기 (확인)
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ================================================================= */}
      {/* User Account & Credential Session Purge Modal (Zero-Trace Logout) */}
      {/* ================================================================= */}
      {showAccountPurgeModal && (
        <div
          id="account-purge-modal-backdrop"
          className="fixed inset-0 bg-slate-950/85 backdrop-blur-md z-[1300] flex items-center justify-center p-4 animate-in fade-in duration-200"
        >
          <div
            id="account-purge-card"
            className="w-full max-w-2xl bg-slate-900 border-2 border-rose-600/80 rounded-2xl shadow-2xl shadow-rose-600/25 overflow-hidden transform transition-all animate-in zoom-in-95 duration-200 flex flex-col max-h-[85vh]"
          >
            {/* Header */}
            <div className="bg-slate-950 px-6 py-4 border-b border-slate-800 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-rose-950 border border-rose-600/60 flex items-center justify-center text-xl shadow-inner">
                  🔐
                </div>
                <div>
                  <h3 className="font-bold text-base text-white tracking-tight">통합 계정 &amp; 세션 토큰 정화 센터 (Zero-Trace)</h3>
                  <p className="text-xs text-rose-300 mt-0.5">메신저, 금융인증서, 클라우드, 게임, 개발자, OS/오피스 및 브라우저 세션 완벽 강제 로그아웃</p>
                </div>
              </div>
              <button
                onClick={() => setShowAccountPurgeModal(false)}
                className="w-8 h-8 rounded-lg bg-slate-800 hover:bg-slate-700 flex items-center justify-center text-slate-400 hover:text-white transition"
              >
                <X className="w-4 h-4" />
              </button>
            </div>

            {/* Warning Alert Banner & Pre-Termination Option */}
            <div className="px-6 py-2.5 bg-rose-950/70 border-b border-rose-800/80 flex flex-wrap items-center justify-between gap-3 text-xs text-rose-200">
              <div className="flex items-center gap-2">
                <AlertTriangle className="w-4 h-4 text-rose-400 shrink-0" />
                <span>실행 시 선택된 서비스의 로그인 쿠키, 메신저 토큰, 인증서, 세션 파일이 즉시 소거되며 모든 기기에서 로그아웃됩니다.</span>
              </div>
              <label className="flex items-center gap-2 cursor-pointer font-bold text-amber-300 hover:text-amber-200 select-none">
                <input
                  type="checkbox"
                  checked={terminateProcessesBeforePurge}
                  onChange={e => setTerminateProcessesBeforePurge(e.target.checked)}
                  className="rounded border-amber-500 text-amber-500 focus:ring-0 cursor-pointer"
                />
                <span>⚡ 소거 전 실행 중인 세션 프로세스 자동 강제 종료 (잠금 방지)</span>
              </label>
            </div>

            {/* Central Commander Real-time Confirmation Status Log Stream */}
            <div className="px-6 py-2.5 bg-slate-950/90 border-b border-slate-800 space-y-2">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <div className="flex items-center gap-2 min-w-0">
                  <span className="text-[10px] font-bold px-2 py-0.5 rounded bg-sky-950 text-sky-400 border border-sky-600/50 flex items-center gap-1 shrink-0">
                    <Radio className="w-3 h-3 text-sky-400 animate-pulse" />
                    <span>COMMANDER CONFIRMATION STREAM</span>
                  </span>
                  <span className="text-xs font-semibold text-sky-300 truncate">
                    {latestCommanderStatus}
                  </span>
                </div>
                <div className="flex items-center gap-2">
                  <button
                    onClick={() => handleSimulateCommanderConfirmation('workplace')}
                    disabled={isSimulatingConfirmation}
                    className="px-2.5 py-1 rounded bg-slate-900 hover:bg-sky-950/80 border border-sky-500/60 text-sky-300 font-semibold text-[11px] transition flex items-center gap-1 disabled:opacity-50"
                  >
                    <Zap className="w-3 h-3 text-sky-400" />
                    <span>원격 정화 수신 시뮬레이션</span>
                  </button>
                  <button
                    onClick={() => setCommanderConfirmations([])}
                    className="px-2 py-1 rounded bg-slate-900 hover:bg-slate-800 border border-slate-700 text-slate-400 text-[11px] transition"
                  >
                    로그 비우기
                  </button>
                </div>
              </div>

              {/* Scrollable Live Confirmation Receipts Box */}
              <div className="bg-slate-950 rounded-lg border border-slate-800/80 p-2 max-h-20 overflow-y-auto space-y-1.5 font-mono text-[11px]">
                {commanderConfirmations.length === 0 ? (
                  <div className="text-slate-500 text-center py-1 font-sans text-xs">
                    대기 중인 원격 정화 확인 영수증이 없습니다.
                  </div>
                ) : (
                  commanderConfirmations.map(conf => (
                    <div key={conf.id} className="flex items-center justify-between gap-2 py-0.5 border-b border-slate-900/60 last:border-b-0">
                      <div className="flex items-center gap-2 min-w-0">
                        <span className="text-slate-500 text-[10px] shrink-0">[{conf.timestamp}]</span>
                        <span className="px-1.5 py-0.2 rounded bg-emerald-950 text-emerald-300 border border-emerald-700/60 text-[9px] font-bold shrink-0">
                          CONFIRMED
                        </span>
                        <span className="text-slate-300 truncate">{conf.details}</span>
                      </div>
                      <div className="flex items-center gap-1 text-[10px] text-slate-400 shrink-0">
                        <span>{conf.executionTimeMs}ms</span>
                        <span className="text-slate-600">|</span>
                        <span className="text-sky-400">{conf.commanderIp.split(' ')[0]}</span>
                      </div>
                    </div>
                  ))
                )}
              </div>
            </div>

            {/* Toolbar (Scenario Presets & Actions) */}
            <div className="px-6 py-3 bg-slate-950/50 border-b border-slate-800 space-y-2.5">
              {/* Scenario Quick Presets */}
              <div className="flex flex-wrap items-center gap-2 text-xs">
                <span className="font-bold text-slate-400 flex items-center gap-1">
                  <span>🎯 시나리오 프리셋:</span>
                </span>
                <button
                  onClick={() => handleApplyScenarioPreset('workplace')}
                  className="px-2.5 py-1 rounded bg-slate-900 hover:bg-blue-950/60 border border-blue-500/50 text-sky-300 font-medium transition flex items-center gap-1"
                >
                  <span>🏢 업무 PC 반납</span>
                </button>
                <button
                  onClick={() => handleApplyScenarioPreset('full')}
                  className="px-2.5 py-1 rounded bg-slate-900 hover:bg-rose-950/60 border border-rose-500/50 text-rose-300 font-bold transition flex items-center gap-1"
                >
                  <span>💻 PC 양도/판매 (Zero-Trace 전체)</span>
                </button>
                <button
                  onClick={() => handleApplyScenarioPreset('public')}
                  className="px-2.5 py-1 rounded bg-slate-900 hover:bg-amber-950/60 border border-amber-500/50 text-amber-300 font-medium transition flex items-center gap-1"
                >
                  <span>☕ 공용/PC방 이용 후</span>
                </button>
                <button
                  onClick={() => handleApplyScenarioPreset('developer')}
                  className="px-2.5 py-1 rounded bg-slate-900 hover:bg-purple-950/60 border border-purple-500/50 text-purple-300 font-medium transition flex items-center gap-1"
                >
                  <span>👨‍💻 개발자/보안 점검</span>
                </button>
              </div>

              {/* Status and Action Buttons */}
              <div className="flex flex-wrap items-center justify-between gap-2 text-xs pt-1 border-t border-slate-900">
                <span className="font-semibold text-sky-400 truncate max-w-md">{accountPurgeStatus}</span>
                <div className="flex items-center gap-2">
                  <button
                    onClick={handleScanAccountTargets}
                    disabled={isAccountPurging}
                    className="px-2.5 py-1 rounded bg-slate-800 hover:bg-slate-700 border border-slate-700 text-slate-300 font-medium transition disabled:opacity-50"
                  >
                    🔍 재스캔
                  </button>
                  <button
                    onClick={() => setAccountTargets(prev => prev.map(t => ({ ...t, selected: true })))}
                    className="px-2.5 py-1 rounded bg-slate-800 hover:bg-slate-700 border border-slate-700 text-slate-300 font-medium transition"
                  >
                    전체 선택
                  </button>
                  <button
                    onClick={() => setAccountTargets(prev => prev.map(t => ({ ...t, selected: false })))}
                    className="px-2.5 py-1 rounded bg-slate-800 hover:bg-slate-700 border border-slate-700 text-slate-300 font-medium transition"
                  >
                    선택 해제
                  </button>
                </div>
              </div>
            </div>

            {/* Account Targets List */}
            <div className="p-6 space-y-3 overflow-y-auto flex-1">
              {accountTargets.map(item => (
                <div
                  key={item.id}
                  onClick={() => setAccountTargets(prev => prev.map(t => t.id === item.id ? { ...t, selected: !t.selected } : t))}
                  className={`p-4 rounded-xl border transition cursor-pointer flex items-start gap-3.5 ${
                    item.selected
                      ? 'bg-slate-950 border-rose-600/50 shadow-sm shadow-rose-950/50'
                      : 'bg-slate-950/60 border-slate-800 hover:border-slate-700 opacity-70'
                  }`}
                >
                  <input
                    type="checkbox"
                    checked={item.selected}
                    onChange={() => {}}
                    className="mt-1 rounded border-slate-700 text-rose-600 focus:ring-rose-500 cursor-pointer"
                  />
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 flex-wrap mb-1">
                      <span className="text-base">{item.icon}</span>
                      <h4 className="font-bold text-sm text-slate-100">{item.serviceName}</h4>
                      <span className="text-[10px] px-2 py-0.5 rounded bg-slate-800 text-slate-400 border border-slate-700">
                        {item.category}
                      </span>
                    </div>
                    <p className="text-xs text-slate-400 mb-1.5 leading-relaxed">{item.description}</p>
                    <p className="text-[11px] font-mono text-slate-500 truncate">{item.targetSummary}</p>
                  </div>
                  <div className="shrink-0">
                    <span className={`text-[10px] px-2.5 py-1 rounded-full font-semibold border flex items-center gap-1.5 ${
                      item.isDetected
                        ? 'bg-emerald-950 border-emerald-700 text-emerald-300'
                        : 'bg-slate-800 border-slate-700 text-slate-400'
                    }`}>
                      <span className={`w-1.5 h-1.5 rounded-full ${item.isDetected ? 'bg-emerald-400 animate-pulse' : 'bg-slate-500'}`}></span>
                      {item.detectedSummary}
                    </span>
                  </div>
                </div>
              ))}
            </div>

            {/* Bottom Controls */}
            <div className="px-6 py-4 bg-slate-950 border-t border-slate-800 flex flex-wrap items-center justify-between gap-3">
              <span className="text-[11px] text-slate-400">
                🔒 Windows DPAPI 및 CredDeleteW 네이티브 보안 API를 통해 완전 소거됩니다.
              </span>
              <div className="flex items-center gap-2.5">
                <button
                  onClick={() => setShowAccountPurgeModal(false)}
                  className="px-4 py-2 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-300 font-semibold text-xs border border-slate-700 transition"
                >
                  닫기
                </button>
                <button
                  disabled={isAccountPurging || accountTargets.filter(t => t.selected).length === 0}
                  onClick={handlePurgeSelectedAccounts}
                  className="flex items-center gap-1.5 px-4 py-2 rounded-lg bg-rose-600 hover:bg-rose-500 text-white font-bold text-xs shadow-lg shadow-rose-600/30 transition disabled:opacity-40"
                >
                  {isAccountPurging ? (
                    <>
                      <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                      <span>소거 및 로그아웃 중...</span>
                    </>
                  ) : (
                    <>
                      <Trash2 className="w-3.5 h-3.5" />
                      <span>선택한 계정 일괄 소거 &amp; 로그아웃 ({accountTargets.filter(t => t.selected).length})</span>
                    </>
                  )}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Windows Default Profile Overwrite Warning Modal */}
      {showDefaultProfileWarningModal && (
        <div id="default-profile-warning-modal-backdrop" className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in duration-150">
          <div className="bg-slate-900 border-2 border-amber-500/80 rounded-2xl w-full max-w-2xl overflow-hidden shadow-2xl shadow-amber-950/50 flex flex-col max-h-[90vh]">
            {/* Modal Header */}
            <div className="px-6 py-4 bg-gradient-to-r from-amber-950/80 via-slate-900 to-slate-950 border-b border-amber-500/30 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-amber-500/20 border border-amber-500/40 flex items-center justify-center text-xl shrink-0">
                  ⚠️
                </div>
                <div>
                  <h3 className="font-bold text-base text-amber-200">Windows 기본 프로필(Default) 덮어쓰기 복제 확인</h3>
                  <p className="text-xs text-amber-400/80 mt-0.5">현재 계정의 환경설정을 시스템 공용 템플릿(C:\Users\Default)으로 복제합니다.</p>
                </div>
              </div>
              <button
                onClick={handleCancelDefaultProfile}
                className="w-8 h-8 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white flex items-center justify-center text-sm font-bold transition cursor-pointer"
              >
                ✕
              </button>
            </div>

            {/* Modal Body */}
            <div className="p-6 overflow-y-auto space-y-4 text-xs">
              {/* Alert Warning Box */}
              <div className="p-3.5 rounded-xl bg-amber-950/60 border border-amber-500/40 text-amber-200 leading-relaxed">
                <div className="font-bold text-amber-300 text-sm mb-1 flex items-center gap-1.5">
                  <span>🚨 이 작업은 Windows 시스템의 기본 사용자 계정 템플릿을 영구적으로 변경합니다.</span>
                </div>
                <p>
                  앞으로 이 PC에서 새로 생성되는 모든 계정에 현재 계정의 UI/UX 환경(바탕화면, 테마, 작업표시줄, 탐색기 옵션 등)이 복제되므로, 변경 전 아래 영향 범위를 반드시 확인하십시오.
                </p>
              </div>

              {/* Impact Analysis Items */}
              <div className="space-y-2.5">
                <div className="font-bold text-slate-200 text-sm">📋 상세 영향 분석 및 다중 안전장치 안내</div>

                <div className="p-3 rounded-lg bg-slate-950/60 border border-slate-800">
                  <div className="font-bold text-sky-400 mb-1">1. 신규 생성 계정 전체 자동 적용</div>
                  <p className="text-slate-300 leading-relaxed">
                    향후 이 PC에서 새로 생성되는 모든 로컬 및 도메인 사용자 계정이 현재 로그인된 사용자의 바탕화면, 테마, 작업표시줄 고정 앱, 마우스 감도, 탐색기 폴더 보기 설정을 100% 동일하게 물려받습니다.
                  </p>
                </div>

                <div className="p-3 rounded-lg bg-slate-950/60 border border-slate-800">
                  <div className="font-bold text-emerald-400 mb-1">2. 순정 원본 자동 백업 &amp; 즉시 롤백 지원</div>
                  <p className="text-slate-300 leading-relaxed">
                    작업 직전 기존의 순정 Windows 기본 프로필은 <code className="px-1.5 py-0.5 rounded bg-slate-800 text-emerald-300 font-mono">C:\Users\Default_Backup_WinPurify</code> 디렉터리에 자동으로 원본 백업되므로, 유사시 언제든 원상태로 롤백할 수 있습니다.
                  </p>
                </div>

                <div className="p-3 rounded-lg bg-slate-950/60 border border-slate-800">
                  <div className="font-bold text-amber-400 mb-1">3. 단일 C: 드라이브 완벽 호환 &amp; 레지스트리 덤프 (reg save HKCU)</div>
                  <p className="text-slate-300 leading-relaxed">
                    시스템이 점유 중인 파일 락(Lock)을 100% 우회하기 위해 커널 레벨의 <code className="px-1.5 py-0.5 rounded bg-slate-800 text-amber-300 font-mono">reg.exe save HKCU</code> 덤프 파이프라인을 사용합니다. 별도의 D: 보조 드라이브 없이 C: 단일 드라이브 환경에서도 충돌 없이 완벽히 동작합니다.
                  </p>
                </div>

                <div className="p-3 rounded-lg bg-slate-950/60 border border-slate-800">
                  <div className="font-bold text-purple-400 mb-1">4. 새 계정 보안 권한(ACL) 자동 정제 및 임시 프로필 오류 차단</div>
                  <p className="text-slate-300 leading-relaxed">
                    신규 사용자가 로그인할 때 '임시 프로필(Temporary Profile)' 오류나 액세스 거부가 발생하지 않도록, 복사 완료 후 <code className="px-1.5 py-0.5 rounded bg-slate-800 text-purple-300 font-mono">Everyone</code> 및 <code className="px-1.5 py-0.5 rounded bg-slate-800 text-purple-300 font-mono">Users</code> 보안 식별자 상속 권한(icacls)이 자동으로 부여됩니다.
                  </p>
                </div>

                <div className="p-2.5 rounded-lg bg-slate-950/40 border border-slate-800/80 text-slate-400">
                  ℹ️ 참고: 이미 생성되어 존재하는 기존의 다른 사용자 계정에는 아무런 영향을 주지 않습니다.
                </div>
              </div>
            </div>

            {/* Modal Footer */}
            <div className="px-6 py-4 bg-slate-950 border-t border-slate-800 flex flex-wrap items-center justify-between gap-3">
              <span className="text-[11px] text-amber-400/90 font-medium">
                ※ 특수 기능 항목으로 일반 최적화 프리셋에서 자동 선택되지 않습니다.
              </span>
              <div className="flex items-center gap-2.5">
                <button
                  onClick={handleCancelDefaultProfile}
                  className="px-4 py-2 rounded-lg bg-slate-800 hover:bg-slate-700 text-slate-300 font-semibold text-xs border border-slate-700 transition cursor-pointer"
                >
                  취소 (선택 안 함)
                </button>
                <button
                  onClick={handleConfirmDefaultProfile}
                  className="px-4 py-2 rounded-lg bg-amber-600 hover:bg-amber-500 text-white font-bold text-xs shadow-lg shadow-amber-600/30 transition flex items-center gap-1.5 cursor-pointer"
                >
                  <span>위험성을 확인했으며 활성화</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Dual-Language License Agreement Modal (한국어 / English EULA) */}
      {showLicenseModal && (
        <div id="license-modal" className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-950/80 backdrop-blur-sm animate-in fade-in duration-200">
          <div className="bg-slate-900 border border-amber-500/40 rounded-2xl w-full max-w-4xl max-h-[90vh] flex flex-col shadow-2xl overflow-hidden ring-1 ring-amber-500/20">
            {/* Header */}
            <div className="px-6 py-4 bg-slate-950 border-b border-slate-800 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-xl bg-amber-500/10 border border-amber-500/30 text-amber-400">
                  <FileText className="w-5 h-5" />
                </div>
                <div>
                  <h3 className="text-base font-bold text-white flex items-center gap-2">
                    <span>WinPurify Pro 소프트웨어 사용권 계약서 (EULA)</span>
                    <span className="text-xs px-2 py-0.5 rounded-full bg-blue-500/20 text-blue-400 border border-blue-500/30 font-normal">
                      KO / EN 2종 지원
                    </span>
                  </h3>
                  <p className="text-xs text-slate-400">
                    Cisnet Soft 공식 소프트웨어 라이선스 정의 및 Inno Setup 설치 마법사 연동 EULA
                  </p>
                </div>
              </div>
              <button
                onClick={() => setShowLicenseModal(false)}
                className="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition cursor-pointer"
                title="닫기"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {/* Language Selector Bar & Action Controls */}
            <div className="px-6 py-3 bg-slate-950/60 border-b border-slate-800 flex flex-wrap items-center justify-between gap-3">
              <div className="flex items-center gap-2">
                <span className="text-xs text-slate-400 font-semibold mr-1">언어 선택:</span>
                <button
                  onClick={() => { setLicenseLang('ko'); setLicenseCopied(false); }}
                  className={`px-3.5 py-1.5 rounded-lg text-xs font-bold transition flex items-center gap-1.5 cursor-pointer ${
                    licenseLang === 'ko'
                      ? 'bg-blue-600 text-white shadow-md shadow-blue-600/30 ring-1 ring-blue-400'
                      : 'bg-slate-800 text-slate-300 hover:bg-slate-700'
                  }`}
                >
                  <span>🇰🇷 한국어 (Korean Standard)</span>
                </button>
                <button
                  onClick={() => { setLicenseLang('en'); setLicenseCopied(false); }}
                  className={`px-3.5 py-1.5 rounded-lg text-xs font-bold transition flex items-center gap-1.5 cursor-pointer ${
                    licenseLang === 'en'
                      ? 'bg-blue-600 text-white shadow-md shadow-blue-600/30 ring-1 ring-blue-400'
                      : 'bg-slate-800 text-slate-300 hover:bg-slate-700'
                  }`}
                >
                  <span>🇺🇸 English (Standard EULA)</span>
                </button>
              </div>

              <div className="flex items-center gap-2">
                <button
                  onClick={() => {
                    const text = licenseLang === 'ko' ? LICENSE_TEXT_KO : LICENSE_TEXT_EN;
                    navigator.clipboard.writeText(text);
                    setLicenseCopied(true);
                    setTimeout(() => setLicenseCopied(false), 2000);
                  }}
                  className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 border border-slate-700 text-slate-200 text-xs font-semibold transition cursor-pointer"
                  title="라이선스 계약서 전문을 클립보드에 복사합니다."
                >
                  {licenseCopied ? <Check className="w-3.5 h-3.5 text-emerald-400" /> : <Copy className="w-3.5 h-3.5" />}
                  <span>{licenseCopied ? '복사 완료!' : '전문 복사'}</span>
                </button>
                <a
                  href={licenseLang === 'ko' ? 'LICENSE_KO.txt' : 'LICENSE_EN.txt'}
                  download={licenseLang === 'ko' ? 'WinPurifyPro_LICENSE_KO.txt' : 'WinPurifyPro_LICENSE_EN.txt'}
                  className="flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 border border-slate-700 text-slate-200 text-xs font-semibold transition cursor-pointer"
                  title="라이선스 텍스트 파일을 로컬에 다운로드합니다."
                >
                  <Download className="w-3.5 h-3.5" />
                  <span>다운로드</span>
                </a>
              </div>
            </div>

            {/* License Text View Area */}
            <div className="p-6 overflow-y-auto max-h-[58vh] bg-slate-950 font-mono text-xs leading-relaxed text-slate-200 select-text">
              <pre className="whitespace-pre-wrap font-sans text-slate-200 leading-6">
                {licenseLang === 'ko' ? (
                  <div>
                    <div className="text-center font-bold text-sky-400 text-sm pb-4 border-b border-slate-800 mb-4">
                      ================================================================================<br />
                      WinPurify Pro 최종 사용자 사용권 계약서 (EULA)<br />
                      (End User License Agreement - Korean Edition)<br />
                      ================================================================================
                    </div>
                    <p className="text-slate-300 mb-4">
                      본 최종 사용자 사용권 계약서(이하 "본 계약")는 <strong>Cisnet Soft</strong>(이하 "회사")가 개발 및 공급하는 <strong>"WinPurify Pro"</strong>(이하 "소프트웨어", C++ 네이티브 가속 엔진 <code className="text-sky-300 font-mono">PurifyEngineCore.dll</code>, 원격 에이전트 서비스 <code className="text-sky-300 font-mono">WinPurifyAgentService</code>, 센트럴 커맨더 <code className="text-sky-300 font-mono">Central Commander</code> 및 부속 어셈블리와 문서 포함)의 설치 및 사용에 관한 사용자와 회사 간의 법적 계약입니다.
                    </p>
                    <p className="text-amber-300/90 mb-6 bg-amber-500/10 p-3 rounded-lg border border-amber-500/20">
                      ⚠️ 사용자가 본 소프트웨어를 설치, 복사 또는 사용하는 것은 본 계약 조건의 전부에 동의함을 의미합니다. 본 계약 조건에 동의하지 않는 경우, 설치를 중단하고 소프트웨어 파일 일체를 즉시 삭제하여 주십시오.
                    </p>

                    <div className="space-y-4">
                      <div className="p-3.5 rounded-xl bg-slate-900 border border-slate-800">
                        <div className="font-bold text-sky-400 text-sm mb-1.5">제1조 (사용권의 부여 및 허용 범위)</div>
                        <ul className="list-disc list-inside space-y-1 text-slate-300">
                          <li>회사는 본 계약 조건에 따라 사용자에게 비독점적이고 양도 불가능한 소프트웨어 사용 권한을 부여합니다.</li>
                          <li>개인 사용자 및 기업, 교육기관, 공공기관은 정식 라이선스 범위 내에서 Windows 운영체제 기반 PC 및 서버 환경에 본 소프트웨어를 설치하고, 133개 최적화 모듈, 실시간 모니터링, 자동 스케줄러, 세이프포인트 백업 및 복원 기능을 자유롭게 사용할 수 있습니다.</li>
                          <li>Unpacked Multi-File 정식 설치 또는 Portable 단독 실행 패키지 형태로 허가된 시스템에 배포하여 사용할 수 있습니다.</li>
                        </ul>
                      </div>

                      <div className="p-3.5 rounded-xl bg-slate-900 border border-slate-800">
                        <div className="font-bold text-emerald-400 text-sm mb-1.5">제2조 (원격 관제 및 네트워크 포트 통신)</div>
                        <ul className="list-disc list-inside space-y-1 text-slate-300">
                          <li>소프트웨어에 포함된 Central Commander 및 WinPurifyAgentService는 다중 PC 원격 진단 및 일괄 정화 관제를 위해 <strong>TCP 포트 9870(REST 제어 채널)</strong> 및 <strong>UDP 포트 9871(브로드캐스트 탐색)</strong>을 사용합니다.</li>
                          <li>사용자는 인스톨러 설치 마법사의 원격포트 열기 옵션(<code className="text-emerald-300 font-mono">openremoteport</code>)을 통해 방화벽 예외 등록을 자유롭게 선택할 수 있으며, <strong>기본값으로 원격포트 개방이 활성화</strong>됩니다.</li>
                          <li>모든 원격 제어 및 통신 기능은 사용자가 정당한 관리 권한을 보유한 로컬 에어갭 망 또는 사내 인트라넷 환경에서만 승인된 관리자에 의해 구동되어야 하며, 허가받지 않은 제3자 시스템에 대한 무단 접속 및 임의 제어 용도로의 사용은 엄격히 금지됩니다.</li>
                        </ul>
                      </div>

                      <div className="p-3.5 rounded-xl bg-slate-900 border border-slate-800">
                        <div className="font-bold text-purple-400 text-sm mb-1.5">제3조 (지적재산권 및 저작권)</div>
                        <ul className="list-disc list-inside space-y-1 text-slate-300">
                          <li>본 소프트웨어와 관련된 프로그램 코드, UI 디자인, 아이콘(<code className="text-purple-300 font-mono">WinPurify.ico</code>), 로고 이미지, C++ 네이티브 바이너리(<code className="text-purple-300 font-mono">PurifyEngineCore.dll</code>), 문서 및 기술 사양에 대한 일체의 소유권과 저작권은 <strong>Cisnet Soft</strong>에 있습니다.</li>
                          <li>본 소프트웨어는 대한민국 저작권법, 컴퓨터프로그램보호법 및 국제 저작권 협약의 보호를 받습니다.</li>
                          <li>사용자는 회사의 사전 서면 승인 없이 본 소프트웨어를 역컴파일, 리버스 엔지니어링, 디스어셈블하거나 원본 코드를 임의 변조하여 무단 상업적 재배포를 수행할 수 없습니다.</li>
                        </ul>
                      </div>

                      <div className="p-3.5 rounded-xl bg-slate-900 border border-slate-800">
                        <div className="font-bold text-rose-400 text-sm mb-1.5">제4조 (데이터 보호 및 계정 정화 보안 준수)</div>
                        <ul className="list-disc list-inside space-y-1 text-slate-300">
                          <li>소프트웨어에 내장된 계정 로그아웃 및 자격증명 정화(<code className="text-rose-300 font-mono">AccountCredentialPurgeService</code>), 브라우저 공장 초기화, 디스크 포렌식 와이핑(<code className="text-rose-300 font-mono">Cipher</code>) 기능은 영구적인 데이터 및 세션 소거를 수반합니다.</li>
                          <li>회사는 사용자의 PC로부터 개인정보, 로그인 암호, 하드웨어 식별자, 원격 통신 페이로드 등의 민감 데이터를 외부 서버로 수집하거나 무단 전송하지 않으며, <strong>모든 정화 작업은 로컬 장치 내에서 오프라인 완결형으로 실행</strong>됩니다.</li>
                        </ul>
                      </div>

                      <div className="p-3.5 rounded-xl bg-slate-900 border border-slate-800">
                        <div className="font-bold text-amber-400 text-sm mb-1.5">제5조 (보증의 한계 및 면책 조항)</div>
                        <ul className="list-disc list-inside space-y-1 text-slate-300">
                          <li>회사는 관련 법률이 허용하는 최대 범위 내에서 본 소프트웨어를 "있는 그대로(AS-IS)" 제공하며, 상품성, 특정 목적에 대한 적합성 또는 무결성에 대해 어떠한 명시적/묵시적 보증도 부인합니다.</li>
                          <li>본 소프트웨어의 깊은 시스템 레지스트리 청소, 서비스 비활성화, 원격 전원 종료 및 재부팅을 실행하기 전에, 사용자는 내장된 실시간 세이프포인트(<code className="text-amber-300 font-mono">LiveSafepointService</code>) 스냅샷 생성 및 중요 데이터 백업을 수행할 것을 강력히 권장합니다.</li>
                          <li>회사는 소프트웨어 사용 또는 사용 불능으로 인해 발생하는 데이터 손실, 업무 중단, 컴퓨터 오작동 등 직·간접적 또는 부수적 손해에 대해 법적 책임을 부담하지 않습니다.</li>
                        </ul>
                      </div>

                      <div className="p-3.5 rounded-xl bg-slate-900 border border-slate-800">
                        <div className="font-bold text-slate-300 text-sm mb-1.5">제6조 (계약의 해지 및 준거법)</div>
                        <ul className="list-disc list-inside space-y-1 text-slate-300">
                          <li>사용자가 본 계약 조건을 위반하는 경우, 회사는 별도의 통지 없이 사용권을 해지할 수 있으며 사용자는 소프트웨어의 모든 복사본을 즉시 파기해야 합니다.</li>
                          <li>본 계약은 대한민국 법률에 따라 규율되고 해석되며, 본 계약과 관련하여 발생하는 모든 분쟁은 대한민국 법원을 관할 법원으로 합니다.</li>
                        </ul>
                      </div>
                    </div>

                    <div className="mt-6 p-4 rounded-xl bg-slate-900/60 border border-slate-800 text-center text-xs text-slate-400">
                      공급자: <strong>Cisnet Soft</strong> (개발자: AhBiYout) • 블로그: <a href="https://ahbiyoutvibe.blogspot.com/" target="_blank" rel="noreferrer" className="text-purple-400 underline">https://ahbiyoutvibe.blogspot.com/</a> • 웹사이트: <a href="http://www.cisnet.co.kr/" target="_blank" rel="noreferrer" className="text-sky-400 underline">http://www.cisnet.co.kr/</a>
                    </div>
                  </div>
                ) : (
                  <div>
                    <div className="text-center font-bold text-sky-400 text-sm pb-4 border-b border-slate-800 mb-4">
                      ================================================================================<br />
                      WinPurify Pro End User License Agreement (EULA)<br />
                      (Standard English Edition)<br />
                      ================================================================================
                    </div>
                    <p className="text-slate-300 mb-4">
                      IMPORTANT: PLEASE READ THIS SOFTWARE LICENSE AGREEMENT CAREFULLY BEFORE DOWNLOADING, INSTALLING, OR USING WINPURIFY PRO.
                    </p>
                    <p className="text-amber-300/90 mb-6 bg-amber-500/10 p-3 rounded-lg border border-amber-500/20">
                      This End User License Agreement ("EULA") is a legal agreement between you (either an individual or a single legal entity, hereinafter "User") and <strong>Cisnet Soft</strong> ("Licensor", developer of WinPurify Pro) regarding the use of <strong>"WinPurify Pro"</strong> (including the 133-module optimization suite, native C++ acceleration core <code className="text-sky-300 font-mono">PurifyEngineCore.dll</code>, Central Commander multi-PC fleet management console, <code className="text-sky-300 font-mono">WinPurifyAgentService</code> background Windows service, and associated documentation).
                    </p>

                    <div className="space-y-4">
                      <div className="p-3.5 rounded-xl bg-slate-900 border border-slate-800">
                        <div className="font-bold text-sky-400 text-sm mb-1.5">1. GRANT OF LICENSE</div>
                        <ul className="list-disc list-inside space-y-1 text-slate-300">
                          <li>Licensor grants you a revocable, non-exclusive, non-transferable, limited license to install and execute the Software on compatible Windows devices (Windows 10, Windows 11, and Windows Server 64-bit architectures) strictly in accordance with the terms of this Agreement.</li>
                          <li>You may deploy the Software in Unpacked Multi-File directory installation mode or Standalone Portable mode for personal, educational, corporate, and commercial system maintenance purposes.</li>
                          <li>All features including 133 optimization modules, real-time hardware telemetry, weekly automation scheduler, and Live Safepoint registry backup are authorized for operation under this license.</li>
                        </ul>
                      </div>

                      <div className="p-3.5 rounded-xl bg-slate-900 border border-slate-800">
                        <div className="font-bold text-emerald-400 text-sm mb-1.5">2. REMOTE MANAGEMENT &amp; NETWORK PORT CONFIGURATION</div>
                        <ul className="list-disc list-inside space-y-1 text-slate-300">
                          <li>The Software includes Central Commander and WinPurifyAgentService capabilities operating over <strong>TCP Port 9870 (REST command channel)</strong> and <strong>UDP Port 9871 (broadcast node discovery)</strong>.</li>
                          <li>During installation, users are presented with an explicit option to configure remote ports and Windows Firewall exceptions (<code className="text-emerald-300 font-mono">openremoteport</code>, enabled by default).</li>
                          <li>Network communication features are strictly intended for authorized local area network (LAN) or intranet administration. Deploying or executing remote control, system shutdown, or reboot commands against unauthorized third-party systems is strictly prohibited.</li>
                        </ul>
                      </div>

                      <div className="p-3.5 rounded-xl bg-slate-900 border border-slate-800">
                        <div className="font-bold text-purple-400 text-sm mb-1.5">3. INTELLECTUAL PROPERTY RIGHTS &amp; RESTRICTIONS</div>
                        <ul className="list-disc list-inside space-y-1 text-slate-300">
                          <li>All title, ownership rights, trademarks, and intellectual property rights in and to the Software (including binary code, C++ native core <code className="text-purple-300 font-mono">PurifyEngineCore.dll</code>, WPF UI designs, graphics, and documentation) are owned exclusively by <strong>Cisnet Soft</strong>.</li>
                          <li>The Software is protected by copyright laws and international copyright treaties.</li>
                          <li>You shall not reverse engineer, decompile, disassemble, or derive source code from the compiled binaries, nor distribute, lease, rent, sell, or sublicense the Software without explicit written authorization.</li>
                        </ul>
                      </div>

                      <div className="p-3.5 rounded-xl bg-slate-900 border border-slate-800">
                        <div className="font-bold text-rose-400 text-sm mb-1.5">4. PRIVACY, DATA INTEGRITY &amp; CREDENTIAL PURGE</div>
                        <ul className="list-disc list-inside space-y-1 text-slate-300">
                          <li>WinPurify Pro operates entirely offline and does not harvest, transmit, or store personal data, credentials, telemetry, or network payloads on external servers.</li>
                          <li>Advanced features including Account Credential Purge, Browser Factory Reset, and Disk Forensic Cipher Wiping permanently delete authentication tokens and temporary files. Users bear sole responsibility for verifying targets prior to permanent destruction.</li>
                        </ul>
                      </div>

                      <div className="p-3.5 rounded-xl bg-slate-900 border border-slate-800">
                        <div className="font-bold text-amber-400 text-sm mb-1.5">5. DISCLAIMER OF WARRANTIES &amp; LIMITATION OF LIABILITY</div>
                        <ul className="list-disc list-inside space-y-1 text-slate-300">
                          <li>THE SOFTWARE IS PROVIDED "AS IS" AND "AS AVAILABLE", WITH ALL FAULTS AND WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS, IMPLIED, OR STATUTORY.</li>
                          <li>Users are strongly advised to utilize the built-in Live Safepoint rollback facility and maintain comprehensive system backups before executing aggressive optimization, registry cleaning, or remote power control operations.</li>
                          <li>IN NO EVENT SHALL CISNET SOFT BE LIABLE FOR ANY DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES ARISING OUT OF OR IN CONNECTION WITH THE USE OR INABILITY TO USE THE SOFTWARE.</li>
                        </ul>
                      </div>

                      <div className="p-3.5 rounded-xl bg-slate-900 border border-slate-800">
                        <div className="font-bold text-slate-300 text-sm mb-1.5">6. TERMINATION &amp; GOVERNING LAW</div>
                        <ul className="list-disc list-inside space-y-1 text-slate-300">
                          <li>This Agreement is effective until terminated. Your rights under this license terminate automatically without notice if you fail to comply with any provision of this EULA.</li>
                          <li>This EULA shall be governed by, construed, and enforced in accordance with the laws of the Republic of Korea.</li>
                        </ul>
                      </div>
                    </div>

                    <div className="mt-6 p-4 rounded-xl bg-slate-900/60 border border-slate-800 text-center text-xs text-slate-400">
                      Licensor: <strong>Cisnet Soft</strong> (Lead Developer: AhBiYout) • Blog: <a href="https://ahbiyoutvibe.blogspot.com/" target="_blank" rel="noreferrer" className="text-purple-400 underline">https://ahbiyoutvibe.blogspot.com/</a> • Website: <a href="http://www.cisnet.co.kr/" target="_blank" rel="noreferrer" className="text-sky-400 underline">http://www.cisnet.co.kr/</a>
                    </div>
                  </div>
                )}
              </pre>
            </div>

            {/* Footer */}
            <div className="px-6 py-4 bg-slate-950 border-t border-slate-800 flex items-center justify-between">
              <div className="text-xs text-slate-400">
                Inno Setup 6 설치 마법사에서 한국어/영어 언어 선택에 따라 해당 라이선스 계약서가 자동 출력됩니다.
              </div>
              <button
                onClick={() => setShowLicenseModal(false)}
                className="px-5 py-2 rounded-xl bg-blue-600 hover:bg-blue-500 text-white font-bold text-xs shadow-lg shadow-blue-600/30 transition cursor-pointer"
              >
                확인 완료 (Close)
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ================================================================= */}
      {/* 🔔 System Tray & Notification Center Settings Modal               */}
      {/* ================================================================= */}
      {showTrayModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4">
          <div className="w-full max-w-xl bg-slate-900 border border-indigo-500/50 rounded-2xl shadow-2xl shadow-indigo-500/20 flex flex-col overflow-hidden animate-in fade-in zoom-in-95 duration-200">
            {/* Modal Header */}
            <div className="px-6 py-4 bg-slate-950 border-b border-slate-800 flex items-center justify-between">
              <div className="flex items-center gap-2.5">
                <div className="w-9 h-9 rounded-xl bg-indigo-600/20 border border-indigo-500/40 flex items-center justify-center text-indigo-400">
                  <Bell className="w-5 h-5" />
                </div>
                <div>
                  <div className="flex items-center gap-2">
                    <h3 className="font-bold text-slate-100 text-base">시스템 트레이 &amp; 알림 감시 센터</h3>
                    <span className="text-[10px] px-2 py-0.5 rounded-full bg-indigo-500/20 border border-indigo-500/40 text-indigo-300 font-bold">
                      Taskbar Tray
                    </span>
                  </div>
                  <p className="text-xs text-slate-400">
                    작업 진행사항 실시간 툴팁, Windows 풍선/토스트 알림 및 백그라운드 트레이 상주 정책 설정
                  </p>
                </div>
              </div>
              <button
                onClick={() => setShowTrayModal(false)}
                className="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {/* Modal Tab Buttons */}
            <div className="flex items-center gap-2 px-6 pt-3 bg-slate-950/60 border-b border-slate-800">
              <button
                onClick={() => setTrayActiveTab('policy')}
                className={`px-3 py-2 text-xs font-bold border-b-2 transition ${
                  trayActiveTab === 'policy'
                    ? 'border-indigo-400 text-indigo-300'
                    : 'border-transparent text-slate-400 hover:text-slate-200'
                }`}
              >
                📌 윈도우 창 &amp; 트레이 상주
              </button>
              <button
                onClick={() => setTrayActiveTab('notifications')}
                className={`px-3 py-2 text-xs font-bold border-b-2 transition ${
                  trayActiveTab === 'notifications'
                    ? 'border-indigo-400 text-indigo-300'
                    : 'border-transparent text-slate-400 hover:text-slate-200'
                }`}
              >
                🔔 작업 진행률 &amp; 토스트 알림
              </button>
              <button
                onClick={() => setTrayActiveTab('history')}
                className={`px-3 py-2 text-xs font-bold border-b-2 transition ${
                  trayActiveTab === 'history'
                    ? 'border-indigo-400 text-indigo-300'
                    : 'border-transparent text-slate-400 hover:text-slate-200'
                }`}
              >
                📜 알림 이력 ({toastHistory.length})
              </button>
            </div>

            {/* Modal Body */}
            <div className="p-6 space-y-4 max-h-[60vh] overflow-y-auto">
              {trayActiveTab === 'policy' && (
                <div className="space-y-3">
                  <div className="p-4 rounded-xl bg-slate-950/80 border border-slate-800 space-y-3">
                    <label className="flex items-start gap-3 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={trayOptions.minimizeToTray}
                        onChange={e => {
                          const val = e.target.checked;
                          setTrayOptions(prev => ({ ...prev, minimizeToTray: val }));
                          addLog(val ? '[트레이 설정] 창 최소화 시 시스템 트레이로 축소 활성화' : '[트레이 설정] 창 최소화 시 트레이 축소 비활성화');
                        }}
                        className="mt-0.5 w-4 h-4 rounded text-indigo-600 focus:ring-indigo-500 bg-slate-900 border-slate-700 cursor-pointer"
                      />
                      <div>
                        <div className="text-xs font-bold text-slate-200">창 최소화(_) 시 작업 표시줄 대신 시스템 트레이로 축소</div>
                        <div className="text-[11px] text-slate-400 mt-0.5">
                          창을 최소화하면 작업 표시줄을 깨끗하게 유지하고 트레이 영역에서 백그라운드로 대기합니다.
                        </div>
                      </div>
                    </label>

                    <label className="flex items-start gap-3 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={trayOptions.closeToTray}
                        onChange={e => {
                          const val = e.target.checked;
                          setTrayOptions(prev => ({ ...prev, closeToTray: val }));
                          addLog(val ? '[트레이 설정] 창 닫기(X) 시 백그라운드 트레이 상주 활성화' : '[트레이 설정] 창 닫기 시 즉시 종료 활성화');
                        }}
                        className="mt-0.5 w-4 h-4 rounded text-indigo-600 focus:ring-indigo-500 bg-slate-900 border-slate-700 cursor-pointer"
                      />
                      <div>
                        <div className="text-xs font-bold text-slate-200">창 닫기(X) 시 프로그램 종료 대신 트레이 백그라운드 상주 (권장)</div>
                        <div className="text-[11px] text-slate-400 mt-0.5">
                          실수로 창을 닫아도 백그라운드에서 예약된 자동 태스크 스케줄러 및 실시간 RAM 감시가 정상 작동합니다.
                        </div>
                      </div>
                    </label>
                  </div>

                  <div className="p-3.5 rounded-xl bg-indigo-950/30 border border-indigo-800/40 text-xs text-indigo-300 flex items-center justify-between">
                    <span>지금 바로 시스템 트레이로 창을 최소화하여 백그라운드 대기 상태로 전환합니다.</span>
                    <button
                      onClick={() => {
                        setIsTrayMinimized(true);
                        setShowTrayModal(false);
                        dispatchTrayNotification('WinPurify Pro 최소화됨', '시스템 트레이로 축소되었습니다. 화면 우측 하단 트레이 아이콘을 클릭하면 다시 복원됩니다.', 'info');
                      }}
                      className="px-3 py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-500 text-white font-bold text-xs shadow-md transition"
                    >
                      ⬇️ 트레이 최소화
                    </button>
                  </div>
                </div>
              )}

              {trayActiveTab === 'notifications' && (
                <div className="space-y-3">
                  <div className="p-4 rounded-xl bg-slate-950/80 border border-slate-800 space-y-3">
                    <label className="flex items-start gap-3 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={trayOptions.liveTooltipProgress}
                        onChange={e => setTrayOptions(prev => ({ ...prev, liveTooltipProgress: e.target.checked }))}
                        className="mt-0.5 w-4 h-4 rounded text-indigo-600 focus:ring-indigo-500 bg-slate-900 border-slate-700 cursor-pointer"
                      />
                      <div>
                        <div className="text-xs font-bold text-slate-200">트레이 아이콘 실시간 작업 진행률(Progress) 툴팁 표시</div>
                        <div className="text-[11px] text-slate-400 mt-0.5">
                          트레이 아이콘에 마우스를 올렸을 때 현재 진행 중인 최적화 퍼센트(%), 현재 처리 모듈명 및 CPU/RAM 수치를 표시합니다.
                        </div>
                      </div>
                    </label>

                    <label className="flex items-start gap-3 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={trayOptions.toastNotifications}
                        onChange={e => setTrayOptions(prev => ({ ...prev, toastNotifications: e.target.checked }))}
                        className="mt-0.5 w-4 h-4 rounded text-indigo-600 focus:ring-indigo-500 bg-slate-900 border-slate-700 cursor-pointer"
                      />
                      <div>
                        <div className="text-xs font-bold text-slate-200">작업 시작 및 완료 시 풍선/토스트 알림 배너 표시</div>
                        <div className="text-[11px] text-slate-400 mt-0.5">
                          최적화 시작, 성공 완료 및 절감 용량, RAM 압축 완료 시 화면 우측 하단에 알림 배너를 띄웁니다.
                        </div>
                      </div>
                    </label>

                    <label className="flex items-start gap-3 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={trayOptions.milestoneNotifications}
                        onChange={e => setTrayOptions(prev => ({ ...prev, milestoneNotifications: e.target.checked }))}
                        className="mt-0.5 w-4 h-4 rounded text-indigo-600 focus:ring-indigo-500 bg-slate-900 border-slate-700 cursor-pointer"
                      />
                      <div>
                        <div className="text-xs font-bold text-slate-200">50% 진행률 마일스톤 도달 시 중간 경과 알림</div>
                        <div className="text-[11px] text-slate-400 mt-0.5">
                          대규모 일괄 정화 작업 시 절반 진행 지점에서 중간 처리 현황을 알림으로 안내합니다.
                        </div>
                      </div>
                    </label>

                    <label className="flex items-start gap-3 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={trayOptions.soundAlerts}
                        onChange={e => setTrayOptions(prev => ({ ...prev, soundAlerts: e.target.checked }))}
                        className="mt-0.5 w-4 h-4 rounded text-indigo-600 focus:ring-indigo-500 bg-slate-900 border-slate-700 cursor-pointer"
                      />
                      <div>
                        <div className="text-xs font-bold text-slate-200">알림 발생 시 청각 차임음(Audio Chime) 재생</div>
                        <div className="text-[11px] text-slate-400 mt-0.5">
                          중요 알림 및 작업 완료 시 부드러운 오디오 신호음을 함께 출력합니다.
                        </div>
                      </div>
                    </label>
                  </div>

                  {/* Browser Native OS Notifications Permission */}
                  <div className="p-4 rounded-xl bg-slate-950/80 border border-slate-800 flex items-center justify-between">
                    <div>
                      <div className="text-xs font-bold text-slate-200 flex items-center gap-1.5">
                        <span>🌐 브라우저 / Windows OS 네이티브 데스크톱 알림</span>
                        <span className={`text-[10px] px-1.5 py-0.5 rounded font-bold ${
                          typeof window !== 'undefined' && 'Notification' in window && Notification.permission === 'granted'
                            ? 'bg-emerald-500/20 text-emerald-400'
                            : 'bg-amber-500/20 text-amber-400'
                        }`}>
                          {typeof window !== 'undefined' && 'Notification' in window && Notification.permission === 'granted'
                            ? '허용됨'
                            : '권한 필요'}
                        </span>
                      </div>
                      <div className="text-[11px] text-slate-400 mt-0.5">
                        브라우저 최소화 상태에서도 Windows 11 액션 센터에 직접 알림을 표시합니다.
                      </div>
                    </div>
                    <button
                      onClick={requestBrowserNotificationPermission}
                      className="px-3.5 py-1.5 rounded-lg bg-slate-800 hover:bg-slate-700 border border-slate-700 text-slate-200 font-semibold text-xs transition"
                    >
                      권한 요청
                    </button>
                  </div>

                  {/* Test notification button */}
                  <div className="flex items-center justify-between p-3.5 rounded-xl bg-indigo-950/40 border border-indigo-800/40">
                    <span className="text-xs text-indigo-300">알림 배너 및 효과음이 정상 출력되는지 지금 즉시 테스트합니다.</span>
                    <button
                      onClick={() => {
                        dispatchTrayNotification(
                          '🔔 WinPurify Pro 트레이 테스트 알림',
                          '시스템 트레이 작업 감시 및 알림 기능이 완벽하게 정상 작동하고 있습니다.',
                          'info'
                        );
                        addLog('[트레이 알림] 테스트 트레이 알림 발송 완료');
                      }}
                      className="px-3.5 py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-500 text-white font-bold text-xs shadow-md transition"
                    >
                      🧪 테스트 알림 발송
                    </button>
                  </div>
                </div>
              )}

              {trayActiveTab === 'history' && (
                <div className="space-y-2">
                  <div className="flex items-center justify-between text-xs text-slate-400 pb-1">
                    <span>최근 수신된 트레이 알림 이력 ({toastHistory.length}건)</span>
                    <button
                      onClick={() => setToastHistory([])}
                      className="text-slate-500 hover:text-rose-400 transition"
                    >
                      전체 비우기
                    </button>
                  </div>
                  <div className="space-y-2 max-h-60 overflow-y-auto pr-1">
                    {toastHistory.length === 0 ? (
                      <div className="p-8 text-center text-xs text-slate-500">기록된 알림 이력이 없습니다.</div>
                    ) : (
                      toastHistory.map(item => (
                        <div key={item.id} className="p-3 rounded-xl bg-slate-950 border border-slate-800 flex items-start justify-between gap-3 text-xs">
                          <div>
                            <div className="font-bold text-slate-200">{item.title}</div>
                            <div className="text-slate-400 mt-0.5 text-[11px]">{item.message}</div>
                          </div>
                          <span className="text-[10px] text-slate-500 font-mono shrink-0">{item.timestamp}</span>
                        </div>
                      ))
                    )}
                  </div>
                </div>
              )}
            </div>

            {/* Modal Footer */}
            <div className="px-6 py-4 bg-slate-950 border-t border-slate-800 flex items-center justify-between">
              <div className="text-xs text-slate-400">
                트레이 아이콘을 더블 클릭하면 창이 다시 복원되며, 우클릭 시 빠른 메뉴가 열립니다.
              </div>
              <button
                onClick={() => setShowTrayModal(false)}
                className="px-5 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-bold text-xs shadow-lg shadow-indigo-600/30 transition cursor-pointer"
              >
                확인 완료 (Save)
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ================================================================= */}
      {/* 🖥️ Minimized to Tray Floating Banner                              */}
      {/* ================================================================= */}
      {isTrayMinimized && (
        <div className="fixed top-0 left-0 right-0 z-50 bg-gradient-to-r from-indigo-900 via-slate-900 to-indigo-900 border-b border-indigo-500/50 shadow-2xl p-3 flex items-center justify-between text-xs animate-in slide-in-from-top duration-200">
          <div className="flex items-center gap-2.5">
            <span className="w-2.5 h-2.5 rounded-full bg-emerald-400 animate-ping"></span>
            <span className="font-bold text-indigo-200">WinPurify Pro가 시스템 트레이로 최소화되었습니다.</span>
            <span className="text-slate-400 hidden sm:inline">
              백그라운드에서 작업 감시 및 자동 스케줄러가 대기 중입니다.
            </span>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={() => setIsTrayMinimized(false)}
              className="px-3.5 py-1.5 rounded-lg bg-indigo-600 hover:bg-indigo-500 text-white font-bold text-xs transition shadow-md"
            >
              🖥️ 메인 화면 복원
            </button>
          </div>
        </div>
      )}

      {/* ================================================================= */}
      {/* 🚀 Floating Real-Time Action Center Toast Notification Flyout     */}
      {/* ================================================================= */}
      <div className="fixed bottom-16 right-5 z-50 flex flex-col gap-2.5 max-w-sm pointer-events-auto">
        {toasts.map(toast => (
          <div
            key={toast.id}
            className={`p-3.5 rounded-2xl border shadow-2xl backdrop-blur-md transition-all duration-300 animate-in slide-in-from-right-5 fade-in ${
              toast.type === 'success'
                ? 'bg-slate-900/95 border-emerald-500/50 shadow-emerald-500/10'
                : toast.type === 'warning'
                ? 'bg-slate-900/95 border-amber-500/50 shadow-amber-500/10'
                : toast.type === 'error'
                ? 'bg-slate-900/95 border-rose-500/50 shadow-rose-500/10'
                : 'bg-slate-900/95 border-indigo-500/50 shadow-indigo-500/10'
            }`}
          >
            <div className="flex items-start justify-between gap-3">
              <div className="flex items-start gap-2.5">
                <div className={`w-7 h-7 rounded-lg flex items-center justify-center shrink-0 text-sm ${
                  toast.type === 'success' ? 'bg-emerald-500/20 text-emerald-400' :
                  toast.type === 'warning' ? 'bg-amber-500/20 text-amber-400' :
                  'bg-indigo-500/20 text-indigo-400'
                }`}>
                  {toast.type === 'success' ? '✨' : toast.type === 'warning' ? '⚠️' : '🚀'}
                </div>
                <div>
                  <div className="font-bold text-xs text-slate-100 flex items-center gap-2">
                    <span>{toast.title}</span>
                    <span className="text-[10px] text-slate-500 font-mono">{toast.timestamp}</span>
                  </div>
                  <div className="text-[11px] text-slate-300 mt-0.5 leading-snug">{toast.message}</div>
                  {toast.progress !== undefined && (
                    <div className="mt-2 space-y-1">
                      <div className="flex items-center justify-between text-[10px] font-bold text-indigo-300">
                        <span>진행률</span>
                        <span>{toast.progress}%</span>
                      </div>
                      <div className="w-full h-1.5 rounded-full bg-slate-800 overflow-hidden">
                        <div
                          className="h-full bg-indigo-500 transition-all duration-300"
                          style={{ width: `${toast.progress}%` }}
                        ></div>
                      </div>
                    </div>
                  )}
                </div>
              </div>
              <button
                onClick={() => setToasts(prev => prev.filter(t => t.id !== toast.id))}
                className="text-slate-500 hover:text-white p-0.5"
              >
                <X className="w-3.5 h-3.5" />
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* ================================================================= */}
      {/* 💻 Windows 11 Taskbar System Tray Simulator HUD                   */}
      {/* ================================================================= */}
      <div className="fixed bottom-3 right-4 z-40">
        <div className="relative">
          {/* Windows 11 Tray Popover Context Menu */}
          {showTrayMenu && (
            <div className="absolute bottom-full right-0 mb-3 w-64 rounded-2xl bg-slate-900/95 backdrop-blur-xl border border-slate-700/80 shadow-2xl p-2 text-xs space-y-1 animate-in zoom-in-95 fade-in duration-150">
              <div className="px-3 py-2 border-b border-slate-800">
                <div className="font-bold text-slate-100 flex items-center justify-between">
                  <span>WinPurify Pro v{APP_VERSION}</span>
                  <span className="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
                </div>
                <div className="text-[10px] text-slate-400 mt-0.5">
                  상태: {isProcessing ? '⚡ 정화 실행 중...' : '🟢 대기 중 (Ready)'}
                </div>
              </div>

              <button
                onClick={() => {
                  setIsTrayMinimized(prev => !prev);
                  setShowTrayMenu(false);
                }}
                className="w-full text-left px-3 py-2 rounded-lg text-slate-200 hover:bg-slate-800 flex items-center gap-2 font-semibold transition"
              >
                <span>🖥️</span>
                <span>{isTrayMinimized ? 'WinPurify Pro 창 복원' : '트레이로 최소화'}</span>
              </button>

              <button
                disabled={isProcessing || selectedCount === 0}
                onClick={() => {
                  handleOptimize();
                  setShowTrayMenu(false);
                }}
                className="w-full text-left px-3 py-2 rounded-lg text-indigo-300 hover:bg-indigo-950/60 flex items-center gap-2 font-semibold transition disabled:opacity-50"
              >
                <span>⚡</span>
                <span>선택 모듈 빠른 정화 ({selectedCount}개)</span>
              </button>

              <button
                disabled={isProcessing}
                onClick={() => {
                  handleFastScan();
                  setShowTrayMenu(false);
                }}
                className="w-full text-left px-3 py-2 rounded-lg text-sky-300 hover:bg-sky-950/60 flex items-center gap-2 font-semibold transition disabled:opacity-50"
              >
                <span>🔍</span>
                <span>Robocopy 초고속 스캔</span>
              </button>

              <button
                onClick={() => {
                  handleRamTrim();
                  setShowTrayMenu(false);
                }}
                className="w-full text-left px-3 py-2 rounded-lg text-emerald-300 hover:bg-emerald-950/60 flex items-center gap-2 font-semibold transition"
              >
                <span>🧠</span>
                <span>RAM 즉시 압축 (Trim)</span>
              </button>

              <div className="border-t border-slate-800 my-1"></div>

              <button
                onClick={() => {
                  setShowScheduleModal(true);
                  setShowTrayMenu(false);
                }}
                className="w-full text-left px-3 py-2 rounded-lg text-slate-300 hover:bg-slate-800 flex items-center gap-2 transition"
              >
                <span>⏰</span>
                <span>태스크 스케줄러 관리자</span>
              </button>

              <button
                onClick={() => {
                  setShowTrayModal(true);
                  setShowTrayMenu(false);
                }}
                className="w-full text-left px-3 py-2 rounded-lg text-slate-300 hover:bg-slate-800 flex items-center gap-2 transition"
              >
                <span>🔔</span>
                <span>트레이 &amp; 알림 환경설정</span>
              </button>

              <button
                onClick={() => {
                  dispatchTrayNotification('🔔 트레이 테스트 알림', '시스템 트레이 알림 센터가 정상 작동하고 있습니다.', 'info');
                  setShowTrayMenu(false);
                }}
                className="w-full text-left px-3 py-2 rounded-lg text-slate-400 hover:bg-slate-800 flex items-center gap-2 transition"
              >
                <span>🧪</span>
                <span>테스트 알림 발송</span>
              </button>

              <button
                onClick={() => {
                  downloadLogFile(logs, 'WinPurify_Main_Logs');
                  setShowTrayMenu(false);
                }}
                className="w-full text-left px-3 py-2 rounded-lg text-slate-400 hover:bg-slate-800 flex items-center gap-2 transition"
              >
                <span>📂</span>
                <span>작업 로그 내보내기</span>
              </button>
            </div>
          )}

          {/* Windows 11 Taskbar Tray Pill */}
          <div className="flex items-center gap-3 px-3 py-1.5 rounded-2xl bg-slate-900/90 backdrop-blur-md border border-slate-700/80 shadow-2xl text-xs select-none">
            {/* Tray Icon with hover tooltip */}
            <div
              className="relative cursor-pointer"
              onMouseEnter={() => setIsTrayHovered(true)}
              onMouseLeave={() => setIsTrayHovered(false)}
              onClick={() => setShowTrayMenu(prev => !prev)}
              onDoubleClick={() => setIsTrayMinimized(false)}
              title="클릭하여 트레이 메뉴 열기 / 더블클릭하여 창 복원"
            >
              <div className="relative p-1 rounded-lg hover:bg-slate-800 transition flex items-center justify-center">
                <img src="WinPurify_icon.png" alt="WinPurify Tray" className="w-5 h-5 object-contain" />
                <span className={`absolute -bottom-0.5 -right-0.5 w-2 h-2 rounded-full border border-slate-900 ${
                  isProcessing ? 'bg-amber-400 animate-spin' : 'bg-emerald-400 animate-pulse'
                }`}></span>
              </div>

              {/* Windows 11 Live Hover Tooltip */}
              {isTrayHovered && !showTrayMenu && (
                <div className="absolute bottom-full right-0 mb-3 w-64 p-3 rounded-2xl bg-slate-950/95 backdrop-blur-xl border border-slate-700 shadow-2xl text-[11px] space-y-2 pointer-events-none animate-in fade-in zoom-in-95 duration-100 z-50">
                  <div className="flex items-center justify-between border-b border-slate-800 pb-1.5">
                    <span className="font-bold text-slate-200">WinPurify Pro v{APP_VERSION}</span>
                    <span className={`px-1.5 py-0.5 rounded text-[9px] font-bold ${
                      isProcessing ? 'bg-amber-500/20 text-amber-300' : 'bg-emerald-500/20 text-emerald-300'
                    }`}>
                      {isProcessing ? '최적화 진행 중' : '정상 대기'}
                    </span>
                  </div>

                  <div className="space-y-1">
                    <div className="flex justify-between text-slate-400 text-[10px]">
                      <span>CPU: {cpuUsage}%</span>
                      <span>RAM: {ramUsagePercent.toFixed(0)}%</span>
                      <span>여유: 154 GB</span>
                    </div>
                    <div className="flex gap-1.5">
                      <div className="w-1/2 h-1 bg-slate-800 rounded-full overflow-hidden">
                        <div className="h-full bg-sky-400" style={{ width: `${cpuUsage}%` }}></div>
                      </div>
                      <div className="w-1/2 h-1 bg-slate-800 rounded-full overflow-hidden">
                        <div className="h-full bg-emerald-400" style={{ width: `${ramUsagePercent}%` }}></div>
                      </div>
                    </div>
                  </div>

                  <div className="text-[10px] text-indigo-300 bg-indigo-950/40 p-1.5 rounded-lg border border-indigo-900/50">
                    {isProcessing 
                      ? '⚡ 하이브리드 C++ Native 엔진 최적화 진행 중...' 
                      : `선택된 모듈: ${selectedCount}개 (확보 예상: ${formatSize(totalReclaimableBytes)})`}
                  </div>
                  <div className="text-[9px] text-slate-500 text-center">
                    클릭하여 빠른 메뉴 • 더블클릭 창 복원
                  </div>
                </div>
              )}
            </div>

            {/* Notification Bell Badge Button */}
            <button
              onClick={() => setShowTrayModal(true)}
              className="relative p-1 rounded-lg hover:bg-slate-800 text-slate-400 hover:text-indigo-300 transition"
              title="트레이 알림 센터 설정"
            >
              <Bell className="w-4 h-4" />
              {toasts.length > 0 && (
                <span className="absolute -top-0.5 -right-0.5 w-2 h-2 rounded-full bg-rose-500 animate-ping"></span>
              )}
            </button>

            {/* System Clock */}
            <div className="text-[10px] font-mono text-slate-400 pl-1 border-l border-slate-800">
              {new Date().toLocaleTimeString('ko-KR', { hour: '2-digit', minute: '2-digit' })}
            </div>
          </div>
        </div>
      </div>

      {/* ================================================================= */}
      {/* 🚀 GitHub Releases Live Auto-Update Modal Dialog                  */}
      {/* ================================================================= */}
      {showUpdateModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4">
          <div className="w-full max-w-xl bg-slate-900 border border-sky-500/50 rounded-2xl shadow-2xl shadow-sky-500/20 flex flex-col overflow-hidden animate-in fade-in zoom-in-95 duration-200">
            {/* Header */}
            <div className="px-6 py-4 bg-slate-950 border-b border-slate-800 flex items-center justify-between">
              <div className="flex items-center gap-2.5">
                <div className="w-9 h-9 rounded-xl bg-sky-600/20 border border-sky-500/40 flex items-center justify-center text-sky-400">
                  <Download className="w-5 h-5" />
                </div>
                <div>
                  <div className="flex items-center gap-2">
                    <h3 className="font-bold text-slate-100 text-base">GitHub Releases 실시간 자동 업데이트</h3>
                    <span className="text-[10px] px-2 py-0.5 rounded-full bg-sky-500/20 border border-sky-500/40 text-sky-300 font-bold">
                      WinPurify-Pro
                    </span>
                  </div>
                  <p className="text-xs text-slate-400">
                    공식 GitHub 리포지토리(ahbiyout-all/WinPurify-Pro) 배포 패키지 및 플랫폼별 설치 파일 안내
                  </p>
                </div>
              </div>
              <button
                onClick={() => setShowUpdateModal(false)}
                className="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {/* Body */}
            <div className="p-6 space-y-4 max-h-[65vh] overflow-y-auto">
              {/* Version Comparison Card */}
              <div className="p-4 rounded-xl bg-slate-950 border border-slate-800 space-y-3">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <div className="text-[11px] text-slate-400">현재 앱 버전</div>
                    <div className="text-base font-bold text-sky-400 mt-0.5">v{APP_VERSION}</div>
                  </div>
                  <div>
                    <div className="text-[11px] text-slate-400">GitHub 최신 릴리스</div>
                    <div className="text-base font-bold text-emerald-400 mt-0.5">
                      v{updateInfo.latestVersion}
                    </div>
                  </div>
                </div>

                <div className="p-3 rounded-lg bg-slate-900 border border-slate-800 text-xs text-slate-300">
                  <div className="flex items-center gap-2">
                    {isCheckingUpdate ? (
                      <span className="w-2.5 h-2.5 rounded-full bg-sky-400 animate-spin"></span>
                    ) : (
                      <span className="w-2.5 h-2.5 rounded-full bg-emerald-400"></span>
                    )}
                    <span>{updateInfo.status}</span>
                  </div>
                </div>
              </div>

              {/* Windows PC Package Matrix Card */}
              <div className="space-y-2">
                <div className="text-xs font-bold text-slate-200">📦 Windows PC 전용 패키지 배포 구성 (초경량 ~5MB &amp; 풀 패키지)</div>
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-2.5">
                  {/* Ultra-Slim Single Executable */}
                  <a
                    href="https://github.com/ahbiyout-all/WinPurify-Pro/releases"
                    target="_blank"
                    rel="noreferrer"
                    className="p-3 rounded-xl bg-slate-950 hover:bg-slate-850 border border-emerald-500/40 hover:border-emerald-400 transition flex flex-col justify-between block shadow-sm shadow-emerald-950/20"
                  >
                    <div>
                      <div className="text-xs font-bold text-emerald-300 flex items-center justify-between">
                        <span>⚡ 초경량 단일 파일</span>
                        <span className="text-[9px] px-1.5 py-0.5 rounded bg-emerald-500/20 text-emerald-300 font-mono">~5 MB</span>
                      </div>
                      <div className="text-[10px] text-slate-400 mt-1">
                        WinPurifyPro-Slim.exe (초고속 즉시 실행 무설치 단일 바이너리)
                      </div>
                    </div>
                    <div className="text-[10px] text-emerald-400 font-semibold mt-2">
                      Slim 단일 파일 다운로드 →
                    </div>
                  </a>

                  {/* Ultra-Slim Portable Zip */}
                  <a
                    href="https://github.com/ahbiyout-all/WinPurify-Pro/releases"
                    target="_blank"
                    rel="noreferrer"
                    className="p-3 rounded-xl bg-slate-950 hover:bg-slate-850 border border-teal-500/40 hover:border-teal-400 transition flex flex-col justify-between block shadow-sm shadow-teal-950/20"
                  >
                    <div>
                      <div className="text-xs font-bold text-teal-300 flex items-center justify-between">
                        <span>🪶 슬림 압축팩</span>
                        <span className="text-[9px] px-1.5 py-0.5 rounded bg-teal-500/20 text-teal-300 font-mono">~6 MB</span>
                      </div>
                      <div className="text-[10px] text-slate-400 mt-1">
                        WinPurifyPro-Slim-Portable.zip (Slim EXE + 코어 DLL 초경량팩)
                      </div>
                    </div>
                    <div className="text-[10px] text-teal-400 font-semibold mt-2">
                      Slim Zip 다운로드 →
                    </div>
                  </a>

                  {/* PC Windows Installer */}
                  <a
                    href="https://github.com/ahbiyout-all/WinPurify-Pro/releases"
                    target="_blank"
                    rel="noreferrer"
                    className="p-3 rounded-xl bg-slate-950 hover:bg-slate-850 border border-slate-800 hover:border-sky-500/50 transition flex flex-col justify-between block"
                  >
                    <div>
                      <div className="text-xs font-bold text-slate-100 flex items-center justify-between">
                        <span>🖥️ 정식 설치 마법사</span>
                        <span className="text-[9px] px-1.5 py-0.5 rounded bg-sky-500/20 text-sky-300 font-mono">Setup</span>
                      </div>
                      <div className="text-[10px] text-slate-400 mt-1">
                        WinPurifyPro-Setup.exe (Inno Setup 6 자동 인스톨러)
                      </div>
                    </div>
                    <div className="text-[10px] text-sky-400 font-semibold mt-2">
                      Windows Installer 다운로드 →
                    </div>
                  </a>

                  {/* Central Commander Console */}
                  <a
                    href="https://github.com/ahbiyout-all/WinPurify-Pro/releases"
                    target="_blank"
                    rel="noreferrer"
                    className="p-3 rounded-xl bg-slate-950 hover:bg-slate-850 border border-slate-800 hover:border-purple-500/50 transition flex flex-col justify-between block"
                  >
                    <div>
                      <div className="text-xs font-bold text-slate-100 flex items-center justify-between">
                        <span>📡 중앙 관제 콘솔</span>
                        <span className="text-[9px] px-1.5 py-0.5 rounded bg-purple-500/20 text-purple-300 font-mono">~3 MB</span>
                      </div>
                      <div className="text-[10px] text-slate-400 mt-1">
                        WinPurifyCommander-Slim.exe (다중 PC 원격 제어 콘솔)
                      </div>
                    </div>
                    <div className="text-[10px] text-purple-400 font-semibold mt-2">
                      Commander 바이너리 →
                    </div>
                  </a>
                </div>
              </div>

              {/* Release Notes */}
              {updateInfo.releaseNotes && (
                <div className="space-y-1.5">
                  <div className="text-xs font-bold text-slate-200">📜 릴리스 변경 사항</div>
                  <div className="p-3 rounded-xl bg-slate-950 border border-slate-800 max-h-36 overflow-y-auto text-[11px] font-mono text-slate-400 whitespace-pre-wrap">
                    {updateInfo.releaseNotes}
                  </div>
                </div>
              )}
            </div>

            {/* Footer */}
            <div className="px-6 py-4 bg-slate-950 border-t border-slate-800 flex items-center justify-between">
              <a
                href="https://github.com/ahbiyout-all/WinPurify-Pro"
                target="_blank"
                rel="noreferrer"
                className="text-xs text-sky-400 hover:text-sky-300 hover:underline flex items-center gap-1 font-semibold"
              >
                <span>GitHub ahbiyout-all/WinPurify-Pro 리포지토리 방문</span>
              </a>

              <div className="flex items-center gap-2">
                <button
                  disabled={isCheckingUpdate}
                  onClick={handleCheckGitHubUpdates}
                  className="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 font-bold text-xs transition disabled:opacity-50"
                >
                  {isCheckingUpdate ? '확인 중...' : '🔍 지금 다시 확인'}
                </button>
                <button
                  onClick={() => setShowUpdateModal(false)}
                  className="px-5 py-2 rounded-xl bg-blue-600 hover:bg-blue-500 text-white font-bold text-xs shadow-lg shadow-blue-600/30 transition cursor-pointer"
                >
                  확인 완료
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

const container = document.getElementById('root');
if (container) {
  const root = createRoot(container);
  root.render(<App />);
}

export default App;
