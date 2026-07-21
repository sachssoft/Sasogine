using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Graphics.Primitives;

[Obsolete("Remove Soon")]
public class QuadPrimitive : PrimitiveBase
{
    private Vector2 _position;
    private Vector2 _size;

    public Vector2 Position
    {
        get => _position;
        set { if (_position != value) { _position = value; MarkDirty(); } }
    }

    public Vector2 Size
    {
        get => _size;
        set { if (_size != value) { _size = value; MarkDirty(); } }
    }

    public QuadPrimitive() : this(Vector2.Zero, Vector2.One, Color.White) { }

    public QuadPrimitive(Vector2 position, Vector2 size, Color color, FlipMode uvFlipMode = FlipMode.None)
    {
        _position = position;
        _size = size;
        FillColor = color;
        UVFlipMode = uvFlipMode;
        MarkDirty();
    }

    public static QuadPrimitive FromBounds(Vector2 topLeft, Vector2 bottomRight, Color color, FlipMode uvFlipMode = FlipMode.None)
    {
        Vector2 position = topLeft;
        Vector2 size = bottomRight - topLeft; // Breite und Höhe
        return new QuadPrimitive(position, size, color, uvFlipMode);
    }

    public override int VertexCount => 4;
    public override int IndexCount => 6;

    public override void Fill(VertexPositionColorNormalTexture[] vertices, int vertexOffset, short[] indices, int indexOffset, short baseVertex)
    {
        var (u0, v0, u1, v1) = GetUV(TextureSize);
        Vector3 normal = new Vector3(0, 0, -1);

        // Eckpunkte berechnen
        Vector2 topLeft = Position;
        Vector2 topRight = new Vector2(Position.X + Size.X, Position.Y);
        Vector2 bottomRight = Position + Size;
        Vector2 bottomLeft = new Vector2(Position.X, Position.Y + Size.Y);

        vertices[vertexOffset + 0] = new VertexPositionColorNormalTexture(new Vector3(topLeft, 0), FillColor, normal, new Vector2(u0, v0));
        vertices[vertexOffset + 1] = new VertexPositionColorNormalTexture(new Vector3(topRight, 0), FillColor, normal, new Vector2(u1, v0));
        vertices[vertexOffset + 2] = new VertexPositionColorNormalTexture(new Vector3(bottomRight, 0), FillColor, normal, new Vector2(u1, v1));
        vertices[vertexOffset + 3] = new VertexPositionColorNormalTexture(new Vector3(bottomLeft, 0), FillColor, normal, new Vector2(u0, v1));

        indices[indexOffset + 0] = (short)(baseVertex + 0);
        indices[indexOffset + 1] = (short)(baseVertex + 1);
        indices[indexOffset + 2] = (short)(baseVertex + 2);
        indices[indexOffset + 3] = (short)(baseVertex + 2);
        indices[indexOffset + 4] = (short)(baseVertex + 3);
        indices[indexOffset + 5] = (short)(baseVertex + 0);
    }
}
