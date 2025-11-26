# Code Signing (Overview)

This document explains the recommended approach to sign your installer or MSIX package for production distribution.

Why sign?
- Code signing establishes publisher identity and reduces SmartScreen warnings.

Test signing
- Visual Studio can generate a temporary test certificate for development. Tests will still show an untrusted publisher unless the certificate is installed on the test machine's Trusted Root store.

Production signing
- Purchase a code signing certificate (PFX) from a trusted CA.
- Keep your private key secure; store PFX in CI secrets if signing in CI.

Signing commands (examples)
- Sign an EXE or installer using signtool:

```powershell
signtool sign /fd SHA256 /a /f "path\to\cert.pfx" /p "<password>" "AUTOCAP-Setup.exe"
```

- Sign an MSIX/Appx package:

```powershell
signtool sign /fd SHA256 /a /f "path\to\cert.pfx" /p "<password>" "output.appx"
```

CI tips
- Store the PFX as a repository or organization secret and use actions that can sign artifacts using the secret (be careful with exposure).
- Alternatively, sign artifacts on a trusted build machine and upload the signed artifact to releases.
