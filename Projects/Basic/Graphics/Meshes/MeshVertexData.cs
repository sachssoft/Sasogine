using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Graphics.Meshes;

/// <summary>
/// Contains source data generated for a mesh vertex before it is converted
/// to a concrete GPU vertex type.
/// </summary>
public readonly struct MeshVertexData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MeshVertexData"/> structure.
    /// </summary>
    /// <param name="position">The position of the vertex in local mesh space.</param>
    /// <param name="normal">The surface normal of the vertex.</param>
    /// <param name="tangent">The tangent vector of the vertex.</param>
    /// <param name="bitangent">The bitangent vector of the vertex.</param>
    /// <param name="color">The color associated with the vertex.</param>
    /// <param name="textureCoordinate">The texture coordinate of the vertex.</param>
    public MeshVertexData(
        Vector3 position,
        Vector3 normal,
        Vector3 tangent,
        Vector3 bitangent,
        Color color,
        Vector2 textureCoordinate)
    {
        Position = position;
        Normal = normal;
        Tangent = tangent;
        Bitangent = bitangent;
        Color = color;
        TextureCoordinate = textureCoordinate;
    }

    /// <summary>
    /// Gets the position of the vertex in local mesh space.
    /// </summary>
    public Vector3 Position { get; }

    /// <summary>
    /// Gets the surface normal of the vertex.
    /// </summary>
    public Vector3 Normal { get; }

    /// <summary>
    /// Gets the tangent vector of the vertex.
    /// </summary>
    public Vector3 Tangent { get; }

    /// <summary>
    /// Gets the bitangent vector of the vertex.
    /// </summary>
    public Vector3 Bitangent { get; }

    /// <summary>
    /// Gets the color associated with the vertex.
    /// </summary>
    public Color Color { get; }

    /// <summary>
    /// Gets the texture coordinate of the vertex.
    /// </summary>
    public Vector2 TextureCoordinate { get; }
}