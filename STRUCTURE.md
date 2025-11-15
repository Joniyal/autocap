# AUTOCAP Repository Structure

Complete project structure for the AUTOCAP offline AI subtitle generator.

## Directory Tree

```
AUTOCAP/
│
├── AUTOCAP.sln                    # Main solution file
│
├── AUTOCAP.Core/                  # Shared business logic (class library)
│   ├── AUTOCAP.Core.csproj
│   ├── GlobalUsings.cs
│   │
│   ├── ASR/                       # Speech recognition
│   │   └── VoskRecognizer.cs      # Vosk wrapper (partial results, final results)
│   │
│   ├── Audio/                     # Audio capture abstractions
│   │   ├── IAudioCapture.cs       # Interface for all platforms
│   │   ├── WindowsAudioCapture.cs # WASAPI loopback
│   │   ├── MacAudioCapture.cs     # CoreAudio (mic fallback)
│   │   ├── AndroidAudioCapture.cs # AudioPlaybackCapture API
│   │   ├── iOSAudioCapture.cs     # AVAudioEngine (mic only)
│   │   └── LinuxAudioCapture.cs   # PulseAudio/PipeWire
│   │
│   ├── Subtitle/                  # Subtitle processing
│   │   └── SubtitleEngine.cs      # Buffer ASR → subtitle lines, export SRT/VTT
│   │
│   ├── Models/                    # Data models & persistence
│   │   ├── VoskModelManager.cs    # Download & cache Vosk models
│   │   └── SessionStorage.cs      # SQLite DB for subtitle sessions
│   │
│   └── Utilities/                 # Helper functions
│       ├── Diagnostics.cs         # System info, troubleshooting
│       └── RuntimeInfo.cs         # Platform detection
│
├── AUTOCAP.App/                   # MAUI cross-platform app
│   ├── AUTOCAP.App.csproj
│   ├── GlobalUsings.cs
│   ├── App.xaml                   # App-level styles & resources
│   ├── App.xaml.cs
│   ├── MainPage.xaml              # Main UI (capture, preview, export)
│   ├── MainPage.xaml.cs
│   ├── MauiProgram.cs             # MAUI initialization & DI
│   │
│   ├── ViewModels/
│   │   └── MainViewModel.cs       # MVVM for main page (ASR pipeline orchestration)
│   │
│   ├── Views/                     # Additional pages
│   │   ├── SettingsPage.xaml      # Settings (font, delay, permissions, privacy)
│   │   ├── SettingsPage.xaml.cs
│   │   ├── SessionsPage.xaml      # Saved sessions management
│   │   ├── SessionsPage.xaml.cs
│   │   ├── HelpPage.xaml          # Help, FAQ, platform limitations, credits
│   │   └── HelpPage.xaml.cs
│   │
│   ├── Services/                  # App-level services
│   │   └── Placeholder.cs         # Future: permission, notification, settings services
│   │
│   ├── Converters/                # XAML value converters
│   │   └── ValueConverters.cs     # IsCapturingColor, InvertedBool
│   │
│   ├── Platforms/                 # Platform-specific implementations
│   │   ├── Android/
│   │   │   ├── AndroidManifest.xml # Permissions, services, overlay
│   │   │   └── OverlayService.cs   # Foreground service + overlay (TODO)
│   │   │
│   │   ├── iOS/
│   │   │   ├── Info.plist          # Microphone permission, iOS limitations
│   │   │   └── iOSSubtitleFloatingView.cs # In-app floating view (TODO)
│   │   │
│   │   ├── Windows/
│   │   │   └── SubtitleOverlayWindow.cs   # Win32 transparent overlay (TODO)
│   │   │
│   │   ├── MacCatalyst/
│   │   │   └── MacOverlayWindow.cs # NSWindow overlay (TODO)
│   │   │
│   │   └── Linux/
│   │       └── LinuxOverlayWindow.cs # GTK/X11 overlay (TODO)
│   │
│   └── Resources/
│       ├── Fonts/                 # App fonts
│       ├── Images/                # App icons & assets
│       └── Styles/                # XAML styles & themes
│
├── AUTOCAP.Tests/                 # Unit tests (xUnit)
│   ├── AUTOCAP.Tests.csproj
│   └── SubtitleEngineTests.cs     # Tests for subtitle generation, export
│
├── Documentation/
│   ├── README.md                  # Comprehensive guide
│   ├── PLATFORM_LIMITATIONS.md    # OS restrictions & workarounds
│   ├── QUICKSTART.md              # 5-minute setup guide
│   ├── CONTRIBUTING.md            # Contributing guidelines
│   ├── ROADMAP.md                 # v1.1, v2.0, v3.0 plans
│   └── MODEL_DOWNLOADS.md         # Available Vosk models
│
└── LICENSE                        # MIT License

```

## Project Configuration

### AUTOCAP.Core.csproj
```xml
<TargetFramework>net8.0</TargetFramework>
<!-- Dependencies: Vosk, sqlite-net-pcl, System.Text.Json -->
```

### AUTOCAP.App.csproj
```xml
<TargetFrameworks>net8.0-android;net8.0-ios;net8.0-maccatalyst;net8.0-windows10.0.19041.0</TargetFrameworks>
<UseMaui>true</UseMaui>
<!-- Dependencies: CommunityToolkit.Maui, CommunityToolkit.Mvvm, Plugin.Permissions -->
```

### AUTOCAP.Tests.csproj
```xml
<TargetFramework>net8.0</TargetFramework>
<IsTestProject>true</IsTestProject>
<!-- Dependencies: xUnit, Microsoft.NET.Test.Sdk -->
```

## Build Targets

```
net8.0-windows10.0.19041.0  → Windows desktop
net8.0-android              → Android 21+
net8.0-ios                  → iOS 14+
net8.0-maccatalyst          → macOS 13.1+
```

## Key Files by Purpose

### Audio Capture
- `IAudioCapture.cs` - Interface
- `WindowsAudioCapture.cs`, `AndroidAudioCapture.cs`, etc. - Implementations

### ASR & Recognition
- `VoskRecognizer.cs` - Vosk integration
- `SubtitleEngine.cs` - Result processing

### UI & MVVM
- `MainViewModel.cs` - Main logic
- `MainPage.xaml` - Main UI
- `SettingsPage.xaml` - Settings
- `HelpPage.xaml` - Help & Info

### Platform Services
- `AndroidManifest.xml` - Android permissions
- `Info.plist` - iOS configuration
- `OverlayService.cs`, `SubtitleOverlayWindow.cs`, etc. - Platform overlays

### Data & Storage
- `VoskModelManager.cs` - Model download & caching
- `SessionStorage.cs` - SQLite sessions

### Testing
- `SubtitleEngineTests.cs` - Unit tests for subtitle generation

## Getting Started

1. **Clone repository**
   ```bash
   git clone https://github.com/yourusername/AUTOCAP.git
   cd AUTOCAP
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Build**
   ```bash
   dotnet build -f net8.0-windows10.0.19041.0  # Windows
   ```

4. **Run**
   ```bash
   dotnet run -f net8.0-windows10.0.19041.0 --project AUTOCAP.App\AUTOCAP.App.csproj
   ```

See [README.md](README.md) and [QUICKSTART.md](QUICKSTART.md) for detailed instructions.

