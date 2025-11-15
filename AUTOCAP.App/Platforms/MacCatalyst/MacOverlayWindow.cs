namespace AUTOCAP.App.Platforms.MacCatalyst;

/// <summary>
/// macOS/MacCatalyst platform - overlay window for subtitle display.
/// TODO: Implement using NSWindow with transparent background
/// </summary>
public class MacOverlayWindow
{
    // TODO: Implement using:
    // 1. NSWindow with NSWindowStyleMaskBorderless
    // 2. setOpaque(false) and backgroundColor = NSColor.clearColor
    // 3. setLevel(NSWindowLevelFloatingWindow)
    // 4. Add NSTextField or NSView subclass for text rendering
    // 5. Implement window positioning and update logic

    public void ShowSubtitle(string text)
    {
        // TODO: Update subtitle text in overlay
    }

    public void HideOverlay()
    {
        // TODO: Hide overlay
    }
}
