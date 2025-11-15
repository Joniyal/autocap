# QUICKSTART - AUTOCAP Setup & Testing

## 5-Minute Setup (Windows)

### Step 1: Install .NET 8 SDK
```powershell
winget install Microsoft.DotNet.SDK.8
```

### Step 2: Clone AUTOCAP
```powershell
cd $home\Documents
git clone https://github.com/yourusername/AUTOCAP.git
cd AUTOCAP
```

### Step 3: Restore & Build
```powershell
dotnet restore
dotnet build -f net8.0-windows10.0.19041.0
```

### Step 4: Run
```powershell
dotnet run -f net8.0-windows10.0.19041.0 --project AUTOCAP.App\AUTOCAP.App.csproj
```

### Step 5: First Launch
1. App opens → Dark UI
2. Tap "Initialize/Download Model" → Downloads ~50MB
3. Wait for completion
4. Tap "Start Capture"
5. Speak or play audio
6. Watch live transcription update
7. Tap "Export as SRT" to save

---

## Testing on Your Device

### Test 1: Vosk Model
```powershell
# Verify model downloaded
$modelPath = "$env:APPDATA\Local\AUTOCAP\vosk_models"
ls $modelPath
# Should see: vosk-model-small-en-us-0.15
```

### Test 2: Microphone Capture
1. Tap "Start Capture"
2. Speak clearly: "Hello world, this is a test"
3. Watch live transcription
4. Should see: "Hello world this is a test" (or similar)

### Test 3: System Audio (Windows)
1. Open YouTube or any audio source
2. Play audio
3. Tap "Start Capture" in AUTOCAP
4. Watch subtitles generate in real-time

### Test 4: Export
1. Generate some subtitles
2. Tap "Export as SRT"
3. Open exported .srt file in notepad
4. Should see valid SRT format:
   ```
   1
   00:00:00,000 --> 00:00:02,500
   Hello world
   ```

---

## Android Testing

### On Physical Device
```bash
# Connect via USB
adb devices

# Grant permissions
adb shell pm grant com.autocap.app android.permission.RECORD_AUDIO
adb shell pm grant com.autocap.app android.permission.SYSTEM_ALERT_WINDOW

# Run
dotnet run -f net8.0-android --project AUTOCAP.App\AUTOCAP.App.csproj
```

### On Emulator
```bash
# Create emulator if needed
android avdmanager create avd -n "Pixel6" -k "system-images;android-12;google_apis;arm64-v8a"

# Run emulator
emulator -avd Pixel6

# Install and run
dotnet run -f net8.0-android --project AUTOCAP.App\AUTOCAP.App.csproj
```

---

## Troubleshooting

### "Model not found" Error
- Solution: Tap "Initialize/Download Model" and wait
- Check internet connection
- Manually download from: https://alphacephei.com/vosk/models/vosk-model-small-en-us-0.15.zip

### "No audio captured" on Windows
- WASAPI loopback not available
- Install virtual audio: https://vb-audio.com/Cable/
- Or use microphone instead

### "Permission denied" on Android
- Grant permissions: Settings → Apps → AUTOCAP → Permissions
- Enable microphone and system overlay

### "Subtitle overlay not visible" on Android
- Check: Settings → Apps → Special app access → Display over other apps → Allow AUTOCAP
- Try restarting app

### Model download stuck
- Check firewall/proxy settings
- Try different network (mobile data vs WiFi)
- Manually place model in app cache directory

---

## File Locations

### Windows
- App Cache: `%APPDATA%\Local\AUTOCAP\`
- Database: `%APPDATA%\Local\AUTOCAP\autocap.db`
- Models: `%APPDATA%\Local\AUTOCAP\vosk_models\`
- Exports: `%APPDATA%\Local\AUTOCAP\` (by default)

### Android
- App Cache: `/data/data/com.autocap.app/cache/`
- Database: `/data/data/com.autocap.app/cache/autocap.db`

### iOS
- App Cache: `~/Library/Caches/com.autocap.app/`
- Database: `~/Library/Caches/com.autocap.app/autocap.db`

---

## Performance Tips

1. **Use Small Model** (default): Faster, lower latency
2. **Enable Low CPU Mode** in settings if battery draining
3. **Close background apps** to reduce noise interference
4. **Ensure clean audio**: Speak clearly, minimize background noise
5. **Use 16kHz+ microphone**: Built-in mics usually sufficient

---

## Advanced: Running Tests

```powershell
# Run unit tests
dotnet test AUTOCAP.Tests\AUTOCAP.Tests.csproj

# Run with verbosity
dotnet test --logger "console;verbosity=detailed"
```

---

## Debugging

Enable verbose logging:
1. Settings → About → (toggle "Verbose Logging")
2. Re-run your test
3. Check console output or logs in app cache directory

---

## Need Help?

1. Check [README.md](README.md) for detailed setup
2. See [PLATFORM_LIMITATIONS.md](PLATFORM_LIMITATIONS.md) for OS restrictions
3. Open issue on [GitHub](https://github.com/yourusername/AUTOCAP/issues)
4. Start discussion on [GitHub Discussions](https://github.com/yourusername/AUTOCAP/discussions)

---

**Happy captioning! 🎯**
