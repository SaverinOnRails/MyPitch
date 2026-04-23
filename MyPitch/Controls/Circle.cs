using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using MyPitch.Models;
using MyPitch.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using static MyPitch.PlatformServiceProvider;

namespace MyPitch.Controls;

internal class CircleOfFifths : Control
{
    private readonly String[] _noteGraduations = MusicTheory.FifthIntervalScaleGraduation;
    public CircleOfFifths()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _animationTimer.Tick += OnAnimationTick;
    }
    private static double EaseInOutCubic(double t)
    => t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
    private void OnAnimationTick(object? sender, EventArgs e)
    {
        _animationElapsedMs += 16.0;

        double t = Math.Clamp(_animationElapsedMs / _animationDurationMs, 0.0, 1.0);
        double eased = EaseInOutCubic(t);

        _animationRotationAngle = eased * _animationRotationAngleTarget;

        if (t >= 1.0)
        {
            _animationRotationAngle = _animationRotationAngleTarget;
            _animationRotationAngleTarget = 0;
            _animationElapsedMs = 0; 
            _animationTimer.Stop();
            _displayTonic = Tonic;
            _animationRotationAngle = 0;

        }

        InvalidateVisual();
    }

    private SolidColorBrush[] _degreeBrushes = new SolidColorBrush[]
    {
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

    private const double FIRST_INNER_RADIUS_RATIO = 0.75;
    private const double SECOND_INNER_RADIUS_RATIO = 0.65;
    private const double THIRD_INNER_RADIUS_RATIO = 0.2;
    private const double THIRTY_DEG_RAD = 30 * Math.PI / 180;
    private SolidColorBrush _accentBrush = new SolidColorBrush(Color.Parse("#E4FF30"));

    private DispatcherTimer _animationTimer;
    private double _animationDurationMs; 
    private double _animationRotationAngleTarget;
    private double _animationRotationAngle;
    private double _animationElapsedMs;

    public static readonly StyledProperty<Models.Key> TonicProperty = AvaloniaProperty.Register<CircleOfFifths, Models.Key>(nameof(Tonic));
    public static readonly StyledProperty<IEnumerable<DegreeItem>> IncludedDegreesProperty = AvaloniaProperty.Register<CircleOfFifths, IEnumerable<DegreeItem>>(nameof(IncludedDegrees));
    public static readonly StyledProperty<int> OctaveProperty = AvaloniaProperty.Register<CircleOfFifths, int>(nameof(Octave));

    public static readonly StyledProperty<int?> GameClickedIndexProperty = AvaloniaProperty.Register<CircleOfFifths, int?>(nameof(GameClickedIndex), null);
    public static readonly StyledProperty<int?> UserClickedIndexProperty = AvaloniaProperty.Register<CircleOfFifths, int?>(nameof(UserClickedIndex), null);
    public static readonly StyledProperty<AnswerState> AnswerStateProperty = AvaloniaProperty.Register<CircleOfFifths, AnswerState>(nameof(AnswerState));
    public IEnumerable<DegreeItem> IncludedDegrees
    {
        get => GetValue(IncludedDegreesProperty);
        set
        {
            SetValue(IncludedDegreesProperty, value);
        }
    }
    public AnswerState AnswerState
    {
        get => GetValue(AnswerStateProperty);
        set
        {
            SetValue(AnswerStateProperty, value);
        }
    }
    public int Octave
    {
        get => GetValue(OctaveProperty);
        set => SetValue(OctaveProperty, value);
    }
    public int? GameClickedIndex
    {
        get => GetValue(GameClickedIndexProperty);
        set => SetValue(GameClickedIndexProperty, value);
    }
    public int? UserClickedIndex
    {
        get => GetValue(UserClickedIndexProperty);
        set => SetValue(UserClickedIndexProperty, value);
    }
    private void IncludedDegreesChanged(object? sender, PropertyChangedEventArgs e)
    {
        InvalidateVisual();  //This might cause issues being done to rapidly
    }

    private Typeface _notoSansTypeface = new Typeface("avares://MyPitch/Assets/Fonts/#Noto Sans");

    private Models.Key _displayTonic = Models.Key.C;
    public Models.Key Tonic
    {
        get => GetValue(TonicProperty);
        set { SetValue(TonicProperty, value); }
    }
    private int? _mouseOnIndex = null;
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == TonicProperty || change.Property == GameClickedIndexProperty)
        {
            InvalidateVisual();
        }
        if (change.Property == AnswerStateProperty)
        {
            HandleAnswerStateChange();
        }
        if (change.Property == IncludedDegreesProperty)
        {
            if (change.NewValue is null) return;
            var value = (IEnumerable<DegreeItem>)change.NewValue;
            foreach (var deg in value)
            {
                deg.PropertyChanged += IncludedDegreesChanged;
            }
        }
        if (change.Property == TonicProperty)
        {
            var oldTonic = (Models.Key?)change.OldValue;
            var newTonic = Tonic;
            if (oldTonic is null || oldTonic == newTonic) return;
            int oldSegment = MusicTheory.FifthSegment(oldTonic.Value, newTonic.ToString());
            var diff = 12 - oldSegment; //number of segments between them when moving clockwise
            if(diff > 6)
            {
                diff = diff - 12;
            }
            _animationRotationAngleTarget = diff * THIRTY_DEG_RAD;
            Debug.WriteLine(_animationRotationAngleTarget);
            _animationDurationMs = Math.Clamp(Math.Abs(diff * 300 ), 300 , 1000);
            _animationTimer.Start();
        }
        base.OnPropertyChanged(change);
    }

    private void HandleAnswerStateChange()
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
            _accentBrush = new SolidColorBrush(Color.Parse("#E4FF30"));
        }
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        var includedDegrees = IncludedDegrees.Where(p => p.IsSelected == true).Select(p => _noteGraduations.IndexOf(p.Label));
        var outer_radius = Math.Min(Bounds.Width, Bounds.Height) / 2;
        var first_inner_radius = outer_radius * FIRST_INNER_RADIUS_RATIO;
        var second_inner_radius = outer_radius * SECOND_INNER_RADIUS_RATIO;
        var third_inner_radius = outer_radius * THIRD_INNER_RADIUS_RATIO;
        Point center = new(Bounds.Width / 2, Bounds.Height / 2);

        for (var i = 0; i < 12; i++)
        {
            var angle = (i * THIRTY_DEG_RAD) - (THIRTY_DEG_RAD / 2); //angle to the vertical
            DrawSegment(i, angle, outer_radius, first_inner_radius, second_inner_radius, center, includedDegrees, context);
        }

        //TONIC BUTTON
        context.DrawEllipse(Brushes.Transparent, new Pen(_accentBrush), center, third_inner_radius, third_inner_radius);
        string tonicString = Tonic.ToString();
        string tonicText = tonicString.Length > 1 ? tonicString[0] + "♭" : tonicString;
        var formattedText = new FormattedText(tonicText.Trim(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _notoSansTypeface, Math.Max(10, third_inner_radius), _accentBrush);
        var textOrigin = new Point(center.X - formattedText.Width / 2, center.Y - formattedText.Height / 2);
        context.DrawText(formattedText, textOrigin);

    }
    private void DrawSegment(int index, double angle, double outer_radius, double first_inner_radius, double second_inner_radius, Point center, IEnumerable<int> includedDegrees, DrawingContext context)
    {
        var clicked = UserClickedIndex == index || GameClickedIndex == index;
        var grayOut = !includedDegrees.Contains(index);
        var geo = new StreamGeometry();
        var end_angle = angle + THIRTY_DEG_RAD;
        using var ctx = geo.Open();
        var p1 = PointOnCircle(center, angle, outer_radius);
        var p2 = PointOnCircle(center, end_angle, outer_radius);
        var p3 = PointOnCircle(center, end_angle, first_inner_radius);
        var p4 = PointOnCircle(center, angle, first_inner_radius);
        ctx.BeginFigure(p1, true);
        ctx.ArcTo(p2, new Size(outer_radius, outer_radius), 0, false, SweepDirection.Clockwise);
        ctx.LineTo(p3);
        ctx.ArcTo(p4, new Size(first_inner_radius, first_inner_radius), 0, false, SweepDirection.CounterClockwise);
        ctx.EndFigure(true);
        //draw segment
        IBrush segmentBackground = new SolidColorBrush(Colors.Transparent, 0.5);
        context.DrawGeometry(clicked ? _degreeBrushes[index] : segmentBackground, new Pen(_accentBrush, 1), geo);
        //draw segment foot
        var segmentFootGeo = new StreamGeometry();
        var p5 = PointOnCircle(center, end_angle + _animationRotationAngle, second_inner_radius);
        var p6 = PointOnCircle(center, angle + _animationRotationAngle, second_inner_radius);
        using var ctx3 = segmentFootGeo.Open();
        var p3prime = PointOnCircle(center, end_angle + _animationRotationAngle, first_inner_radius);
        var p4prime = PointOnCircle(center, angle + _animationRotationAngle, first_inner_radius);
        ctx3.BeginFigure(p3prime, true);
        ctx3.LineTo(p5);
        ctx3.ArcTo(p6, new Size(second_inner_radius, second_inner_radius), 0, false, SweepDirection.CounterClockwise);
        ctx3.LineTo(p4prime);
        ctx3.EndFigure(false);
        context.DrawGeometry(Brushes.Transparent, new Pen(new SolidColorBrush(_accentBrush.Color, grayOut ? 0.3 : 1)), segmentFootGeo);
        double midRadius1 = (first_inner_radius + second_inner_radius) / 2;
        double midAngle1 = angle + (THIRTY_DEG_RAD / 2);
        var textPos1 = PointOnCircle(center, midAngle1 + _animationRotationAngle, midRadius1);
        //notes for degree
        var noteAtDeg = MusicTheory.NoteAtDegree(_displayTonic, index + 1, true);
        var ft1 = new FormattedText(noteAtDeg.Length > 1 ? noteAtDeg[0] + "♭" : noteAtDeg, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _notoSansTypeface, Math.Max(15, (first_inner_radius - second_inner_radius) / 2), _degreeBrushes[index]);
        var textOrigin1 = new Point(textPos1.X - ft1.Width / 2, textPos1.Y - ft1.Height / 2);
        context.DrawText(ft1, textOrigin1);

        //For Hover
        if (_mouseOnIndex == index)
        {
            //arc thickness but i do not want to draw the arc seperately. So we just draw another one
            var arcThicknessGeo = new StreamGeometry();
            using var ctx2 = arcThicknessGeo.Open();
            ctx2.BeginFigure(p1, true);
            ctx2.ArcTo(p2, new Size(outer_radius, outer_radius), 0, false, SweepDirection.Clockwise);
            ctx2.EndFigure(false);
            //draw arc
            context.DrawGeometry(Brushes.Transparent, new Pen(_degreeBrushes[index], 10), arcThicknessGeo);
        }

        double midRadius = (outer_radius + first_inner_radius) / 2;
        double midAngle = angle + (THIRTY_DEG_RAD / 2);
        var textPos = PointOnCircle(center, midAngle, midRadius);
        var ft = new FormattedText(_noteGraduations[index], CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _notoSansTypeface, Math.Max(10, (outer_radius - first_inner_radius) / 2), clicked ? Brushes.White : new SolidColorBrush(_degreeBrushes[index].Color, grayOut ? 0.2 : 1));
        var textOrigin = new Point(textPos.X - ft.Width / 2, textPos.Y - ft.Height / 2);
        context.DrawText(ft, textOrigin);
    }
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        //do not do hover effect if on touch
        if (e.Pointer.Type != PointerType.Touch)
        {
            HitTestSegment(e.GetCurrentPoint(this));
        }
    }


    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        HitTestSegment(e.GetCurrentPoint(this), true);

    }
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (UserClickedIndex is not null)
        {
            var note = MusicTheory.ToMidiNote(Tonic.ToString(), MusicTheory.NoteAtDegree(Tonic, UserClickedIndex.Value + 1, true), Octave);
            AudioDriver!.Release(note);
        }

        UserClickedIndex = null;
        InvalidateVisual();
    }
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _mouseOnIndex = null;
        InvalidateVisual();
    }
    private void HitTestSegment(PointerPoint point, bool click = false)
    {
        Point center = new(Bounds.Width / 2, Bounds.Height / 2);
        var outerRadius = Math.Min(Bounds.Width, Bounds.Height) / 2;
        var innerRadius = outerRadius * FIRST_INNER_RADIUS_RATIO;
        var p = point.Position;
        double dx = p.X - center.X;
        double dy = p.Y - center.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);
        if (dist < innerRadius || dist > outerRadius) return;
        double angle = Math.Atan2(dx, -dy);
        if (angle < 0) angle += 2 * Math.PI;
        double offsetAngle = angle + THIRTY_DEG_RAD / 2;
        if (offsetAngle >= 2 * Math.PI) offsetAngle -= 2 * Math.PI;
        int index = (int)(offsetAngle / (Math.PI / 6)) % 12;
        if (click == false)
        {
            if (index == _mouseOnIndex) return;
            _mouseOnIndex = index;
        }
        else
        {
            UserClickedIndex = index;
            var note = MusicTheory.ToMidiNote(Tonic.ToString(), MusicTheory.NoteAtDegree(Tonic, index + 1, true), Octave);
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

public class CircleHaloEffect : Control
{
    public override void Render(DrawingContext context)
    {
        base.Render(context);
        Point center = new(Bounds.Width / 2, Bounds.Height / 2);
        var outer_radius = Math.Min(Bounds.Width, Bounds.Height) / 2 + 1;
        context.DrawEllipse(Brushes.Transparent, new Pen(Brushes.White, 1), center, outer_radius, outer_radius);
    }

    public CircleHaloEffect()
    {
        var blurEffect = new DropShadowEffect
        {
            OffsetX = 0,
            OffsetY = 0,
            BlurRadius = 20,
            Color = Colors.White,
            Opacity = 1
        };
        Effect = blurEffect;
    }
}