using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Primitives
{
    /// <summary>
    /// A simple UV sphere primitive, 3D capable.
    /// </summary>
    public class SpherePrimitive : PrimitiveBase
    {
        private readonly List<Vector3> _vertices = new();
        private readonly List<short> _indices = new();

        private int _segments = 16;
        private int _rings = 16;

        public SpherePrimitive(int segments = 16, int rings = 16)
        {
            _segments = int.Max(segments, 3);
            _rings = int.Max(rings, 2);

            GenerateSphere();
        }

        private void GenerateSphere()
        {
            _vertices.Clear();
            _indices.Clear();

            for (int ring = 0; ring <= _rings; ring++)
            {
                float theta = MathF.PI * ring / _rings;
                float sinTheta = MathF.Sin(theta);
                float cosTheta = MathF.Cos(theta);

                for (int seg = 0; seg <= _segments; seg++)
                {
                    float phi = 2 * MathF.PI * seg / _segments;
                    float sinPhi = MathF.Sin(phi);
                    float cosPhi = MathF.Cos(phi);

                    float x = cosPhi * sinTheta * 0.5f;
                    float y = cosTheta * 0.5f;
                    float z = sinPhi * sinTheta * 0.5f;

                    _vertices.Add(new Vector3(x, y, z));
                }
            }

            for (int ring = 0; ring < _rings; ring++)
            {
                for (int seg = 0; seg < _segments; seg++)
                {
                    int first = (ring * (_segments + 1)) + seg;
                    int second = first + _segments + 1;

                    _indices.Add((short)first);
                    _indices.Add((short)second);
                    _indices.Add((short)(first + 1));

                    _indices.Add((short)(first + 1));
                    _indices.Add((short)second);
                    _indices.Add((short)(second + 1));
                }
            }
        }

        public override int VertexCount => _vertices.Count;
        public override int IndexCount => _indices.Count;

        public override void Fill(VertexPositionColorNormalTexture[] vertices, int vertexOffset, short[] indices, int indexOffset, short baseVertex)
        {
            for (int i = 0; i < _vertices.Count; i++)
            {
                vertices[vertexOffset + i] = new VertexPositionColorNormalTexture(
                    _vertices[i],
                    FillColor,
                    Vector3.Normalize(_vertices[i]),
                    new Vector2((_vertices[i].X + 0.5f), (_vertices[i].Y + 0.5f))
                );
            }

            for (int i = 0; i < _indices.Count; i++)
            {
                indices[indexOffset + i] = (short)(_indices[i] + baseVertex);
            }
        }

        protected override void EffectSetup(IEffectAdapter effect, ICamera camera, Matrix? transform)
        {
            effect.Color = FillColor;
        }
    }
}
