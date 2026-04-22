using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyPitch.Controls;

internal class MelodyBar : ContentControl
{
    public static readonly StyledProperty<int> NoteCountProperty = AvaloniaProperty.Register<MelodyBar, int>(nameof(NoteCount), 0);
    public int NoteCount
    {
        get => GetValue(NoteCountProperty);
        set
        {
            SetValue(NoteCountProperty, value);
        }
    }
    public MelodyBar()
    {
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        Margin = new(10);
    }
    public void Draw(Dictionary<int, string> fills)
    {
        if (NoteCount == 0) return;
        var root = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 10,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
        for (int i = 0; i < NoteCount; i++)
        {
            var canvas = new Canvas
            {
                Width = 30,
                Height = 30
            };
            var ellipse = new Ellipse
            {
                Stroke = new SolidColorBrush(Color.Parse("#C44545")),
                StrokeThickness = 1,
                Width = 30,
                Height = 30
            };
            canvas.Children.Add(ellipse);
            if (fills.TryGetValue(i, out string? deg))
            {
                var text = new TextBlock
                {
                    Text = deg,
                    FontSize = 20,
                    Foreground = new SolidColorBrush(Color.Parse("#76D2DB")),
                };
                //Canvas.SetTop(text, 15);
                Canvas.SetLeft(text, deg.Length == 1 ? 10 : 5);
                canvas.Children.Add(text);
            }
            root.Children.Add(canvas);
        }
        Content = root;
    }
    private void setTextAtEllipse(int index, string text)
    {
    }
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == NoteCountProperty)
        {
            Draw(new());
        }
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Draw(new());
    }
}
