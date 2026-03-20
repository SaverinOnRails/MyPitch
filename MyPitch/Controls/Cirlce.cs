using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MyPitch.Controls;

internal class CirlceOfFifths : Control
{
    public override void Render(DrawingContext context)
    {
        var outer_radius = Math.Min(Bounds.Width, Bounds.Height) / 2;
        var inner_radius = outer_radius * 0.85;
        Point center = new(Bounds.Width / 2, Bounds.Height / 2);

        for (var i = 0; i < 12; i++)
        {
            var angle = ((i * 30 + 0) * Math.PI / 180); //angle to the
            DrawSegment(i, angle, outer_radius, inner_radius, center, context);
        }

        base.Render(context);
    }

    private void DrawSegment(int index, double angle, double outer_radius, double inner_radius, Point center, DrawingContext context)
    {
        var geo = new StreamGeometry();
        var end_angle = angle + (Math.PI / 6);
        using (var ctx = geo.Open())
        {
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

            context.DrawGeometry(index == 0 ? Brushes.Red : Brushes.Transparent, new Pen(Brushes.Teal, 1), geo);
        }
    }

    private static Point PointOnCircle(Point center, double angle, double distance)
    {
        return new(
            center.X + distance * Math.Sin(angle),
            center.Y - distance * Math.Cos(angle)
        );
    }
}
