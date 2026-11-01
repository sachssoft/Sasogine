using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Engine.Controls;

/// <summary>Arranges a model-aware color plane beside channel radios, numeric inputs, and dialog actions.</summary>
internal sealed class ColorPickerLayout : ContainerControl
{
    /// <summary>Stores the cancel action.</summary>
    private readonly Button _cancel;
    /// <summary>Stores the accept action.</summary>
    private readonly Button _ok;
    /// <summary>Stores the pipette action.</summary>
    private readonly Button _pick;

    /// <summary>Provides model-specific channel captions.</summary>
    private readonly ColorPickerDialog _owner;

    /// <summary>Stores the interactive color plane.</summary>
    private readonly ColorPlane _plane;

    /// <summary>Stores the model selector.</summary>
    private readonly ComboBox _models;

    /// <summary>Stores the three color-channel selectors, excluding alpha.</summary>
    private readonly RadioButton[] _selectors;

    /// <summary>Stores the hexadecimal editor.</summary>
    private readonly TextBox _hex;

    /// <summary>Stores the four model-specific channel inputs.</summary>
    private readonly TextBox[] _channels;

    /// <summary>Creates the layout and takes ownership of its supplied child controls.</summary>
    /// <param name="owner">The dialog supplying captions and model state.</param>
    /// <param name="plane">The interactive plane.</param>
    /// <param name="models">The model selector.</param>
    /// <param name="selectors">The three scoped channel selectors.</param>
    /// <param name="channels">The four numeric channel editors.</param>
    /// <param name="hex">The hexadecimal editor.</param>
    /// <param name="ok">The accept button.</param>
    /// <param name="cancel">The cancel button.</param>
    /// <param name="pick">The pipette button.</param>
    internal ColorPickerLayout(ColorPickerDialog owner, ColorPlane plane, ComboBox models, RadioButton[] selectors,
        TextBox[] channels, TextBox hex, Button ok, Button cancel, Button pick)
    {
        _owner = owner;
        _plane = plane;
        _models = models;
        _selectors = selectors;
        _channels = channels;
        _hex = hex;
        _ok = ok;
        _cancel = cancel;
        _pick = pick;
        Add(plane);
        Add(models);
        foreach (var selector in selectors) Add(selector);
        foreach (var channel in channels) Add(channel);
        Add(hex);
        Add(ok);
        Add(cancel);
        Add(pick);
    }

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        foreach (var child in Children) child.Measure(context, available);
        return new Size2(540, 320);
    }

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        float width = MathF.Max(100, bounds.Width * 0.53f);
        _plane.Arrange(new Bounds2(bounds.X, bounds.Y, width, MathF.Max(60, bounds.Height - 44)), true);
        float x = bounds.X + width + 14;
        float inputWidth = MathF.Max(0, bounds.Right - x);
        _models.Arrange(new Bounds2(x, bounds.Y, inputWidth, 32), true);
        for (int i = 0; i < _channels.Length; i++)
        {
            float y = bounds.Y + 44 + i * 39;
            if (i < _selectors.Length) _selectors[i].Arrange(new Bounds2(x, y, 70, 32), true);
            _channels[i].Arrange(new Bounds2(x + 70, y, MathF.Max(0, inputWidth - 70), 32), true);
        }
        _hex.Arrange(new Bounds2(x, bounds.Y + 208, inputWidth, 32), true);
        _pick.Arrange(new Bounds2(bounds.X, bounds.Bottom - 34, 130, 32), true);
        _ok.Arrange(new Bounds2(bounds.Right - 188, bounds.Bottom - 34, 90, 32), true);
        _cancel.Arrange(new Bounds2(bounds.Right - 90, bounds.Bottom - 34, 90, 32), true);
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        context.DrawText(GetFont(context), _owner.ChannelLabels[3],
            new Vector2(_channels[3].Bounds.X - Bounds.X - 42, _channels[3].Bounds.Y - Bounds.Y + 5), TextColor);
    }
}