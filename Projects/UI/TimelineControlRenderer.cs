using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Controls;

/// <summary>Draws prepared timeline geometry without changing models, layout, or interaction state.</summary>
internal static class TimelineControlRenderer
{
    /// <summary>Draws the clipped time ruler, visible rows, blocks, selection, and playhead.</summary>
    /// <param name="control">The prepared timeline.</param>
    /// <param name="context">The active ordered control drawing context.</param>
    internal static void Draw(TimelineControl control, ControlDrawContext context)
    {
        var absolute = control.PlotBounds;
        if (absolute.Width <= 0 || absolute.Height <= 0) return;

        var plot = new Bounds2(absolute.X - control.Bounds.X, absolute.Y - control.Bounds.Y, absolute.Width, absolute.Height);
        var content = control.ContentBounds;
        var headers = new Bounds2(content.X - control.Bounds.X, plot.Y, absolute.X - content.X, plot.Height);
        var ruler = new Bounds2(plot.X, content.Y - control.Bounds.Y, plot.Width, absolute.Y - content.Y);
        var font = control.GetFont(context);
        Color text = control.IsEnabled ? control.TextColor : control.DisabledTextColor;
        var rows = control.VisibleTracks;
        var ticks = control.Ticks;
        var items = control.VisibleItems;

        using (context.PushClipScope(plot))
        {
            for (int index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                var body = new Bounds2(plot.X, row.Bounds.Y, plot.Width, row.Bounds.Height);
                if ((row.Index & 1) != 0) context.FillRectangle(body, control.GridColor * 0.12f);
                if (ReferenceEquals(row.Track, control.SelectedTrack)) context.FillRectangle(body, control.HoverBackground * 0.35f);
                context.DrawLine(new Vector2(plot.X, row.Bounds.Bottom), new Vector2(plot.Right, row.Bounds.Bottom), control.GridColor);
            }
            for (int index = 0; index < ticks.Count; index++)
            {
                var tick = ticks[index];
                context.DrawLine(new Vector2(tick.X, plot.Y), new Vector2(tick.X, plot.Bottom), control.GridColor * 0.6f);
            }

            for (int index = 0; index < items.Count; index++)
            {
                var block = items[index];
                bool selected = ReferenceEquals(block.Item, control.SelectedItem) && ReferenceEquals(block.Track, control.SelectedTrack);
                Color color = selected ? control.SelectedItemBackground : block.Item.Color ?? control.ItemBackground;
                if (!control.IsEnabled || block.Item.IsLocked || block.Track.IsLocked) color *= 0.55f;
                context.FillRectangle(block.Bounds, color);
                context.StrokeRectangle(block.Bounds, selected ? control.PlayheadColor : text * 0.35f, selected ? 2 : 1);
                using (context.PushClipScope(block.Bounds))
                    context.DrawText(font, block.Item.Title,
                        new Vector2(block.Bounds.X + 5, block.Bounds.Y + (block.Bounds.Height - control.LineHeight) / 2), text);

                if (selected && control.CanEdit(block.Track, block.Item) && block.Bounds.Width >= control.ResizeHandleWidth * 2 + 4)
                {
                    if (block.HasStart) DrawHandle(context, block.Bounds.X + 3, block.Bounds, text);
                    if (block.HasEnd) DrawHandle(context, block.Bounds.Right - 3, block.Bounds, text);
                }
            }
        }

        if (headers.Width > 0)
            using (context.PushClipScope(headers))
                for (int index = 0; index < rows.Count; index++)
                {
                    var row = rows[index];
                    if (ReferenceEquals(row.Track, control.SelectedTrack)) context.FillRectangle(row.Bounds, control.HoverBackground * 0.35f);
                    using (context.PushClipScope(row.Bounds))
                        context.DrawText(font, row.Track.Title,
                            new Vector2(row.Bounds.X + 6, row.Bounds.Y + (row.Bounds.Height - control.LineHeight) / 2), row.Track.IsLocked ? text * 0.55f : text);
                    context.DrawLine(new Vector2(headers.X, row.Bounds.Bottom), new Vector2(headers.Right, row.Bounds.Bottom), control.GridColor);
                }

        using (context.PushClipScope(ruler))
            for (int index = 0; index < ticks.Count; index++)
            {
                var tick = ticks[index];
                context.DrawText(font, tick.Label, new Vector2(tick.X + 3, ruler.Y + 3), text);
                context.DrawLine(new Vector2(tick.X, ruler.Bottom - 5), new Vector2(tick.X, ruler.Bottom), control.GridColor);
            }

        context.DrawLine(new Vector2(plot.X, ruler.Y), new Vector2(plot.X, plot.Bottom), control.GridColor);
        context.DrawLine(new Vector2(plot.X, plot.Y), new Vector2(plot.Right, plot.Y), control.GridColor);
        double offset = (control.Time - control.ViewStart) * control.PixelsPerSecond;
        if (offset >= 0 && offset <= plot.Width)
        {
            float x = plot.X + (float)offset;
            using (context.PushClipScope(new Bounds2(plot.X, ruler.Y, plot.Width, ruler.Height + plot.Height)))
            {
                context.DrawLine(new Vector2(x, ruler.Y), new Vector2(x, plot.Bottom), control.PlayheadColor, 2);
                context.FillRectangle(new Bounds2(x - 4, ruler.Bottom - 7, 8, 7), control.PlayheadColor);
            }
        }
    }

    /// <summary>Draws the grip of an editable block edge.</summary>
    /// <param name="context">The active drawing context.</param>
    /// <param name="x">The horizontal grip position.</param>
    /// <param name="bounds">The local block rectangle.</param>
    /// <param name="color">The grip color.</param>
    private static void DrawHandle(ControlDrawContext context, float x, Bounds2 bounds, Color color)
    {
        float inset = MathF.Min(6, bounds.Height / 3);
        context.DrawLine(new Vector2(x, bounds.Y + inset), new Vector2(x, bounds.Bottom - inset), color);
    }
}
