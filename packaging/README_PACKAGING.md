Packaging instructions

This folder contains helper files to create a Windows installer for AUTOCAP.

1) Publish the app

From repository root, produce a self-contained single-file publish (recommended):

```powershell
dotnet publish AUTOCAP.App -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true -o .\publish\win-x64
```

2) Build installer (Inno Setup)

- Install Inno Setup 6: https://jrsoftware.org
- Run the helper script (PowerShell):

```powershell
cd packaging
.\build_installer.ps1 -PublishDir "..\publish\win-x64"
```

This script will create a staging folder, generate an Inno Setup script with the publish contents and invoke `ISCC.exe` to compile the installer. The installer will be placed in `packaging/output`.

Notes
- Trimming (`PublishTrimmed=true`) reduces outputs but may break functionality. If you encounter runtime errors, publish without trimming.
- For production distribution, sign the installer with a code-signing certificate using `signtool`.
- MSIX packaging (recommended for MAUI/WinUI) is not automated here; use Visual Studio Packaging Project for MSIX creation.

If you want, I can:
- Add an MSIX Packaging Project to the repo.
- Compile the Inno Setup installer locally (requires Inno installed on this machine) and attach the resulting EXE to a release.

Note: This repository includes a small Inno Setup script and helper to produce an installer locally. Build artifacts will be placed in `packaging/output` by the helper script.
