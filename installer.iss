; =====================================================================
; WinPurify Pro - Inno Setup 6 Official Installer Script
; Next-Gen Decoupled Native C++ & C# WPF 133-Module Optimization Suite
; Unpacked Multi-File Directory Architecture (폴더 풀림 정식 설치)
; =====================================================================

#define MyAppName "WinPurify Pro"
#define MyAppVersion "4.42.0"
#define MyAppPublisher "Cisnet Soft"
#define MyAppURL "https://ahbiyoutvibe.blogspot.com/"
#define MyAppExeName "WinPurifyPro.exe"

#ifndef AppSourceDir
  #define AppSourceDir "publish\App"
#endif
#ifndef OutputDir
  #define OutputDir "publish\Installer"
#endif
#ifndef OutputBaseFilename
  #define OutputBaseFilename "WinPurifyPro_v" + MyAppVersion + "_Setup"
#endif

[Setup]
; AppId uniquely identifies this application in Windows Registry / Add-Remove Programs
AppId={{6904B914-A0E9-4BFA-BA5F-BCB0BEA7EF0F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} v{#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; 64-bit Windows 10 & 11 Optimized Architecture
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir={#OutputDir}
OutputBaseFilename={#OutputBaseFilename}
SetupIconFile=WinPurify.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=commandline
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion={#MyAppVersion}.0
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Setup Installer (Unpacked Multi-File Individual Assemblies)
VersionInfoProductName={#MyAppName}
; 실행 중인 프로세스 감지 및 안전 종료
AppMutex=WinPurifyPro_App_Mutex_Global_6904B914
CloseApplications=yes
CloseApplicationsFilter=*.exe
RestartApplications=no
DirExistsWarning=no

[Languages]
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"; LicenseFile: "LICENSE_KO.txt"
Name: "english"; MessagesFile: "compiler:Default.isl"; LicenseFile: "LICENSE_EN.txt"

[CustomMessages]
korean.OpenRemotePortTitle=원격 관제 포트 및 방화벽 예외 열기 (권장: Central Commander 원격 최적화 및 모니터링용 TCP 9870 / UDP 9871 개방)
korean.NetworkGroupDesc=네트워크 및 원격 관리 옵션:
english.OpenRemotePortTitle=Open remote agent ports and Windows Firewall exceptions (Recommended: TCP 9870 / UDP 9871 for Central Commander)
english.NetworkGroupDesc=Network and Remote Management Options:

[Types]
Name: "full"; Description: "표준 정식 설치 (권장: 폴더 풀림 Unpacked Multi-File 개별파일 설치 - 150+ 종속 어셈블리 전체 풀림)"
Name: "compact"; Description: "최소 설치 (WinPurify Pro 메인 클라이언트)"
Name: "custom"; Description: "사용자 정의 설치"; Flags: iscustom

[Components]
Name: "main"; Description: "WinPurify Pro 메인 최적화 클라이언트 (133개 튜닝 모듈, 세이프포인트 엔진, WPF UI)"; Types: full compact custom; Flags: fixed
Name: "nativecore"; Description: "PurifyEngineCore.dll (C++ 네이티브 고속 커널 하드웨어 가속 코어)"; Types: full compact custom; Flags: fixed

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce
Name: "openremoteport"; Description: "{cm:OpenRemotePortTitle}"; GroupDescription: "{cm:NetworkGroupDesc}"; Flags: checkedonce

[Files]
; =====================================================================
; [1] Unpacked Multi-File Primary Application Directory (publish\App)
;     설치 대상 폴더({app})에 실행 파일, 모든 DLL, json 설정, runtimes 하위 폴더가
;     온전한 폴더 트리 형태로 풀려서 직접 배치됩니다 (임시 압축 해제 오버헤드 Zero)
; =====================================================================
Source: "{#AppSourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; =====================================================================
; [2] 핵심 필수 네이티브 코어 및 리소스 보완 (publish\App 미포함 시 대비)
; =====================================================================
Source: "PurifyEngineCore.dll"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "WinPurify.ico"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "WinPurify_logo.png"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "WinPurify_icon.png"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
Source: "installed.tag"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist
; [3] 한국어 및 영어 2종 정식 라이선스 계약서 (EULA - Korean / English)
Source: "LICENSE_KO.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "LICENSE_EN.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}\WinPurify Pro"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\WinPurify.ico"; Components: main
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\WinPurify.ico"; Tasks: desktopicon; Components: main
Name: "{autoprograms}\{#MyAppName}\라이선스 계약서 (Korean EULA)"; Filename: "{app}\LICENSE_KO.txt"; WorkingDir: "{app}"; Languages: korean
Name: "{autoprograms}\{#MyAppName}\License Agreement (English EULA)"; Filename: "{app}\LICENSE_EN.txt"; WorkingDir: "{app}"; Languages: english

[Run]
; =====================================================================
; [1] 중앙 관제 원격 에이전트 윈도우 서비스(WinPurifyAgentService) 등록 및 시작
;     원격포트 열기 옵션(openremoteport - 기본 체크) 선택 시 시스템 서비스로 상시 구동되어 원격 커맨더 명령을 안정적으로 수신합니다.
; =====================================================================
Filename: "{sys}\sc.exe"; Parameters: "create WinPurifyAgentService binPath= """"{app}\{#MyAppExeName}"" --service"" start= auto DisplayName= ""WinPurify Pro Remote Agent Service"""; Flags: runhidden; Tasks: openremoteport; StatusMsg: "원격 관제 윈도우 서비스(WinPurifyAgentService) 등록 중..."
Filename: "{sys}\sc.exe"; Parameters: "description WinPurifyAgentService ""WinPurify Pro 중앙 관제(Central Commander) 원격 최적화 및 시스템 유지보수 명령 수신 백그라운드 서비스"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\sc.exe"; Parameters: "start WinPurifyAgentService"; Flags: runhidden; Tasks: openremoteport; StatusMsg: "원격 관제 윈도우 서비스 시작 중..."

; =====================================================================
; [2] Windows 방화벽 원격 포트(TCP 9870 / UDP 9871) 및 앱 예외 규칙 등록
;     원격포트 열기 옵션(openremoteport - 기본 체크) 선택 시 실행됩니다.
; =====================================================================
; 1) 원격 관제 REST 수신 포트(TCP 9870) 인바운드/아웃바운드 개방
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Remote Port TCP 9870"""; Flags: runhidden; Tasks: openremoteport; StatusMsg: "원격 관제 포트(TCP 9870) 방화벽 개방 중..."
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Remote Port TCP 9870 Outbound"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""WinPurify Remote Port TCP 9870"" dir=in action=allow protocol=TCP localport=9870 enable=yes profile=any description=""WinPurify Pro 원격 관제 HTTP REST 수신 포트(TCP 9870)"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""WinPurify Remote Port TCP 9870 Outbound"" dir=out action=allow protocol=TCP localport=9870 enable=yes profile=any description=""WinPurify Pro 원격 관제 HTTP REST 응답 포트(TCP 9870 Outbound)"""; Flags: runhidden; Tasks: openremoteport

; 2) 원격 자동 탐색 브로드캐스트 포트(UDP 9871) 인바운드/아웃바운드 개방
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Remote Discovery UDP 9871"""; Flags: runhidden; Tasks: openremoteport; StatusMsg: "원격 탐색 포트(UDP 9871) 방화벽 개방 중..."
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Remote Discovery UDP 9871 Outbound"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""WinPurify Remote Discovery UDP 9871"" dir=in action=allow protocol=UDP localport=9871 enable=yes profile=any description=""WinPurify Pro 원격 자동 탐색 브로드캐스트 포트(UDP 9871)"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""WinPurify Remote Discovery UDP 9871 Outbound"" dir=out action=allow protocol=UDP localport=9871 enable=yes profile=any description=""WinPurify Pro 원격 자동 탐색 브로드캐스트 송신 포트(UDP 9871 Outbound)"""; Flags: runhidden; Tasks: openremoteport

; 3) 메인 클라이언트 버전 표기 규칙 (Inbound & Outbound)
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""{#MyAppName} v{#MyAppVersion}"""; Flags: runhidden; Tasks: openremoteport; StatusMsg: "Windows 방화벽 예외 규칙 등록 중 (메인 클라이언트 및 Central Commander)..."
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""{#MyAppName} v{#MyAppVersion} (Outbound)"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""{#MyAppName} v{#MyAppVersion}"" dir=in action=allow program=""""{app}\{#MyAppExeName}"""" enable=yes profile=any description=""{#MyAppName} v{#MyAppVersion} 메인 최적화 클라이언트 인바운드 방화벽 허용 규칙"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""{#MyAppName} v{#MyAppVersion} (Outbound)"" dir=out action=allow program=""""{app}\{#MyAppExeName}"""" enable=yes profile=any description=""{#MyAppName} v{#MyAppVersion} 메인 최적화 클라이언트 아웃바운드 방화벽 허용 규칙"""; Flags: runhidden; Tasks: openremoteport

; 4) 메인 클라이언트 범용 규칙 (Inbound & Outbound)
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""{#MyAppName} Main Client"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""{#MyAppName} Main Client Outbound"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""{#MyAppName} Main Client"" dir=in action=allow program=""""{app}\{#MyAppExeName}"""" enable=yes profile=any description=""{#MyAppName} 메인 최적화 클라이언트 범용 인바운드 방화벽 허용 규칙"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""{#MyAppName} Main Client Outbound"" dir=out action=allow program=""""{app}\{#MyAppExeName}"""" enable=yes profile=any description=""{#MyAppName} 메인 최적화 클라이언트 범용 아웃바운드 방화벽 허용 규칙"""; Flags: runhidden; Tasks: openremoteport

; 5) Central Commander 버전 표기 규칙 (Inbound & Outbound)
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Central Commander v{#MyAppVersion}"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Central Commander v{#MyAppVersion} (Outbound)"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""WinPurify Central Commander v{#MyAppVersion}"" dir=in action=allow program=""""{app}\WinPurifyCommander.exe"""" enable=yes profile=any description=""WinPurify Central Commander v{#MyAppVersion} 원격 관제 콘솔 인바운드 방화벽 허용 규칙"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""WinPurify Central Commander v{#MyAppVersion} (Outbound)"" dir=out action=allow program=""""{app}\WinPurifyCommander.exe"""" enable=yes profile=any description=""WinPurify Central Commander v{#MyAppVersion} 원격 관제 콘솔 아웃바운드 방화벽 허용 규칙"""; Flags: runhidden; Tasks: openremoteport

; 6) Central Commander 범용 규칙 (Inbound & Outbound)
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Central Commander"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Central Commander Outbound"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""WinPurify Central Commander"" dir=in action=allow program=""""{app}\WinPurifyCommander.exe"""" enable=yes profile=any description=""WinPurify Central Commander 원격 관제 콘솔 범용 인바운드 방화벽 허용 규칙"""; Flags: runhidden; Tasks: openremoteport
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name=""WinPurify Central Commander Outbound"" dir=out action=allow program=""""{app}\WinPurifyCommander.exe"""" enable=yes profile=any description=""WinPurify Central Commander 원격 관제 콘솔 범용 아웃바운드 방화벽 허용 규칙"""; Flags: runhidden; Tasks: openremoteport

; =====================================================================
; [3] 메인 클라이언트 애플리케이션 실행
; =====================================================================
Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent runascurrentuser

[UninstallRun]
; =====================================================================
; [1] 언인스톨 시 원격 에이전트 윈도우 서비스 안전 중지 및 서비스 목록에서 제거
; =====================================================================
Filename: "{sys}\sc.exe"; Parameters: "stop WinPurifyAgentService"; Flags: runhidden
Filename: "{sys}\sc.exe"; Parameters: "delete WinPurifyAgentService"; Flags: runhidden

; =====================================================================
; [2] 언인스톨 시 사전에 등록된 Windows 방화벽 예외 규칙 및 포트 삭제
; =====================================================================
; 원격 관제 포트 삭제 (TCP 9870 & UDP 9871)
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Remote Port TCP 9870"""; Flags: runhidden
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Remote Port TCP 9870 Outbound"""; Flags: runhidden
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Remote Discovery UDP 9871"""; Flags: runhidden
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Remote Discovery UDP 9871 Outbound"""; Flags: runhidden

; 애플리케이션 규칙 삭제
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""{#MyAppName} v{#MyAppVersion}"""; Flags: runhidden
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""{#MyAppName} v{#MyAppVersion} (Outbound)"""; Flags: runhidden
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""{#MyAppName} Main Client"""; Flags: runhidden
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""{#MyAppName} Main Client Outbound"""; Flags: runhidden

Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Central Commander v{#MyAppVersion}"""; Flags: runhidden
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Central Commander v{#MyAppVersion} (Outbound)"""; Flags: runhidden
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Central Commander"""; Flags: runhidden
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""WinPurify Central Commander Outbound"""; Flags: runhidden

[UninstallDelete]
Type: files; Name: "{app}\installed.tag"
Type: filesandordirs; Name: "{app}\runtimes"
Type: filesandordirs; Name: "{app}\logs"
Type: files; Name: "{app}\*.log"
Type: files; Name: "{app}\*.bak"
Type: files; Name: "{app}\*.tmp"
Type: dirifempty; Name: "{app}"

[Code]
// =====================================================================
// WinPurify Pro - Existing Application Detection & Uninstall/Overwrite Prompt
// 기존 설치본 감지 시 사용자에게 [삭제 후 클린 설치] 또는 [덮어쓰기 업데이트] 확인
// =====================================================================

var
  G_ExistingAppPath: String;
  G_ExistingVersion: String;
  G_ExistingUninstaller: String;
  G_PromptAlreadyHandled: Boolean;

// 64-bit / 32-bit Registry View query helper
function GetUninstallRegistryString(const SubKey, ValueName: String; var OutValue: String): Boolean;
begin
  Result := False;
  OutValue := '';

  if IsWin64 then
  begin
    if RegQueryStringValue(HKLM64, SubKey, ValueName, OutValue) then
    begin
      Result := True;
      Exit;
    end;
    if RegQueryStringValue(HKCU64, SubKey, ValueName, OutValue) then
    begin
      Result := True;
      Exit;
    end;
  end;

  if RegQueryStringValue(HKLM32, SubKey, ValueName, OutValue) then
  begin
    Result := True;
    Exit;
  end;
  if RegQueryStringValue(HKCU32, SubKey, ValueName, OutValue) then
  begin
    Result := True;
    Exit;
  end;
end;

// Detect existing installation via Registry or default install directory
function DetectExistingInstallation(var InstallPath, Version, UninstallerExe: String): Boolean;
var
  SubKey, UninstStr, AppPathStr, VerStr, CleanExe: String;
  DefaultDir: String;
begin
  Result := False;
  InstallPath := '';
  Version := '';
  UninstallerExe := '';

  // AppId: {6904B914-A0E9-4BFA-BA5F-BCB0BEA7EF0F}
  SubKey := 'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{6904B914-A0E9-4BFA-BA5F-BCB0BEA7EF0F}_is1';

  // 1. Registry detection (64-bit & 32-bit HKLM / HKCU)
  if GetUninstallRegistryString(SubKey, 'UninstallString', UninstStr) then
  begin
    CleanExe := RemoveQuotes(UninstStr);
    GetUninstallRegistryString(SubKey, 'Inno Setup: App Path', AppPathStr);
    GetUninstallRegistryString(SubKey, 'DisplayVersion', VerStr);

    if FileExists(CleanExe) then
    begin
      UninstallerExe := CleanExe;
      if AppPathStr <> '' then
        InstallPath := AppPathStr
      else
        InstallPath := ExtractFilePath(CleanExe);
      Version := VerStr;
      Result := True;
      Exit;
    end;
  end;

  // 2. File-system check in default install path
  DefaultDir := ExpandConstant('{autopf}\{#MyAppName}');
  if FileExists(DefaultDir + '\{#MyAppExeName}') then
  begin
    InstallPath := DefaultDir;
    if FileExists(DefaultDir + '\unins000.exe') then
      UninstallerExe := DefaultDir + '\unins000.exe';
    Result := True;
    Exit;
  end;
end;

// Ask user whether to Uninstall (Clean Install) or Overwrite (Update)
function AskUninstallOrOverwrite(const InstallPath, Version, UninstallerExe: String): Boolean;
var
  Prompt: String;
  VerInfo: String;
  UserChoice: Integer;
  ResultCode: Integer;
  WaitCount: Integer;
  IsCleanInstallParam: Boolean;
begin
  // 무인 설치(/SILENT 또는 /VERYSILENT) 모드일 경우 대화상자를 띄우지 않고 자동 처리
  if WizardSilent then
  begin
    G_PromptAlreadyHandled := True;
    IsCleanInstallParam := (ExpandConstant('{param:clean|0}') = '1') or (ExpandConstant('{param:clean|false}') = 'true');

    // /CLEAN 옵션 지정 시 기존 언인스톨러를 무인 모드로 실행 후 클린 설치
    if IsCleanInstallParam and (UninstallerExe <> '') and FileExists(UninstallerExe) then
    begin
      Exec(UninstallerExe, '/VERYSILENT /NORESTART /SUPPRESSMSGBOXES', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
      WaitCount := 0;
      while FileExists(UninstallerExe) and (WaitCount < 20) do
      begin
        Sleep(250);
        Inc(WaitCount);
      end;
    end;

    // 기본 무인 설치는 안전 덮어쓰기(업데이트) 모드로 통과
    Result := True;
    Exit;
  end;

  if Version <> '' then
    VerInfo := ' (v' + Version + ')'
  else
    VerInfo := '';

  if ActiveLanguage = 'korean' then
  begin
    Prompt :=
      '기존 버전의 {#MyAppName}' + VerInfo + '가 시스템에 이미 설치되어 있습니다.' + #13#10 +
      '설치 경로: ' + InstallPath + #13#10#13#10 +
      '설치 진행 방식을 선택해 주십시오:' + #13#10#13#10 +
      '• [예(Y)]    : 기존 앱을 완전히 삭제(언인스톨)한 후 새로 클린 설치합니다. (권장)' + #13#10 +
      '• [아니오(N)] : 기존 앱을 삭제하지 않고 새 버전으로 덮어쓰기(업데이트)합니다.' + #13#10 +
      '• [취소]     : 설치를 중단하고 마법사를 종료합니다.';
  end
  else
  begin
    Prompt :=
      'A previous version of {#MyAppName}' + VerInfo + ' is already installed on this system.' + #13#10 +
      'Install Path: ' + InstallPath + #13#10#13#10 +
      'Please select how you would like to proceed:' + #13#10#13#10 +
      '• [Yes]    : Completely uninstall the previous version, then perform a clean install. (Recommended)' + #13#10 +
      '• [No]     : Overwrite (update) the existing installation with new files.' + #13#10 +
      '• [Cancel] : Abort and exit setup.';
  end;

  UserChoice := MsgBox(Prompt, mbConfirmation, MB_YESNOCANCEL);

  if UserChoice = IDYES then
  begin
    // [예]: 기존 앱 완전 삭제 후 클린 설치
    G_PromptAlreadyHandled := True;
    if (UninstallerExe <> '') and FileExists(UninstallerExe) then
    begin
      // 기존 언인스톨러를 무인 모드로 실행하여 안전하게 서비스 중지 및 기존 파일 제거
      if not Exec(UninstallerExe, '/SILENT /NORESTART /SUPPRESSMSGBOXES', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
      begin
        if ActiveLanguage = 'korean' then
          MsgBox('기존 버전 언인스톨러 실행에 실패했습니다. 설치를 중단합니다.', mbError, MB_OK)
        else
          MsgBox('Failed to run previous uninstaller. Setup will abort.', mbError, MB_OK);
        Result := False;
        Exit;
      end;

      // 프로세스 종료 및 파일 핸들 정리 대기
      WaitCount := 0;
      while FileExists(UninstallerExe) and (WaitCount < 20) do
      begin
        Sleep(250);
        Inc(WaitCount);
      end;

      if ResultCode <> 0 then
      begin
        if ActiveLanguage = 'korean' then
          MsgBox('기존 버전 삭제 작업이 중단되었거나 오류가 발생했습니다. 설치를 취소합니다.', mbError, MB_OK)
        else
          MsgBox('Uninstallation was cancelled or failed. Setup will abort.', mbError, MB_OK);
        Result := False;
        Exit;
      end;
    end;
    Result := True;
  end
  else if UserChoice = IDNO then
  begin
    // [아니오]: 덮어쓰기(업데이트) 진행
    G_PromptAlreadyHandled := True;
    Result := True;
  end
  else
  begin
    // [취소]: 설치 중단
    Result := False;
  end;
end;

// Setup Initialization event
function InitializeSetup(): Boolean;
begin
  G_PromptAlreadyHandled := False;
  G_ExistingAppPath := '';
  G_ExistingVersion := '';
  G_ExistingUninstaller := '';

  if DetectExistingInstallation(G_ExistingAppPath, G_ExistingVersion, G_ExistingUninstaller) then
  begin
    Result := AskUninstallOrOverwrite(G_ExistingAppPath, G_ExistingVersion, G_ExistingUninstaller);
  end
  else
  begin
    Result := True;
  end;
end;

// Wizard initialization event
procedure InitializeWizard();
begin
  // 기존 설치 경로가 감지된 경우 설치 대상 경로를 유지
  if G_ExistingAppPath <> '' then
  begin
    WizardForm.DirEdit.Text := G_ExistingAppPath;
  end;
end;

// Prepare to install event: 파일 잠금 방지용 서비스 안전 중지
function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ResultCode: Integer;
begin
  Result := '';
  // 덮어쓰기 시 WinPurifyAgentService가 실행 중이면 파일 잠금이 발생하므로 사전 중지
  Exec('sc.exe', 'stop WinPurifyAgentService', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Sleep(500);
end;

// Next button click event (Directory Selection Page fallback check)
function NextButtonClick(CurPageID: Integer): Boolean;
var
  SelectedDir, UninstPath: String;
begin
  Result := True;

  // 디렉토리 선택 페이지 통과 시 추가 확인 (InitializeSetup 단계에서 감지되지 않은 커스텀 경로인 경우)
  if (CurPageID = wpSelectDir) and (not G_PromptAlreadyHandled) then
  begin
    SelectedDir := WizardDirValue;
    if FileExists(SelectedDir + '\{#MyAppExeName}') or FileExists(SelectedDir + '\unins000.exe') then
    begin
      UninstPath := SelectedDir + '\unins000.exe';
      if not FileExists(UninstPath) then
        UninstPath := '';
      Result := AskUninstallOrOverwrite(SelectedDir, '', UninstPath);
    end;
  end;
end;

// =====================================================================
// 원격 에이전트 윈도우 서비스(WinPurifyAgentService) 안전 등록/해제 파이프라인
// =====================================================================
procedure RegisterAgentService();
var
  AppExe, BinPathArg, ScExe: String;
  ResultCode: Integer;
  IsNoService: Boolean;
  IsOpenRemotePortSelected: Boolean;
  IsNoOpenPortParam: Boolean;
begin
  // 1. 원격포트 열기 옵션(openremoteport - 기본 체크) 선택 여부 확인
  IsOpenRemotePortSelected := WizardIsTaskSelected('openremoteport');
  IsNoOpenPortParam := (ExpandConstant('{param:noopenport|0}') = '1') or (ExpandConstant('{param:noopenport|false}') = 'true');

  if (not IsOpenRemotePortSelected) or IsNoOpenPortParam then
  begin
    // 사용자가 원격포트 열기 옵션을 해제한 경우 서비스 등록을 건너뜁니다.
    Exit;
  end;

  // /NOSERVICE 커맨드라인 매개변수 확인 (지정 시 서비스 등록 건너뜀)
  IsNoService := (ExpandConstant('{param:noservice|0}') = '1') or (ExpandConstant('{param:noservice|false}') = 'true');
  if IsNoService then Exit;

  AppExe := ExpandConstant('{app}\{#MyAppExeName}');
  ScExe := ExpandConstant('{sys}\sc.exe');

  if FileExists(AppExe) and FileExists(ScExe) then
  begin
    // 1. 기존 잔여 서비스 중지 및 삭제 시도 (클린 상태 보장)
    Exec(ScExe, 'stop WinPurifyAgentService', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Exec(ScExe, 'delete WinPurifyAgentService', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Sleep(300);

    // 2. 서비스 생성 (경로 인용부호 완벽 이스케이프: binPath= "\"C:\...\WinPurifyPro.exe\" --service")
    BinPathArg := 'binPath= "\"' + AppExe + '\" --service"';
    Exec(ScExe, 'create WinPurifyAgentService ' + BinPathArg + ' start= auto DisplayName= "WinPurify Pro Remote Agent Service"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);

    // 3. 서비스 설명 등록
    Exec(ScExe, 'description WinPurifyAgentService "WinPurify Pro 중앙 관제(Central Commander) 원격 최적화 및 시스템 유지보수 명령 수신 백그라운드 서비스"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);

    // 4. 서비스 즉시 시작
    Exec(ScExe, 'start WinPurifyAgentService', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;
end;

procedure UnregisterAgentService();
var
  ScExe: String;
  ResultCode: Integer;
begin
  ScExe := ExpandConstant('{sys}\sc.exe');
  if FileExists(ScExe) then
  begin
    Exec(ScExe, 'stop WinPurifyAgentService', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
    Exec(ScExe, 'delete WinPurifyAgentService', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  end;
end;

// 설치 완료 단계(ssPostInstall)에서 서비스 및 무인 자동실행(/AUTORUN) 처리
procedure CurStepChanged(CurStep: TSetupStep);
var
  AppExe: String;
  ResultCode: Integer;
  IsAutoRun: Boolean;
begin
  if CurStep = ssPostInstall then
  begin
    RegisterAgentService();

    // 무인 설치 환경에서 /AUTORUN 매개변수가 지정된 경우 앱 자동 실행
    IsAutoRun := (ExpandConstant('{param:autorun|0}') = '1') or (ExpandConstant('{param:autorun|false}') = 'true');
    if IsAutoRun then
    begin
      AppExe := ExpandConstant('{app}\{#MyAppExeName}');
      if FileExists(AppExe) then
      begin
        Exec(AppExe, '', '', SW_SHOW, ewNoWait, ResultCode);
      end;
    end;
  end;
end;

// 언인스톨 단계에서 서비스 중지 및 삭제
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usUninstall then
  begin
    UnregisterAgentService();
  end;
end;



