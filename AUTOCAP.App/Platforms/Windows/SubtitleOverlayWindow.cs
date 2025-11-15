namespace AUTOCAP.App.Platforms.Windows;

using System.Runtime.InteropServices;

/// <summary>
/// Windows platform - transparent overlay window for subtitle display.
/// Uses Win32 API to create an always-on-top window.
/// TODO: Implement using WinUI 3 or custom Win32 interop
/// </summary>
public class SubtitleOverlayWindow
{
    // TODO: Implement using:
    // 1. Win32 CreateWindowEx with WS_EX_TOPMOST and WS_EX_TRANSPARENT
    // 2. Set window class with CS_HREDRAW | CS_VREDRAW
    // 3. Handle WM_PAINT to render subtitle text using GDI+
    // 4. Update window position/size via SetWindowPos
    
    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string className, string windowName);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;

    public void ShowSubtitle(string text)
    {
        // TODO: Implement subtitle rendering in overlay window
    }

    public void HideOverlay()
    {
        // TODO: Hide overlay window
    }
}
