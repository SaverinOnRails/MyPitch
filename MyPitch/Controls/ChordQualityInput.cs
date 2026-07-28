using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using MyPitch.ViewModels;

namespace MyPitch.Controls;

public class ChordQualityInput : ContentControl
{

    public static readonly StyledProperty<IList<MultiSelectableItem>> IncludedChordQualitiesProperty = AvaloniaProperty.Register<ChordQualityInput, IList<MultiSelectableItem>>(nameof(IncludedChordQualities));

    public IList<MultiSelectableItem> IncludedChordQualities
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
            var value = (IList<MultiSelectableItem>)change.NewValue;
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
        var i = IncludedChordQualities.Count(p => p.IsSelected == true);
        int maxCols = Math.Min(i, 3);

        Grid main = new() { RowDefinitions = new("Auto,*") };

        var uniformGrid = new UniformGrid
        {
            Columns = maxCols,
        };
        Grid.SetRow(uniformGrid, 1);
        for (int j = 0; j < i; j++)
        {
            var buttonPanel = new PressableBorder();
            buttonPanel.Classes.Add("quality-input-button");
            var text = new TextBlock
            {
                Text = IncludedChordQualities[j].Label,
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
