; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "CCCScript"
#define MyAppVersion "1.0"
#define MyAppPublisher "Jerry Liang"
#define MyAppURL "http://www.google.com/q=CCCSCript"
#define MyAppExeName "CCCScript.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{94B53798-B1E5-4C08-91A0-1D4DA31DDCCF}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputBaseFilename=setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: mypAssociation; Description: "Associate ""ccc"" extension"; GroupDescription: File extensions:

[Registry]
Root: HKCR; Subkey: ".ccc"; ValueType: string; ValueName: ""; ValueData: "CCCScript"; Flags: uninsdeletevalue; Tasks: mypAssociation 
Root: HKCR; Subkey: "CCCScript\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\CCCScript.exe"" ""%1"""


[Files]
Source: "bin\Release\CCCScript.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\ScriptEngine.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\CCCScript.dll"; DestDir: "{app}"; Flags: ignoreversion

; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"


