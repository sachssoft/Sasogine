using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Graphics.Text;
using System.Runtime.CompilerServices;

namespace Sachssoft.Engine.Controls.Benchmarks;

internal sealed class SharedFontGlyphRenderer(IFontGlyphSink sink) : IFontStashRenderer2
{
    private static readonly ConditionalWeakTable<IFontGlyphSink, SharedFontGlyphRenderer> Renderers = new();

    internal static SharedFontGlyphRenderer Get(IFontGlyphSink sink) =>
        Renderers.GetValue(sink, static value => new SharedFontGlyphRenderer(value));

    public GraphicsDevice GraphicsDevice => sink.GraphicsDevice;

    public void DrawQuad(Texture2D texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight,
        ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight) =>
        sink.DrawQuad(texture, ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
}
