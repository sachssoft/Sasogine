using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine.Graphics.Text;

namespace Sachssoft.Engine.Graphics.Rendering.Batches;

internal sealed class FontBatchRenderer(GraphicsDevice device, MultiFrameBatch batch) : IFontGlyphSink
{
    public GraphicsDevice GraphicsDevice => device;

    public void DrawQuad(Texture2D texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight,
        ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight) =>
        batch.AddGlyphQuad(texture, topLeft, topRight, bottomLeft, bottomRight);
}
