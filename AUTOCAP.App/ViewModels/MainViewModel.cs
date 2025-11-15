namespace AUTOCAP.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AUTOCAP.Core.ASR;
using AUTOCAP.Core.Audio;
using AUTOCAP.Core.Subtitle;
using AUTOCAP.Core.Models;
using System.Collections.ObjectModel;

/// <summary>
/// Main view model for the AUTOCAP application.
/// Coordinates audio capture, ASR, subtitle generation, and UI updates.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IAudioCapture? _audioCapture;
    private readonly VoskRecognizer? _vosk;
    private readonly SubtitleEngine _subtitleEngine = new();
    private readonly VoskModelManager? _modelManager;
    private readonly SessionDatabaseService? _sessionDb;
    private Platforms.Windows.SubtitleOverlayWindow? _overlayWindow;

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
            _subtitleEngine = new SubtitleEngine();
            _subtitleEngine.OnNewSubtitleLine += OnNewSubtitleLine;
            _subtitleEngine.OnPartialSubtitle += OnPartialSubtitle;

            string appCacheDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "AUTOCAP");
            Directory.CreateDirectory(appCacheDir);
            
            _modelManager = new VoskModelManager(appCacheDir);

            // Platform-specific audio capture
            _audioCapture = GetPlatformAudioCapture();
            
            // Wire audio capture to send data to recognizer
            if (_audioCapture != null)
            {
                _audioCapture.OnAudioFrame += OnAudioFrameReceived;
            }

            // Initialize Vosk
            _vosk = new VoskRecognizer(16000);
            _vosk.OnPartialResult += Vosk_OnPartialResult;
            _vosk.OnFinalResult += Vosk_OnFinalResult;
            _vosk.OnError += Vosk_OnError;
            
            // Initialize overlay window for Windows
            try
            {
                _overlayWindow = new Platforms.Windows.SubtitleOverlayWindow();
                _overlayWindow.Initialize();
            }
            catch
            {
                // Overlay not available on this platform or initialization failed
            }

            StatusMessage = "Ready to capture";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Initialization error: {ex.Message}";
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
        if (_modelManager == null)
        {
            StatusMessage = "Model manager not initialized";
            return;
        }

        string? modelPath = _modelManager.GetEnglishModelPath();
        
        if (modelPath != null)
        {
            // Model already exists
            bool success = _vosk?.InitializeModel(modelPath) ?? false;
            StatusMessage = success ? "Model loaded successfully" : "Failed to load model";
            return;
        }

        // Download model
        IsDownloadingModel = true;
        StatusMessage = "Downloading model...";
        DownloadProgress = 0f;

        bool downloaded = await _modelManager.DownloadEnglishModelAsync(progress =>
        {
            DownloadProgress = progress;
        });

        IsDownloadingModel = false;

        if (downloaded)
        {
            modelPath = _modelManager.GetEnglishModelPath();
            if (modelPath != null)
            {
                bool success = _vosk?.InitializeModel(modelPath) ?? false;
                StatusMessage = success ? "Model downloaded and loaded" : "Model downloaded but failed to load";
            }
        }
        else
        {
            StatusMessage = "Model download failed. Check internet connection.";
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
        if (_audioCapture == null || _vosk == null)
        {
            StatusMessage = "Audio capture or ASR not initialized";
            return;
        }

        try
        {
            _subtitleEngine.Clear();
            Subtitles.Clear();
            CurrentTranscription = string.Empty;

            await _audioCapture.StartAsync();
            IsCapturing = true;
            StatusMessage = "Capturing...";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
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
            _overlayWindow?.HideOverlay();
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

    private void OnAudioFrameReceived(object? sender, AudioFrameEventArgs e)
    {
        // Send audio data to Vosk for real-time recognition
        _vosk?.AcceptWaveform(e.Buffer);
    }

    private void Vosk_OnPartialResult(object? sender, PartialResultEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentTranscription = e.Text;
            _subtitleEngine.ProcessPartialResult(e.Text);
            
            // Show partial result in overlay window
            _overlayWindow?.ShowSubtitle(e.Text);
        });
    }

    private void Vosk_OnFinalResult(object? sender, FinalResultEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _subtitleEngine.ProcessFinalResult(e.Text);
            
            // Show final result in overlay window
            _overlayWindow?.ShowSubtitle(e.Text);
        });
    }

    private void Vosk_OnError(object? sender, string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusMessage = $"ASR Error: {message}";
        });
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
            _overlayWindow?.ShowSubtitle(line.Text);
        });
    }

    private void OnPartialSubtitle(object? sender, string text)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            CurrentTranscription = text;
            
            // Show partial subtitle in overlay
            _overlayWindow?.ShowSubtitle(text);
        });
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
