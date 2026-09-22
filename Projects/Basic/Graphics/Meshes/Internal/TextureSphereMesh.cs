using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Engine.Graphics.Meshes.Internal;

internal sealed class TextureSphereMesh<TVertex> : Mesh<TVertex>
    where TVertex : struct, IVertexType
{
    public TextureSphereMesh(
        GraphicsDevice graphicsDevice,
        MeshVertexFactory<TVertex> vertexFactory,
        float radius = 0.5f,
        int segments = 32,
        int rings = 16)
        : base(
            graphicsDevice,
            CreateVertices(radius, segments, rings, vertexFactory),
            CreateIndices(segments, rings))
    {
    }

    private static TVertex[] CreateVertices(
        float radius,
        int segments,
        int rings,
        MeshVertexFactory<TVertex> vertexFactory)
    {
        Validate(segments, rings);

        var vertices = new TVertex[(segments + 1) * (rings + 1)];
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

                var normal = new Vector3(
                    sinPhi * cosTheta,
                    cosPhi,
                    sinPhi * sinTheta);

                var tangent = new Vector3(-sinTheta, 0f, cosTheta);

                if (tangent.LengthSquared() > 0f)
                    tangent.Normalize();
                else
                    tangent = Vector3.UnitX;

                var bitangent = Vector3.Cross(normal, tangent);

                if (bitangent.LengthSquared() > 0f)
                    bitangent.Normalize();

                var data = new MeshVertexData(
                    normal * radius,
                    normal,
                    tangent,
                    bitangent,
                    Color.White,
                    new Vector2(u, v));

                vertices[index++] = vertexFactory(in data);
            }
        }

        return vertices;
    }

    private static short[] CreateIndices(int segments, int rings)
    {
        Validate(segments, rings);

        var indices = new short[segments * rings * 6];
        int index = 0;

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

        return indices;
    }

    private static void Validate(int segments, int rings)
    {
        if (segments < 3)
            throw new ArgumentOutOfRangeException(nameof(segments));

        if (rings < 2)
            throw new ArgumentOutOfRangeException(nameof(rings));

        int count = (segments + 1) * (rings + 1);

        if (count > short.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(segments),
                "The sphere contains too many vertices for a 16-bit index buffer.");
        }
    }
}