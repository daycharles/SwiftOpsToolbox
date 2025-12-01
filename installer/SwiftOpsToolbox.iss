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
Source: "c:\users\cd104535\source\repos\swiftopstoolbox\swiftopstoolbox\bin\release\net8.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "c:\users\cd104535\source\repos\swiftopstoolbox\docs\*"; DestDir: "{app}\docs"; Flags: ignoreversion recursesubdirs createallsubdirs

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; 
Name: "startmenuicon"; Description: "Create a Start Menu icon"; GroupDescription: "Additional icons:"; Flags: unchecked
Name: "runafterinstall"; Description: "Run SwiftOps Toolbox after installation"; GroupDescription: "Additional options:";
Name: "opendocs"; Description: "Open the user documentation after installation"; Flags: unchecked

[Icons]
Name: "{group}\SwiftOps Toolbox"; Filename: "{app}\SwiftOpsToolbox.exe"; Tasks: startmenuicon; IconFilename: "{app}\SwiftOpsToolboxLogoNoTextPNG.ico"
Name: "{group}\Uninstall SwiftOps Toolbox"; Filename: "{uninstallexe}"; Tasks: startmenuicon; IconFilename: "{app}\SwiftOpsToolboxLogoNoTextPNG.ico"
Name: "{commondesktop}\SwiftOps Toolbox"; Filename: "{app}\SwiftOpsToolbox.exe"; Tasks: desktopicon; IconFilename: "{app}\SwiftOpsToolboxLogoNoTextPNG.ico"

[Run]
Filename: "{app}\SwiftOpsToolbox.exe"; Description: "Launch SwiftOps Toolbox"; Flags: nowait postinstall skipifsilent; Tasks: runafterinstall
Filename: "{app}\docs\USER_DOCUMENTATION.md"; Description: "Open user documentation"; Flags: shellexec postinstall skipifsilent; Tasks: opendocs
