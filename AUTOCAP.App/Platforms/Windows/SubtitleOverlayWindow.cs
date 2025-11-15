namespace AUTOCAP.App.Platforms.Windows;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System.Runtime.InteropServices;
using WinRT.Interop;

/// <summary>
/// Windows platform - transparent overlay window for subtitle display.
/// Always-on-top window that shows subtitles over all other applications.
/// </summary>
public class SubtitleOverlayWindow : IDisposable
{
    private Window? _overlayWindow;
    private TextBlock? _subtitleText;
    private string _currentText = string.Empty;
    
    // Win32 API imports for window manipulation
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;

    public void Initialize()
    {
        // Create a borderless, completely transparent overlay window
        _overlayWindow = new Window
        {
            Title = "AUTOCAP Subtitles"
        };
        
        // Remove system backdrop for full transparency
        _overlayWindow.SystemBackdrop = null;
        
        // Create subtitle text block with drop shadow for readability
        _subtitleText = new TextBlock
        {
            Text = "",
            FontSize = 48, // Larger for better visibility
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(40, 0, 40, 80) // Bottom margin for subtitle positioning
        };
        
        // Add text shadow/outline effect for readability on any background
        _subtitleText.Shadow = new Microsoft.UI.Xaml.Media.ThemeShadow();
        
        // Transparent grid - no visible background
        var grid = new Grid
        {
            Background = new SolidColorBrush(Colors.Transparent),
            Children = { _subtitleText }
        };
        
        _overlayWindow.Content = grid;
        
        // Activate window (but it will be invisible until text is shown)
        _overlayWindow.Activate();
        
        // Get window handle and make it fullscreen, topmost, and click-through
        var hwnd = WindowNative.GetWindowHandle(_overlayWindow);
        
        // Get screen dimensions
        int screenWidth = (int)Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Width;
        int screenHeight = (int)Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Height;
        
        // Make fullscreen and topmost
        SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, screenWidth, screenHeight, SWP_NOACTIVATE | SWP_SHOWWINDOW);
        
        // Make window click-through (transparent to mouse and keyboard)
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
    }

    public void ShowSubtitle(string text)
    {
        if (_subtitleText == null || _overlayWindow == null) return;
        
        _currentText = text;
        
        // Update on UI thread
        _overlayWindow.DispatcherQueue.TryEnqueue(() =>
        {
            _subtitleText.Text = text;
            _overlayWindow.Activate(); // Ensure visible
        });
    }

    public void HideOverlay()
    {
        if (_overlayWindow == null) return;
        
        _overlayWindow.DispatcherQueue.TryEnqueue(() =>
        {
            _subtitleText.Text = "";
        });
    }
    
    public void SetPosition(int x, int y, int width, int height)
    {
        if (_overlayWindow == null) return;
        
        var hwnd = WindowNative.GetWindowHandle(_overlayWindow)!;
        SetWindowPos(hwnd, HWND_TOPMOST, x, y, width, height, SWP_NOACTIVATE | SWP_SHOWWINDOW);
    }

    public void Dispose()
    {
        _overlayWindow?.Close();
        _overlayWindow = null;
    }
}
