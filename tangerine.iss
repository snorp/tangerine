[Setup]
AppName=Tangerine
AppVerName=Tangerine 0.2.6
DefaultDirName={pf}\Tangerine
DefaultGroupName=Tangerine
OutputBaseFilename=TangerineSetup
InfoBeforeFile=

[Dirs]
Name: "{app}\plugins"

[Files]
Source: "bin\Release\*.dll"; DestDir: "{app}"
Source: "bin\Release\*.exe"; DestDir: "{app}"
Source: "tangerine.ico"; DestDir: "{app}"
Source: "bin\Release\plugins\*.dll"; DestDir: "{app}\plugins"
Source: "PSetup.exe"; DestDir: "{app}"
Source: "PSetup.ini"; DestDir: "{app}"
Source: "C:\mDNSResponder-binary\Output\mDNSResponderSetup.exe"; DestDir: "{app}"

[Icons]
Name: "{group}\Tangerine Preferences"; IconFilename: "{app}\tangerine.ico"; Filename: "{app}\tangerine-preferences.exe"; WorkingDir: "{app}"
Name: "{group}\Uninstall Tangerine"; Filename: "{uninstallexe}"

[UninstallDelete]
Type: files; Name: "{userstartup}\tangerine.lnk"

[Code]
function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
begin
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

function InitilizeUninstall(): Boolean;
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{win}\system32\TaskKill.exe'), '/IM tangerine-daemon', '', SW_SHOW, ewWaitUntilTerminated, ResultCode)
  Exec(ExpandConstant('{win}\system32\TaskKill.exe'), '/IM tangerine-preferences', '', SW_SHOW, ewWaitUntilTerminated, ResultCode)
end;
