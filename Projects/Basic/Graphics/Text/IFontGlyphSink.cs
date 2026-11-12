using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Engine.Graphics.Text;

/// <summary>Receives textured glyph quads in local rendering coordinates.</summary>
public interface IFontGlyphSink
{
    /// <summary>Gets the device that owns glyph textures.</summary>
    GraphicsDevice GraphicsDevice { get; }

    /// <summary>Appends a textured glyph quad.</summary>
    /// <param name="texture">The texture containing the glyph.</param>
    /// <param name="topLeft">The local top-left vertex.</param>
    /// <param name="topRight">The local top-right vertex.</param>
    /// <param name="bottomLeft">The local bottom-left vertex.</param>
    /// <param name="bottomRight">The local bottom-right vertex.</param>
    void DrawQuad(Texture2D texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight,
        ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight);
}
