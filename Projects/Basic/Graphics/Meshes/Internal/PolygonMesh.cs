using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Graphics.Meshes.Internal;

internal sealed class PolygonMesh<TVertex> : Mesh<TVertex>
    where TVertex : struct, IVertexType
{
    public PolygonMesh(
        GraphicsDevice graphicsDevice,
        IReadOnlyList<Vector2> positions,
        IReadOnlyList<int> indices,
        MeshVertexFactory<TVertex> vertexFactory,
        float size,
        bool centerOrigin)
        : base(
            graphicsDevice,
            CreateVertices(positions, vertexFactory, size, centerOrigin),
            CreateIndices(indices))
    {
    }

    private static TVertex[] CreateVertices(
        IReadOnlyList<Vector2> positions,
        MeshVertexFactory<TVertex> vertexFactory,
        float size,
        bool centerOrigin)
    {
        if (positions.Count == 0)
            return [];

        GetBounds(positions, out var min, out var max);

        var offset = centerOrigin ? (min + max) * 0.5f : Vector2.Zero;
        var boundsSize = max - min;
        var vertices = new TVertex[positions.Count];

        for (var i = 0; i < positions.Count; i++)
        {
            var source = positions[i];
            var local = (source - offset) * size;

            var uv = new Vector2(
                boundsSize.X != 0f ? (source.X - min.X) / boundsSize.X : 0f,
                boundsSize.Y != 0f ? (source.Y - min.Y) / boundsSize.Y : 0f);

            var data = new MeshVertexData(
                new Vector3(local, 0f),
                Vector3.UnitZ,
                Vector3.UnitX,
                Vector3.UnitY,
                Color.White,
                uv);

            vertices[i] = vertexFactory(in data);
        }

        return vertices;
    }

    private static int[] CreateIndices(IReadOnlyList<int> indices)
    {
        var result = new int[indices.Count];

        for (var i = 0; i < indices.Count; i++)
            result[i] = indices[i];

        return result;
    }

    private static void GetBounds(
        IReadOnlyList<Vector2> positions,
        out Vector2 min,
        out Vector2 max)
    {
        min = positions[0];
        max = positions[0];

        for (var i = 1; i < positions.Count; i++)
        {
            min = Vector2.Min(min, positions[i]);
            max = Vector2.Max(max, positions[i]);
        }
    }
}