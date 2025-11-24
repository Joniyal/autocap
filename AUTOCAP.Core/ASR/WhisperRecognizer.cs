using Whisper.net;
using Whisper.net.Ggml;

namespace AUTOCAP.Core.ASR;

/// <summary>
/// OpenAI Whisper speech recognizer - Much more accurate than Vosk.
/// Uses whisper.cpp for fast local processing without API costs.
/// </summary>
public class WhisperRecognizer : IDisposable
{
    private bool _isInitialized = false;
    private readonly int _sampleRate;
    private WhisperProcessor? _processor;
    private List<float> _audioBuffer = new();
    private readonly object _bufferLock = new();
    private CancellationTokenSource? _processingCts;

    /// <summary>
    /// Raised when partial recognition result is available
    /// </summary>
    public event EventHandler<PartialResultEventArgs>? OnPartialResult;

    /// <summary>
    /// Raised when final recognition result is available
    /// </summary>
    public event EventHandler<FinalResultEventArgs>? OnFinalResult;

    /// <summary>
    /// Raised when an error occurs
    /// </summary>
    public event EventHandler<string>? OnError;

    public WhisperRecognizer(int sampleRate = 16000)
    {
        _sampleRate = sampleRate;
    }

    /// <summary>
    /// Initialize with Whisper model. Downloads if needed.
    /// </summary>
    public async Task<bool> InitializeModelAsync(string modelPath)
    {
        try
        {
            if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
            {
                OnError?.Invoke(this, $"Model file not found: {modelPath}");
                return false;
            }

            // Load Whisper model - DON'T dispose factory, keep it alive
            var whisperFactory = WhisperFactory.FromPath(modelPath);
            _processor = whisperFactory.CreateBuilder()
                .WithLanguage("en")
                .WithThreads(4)
                .Build();

            _isInitialized = true;
            _processingCts = new CancellationTokenSource();
            
            // Start background processing
            _ = Task.Run(ProcessAudioBufferAsync);
            
            return true;
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, $"Failed to initialize Whisper: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Process audio frame (PCM 16-bit mono at initialized sample rate).
    /// Buffers audio and processes in chunks for real-time recognition.
    /// </summary>
    public bool AcceptWaveform(byte[] audioData)
    {
        if (!_isInitialized || _processor == null)
        {
            OnError?.Invoke(this, "Model not initialized");
            return false;
        }

        try
        {
            // Convert byte[] to float[] (PCM 16-bit to float32)
            float[] floatSamples = new float[audioData.Length / 2];
            for (int i = 0; i < floatSamples.Length; i++)
            {
                short sample = BitConverter.ToInt16(audioData, i * 2);
                floatSamples[i] = sample / 32768f;
            }

            lock (_bufferLock)
            {
                _audioBuffer.AddRange(floatSamples);
            }

            return false;
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, $"Error processing audio: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Public accessor to check whether the recognizer is initialized.
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Background task that processes buffered audio in real-time
    /// </summary>
    private async Task ProcessAudioBufferAsync()
    {
        try
        {
            // Process every 3 seconds of audio for better Whisper detection
            int chunkSize = _sampleRate * 3; // 3 second chunks - Whisper needs longer audio
            
            while (!(_processingCts?.IsCancellationRequested ?? true))
            {
                try
                {
                    float[]? chunk = null;
                    
                    lock (_bufferLock)
                    {
                        if (_audioBuffer.Count >= chunkSize)
                        {
                            chunk = _audioBuffer.Take(chunkSize).ToArray();
                            _audioBuffer.RemoveRange(0, chunkSize);
                        }
                    }

                    if (chunk != null && _processor != null)
                    {
                        // Check audio level before processing
                        float audioLevel = CalculateAudioLevel(chunk);
                        
                        // Only process if there's actual audio (not silence)
                        if (audioLevel < 0.001f)
                        {
                            // Skip silent chunks
                            continue;
                        }
                        
                        try
                        {
                            // Process with Whisper - wrap in try/catch for safety
                            int segmentCount = 0;
                            await foreach (var segment in _processor.ProcessAsync(chunk))
                            {
                                segmentCount++;
                                if (segment != null && !string.IsNullOrWhiteSpace(segment.Text))
                                {
                                    try
                                    {
                                        // Emit as partial result immediately
                                        OnPartialResult?.Invoke(this, new PartialResultEventArgs 
                                        { 
                                            Text = segment.Text.Trim() 
                                        });

                                        // Also emit as final after a short delay
                                        await Task.Delay(100);
                                        OnFinalResult?.Invoke(this, new FinalResultEventArgs
                                        {
                                            Text = segment.Text.Trim(),
                                            Timestamp = DateTime.UtcNow
                                        });
                                    }
                                    catch (Exception evEx)
                                    {
                                        OnError?.Invoke(this, $"Event invocation error: {evEx.Message}");
                                    }
                                }
                            }
                            
                            // Log if no segments produced (with audio level info)
                            if (segmentCount == 0)
                            {
                                OnError?.Invoke(this, $"Whisper processed chunk but produced 0 segments (chunk size: {chunk.Length}, audio level: {audioLevel:F4})");
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // Expected on shutdown
                            break;
                        }
                        catch (Exception procEx)
                        {
                            OnError?.Invoke(this, $"Whisper process error: {procEx.Message}");
                            await Task.Delay(500);
                        }
                    }
                    else
                    {
                        // Wait a bit before checking buffer again
                        await Task.Delay(100);
                    }
                }
                catch (Exception loopEx)
                {
                    OnError?.Invoke(this, $"Loop error: {loopEx.Message}");
                    await Task.Delay(500);
                }
            }
        }
        catch (Exception outerEx)
        {
            OnError?.Invoke(this, $"Fatal processing error: {outerEx.Message}");
        }
    }

    private float CalculateAudioLevel(float[] audioData)
    {
        if (audioData == null || audioData.Length == 0) return 0f;
        
        float sum = 0;
        for (int i = 0; i < audioData.Length; i++)
        {
            sum += Math.Abs(audioData[i]);
        }
        
        return sum / audioData.Length;
    }

    public void Dispose()
    {
        _processingCts?.Cancel();
        _processingCts?.Dispose();
        _processor?.Dispose();
        _isInitialized = false;
    }
}
