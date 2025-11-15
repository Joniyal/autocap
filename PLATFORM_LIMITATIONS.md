# PLATFORM LIMITATIONS & KNOWN ISSUES

This document outlines OS-level restrictions that affect AUTOCAP's functionality on each platform.

## iOS - Internal Audio Capture Restriction

### Limitation
**iOS does NOT allow apps to capture internal audio from other apps.**

This is an Apple OS-level restriction enforced for privacy reasons:
- Apps cannot use APIs to record system audio, media player output, or other app audio
- Even with all permissions granted, the OS denies access
- This applies to all apps on the App Store; no exceptions

### Impact
- ❌ Cannot capture system audio (music, videos, calls, podcasts, etc.)
- ✅ Microphone capture still works
- ✅ Can capture audio played through speaker + microphone (workaround)

### Workarounds

#### Workaround 1: Speaker + Microphone Routing (Easiest)
1. Play audio through **speaker** (not earpiece/headphones)
2. Position iPhone microphone near speaker
3. Open AUTOCAP and start microphone capture
4. Microphone will pick up audio from speaker

**Pros**: No hardware needed
**Cons**: Lower audio quality, background noise may interfere

#### Workaround 2: External Loopback Hardware
1. Purchase external audio loopback hardware (e.g., tape loop adapter):
   - Headphone jack → microphone jack adapter
   - Or use a 3.5mm stereo splitter
2. Route audio output → loopback → microphone input
3. Use AUTOCAP microphone capture

**Pros**: Better audio quality
**Cons**: Requires additional hardware (~$10–20)

#### Workaround 3: Second Device Route via AirPlay
1. Set up AirPlay audio routing to a Mac or Apple TV
2. Run AUTOCAP on that device with system audio capture
3. Share transcription results back to iPhone

**Pros**: Uses native Apple ecosystem
**Cons**: Requires second device

