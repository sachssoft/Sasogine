using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Engine.Graphics.Meshes.Internal;

internal sealed class QuadMesh<TVertex> : Mesh<TVertex>
    where TVertex : struct, IVertexType
{
    public QuadMesh(
        GraphicsDevice graphicsDevice,
        MeshVertexFactory<TVertex> vertexFactory,
        float size = 1f,
        bool centerOrigin = false,
        Texture2DFlipMode flipMode = Texture2DFlipMode.None)
        : base(
            graphicsDevice,
            CreateVertices(size, centerOrigin, flipMode, vertexFactory),
            CreateIndices())
    {
    }

    private static TVertex[] CreateVertices(
        float size,
        bool centerOrigin,
        Texture2DFlipMode flipMode,
        MeshVertexFactory<TVertex> vertexFactory)
    {
        float offset = centerOrigin ? size * 0.5f : 0f;

        bool flipHorizontal = (flipMode & Texture2DFlipMode.Horizontal) != 0;
        bool flipVertical = (flipMode & Texture2DFlipMode.Vertical) != 0;

        float left = flipHorizontal ? 1f : 0f;
        float right = flipHorizontal ? 0f : 1f;
        float top = flipVertical ? 0f : 1f;
        float bottom = flipVertical ? 1f : 0f;

        var normal = Vector3.UnitZ;
        var tangent = Vector3.UnitX;
        var bitangent = Vector3.UnitY;

        MeshVertexData[] data =
        [
            new(new Vector3(-offset, -offset, 0f), normal, tangent, bitangent,
                Color.White, new Vector2(left, top)),
            new(new Vector3(size - offset, -offset, 0f), normal, tangent, bitangent,
                Color.White, new Vector2(right, top)),
            new(new Vector3(size - offset, size - offset, 0f), normal, tangent, bitangent,
                Color.White, new Vector2(right, bottom)),
            new(new Vector3(-offset, size - offset, 0f), normal, tangent, bitangent,
                Color.White, new Vector2(left, bottom))
        ];

        var vertices = new TVertex[data.Length];

        for (int i = 0; i < data.Length; i++)
            vertices[i] = vertexFactory(data[i]);

        return vertices;
    }

    private static int[] CreateIndices()
    {
        return [0, 1, 2, 0, 2, 3];
    }
}