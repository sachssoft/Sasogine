using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Geometry;
using Sachssoft.Sasogine.Geometry.Internal;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Meshes;

/// <summary>
/// Provides experimental mesh generation methods that allow callers to
/// control the concrete GPU vertex layout through a vertex factory.
/// </summary>
public static partial class MeshGenerator
{
    public static IMesh CreateQuad<TVertex>(
        GraphicsDevice graphicsDevice,
        MeshVertexFactory<TVertex> vertexFactory,
        float size = 1f,
        bool centerOrigin = false)
        where TVertex : struct, IVertexType
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(vertexFactory);

        float offset = centerOrigin ? size * 0.5f : 0f;
        var normal = Vector3.UnitZ;
        var tangent = Vector3.UnitX;
        var bitangent = Vector3.UnitY;

        MeshVertexData[] data =
        [
            new(new Vector3(-offset, -offset, 0f), normal, tangent, bitangent, Color.White, new Vector2(0f, 1f)),
            new(new Vector3(size - offset, -offset, 0f), normal, tangent, bitangent, Color.White, new Vector2(1f, 1f)),
            new(new Vector3(size - offset, size - offset, 0f), normal, tangent, bitangent, Color.White, new Vector2(1f, 0f)),
            new(new Vector3(-offset, size - offset, 0f), normal, tangent, bitangent, Color.White, new Vector2(0f, 0f))
        ];

        int[] indices = [0, 1, 2, 0, 2, 3];

