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
        "Demo final subtitle line."
    };
    public event Action<string>? OnPartialResult;
    public event Action<string>? OnFinalResult;
    public event Action<string>? OnError;

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
            // Simple heuristic: every N calls emit a partial, after M calls emit a final
            _bufferedFrames++;
            if (_bufferedFrames % 20 == 0)
            {
                var idx = (_bufferedFrames / 20 - 1) % _demoLines.Length;
                OnPartialResult?.Invoke(_demoLines[idx]);
            }

            if (_bufferedFrames >= 60)
            {
                var idx = (_bufferedFrames / 20 - 1) % _demoLines.Length;
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
}
