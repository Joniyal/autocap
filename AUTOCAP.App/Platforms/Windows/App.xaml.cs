using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml;

namespace AUTOCAP.App.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
        
        // Add global exception handler to catch crashes
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            try
            {
                var ex = e.ExceptionObject as Exception;
                DebugBootstrapLog.Log($"UNHANDLED EXCEPTION: {ex?.ToString() ?? e.ExceptionObject?.ToString() ?? "Unknown"}");
                DebugBootstrapLog.Log($"IsTerminating: {e.IsTerminating}");
            }
            catch { }
        };
        
        Microsoft.UI.Xaml.Application.Current.UnhandledException += (s, e) =>
        {
            try
            {
                DebugBootstrapLog.Log($"XAML UNHANDLED EXCEPTION: {e.Exception}");
                DebugBootstrapLog.Log($"Message: {e.Message}");
                e.Handled = true; // Prevent crash
            }
            catch { }
        };
        
        // Catch unobserved Task exceptions (async crashes)
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            try
            {
                DebugBootstrapLog.Log($"UNOBSERVED TASK EXCEPTION: {e.Exception}");
                e.SetObserved(); // Prevent crash
            }
            catch { }
        };
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
