using System;
using Avalonia;
using MyPitch.Converters;
using Avalonia.Controls;
using System.Windows.Input;
using Avalonia.Interactivity;

namespace MyPitch.Controls;

public class FolkMediaControl : ContentControl
{

    public static readonly StyledProperty<TimeSpan> FolkMediaDurationProperty = AvaloniaProperty.Register<FolkMediaControl, TimeSpan>(nameof(FolkMediaDuration), default);

    public static readonly StyledProperty<ICommand> SeekCommandProperty = AvaloniaProperty.Register<FolkMediaControl, ICommand>(nameof(SeekCommand));

    public static readonly StyledProperty<float> PlaybackSpeedProperty = AvaloniaProperty.Register<FolkMediaControl, float>(nameof(PlaybackSpeed));

    public float PlaybackSpeed
    {
        get => GetValue(PlaybackSpeedProperty);
        set => SetValue(PlaybackSpeedProperty, value);
    }
    //speed controls
    private Button _point25xButton;
    private Button _point5xButton;
    private Button _oneXButton;

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
        StackPanel mainStackPanel = new() { Orientation = Avalonia.Layout.Orientation.Vertical };
        var grid = new Grid() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, ColumnDefinitions = new("Auto,0.5*,Auto"), Margin = new(20, 0), ColumnSpacing = 20 };
        mainStackPanel.Children.Add(grid);
        var progressbar = new Slider() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
        Grid.SetColumn(progressbar, 1);
        bool limit = false;
        progressbar.ValueChanged += (s, e) =>
        {
            var diff = Math.Abs(e.OldValue - e.NewValue);
            if (limit == true)
            {
                limit = false;
            }
            else
            {
                //probably done by the user
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

        //Toggle button set, this implementation is a bit crude
        var multiToggleButtonSetStackPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
        _point25xButton = new Button() { Content = "0.25x", Tag = 0.25f };
        _point25xButton.Click += SetPlaybackSpeed;
        _point25xButton.CornerRadius = new(10, 0, 0, 10);
        _point25xButton.Classes.Add("Secondary");

        _point5xButton = new Button() { Content = "0.5x", Tag = 0.5f };
        _point5xButton.Click += SetPlaybackSpeed;
        _point5xButton.CornerRadius = new(0, 0, 0, 0);
        _point5xButton.Classes.Add("Secondary");

        _oneXButton = new Button() { Content = "1x", Tag = 1.0f };
        _oneXButton.Click += SetPlaybackSpeed;
        _oneXButton.CornerRadius = new(0, 10, 10, 0);
        _oneXButton.Classes.Add("Primary");


        multiToggleButtonSetStackPanel.Children.Add(_point25xButton);
        multiToggleButtonSetStackPanel.Children.Add(_point5xButton);
        multiToggleButtonSetStackPanel.Children.Add(_oneXButton);
        mainStackPanel.Children.Add(multiToggleButtonSetStackPanel);
        Content = mainStackPanel;
    }

    private void SetPlaybackSpeed(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;
        if (button.Tag is null) return;

        var tag = (float)button.Tag;
        Button[] buttons = [_point25xButton, _point5xButton, _oneXButton];
        PlaybackSpeed = tag;
        foreach (var b in buttons)
        {
            if (b.Tag is null) continue;
            if ((float)b.Tag != tag)
            {
                b.Classes.Remove("Primary");
                b.Classes.Add("Secondary");
            }
            else
            {
                b.Classes.Remove("Secondary");
                b.Classes.Add("Primary");
            }
        }
    }
}
