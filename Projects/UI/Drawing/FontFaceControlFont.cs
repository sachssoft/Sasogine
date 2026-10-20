using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Graphics.Text;
using System;

namespace Sachssoft.Engine.Controls.Drawing;
/// <summary>Adapts an existing Sasogine FontFace using FontStashSharp for matching text metrics.</summary>
public sealed class FontFaceControlFont : IControlBatchFont, IDisposable
{
    private readonly FontSystem _system;
    private readonly SpriteFontBase _font;
    private bool _disposed;
    /// <summary>Creates an independent font cache. The caller retains ownership of the FontFace.</summary>
    public FontFaceControlFont(FontFace face, int size)
    {
        ArgumentNullException.ThrowIfNull(face);
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size));
        Face = face;
        Size = size;
        _system = new FontSystem();
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

    /// <summary>Gets the source face.</summary>
    public FontFace Face { get; }
    /// <summary>Gets the logical font size.</summary>
    public int Size { get; }

    /// <inheritdoc/>
    public Size2 Measure(string text)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return new Size2(_font.MeasureString(text));
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

    /// <summary>Releases this adapter's font cache. Does not dispose the source face.</summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        _system.Dispose();
        _disposed = true;
    }
}
