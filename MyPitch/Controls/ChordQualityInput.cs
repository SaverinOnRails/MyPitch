using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using MyPitch.Models;
using MyPitch.ViewModels;

namespace MyPitch.Controls;

public class ChordQualityInput : ContentControl

{
    public static readonly StyledProperty<IList<MultiSelectableItem<ChordQuality>>> IncludedChordQualitiesProperty = AvaloniaProperty.Register<ChordQualityInput, IList<MultiSelectableItem<ChordQuality>>>(nameof(IncludedChordQualities));

    public static readonly StyledProperty<Models.Key> TonicProperty = AvaloniaProperty.Register<ChordQualityInput, Models.Key>(nameof(Tonic));

    public Models.Key Tonic
    {
        get => GetValue(TonicProperty);
        set
        {
            SetValue(TonicProperty, value);
        }
    }
    public IList<MultiSelectableItem<ChordQuality>> IncludedChordQualities
    {
        get => GetValue(IncludedChordQualitiesProperty);
        set
        {
            SetValue(IncludedChordQualitiesProperty, value);
        }
    }

    public ChordQualityInput()
    {
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IncludedChordQualitiesProperty)
        {
            if (change.NewValue is null) return;
            var value = (IList<MultiSelectableItem<ChordQuality>>)change.NewValue;
            foreach (var deg in value)
            {
                deg.PropertyChanged += IncludedChordQualitiesChanged;
            }
        }
    }

    private void IncludedChordQualitiesChanged(object? sender, PropertyChangedEventArgs e)
    {
        Build();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Build();
    }

    void Build()
    {
        var selections = IncludedChordQualities.Where(p => p.IsSelected == true).ToList();
        int maxCols = Math.Min(selections.Count(), 3);

        Grid main = new() { RowDefinitions = new("Auto,*") };

        var uniformGrid = new UniformGrid
        {
            Columns = maxCols,
        };
        Grid.SetRow(uniformGrid, 1);
        for (int j = 0; j < selections.Count(); j++)
        {
            var chordQuality = selections[j];
            var buttonPanel = new PressableBorder() { Tag = chordQuality.Data};
            buttonPanel.Classes.Add("quality-input-button");
            buttonPanel.PointerPressed += buttonPanelPointerPressed;
            var text = new TextBlock
            {
                Text = chordQuality.Label,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            buttonPanel.Child = text;
            uniformGrid.Children.Add(buttonPanel);
        }

        StackPanel topPanel = new() { Height = 50 };
        Grid.SetRow(topPanel, 0);
        Button repeat = new() { Content = "Repeat", Width = 150 };
        repeat.Classes.Add("Destructive");
        topPanel.Children.Add(repeat);

        main.Children.Add(topPanel);
        main.Children.Add(uniformGrid);
        Content = main;
    }

    private void buttonPanelPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is null || ((PressableBorder)sender).Tag is not ChordQuality qual) return;
        var chord = MusicTheory.BuildChord(Tonic, Tonic, qual);
        PlatformServiceProvider.AudioDriver.PlayChord(chord);
    }
}

public class PressableBorder : Border
{
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        PseudoClasses.Add(":pressed");
    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        PseudoClasses.Remove(":pressed");
    }
}
