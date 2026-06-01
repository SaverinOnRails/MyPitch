using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace MyPitch.Converters;

public class TimeSpanToMillisecondsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is TimeSpan ts ? ts.TotalMilliseconds : 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is double ms ? TimeSpan.FromMilliseconds(ms) : TimeSpan.Zero;
    }
}
