using FontStashSharp.Interfaces;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.CompilerServices;

namespace Sachssoft.Engine.Graphics.Text;

internal sealed class FontStashGlyphRenderer(IFontGlyphSink sink) : IFontStashRenderer2
{
    private static readonly ConditionalWeakTable<IFontGlyphSink, FontStashGlyphRenderer> Renderers = new();

    internal static FontStashGlyphRenderer Get(IFontGlyphSink target) =>
        Renderers.GetValue(target, static value => new FontStashGlyphRenderer(value));

    public GraphicsDevice GraphicsDevice => sink.GraphicsDevice;

    public void DrawQuad(Texture2D texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight,
        ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight) =>
        sink.DrawQuad(texture, ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
}
