using Avalonia.Data.Converters;
using MyPitch.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MyPitch.Converters;

internal class ScaleModeDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return "";
        var mode = (ScaleMode)value;
        if (mode == ScaleMode.Ionian) return "Ionian (Major Scale)";
        if (mode == ScaleMode.Aeolian) return "Aeolian (Natural Minor)";
        return mode.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
