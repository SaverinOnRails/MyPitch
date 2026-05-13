using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace MyPitch.Controls;

internal class CircularProgress : Control
{
    public static readonly StyledProperty<float> ProgressProperty = AvaloniaProperty.Register<CircularProgress, float>(nameof(Progress));
    private float _animationDurationMs = 2000;
    private SolidColorBrush _accentBrush =
        new SolidColorBrush((Color)Application.Current!.Resources["PrimaryColor"]!);
    private DateTime _animationStartTime;
    private float _animationTickMs = 16;
    private DispatcherTimer _animationTimer = new();

    public CircularProgress()
    {
        _animationTimer.Interval = TimeSpan.FromMilliseconds(_animationTickMs);
        _animationTimer.Tick += AnimationTimerTick;
    }

    private void AnimationTimerTick(object? sender, EventArgs e)
    {
        var ellapsedMs = (DateTime.Now - _animationStartTime).TotalMilliseconds;
        double t = Math.Clamp(ellapsedMs / _animationDurationMs, 0.0, 1);
        double eased = CircleOfFifths.EaseInOutCubic(t);
        _displayProgress = (float)(eased * Progress);
        InvalidateVisual();
        if (t >= 1)
        {
            _displayProgress = Progress;
            _animationTimer.Stop();
        }
    }

    public float Progress
    {
        get => GetValue(ProgressProperty);
        set
        {
            SetValue(ProgressProperty, value);
        }
    }
    private float _displayProgress = 0;
    private TopLevel? _toplevel;

    public override void Render(Avalonia.Media.DrawingContext context)
    {
        DrawBackgroundArc(context);
        DrawFillArc(context);
    }
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ProgressProperty)
        {
            StartAnimation();
        }
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _toplevel = TopLevel.GetTopLevel(this);
    }
    private void StartAnimation()
    {
        _animationStartTime = DateTime.Now;
        _animationTimer.Start();
    }

    private void OnToplevelRenderFrame(TimeSpan span)
    {
        var ellapsedMs = (DateTime.Now - _animationStartTime).TotalMilliseconds;
        double t = Math.Clamp(ellapsedMs / _animationDurationMs, 0.0, 1);
        double eased = CircleOfFifths.EaseInOutCubic(t);
        _displayProgress = (float)(eased * Progress);
        InvalidateVisual();
        if (t >= 1)
        {
            _displayProgress = Progress;
            return;
        }
        _toplevel?.RequestAnimationFrame(OnToplevelRenderFrame);
    }

    private void DrawBackgroundArc(DrawingContext context)
    {
        var geo = new StreamGeometry();
        double width = Bounds.Width;
        double height = Bounds.Height;
        Point bottomLeft = new(0, height);
        Point bottomRight = new(width, height);
        var radius = width / 2;
        using var ctx = geo.Open();

        ctx.BeginFigure(bottomLeft, false);
        ctx.ArcTo(bottomRight, new(radius, radius), 0, false, SweepDirection.Clockwise);
        var brush = new SolidColorBrush(Colors.LightGray, 0.7);
        context.DrawGeometry(brush, new Pen(brush, 10,lineCap: PenLineCap.Round), geo);

        string text = $"{Progress:F1}%";
        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface("Arial"),
            25,
            Brushes.White
        );
        // Center horizontally
        double x = (width - formattedText.Width) / 2;
        // Center visually inside arc
        double y = (height - formattedText.Height) / 2;
        context.DrawText(formattedText, new Point(x, y));
    }
    private void DrawFillArc(DrawingContext context)
    {
        var geo = new StreamGeometry();
        double width = Bounds.Width;
        double height = Bounds.Height;
        Point bottomLeft = new(0, height);
        var radius = width / 2;
        using var ctx = geo.Open();
        ctx.BeginFigure(bottomLeft, false);
        var angle = _displayProgress / 100.00 * Math.PI;
        var centerX = radius;
        Point endAt = new(
            radius - Math.Cos(angle) * radius,
            (height - Math.Sin(angle) * radius)
        );
        ctx.ArcTo(endAt, new(radius, radius), 0, false, SweepDirection.Clockwise);
        context.DrawGeometry(_accentBrush, new Pen(_accentBrush, 10, lineCap: PenLineCap.Round), geo);
    }
}

