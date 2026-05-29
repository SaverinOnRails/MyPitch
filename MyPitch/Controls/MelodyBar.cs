using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using MyPitch.Models;
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
            if (incorrectDegs.Contains(i)) ellipse.Fill = new SolidColorBrush(Color.Parse("#C40C0C"));
            canvas.Children.Add(ellipse);
            if (fills.TryGetValue(i, out string? deg))
            {
                var text = new TextBlock
                {
                    Text = deg,
                    FontSize = size * 0.6,
                    Foreground = new SolidColorBrush(Color.Parse("#76D2DB")),
                    FontFamily = CircleOfFifths.NotoSansTypeface.FontFamily,
                };
                Point center = new(size / 2, size / 2);
                text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var desired = text.DesiredSize;
                Canvas.SetLeft(text, center.X - desired.Width / 2);
                Canvas.SetTop(text, center.Y - desired.Height / 2);
                canvas.Children.Add(text);
            }
            root.Children.Add(canvas);
        }
        Content = root;
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
