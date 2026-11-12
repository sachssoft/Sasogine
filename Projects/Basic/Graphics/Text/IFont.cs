using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Engine.Graphics.Text;

/// <summary>Represents a sized font with shared measurement and drawing metrics.</summary>
public interface IFont
{
    /// <summary>Gets the source face when the font can be resized or restyled.</summary>
    FontFace? Face => null;

    /// <summary>Gets the logical size, or zero when the source has no scalable size.</summary>
    float Size => 0;

    /// <summary>Measures unwrapped text in logical units.</summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>The logical size of the text.</returns>
    Size2 Measure(string text);

    /// <summary>Gets whether the font can measure actual glyph ink bounds.</summary>
    bool HasInkBounds => false;

    /// <summary>Measures the visible bounds relative to the text origin.</summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>The visible bounds, or the logical bounds when ink metrics are unavailable.</returns>
    Bounds2 MeasureInkBounds(string text)
    {
        Size2 size = Measure(text);
        return new Bounds2(0, 0, size.Width, size.Height);
    }

    /// <summary>Draws text into an active sprite batch.</summary>
    /// <param name="batch">The active batch.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="position">The text origin.</param>
    /// <param name="color">The text tint.</param>
    void Draw(SpriteBatch batch, string text, Vector2 position, Color color);

    /// <summary>Draws text into an active sprite batch with optional character spacing.</summary>
    /// <param name="batch">The active batch.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="position">The text origin.</param>
    /// <param name="color">The text tint.</param>
    /// <param name="layout">The optional character and line spacing.</param>
    void Draw(SpriteBatch batch, string text, Vector2 position, Color color, CharacterLayoutOptions? layout) =>
        Draw(batch, text, position, color);

    /// <summary>Gets whether the font can append textured glyphs directly to a geometry batch.</summary>
    bool CanDrawGlyphs => false;

    /// <summary>Appends glyphs to an active geometry batch.</summary>
    /// <param name="sink">The active glyph sink.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="transform">The local text transform.</param>
    /// <param name="color">The text tint.</param>
    /// <param name="layout">The optional character and line spacing.</param>
    /// <exception cref="NotSupportedException">The font has no geometry-batch implementation.</exception>
    void DrawGlyphs(IFontGlyphSink sink, string text, TextTransform transform, Color color,
        CharacterLayoutOptions? layout = null) => throw new NotSupportedException("This font does not expose batched glyphs.");
}
