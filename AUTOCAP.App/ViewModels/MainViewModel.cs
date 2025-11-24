namespace AUTOCAP.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AUTOCAP.Core.ASR;
using AUTOCAP.Core.Audio;
using AUTOCAP.Core.Subtitle;
using AUTOCAP.App;
using AUTOCAP.Core.Models;
using System.Collections.ObjectModel;

/// <summary>
/// Main view model for the AUTOCAP application.
/// Coordinates audio capture, ASR, subtitle generation, and UI updates.
/// Hotkeys: [ = decrease text, ] = increase text, arrow keys = move overlay
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IAudioCapture? _audioCapture;
    private readonly WhisperRecognizer? _whisper;
    private VoskRecognizerStub? _voskStub;
    private readonly SubtitleEngine _subtitleEngine = new();
    private readonly WhisperModelManager? _modelManager;
    private readonly SessionDatabaseService? _sessionDb;
    private Platforms.Windows.WinFormsOverlayManager? _winFormsOverlay;

    [ObservableProperty]
    private bool isCapturing = false;

    [ObservableProperty]
    private string currentTranscription = "Ready to capture...";

    [ObservableProperty]
    private string audioSource = "Not initialized";

    [ObservableProperty]
    private float downloadProgress = 0f;

    [ObservableProperty]
    private bool isDownloadingModel = false;

    [ObservableProperty]
    private ObservableCollection<SubtitleLineViewModel> subtitles = new();

    [ObservableProperty]
    private string statusMessage = string.Empty;

    // Settings
    [ObservableProperty]
    private float subtitleFontSize = 18f;

    [ObservableProperty]
    private float subtitleDelay = 0f; // milliseconds

    [ObservableProperty]
    private float subtitleOpacity = 0.8f;

    [ObservableProperty]
    private bool autoCapitalize = true;

    [ObservableProperty]
    private bool autoPunctuate = true;

    [ObservableProperty]
    private bool lowCpuMode = false;

    public MainViewModel()
    {
        try
        {
            DebugBootstrapLog.Log("MainViewModel: Starting initialization");
            
            _subtitleEngine = new SubtitleEngine();
            _subtitleEngine.OnNewSubtitleLine += OnNewSubtitleLine;
            _subtitleEngine.OnPartialSubtitle += OnPartialSubtitle;

            string appCacheDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "AUTOCAP");
            Directory.CreateDirectory(appCacheDir);
            
            _modelManager = new WhisperModelManager(appCacheDir);
            DebugBootstrapLog.Log("MainViewModel: Model manager created");

            // Platform-specific audio capture
            _audioCapture = GetPlatformAudioCapture();
            DebugBootstrapLog.Log($"MainViewModel: Audio capture initialized: {(_audioCapture != null ? "OK" : "NULL")}");
            
            // Wire audio capture to send data to recognizer
            if (_audioCapture != null)
            {
                _audioCapture.OnAudioFrame += OnAudioFrameReceived;
                _audioCapture.OnError += (s, msg) =>
                {
                    try
                    {
                        DebugBootstrapLog.Log($"Audio capture error: {msg}");
                        MainThread.BeginInvokeOnMainThread(() => { StatusMessage = $"Audio capture error: {msg}"; });
                    }
                    catch (Exception ex) 
                    { 
                        DebugBootstrapLog.Log($"Error handler exception: {ex.Message}"); 
                    }
                };
            }

            // Initialize Whisper
            _whisper = new WhisperRecognizer(16000);
            _whisper.OnPartialResult += Whisper_OnPartialResult;
            _whisper.OnFinalResult += Whisper_OnFinalResult;
            _whisper.OnError += Whisper_OnError;
            
            // Initialize Vosk stub (demo) as a fallback - do not block on model files
            _voskStub = new AUTOCAP.Core.ASR.VoskRecognizerStub();
            _voskStub.OnPartialResult += Vosk_OnPartial;
            _voskStub.OnFinalResult += Vosk_OnFinal;
            _voskStub.OnError += Vosk_OnError;
            // Initialize in demo mode so it will simulate results if no model is present
            _voskStub.InitializeModel("DEMO");
            DebugBootstrapLog.Log("MainViewModel: Whisper recognizer created");
            
            // Don't initialize overlay yet - will create when capturing starts
            _winFormsOverlay = null;

            StatusMessage = "Ready. Tap 'Initialize/Download Model' to begin";
            DebugBootstrapLog.Log("MainViewModel: Initialization complete");
        }
        catch (Exception ex)
        {
            DebugBootstrapLog.Log($"MainViewModel init error: {ex}");
            StatusMessage = $"Init error: {ex.Message}";
        }

        // Initialize database
        try
        {
            string appCacheDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "AUTOCAP");
            string dbPath = Path.Combine(appCacheDir, "autocap.db");
            _sessionDb = new SessionDatabaseService(dbPath);
        }
        catch
        {
            // Database optional for demo
        }

        AudioSource = _audioCapture?.SourceDescription ?? "Not available";
        if (string.IsNullOrEmpty(StatusMessage))
        {
            StatusMessage = "Initialize model to begin";
        }
    }

    [RelayCommand]
    public async Task InitializeModelAsync()
    {
        DebugBootstrapLog.Log("InitializeModelAsync: Starting");
        
        if (_modelManager == null)
        {
            DebugBootstrapLog.Log("InitializeModelAsync: Model manager NULL");
            StatusMessage = "Model manager not initialized";
            return;
        }

        string? modelPath = _modelManager.GetModelPath("ggml-base.bin");
        DebugBootstrapLog.Log($"InitializeModelAsync: Model path = {modelPath}");
        
        if (modelPath != null)
        {
            // Model already exists
            DebugBootstrapLog.Log("InitializeModelAsync: Base model file found, attempting to load");
            bool success = await (_whisper?.InitializeModelAsync(modelPath) ?? Task.FromResult(false));
            if (success)
            {
                DebugBootstrapLog.Log("InitializeModelAsync: Base model loaded successfully");
                StatusMessage = "✅ Whisper model ready!";
                return;
            }
            else
            {
                // Try fallback to tiny model if available
                DebugBootstrapLog.Log("InitializeModelAsync: Base model failed, trying tiny fallback");
                StatusMessage = "Base model failed, trying tiny model...";
                var tinyPath = _modelManager.GetModelPath("ggml-tiny.bin");
                if (tinyPath != null)
                {
                    bool tinyOk = await (_whisper?.InitializeModelAsync(tinyPath) ?? Task.FromResult(false));
                    if (tinyOk)
                    {
                        DebugBootstrapLog.Log("InitializeModelAsync: Tiny model loaded as fallback");
                        StatusMessage = "✅ Whisper tiny model ready (fallback)";
                    }
                    else
                    {
                        DebugBootstrapLog.Log("InitializeModelAsync: Tiny model failed to load");
                        StatusMessage = "❌ Failed to load any model";
                    }
                    return;
                }
                // continue to download flow below
            }
        }

        // Download Whisper base model (142MB, accurate)
        IsDownloadingModel = true;
        StatusMessage = "Downloading Whisper base model (142MB)...";
        DownloadProgress = 0f;

        bool downloaded = await _modelManager.DownloadBaseModelAsync(progress =>
        {
            DownloadProgress = progress;
        });

        IsDownloadingModel = false;

        if (downloaded)
        {
            modelPath = _modelManager.GetModelPath("ggml-base.bin");
            if (modelPath != null)
            {
                bool success = await (_whisper?.InitializeModelAsync(modelPath) ?? Task.FromResult(false));
                if (success)
                {
                    StatusMessage = "Whisper model ready!";
                    return;
                }
                else
                {
                    // Try tiny fallback download
                    StatusMessage = "Base model failed to initialize; trying tiny fallback...";
                    bool tinyDownloaded = await _modelManager.DownloadTinyModelAsync(progress => { DownloadProgress = progress; });
                    if (tinyDownloaded)
                    {
                        var tinyPath = _modelManager.GetModelPath("ggml-tiny.bin");
                        if (tinyPath != null)
                        {
                            bool tinyOk = await (_whisper?.InitializeModelAsync(tinyPath) ?? Task.FromResult(false));
                            StatusMessage = tinyOk ? "Whisper tiny model loaded as fallback" : "Model downloaded but failed to load";
                            return;
                        }
                    }
                }
            }
        }
        else
        {
            // Try to surface a more detailed error if the model manager wrote one
            try
            {
                string errPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AUTOCAP", "whisper_download_error.txt");
                if (File.Exists(errPath))
                {
                    string details = await File.ReadAllTextAsync(errPath);
                    StatusMessage = $"Model download failed: {details}";
                }
                else
                {
                    StatusMessage = "Model download failed. Check internet connection.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Model download failed: {ex.Message}";
            }
        }
    }

    [RelayCommand]
    public async Task ToggleCaptureAsync()
    {
        if (IsCapturing)
        {
            await StopCaptureAsync();
        }
        else
        {
            await StartCaptureAsync();
        }
    }

    private async Task StartCaptureAsync()
    {
        try
        {
            DebugBootstrapLog.Log("StartCaptureAsync: Checking prerequisites");
            
            if (_audioCapture == null)
            {
                DebugBootstrapLog.Log("StartCaptureAsync: Audio capture NULL");
                StatusMessage = "Audio capture not initialized";
                return;
            }

            if (_whisper == null || !_whisper.IsInitialized)
            {
                DebugBootstrapLog.Log($"StartCaptureAsync: Whisper not ready (whisper={_whisper != null}, initialized={_whisper?.IsInitialized})");
                // If Whisper not ready, allow capture and use Vosk stub (demo) so overlay still shows text
                DebugBootstrapLog.Log("Whisper not initialized - using Vosk demo stub for recognition");
                StatusMessage = "Model not initialized - using demo ASR fallback";
                // continue - capture will start and audio frames will be fed to the stub as well
            }

            DebugBootstrapLog.Log("StartCaptureAsync: All checks passed, starting capture");
            
            _subtitleEngine.Clear();
            Subtitles.Clear();
            CurrentTranscription = string.Empty;

            // Initialize overlay window when starting capture
            if (_winFormsOverlay == null)
            {
                try
                {
                    DebugBootstrapLog.Log("StartCapture: Creating WinForms overlay");
                    _winFormsOverlay = new Platforms.Windows.WinFormsOverlayManager();
                    _winFormsOverlay.Initialize();
                    DebugBootstrapLog.Log("StartCaptureAsync: Overlay window initialized");
                }
                catch (Exception ex)
                {
                    DebugBootstrapLog.Log($"StartCaptureAsync: Overlay init failed: {ex}");
                    StatusMessage = $"Overlay failed: {ex.Message}";
                    // Continue anyway - can work without overlay
                }
            }

            DebugBootstrapLog.Log("StartCaptureAsync: Starting audio capture");
            await _audioCapture.StartAsync();
            IsCapturing = true;
            StatusMessage = "🔴 CAPTURING...";
            DebugBootstrapLog.Log("StartCaptureAsync: Audio capture started successfully");
        }
        catch (Exception ex)
        {
            DebugBootstrapLog.Log($"StartCaptureAsync: Exception: {ex}");
            StatusMessage = $"Capture error: {ex.Message}";
        }
    }

    private async Task StopCaptureAsync()
    {
        if (_audioCapture == null)
            return;

        try
        {
            await _audioCapture.StopAsync();
            _subtitleEngine.FlushCurrentLine();
            IsCapturing = false;
            StatusMessage = "Stopped";
            
            // Hide overlay when stopped
            _winFormsOverlay?.Hide();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task ExportSRTAsync()
    {
        string srt = _subtitleEngine.ExportSRT();
        if (string.IsNullOrEmpty(srt))
        {
            StatusMessage = "No subtitles to export";
            return;
        }

        try
        {
            string fileName = $"autocap_{DateTime.Now:yyyyMMdd_HHmmss}.srt";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(filePath, srt);

            StatusMessage = $"Exported: {fileName}";

            // Save session
            if (_sessionDb != null)
            {
                await _sessionDb.InitializeAsync();
                var session = new SubtitleSession
                {
                    Title = $"Session {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    SubtitleData = srt,
                    AudioSource = AudioSource,
                    LineCount = _subtitleEngine.GetSubtitleLines().Count
                };
                await _sessionDb.SaveSessionAsync(session);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Export failed: {ex.Message}";
        }
    }

    [RelayCommand]
    public void ClearSubtitles()
    {
        _subtitleEngine.Clear();
        Subtitles.Clear();
        CurrentTranscription = string.Empty;
        StatusMessage = "Subtitles cleared";
    }

    public void UpdateOverlayFontSize(float fontSize)
    {
        // WinForms overlay uses fixed font size
    }

    public void UpdateOverlayPosition(int bottomMargin)
    {
        // WinForms overlay has fixed position
    }

    [RelayCommand]
    public void TestOverlay()
    {
        if (_winFormsOverlay == null)
        {
            // Initialize overlay if not done yet
            try
            {
                _winFormsOverlay = new Platforms.Windows.WinFormsOverlayManager();
                _winFormsOverlay.Initialize();
                StatusMessage = "Overlay window created and initialized";
                
                // Give overlay time to initialize
                Task.Delay(500).Wait();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Overlay init failed: {ex.Message}";
                DebugBootstrapLog.Log($"TestOverlay failed: {ex}");
                return;
            }
        }
        
        try
        {
            // Show test text for 10 seconds (longer for testing)
            _winFormsOverlay.UpdateSubtitle("★ FLOATING SUBTITLE TEST ★\nYou should see ONLY this white text with black shadow.\nNO window background should be visible!");
            _winFormsOverlay.Show();
            StatusMessage = "✓ Test overlay shown! Check bottom of screen. This should be TRANSPARENT floating text only.";
            DebugBootstrapLog.Log("TestOverlay: Showing test subtitle");
            
            // Hide after 10 seconds
            Task.Delay(10000).ContinueWith(_ =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _winFormsOverlay?.Hide();
                    StatusMessage = "Test subtitle hidden";
                    DebugBootstrapLog.Log("TestOverlay: Hidden test subtitle");
                });
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to show overlay: {ex.Message}";
            DebugBootstrapLog.Log($"TestOverlay show failed: {ex}");
        }
    }

    private int _audioFrameCount = 0;
    
    private void OnAudioFrameReceived(object? sender, AudioFrameEventArgs e)
    {
        try
        {
            // Send audio data to Whisper for real-time recognition
            _audioFrameCount++;
            
            // Log every 500 frames to verify audio is coming through
            if (_audioFrameCount % 500 == 0)
            {
                DebugBootstrapLog.Log($"OnAudioFrameReceived: Frame count = {_audioFrameCount}, buffer = {e.ByteCount} bytes");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    StatusMessage = $"Audio frames received: {_audioFrameCount}";
                });
            }
            
            if (_whisper != null)
            {
                _whisper.AcceptWaveform(e.Buffer);
            }
            // Also feed stub recognizer if available
            _voskStub?.AcceptWaveform(e.Buffer);
        }
        catch (Exception ex)
        {
            DebugBootstrapLog.Log($"OnAudioFrameReceived exception: {ex}");
            MainThread.BeginInvokeOnMainThread(() => { StatusMessage = $"Audio processing error: {ex.Message}"; });
        }
    }    private void Whisper_OnPartialResult(object? sender, PartialResultEventArgs e)
    {
        // Show IMMEDIATELY - no filtering, no delays
        if (_winFormsOverlay != null && !string.IsNullOrWhiteSpace(e.Text))
        {
            _winFormsOverlay.UpdateSubtitle(e.Text);
            _winFormsOverlay.Show();
        }
        
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentTranscription = e.Text;
            _subtitleEngine.ProcessPartialResult(e.Text);
            StatusMessage = $"Live: {e.Text}";
        });
    }

    private void Whisper_OnFinalResult(object? sender, FinalResultEventArgs e)
    {
        // Show IMMEDIATELY - no filtering
        if (_winFormsOverlay != null && !string.IsNullOrWhiteSpace(e.Text))
        {
            _winFormsOverlay.UpdateSubtitle(e.Text);
            _winFormsOverlay.Show();
        }
        
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _subtitleEngine.ProcessFinalResult(e.Text);
            StatusMessage = $"Final: {e.Text}";
        });
    }

    private void Whisper_OnError(object? sender, string message)
    {
        DebugBootstrapLog.Log($"Whisper_OnError: {message}");
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusMessage = $"❌ ASR Error: {message}";
        });
    }

    // Vosk stub handlers (demo fallback)
    private void Vosk_OnPartial(string text)
    {
        if (_winFormsOverlay != null && !string.IsNullOrWhiteSpace(text))
        {
            _winFormsOverlay.UpdateSubtitle(text);
            _winFormsOverlay.Show();
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentTranscription = text;
            _subtitleEngine.ProcessPartialResult(text);
            StatusMessage = $"Live (demo): {text}";
        });
    }

    private void Vosk_OnFinal(string text)
    {
        if (_winFormsOverlay != null && !string.IsNullOrWhiteSpace(text))
        {
            _winFormsOverlay.UpdateSubtitle(text);
            _winFormsOverlay.Show();
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            _subtitleEngine.ProcessFinalResult(text);
            StatusMessage = $"Final (demo): {text}";
        });
    }

    private void Vosk_OnError(string message)
    {
        DebugBootstrapLog.Log($"Vosk stub error: {message}");
        MainThread.BeginInvokeOnMainThread(() => { StatusMessage = $"ASR stub error: {message}"; });
    }

    private void OnNewSubtitleLine(object? sender, SubtitleLine line)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Subtitles.Add(new SubtitleLineViewModel
            {
                Index = line.Index,
                Text = line.Text,
                StartTime = line.StartTime,
                EndTime = line.EndTime
            });
            
            // Show complete subtitle line in overlay
            _winFormsOverlay?.UpdateSubtitle(line.Text);
            _winFormsOverlay?.Show();
        });
    }

    private void OnPartialSubtitle(object? sender, string text)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentTranscription = text;
            
            // Show partial subtitle in overlay
            _winFormsOverlay?.UpdateSubtitle(text);
            _winFormsOverlay?.Show();
        });
    }

    [RelayCommand]
    public void IncreaseFontSize()
    {
        _winFormsOverlay?.IncreaseFontSize();
        StatusMessage = $"Font size: {_winFormsOverlay?.GetFontSize():F0}pt";
    }

    [RelayCommand]
    public void DecreaseFontSize()
    {
        _winFormsOverlay?.DecreaseFontSize();
        StatusMessage = $"Font size: {_winFormsOverlay?.GetFontSize():F0}pt";
    }

    [RelayCommand]
    public void MoveOverlayUp()
    {
        _winFormsOverlay?.MoveOverlay(0, -20);
    }

    [RelayCommand]
    public void MoveOverlayDown()
    {
        _winFormsOverlay?.MoveOverlay(0, 20);
    }

    [RelayCommand]
    public void MoveOverlayLeft()
    {
        _winFormsOverlay?.MoveOverlay(-20, 0);
    }

    [RelayCommand]
    public void MoveOverlayRight()
    {
        _winFormsOverlay?.MoveOverlay(20, 0);
    }

    private IAudioCapture? GetPlatformAudioCapture()
    {
#if WINDOWS
        return new WindowsAudioCapture();
#elif MACCATALYST
        return new MacAudioCapture();
#elif ANDROID
        return new AndroidAudioCapture();
#elif IOS
        return new iOSAudioCapture();
#else
        return null;
#endif
    }
}

public class SubtitleLineViewModel
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
