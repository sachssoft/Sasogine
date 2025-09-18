using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine;
using Sachssoft.Sasogine.Graphics.Primitives;

public sealed class QuadPrimitive : PrimitiveBase
{
    public Vector2 TopLeft { get; set; }
    public Vector2 BottomRight { get; set; }

    public QuadPrimitive(Vector2 position, Vector2 size, Color color)
    {
        TopLeft = position;
        BottomRight = position + size;
        Color = color;
    }

    public override int VertexCount => 4;   // 4 Vertices
    public override int IndexCount => 6;    // 6 Indices für 2 Dreiecke

    public override void Fill(
        VertexPositionColorNormalTexture[] vertices,
        int vertexOffset,
        short[] indices,
        int indexOffset,
        short baseVertex,
        Texture2D? texture = null)
    {
        // Texture-UVs berechnen
        var (u0, v0, u1, v1) = GetUV(texture);

        Vector2 topRight = new Vector2(BottomRight.X, TopLeft.Y);
        Vector2 bottomLeft = new Vector2(TopLeft.X, BottomRight.Y);

        Vector3 normal = new Vector3(0, 0, -1f);

        // Vertices
        vertices[vertexOffset + 0] = new VertexPositionColorNormalTexture(new Vector3(TopLeft, 0), Color, normal, new Vector2(u0, v0));
        vertices[vertexOffset + 1] = new VertexPositionColorNormalTexture(new Vector3(topRight, 0), Color, normal, new Vector2(u1, v0));
        vertices[vertexOffset + 2] = new VertexPositionColorNormalTexture(new Vector3(BottomRight, 0), Color, normal, new Vector2(u1, v1));
        vertices[vertexOffset + 3] = new VertexPositionColorNormalTexture(new Vector3(bottomLeft, 0), Color, normal, new Vector2(u0, v1));

        // Indices: zwei Dreiecke
        indices[indexOffset + 0] = (short)(baseVertex + 0);
        indices[indexOffset + 1] = (short)(baseVertex + 1);
        indices[indexOffset + 2] = (short)(baseVertex + 2);

        indices[indexOffset + 3] = (short)(baseVertex + 2);
        indices[indexOffset + 4] = (short)(baseVertex + 3);
        indices[indexOffset + 5] = (short)(baseVertex + 0);
    }

    public override void Update(GameContext context)
    {
        // z. B. Animationen, Rotation, Farbe
    }
}
