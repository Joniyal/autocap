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
            DebugBootstrapLog.Log($"UNHANDLED EXCEPTION: {e.ExceptionObject}");
        };
        
        Microsoft.UI.Xaml.Application.Current.UnhandledException += (s, e) =>
        {
            DebugBootstrapLog.Log($"XAML UNHANDLED EXCEPTION: {e.Exception}");
            e.Handled = true; // Prevent crash
        };
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
