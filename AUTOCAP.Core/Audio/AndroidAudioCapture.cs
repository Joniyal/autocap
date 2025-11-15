namespace AUTOCAP.Core.Audio;

/// <summary>
/// Android audio capture using AudioPlaybackCapture API (Android 10+) or microphone fallback.
/// 
/// Platform: Android 10+ (API 29+)
/// Permissions Required:
///   - android.permission.RECORD_AUDIO
///   - android.permission.FOREGROUND_SERVICE
///   - android.permission.WAKE_LOCK
///   - android.permission.SYSTEM_ALERT_WINDOW (for overlay)
///
/// Notes:
///   - Android 10+: AudioPlaybackCapture can capture system audio directly
///   - Requires foreground service + notification to keep running
///   - Overlay requires SYSTEM_ALERT_WINDOW permission
///   - Fallback: Microphone capture works on all Android versions
/// </summary>
public class AndroidAudioCapture : IAudioCapture
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
            
            // Determine Android API level and available capture method
            // For API 29+: Try AudioPlaybackCapture
            // Fallback: Use AudioRecord for microphone
            
            _sourceDescription = "System Audio (AudioPlaybackCapture) or Microphone Fallback";
            
            // TODO: Implement Android-specific capture
            // This requires platform-specific code in Platforms/Android/
            // 1. Check if AudioPlaybackCapture is available (API 29+)
            // 2. If yes, set up AudioPlaybackCapture with foreground service
            // 3. If no, fall back to AudioRecord (microphone)
            // 4. Run capture in background thread
            // 5. Emit OnAudioFrame for each 16kHz mono 16-bit frame
            
            OnError?.Invoke(this, "Android audio capture requires platform-specific Java/Kotlin bindings. " +
                "Implement in Platforms/Android/ with foreground service and AudioPlaybackCapture API.");

            await Task.Delay(100); // Placeholder async
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, $"Android audio capture failed: {ex.Message}");
        }
    }

    public async Task StopAsync()
    {
        _isCapturing = false;
        // TODO: Stop background service and release resources
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_isCapturing)
        {
            StopAsync().Wait();
        }
    }
}
