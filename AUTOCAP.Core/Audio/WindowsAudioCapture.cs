using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace AUTOCAP.Core.Audio;

/// <summary>
/// Windows audio capture using WASAPI loopback (system audio) or microphone fallback.
/// Captures audio from whatever is playing on the system (movies, videos, games, etc.)
/// 
/// Platform: Windows 10+
/// Permissions: None required
/// </summary>
public class WindowsAudioCapture : IAudioCapture
{
    private readonly AudioFormat _format = new();
    private bool _isCapturing = false;
    private string _sourceDescription = "Not initialized";
    
    private WasapiLoopbackCapture? _loopbackCapture;
    private WaveInEvent? _microphoneCapture;
    private bool _isUsingLoopback = false;

    public AudioFormat Format => _format;
    public bool IsCapturing => _isCapturing;
    public string SourceDescription => _sourceDescription;

    public event EventHandler<AudioFrameEventArgs>? OnAudioFrame;
    public event EventHandler<string>? OnError;

    public async Task StartAsync()
    {
        try
        {
            // Try WASAPI loopback first (captures system audio)
            _loopbackCapture = new WasapiLoopbackCapture();
            _loopbackCapture.DataAvailable += OnLoopbackDataAvailable;
            _loopbackCapture.RecordingStopped += OnRecordingStopped;
            _loopbackCapture.StartRecording();
            
            _isUsingLoopback = true;
            _isCapturing = true;
            _sourceDescription = "System Audio (WASAPI Loopback) - Capturing audio from movies/videos playing";
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // Fallback to microphone if loopback fails
            try
            {
                _microphoneCapture = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(16000, 1) // 16kHz mono
                };
                _microphoneCapture.DataAvailable += OnMicrophoneDataAvailable;
                _microphoneCapture.RecordingStopped += OnRecordingStopped;
                _microphoneCapture.StartRecording();
                
                _isUsingLoopback = false;
                _isCapturing = true;
                _sourceDescription = "Microphone (Fallback)";
                OnError?.Invoke(this, $"WASAPI loopback not available: {ex.Message}. Using microphone.");
            }
            catch (Exception micEx)
            {
                OnError?.Invoke(this, $"Failed to start audio capture: {micEx.Message}");
                throw;
            }
        }
    }
    
    private void OnLoopbackDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (e.BytesRecorded == 0 || _loopbackCapture == null) return;
        
        // Convert to 16kHz mono 16-bit PCM if needed
        byte[] processedData = ConvertToTargetFormat(e.Buffer, e.BytesRecorded, _loopbackCapture.WaveFormat);
        
        OnAudioFrame?.Invoke(this, new AudioFrameEventArgs 
        { 
            Buffer = processedData,
            ByteCount = processedData.Length,
            Timestamp = DateTime.UtcNow
        });
    }
    
    private void OnMicrophoneDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (e.BytesRecorded == 0) return;
        
        byte[] data = new byte[e.BytesRecorded];
        Buffer.BlockCopy(e.Buffer, 0, data, 0, e.BytesRecorded);
        
        OnAudioFrame?.Invoke(this, new AudioFrameEventArgs 
        { 
            Buffer = data,
            ByteCount = data.Length,
            Timestamp = DateTime.UtcNow
        });
    }
    
    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            OnError?.Invoke(this, $"Recording stopped with error: {e.Exception.Message}");
        }
    }
    
    private byte[] ConvertToTargetFormat(byte[] sourceData, int length, WaveFormat sourceFormat)
    {
        // If already correct format, return as-is
        if (sourceFormat.SampleRate == 16000 && sourceFormat.Channels == 1 && sourceFormat.BitsPerSample == 16)
        {
            byte[] output = new byte[length];
            Buffer.BlockCopy(sourceData, 0, output, 0, length);
            return output;
        }
        
        // Simple conversion: downsample to mono and resample to 16kHz
        // For production, use NAudio's resampler for better quality
        using var sourceStream = new MemoryStream(sourceData, 0, length);
        using var sourceProvider = new RawSourceWaveStream(sourceStream, sourceFormat);
        
        var targetFormat = new WaveFormat(16000, 1);
        using var resampler = new MediaFoundationResampler(sourceProvider, targetFormat);
        
        byte[] buffer = new byte[length * 2]; // Allocate extra space
        int bytesRead = resampler.Read(buffer, 0, buffer.Length);
        
        byte[] finalOutput = new byte[bytesRead];
        Buffer.BlockCopy(buffer, 0, finalOutput, 0, bytesRead);
        return finalOutput;
    }

    public async Task StopAsync()
    {
        _isCapturing = false;
        
        if (_loopbackCapture != null)
        {
            _loopbackCapture.StopRecording();
            _loopbackCapture.Dispose();
            _loopbackCapture = null;
        }
        
        if (_microphoneCapture != null)
        {
            _microphoneCapture.StopRecording();
            _microphoneCapture.Dispose();
            _microphoneCapture = null;
        }
        
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_isCapturing)
        {
            StopAsync().Wait();
        }
        
        _loopbackCapture?.Dispose();
        _microphoneCapture?.Dispose();
    }
}
