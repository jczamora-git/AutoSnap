; Inno Setup Script for AutoSnap
; Supports dynamic version and directories passed via command-line defines:
; ISCC.exe /DAppVersion=1.0.0 /DSourceDir=".\artifacts\AutoSnap-v1.0.0-portable" /DOutputDir=".\artifacts" /DOutputBaseFilename="AutoSnap-Setup-v1.0.0" installer\AutoSnap.iss

#ifndef AppVersion
#define AppVersion "1.0.0"
#endif

#ifndef SourceDir
#define SourceDir "..\artifacts\AutoSnap-v" + AppVersion + "-portable"
#endif

#ifndef OutputDir
#define OutputDir "..\artifacts"
#endif

#ifndef OutputBaseFilename
#define OutputBaseFilename "AutoSnap-Setup-v" + AppVersion
#endif

[Setup]
AppId={{D37E749A-66F5-4B82-9B77-D8A375F6118D}
AppName=AutoSnap
AppVersion={#AppVersion}
AppVerName=AutoSnap v{#AppVersion}
AppPublisher=jczamora-git
AppPublisherURL=https://github.com/jczamora-git/AutoSnap
AppSupportURL=https://github.com/jczamora-git/AutoSnap/issues
AppUpdatesURL=https://github.com/jczamora-git/AutoSnap/releases
DefaultDirName={autopf}\AutoSnap
DefaultGroupName=AutoSnap
AllowNoIcons=yes
OutputDir={#OutputDir}
OutputBaseFilename={#OutputBaseFilename}
Compression=lzma2/max
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
UninstallDisplayIcon={app}\AutoSnap.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\AutoSnap"; Filename: "{app}\AutoSnap.exe"
Name: "{group}\{cm:UninstallProgram,AutoSnap}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\AutoSnap"; Filename: "{app}\AutoSnap.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\AutoSnap.exe"; Description: "{cm:LaunchProgram,AutoSnap}"; Flags: nowait postinstall skipifsilent
