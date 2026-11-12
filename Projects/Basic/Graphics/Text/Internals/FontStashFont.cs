using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Engine.Graphics.Text;

internal sealed class FontStashFont(FontFace face, float size) : IFont
{
    private const int CacheSize = 256;
    private const int MaximumCachedLength = 128;
    private (string? Text, Size2 Size)[]? _measurements;
    private FontSystem? _measurementSystem;

    public FontFace Face => face;

    public float Size => size;

    public bool CanDrawGlyphs => true;
    public bool HasInkBounds => true;

    public Size2 Measure(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var system = _measurementSystem ??= FontStashSharpBackend.GetMeasurementSystem(face);
        lock (system)
        {
            if (text.Length == 0 || text.Length > MaximumCachedLength)
                return new Size2(system.GetFont(size).MeasureString(text));
            var measurements = _measurements ??= new (string?, Size2)[CacheSize];
            ref var entry = ref measurements[text.GetHashCode(StringComparison.Ordinal) & (CacheSize - 1)];
            if (string.Equals(entry.Text, text, StringComparison.Ordinal))
                return entry.Size;
            Size2 measured = new(system.GetFont(size).MeasureString(text));
            entry = (text, measured);
            return measured;
        }
    }

    public Bounds2 MeasureInkBounds(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.Length == 0)
            return Bounds2.Zero;
        var system = _measurementSystem ??= FontStashSharpBackend.GetMeasurementSystem(face);
        lock (system)
        {
            var bounds = system.GetFont(size).TextBounds(text, Vector2.Zero);
            return new Bounds2(bounds.X, bounds.Y, bounds.X2 - bounds.X, bounds.Y2 - bounds.Y);
        }
    }

    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color) =>
        Draw(batch, text, position, color, null);

    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color, CharacterLayoutOptions? layout)
    {
        ArgumentNullException.ThrowIfNull(batch);
        FontStashSharpBackend.GetRenderFont(batch.GraphicsDevice, face, size).DrawText(batch, text, position, color,
            characterSpacing: layout?.CharacterSpacing ?? 0f, lineSpacing: layout?.LineSpacing ?? 0f);
    }

    public void DrawGlyphs(IFontGlyphSink sink, string text, TextTransform transform, Color color,
        CharacterLayoutOptions? layout = null)
    {
        ArgumentNullException.ThrowIfNull(sink);
        FontStashSharpBackend.GetRenderFont(sink.GraphicsDevice, face, size).DrawText(FontStashGlyphRenderer.Get(sink),
            text, new Vector2(transform.Position.X, transform.Position.Y), color, rotation: transform.Rotation,
            origin: new Vector2(transform.Pivot.X, transform.Pivot.Y), scale: transform.Scale,
            characterSpacing: layout?.CharacterSpacing ?? 0f, lineSpacing: layout?.LineSpacing ?? 0f);
    }
}
