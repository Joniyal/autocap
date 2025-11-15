using System.Text.Json;

namespace AUTOCAP.Core.ASR;

/// <summary>
/// Wrapper around Vosk offline speech recognizer.
/// Handles model loading, frame processing, and result event emission.
/// For production, install the Vosk NuGet package.
/// </summary>
public class VoskRecognizer : IDisposable
{
    private bool _isInitialized = false;
    private readonly int _sampleRate;

    /// <summary>
    /// Raised when partial recognition result is available (interim hypothesis)
    /// </summary>
    public event EventHandler<PartialResultEventArgs>? OnPartialResult;

    /// <summary>
    /// Raised when final recognition result is available (end of utterance)
    /// </summary>
    public event EventHandler<FinalResultEventArgs>? OnFinalResult;

    /// <summary>
    /// Raised when an error occurs during recognition
    /// </summary>
    public event EventHandler<string>? OnError;

    public VoskRecognizer(int sampleRate = 16000)
    {
        _sampleRate = sampleRate;
    }

    /// <summary>
    /// Initialize recognizer with a Vosk model path.
    /// Model must be downloaded and available at the specified path.
    /// </summary>
    public bool InitializeModel(string modelPath)
    {
        try
        {
            if (string.IsNullOrEmpty(modelPath) || !Directory.Exists(modelPath))
            {
                OnError?.Invoke(this, $"Model path not found: {modelPath}");
                return false;
            }

            // Initialize recognizer (stub for demo - Vosk package not available)
            _isInitialized = true;
            return true;
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, $"Failed to initialize Vosk model: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Process audio frame (PCM 16-bit mono at initialized sample rate).
    /// Returns true if final result was generated, false if partial.
    /// For demo, simulates ASR with mock results.
    /// </summary>
    public bool AcceptWaveform(byte[] audioData)
    {
        if (!_isInitialized)
        {
            OnError?.Invoke(this, "Recognizer not initialized");
            return false;
        }

        try
        {
            // Demo: simulate recognition
            // Real implementation would call actual Vosk library
            return false;
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, $"Error processing waveform: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get any remaining final result (call at end of stream)
    /// </summary>
    public string GetFinalResult()
    {
        return "Demo result";
    }

    private void ProcessPartialResult(string jsonResult)
    {
        try
        {
            // Partial result format: {"result": [partial words], "result_text": "partial text"}
            // or just {"partial": "partial text"}
            using JsonDocument doc = JsonDocument.Parse(jsonResult);
            string? partial = null;

            if (doc.RootElement.TryGetProperty("partial", out var partialElem))
                partial = partialElem.GetString();
            else if (doc.RootElement.TryGetProperty("result_text", out var textElem))
                partial = textElem.GetString();

            if (!string.IsNullOrEmpty(partial))
            {
                OnPartialResult?.Invoke(this, new PartialResultEventArgs { Text = partial });
            }
        }
        catch
        {
            // Invalid JSON or unexpected format, ignore
        }
    }

    private void ProcessFinalResult(string jsonResult)
    {
        try
        {
            // Final result format: {"result": [...], "text": "final text"} or {"text": "final text"}
            using JsonDocument doc = JsonDocument.Parse(jsonResult);
            string? text = null;
            List<string>? words = null;

            if (doc.RootElement.TryGetProperty("text", out var textElem))
                text = textElem.GetString();

            if (doc.RootElement.TryGetProperty("result", out var resultElem) && resultElem.ValueKind == JsonValueKind.Array)
            {
                words = new List<string>();
                foreach (var item in resultElem.EnumerateArray())
                {
                    if (item.TryGetProperty("conf", out _) && item.TryGetProperty("result", out var wordElem))
                        words.Add(wordElem.GetString() ?? string.Empty);
                }
            }

            if (!string.IsNullOrEmpty(text))
            {
                OnFinalResult?.Invoke(this, new FinalResultEventArgs
                {
                    Text = text,
                    Words = words,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        catch
        {
            // Invalid JSON or unexpected format, ignore
        }
    }

    public void Dispose()
    {
        _isInitialized = false;
    }
}

public class PartialResultEventArgs : EventArgs
{
    public required string Text { get; set; }
}

public class FinalResultEventArgs : EventArgs
{
    public required string Text { get; set; }
    public List<string>? Words { get; set; }
    public DateTime Timestamp { get; set; }
}
