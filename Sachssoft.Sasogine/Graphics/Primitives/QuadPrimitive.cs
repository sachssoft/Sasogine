using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Graphics.Primitives;

public class QuadPrimitive : PrimitiveBase
{
    private Vector2 _topLeft;
    private Vector2 _bottomRight;
    private Color _fillColor = Color.White;

    public Vector2 TopLeft
    {
        get => _topLeft;
        set { if (_topLeft != value) { _topLeft = value; MarkDirty(); } }
    }

    public Vector2 BottomRight
    {
        get => _bottomRight;
        set { if (_bottomRight != value) { _bottomRight = value; MarkDirty(); } }
    }

    public Color FillColor
    {
        get => _fillColor;
        set { if (_fillColor != value) { _fillColor = value; MarkDirty(); } }
    }

    public QuadPrimitive() : this(Vector2.Zero, Vector2.One, Color.White) { }

    public QuadPrimitive(Vector2 topLeft, Vector2 bottomRight, Color color)
    {
        _topLeft = topLeft;
        _bottomRight = bottomRight;
        _fillColor = color;
        MarkDirty();
    }

    public override int VertexCount => 4;

    public override int IndexCount => 6;

    public override void Fill(VertexPositionColorNormalTexture[] vertices, int vertexOffset, short[] indices, int indexOffset, short baseVertex)
    {
        var (u0, v0, u1, v1) = GetUV(new Point(1, 1));
        Vector3 normal = new Vector3(0, 0, -1);

        Vector2 topRight = new Vector2(BottomRight.X, TopLeft.Y);
        Vector2 bottomLeft = new Vector2(TopLeft.X, BottomRight.Y);

        vertices[vertexOffset + 0] = new VertexPositionColorNormalTexture(new Vector3(TopLeft, 0), FillColor, normal, new Vector2(u0, v0));
        vertices[vertexOffset + 1] = new VertexPositionColorNormalTexture(new Vector3(topRight, 0), FillColor, normal, new Vector2(u1, v0));
        vertices[vertexOffset + 2] = new VertexPositionColorNormalTexture(new Vector3(BottomRight, 0), FillColor, normal, new Vector2(u1, v1));
        vertices[vertexOffset + 3] = new VertexPositionColorNormalTexture(new Vector3(bottomLeft, 0), FillColor, normal, new Vector2(u0, v1));

        indices[indexOffset + 0] = (short)(baseVertex + 0);
        indices[indexOffset + 1] = (short)(baseVertex + 1);
        indices[indexOffset + 2] = (short)(baseVertex + 2);

        indices[indexOffset + 3] = (short)(baseVertex + 2);
        indices[indexOffset + 4] = (short)(baseVertex + 3);
        indices[indexOffset + 5] = (short)(baseVertex + 0);
    }
}
