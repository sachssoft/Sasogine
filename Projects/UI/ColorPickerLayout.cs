using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Controls;
internal sealed class ColorPickerLayout : ContainerControl
{
    private readonly ColorPickerDialog _owner;
    private readonly ColorPlane _plane;
    private readonly ComboBox _models;
    private readonly TextBox[] _channels;
    private readonly TextBox _hex;
    private readonly Button _ok, _cancel, _pick;
    internal ColorPickerLayout(ColorPickerDialog owner, ColorPlane plane, ComboBox models, TextBox[] channels, TextBox hex, Button ok, Button cancel, Button pick) : base()
    {
        _owner = owner;
        _plane = plane;
        _models = models;
        _channels = channels;
        _hex = hex;
        _ok = ok;
        _cancel = cancel;
        _pick = pick;
        Add(plane);
        Add(models);
        foreach (var channel in channels)
            Add(channel);
        Add(hex);
        Add(ok);
        Add(cancel);
        Add(pick);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        foreach (var child in Children)
            child.Measure(context, available);
        return new Size2(540, 320);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 b)
    {
        float width = MathF.Max(100, b.Width * .53f);
        _plane.Arrange(new Bounds2(b.X, b.Y, width, MathF.Max(60, b.Height - 44)), true);
        float x = b.X + width + 14, w = MathF.Max(0, b.Right - x);
        _models.Arrange(new Bounds2(x, b.Y, w, 32), true);
        for (int i = 0; i < 4; i++)
            _channels[i].Arrange(new Bounds2(x + 42, b.Y + 44 + i * 39, MathF.Max(0, w - 42), 32), true);
        _hex.Arrange(new Bounds2(x, b.Y + 208, w, 32), true);
        _pick.Arrange(new Bounds2(b.X, b.Bottom - 34, 130, 32), true);
        _ok.Arrange(new Bounds2(b.Right - 188, b.Bottom - 34, 90, 32), true);
        _cancel.Arrange(new Bounds2(b.Right - 90, b.Bottom - 34, 90, 32), true);
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        for (int i = 0; i < 4; i++)
            context.DrawText(GetFont(context), _owner.ChannelLabels[i], new Vector2(_channels[i].Bounds.X - Bounds.X - 42, _channels[i].Bounds.Y - Bounds.Y + 5), new Color(150, 168, 191));
    }

    internal override bool IsStyleLocal(string property)
    {
        switch (property)
        {
            default:
                return base.IsStyleLocal(property);
        }
    }

    /// <summary>Removes a local override and restores the current style value.</summary>
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
