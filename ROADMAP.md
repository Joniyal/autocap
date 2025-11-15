# AUTOCAP Development Roadmap

## Version 1.0 (Current) ✅

### Core Features
- [x] Offline Vosk ASR
- [x] Real-time subtitle generation
- [x] Multi-platform MAUI UI
- [x] SRT/VTT export
- [x] Session storage (SQLite)
- [x] Model download manager

### Platform Support
- [x] Windows (WASAPI loopback + mic fallback)
- [x] Android (AudioPlaybackCapture + mic fallback)
- [x] iOS (microphone capture only)
- [x] macOS (BlackHole loopback + mic)
- [x] Linux (PulseAudio/PipeWire)

### Documentation
- [x] Comprehensive README
- [x] Platform limitation guide
- [x] Contributing guidelines
- [x] Build instructions per platform

---

## Version 1.1 (Q2 2025)

### Language Support
- [ ] Multi-language model selection
- [ ] Spanish, French, German, Mandarin Chinese
- [ ] Language auto-detection

### Performance
- [ ] GPU acceleration for audio resampling
- [ ] Reduced latency optimization
- [ ] Battery usage improvements on mobile

### Features
- [ ] Custom font/color themes
- [ ] Subtitle position customization (top, center, bottom)
- [ ] Keyboard shortcuts (Windows/macOS)
- [ ] Batch export (multiple formats)

---

## Version 2.0 (Q4 2025)

### Advanced Features
- [ ] **Automatic Translation**: Offline translation pipeline
- [ ] **Speaker Identification**: Distinguish multiple speakers
- [ ] **Subtitle Burning**: FFmpeg integration to burn subs into video
- [ ] **WebSocket API**: Stream transcription to web UI / remote devices
- [ ] **Accessibility**: Closed captions formatting (style preservation)

### Quality of Life
- [ ] Dark/Light theme toggle
- [ ] Custom punctuation rules
- [ ] Undo/redo for subtitle editing
- [ ] Batch processing for video files
- [ ] Mobile widget (Android 12+)

### Platform Enhancements
- [ ] iOS Picture-in-Picture subtitle overlay
- [ ] Android quick settings tile (Android 12+)
- [ ] Linux Wayland full support
- [ ] Windows 11 taskbar integration

---

## Version 3.0 (Planned)

### AI Enhancements
- [ ] Larger, more accurate Vosk models
- [ ] Context-aware punctuation
- [ ] Profanity filtering options
- [ ] Technical term dictionary support
- [ ] Custom domain models (medical, legal, technical)

### Enterprise Features
- [ ] Batch video processing
- [ ] API endpoint for server deployment
- [ ] Multi-user session management
- [ ] Advanced analytics & metrics

### Monetization (Optional)
- [ ] Free tier (single language, basic features)
- [ ] Pro tier (multi-language, advanced export, no watermark)
- [ ] Cloud sync (end-to-end encrypted)

---

## Known Limitations Being Addressed

### iOS Audio Capture
- **Current**: Microphone only (OS restriction)
- **Goal v2.0**: Floating subtitle view + workaround instructions
- **Goal v3.0**: Explore AirPlay routing or alternative solutions

### Platform-Specific Overlays
- **Current**: Skeleton implementations
- **Goal v1.1**: Full implementations for all platforms
- **Goal v2.0**: Advanced overlay customization

### Model Size
- **Current**: ~50MB (small model only)
- **Goal v2.0**: Offer multiple model sizes
- **Goal v3.0**: Quantized models for ultra-low-end devices

---

## Research & Exploration

- [ ] RNNoise integration for noise suppression
- [ ] Faster Whisper (OpenAI) as alternative ASR (with offline fallback)
- [ ] Real-time TTS (text-to-speech) for testing
- [ ] Mobile app for transcription distribution
- [ ] Browser extension for web video subtitles

---

## Community Wishlist

Have a feature request? Add it to [GitHub Discussions](https://github.com/yourusername/AUTOCAP/discussions)!

**Top voted features:**
1. [ ] Multi-speaker detection
2. [ ] Batch video processing
3. [ ] WebSocket API
4. [ ] Translation support
5. [ ] Advanced styling/themes

---

## Contributing to Roadmap

Want to help accelerate development? 
- Contribute code or documentation
- Test on various devices and platforms
- Provide feedback on features
- Sponsor the project

See [CONTRIBUTING.md](CONTRIBUTING.md) for details.
