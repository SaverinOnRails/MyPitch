using System;
using System.Globalization;
using Avalonia.Data.Converters;
using MyPitch.Models;

namespace MyPitch.Converters;

public class GameModeDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GameMode mode)
        {
            if (mode == GameMode.ChordQuality)
            {
                return "Chord Quality";
            }
            if (mode == GameMode.ChordProgression)
            {
                return "Chord Progression";
            }
        }
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
