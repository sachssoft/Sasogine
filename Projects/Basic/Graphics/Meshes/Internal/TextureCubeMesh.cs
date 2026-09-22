using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Sachssoft.Engine.Graphics.Meshes.Internal;

internal sealed class TextureCubeMesh<TVertex> : Mesh<TVertex>
    where TVertex : struct, IVertexType
{
    public TextureCubeMesh(
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
        var data = new List<MeshVertexData>(24);

        AddFace(data, new Vector3(-h, -h, -h), new Vector3(h, -h, -h),
            new Vector3(h, h, -h), new Vector3(-h, h, -h), -Vector3.UnitZ);

        AddFace(data, new Vector3(h, -h, h), new Vector3(-h, -h, h),
            new Vector3(-h, h, h), new Vector3(h, h, h), Vector3.UnitZ);

        AddFace(data, new Vector3(-h, h, -h), new Vector3(h, h, -h),
            new Vector3(h, h, h), new Vector3(-h, h, h), Vector3.UnitY);

        AddFace(data, new Vector3(-h, -h, h), new Vector3(h, -h, h),
            new Vector3(h, -h, -h), new Vector3(-h, -h, -h), -Vector3.UnitY);

        AddFace(data, new Vector3(h, -h, -h), new Vector3(h, -h, h),
            new Vector3(h, h, h), new Vector3(h, h, -h), Vector3.UnitX);

        AddFace(data, new Vector3(-h, -h, h), new Vector3(-h, -h, -h),
            new Vector3(-h, h, -h), new Vector3(-h, h, h), -Vector3.UnitX);

        var vertices = new TVertex[data.Count];

        for (var i = 0; i < data.Count; i++)
        {
            var vertexData = data[i];
            vertices[i] = vertexFactory(in vertexData);
        }

        return vertices;
    }

    private static int[] CreateIndices()
    {
        var indices = new int[36];
        var k = 0;

        for (var face = 0; face < 6; face++)
        {
            int b = face * 4;

            indices[k++] = b;
            indices[k++] = b + 1;
            indices[k++] = b + 2;
            indices[k++] = b;
            indices[k++] = b + 2;
            indices[k++] = b + 3;
        }

        return indices;
    }

    private static void AddFace(
        List<MeshVertexData> data,
        Vector3 a,
        Vector3 b,
        Vector3 c,
        Vector3 d,
        Vector3 normal)
    {
        var tangent = Vector3.Normalize(b - a);
        var bitangent = Vector3.Normalize(d - a);

        data.Add(new(a, normal, tangent, bitangent, Color.White, new Vector2(0f, 1f)));
        data.Add(new(b, normal, tangent, bitangent, Color.White, new Vector2(1f, 1f)));
        data.Add(new(c, normal, tangent, bitangent, Color.White, new Vector2(1f, 0f)));
        data.Add(new(d, normal, tangent, bitangent, Color.White, new Vector2(0f, 0f)));
    }
}