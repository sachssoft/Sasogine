using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace Sachssoft.Engine.Graphics.Rendering.Batches;

/// <summary>
/// Represents a vertex used by <see cref="ShapeBatch"/>.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ShapeVertex : IVertexType
{
    /// <summary>
    /// Describes the shape vertex layout.
    /// </summary>
    public static readonly VertexDeclaration VertexDeclaration = new(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
        new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

    /// <summary>
    /// The vertex position.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// The vertex color.
    /// </summary>
    public Color Color;

    /// <summary>
    /// The vertex texture coordinate.
    /// </summary>
    public Vector2 TextureCoordinate;

    /// <summary>
    /// Initializes a new shape vertex.
    /// </summary>
    public ShapeVertex(
        Vector3 position,
        Color color,
        Vector2 textureCoordinate)
    {
        Position = position;
        Color = color;
        TextureCoordinate = textureCoordinate;
    }
}
