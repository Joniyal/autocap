# CONTRIBUTING TO AUTOCAP

Thank you for your interest in contributing to AUTOCAP!

## How to Contribute

### Reporting Bugs
1. Check [GitHub Issues](https://github.com/yourusername/AUTOCAP/issues) for duplicates
2. Provide:
   - Platform (Windows/Android/iOS/macOS/Linux)
   - AUTOCAP version
   - Steps to reproduce
   - Expected vs. actual behavior
   - Logs (enable verbose logging in Settings)

### Suggesting Features
1. Open a [Discussion](https://github.com/yourusername/AUTOCAP/discussions)
2. Describe the feature and use case
3. Examples or mockups are helpful

### Code Contributions

#### Setup Development Environment
```bash
# Install .NET 8 SDK
# Clone repository
git clone https://github.com/yourusername/AUTOCAP.git
cd AUTOCAP

# Restore & build
dotnet restore
dotnet build
```

#### Areas Needing Help
- [ ] **Android**: Implement AudioPlaybackCapture + foreground service + overlay
- [ ] **iOS**: Implement AVAudioEngine microphone capture + floating view
- [ ] **Windows**: Implement WASAPI loopback + transparent overlay window
- [ ] **macOS**: Implement CoreAudio + NSWindow overlay
- [ ] **Linux**: Implement PulseAudio/PipeWire + X11/Wayland overlay
- [ ] **Features**: Translation, multi-language, WebSocket API
- [ ] **Tests**: Add more unit & integration tests
- [ ] **Documentation**: Platform guides, troubleshooting, video tutorials

#### Making a Pull Request

1. **Fork & branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Code & commit**
   - Follow C# naming conventions (PascalCase for public, _camelCase for private)
   - Add XML documentation comments for public APIs
   - Write unit tests for new logic
   - Test on at least one platform

3. **Push & create PR**
   ```bash
   git push origin feature/your-feature-name
   ```
   - Title: Brief description
   - Description: Why, what, how
   - Link related issues

#### Code Style
- C# 11+ features allowed (nullable reference types required)
- Use MVVM pattern in ViewModels
- Separate platform-specific code into `Platforms/` folders
- Add comments explaining non-obvious logic

#### Testing
```bash
# Run unit tests
dotnet test

# Test specific platform
dotnet build -f net8.0-android
dotnet run -f net8.0-android --project AUTOCAP.App\AUTOCAP.App.csproj
```

### Documentation Improvements
- Fix typos in README, help pages, code comments
- Add platform-specific setup guides
- Create video tutorials
- Translate documentation

---

## Code Review Process

1. Automated checks (build, tests)
2. Manual review by maintainers
3. Feedback and iterations
4. Merge when approved

---

## Licensing
All contributions are under the [MIT License](LICENSE). By contributing, you agree to release your code under this license.

---

## Questions?
- Start a [Discussion](https://github.com/yourusername/AUTOCAP/discussions)
- Email: maintainer@example.com
- Twitter: [@autocap_dev](https://twitter.com/autocap_dev)

**We appreciate your contributions! 🎉**
