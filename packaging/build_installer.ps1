# PowerShell helper to build Inno Setup installer (requires Inno Setup installed)
# Usage: Open PowerShell as Admin and run: .\build_installer.ps1 -PublishDir ".\\publish\\win-x64"
#
# Note: This script copies the publish output to a temporary staging folder and generates
# an Inno Setup script for compilation. It is intended for local use by developers.
param(
    [Parameter(Mandatory=$false)]
    [string]$PublishDir = "./publish/win-x64",

    [string]$InnoCompilerPath = "C:\\Program Files (x86)\\Inno Setup 6\\ISCC.exe",

    [string]$OutputDir = "./packaging/output"
)

$publishFull = Resolve-Path $PublishDir
if (-not (Test-Path $publishFull)) {
    Write-Error "Publish directory not found: $publishFull"
    exit 1
}

# Prepare a temporary copy for packaging
$staging = Join-Path $env:TEMP "autocap_publish"
if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
New-Item -ItemType Directory -Path $staging | Out-Null
Copy-Item -Path (Join-Path $publishFull "*") -Destination $staging -Recurse -Force

# Prepare Inno script: replace {#src} token with actual source path
$issTemplate = "./packaging/autocap_installer.iss"
$issWorking = "./packaging/autocap_installer_generated.iss"
$issText = Get-Content $issTemplate -Raw
$issText = $issText -replace "\{#src\}", $staging -replace '/', '\\'
Set-Content -Path $issWorking -Value $issText -Encoding UTF8

# Ensure output dir exists
if (-not (Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir | Out-Null }

# Run Inno Setup compiler
if (-not (Test-Path $InnoCompilerPath)) {
    Write-Error "Inno Setup compiler not found at $InnoCompilerPath. Install Inno Setup or set -InnoCompilerPath parameter."
    exit 1
}

& "$InnoCompilerPath" "$issWorking" /O"$OutputDir"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Installer built successfully. Output: $OutputDir"
} else {
    Write-Error "Inno Setup failed with exit code $LASTEXITCODE"
}
