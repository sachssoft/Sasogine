using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Sachssoft.Sasogine.Graphics.Primitives
{
    public class EllipsePrimitive : PrimitiveBase
    {
        private int _segments;

        /// <summary>
        /// Anzahl der Segmente (Glätte)
        /// </summary>
        public int Segments
        {
            get => _segments;
            set
            {
                if (value < 3) value = 3;
                if (_segments != value)
                {
                    _segments = value;
                    MarkDirty();
                }
            }
        }

        public EllipsePrimitive(int segments = 32)
        {
            _segments = segments < 3 ? 3 : segments;
        }

        public override int VertexCount => _segments + 1;
        public override int IndexCount => _segments * 3;

        /// <summary>
        /// Füllt das Einheitsquadrat [0..1] mit einer Ellipse, zentriert bei 0.5,0.5
        /// </summary>
        public override void Fill(VertexPositionColorNormalTexture[] vertices, int vertexOffset, short[] indices, int indexOffset, short baseVertex)
        {
            float centerX = 0.5f;
            float centerY = 0.5f;
            float radiusX = 0.5f;
            float radiusY = 0.5f;

            // Mittelpunkt
            vertices[vertexOffset + 0] = new VertexPositionColorNormalTexture(
                new Microsoft.Xna.Framework.Vector3(centerX, centerY, 0f),
                FillColor,
                Microsoft.Xna.Framework.Vector3.Backward,
                new Microsoft.Xna.Framework.Vector2(centerX, centerY)
            );

            float angleStep = MathHelper.TwoPi / _segments;
            for (int i = 0; i < _segments; i++)
            {
                float angle = i * angleStep;
                float x = centerX + radiusX * MathF.Cos(angle);
                float y = centerY + radiusY * MathF.Sin(angle);

                vertices[vertexOffset + 1 + i] = new VertexPositionColorNormalTexture(
                    new Microsoft.Xna.Framework.Vector3(x, y, 0f),
                    FillColor,
                    Microsoft.Xna.Framework.Vector3.Backward,
                    new Microsoft.Xna.Framework.Vector2(x, y)
                );
            }

            // Indices für Dreiecksfan
            for (int i = 0; i < _segments; i++)
            {
                indices[indexOffset + i * 3 + 0] = (short)baseVertex;
                indices[indexOffset + i * 3 + 1] = (short)(baseVertex + 1 + i);
                indices[indexOffset + i * 3 + 2] = (short)(baseVertex + 1 + ((i + 1) % _segments));
            }
        }
    }
}
