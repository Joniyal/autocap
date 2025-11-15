using Microsoft.Maui.ApplicationModel;

namespace AUTOCAP.App;

public partial class App : Application
{
    public App()
    {
        try
        {
            DebugBootstrapLog.Log("App constructor start");
            InitializeComponent();
            DebugBootstrapLog.Log("InitializeComponent completed");
            
            // Simple synchronous assignment - standard MAUI pattern
            MainPage = new MainPage();
            DebugBootstrapLog.Log("MainPage assigned in constructor");
        }
        catch (Exception ex)
        {
            DebugBootstrapLog.Log($"App constructor exception: {ex}");
            throw;
        }
    }
}