### Official Apple Statement
- [Apple Human Interface Guidelines - Privacy](https://developer.apple.com/design/human-interface-guidelines/ios/user-interaction/privacy/)
- [AVAudioSession Documentation](https://developer.apple.com/documentation/avfaudio/avaudiosession)

### Expected Future?
Unlikely. Apple treats audio privacy as a core platform principle. No API is expected in the near future.

---

## macOS - System Audio Capture Requires Driver

### Limitation
**macOS does NOT provide native system audio capture API for third-party apps.**

The OS restricts direct access to internal audio routing to prevent eavesdropping and maintain privacy.

### Workaround: BlackHole Virtual Audio Device
Install **BlackHole** (free, open-source loopback driver):

```bash
# Using Homebrew
brew install blackhole-2ch

# OR visit: https://existential.audio/blackhole/
```

#### Setup Steps
1. Install BlackHole
2. Open System Preferences → Sound → Output
3. Select "BlackHole 2ch" as output device
4. Launch AUTOCAP
5. AUTOCAP will detect and use BlackHole automatically

#### How It Works
- Audio routed to BlackHole appears as a "virtual microphone"
- AUTOCAP captures this virtual input
- Works for all audio: music, videos, system sounds, etc.

### Alternative: Microphone Capture (No Setup)
- Use microphone to capture audio from speaker
- Requires speaker output + microphone positioning

---

## Windows - WASAPI Loopback May Not Be Available

### Limitation
**Some Windows systems don't have WASAPI loopback device enabled by default.**

This depends on:
- Audio driver version
- Hardware manufacturer
- Windows version (11 > 10 > 8)

### Workaround: Virtual Loopback Device
Install **VB-Audio Virtual Cable** (paid, ~$5) or **VirtualAudio** (free):

```powershell
# Using Chocolatey
choco install vb-audio-cable

# OR download: https://vb-audio.com/Cable/
```

#### Setup
1. Install virtual audio device driver
2. Reboot
3. Open Sound Settings → Advanced → App volume and device preferences
4. Set audio output to virtual device
5. Launch AUTOCAP; it will detect and use the loopback device

### Fallback
If loopback is unavailable, AUTOCAP automatically falls back to **microphone capture**.

---

## Android - AudioPlaybackCapture Requires Android 10+

### Limitation
**AudioPlaybackCapture API (system audio capture) is only available on Android 10+ (API 29+).**

Older Android versions cannot access internal audio streams.

### Impact
| Android Version | System Audio | Microphone | Notes |
|------------------|:------------:|:----------:|-------|
| Android 10+ | ✅ Full access | ✅ | Recommended |
| Android 9 (API 28) | ❌ Not available | ✅ | Fallback to mic |
| Android 8 & below | ❌ Not available | ✅ | Fallback to mic |

### Workaround
- On Android 9 and below, microphone capture is the only option
- Position device microphone near speaker for audio capture

### Required Permissions
```xml
<uses-permission android:name="android.permission.RECORD_AUDIO" />
<uses-permission android:name="android.permission.FOREGROUND_SERVICE" />
<uses-permission android:name="android.permission.WAKE_LOCK" />
<uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW" />
```

Grant these in Settings → Apps → AUTOCAP → Permissions

---

## Linux - Overlay Functionality Limited

### Limitation
**X11/Wayland window managers vary in overlay support.**

Linux doesn't have a unified window manager; different distributions and desktop environments handle overlays differently.

### Impact
- Overlay subtitle window may not work on all window managers
- Tested on: KDE Plasma, GNOME (Wayland), Xfce
- May not work on: i3, sway, other minimalist WMs

### Workaround
- Use in-app subtitle preview instead of overlay
- Contribute window manager-specific code!

### PulseAudio / PipeWire Detection
AUTOCAP automatically detects:
- PulseAudio (standard on most distributions)
- PipeWire (newer, replacing PulseAudio)

If neither is available, falls back to microphone capture.

---

## Vosk Model Accuracy

### Not a Limitation, but Reality Check

**Expected Accuracy: 70–80% for clean audio**

Factors affecting accuracy:
1. **Background Noise**: Restaurant, street traffic, etc. reduces accuracy
2. **Accent**: Trained on US English. Non-native speakers may see lower accuracy
3. **Audio Quality**: Low sample rate (8kHz) vs. high (16kHz+)
4. **Domain**: General English; technical jargon may be misrecognized
5. **Model Size**: Smaller models (~50MB) trade accuracy for speed

### Improving Accuracy
- Use higher quality audio input
- Reduce background noise (quiet environment)
- Speak clearly and at normal pace
- Post-process with punctuation corrections

### Future Improvements
- Larger Vosk models (v2 planned)
- Integration with language models for context-aware correction
- User-trained models for specific domains

---

## Common Issues & Solutions

### "Permission Denied" on Android
**Solution**: 
1. Settings → Apps → AUTOCAP → Permissions
2. Enable: Microphone, Display over other apps (for overlay)

### "No Audio Source Available" on macOS
**Solution**:
1. Install BlackHole (see macOS section above)
2. Configure as default output device

### Subtitle Overlay Not Appearing
**Solution**:
1. Check if app has overlay permission (Android: Settings → Display over other apps)
2. Try disabling and re-enabling overlay in settings
3. Restart app

### Model Download Fails
**Solution**:
1. Check internet connection
2. Retry download
3. Manual download: https://alphacephei.com/vosk/models/vosk-model-small-en-us-0.15.zip
4. Place in app cache directory

---

## Reporting Issues

If you encounter platform-specific issues:

1. **Describe your system**:
   - OS name and version
   - Device model (if mobile)
   - AUTOCAP version

2. **Include logs**:
   - Enable "Verbose Logging" in Settings
   - Screenshot or copy error messages

3. **Submit on GitHub**:
   - https://github.com/yourusername/AUTOCAP/issues

---

## Contributing Fixes

Help us improve platform support!

Areas needing work:
- [ ] iOS AVAudioEngine implementation (microphone capture only)
- [ ] Android foreground service + overlay
- [ ] Windows WASAPI loopback fallback logic
- [ ] Linux overlay using XCB/Wayland APIs
- [ ] macOS CoreAudio implementation

See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

