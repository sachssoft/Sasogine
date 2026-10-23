using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Adapts a caller-owned MonoGame SpriteFont to UI text measurement and drawing.
/// </summary>
public sealed class SpriteFontControlFont : IControlBatchFont
{
    private readonly Dictionary<char, SpriteFont.Glyph> _glyphs;
    /// <summary>
    /// Initializes the adapter.
    /// </summary>
    public SpriteFontControlFont(SpriteFont font)
    {
        Font = font ?? throw new ArgumentNullException(nameof(font));
        _glyphs = font.GetGlyphs();
    }

    /// <summary>
    /// Gets the caller-owned font.
    /// </summary>
    public SpriteFont Font { get; }

    /// <inheritdoc/>
    public Size2 Measure(string text) => new(Font.MeasureString(text));
    /// <inheritdoc/>
    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color) => batch.DrawString(Font, text, position, color);
    /// <inheritdoc/>
    public void Draw(ControlFrameBatch batch, string text, Vector2 position, Color color)
    {
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentNullException.ThrowIfNull(text);
        float x = 0, y = 0;
        bool first = true;
        foreach (char character in text)
        {
            if (character == '\r')
                continue;
            if (character == '\n')
            {
                x = 0;
                y += Font.LineSpacing;
                first = true;
                continue;
            }

            if (!_glyphs.TryGetValue(character, out var glyph) && !(Font.DefaultCharacter is char fallback && _glyphs.TryGetValue(fallback, out glyph)))
                throw new ArgumentException("The SpriteFont does not contain a requested character.", nameof(text));
            if (first)
            {
                x = MathF.Max(glyph.LeftSideBearing, 0);
                first = false;
            }
            else
                x += Font.Spacing + glyph.LeftSideBearing;
            batch.Draw(Font.Texture, position + new Vector2(x + glyph.Cropping.X, y + glyph.Cropping.Y), glyph.BoundsInTexture, color, 0, Vector2.Zero, Vector2.One);
            x += glyph.Width + glyph.RightSideBearing;
        }
    }
}
