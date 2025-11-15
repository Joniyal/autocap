// Quick start guide and troubleshooting
// Run this from project root to validate setup

using System.Runtime.InteropServices;
using System.Text;

namespace AUTOCAP.Core.Utilities;

/// <summary>
/// Diagnostic utilities for troubleshooting AUTOCAP setup and runtime issues
/// </summary>
public static class Diagnostics
{
    /// <summary>
    /// Get system information and audio source details
    /// </summary>
    public static string GetSystemInfo()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== AUTOCAP System Information ===");
        sb.AppendLine($"OS: {GetOperatingSystem()}");
        sb.AppendLine($"Runtime: {RuntimeInformation.FrameworkDescription}");
        sb.AppendLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
        sb.AppendLine($"Local Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        
        // Try to get app directories if MAUI is available
        try
        {
            var cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AUTOCAP");
            sb.AppendLine($"App Cache Dir: {cacheDir}");
        }
        catch
        {
            sb.AppendLine("App Cache Dir: (unavailable)");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Check if Vosk model is available
    /// </summary>
    public static bool IsVoskModelAvailable(string cacheDir)
    {
        var modelPath = Path.Combine(cacheDir, "vosk-model-small-en-us-0.15");
        return Directory.Exists(modelPath);
    }

    /// <summary>
    /// Get path to available Vosk model
    /// </summary>
    public static string? GetVoskModelPath(string cacheDir)
    {
        var modelPath = Path.Combine(cacheDir, "vosk-model-small-en-us-0.15");
        return Directory.Exists(modelPath) ? modelPath : null;
    }

    private static string GetOperatingSystem()
    {
#if WINDOWS
        return "Windows";
#elif MACCATALYST
        return "macOS";
#elif ANDROID
        return "Android";
#elif IOS
        return "iOS";
#else
        return "Unknown";
#endif
    }
}
