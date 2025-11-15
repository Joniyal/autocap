namespace AUTOCAP.App.Platforms.iOS;

/// <summary>
/// iOS platform - floating subtitle view.
/// iOS does not allow system-wide overlays for other apps, so this uses in-app floating view only.
/// TODO: Implement using UIView with CADisplayLink for animation
/// </summary>
public class iOSSubtitleFloatingView
{
    // TODO: Implement using:
    // 1. UIView subclass with rounded corners and semi-transparent background
    // 2. CADisplayLink for smooth updates
    // 3. UILabel for subtitle text
    // 4. Pan gesture recognizer to allow repositioning
    // 5. Context: This view only works while app is in foreground

    public void ShowSubtitle(string text)
    {
        // TODO: Display text in floating view
    }

    public void HideView()
    {
        // TODO: Hide view
    }
}
