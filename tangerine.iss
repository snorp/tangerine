[Setup]
AppId=Tangerine
AppName=Tangerine
AppVerName=Tangerine 0.3.0
DefaultDirName={pf}\Tangerine
DefaultGroupName=Tangerine
OutputBaseFilename=TangerineSetup
InfoBeforeFile=
UninstallDisplayIcon={app}\tangerine.ico

[Dirs]
Name: "{app}\plugins"

[Files]
Source: "bin\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\*.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "tangerine.ico"; DestDir: "{app}"
Source: "bin\Release\plugins\*.dll"; DestDir: "{app}\plugins"; Flags: ignoreversion
Source: "*.provider"; DestDir: "{app}\plugins"
Source: "PSetup.exe"; DestDir: "{app}"; Flags: dontcopy
Source: "PSetup.ini"; DestDir: "{app}"; Flags: dontcopy
Source: "C:\mDNSResponder-binary\Output\mDNSResponderSetup.exe"; DestDir: "{app}"; Flags: dontcopy

[Icons]
Name: "{group}\Tangerine Preferences"; IconFilename: "{app}\tangerine.ico"; Filename: "{app}\tangerine-preferences.exe"; WorkingDir: "{app}"
Name: "{group}\Uninstall Tangerine"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\tangerine-preferences.exe"; Description: "Configure Tangerine"; Flags: postinstall nowait

[UninstallDelete]
Type: files; Name: "{userstartup}\tangerine.lnk"

[Code]
var
  NeedStart: Boolean;
  
function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{win}\system32\TaskKill.exe'), '/F /IM tangerine-daemon.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
  NeedStart := ResultCode = 0
  
  Result := RegValueExists(HKEY_LOCAL_MACHINE, 'SOFTWARE\Microsoft\.NETFramework\policy\v2.0', '50727')
  if not Result then begin
    ExtractTemporaryFile('PSetup.exe')
    ExtractTemporaryFile('PSetup.ini')
    Exec(ExpandConstant('{tmp}\PSetup.exe'), '', '', SW_SHOW, ewWaitUntilTerminated, ResultCode)
  end
  
  if not FileExists(ExpandConstant('{sys}\dnssd.dll')) then begin
    ExtractTemporaryFile('mDNSResponderSetup.exe')
    Exec(ExpandConstant('{tmp}\mDNSResponderSetup.exe'), '', '', SW_SHOW, ewWaitUntilTerminated, ResultCode)
  end;

  Result := True
end;

function InitializeUninstall(): Boolean;
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{win}\system32\TaskKill.exe'), '/F /IM tangerine-daemon.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
  Exec(ExpandConstant('{win}\system32\TaskKill.exe'), '/F /IM tangerine-preferences.exe', '', SW_HIDE, ewWaitUntilTerminated, ResultCode)
  Result := True
end;

function DeinitializeSetup():
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{app}\tangerine-daemon.exe'), '', '', SW_HIDE, ewNoWait, ResultCode)
end;
