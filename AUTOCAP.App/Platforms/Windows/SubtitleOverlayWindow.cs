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
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const int GWL_EXSTYLE = -20;
    private const int GWL_STYLE = -16;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const uint SWP_HIDEWINDOW = 0x0080;
    private const int SW_HIDE = 0;
    private const int SW_SHOWNA = 8;

    public void Initialize()
    {
        // Create an independent overlay window that won't be affected by parent
        _overlayWindow = new Window
        {
            Title = "AUTOCAP Subtitles"
        };
        
        // Make window completely borderless - no title bar, no buttons
        _overlayWindow.ExtendsContentIntoTitleBar = true;
        _overlayWindow.AppWindow.TitleBar.IconShowOptions = Microsoft.UI.Windowing.IconShowOptions.HideIconAndSystemMenu;
        _overlayWindow.AppWindow.SetPresenter(Microsoft.UI.Windowing.AppWindowPresenterKind.CompactOverlay);
        
        // Remove system backdrop for full transparency
        _overlayWindow.SystemBackdrop = null;
        
        // Create subtitle text - white bold text like movie subtitles
        _subtitleText = new TextBlock
        {
            Text = "",
            FontSize = 42,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White),
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Padding = new Thickness(16, 6, 16, 6)
        };
        
        // Semi-transparent black background behind text (like real subtitles)
        var textBackground = new Border
        {
            Background = new SolidColorBrush(ColorHelper.FromArgb(180, 0, 0, 0)), // 70% opacity black
            CornerRadius = new CornerRadius(4),
            Child = _subtitleText,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        // Add right-click menu for easy control
        var menuFlyout = new Microsoft.UI.Xaml.Controls.MenuFlyout();
        var hideItem = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem { Text = "Hide Subtitles" };
        hideItem.Click += (s, e) => HideOverlay();
        menuFlyout.Items.Add(hideItem);
        textBackground.ContextFlyout = menuFlyout;
        
        // Fully transparent grid container
        var grid = new Grid
        {
            Background = new SolidColorBrush(Colors.Transparent),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MaxWidth = 1600,
            Padding = new Thickness(40, 20, 40, 20)
        };
        grid.Children.Add(textBackground);
        
        _overlayWindow.Content = grid;
        
        // Activate window
        _overlayWindow.Activate();
        
        // Get window handle
        var hwnd = WindowNative.GetWindowHandle(_overlayWindow);
        
        // Get screen dimensions
        int screenWidth = (int)Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Width;
        int screenHeight = (int)Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Height;
        
        // Position at bottom center of screen - 150px tall subtitle area
        int subtitleHeight = 150;
        int yPosition = screenHeight - subtitleHeight - 100; // 100px from bottom (visible by default)
        
        // CRITICAL: Make window independent - won't minimize with parent
        // WS_EX_TOOLWINDOW: Hides from taskbar
        // WS_EX_NOACTIVATE: Never takes focus
        // WS_EX_TOPMOST: Always on top
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE);
        
        // Position window at bottom center, ALWAYS TOPMOST
        SetWindowPos(hwnd, HWND_TOPMOST, 0, yPosition, screenWidth, subtitleHeight, SWP_NOACTIVATE | SWP_SHOWWINDOW);
        
        // Start hidden - will show when text appears
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
                
                // Force window to be ALWAYS ON TOP and VISIBLE
                SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
                
                // Show the window without activating it
                ShowWindow(hwnd, SW_SHOWNA);
                
                // Force topmost again to ensure it stays above everything
                SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
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
