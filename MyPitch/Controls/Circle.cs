using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using MyPitch.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using static MyPitch.PlatformServiceProvider;

namespace MyPitch.Controls;

internal class CircleOfFifths : GameInputControl<string>
{
    private readonly String[] _noteGraduations = MusicTheory.FifthIntervalScaleGraduation;
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _toplevel = TopLevel.GetTopLevel(this)!;
    }

    public static double EaseInOutCubic(double t) => t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;

    private void OnToplevelRenderFrame(TimeSpan span)
    {
        var ellapsedMs = (DateTime.Now - _animationStartTime).TotalMilliseconds;
        double t = Math.Clamp(ellapsedMs / _animationDurationMs, 0.0, 1.0);
        double eased = EaseInOutCubic(t);
        _animationRotationAngle = eased * _animationRotationAngleTarget;
        InvalidateVisual();
        if (t >= 1.0)
        {
            _animationRotationAngle = _animationRotationAngleTarget;
            _animationRotationAngleTarget = 0;
            _displayTonic = Tonic;
            _animationRotationAngle = 0;
            return;
        }
        if (_toplevel is null) return;
        _toplevel.RequestAnimationFrame(OnToplevelRenderFrame);

    }
    private SolidColorBrush[] _degreeBrushes = new SolidColorBrush[] {
      new SolidColorBrush(Color.Parse("#00A933")),
      new SolidColorBrush(Color.Parse("#79D513")),
      new SolidColorBrush(Color.Parse("#FFE400")),
      new SolidColorBrush(Color.Parse("#FFBE00")),
      new SolidColorBrush(Color.Parse("#FF8000")),
      new SolidColorBrush(Color.Parse("#FF3E00")),
      new SolidColorBrush(Color.Parse("#FF0000")),
      new SolidColorBrush(Color.Parse("#C2003D")),
      new SolidColorBrush(Color.Parse("#810081")),
      new SolidColorBrush(Color.Parse("#662B99")),
      new SolidColorBrush(Color.Parse("#336699")),
      new SolidColorBrush(Color.Parse("#198066"))
  };

    private
    const double FIRST_INNER_RADIUS_RATIO = 0.75;
    private
    const double SECOND_INNER_RADIUS_RATIO = 0.65;
    private
    const double THIRD_INNER_RADIUS_RATIO = 0.2;
    private
    const double THIRTY_DEG_RAD = 30 * Math.PI / 180;
    private SolidColorBrush _accentBrush =
        new SolidColorBrush((Color)Application.Current!.Resources["PrimaryColor"]!);

    private double _animationDurationMs;
    private DateTime _animationStartTime;
    private double _animationRotationAngleTarget;
    private double _animationRotationAngle;
    private bool _mouseOnRepeatButton = false;
    public static readonly StyledProperty<Models.GameMode> GameModeProperty = AvaloniaProperty.Register<CircleOfFifths, Models.GameMode>(nameof(GameMode));
    public static readonly StyledProperty<int> OctaveProperty = AvaloniaProperty.Register<CircleOfFifths, int>(nameof(Octave));

    public int Octave
    {
        get => GetValue(OctaveProperty);
        set => SetValue(OctaveProperty, value);
    }
    public GameMode GameMode
    {
        get => GetValue(GameModeProperty);
        set => SetValue(GameModeProperty, value);
    }
    private void IncludedDegreesChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateVisual(); //This might cause issues being done to rapidly
    }
    private int? _userClickedIndex;
    public static Typeface NotoSansTypeface = new Typeface("avares://MyPitch/Assets/Fonts/#Noto Sans");

    private Models.Key _displayTonic = Models.Key.C;

    private int? _mouseOnSegmentIndex = null;
    private TopLevel? _toplevel;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == GameModeProperty)
        {
            InvalidateVisual();
        }
        if (change.Property == TonicProperty)
        {
            var oldTonic = (Models.Key?)change.OldValue;
            var newTonic = Tonic;
            if (oldTonic is null || oldTonic == newTonic) return;
            int oldSegment = MusicTheory.FifthSegment(oldTonic.Value, newTonic.ToString());
            var diff = 12 - oldSegment; //number of segments between them when moving clockwise
            if (diff > 6)
            {
                diff = diff - 12;
            }

            _animationRotationAngleTarget = diff * THIRTY_DEG_RAD;
            _animationDurationMs = Math.Clamp(Math.Abs(diff * 300), 300, 1000);
            _animationStartTime = DateTime.Now;
            _toplevel?.RequestAnimationFrame(OnToplevelRenderFrame);
        }
        base.OnPropertyChanged(change);
    }

    public override void Render(DrawingContext context)
    {

        var outer_radius = Math.Min(Bounds.Width, Bounds.Height) / 2;
        var center = new Point(Bounds.Width / 2, Bounds.Height / 2);

        var selectedDegreeIndices = IncludedMultiSelectable
            .Where(p => p.IsSelected)
            .Select(p => _noteGraduations.IndexOf(p.Label));

        for (int i = 0; i < 12; i++)
        {
            double angle = i * THIRTY_DEG_RAD - THIRTY_DEG_RAD / 2;
            DrawSegment(i, angle, outer_radius, center, selectedDegreeIndices, context);
        }

        if (GameMode == GameMode.Interactive || GameMode == GameMode.Melody)
        {
            DrawRepeatSymbol(center, outer_radius, context);
        }
    }

    private void DrawRepeatSymbol(Point center, double outerRadius, DrawingContext context)
    {
        double radius = outerRadius * THIRD_INNER_RADIUS_RATIO;
        context.DrawEllipse(Brushes.Transparent, new Pen(Brushes.Transparent), center, radius, radius);
        var repeatIcon = StreamGeometry.Parse(
            "M6 7L7 6L4.70711 3.70711L5.19868 3.21553C5.97697 2.43724 7.03256 2 8.13323 2C11.361 2 14 4.68015 14 7.93274C14 11.2589 11.3013 14 8 14C6.46292 14 4.92913 13.4144 3.75736 12.2426L2.34315 13.6569C3.90505 15.2188 5.95417 16 8 16C12.4307 16 16 12.3385 16 7.93274C16 3.60052 12.4903 0 8.13323 0C6.50213 0 4.93783 0.647954 3.78447 1.80132L3.29289 2.29289L1 0L0 1V7H6Z");
        IBrush brush = _mouseOnRepeatButton
            ? Brushes.Aquamarine
            : Brushes.LightCoral;
        var size = Math.Max(repeatIcon.Bounds.Height, repeatIcon.Bounds.Width);
        double iconSize = radius * 3 / 2;
        double scale = iconSize / size;
        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(new TranslateTransform(-size / 2, -size / 2));
        transformGroup.Children.Add(new ScaleTransform(scale, scale));
        transformGroup.Children.Add(new TranslateTransform(center.X, center.Y));
        repeatIcon.Transform = transformGroup;
        context.DrawGeometry(brush, new Pen(brush), repeatIcon);
    }

    private void DrawSegment(
        int index,
        double startAngle,
        double outerRadius,
        Point center,
        IEnumerable<int> selectedDegreeIndices,
        DrawingContext context)
    {
        double firstInnerRadius = outerRadius * FIRST_INNER_RADIUS_RATIO;
        double secondInnerRadius = outerRadius * SECOND_INNER_RADIUS_RATIO;
        double endAngle = startAngle + THIRTY_DEG_RAD;
        double midAngle = startAngle + THIRTY_DEG_RAD / 2;

        bool isClicked = _userClickedIndex == index;
        if (GameResponse is CircleIndexResponse r)
        {
            isClicked = isClicked || (r.Index == index);
        }
        bool isHovered = _mouseOnSegmentIndex == index;
        bool isGrayedOut = !selectedDegreeIndices.Contains(index);

        DrawSegmentBody(index, startAngle, endAngle, outerRadius, firstInnerRadius, center, isClicked, isHovered, context);
        DrawSegmentFoot(startAngle, endAngle, firstInnerRadius, secondInnerRadius, center, context);
        DrawNoteLabel(index, midAngle, firstInnerRadius, secondInnerRadius, center, context);
        DrawDegreeLabel(index, midAngle, outerRadius, firstInnerRadius, center, isClicked, isGrayedOut, context);
    }

    private void DrawSegmentBody(
        int index,
        double startAngle,
        double endAngle,
        double outerRadius,
        double innerRadius,
        Point center,
        bool isClicked,
        bool isHovered,
        DrawingContext context)
    {
        var outerStart = PointOnCircle(center, startAngle, outerRadius);
        var outerEnd = PointOnCircle(center, endAngle, outerRadius);
        var innerEnd = PointOnCircle(center, endAngle, innerRadius);
        var innerStart = PointOnCircle(center, startAngle, innerRadius);

        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            ctx.BeginFigure(outerStart, isFilled: true);
            ctx.ArcTo(outerEnd, new Size(outerRadius, outerRadius), rotationAngle: 0, isLargeArc: false, SweepDirection.Clockwise);
            ctx.LineTo(innerEnd);
            ctx.ArcTo(innerStart, new Size(innerRadius, innerRadius), rotationAngle: 0, isLargeArc: false, SweepDirection.CounterClockwise);
            ctx.EndFigure(isClosed: true);
        }

        IBrush fill = isClicked
            ? _degreeBrushes[index]
            : new SolidColorBrush(Colors.Transparent, 0.5);

        context.DrawGeometry(fill, new Pen(_accentBrush, 2), geo);

        if (isHovered)
            DrawHoverArc(outerStart, outerEnd, outerRadius, index, context);
    }

    private void DrawHoverArc(Point arcStart, Point arcEnd, double radius, int index, DrawingContext context)
    {
        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            ctx.BeginFigure(arcStart, isFilled: true);
            ctx.ArcTo(arcEnd, new Size(radius, radius), rotationAngle: 0, isLargeArc: false, SweepDirection.Clockwise);
            ctx.EndFigure(isClosed: false);
        }

        context.DrawGeometry(Brushes.Transparent, new Pen(_degreeBrushes[index], 10), geo);
    }

    private void DrawSegmentFoot(
        double startAngle,
        double endAngle,
        double outerRadius,
        double innerRadius,
        Point center,
        DrawingContext context)
    {
        double rotatedStart = startAngle + _animationRotationAngle;
        double rotatedEnd = endAngle + _animationRotationAngle;

        var outerStart = PointOnCircle(center, rotatedStart, outerRadius);
        var outerEnd = PointOnCircle(center, rotatedEnd, outerRadius);
        var innerStart = PointOnCircle(center, rotatedStart, innerRadius);
        var innerEnd = PointOnCircle(center, rotatedEnd, innerRadius);

        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            ctx.BeginFigure(outerEnd, isFilled: true);
            ctx.LineTo(innerEnd);
            ctx.ArcTo(innerStart, new Size(innerRadius, innerRadius), rotationAngle: 0, isLargeArc: false, SweepDirection.CounterClockwise);
            ctx.LineTo(outerStart);
            ctx.EndFigure(isClosed: false);
        }
        context.DrawGeometry(Brushes.Transparent, new Pen(new SolidColorBrush(_accentBrush.Color)), geo);
    }

    private void DrawNoteLabel(
        int index,
        double midAngle,
        double outerRadius,
        double innerRadius,
        Point center,
        DrawingContext context)
    {
        double midRadius = (outerRadius + innerRadius) / 2;
        double fontSize = Math.Max(15, (outerRadius - innerRadius) / 2);
        string note = MusicTheory.NoteAtDegree(_displayTonic, index + 1, correctForFifths: true);
        string noteDisplay = note.Length > 1 ? note[0] + "♭" : note;
        var brush = new SolidColorBrush(Color.Parse("#76D2DB"));

        DrawLabel(noteDisplay, PointOnCircle(center, midAngle + _animationRotationAngle, midRadius), fontSize, brush, context);
    }

    private void DrawDegreeLabel(
        int index,
        double midAngle,
        double outerRadius,
        double innerRadius,
        Point center,
        bool isClicked,
        bool isGrayedOut,
        DrawingContext context)
    {
        double midRadius = (outerRadius + innerRadius) / 2;
        double fontSize = Math.Max(10, (outerRadius - innerRadius) / 2);

        IBrush brush = isClicked
            ? Brushes.White
            : new SolidColorBrush(_degreeBrushes[index].Color, isGrayedOut ? 0.1 : 1.0);

        DrawLabel(_noteGraduations[index], PointOnCircle(center, midAngle, midRadius), fontSize, brush, context);
    }

    private void DrawLabel(string text, Point position, double fontSize, IBrush brush, DrawingContext context)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            NotoSansTypeface,
            fontSize,
            brush);

        var origin = new Point(position.X - formatted.Width / 2, position.Y - formatted.Height / 2);
        context.DrawText(formatted, origin);
    }
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        //do not do hover effect if on touch
        if (e.Pointer.Type != PointerType.Touch)
        {
            HitTest(e.GetCurrentPoint(this));
        }
    }

    protected override void DrawFunc() => InvalidateVisual();

    protected override void OnAnswerStateChanged()
    {
        if (AnswerState == AnswerState.Correct)
        {
            _accentBrush = new SolidColorBrush(Color.Parse("#48A111"));
        }
        else if (AnswerState == AnswerState.Incorrect)
        {
            _accentBrush = new SolidColorBrush(Color.Parse("#C40C0C"));
        }
        else
        {
            _accentBrush = new SolidColorBrush((Color)Application.Current!.Resources["PrimaryColor"]!); //is it costly to keep calling this?
        }
        base.OnAnswerStateChanged();
    }
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        HitTest(e.GetCurrentPoint(this), true);

    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_userClickedIndex is not null)
        {
            var note = MusicTheory.ToMidiNote(Tonic, MusicTheory.NoteAtDegree(Tonic, _userClickedIndex.Value + 1, true), Octave);
            AudioDriver!.Release(note);
        }
        UserResponse = null;
        _userClickedIndex = null;
        InvalidateVisual();
    }
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _mouseOnSegmentIndex = null;
        _mouseOnRepeatButton = false;
        InvalidateVisual();
    }
    private void HitTest(PointerPoint point, bool click = false)
    {
        Point center = new(Bounds.Width / 2, Bounds.Height / 2);
        var outerRadius = Math.Min(Bounds.Width, Bounds.Height) / 2;
        var innerRadius = outerRadius * FIRST_INNER_RADIUS_RATIO;
        var third_inner_radius = outerRadius * THIRD_INNER_RADIUS_RATIO; //radius of circle where repeat button is
        var p = point.Position;
        double dx = p.X - center.X;
        double dy = p.Y - center.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);
        if (dist < third_inner_radius)
        {
            HitTestRepeatButton(click);
        }
        else if (dist > innerRadius && dist < outerRadius)
        {
            HitTestSegment(dx, dy, click);
        }
        else
        {
            if (_mouseOnRepeatButton)
            {
                _mouseOnRepeatButton = false;
                InvalidateVisual();
            }
        }
    }

    private void HitTestRepeatButton(bool click)
    {
        if (click)
        {
            RepeatCommand.Execute(null);
        }
        else
        {
            _mouseOnRepeatButton = true;
        }
        InvalidateVisual();
    }

    private void HitTestSegment(double dx, double dy, bool click)
    {
        double angle = Math.Atan2(dx, -dy);
        if (angle < 0) angle += 2 * Math.PI;
        double offsetAngle = angle + THIRTY_DEG_RAD / 2;
        if (offsetAngle >= 2 * Math.PI) offsetAngle -= 2 * Math.PI;
        int index = (int)(offsetAngle / (Math.PI / 6)) % 12;
        if (click == false)
        {
            if (index == _mouseOnSegmentIndex) return;
            _mouseOnSegmentIndex = index;
        }
        else
        {
            _userClickedIndex = index;
            UserResponse = new CircleIndexResponse(index);
            var note = MusicTheory.ToMidiNote(Tonic, MusicTheory.NoteAtDegree(Tonic, index + 1, true), Octave);
            AudioDriver.Play(note);
        }
        InvalidateVisual();
    }

    private static Point PointOnCircle(Point center, double angle, double distance)
    {
        return new(
          center.X + distance * Math.Sin(angle),
          center.Y - distance * Math.Cos(angle)
        );
    }

}

