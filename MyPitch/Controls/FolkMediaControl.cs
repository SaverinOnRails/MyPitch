using System;
using Avalonia;
using MyPitch.Converters;
using Avalonia.Controls;
using System.Windows.Input;

namespace MyPitch.Controls;

public class FolkMediaControl : ContentControl
{

    public static readonly StyledProperty<TimeSpan> FolkMediaDurationProperty = AvaloniaProperty.Register<FolkMediaControl, TimeSpan>(nameof(FolkMediaDuration), default);

    public static readonly StyledProperty<ICommand> SeekCommandProperty = AvaloniaProperty.Register<CircleOfFifths, ICommand>(nameof(SeekCommand));

    public ICommand SeekCommand
    {
        get => GetValue(SeekCommandProperty);
        set
        {
            SetValue(SeekCommandProperty, value);
        }
    }

    public TimeSpan FolkMediaDuration
    {
        get => GetValue(FolkMediaDurationProperty);
        set => SetValue(FolkMediaDurationProperty, value);
    }
    public static readonly StyledProperty<TimeSpan> FolkMediaProgressProperty = AvaloniaProperty.Register<FolkMediaControl, TimeSpan>(nameof(FolkMediaProgress), default);

    public TimeSpan FolkMediaProgress
    {
        get => GetValue(FolkMediaProgressProperty);
        set => SetValue(FolkMediaProgressProperty, value);
    }

    public FolkMediaControl()
    {
        var grid = new Grid() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, ColumnDefinitions = new("Auto,0.5*,Auto"), Margin = new(20, 0), ColumnSpacing = 20 };
        var progressbar = new Slider() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
        Grid.SetColumn(progressbar, 1);
        bool limit = false;
        progressbar.ValueChanged += (s, e) =>
        {
            var diff = Math.Abs(e.OldValue - e.NewValue);
            //probably done by the user
            if (limit == true)
            {
                limit = false;
            }
            else
            {
                if (diff > 100)
                {
                    SeekCommand.Execute(e.NewValue);
                    limit = true;
                }
            }
        };
        progressbar.Bind(
            Slider.MaximumProperty,
            new Avalonia.Data.Binding(nameof(FolkMediaDuration))
            {
                Source = this,
                Mode = Avalonia.Data.BindingMode.OneWay,
                Converter = new TimeSpanToMillisecondsConverter()
            });

        progressbar.Bind(
            Slider.ValueProperty,
            new Avalonia.Data.Binding(nameof(FolkMediaProgress))
            {
                Source = this,
                Mode = Avalonia.Data.BindingMode.OneWay,
                Converter = new TimeSpanToMillisecondsConverter()
            });

        var currentLocation = new TextBlock() { Margin = new(0, 15, 0, 0), FontSize = 20 };
        Grid.SetColumn(currentLocation, 0);
        currentLocation.Bind(
            TextBlock.TextProperty,
            new Avalonia.Data.Binding(nameof(FolkMediaProgress))
            {
                Source = this,
                Mode = Avalonia.Data.BindingMode.OneWay,
                StringFormat = "{0:mm\\:ss}"
            });


        var totalDuration = new TextBlock() { FontSize = 20, Margin = new(0, 15, 0, 0) };
        Grid.SetColumn(totalDuration, 2);
        totalDuration.Bind(
            TextBlock.TextProperty,
            new Avalonia.Data.Binding(nameof(FolkMediaDuration))
            {
                Source = this,
                Mode = Avalonia.Data.BindingMode.OneWay,
                StringFormat = "{0:mm\\:ss}"
            });
        grid.Children.Add(currentLocation);
        grid.Children.Add(progressbar);
        grid.Children.Add(totalDuration);
        Content = grid;
    }
}
