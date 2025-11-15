# AUTOCAP - Complete Installation & Setup Guide

Welcome to **AUTOCAP**, the offline AI subtitle generator! This guide walks you through installation, setup, and first use on your platform.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Windows Installation](#windows-installation)
3. [Android Installation](#android-installation)
4. [iOS Installation](#ios-installation)
5. [macOS Installation](#macos-installation)
6. [Linux Installation](#linux-installation)
7. [First Run & Testing](#first-run--testing)
8. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### All Platforms
- **.NET 8 SDK** — Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/)
- **AUTOCAP Source Code** — Clone from GitHub or extract downloaded archive

### Verify .NET Installation
```bash
dotnet --version
# Should output: 8.0.x or higher
```

### Check System Requirements
- **Minimum RAM**: 2 GB
- **Disk Space**: 1 GB (including Vosk model)
- **Microphone**: Required for all platforms
- **Internet**: Required for initial model download (~50 MB)

---

## Windows Installation

### Step 1: Install .NET 8 SDK

**Option A: Using winget (Recommended)**
```powershell
winget install Microsoft.DotNet.SDK.8
```

**Option B: Manual Download**
1. Visit [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
2. Download ".NET 8 SDK"
3. Run installer and follow prompts

**Option C: Using Chocolatey**
```powershell
choco install dotnet-sdk
```

### Step 2: Get AUTOCAP Source

```powershell
# Clone from GitHub
git clone https://github.com/yourusername/AUTOCAP.git
cd AUTOCAP

# OR: Extract ZIP file if downloaded
# Extract AUTOCAP.zip to desired location
# Open PowerShell in that directory
```

### Step 3: Restore NuGet Packages

```powershell
cd AUTOCAP
dotnet restore
```

**Expected Output:**
```
Restoring AUTOCAP.Core.csproj
Restoring AUTOCAP.App.csproj
...
Restore completed in X seconds.
```

### Step 4: Build

```powershell
# Build for Windows 10/11
dotnet build -f net8.0-windows10.0.19041.0 -c Release
```

**Expected Output:**
```
Build succeeded after X seconds.
```

### Step 5: Run AUTOCAP

```powershell
dotnet run -f net8.0-windows10.0.19041.0 --project AUTOCAP.App\AUTOCAP.App.csproj
```

**First Run:**
- App window opens with dark UI
- "Status: Initialize model to begin"
- Click "Initialize/Download Model"
- Wait 1-2 minutes for download

### Post-Installation

**Optional: Install WASAPI Loopback Driver** (for system audio capture)
- Download: [VB-Audio Virtual Cable](https://vb-audio.com/Cable/)
- Or: Install BlackHole (if also using macOS)

---

## Android Installation

### Prerequisites
- **Android SDK**: API 21+ (minimum), API 29+ for system audio
- **Android Device or Emulator**: With microphone
- **Android Studio**: For emulator and SDK management
- **ADB (Android Debug Bridge)**: For device connection

### Step 1: Install Android Development Tools

**Windows with Android Studio:**
1. Download [Android Studio](https://developer.android.com/studio)
2. Run installer
3. Go through "Android SDK Setup Wizard"
4. Install minimum SDK 21, recommended SDK 12+

**Verify Installation:**
```bash
adb --version
# Should output: Android Debug Bridge version X.X.X
```

### Step 2: Setup Device or Emulator

**Physical Device:**
1. Connect Android phone via USB
2. Enable "Developer mode": Settings → About Phone → Tap "Build Number" 7 times
3. Go back to Settings → Developer Options → Enable USB Debugging
4. Verify connection:
   ```bash
   adb devices
   # Should list your device
   ```

**Android Emulator:**
1. Open Android Studio → Device Manager
2. Create or select virtual device (Android 12+ recommended)
3. Start the emulator
4. Verify:
   ```bash
   adb devices
   # Should list emulator (e.g., "emulator-5554")
   ```

### Step 3: Get AUTOCAP Source

```bash
git clone https://github.com/yourusername/AUTOCAP.git
cd AUTOCAP
dotnet restore
```

### Step 4: Install .NET for Android

```bash
dotnet workload restore
```

This downloads Android workload for .NET (may take several minutes).

### Step 5: Build & Deploy

```bash
# Build
dotnet build -f net8.0-android -c Release

# Run on connected device/emulator
dotnet run -f net8.0-android --project AUTOCAP.App\AUTOCAP.App.csproj
```

**Expected:**
- App installs and launches on device/emulator
- Permissions dialog appears (Microphone, System Alert Window)
- Grant all permissions when prompted

### Step 6: Permissions Setup

**In AUTOCAP App:**
1. Tap "Initialize/Download Model" → Wait for download
2. Grant microphone permission when prompted
3. For system audio overlay: Settings → Apps → Special App Access → Display Over Other Apps → Allow AUTOCAP

### Post-Installation

Test features:
1. Tap "Start Capture"
2. Speak or play audio from speaker
3. Watch transcription update in real-time
4. Tap "Export as SRT" to save

---

## iOS Installation

### Prerequisites
- **macOS** with Xcode (iOS development requires Mac)
- **Xcode**: Download from App Store or [developer.apple.com](https://developer.apple.com)
- **Apple Developer Account** (free tier okay for simulator)
- **iPhone Simulator**: Part of Xcode (can test on simulator)

### Step 1: Install Xcode

```bash
# Using App Store (easiest)
# Search for "Xcode" and click Install

# OR using command line
xcode-select --install
```

### Step 2: Get AUTOCAP Source

```bash
git clone https://github.com/yourusername/AUTOCAP.git
cd AUTOCAP
dotnet restore
```

### Step 3: Install .NET for iOS

```bash
dotnet workload restore
```

This downloads iOS workload for .NET.

### Step 4: Build for Simulator

```bash
# Build for iOS simulator
dotnet build -f net8.0-ios -c Release

# Run on simulator
dotnet run -f net8.0-ios --project AUTOCAP.App\AUTOCAP.App.csproj
```

**Expected:**
- Xcode simulator starts
- App launches
- Microphone permission prompt (tap "Allow")
- Note on screen: "iOS cannot capture internal audio"

### Step 5: Test on Physical Device (Optional)

**Requirements:**
- Apple Developer Account with paid subscription ($99/year)
- iPhone connected via USB
- Xcode provisioning profile configured

```bash
# Build for physical device
dotnet build -f net8.0-ios -c Release -p:RuntimeIdentifier=ios-arm64

# Deploy
dotnet run -f net8.0-ios --project AUTOCAP.App\AUTOCAP.App.csproj
```

### iOS-Specific Limitations

⚠️ **iOS does not allow internal audio capture** (App Store policy)
- **Solution**: Use microphone capture
- **Workaround**: Route audio to speaker + position mic near speaker
- **Advanced**: Use external loopback hardware

See [PLATFORM_LIMITATIONS.md](PLATFORM_LIMITATIONS.md#ios---internal-audio-capture-restriction) for details.

---

## macOS Installation

### Prerequisites
- **macOS 10.13+**
- **Xcode**: For MacCatalyst development
- **Homebrew**: For easy package installation (optional)
- **BlackHole** (optional): For system audio capture

### Step 1: Install Xcode

```bash
# Using App Store
# Search "Xcode" → Install

# Using command line
xcode-select --install

# Verify
xcode-select -p
# Should output: /Applications/Xcode.app/Contents/Developer
```

### Step 2: Install BlackHole (Optional, for System Audio)

```bash
# Using Homebrew
brew install blackhole-2ch

# OR: Manual download
# Visit: https://existential.audio/blackhole/
```

**Configure BlackHole:**
1. Open System Preferences → Sound
2. Select "BlackHole 2ch" as Output device
3. Relaunch AUTOCAP

### Step 3: Get AUTOCAP Source

```bash
git clone https://github.com/yourusername/AUTOCAP.git
cd AUTOCAP
dotnet restore
```

### Step 4: Install .NET for MacCatalyst

```bash
dotnet workload restore
```

### Step 5: Build & Run

```bash
# Build for MacCatalyst
dotnet build -f net8.0-maccatalyst -c Release

# Run
dotnet run -f net8.0-maccatalyst --project AUTOCAP.App\AUTOCAP.App.csproj
```

**Expected:**
- Native macOS app launches
- Model manager available
- Microphone capture works immediately
- System audio works if BlackHole installed

---

## Linux Installation

### Prerequisites
- **.NET 8 SDK**: Per your Linux distribution
- **PulseAudio or PipeWire**: Audio system (usually pre-installed)
- **Git**: For cloning source

### Step 1: Install .NET 8 SDK

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install dotnet-sdk-8.0
```

**Fedora:**
```bash
sudo dnf install dotnet-sdk-8.0
```

**Arch Linux:**
```bash
sudo pacman -S dotnet-sdk
```

**Verify:**
```bash
dotnet --version
# Should output: 8.0.x or higher
```

### Step 2: Ensure Audio System

```bash
# Check PulseAudio
pulseaudio --version

# OR check PipeWire
pipewire --version

# If neither installed (rare)
sudo apt install pulseaudio  # or: pipewire
```

### Step 3: Get AUTOCAP Source

```bash
git clone https://github.com/yourusername/AUTOCAP.git
cd AUTOCAP
dotnet restore
```

### Step 4: Build

```bash
dotnet build -c Release
```

### Step 5: Run

```bash
# Generic Linux run (uses base .NET 8)
dotnet run --project AUTOCAP.App/AUTOCAP.App.csproj

# OR: Specify desktop target if available
dotnet run -f net8.0-linux-x64 --project AUTOCAP.App/AUTOCAP.App.csproj
```

**Expected:**
- App window opens with GTK/native backend
- Model download available
- Microphone capture works immediately

### Troubleshooting Audio

```bash
# List audio sources
pactl list sources short

# Check default microphone
pactl get-default-source

# If issues, try PipeWire
systemctl --user restart pipewire
```

---

## First Run & Testing

### Complete First Run (All Platforms)

1. **Launch AUTOCAP**
   - Windows: `dotnet run -f net8.0-windows10.0.19041.0 --project AUTOCAP.App\AUTOCAP.App.csproj`
   - Android: `dotnet run -f net8.0-android --project AUTOCAP.App\AUTOCAP.App.csproj`
   - iOS: `dotnet run -f net8.0-ios --project AUTOCAP.App\AUTOCAP.App.csproj`
   - macOS: `dotnet run -f net8.0-maccatalyst --project AUTOCAP.App\AUTOCAP.App.csproj`
   - Linux: `dotnet run --project AUTOCAP.App/AUTOCAP.App.csproj`

2. **Grant Permissions**
   - Microphone access
   - System alert window (Android)
   - Other OS-specific prompts

3. **Initialize Model**
   - Tap "Initialize/Download Model"
   - Status shows: "Downloading..."
   - Wait 1-2 minutes (~50 MB download)
   - Completion message appears

4. **Test Microphone Capture**
   - Tap "Start Capture"
   - Speak: "Hello world, this is a test"
   - Check live transcription display
   - Should see approximate text

5. **Test Export**
   - After capturing text
   - Tap "Export as SRT"
   - Confirmation message appears
   - File saved to app cache directory

6. **Explore Settings**
   - Tap "Settings" button
   - Adjust font size, delay, opacity
   - Check privacy notice

7. **View Help**
   - Tap "Help" button
   - Read platform limitations
   - Check FAQ section

### Expected Results After First Run

✅ App launches without errors
✅ Model downloads successfully
✅ Microphone permission granted
✅ Live transcription captures speech
✅ Export creates valid SRT file
✅ Settings adjustable
✅ Help accessible

---

## Troubleshooting

### "Model not found" Error

**Symptom:** App says "Model not available"

**Solution:**
1. Tap "Initialize/Download Model"
2. Wait for download (check internet connection)
3. Verify disk space (need 1 GB minimum)
4. Retry download

**Manual Download:**
```bash
# Download model manually
curl -o vosk-model.zip "https://alphacephei.com/vosk/models/vosk-model-small-en-us-0.15.zip"

# Extract
unzip vosk-model.zip

# Place in app cache:
# Windows: %APPDATA%\Local\AUTOCAP\vosk_models\
# Linux: ~/.cache/AUTOCAP/vosk_models/
# macOS: ~/Library/Caches/com.autocap.app/vosk_models/
```

### "No Audio Captured" on Windows

**Symptom:** Status says "No audio source" or transcription empty

**Causes:**
- WASAPI loopback not available
- Microphone not selected
- Audio permissions denied

**Solution:**
1. Check status bar for audio source
2. Install virtual loopback driver: [VB-Audio Cable](https://vb-audio.com/Cable/)
3. Verify microphone works: Settings → Sound → Input device
4. Grant app permissions in Windows Settings

### "Permission Denied" on Android

**Symptom:** App crashes or no audio

**Solution:**
1. Settings → Apps → AUTOCAP → Permissions
2. Enable: Microphone
3. Enable: Display over other apps (for overlay)
4. Restart app

### "Can't Capture on iOS"

**Symptom:** No audio from other apps, silent transcription

**This is expected!** iOS doesn't allow internal audio capture.

**Solution:**
1. Use microphone capture only
2. Play audio through speaker (not earpiece)
3. Position iPhone mic near speaker
4. Speak or route audio accordingly

**OR use workaround:**
- Connect external loopback hardware
- Use AirPlay to Mac + run AUTOCAP on Mac

### "Build Failed" Error

**Symptom:** `dotnet build` fails with compilation error

**Solutions:**
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build -c Release

# Check .NET version
dotnet --version

# Update if needed
dotnet sdk update

# Check for typos
# Re-read error messages carefully
```

### "Model Download Stuck" or Timeout

**Symptom:** Download progress not moving, timeout after 10 minutes

**Solution:**
1. Check internet connection
2. Try different network (WiFi vs Mobile data)
3. Restart app
4. Download manually (see "Model not found" section above)

### "CPU Usage Very High" or "Battery Draining Fast"

**Symptom:** App consuming 50%+ CPU, battery depleting quickly (mobile)

**Solutions:**
1. Enable "Low CPU Mode" in Settings
2. Close background apps
3. Lower audio sample rate if possible
4. Use smaller Vosk model (default is already optimized)

### Audio Capture Works but No Transcription

**Symptom:** Audio captured but transcript stays empty

**Likely Causes:**
- Model not initialized
- Vosk model not downloaded
- Audio format mismatch

**Solution:**
1. Verify model initialized: Status should say model name
2. Re-download model if corrupted
3. Check audio source setting
4. Restart app

---

## File Locations

### Windows
- **App Cache**: `%APPDATA%\Local\AUTOCAP\`
- **Models**: `%APPDATA%\Local\AUTOCAP\vosk_models\`
- **Database**: `%APPDATA%\Local\AUTOCAP\autocap.db`
- **Exports**: `%APPDATA%\Local\AUTOCAP\` (default)

### Android
- **App Cache**: `/data/data/com.autocap.app/cache/`
- **Models**: `/data/data/com.autocap.app/cache/vosk_models/`
- **Database**: `/data/data/com.autocap.app/cache/autocap.db`

### iOS
- **App Cache**: `~/Library/Caches/com.autocap.app/`
- **Models**: `~/Library/Caches/com.autocap.app/vosk_models/`
- **Database**: `~/Library/Caches/com.autocap.app/autocap.db`

### macOS
- **App Cache**: `~/Library/Caches/com.autocap.app/`
- **Models**: `~/Library/Caches/com.autocap.app/vosk_models/`
- **Database**: `~/Library/Caches/com.autocap.app/autocap.db`

### Linux
- **App Cache**: `~/.cache/AUTOCAP/`
- **Models**: `~/.cache/AUTOCAP/vosk_models/`
- **Database**: `~/.cache/AUTOCAP/autocap.db`

---

## Next Steps

After successful installation:

1. **[Read the README](README.md)** — Full feature guide
2. **[Check Platform Limitations](PLATFORM_LIMITATIONS.md)** — Platform-specific info
3. **[Try Advanced Features](README.md#usage-guide)** — Sessions, export, settings
4. **[Explore the Code](STRUCTURE.md)** — If interested in development
5. **[Contribute!](CONTRIBUTING.md)** — Help improve AUTOCAP

---

## Getting Help

| Issue | Resource |
|-------|----------|
| Build problems | This guide's troubleshooting section |
| Feature questions | [README.md FAQ](README.md#faq) |
| Platform issues | [PLATFORM_LIMITATIONS.md](PLATFORM_LIMITATIONS.md) |
| Development | [CONTRIBUTING.md](CONTRIBUTING.md) |
| General help | [GitHub Discussions](https://github.com/yourusername/AUTOCAP/discussions) |

---

## Summary

✅ **You now have AUTOCAP installed and ready to use!**

- **Windows**: Open app, download model, start capturing
- **Android**: Install APK, grant permissions, download model
- **iOS**: Run from Xcode simulator or device, use microphone
- **macOS**: Build and run, optionally install BlackHole
- **Linux**: Build and run, use PulseAudio/PipeWire

**Start with microphone capture**, then explore advanced features like system audio (where supported).

**Happy captioning! 🎯**

For issues or questions, see [PLATFORM_LIMITATIONS.md](PLATFORM_LIMITATIONS.md) or open a GitHub issue.

---

*Last Updated: November 2025*
*AUTOCAP v1.0 Installation Guide*
