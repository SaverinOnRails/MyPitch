using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using MyPitch.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using static MyPitch.ServiceProvider;

namespace MyPitch.Controls;

internal class CircleOfFifths : Control
{
    private readonly String[] _noteGraduations = { "1", "5", "2", "6", "3", "7", "#4", "♭2", "♭6 ", "♭3 ", "♭7 ", "4" };

    private string[] _degreeColors = new string[] { "#00A933", "#79D513", "#FFE400", "#FFBE00", "#FF8000", "#FF3E00", "#FF0000", "#C2003D", "#810081", "#662B99", "#336699", "#198066" };

    private const double FIRST_INNER_RADIUS_RATIO = 0.75;
    private const double SECOND_INNER_RADIUS_RATIO = 0.65;
    private const double THIRD_INNER_RADIUS_RATIO = 0.2;
    private readonly IBrush _accentBrush = new SolidColorBrush(Color.Parse("#FAEB92"));

    public static readonly StyledProperty<Models.Key> TonicProperty = AvaloniaProperty.Register<CircleOfFifths, Models.Key>(nameof(Tonic));

    private Typeface _notoSansTypeface = new Typeface("avares://MyPitch/Assets/Fonts/#Noto Sans");
    public Models.Key Tonic
    {
        get => GetValue(TonicProperty);
        set { SetValue(TonicProperty, value); }
    }
    private int? _mouseOnIndex = null;
    private int? _clickedIndex = null;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == TonicProperty)
        {
            Debug.WriteLine("Tonic changed");
            InvalidateVisual();
        }
        base.OnPropertyChanged(change);
    }

    public override void Render(DrawingContext context)
    {
        var outer_radius = Math.Min(Bounds.Width, Bounds.Height) / 2;
        var first_inner_radius = outer_radius * FIRST_INNER_RADIUS_RATIO;
        var second_inner_radius = outer_radius * SECOND_INNER_RADIUS_RATIO;
        var third_inner_radius = outer_radius * THIRD_INNER_RADIUS_RATIO;
        Point center = new(Bounds.Width / 2, Bounds.Height / 2);

        for (var i = 0; i < 12; i++)
        {
            var angle = ((i * 30 + 0) * Math.PI / 180) - (15 * Math.PI / 180); //angle to the vertical
            DrawSegment(i, angle, outer_radius, first_inner_radius, second_inner_radius, center, context);
        }

        //TONIC BUTTON
        context.DrawEllipse(Brushes.Transparent, new Pen(_accentBrush), center, third_inner_radius, third_inner_radius);
        string tonicString = Tonic.ToString();
        string tonicText = tonicString.Length > 1 ? tonicString[0] + "♭" : tonicString;
        var formattedText = new FormattedText(tonicText.Trim(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _notoSansTypeface, Math.Max(10, third_inner_radius), _accentBrush);
        var textOrigin = new Point(center.X - formattedText.Width / 2, center.Y - formattedText.Height / 2);
        context.DrawText(formattedText, textOrigin);



    }
    private void DrawSegment(int index, double angle, double outer_radius, double first_inner_radius, double second_inner_radius, Point center, DrawingContext context)
    {
        var geo = new StreamGeometry();
        var end_angle = angle + (Math.PI / 6);
        using var ctx = geo.Open();
        //four points on a segment
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
        context.DrawGeometry(_clickedIndex == index ? new SolidColorBrush(Color.Parse(_degreeColors[index])) : Brushes.Transparent, new Pen(_accentBrush, 1), geo);

        //draw segment foot
        var segmentFootGeo = new StreamGeometry();
        var p5 = PointOnCircle(center, end_angle, second_inner_radius);
        var p6 = PointOnCircle(center, angle, second_inner_radius);
        using var ctx3 = segmentFootGeo.Open();
        ctx3.BeginFigure(p3, true);
        ctx3.LineTo(p5);
        ctx3.ArcTo(p6, new Size(second_inner_radius, second_inner_radius), 0, false, SweepDirection.CounterClockwise);
        ctx3.LineTo(p4);
        ctx3.EndFigure(false);
        context.DrawGeometry(Brushes.Transparent, new Pen(_accentBrush), segmentFootGeo);
        double midRadius1 = (first_inner_radius + second_inner_radius) / 2;
        double midAngle1 = angle + (Math.PI / 12);
        var textPos1 = PointOnCircle(center, midAngle1, midRadius1);
        //notes for degree
        var noteAtDeg = MusicTheory.NoteAtDegree(Tonic, index + 1, true);
        var ft1 = new FormattedText(noteAtDeg.Length > 1 ? noteAtDeg[0] + "♭" : noteAtDeg, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _notoSansTypeface, Math.Max(15, (first_inner_radius - second_inner_radius) / 2), _accentBrush);
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
            context.DrawGeometry(Brushes.Transparent, new Pen(new SolidColorBrush(Color.Parse(_degreeColors[index])), 10), arcThicknessGeo);
        }

        double midRadius = (outer_radius + first_inner_radius) / 2;
        double midAngle = angle + (Math.PI / 12);
        var textPos = PointOnCircle(center, midAngle, midRadius);
        var ft = new FormattedText(_noteGraduations[index], CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _notoSansTypeface, Math.Max(10, (outer_radius - first_inner_radius) / 2), new SolidColorBrush(_clickedIndex == index ? Colors.White : Color.Parse(_degreeColors[index])));
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
        if (_clickedIndex is not null)
        {
            var note = MusicTheory.ToMidiNote(Tonic.ToString(), MusicTheory.NoteAtDegree(Tonic, _clickedIndex.Value + 1, true));
            AudioDriver!.Release(note);
        }

        _clickedIndex = null;
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
        double offsetAngle = angle + (15 * Math.PI / 180);
        if (offsetAngle >= 2 * Math.PI) offsetAngle -= 2 * Math.PI;
        int index = (int)(offsetAngle / (Math.PI / 6)) % 12;
        if (click == false)
        {
            if (index == _mouseOnIndex) return;
            _mouseOnIndex = index;
        }
        else
        {
            _clickedIndex = index;
            var note = MusicTheory.ToMidiNote(Tonic.ToString(), MusicTheory.NoteAtDegree(Tonic, index + 1, true));
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

