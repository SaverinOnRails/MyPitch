using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using MyPitch.Models;
using System.Collections.Generic;
namespace MyPitch.Controls;

internal class MelodyBar : ContentControl
{
    public static readonly StyledProperty<int> NoteCountProperty = AvaloniaProperty.Register<MelodyBar, int>(nameof(NoteCount), 0);
    public static readonly StyledProperty<MelodyBarState> BarStateProperty = AvaloniaProperty.Register<MelodyBar, MelodyBarState>(nameof(BarState));
    public int NoteCount
    {
        get => GetValue(NoteCountProperty);
        set
        {
            SetValue(NoteCountProperty, value);
        }
    }
    public MelodyBarState BarState
    {
        get => GetValue(BarStateProperty);
        set
        {
            SetValue(BarStateProperty, value);
        }
    }
    public MelodyBar()
    {
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        Margin = new(10);
    }
    public void Build()
    {
        var size = 50;
        var fills = BarState?.UserChoices ?? new();
        var incorrectDegs = BarState?.IncorrectChoices ?? new();
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
                Width = size,
                Height = size
            };
            var ellipse = new Ellipse
            {
                Stroke = new SolidColorBrush(Color.Parse("#C44545")),
                StrokeThickness = 1,
                Width = size,
                Height = size
            };
            if(incorrectDegs.Contains(i)) ellipse.Fill = new SolidColorBrush(Color.Parse("#C40C0C"));
            canvas.Children.Add(ellipse);
            if (fills.TryGetValue(i, out string? deg))
            {
                var text = new TextBlock
                {
                    Text = deg,
                    FontSize = size * 0.667,
                    Foreground = new SolidColorBrush(Color.Parse("#76D2DB")),
                };
                //Canvas.SetTop(text, 15);
                Canvas.SetLeft(text, deg.Length == 1 ? size * 0.333 : size * 0.167);
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
        if (change.Property == NoteCountProperty || change.Property == BarStateProperty)
        {
            Build();
        }
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Build();
    }
}
