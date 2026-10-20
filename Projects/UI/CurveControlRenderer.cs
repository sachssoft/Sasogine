using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Sachssoft.Engine.Controls;
/// <summary>Font-independent graph geometry and clipped polylines using the ordered control draw context.</summary>
internal static class CurveControlRenderer
{
    internal static void Draw(CurveControl control, ControlDrawContext context, IReadOnlyList<CurveSample> samples)
    {
        var plot = control.PlotBounds;
        if (plot.Width <= 0 || plot.Height <= 0)
            return;
        var local = new Bounds2(plot.X - control.Bounds.X, plot.Y - control.Bounds.Y, plot.Width, plot.Height);
        var d = control;
        var minimum = d.ConfigCurveViewMinimum;
        var maximum = d.ConfigCurveViewMaximum;
        var font = d.ConfigFont ?? context.DefaultFont;
        double xStep = NiceStep(((double)maximum.X - minimum.X) / 6), yStep = NiceStep(((double)maximum.Y - minimum.Y) / 5);
        using (context.PushClipScope(local))
        {
            if (d.ConfigShowGrid)
            {
                for (double x = Math.Ceiling(minimum.X / xStep) * xStep, count = 0; x <= maximum.X && count < 32; x += xStep, count++)
                {
                    float screen = (float)(local.X + (x - minimum.X) / (maximum.X - minimum.X) * local.Width);
                    context.DrawLine(new(screen, local.Y), new(screen, local.Bottom), Math.Abs(x) < xStep * .0001 ? d.ConfigTextColor * .5f : d.ConfigGridColor);
                }

                for (double y = Math.Ceiling(minimum.Y / yStep) * yStep, count = 0; y <= maximum.Y && count < 32; y += yStep, count++)
                {
                    float screen = (float)(local.Bottom - (y - minimum.Y) / (maximum.Y - minimum.Y) * local.Height);
                    context.DrawLine(new(local.X, screen), new(local.Right, screen), Math.Abs(y) < yStep * .0001 ? d.ConfigTextColor * .5f : d.ConfigGridColor);
                }
            }

            for (int i = 1; i < samples.Count; i++)
            {
                var a = samples[i - 1];
                var b = samples[i];
                if (!a.IsValid || !b.IsValid)
                    continue;
                double ax = (a.X - minimum.X) / (maximum.X - minimum.X) * local.Width, ay = local.Height - (a.Y - minimum.Y) / (maximum.Y - minimum.Y) * local.Height;
                double bx = (b.X - minimum.X) / (maximum.X - minimum.X) * local.Width, by = local.Height - (b.Y - minimum.Y) / (maximum.Y - minimum.Y) * local.Height;
                if (ClipLine(ref ax, ref ay, ref bx, ref by, local.Width, local.Height))
                    context.DrawLine(new Vector2(local.X + (float)ax, local.Y + (float)ay), new Vector2(local.X + (float)bx, local.Y + (float)by), control.IsEnabled ? d.ConfigCurveColor : d.ConfigDisabledTextColor, d.ConfigCurveLineThickness);
            }
        }

        context.StrokeRectangle(local, d.ConfigBorderColor, 1);
        for (double x = Math.Ceiling(minimum.X / xStep) * xStep, count = 0; x <= maximum.X && count < 32; x += xStep, count++)
        {
            string text = Format(x, xStep);
            var size = font.Measure(text);
            float screen = (float)(local.X + (x - minimum.X) / (maximum.X - minimum.X) * local.Width);
            context.DrawText(font, text, new Vector2(Math.Clamp(screen - size.Width / 2, 0, MathF.Max(0, control.Bounds.Width - size.Width)), local.Bottom + 4), d.ConfigTextColor);
        }

        for (double y = Math.Ceiling(minimum.Y / yStep) * yStep, count = 0; y <= maximum.Y && count < 32; y += yStep, count++)
        {
            string text = Format(y, yStep);
            var size = font.Measure(text);
            float screen = (float)(local.Bottom - (y - minimum.Y) / (maximum.Y - minimum.Y) * local.Height);
            context.DrawText(font, text, new Vector2(MathF.Max(0, local.X - size.Width - 8), screen - size.Height / 2), d.ConfigTextColor);
        }

        if (d.ConfigShowPoints && control.EditableCurve is EditableCurve model)
        {
            for (int i = 0; i < model.Points.Count; i++)
            {
                var point = model.Points[i];
                if (point.X < minimum.X || point.X > maximum.X || point.Y < minimum.Y || point.Y > maximum.Y)
                    continue;
                Vector2 center = control.ToScreen(point) - new Vector2(control.Bounds.X, control.Bounds.Y);
                float radius = d.ConfigPointRadius;
                Color color = control.IsEnabled ? (i == control.SelectedIndex ? d.ConfigCurveColor : d.ConfigPointColor) : d.ConfigDisabledTextColor;
                var box = new Bounds2(center.X - radius, center.Y - radius, radius * 2, radius * 2);
                context.FillRectangle(box, color);
                context.StrokeRectangle(box, d.ConfigBackground, 1);
                if (i == control.SelectedIndex)
                    context.StrokeRectangle(new Bounds2(box.X - 2, box.Y - 2, box.Width + 4, box.Height + 4), d.ConfigFocusColor, 1);
            }
        }

        if (control.HasInvalidSamples)
            context.DrawText(font, "Invalid curve samples", new Vector2(local.X + 8, local.Y + 6), new Color(230, 120, 110));
    }

    private static string Format(double value, double step) => Math.Abs(value) < step * .0001 ? "0" : value.ToString("G3", CultureInfo.InvariantCulture);
    private static double NiceStep(double step)
    {
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(step))), fraction = step / magnitude;
        return magnitude * (fraction <= 1 ? 1 : fraction <= 2 ? 2 : fraction <= 5 ? 5 : 10);
    }

    private static bool Clip(double p, double q, ref double low, ref double high)
    {
        if (p == 0)
            return q >= 0;
        double t = q / p;
        if (p < 0)
        {
            if (t > high)
                return false;
            if (t > low)
                low = t;
        }
        else
        {
            if (t < low)
                return false;
            if (t < high)
                high = t;
        }

        return true;
    }

    private static bool ClipLine(ref double ax, ref double ay, ref double bx, ref double by, double width, double height)
    {
        double x = ax, y = ay, dx = bx - ax, dy = by - ay, low = 0, high = 1;
        if (!Clip(-dx, x, ref low, ref high) || !Clip(dx, width - x, ref low, ref high) || !Clip(-dy, y, ref low, ref high) || !Clip(dy, height - y, ref low, ref high))
            return false;
        ax = x + low * dx;
        ay = y + low * dy;
        bx = x + high * dx;
        by = y + high * dy;
        return true;
    }
}
