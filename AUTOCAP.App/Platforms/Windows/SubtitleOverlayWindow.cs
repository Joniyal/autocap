namespace AUTOCAP.App.Platforms.Windows;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI;
using System.Runtime.InteropServices;
using WinRT.Interop;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas;

/// <summary>
/// TRUE transparent overlay using Win2D canvas rendering - works with any Windows theme.
/// Renders text directly to pixels with full alpha channel control.
/// </summary>
public class SubtitleOverlayWindow : IDisposable
{
    private Window? _overlayWindow;
    private CanvasControl? _canvas;
    private string _currentText = "";
    
    // Win32 APIs
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    
    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const uint LWA_COLORKEY = 0x00000001;

    public void Initialize()
    {
        // Create window
        _overlayWindow = new Window();
        _overlayWindow.SystemBackdrop = null;
        
        // Remove title bar
        _overlayWindow.ExtendsContentIntoTitleBar = true;
        _overlayWindow.AppWindow.TitleBar.IconShowOptions = Microsoft.UI.Windowing.IconShowOptions.HideIconAndSystemMenu;
        
        // Create Win2D canvas for custom rendering
        _canvas = new CanvasControl();
        _canvas.Draw += Canvas_Draw;
        _canvas.ClearColor = Colors.Black;
        
        _overlayWindow.Content = _canvas;
        _overlayWindow.Activate();
        
        // Get window handle
        var hwnd = WindowNative.GetWindowHandle(_overlayWindow);
        
        // Get screen dimensions
        int screenWidth = (int)Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Width;
        int screenHeight = (int)Microsoft.Maui.Devices.DeviceDisplay.Current.MainDisplayInfo.Height;
        
        // Position at bottom
        int windowHeight = 200;
        int yPosition = screenHeight - windowHeight - 100;
        
        // Apply window styles for transparency
        int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT);
        
        // Use color key transparency - make BLACK (0x00000000) invisible
        SetLayeredWindowAttributes(hwnd, 0x00000000, 255, LWA_COLORKEY);
        
        // Position window
        SetWindowPos(hwnd, HWND_TOPMOST, 0, yPosition, screenWidth, windowHeight, SWP_NOACTIVATE | SWP_SHOWWINDOW);
    }

    private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        if (string.IsNullOrWhiteSpace(_currentText))
        {
            // Draw nothing - fully transparent
            return;
        }
        
        var canvas = args.DrawingSession;
        
        // Canvas size
        float width = (float)sender.ActualWidth;
        float height = (float)sender.ActualHeight;
        
        // Text format - centered, wrapped
        var textFormat = new CanvasTextFormat
        {
            FontSize = 48,
            FontFamily = "Segoe UI",
            HorizontalAlignment = CanvasHorizontalAlignment.Center,
            VerticalAlignment = CanvasVerticalAlignment.Center,
            WordWrapping = CanvasWordWrapping.Wrap
        };
        
        // Calculate text position (center)
        float x = width / 2;
        float y = height / 2;
        
        // Draw dark shadow/outline (NOT black - black is transparent!)
        var shadowColor = Microsoft.UI.ColorHelper.FromArgb(255, 32, 32, 32);
        for (int offsetX = -2; offsetX <= 2; offsetX++)
        {
            for (int offsetY = -2; offsetY <= 2; offsetY++)
            {
                if (offsetX != 0 || offsetY != 0)
                {
                    canvas.DrawText(
                        _currentText,
                        x + offsetX,
                        y + offsetY,
                        shadowColor,
                        textFormat
                    );
                }
            }
        }
        
        // Draw main white text on top
        canvas.DrawText(_currentText, x, y, Colors.White, textFormat);
    }

    public void ShowSubtitle(string text)
    {
        if (_canvas == null || _overlayWindow == null) return;
        
        _overlayWindow.DispatcherQueue.TryEnqueue(() =>
        {
            _currentText = text;
            _canvas.Invalidate(); // Redraw canvas
            
            if (!string.IsNullOrWhiteSpace(text))
            {
                var hwnd = WindowNative.GetWindowHandle(_overlayWindow);
                SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
            }
        });
    }

    public void HideOverlay()
    {
        if (_overlayWindow == null || _canvas == null) return;
        
        _overlayWindow.DispatcherQueue.TryEnqueue(() =>
        {
            _currentText = "";
            _canvas.Invalidate();
        });
    }

    public void SetFontSize(float size)
    {
        // Font size is handled in Canvas_Draw - could add a property if needed
    }

    public void SetPosition(int x, int y, int width, int height)
    {
        if (_overlayWindow == null) return;
        var hwnd = WindowNative.GetWindowHandle(_overlayWindow);
        SetWindowPos(hwnd, HWND_TOPMOST, x, y, width, height, SWP_NOACTIVATE | SWP_SHOWWINDOW);
    }

    public void Dispose()
    {
        if (_canvas != null)
        {
            _canvas.RemoveFromVisualTree();
            _canvas = null;
        }
        _overlayWindow?.Close();
        _overlayWindow = null;
    }
}
