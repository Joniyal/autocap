# MSIX Packaging Guide (Overview)

This document explains how to create an MSIX package for AUTOCAP on Windows. MSIX provides a modern packaging format for Windows apps with clean install/uninstall semantics and Store readiness.

Prerequisites
- Windows 10 or later
- Visual Studio 2022/2023 with "Universal Windows Platform development" and "Windows Application Packaging Project" workload
- Optional: a code-signing certificate (for production installs)

Quick steps (Visual Studio)
1. Open the solution in Visual Studio.
2. Add a new project: `Windows Application Packaging Project`.
3. In the Packaging project, right-click References -> Add Reference -> Projects -> select `AUTOCAP.App`.
4. Edit `Package.appxmanifest` to set display name, publisher, and capabilities.
5. Right-click Packaging project -> Publish -> Create App Packages... and follow the wizard.

CLI approach (advanced)
- Use the MSIX Packaging Tool or `makeappx.exe`/`signtool.exe`. The Visual Studio Packaging Project is the recommended and simpler approach.

Notes
- For test installs, Visual Studio can create a temporary self-signed test certificate.
- For production distribution you should sign with a trusted certificate and consider Store distribution for automatic updates.
