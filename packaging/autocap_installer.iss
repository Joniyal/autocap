[Setup]
; Installer package settings for AUTOCAP
AppName=AUTOCAP
AppVersion=0.1.1
DefaultDirName={pf}\AUTOCAP
DefaultGroupName=AUTOCAP
UninstallDisplayIcon={app}\AUTOCAP.exe
Compression=lzma
SolidCompression=yes
OutputBaseFilename=AUTOCAP-Setup
SetupIconFile=installer_icon.ico

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon";GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
; Copy all published files
Source: "{#src}"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\AUTOCAP"; Filename: "{app}\AUTOCAP.exe"
Name: "{commondesktop}\AUTOCAP"; Filename: "{app}\AUTOCAP.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\AUTOCAP.exe"; Description: "Launch AUTOCAP"; Flags: nowait postinstall skipifsilent
