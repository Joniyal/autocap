using System.IO;

namespace AUTOCAP.App;

public static class DebugBootstrapLog
{
    public static void Log(string message)
    {
        try
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "autocap_bootstrap.log");
            File.AppendAllText(path, DateTime.Now.ToString("o") + " - " + message + "\n");
        }
        catch
        {
            // ignore
        }
    }
}
