namespace AUTOCAP.App.Platforms.Linux;

/// <summary>
/// Linux platform - subtitle overlay using X11/Wayland.
/// TODO: Implement using Gtk#, Avalonia, or direct X11/Wayland bindings
/// </summary>
public class LinuxOverlayWindow
{
    // TODO: Implement using:
    // 1. GTK# for window creation and rendering
    // 2. Gtk.Window with TransientFor set to root window
    // 3. Set SKIP_TASKBAR and KEEP_ABOVE hints
    // 4. Use Cairo for subtitle text rendering
    // 5. Or use Avalonia for cross-platform overlay

    public void ShowSubtitle(string text)
    {
        // TODO: Update subtitle text in overlay window
    }

    public void HideOverlay()
    {
        // TODO: Hide overlay
    }
}
