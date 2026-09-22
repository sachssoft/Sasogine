using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Engine.Graphics.Meshes.Internal;

internal sealed class TextureSkyboxMesh<TVertex> : Mesh<TVertex>
    where TVertex : struct, IVertexType
{
    public TextureSkyboxMesh(
        GraphicsDevice graphicsDevice,
        MeshVertexFactory<TVertex> vertexFactory,
        float size = 1f)
        : base(
            graphicsDevice,
            CreateVertices(size, vertexFactory),
            CreateIndices())
    {
    }

    private static TVertex[] CreateVertices(
        float size,
        MeshVertexFactory<TVertex> vertexFactory)
    {
        float h = size * 0.5f;

        MeshVertexData[] data =
        [
            // Front
            CreateVertex(new Vector3(-h, -h, -h), new Vector2(0f, 1f)),
            CreateVertex(new Vector3(h, -h, -h), new Vector2(1f, 1f)),
            CreateVertex(new Vector3(h, h, -h), new Vector2(1f, 0f)),
            CreateVertex(new Vector3(-h, h, -h), new Vector2(0f, 0f)),

            // Back
            CreateVertex(new Vector3(h, -h, h), new Vector2(0f, 1f)),
            CreateVertex(new Vector3(-h, -h, h), new Vector2(1f, 1f)),
            CreateVertex(new Vector3(-h, h, h), new Vector2(1f, 0f)),
            CreateVertex(new Vector3(h, h, h), new Vector2(0f, 0f)),

            // Top
            CreateVertex(new Vector3(-h, h, -h), new Vector2(0f, 1f)),
            CreateVertex(new Vector3(h, h, -h), new Vector2(1f, 1f)),
            CreateVertex(new Vector3(h, h, h), new Vector2(1f, 0f)),
            CreateVertex(new Vector3(-h, h, h), new Vector2(0f, 0f)),

            // Bottom
            CreateVertex(new Vector3(-h, -h, h), new Vector2(0f, 1f)),
            CreateVertex(new Vector3(h, -h, h), new Vector2(1f, 1f)),
            CreateVertex(new Vector3(h, -h, -h), new Vector2(1f, 0f)),
            CreateVertex(new Vector3(-h, -h, -h), new Vector2(0f, 0f)),

            // Right
            CreateVertex(new Vector3(h, -h, -h), new Vector2(0f, 1f)),
            CreateVertex(new Vector3(h, -h, h), new Vector2(1f, 1f)),
            CreateVertex(new Vector3(h, h, h), new Vector2(1f, 0f)),
            CreateVertex(new Vector3(h, h, -h), new Vector2(0f, 0f)),

            // Left
            CreateVertex(new Vector3(-h, -h, h), new Vector2(0f, 1f)),
            CreateVertex(new Vector3(-h, -h, -h), new Vector2(1f, 1f)),
            CreateVertex(new Vector3(-h, h, -h), new Vector2(1f, 0f)),
            CreateVertex(new Vector3(-h, h, h), new Vector2(0f, 0f))
        ];

        var vertices = new TVertex[data.Length];

        for (var i = 0; i < data.Length; i++)
            vertices[i] = vertexFactory(in data[i]);

        return vertices;
    }

    private static MeshVertexData CreateVertex(
        Vector3 position,
        Vector2 textureCoordinate)
    {
        var normal = -Vector3.Normalize(position);

        return new MeshVertexData(
            position,
            normal,
            Vector3.Zero,
            Vector3.Zero,
            Color.White,
            textureCoordinate);
    }

    private static short[] CreateIndices()
    {
        return
        [
            // Umgekehrtes Winding für die Innenansicht.

            // Front
            0, 2, 1,
            0, 3, 2,

            // Back
            4, 5, 6,
            4, 6, 7,

            // Top
            8, 10, 9,
            8, 11, 10,

            // Bottom
            12, 14, 13,
            12, 15, 14,

            // Right
            16, 18, 17,
            16, 19, 18,

            // Left
            20, 21, 22,
            20, 22, 23
        ];
    }
}