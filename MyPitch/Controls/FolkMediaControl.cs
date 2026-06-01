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
        var stackPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Spacing = 10 };
        var progressbar = new Slider() { Width = 400 };
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
                if (diff > 20)
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
        currentLocation.Bind(
            TextBlock.TextProperty,
            new Avalonia.Data.Binding(nameof(FolkMediaProgress))
            {
                Source = this,
                Mode = Avalonia.Data.BindingMode.OneWay,
                StringFormat = "{0:mm\\:ss}"
            });


        var totalDuration = new TextBlock() { FontSize = 20, Margin = new(0, 15, 0, 0) };
        totalDuration.Bind(
            TextBlock.TextProperty,
            new Avalonia.Data.Binding(nameof(FolkMediaDuration))
            {
                Source = this,
                Mode = Avalonia.Data.BindingMode.OneWay,
                StringFormat = "{0:mm\\:ss}"
            });
        stackPanel.Children.Add(currentLocation);
        stackPanel.Children.Add(progressbar);
        stackPanel.Children.Add(totalDuration);
        Content = stackPanel;
    }
}
