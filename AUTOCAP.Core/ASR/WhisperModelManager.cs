using Whisper.net.Ggml;

namespace AUTOCAP.Core.ASR;

/// <summary>
/// Manages Whisper model downloads and storage
/// </summary>
public class WhisperModelManager
{
    private readonly string _modelDirectory;

    public WhisperModelManager(string modelDirectory)
    {
        _modelDirectory = modelDirectory;
        Directory.CreateDirectory(_modelDirectory);
    }

    public string? GetModelPath(string modelName = "ggml-base.bin")
    {
        string path = Path.Combine(_modelDirectory, modelName);
        return File.Exists(path) ? path : null;
    }

    /// <summary>
    /// Download Whisper base model (fast and accurate, 142MB)
    /// </summary>
    public async Task<bool> DownloadBaseModelAsync(Action<float>? onProgress = null)
    {
        try
        {
            string modelPath = Path.Combine(_modelDirectory, "ggml-base.bin");
            
            if (File.Exists(modelPath))
            {
                onProgress?.Invoke(1.0f);
                return true;
            }

            // Download using Whisper.net's built-in downloader
            var ggmlType = GgmlType.Base;
            await using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
            
            await using var fileStream = File.Create(modelPath);
            
            var buffer = new byte[8192];
            long totalBytes = 0;
            long expectedBytes = 142_000_000; // ~142MB for base model
            
            int bytesRead;
            while ((bytesRead = await modelStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalBytes += bytesRead;
                onProgress?.Invoke((float)totalBytes / expectedBytes);
            }

            onProgress?.Invoke(1.0f);
            return true;
        }
        catch
        {
            // Write a small error file so the UI can display details
            try
            {
                var errPath = Path.Combine(_modelDirectory, "whisper_download_error.txt");
                await File.WriteAllTextAsync(errPath, "Download failed (see network or permissions)");
            }
            catch { }
            return false;
        }
    }

    /// <summary>
    /// Download tiny model (fastest, 75MB, less accurate)
    /// </summary>
    public async Task<bool> DownloadTinyModelAsync(Action<float>? onProgress = null)
    {
        try
        {
            string modelPath = Path.Combine(_modelDirectory, "ggml-tiny.bin");
            
            if (File.Exists(modelPath))
            {
                onProgress?.Invoke(1.0f);
                return true;
            }

            var ggmlType = GgmlType.Tiny;
            await using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
            
            await using var fileStream = File.Create(modelPath);
            await modelStream.CopyToAsync(fileStream);

            onProgress?.Invoke(1.0f);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
