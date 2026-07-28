using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Sachssoft.Sasogine.Graphics.Meshes.Internal;

internal sealed class TextureSphereMesh
    : Mesh<VertexPositionTexture>
{
    public TextureSphereMesh(
        GraphicsDevice graphicsDevice,
        float radius = 0.5f,
        int segments = 32,
        int rings = 16)
        : base(
            graphicsDevice,
            CreateVertices(radius, segments, rings),
            CreateIndices(segments, rings))
    {
    }


    private static VertexPositionTexture[] CreateVertices(
        float radius,
        int segments,
        int rings)
    {
        var vertices = new VertexPositionTexture[(segments + 1) * (rings + 1)];

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

                Vector3 position = new(
                    sinPhi * cosTheta,
                    cosPhi,
                    sinPhi * sinTheta);

                vertices[index++] = new VertexPositionTexture(
                    position * radius,
                    new Vector2(u, v));
            }
        }

        return vertices;
    }


    private static short[] CreateIndices(
        int segments,
        int rings)
    {
        var indices = new short[segments * rings * 6];

        int index = 0;

        for (int y = 0; y < rings; y++)
        {
            for (int x = 0; x < segments; x++)
            {
                short current = (short)(y * (segments + 1) + x);
                short next = (short)(current + segments + 1);

                indices[index++] = current;
                indices[index++] = next;
                indices[index++] = (short)(current + 1);

                indices[index++] = (short)(current + 1);
                indices[index++] = next;
                indices[index++] = (short)(next + 1);
            }
        }

        return indices;
    }
}