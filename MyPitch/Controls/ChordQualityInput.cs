using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace MyPitch.Controls;

public class ChordQualityInput : ContentControl
{
    public ChordQualityInput()
    {
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
        Build();
    }
    void Build()
    {
        var i = 6;
        int maxCols;
        maxCols = Math.Min(i, 3);
        var main = new UniformGrid
        {
            Columns = maxCols,
        };

        for (int j = 0; j < i; j++)
        {
            var buttonPanel = new PressableBorder();
            buttonPanel.Classes.Add("quality-input-button");
            var text = new TextBlock
            {
                Text = "Major",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            buttonPanel.Child = text;
            main.Children.Add(buttonPanel);
        }

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
