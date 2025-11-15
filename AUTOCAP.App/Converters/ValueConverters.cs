// Value converters for XAML bindings
using Microsoft.Maui.Controls;
using System.Globalization;

namespace AUTOCAP.App.Converters;

/// <summary>
/// Converts boolean to Color (true = green, false = red)
/// Used for capture button color feedback
/// </summary>
public class IsCapturingColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isCapturing)
        {
            return isCapturing ? Color.FromArgb("#d32f2f") : Color.FromArgb("#0d47a1");
        }
        return Color.FromArgb("#0d47a1");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts boolean to inverted value
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !(value as bool? ?? false);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !(value as bool? ?? false);
    }
}
