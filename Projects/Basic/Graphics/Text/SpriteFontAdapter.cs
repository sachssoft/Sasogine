using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Graphics.Text;

/// <summary>Uses an existing MonoGame bitmap font as a shared text font.</summary>
public sealed class SpriteFontAdapter : IFont
{
    private readonly Dictionary<char, SpriteFont.Glyph> _glyphs;

    /// <summary>Wraps a caller-owned bitmap font.</summary>
    /// <param name="font">The font whose texture and metrics are borrowed.</param>
    public SpriteFontAdapter(SpriteFont font)
    {
        Font = font ?? throw new ArgumentNullException(nameof(font));
        _glyphs = font.GetGlyphs();
    }

    /// <summary>Gets the caller-owned bitmap font.</summary>
    public SpriteFont Font { get; }

    /// <inheritdoc/>
    public bool CanDrawGlyphs => true;

    /// <inheritdoc/>
    public Size2 Measure(string text) => new(Font.MeasureString(text));

    /// <inheritdoc/>
    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color) =>
        batch.DrawString(Font, text, position, color);

    /// <inheritdoc/>
    public void DrawGlyphs(IFontGlyphSink sink, string text, TextTransform transform, Color color,
        CharacterLayoutOptions? layout = null)
    {
        ArgumentNullException.ThrowIfNull(sink);
        ArgumentNullException.ThrowIfNull(text);
        Texture2D texture = Font.Texture;
        float uScale = 1f / texture.Width, vScale = 1f / texture.Height;
        float sine = MathF.Sin(transform.Rotation), cosine = MathF.Cos(transform.Rotation);
        float x = 0, y = 0;
        bool first = true;
        foreach (char character in text)
        {
            if (character == '\r')
                continue;
            if (character == '\n')
            {
                x = 0;
                y += Font.LineSpacing + (layout?.LineSpacing ?? 0);
                first = true;
                continue;
            }

            if (!_glyphs.TryGetValue(character, out var glyph) &&
                !(Font.DefaultCharacter is char fallback && _glyphs.TryGetValue(fallback, out glyph)))
                throw new ArgumentException("The SpriteFont does not contain a requested character.", nameof(text));
            if (first)
            {
                x = MathF.Max(glyph.LeftSideBearing, 0);
                first = false;
            }
            else
            {
                x += Font.Spacing + glyph.LeftSideBearing + (layout?.CharacterSpacing ?? 0);
            }

            Rectangle bounds = glyph.BoundsInTexture;
            if (bounds.Width > 0 && bounds.Height > 0)
            {
                float left = x + glyph.Cropping.X, top = y + glyph.Cropping.Y;
                var a = Vertex(left, top, bounds.Left, bounds.Top);
                var b = Vertex(left + bounds.Width, top, bounds.Right, bounds.Top);
                var c = Vertex(left, top + bounds.Height, bounds.Left, bounds.Bottom);
                var d = Vertex(left + bounds.Width, top + bounds.Height, bounds.Right, bounds.Bottom);
                sink.DrawQuad(texture, ref a, ref b, ref c, ref d);
            }
            x += glyph.Width + glyph.RightSideBearing;
        }

        VertexPositionColorTexture Vertex(float localX, float localY, int u, int v)
        {
            float px = (localX - transform.Pivot.X) * transform.Scale.X;
            float py = (localY - transform.Pivot.Y) * transform.Scale.Y;
            return new(new Vector3(transform.Position.X + px * cosine - py * sine,
                transform.Position.Y + px * sine + py * cosine, 0), color, new Vector2(u * uScale, v * vScale));
        }
    }
}
