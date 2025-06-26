using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace sachssoft.Sasogine.Graphics3D;

public class SphericPrimitive
{
    private VertexPositionNormalTexture[] _vertices;
    private short[] _indices;

    public VertexBuffer VertexBuffer { get; private set; }
    public IndexBuffer IndexBuffer { get; private set; }
    public int PrimitiveCount => _indices.Length / 3;

    public int LatitudeSegments { get; }
    public int LongitudeSegments { get; }
    public float Radius { get; }

    public BoundingSphere BoundingSphere { get; private set; }

    public SphericPrimitive(GraphicsDevice graphics_device, float radius = 1f, int latitude_segments = 16, int longitude_segments = 16)
    {
        Radius = radius;
        LatitudeSegments = latitude_segments;
        LongitudeSegments = longitude_segments;

        GenerateSphere(graphics_device);
        BoundingSphere = new BoundingSphere(Vector3.Zero, radius);
    }

    private void GenerateSphere(GraphicsDevice graphics_device)
    {
        var vertices = new List<VertexPositionNormalTexture>();
        var indices = new List<short>();

        for (int lat = 0; lat <= LatitudeSegments; lat++)
        {
            float theta = float.Pi * lat / LatitudeSegments;
            float sin_theta = float.Sin(theta);
            float cos_theta = float.Cos(theta);

            for (int lon = 0; lon <= LongitudeSegments; lon++)
            {
                float phi = 2f * float.Pi * lon / LongitudeSegments;
                float sin_phi = float.Sin(phi);
                float cos_phi = float.Cos(phi);

                Vector3 normal = new Vector3(
                    cos_phi * sin_theta,
                    cos_theta,
                    sin_phi * sin_theta
                );

                Vector2 texture = new Vector2((float)lon / LongitudeSegments, (float)lat / LatitudeSegments);
                Vector3 position = normal * Radius;

                vertices.Add(new VertexPositionNormalTexture(position, normal, texture));
            }
        }

        for (int lat = 0; lat < LatitudeSegments; lat++)
        {
            for (int lon = 0; lon < LongitudeSegments; lon++)
            {
                int first = lat * (LongitudeSegments + 1) + lon;
                int second = first + LongitudeSegments + 1;

                indices.Add((short)first);
                indices.Add((short)second);
                indices.Add((short)(first + 1));

                indices.Add((short)(first + 1));
                indices.Add((short)second);
                indices.Add((short)(second + 1));
            }
        }

        _vertices = vertices.ToArray();
        _indices = indices.ToArray();

        VertexBuffer = new VertexBuffer(graphics_device, typeof(VertexPositionNormalTexture), _vertices.Length, BufferUsage.WriteOnly);
        VertexBuffer.SetData(_vertices);

        IndexBuffer = new IndexBuffer(graphics_device, IndexElementSize.SixteenBits, _indices.Length, BufferUsage.WriteOnly);
        IndexBuffer.SetData(_indices);
    }

    public void Draw(GraphicsDevice device, BasicEffect effect, bool draw_wireframe = false)
    {
        var original_state = device.RasterizerState;

        if (draw_wireframe)
        {
            var wire = new RasterizerState { FillMode = FillMode.WireFrame, CullMode = CullMode.None };
            device.RasterizerState = wire;
        }

        device.SetVertexBuffer(VertexBuffer);
        device.Indices = IndexBuffer;

        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, PrimitiveCount);
        }

        if (draw_wireframe)
            device.RasterizerState = original_state;
    }
}
