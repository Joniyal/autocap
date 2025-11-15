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
    
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const uint SWP_HIDEWINDOW = 0x0080;
    private const int SW_HIDE = 0;
    private const int SW_SHOWNOACTIVATE = 4;

    public void Initialize()
    {
        // Create a borderless, completely transparent overlay window
        _overlayWindow = new Window
        {
            Title = "AUTOCAP Subtitles"
        };
        
        // Remove system backdrop for full transparency
        _overlayWindow.SystemBackdrop = null;
        
        // Create subtitle text - PURE TEXT with outline effect, no background box
        _subtitleText = new TextBlock
        {
            Text = "",
            FontSize = 36,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Padding = new Thickness(20)
        };
        
        // Add text stroke/outline for visibility on any background
        // This creates the effect like in the screenshot
        var strokeGeometry = new Microsoft.UI.Xaml.Media.GeometryGroup();
        
        // Fully transparent grid - NO BACKGROUND AT ALL
        var grid = new Grid
        {
            Background = new SolidColorBrush(Colors.Transparent),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 80),
            MaxWidth = 1400
        };
        grid.Children.Add(_subtitleText);
        
        _overlayWindow.Content = grid;
        
        // Activate window (but hide it initially)
        _overlayWindow.Activate();
        
        // Get window handle
        var hwnd = WindowNative.GetWindowHandle(_overlayWindow);
        
        // Get screen dimensions
        int screenWidth = (int)Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Width;
        int screenHeight = (int)Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Height;
        
        // Position at bottom of screen - only 120px tall for text
        int subtitleHeight = 120;
        int yPosition = screenHeight - subtitleHeight;
        SetWindowPos(hwnd, HWND_TOPMOST, 0, yPosition, screenWidth, subtitleHeight, SWP_NOACTIVATE);
        
        // Make window click-through (transparent to mouse and keyboard)
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        
        // Hide window initially - will only show when there's subtitle text
        ShowWindow(hwnd, SW_HIDE);
    }

    public void ShowSubtitle(string text)
    {
        if (_subtitleText == null || _overlayWindow == null) return;
        
        _currentText = text;
        
        // Update on UI thread
        _overlayWindow.DispatcherQueue.TryEnqueue(() =>
        {
            _subtitleText.Text = text;
            
            // Only show window if there's actual text to display
            if (!string.IsNullOrWhiteSpace(text))
            {
                var hwnd = WindowNative.GetWindowHandle(_overlayWindow);
                ShowWindow(hwnd, SW_SHOWNOACTIVATE);
            }
        });
    }

    public void HideOverlay()
    {
        if (_overlayWindow == null) return;
        
        _overlayWindow.DispatcherQueue.TryEnqueue(() =>
        {
            _subtitleText.Text = "";
            
            // Hide the window when no text
            var hwnd = WindowNative.GetWindowHandle(_overlayWindow);
            ShowWindow(hwnd, SW_HIDE);
        });
    }
    
    public void SetPosition(int x, int y, int width, int height)
    {
        if (_overlayWindow == null) return;
        
        var hwnd = WindowNative.GetWindowHandle(_overlayWindow)!;
        SetWindowPos(hwnd, HWND_TOPMOST, x, y, width, height, SWP_NOACTIVATE | SWP_SHOWWINDOW);
    }
    
    public void SetFontSize(float size)
    {
        if (_subtitleText == null) return;
        
        _overlayWindow?.DispatcherQueue.TryEnqueue(() =>
        {
            _subtitleText.FontSize = size;
        });
    }
    
    public void SetTextColor(byte r, byte g, byte b)
    {
        if (_subtitleText == null) return;
        
        _overlayWindow?.DispatcherQueue.TryEnqueue(() =>
        {
            _subtitleText.Foreground = new SolidColorBrush(ColorHelper.FromArgb(255, r, g, b));
        });
    }

    public void Dispose()
    {
        _overlayWindow?.Close();
        _overlayWindow = null;
    }
}
