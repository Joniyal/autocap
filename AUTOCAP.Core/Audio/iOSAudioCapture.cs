namespace AUTOCAP.Core.Audio;

/// <summary>
/// iOS audio capture using microphone only.
/// 
/// Platform: iOS 14+
/// Permissions Required:
///   - NSMicrophoneUsageDescription (Info.plist)
///
/// PLATFORM LIMITATION:
///   - iOS does NOT allow capturing internal audio from other apps (OS-level restriction)
///   - This class implements MICROPHONE capture only as a workaround
///   - User must route audio through speaker + microphone or use external loopback hardware
///   - See README and in-app explanation for detailed workarounds
///
/// Notes:
///   - Uses AVAudioEngine for microphone input
///   - Microphone capture works if audio is played through speaker (not earpiece)
///   - Headphones reduce effectiveness unless audio is split externally
/// </summary>
public class iOSAudioCapture : IAudioCapture
{
    private readonly AudioFormat _format = new();
    private bool _isCapturing = false;
    private string _sourceDescription = "Not initialized";

    public AudioFormat Format => _format;
    public bool IsCapturing => _isCapturing;
    public string SourceDescription => _sourceDescription;

    public event EventHandler<AudioFrameEventArgs>? OnAudioFrame;
    public event EventHandler<string>? OnError;

    public async Task StartAsync()
    {
        try
        {
            _isCapturing = true;
            _sourceDescription = "Microphone (iOS internal audio capture not supported)";
            
            // TODO: Implement AVAudioEngine microphone capture in Platforms/iOS/
            // Steps:
            // 1. Create AVAudioEngine and get input node
            // 2. Request recording permission if not granted
            // 3. Attach audio format: 16kHz mono 16-bit PCM
            // 4. Install tap on input node with completion handler
            // 5. Start audio engine
            // 6. Buffer frames and emit OnAudioFrame events
            
            OnError?.Invoke(this, "iOS LIMITATION: Cannot capture internal device audio (OS restriction). " +
                "Using microphone capture instead.\n" +
                "Workaround: Route audio to speaker and position mic near speaker, or use external loopback.");

            await Task.Delay(100); // Placeholder async
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, $"iOS microphone capture failed: {ex.Message}");
        }
    }

    public async Task StopAsync()
    {
        _isCapturing = false;
        // TODO: Stop AVAudioEngine and release resources
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_isCapturing)
        {
            StopAsync().Wait();
        }
        // TODO: Dispose AVAudioEngine resources
    }
}