        return new Mesh<TVertex>(graphicsDevice, CreateVertices(data, vertexFactory), indices);
    }

    public static IMesh CreatePolygon<TVertex>(
        GraphicsDevice graphicsDevice,
        IReadOnlyList<IReadOnlyList<Vector2>> paths,
        MeshVertexFactory<TVertex> vertexFactory,
        float size = 1f,
        bool centerOrigin = false,
        IPolygonTriangulator? triangulatorBackend = null)
        where TVertex : struct, IVertexType
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(vertexFactory);

        triangulatorBackend ??= new LibTessPolygonTriangulator();
        var result = triangulatorBackend.Triangulate(paths, new PolygonTriangulationOptions());
        var positions = result.Vertices;

        if (positions.Count == 0)
            return new Mesh<TVertex>(graphicsDevice, [], Array.Empty<int>());

        GetBounds(positions, out var min, out var max);
        var offset = centerOrigin ? (min + max) * 0.5f : Vector2.Zero;
        var boundsSize = max - min;
        var data = new MeshVertexData[positions.Count];

        for (var i = 0; i < positions.Count; i++)
        {
            var source = positions[i];
            var local = (source - offset) * size;
            var uv = new Vector2(
                boundsSize.X != 0f ? (source.X - min.X) / boundsSize.X : 0f,
                boundsSize.Y != 0f ? (source.Y - min.Y) / boundsSize.Y : 0f);

            data[i] = new MeshVertexData(
                new Vector3(local, 0f),
                Vector3.UnitZ,
                Vector3.UnitX,
                Vector3.UnitY,
                Color.White,
                uv);
        }

        var indices = new int[result.Indices.Count];
        for (var i = 0; i < indices.Length; i++)
            indices[i] = result.Indices[i];

        return new Mesh<TVertex>(graphicsDevice, CreateVertices(data, vertexFactory), indices);
    }

    public static IMesh CreateCube<TVertex>(
        GraphicsDevice graphicsDevice,
        MeshVertexFactory<TVertex> vertexFactory,
        float size = 1f)
        where TVertex : struct, IVertexType
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(vertexFactory);

        float h = size * 0.5f;
        var data = new List<MeshVertexData>(24);

        AddFace(data, new Vector3(-h, -h, -h), new Vector3(h, -h, -h), new Vector3(h, h, -h), new Vector3(-h, h, -h), -Vector3.UnitZ);
        AddFace(data, new Vector3(h, -h, h), new Vector3(-h, -h, h), new Vector3(-h, h, h), new Vector3(h, h, h), Vector3.UnitZ);
        AddFace(data, new Vector3(-h, h, -h), new Vector3(h, h, -h), new Vector3(h, h, h), new Vector3(-h, h, h), Vector3.UnitY);
        AddFace(data, new Vector3(-h, -h, h), new Vector3(h, -h, h), new Vector3(h, -h, -h), new Vector3(-h, -h, -h), -Vector3.UnitY);
        AddFace(data, new Vector3(h, -h, -h), new Vector3(h, -h, h), new Vector3(h, h, h), new Vector3(h, h, -h), Vector3.UnitX);
        AddFace(data, new Vector3(-h, -h, h), new Vector3(-h, -h, -h), new Vector3(-h, h, -h), new Vector3(-h, h, h), -Vector3.UnitX);

        short[] indices = new short[36];
        var k = 0;
        for (short face = 0; face < 6; face++)
        {
            short b = (short)(face * 4);
            indices[k++] = b; indices[k++] = (short)(b + 1); indices[k++] = (short)(b + 2);
            indices[k++] = b; indices[k++] = (short)(b + 2); indices[k++] = (short)(b + 3);
        }

        return new Mesh<TVertex>(graphicsDevice, CreateVertices(data, vertexFactory), indices);
    }

    public static IMesh CreateSphere<TVertex>(
        GraphicsDevice graphicsDevice,
        MeshVertexFactory<TVertex> vertexFactory,
        float radius = 0.5f,
        int segments = 32,
        int rings = 16)
        where TVertex : struct, IVertexType
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(vertexFactory);
        if (segments < 3) throw new ArgumentOutOfRangeException(nameof(segments));
        if (rings < 2) throw new ArgumentOutOfRangeException(nameof(rings));

        var count = (segments + 1) * (rings + 1);
        if (count > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(segments), "The sphere contains too many vertices for a 16-bit index buffer.");

        var data = new MeshVertexData[count];
        int index = 0;

        for (int y = 0; y <= rings; y++)
        {
            float v = (float)y / rings;
            float phi = v * MathHelper.Pi;
            float sinPhi = float.Sin(phi);
            float cosPhi = float.Cos(phi);

            for (int x = 0; x <= segments; x++)
            {
                float u = (float)x / segments;
                float theta = u * MathHelper.TwoPi;
                float sinTheta = float.Sin(theta);
                float cosTheta = float.Cos(theta);

                var normal = new Vector3(sinPhi * cosTheta, cosPhi, sinPhi * sinTheta);
                var tangent = new Vector3(-sinTheta, 0f, cosTheta);
                if (tangent.LengthSquared() > 0f) tangent.Normalize();
                else tangent = Vector3.UnitX;
                var bitangent = Vector3.Cross(normal, tangent);
                if (bitangent.LengthSquared() > 0f) bitangent.Normalize();

                data[index++] = new MeshVertexData(normal * radius, normal, tangent, bitangent, Color.White, new Vector2(u, v));
            }
        }

        var indices = new short[segments * rings * 6];
        index = 0;
        for (int y = 0; y < rings; y++)
        {
            for (int x = 0; x < segments; x++)
            {
                short current = checked((short)(y * (segments + 1) + x));
                short next = checked((short)(current + segments + 1));
                indices[index++] = current;
                indices[index++] = next;
                indices[index++] = checked((short)(current + 1));
                indices[index++] = checked((short)(current + 1));
                indices[index++] = next;
                indices[index++] = checked((short)(next + 1));
            }
        }

        return new Mesh<TVertex>(graphicsDevice, CreateVertices(data, vertexFactory), indices);
    }

    public static IMesh CreateSkybox<TVertex>(
        GraphicsDevice graphicsDevice,
        MeshVertexFactory<TVertex> vertexFactory,
        float size = 1f)
        where TVertex : struct, IVertexType
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(vertexFactory);

        float h = size * 0.5f;
        var data = new List<MeshVertexData>(24);
        AddFace(data, new Vector3(-h, -h, -h), new Vector3(h, -h, -h), new Vector3(h, h, -h), new Vector3(-h, h, -h), Vector3.UnitZ);
        AddFace(data, new Vector3(h, -h, h), new Vector3(-h, -h, h), new Vector3(-h, h, h), new Vector3(h, h, h), -Vector3.UnitZ);
        AddFace(data, new Vector3(-h, h, -h), new Vector3(h, h, -h), new Vector3(h, h, h), new Vector3(-h, h, h), -Vector3.UnitY);
        AddFace(data, new Vector3(-h, -h, h), new Vector3(h, -h, h), new Vector3(h, -h, -h), new Vector3(-h, -h, -h), Vector3.UnitY);
        AddFace(data, new Vector3(h, -h, -h), new Vector3(h, -h, h), new Vector3(h, h, h), new Vector3(h, h, -h), -Vector3.UnitX);
        AddFace(data, new Vector3(-h, -h, h), new Vector3(-h, -h, -h), new Vector3(-h, h, -h), new Vector3(-h, h, h), Vector3.UnitX);

        short[] indices =
        [
            0,2,1, 0,3,2,
            4,5,6, 4,6,7,
            8,10,9, 8,11,10,
            12,14,13, 12,15,14,
            16,18,17, 16,19,18,
            20,21,22, 20,22,23
        ];

        return new Mesh<TVertex>(graphicsDevice, CreateVertices(data, vertexFactory), indices);
    }

    private static TVertex[] CreateVertices<TVertex>(
        IReadOnlyList<MeshVertexData> data,
        MeshVertexFactory<TVertex> factory)
        where TVertex : struct, IVertexType
    {
        var vertices = new TVertex[data.Count];

        for (var i = 0; i < data.Count; i++)
        {
            var vertexData = data[i];
            vertices[i] = factory(in vertexData);
        }

        return vertices;
    }

    private static void AddFace(List<MeshVertexData> data, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal)
    {
        var tangent = p1 - p0;
        tangent.Normalize();
        var bitangent = p3 - p0;
        bitangent.Normalize();

        data.Add(new MeshVertexData(p0, normal, tangent, bitangent, Color.White, new Vector2(0f, 1f)));
        data.Add(new MeshVertexData(p1, normal, tangent, bitangent, Color.White, new Vector2(1f, 1f)));
        data.Add(new MeshVertexData(p2, normal, tangent, bitangent, Color.White, new Vector2(1f, 0f)));
        data.Add(new MeshVertexData(p3, normal, tangent, bitangent, Color.White, new Vector2(0f, 0f)));
    }

    private static void GetBounds(IReadOnlyList<Vector2> positions, out Vector2 min, out Vector2 max)
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
