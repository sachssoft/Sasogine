using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Controls;

/// <summary>Uses TextBlock's cached layout to draw hyperlink state colors and per-line underlines.</summary>
internal sealed class HyperlinkTextPresenter : TextBlock
{
    /// <summary>The link providing activation and presentation state.</summary>
    private readonly Hyperlink _owner;

    /// <summary>Stores relative underline rectangles without per-frame allocation.</summary>
    private readonly List<Bounds2> _underlines = new();

    /// <summary>Stores the font used for cached underline geometry.</summary>
    private IControlFont? _underlineFont;

    /// <summary>Whether all cached underline measurements completed successfully.</summary>
    private bool _underlinesValid;

    /// <summary>Stores the display text used for cached underline geometry.</summary>
    private string? _underlineText;

    /// <summary>Creates a retained, state-aware text presenter.</summary>
    /// <param name = "owner">The hyperlink whose content is displayed.</param>
    internal HyperlinkTextPresenter(Hyperlink owner)
    {
        _owner = owner;
    }

    /// <summary>Gets the cached per-line underline rectangles relative to the text origin.</summary>
    internal IReadOnlyList<Bounds2> UnderlineSegments => _underlines;

    /// <inheritdoc/>
    protected override Size2 MeasureOverride(ControlLayoutContext context, Size2 available)
    {
        Size2 desired = base.MeasureOverride(context, available);
        IControlFont font = GetFont(context);
        if (!_underlinesValid || !ReferenceEquals(_underlineFont, font) || _underlineText != DisplayText)
        {
            _underlinesValid = false;
            _underlines.Clear();
            if (DisplayText.Length > 0)
            {
                float firstHeight = font.Measure("M").Height;
                float advance = MathF.Max(firstHeight, font.Measure("M\nM").Height - firstHeight);
                string[] lines = DisplayText.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    Size2 size = font.Measure(lines[i]);
                    ValidateSize(size, false);
                    if (size.Width > 0)
                        _underlines.Add(new Bounds2(0, MathF.Max(0, i * advance + firstHeight - 1), size.Width, 1));
                }
            }

            _underlineFont = font;
            _underlineText = DisplayText;
            _underlinesValid = true;
        }

        return desired;
    }

    /// <inheritdoc/>
    protected override void DrawText(ControlDrawContext context)
    {
        Color color = !IsAvailable() ? _owner.DisabledTextColor : IsStyleLocal(nameof(TextColor)) ? TextColor : _owner.EffectiveTextColor;
        DrawText(context, color);
        if (!_owner.Underline)
            return;
        Vector2 origin = TextPosition;
        foreach (Bounds2 line in _underlines)
            context.FillRectangle(new Bounds2(origin.X + line.X, origin.Y + line.Y, line.Width, line.Height), color);
    }

    /// <inheritdoc/>
    protected override void OnFontChanged()
    {
        _underlinesValid = false;
        base.OnFontChanged();
    }

    /// <inheritdoc/>
    public override void InvalidateLayout()
    {
        _underlinesValid = false;
        base.InvalidateLayout();
    }
}
