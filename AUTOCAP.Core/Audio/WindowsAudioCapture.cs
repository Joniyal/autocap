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
            // Use smaller buffer for lower latency (20ms instead of default 100ms)
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
        
        try
        {
            // Convert to 16kHz mono 16-bit PCM if needed
            byte[] processedData = ConvertToTargetFormat(e.Buffer, e.BytesRecorded, _loopbackCapture.WaveFormat);
            
            // Calculate audio level for debugging
            float audioLevel = CalculateAudioLevel(processedData);
            
            OnAudioFrame?.Invoke(this, new AudioFrameEventArgs 
            { 
                Buffer = processedData,
                ByteCount = processedData.Length,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, $"Audio processing error: {ex.Message}");
        }
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
        
        try
        {
            // Use MediaFoundationResampler for conversion
            using var sourceStream = new MemoryStream(sourceData, 0, length);
            using var sourceProvider = new RawSourceWaveStream(sourceStream, sourceFormat);
            
            var targetFormat = new WaveFormat(16000, 1);
            using var resampler = new MediaFoundationResampler(sourceProvider, targetFormat);
            
            byte[] buffer = new byte[length * 2];
            int bytesRead = resampler.Read(buffer, 0, buffer.Length);
            
            byte[] finalOutput = new byte[bytesRead];
            Buffer.BlockCopy(buffer, 0, finalOutput, 0, bytesRead);
            return finalOutput;
        }
        catch
        {
            // Fallback to manual conversion if MediaFoundation fails
            return ManualResample(sourceData, length, sourceFormat);
        }
    }
    
    private byte[] ManualResample(byte[] sourceData, int length, WaveFormat sourceFormat)
    {
        // Convert to mono first
        int sourceBytesPerSample = sourceFormat.BitsPerSample / 8;
        int sourceSamplesPerFrame = sourceFormat.Channels;
        int totalSourceSamples = length / (sourceBytesPerSample * sourceSamplesPerFrame);
        
        short[] monoSamples = new short[totalSourceSamples];
        for (int i = 0; i < totalSourceSamples; i++)
        {
            int sum = 0;
            for (int ch = 0; ch < sourceSamplesPerFrame; ch++)
            {
                int offset = (i * sourceSamplesPerFrame + ch) * sourceBytesPerSample;
                sum += BitConverter.ToInt16(sourceData, offset);
            }
            monoSamples[i] = (short)(sum / sourceSamplesPerFrame);
        }
        
        // Simple resampling using linear interpolation
        double ratio = (double)sourceFormat.SampleRate / 16000.0;
        int outputSamples = (int)(totalSourceSamples / ratio);
        byte[] output = new byte[outputSamples * 2];
        
        for (int i = 0; i < outputSamples; i++)
        {
            double sourceIndex = i * ratio;
            int index1 = (int)sourceIndex;
            int index2 = Math.Min(index1 + 1, monoSamples.Length - 1);
            double fraction = sourceIndex - index1;
            
            short sample = (short)(monoSamples[index1] * (1 - fraction) + monoSamples[index2] * fraction);
            BitConverter.GetBytes(sample).CopyTo(output, i * 2);
        }
        
        return output;
    }
    
    private float CalculateAudioLevel(byte[] audioData)
    {
        if (audioData.Length < 2) return 0f;
        
        float sum = 0;
        int sampleCount = audioData.Length / 2;
        
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(audioData, i * 2);
            sum += Math.Abs(sample);
        }
        
        return sum / sampleCount / 32768f; // Normalize to 0-1
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
