using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Graphics.Text;
using System;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>
/// Adapts an existing Sasogine FontFace using FontStashSharp for matching text metrics.
/// </summary>
public sealed class FontFaceControlFont : IControlBatchFont, IDisposable
{
    /// <summary>Limits caching to short captions and wrapped label fragments.</summary>
    private const int MaximumCachedLength = 128;

    /// <summary>Bounds the number of retained strings for each immutable face and size.</summary>
    private const int MeasurementCacheSize = 256;

    private readonly FontSystem _system;
    private readonly SpriteFontBase _font;
    private readonly bool _ownsSystem;
    private bool _disposed;

    /// <summary>Stores recent text measurements, replacing entries when their slots collide.</summary>
    private (string? Text, Size2 Size)[]? _measurements;
    /// <summary>
    /// Creates an independent font cache. The caller retains ownership of the FontFace.
    /// </summary>
    public FontFaceControlFont(FontFace face, int size)
    {
        ArgumentNullException.ThrowIfNull(face);
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size));
        Face = face;
        Size = size;
        _system = new FontSystem();
        _ownsSystem = true;
        try
        {
            _system.AddFont(face.Data.ToArray());
            _font = _system.GetFont(size);
        }
        catch
        {
            _system.Dispose();
            throw;
        }
    }

    internal FontFaceControlFont(FontFace face, int size, FontSystem system)
    {
        Face = face;
        Size = size;
        _system = system;
        _font = system.GetFont(size);
    }

    /// <summary>
    /// Gets the source face.
    /// </summary>
    public FontFace Face { get; }
    /// <summary>
    /// Gets the logical font size.
    /// </summary>
    public int Size { get; }

    /// <inheritdoc/>
    public Size2 Measure(string text)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (text is null || text.Length == 0 || text.Length > MaximumCachedLength)
            return new Size2(_font.MeasureString(text!));

        var measurements = _measurements ??= new (string?, Size2)[MeasurementCacheSize];
        int index = text.GetHashCode(StringComparison.Ordinal) & (MeasurementCacheSize - 1);
        ref var entry = ref measurements[index];
        if (string.Equals(entry.Text, text, StringComparison.Ordinal))
            return entry.Size;

        Size2 measured = new(_font.MeasureString(text));
        entry = (text, measured);
        return measured;
    }

    /// <summary>
    /// Measures the visible glyph bounds relative to the text drawing origin.
    /// </summary>
    /// <param name="text">The text whose rendered glyph bounds are requested.</param>
    /// <returns>The visible bounds, including their offset from the drawing origin.</returns>
    /// <exception cref="ObjectDisposedException">This adapter has been disposed.</exception>
    /// <exception cref="ArgumentNullException">The text is null.</exception>
    public Bounds2 MeasureInkBounds(string text)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0)
            return Bounds2.Zero;

        var bounds = _font.TextBounds(text, Vector2.Zero);
        return new Bounds2(bounds.X, bounds.Y, bounds.X2 - bounds.X, bounds.Y2 - bounds.Y);
    }

    /// <inheritdoc/>
    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _font.DrawText(batch, text, position, color);
    }

    /// <inheritdoc/>
    public void Draw(ControlFrameBatch batch, string text, Vector2 position, Color color)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        batch.DrawText(_font, text, position, color);
    }

    /// <summary>
    /// Releases this adapter's font cache. Does not dispose the source face.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        if (_ownsSystem)
            _system.Dispose();
        _measurements = null;
        _disposed = true;
    }
}
