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
        // Create overlay window on UI thread
        _overlayWindow = new Window
        {
            Title = "AUTOCAP Subtitles",
            SystemBackdrop = new MicaBackdrop() // Transparent mica effect
        };
        
        // Create subtitle text block with shadow effect
        _subtitleText = new TextBlock
        {
            Text = "",
            FontSize = 32,
            // FontWeight = Bold (set via styles if needed)
            Foreground = new SolidColorBrush(Colors.White),
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(20, 20, 20, 100) // Bottom margin for typical subtitle position
        };
        
        // Black semi-transparent background
        var background = new Border
        {
            Background = new SolidColorBrush(ColorHelper.FromArgb(180, 0, 0, 0)),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(20),
            Child = _subtitleText
        };
        
        var grid = new Grid
        {
            Children = { background }
        };
        
        _overlayWindow.Content = grid;
        
        // Set window size and position at bottom of screen
        _overlayWindow.Activate();
        
        // Make window topmost and click-through
        var hwnd = WindowNative.GetWindowHandle(_overlayWindow);
        SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        
        // Make window click-through (transparent to mouse)
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
