# Model Downloads Configuration

This file defines available Vosk models and their download URLs.

## Available Models

### English - Small (Recommended for Mobile)
- **Name**: vosk-model-small-en-us-0.15
- **Size**: ~50 MB
- **Accuracy**: 70-75%
- **Latency**: Low (good for mobile)
- **URL**: https://alphacephei.com/vosk/models/vosk-model-small-en-us-0.15.zip

### English - Full
- **Name**: vosk-model-en-us-0.22
- **Size**: ~250 MB
- **Accuracy**: 75-80%
- **Latency**: Medium (requires more CPU)
- **URL**: https://alphacephei.com/vosk/models/vosk-model-en-us-0.22.zip

## Adding New Models

1. Update the `model_downloads.json` with model metadata
2. Add download URL to VoskModelManager.cs
3. Test on each platform (Windows, Android, iOS, macOS)

## Future Languages

Support for additional languages can be added by:
1. Finding compatible Vosk models from https://alphacephei.com/vosk/models
2. Adding configuration entries
3. Updating app UI to allow language selection

Planned for v1.1+:
- Spanish
- French
- German
- Mandarin Chinese
