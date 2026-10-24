using System;
using Sachssoft.Engine.Controls.Drawing;

namespace Sachssoft.Engine.Controls;

/// <summary>Draws a small page indicator without inheriting a full toggle-button background.</summary>
internal sealed class CarouselIndicatorButton : ToggleButton
{
    /// <summary>Caches circle scanlines in local coordinates until the arranged size changes.</summary>
    private Bounds2[] _scanLines = Array.Empty<Bounds2>();

    /// <summary>Stores the height used for the cached circle geometry.</summary>
    private float _circleHeight = -1;

    /// <summary>Stores the width used for the cached circle geometry.</summary>
    private float _circleWidth = -1;

    /// <inheritdoc/>
    protected override string DefaultStyleName => "Carousel.Indicator";

    /// <inheritdoc/>
    protected override void ArrangeOverride(Bounds2 bounds)
    {
        base.ArrangeOverride(bounds);
        if (_circleWidth == Bounds.Width && _circleHeight == Bounds.Height)
            return;
        _circleWidth = Bounds.Width;
        _circleHeight = Bounds.Height;
        float diameter = MathF.Max(0, MathF.Min(Bounds.Width, Bounds.Height) - 4);
        int count = (int)MathF.Ceiling(diameter);
        _scanLines = new Bounds2[count];
        float radius = diameter / 2;
        float top = (Bounds.Height - diameter) / 2;
        for (int i = 0; i < count; i++)
        {
            float height = MathF.Min(1, diameter - i);
            float y = i + height / 2 - radius;
            float half = MathF.Sqrt(MathF.Max(0, radius * radius - y * y));
            _scanLines[i] = new Bounds2(Bounds.Width / 2 - half, top + i, half * 2, height);
        }
    }

    /// <inheritdoc/>
    protected override void OnDraw(ControlDrawContext context)
    {
        bool available = IsAvailable();
        string state = IsChecked ? !available ? "Checked.Disabled" : IsHovered ? "Checked.Hover" : "Checked.Normal" : !available ? "Unchecked.Disabled" : IsHovered ? "Unchecked.Hover" : "Unchecked.Normal";
        if (context.DrawSkinPart(this, "Indicator", LocalBounds, state))
            return;
        var color = !available ? DisabledTextColor : IsChecked ? PressedBackground : IsHovered ? TextColor : TextColor * .45f;
        foreach (Bounds2 line in _scanLines)
            context.FillRectangle(line, color);
    }
}
