using Avalonia.Data.Converters;
using MyPitch.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MyPitch.Converters;

internal class TonicDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return null;
        var key = ((Key)value).ToString();
        return key.Length > 1 ? key[0] + "♭" : key;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
