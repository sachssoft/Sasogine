using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics.Meshes;

/// <summary>
/// Contains source data generated for a mesh vertex before it is converted
/// to a concrete GPU vertex type.
/// </summary>
public readonly struct MeshVertexData
{
    public Vector3 Position { get; }
    public Vector3 Normal { get; }
    public Vector3 Tangent { get; }
    public Vector3 Bitangent { get; }
    public Color Color { get; }
    public Vector2 TextureCoordinate { get; }

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
}
