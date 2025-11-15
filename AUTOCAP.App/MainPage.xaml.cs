using AUTOCAP.App.ViewModels;

namespace AUTOCAP.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        try
        {
            DebugBootstrapLog.Log("MainPage constructor start");
            InitializeComponent();
            DebugBootstrapLog.Log("MainPage InitializeComponent completed");
            // Re-enable MainViewModel now that startup race is mitigated
            try
            {
                BindingContext = new MainViewModel();
                DebugBootstrapLog.Log("MainViewModel assigned to BindingContext");
            }
            catch (Exception ex)
            {
                DebugBootstrapLog.Log($"MainViewModel assignment error: {ex}");
            }
        }
        catch (Exception ex)
        {
            DebugBootstrapLog.Log($"MainPage constructor exception: {ex}");
            throw;
        }
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Views.SettingsPage());
    }

    private async void OnSessionsClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Views.SessionsPage());
    }

    private async void OnHelpClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new Views.HelpPage());
    }
}
