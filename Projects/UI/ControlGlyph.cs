using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Controls;
/// <summary>
/// Small atlas-backed arrow or vector calendar icon, usable as button content.
/// </summary>
internal sealed class ControlGlyph : Control
{
    private readonly ControlGlyphKind _kind;
    internal ControlGlyph(ControlGlyphKind kind) : base()
    {
        _kind = kind;
        Width = Height = 14;
        IsHitTestVisible = false;
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available) => new(MathF.Min(14, available.Width), MathF.Min(14, available.Height));
    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        var b = new Bounds2(0, 0, Bounds.Width, Bounds.Height);
        Vector2 c = new(b.Width / 2, b.Height / 2);
        float r = MathF.Max(1, MathF.Min(b.Width, b.Height) / 2 - 2);
        Color color = IsAvailable() ? ConfigTextColor : ConfigDisabledTextColor;
        if (_kind == ControlGlyphKind.Calendar)
        {
            context.StrokeRectangle(new Bounds2(c.X - r, c.Y - r, r * 2, r * 2), color, 1);
            context.DrawLine(new Vector2(c.X - r, c.Y - r + 3), new Vector2(c.X + r, c.Y - r + 3), color);
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 3; x++)
                    context.FillRectangle(new Bounds2(c.X - r + 2 + x * 3, c.Y - r + 5 + y * 3, 1, 1), color);
            return;
        }

        bool vertical = _kind is ControlGlyphKind.Up or ControlGlyphKind.Down;
        bool end = _kind is ControlGlyphKind.Down or ControlGlyphKind.Right;
        if (context.DrawSkinStylePart(vertical ? "ScrollBar.Vertical" : "ScrollBar.Horizontal", end ? "IncreaseGlyph" : "DecreaseGlyph", b, "Normal", IsAvailable() ? Color.White : ConfigDisabledTextColor))
            return;
        Vector2 tip = _kind switch
        {
            ControlGlyphKind.Up => new(c.X, c.Y - r),
            ControlGlyphKind.Down => new(c.X, c.Y + r),
            ControlGlyphKind.Left => new(c.X - r, c.Y),
            _ => new(c.X + r, c.Y)};
        Vector2 a = vertical ? new(c.X - r, c.Y + (end ? -r : r)) : new(c.X + (end ? -r : r), c.Y - r), d = vertical ? new(c.X + r, a.Y) : new(a.X, c.Y + r);
        context.DrawLine(a, tip, color, 1.5f);
        context.DrawLine(tip, d, color, 1.5f);
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>
    /// Removes a local override and restores the current style value.
    /// </summary>
    public override void ResetStyleProperty(string property)
    {
        switch (property)
        {
            default:
                base.ResetStyleProperty(property);
                return;
        }
    }

    /// <inheritdoc/>
    protected override void ResetStyledValues()
    {
        base.ResetStyledValues();
    }
}
