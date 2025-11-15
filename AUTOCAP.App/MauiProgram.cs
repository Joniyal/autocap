using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using CommunityToolkit.Maui;

namespace AUTOCAP.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Fonts are optional for demo
                try
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSans");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                }
                catch
                {
                    // Use system fonts if custom fonts not available
                }
            })
            .UseMauiCommunityToolkit();

        return builder.Build();
    }
}
