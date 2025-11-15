namespace AUTOCAP.Core.Models;

/// <summary>
/// Manages Vosk model downloading and caching.
/// Models are stored in app cache directory and can be updated.
/// </summary>
public class VoskModelManager
{
    private readonly string _modelCacheDir;
    private static readonly string ENGLISH_MODEL_URL = "https://alphacephei.com/vosk/models/vosk-model-small-en-us-0.15.zip";

    public VoskModelManager(string cacheDir)
    {
        _modelCacheDir = Path.Combine(cacheDir, "vosk_models");
        if (!Directory.Exists(_modelCacheDir))
        {
            Directory.CreateDirectory(_modelCacheDir);
        }
    }

    /// <summary>
    /// Get the path to the downloaded English model.
    /// Returns null if not downloaded yet.
    /// </summary>
    public string? GetEnglishModelPath()
    {
        var modelDir = Path.Combine(_modelCacheDir, "vosk-model-small-en-us-0.15");
        if (Directory.Exists(modelDir))
        {
            return modelDir;
        }
        return null;
    }

    /// <summary>
    /// Download English model asynchronously.
    /// Reports progress via the onProgress callback.
    /// </summary>
    public async Task<bool> DownloadEnglishModelAsync(Action<float>? onProgress = null)
    {
        try
        {
            string tempZip = Path.Combine(_modelCacheDir, "model.zip");
            string extractDir = _modelCacheDir;

            using var client = new HttpClient();
            using var response = await client.GetAsync(ENGLISH_MODEL_URL, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            long totalBytes = response.Content.Headers.ContentLength ?? -1;
            bool canReportProgress = totalBytes != -1;

            using (var contentStream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = File.Create(tempZip))
            {
                var buffer = new byte[8192];
                long totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    totalBytesRead += bytesRead;

                    if (canReportProgress)
                    {
                        float percentage = (float)totalBytesRead / totalBytes;
                        onProgress?.Invoke(percentage);
                    }
                }
            }

            // Extract ZIP
            System.IO.Compression.ZipFile.ExtractToDirectory(tempZip, extractDir, overwriteFiles: true);
            File.Delete(tempZip);

            onProgress?.Invoke(1.0f); // 100%
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Model download failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Check if any model is downloaded.
    /// </summary>
    public bool IsModelAvailable()
    {
        return Directory.EnumerateDirectories(_modelCacheDir).Any();
    }
}
