# Release Notes - AUTOCAP

Version: {version}
Date: {date}

Highlights
- 

Bug fixes
- 

Known issues
- 

How to install
- Download the installer from the release assets and run the EXE (Windows).

Notes for packagers
- The publish artifact is created by `dotnet publish AUTOCAP.App -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true`.
- Inno Setup script is `packaging/autocap_installer.iss`.
