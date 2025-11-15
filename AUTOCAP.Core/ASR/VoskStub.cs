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
    public event Action<string>? OnPartialResult;
    public event Action<string>? OnFinalResult;
    public event Action<string>? OnError;

    public void InitializeModel(string modelPath)
    {
        if (!File.Exists(modelPath))
        {
            OnError?.Invoke($"Model not found at {modelPath}");
            return;
        }
        
        _model = new VoskModel(modelPath);
    }

    public void AcceptWaveform(byte[] audioData)
    {
        if (_model == null)
        {
            OnError?.Invoke("Model not initialized");
            return;
        }
        
        // Demo: simulate recognition
        OnPartialResult?.Invoke("...");
    }

    public void FinishStream()
    {
        OnFinalResult?.Invoke("Demo subtitle text");
    }
}
