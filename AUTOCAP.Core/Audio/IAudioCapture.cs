namespace AUTOCAP.Core.Audio;

/// <summary>
/// Interface for cross-platform audio capture implementations.
/// Each platform provides a concrete implementation (Windows WASAPI, macOS CoreAudio, Android AudioPlaybackCapture, etc.)
/// </summary>
public interface IAudioCapture : IDisposable
{
    /// <summary>
    /// Audio format information
    /// </summary>
    AudioFormat Format { get; }

    /// <summary>
    /// Raised when a new audio frame is available (PCM bytes)
    /// </summary>
    event EventHandler<AudioFrameEventArgs>? OnAudioFrame;

    /// <summary>
    /// Raised when an error occurs during capture
    /// </summary>
    event EventHandler<string>? OnError;

    /// <summary>
    /// Start capturing audio
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stop capturing audio
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Get the current capture status
    /// </summary>
    bool IsCapturing { get; }

    /// <summary>
    /// Get a description of the audio source (e.g., "System Loopback", "Microphone", "Unknown")
    /// </summary>
    string SourceDescription { get; }
}

/// <summary>
/// Audio format specification
/// </summary>
public class AudioFormat
{
    public int SampleRate { get; set; } = 16000; // 16 kHz (standard for ASR)
    public int Channels { get; set; } = 1; // Mono
    public int BitsPerSample { get; set; } = 16; // 16-bit PCM
    public int BytesPerFrame => (BitsPerSample / 8) * Channels;
}

/// <summary>
/// Audio frame data
/// </summary>
public class AudioFrameEventArgs : EventArgs
{
    public required byte[] Buffer { get; set; }
    public required int ByteCount { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
