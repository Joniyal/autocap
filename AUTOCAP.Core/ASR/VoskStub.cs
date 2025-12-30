namespace AUTOCAP.Core.ASR;

/// <summary>
/// Stub Vosk implementation for demonstration.
/// In production, install the Vosk NuGet package.
/// </summary>
public class VoskModel
{
    private readonly string _modelPath;

    public VoskModel(string modelPath)
    {
        _modelPath = modelPath;
    }
}

public class VoskRecognizerStub
{
    private VoskModel? _model;
    private bool _demoMode = false;
    private int _bufferedFrames = 0;
    private readonly string[] _demoLines = new[]
    {
        "This is a simulated subtitle.",
        "Testing live captions using Vosk stub.",
        "Try speaking now — this is simulated output.",
        "Demo final subtitle line.",
        "(Demo) quick caption sample added for packaging tests."
    };
    public event Action<string>? OnPartialResult;
    public event Action<string>? OnFinalResult;
    public event Action<string>? OnError;

    // NOTE: This demo stub emits simulated partial and final events
    // to support offline testing and packaging verification.

    public void InitializeModel(string modelPath)
    {
        // Allow a special "DEMO" modelPath or empty/null to enable a simulated demo mode
        if (string.IsNullOrWhiteSpace(modelPath) || string.Equals(modelPath, "DEMO", StringComparison.OrdinalIgnoreCase))
        {
            _demoMode = true;
            _model = new VoskModel("<demo>");
            return;
        }

        if (!File.Exists(modelPath))
        {
            OnError?.Invoke($"Model not found at {modelPath}");
            return;
        }

        _model = new VoskModel(modelPath);
    }

    public void AcceptWaveform(byte[] audioData)
    {
        if (_model == null && !_demoMode)
        {
            OnError?.Invoke("Model not initialized");
            return;
        }

        if (_demoMode)
        {
            // Make demo much faster: emit partials every 5 frames and finals every 15 frames
            _bufferedFrames++;
            if (_bufferedFrames % 5 == 0)
            {
                var idx = (_bufferedFrames / 5 - 1) % _demoLines.Length;
                OnPartialResult?.Invoke(_demoLines[idx]);
            }

            if (_bufferedFrames >= 15)
            {
                var idx = (_bufferedFrames / 5 - 1) % _demoLines.Length;
                OnFinalResult?.Invoke(_demoLines[idx]);
                _bufferedFrames = 0;
            }

            return;
        }

        // Real model would process audioData here. For stub, emit a lightweight partial marker.
        OnPartialResult?.Invoke("...");
    }

    public void FinishStream()
    {
        if (_demoMode)
        {
            OnFinalResult?.Invoke("Demo subtitle text");
            _bufferedFrames = 0;
            return;
        }

        OnFinalResult?.Invoke("Demo subtitle text");
    }

    /// <summary>
    /// Enable or disable demo mode at runtime
    /// </summary>
    public void SetDemoMode(bool enabled)
    {
        _demoMode = enabled;
        _bufferedFrames = 0;
        if (enabled && _model == null)
        {
            _model = new VoskModel("<demo>");
        }
    }
}
