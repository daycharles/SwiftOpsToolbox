; Inno Setup script for SwiftOpsToolbox
[Setup]
AppName=SwiftOps Toolbox
AppVersion=1.0
DefaultDirName={pf}\SwiftOps Toolbox
DefaultGroupName=SwiftOps Toolbox
OutputBaseFilename=SwiftOpsToolbox_Setup
Compression=lzma
SolidCompression=yes

[Files]
Source: "{#SourcePath}\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\SwiftOps Toolbox"; Filename: "{app}\SwiftOpsToolbox.exe"
Name: "{group}\Uninstall SwiftOps Toolbox"; Filename: "{uninstallexe}"
