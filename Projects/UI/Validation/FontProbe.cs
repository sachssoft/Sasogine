using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Controls;
using Sachssoft.Engine.Graphics.Text;

/// <summary>A disposable font adapter for checking font identity and layout without graphics.</summary>
internal sealed class FontProbe : IControlFont, IDisposable
{
    internal FontProbe(FontFace face, int size)
    {
        Face = face;
        Size = size;
    }

    internal FontFace Face { get; }
    internal int Size { get; }
    internal int Disposals { get; private set; }

    public Size2 Measure(string text)
    {
        ObjectDisposedException.ThrowIf(Disposals > 0, this);
        return new(text.Length * Size / 2f, Size);
    }

    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color) => throw new NotSupportedException();
    public void Dispose() => Disposals++;
}
