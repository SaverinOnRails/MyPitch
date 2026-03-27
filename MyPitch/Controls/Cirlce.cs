using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using MyPitch.Models.MusicTheory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using static MyPitch.ServiceProvider;

namespace MyPitch.Controls;

internal class CircleOfFifths : Control
{
    private readonly String[] _noteGraduations = { "1", "5", "2", "6", "3", "7", "#4", "♭2", "♭6 ", "♭3 ", "♭7 ", "4" };


    private const double INNER_RADIUS_RATIO = 0.75;

    private Models.MusicTheory.Key _tonic = Models.MusicTheory.Key.C;

    private int? _mouseOnIndex = null;
    private int? _clickedIndex = null;
    public override void Render(DrawingContext context)
    {
        var outer_radius = Math.Min(Bounds.Width, Bounds.Height) / 2;
        var inner_radius = outer_radius * INNER_RADIUS_RATIO;
        Point center = new(Bounds.Width / 2, Bounds.Height / 2);

        for (var i = 0; i < 12; i++)
        {
            var angle = ((i * 30 + 0) * Math.PI / 180) - (15 * Math.PI / 180); //angle to the vertical
            DrawSegment(i, angle, outer_radius, inner_radius, center, context);
        }

        base.Render(context);
    }
    private void DrawSegment(int index, double angle, double outer_radius, double inner_radius, Point center, DrawingContext context)
    {
        var geo = new StreamGeometry();
        var end_angle = angle + (Math.PI / 6);
        using var ctx = geo.Open();
        //four points on a segment
        var p1 = PointOnCircle(center, angle, outer_radius);
        var p2 = PointOnCircle(center, end_angle, outer_radius);
        var p3 = PointOnCircle(center, end_angle, inner_radius);
        var p4 = PointOnCircle(center, angle, inner_radius);

        ctx.BeginFigure(p1, true);
        ctx.ArcTo(p2, new Size(outer_radius, outer_radius), 0, false, SweepDirection.Clockwise);
        ctx.LineTo(p3);
        ctx.ArcTo(p4, new Size(inner_radius, inner_radius), 0, false, SweepDirection.CounterClockwise);
        ctx.EndFigure(true);
        //draw segment
        context.DrawGeometry(_clickedIndex == index ? Brushes.Teal : Brushes.Transparent, new Pen(Brushes.Teal, 1), geo);
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
            context.DrawGeometry(Brushes.Transparent, new Pen(Brushes.Teal, 10), arcThicknessGeo);
        }
        double midRadius = (outer_radius + inner_radius) / 2;
        double midAngle = angle + (Math.PI / 12);
        var textPos = PointOnCircle(center, midAngle, midRadius);
        var ft = new FormattedText(_noteGraduations[index], CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("avares://MyPitch/Assets/Fonts/#Noto Sans"), Math.Max(10, (outer_radius - inner_radius) / 2), new SolidColorBrush(_clickedIndex == index ? Colors.White : Colors.Teal));
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
        AudioDriver!.Release();
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
        var innerRadius = outerRadius * INNER_RADIUS_RATIO;
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
            var note = MusicTheory.ToMidiNote(MusicTheory.NoteAtDegree(_tonic, index + 1, true));
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

