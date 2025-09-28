using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine;
using Sachssoft.Sasogine.Graphics.Primitives;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Primitives
{
    public sealed class SpherePrimitive : PrimitiveBase
    {
        private readonly VertexPositionNormalTexture[] _vertices;
        private readonly short[] _indices;

        public Texture2D? Texture { get; set; } 

        public float Radius { get; }
        public int LatitudeSegments { get; }
        public int LongitudeSegments { get; }

        public override int VertexCount => _vertices.Length;
        public override int IndexCount => _indices.Length;

        public SpherePrimitive(float radius = 1f, int latitudeSegments = 16, int longitudeSegments = 16)
        {
            Radius = radius;
            LatitudeSegments = latitudeSegments;
            LongitudeSegments = longitudeSegments;

            (_vertices, _indices) = GenerateSphere(radius, latitudeSegments, longitudeSegments);
        }

        private static (VertexPositionNormalTexture[], short[]) GenerateSphere(float radius, int latSeg, int lonSeg)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<short>();

            for (int lat = 0; lat <= latSeg; lat++)
            {
                float theta = MathHelper.Pi * lat / latSeg;
                float sinTheta = (float)Math.Sin(theta);
                float cosTheta = (float)Math.Cos(theta);

                for (int lon = 0; lon <= lonSeg; lon++)
                {
                    float phi = 2f * MathHelper.Pi * lon / lonSeg;
                    float sinPhi = (float)Math.Sin(phi);
                    float cosPhi = (float)Math.Cos(phi);

                    Vector3 normal = new Vector3(
                        cosPhi * sinTheta,
                        cosTheta,
                        sinPhi * sinTheta
                    );

                    Vector3 position = normal * radius;
                    Vector2 texCoord = new Vector2((float)lon / lonSeg, (float)lat / latSeg);

                    vertices.Add(new VertexPositionNormalTexture(position, normal, texCoord));
                }
            }

            for (int lat = 0; lat < latSeg; lat++)
            {
                for (int lon = 0; lon < lonSeg; lon++)
                {
                    int first = lat * (lonSeg + 1) + lon;
                    int second = first + lonSeg + 1;

                    indices.Add((short)first);
                    indices.Add((short)second);
                    indices.Add((short)(first + 1));

                    indices.Add((short)(first + 1));
                    indices.Add((short)second);
                    indices.Add((short)(second + 1));
                }
            }

            return (vertices.ToArray(), indices.ToArray());
        }

        public override void Fill(
            VertexPositionColorNormalTexture[] verticesBuffer,
            int vertexOffset,
            short[] indicesBuffer,
            int indexOffset,
            short baseVertex)
        {
            // Berechne UVs
            var (u0, v0, u1, v1) = GetUV(TextureSize);

            for (int i = 0; i < _vertices.Length; i++)
            {
                var v = _vertices[i];
                Vector2 uv = new Vector2(
                    u0 + (u1 - u0) * v.TextureCoordinate.X,
                    v0 + (v1 - v0) * v.TextureCoordinate.Y
                );

                verticesBuffer[vertexOffset + i] = new VertexPositionColorNormalTexture(
                    v.Position,
                    FillColor,
                    v.Normal,
                    uv
                );
            }

            for (int i = 0; i < _indices.Length; i++)
            {
                indicesBuffer[indexOffset + i] = (short)(_indices[i] + baseVertex);
            }
        }

        protected override void EffectSetup(IEffectAdapter effect, CameraBase camera, Matrix? transform)
        {
            effect.Texture = Texture;
            base.EffectSetup(effect, camera, transform);
        }

        public override void Update(GameFrameContext context)
        {
            // Optional: Rotation, Animation, Farbe
        }
    }
}
