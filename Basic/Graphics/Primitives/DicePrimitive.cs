using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Components.Rendering.Camera;
using Sachssoft.Sasogine.Graphics.Primitives;

namespace Sachssoft.Sasogine.Graphics.Primitives
{
    /// <summary>
    /// A simple cube (dice) primitive, 3D capable.
    /// </summary>
    public class DicePrimitive : PrimitiveBase
    {
        private readonly Vector3[] _vertices;
        private readonly short[] _indices;

        public DicePrimitive()
        {
            // Einheitswürfel zentriert auf 0,0,0
            _vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, -0.5f), // 0
                new Vector3( 0.5f, -0.5f, -0.5f), // 1
                new Vector3( 0.5f,  0.5f, -0.5f), // 2
                new Vector3(-0.5f,  0.5f, -0.5f), // 3
                new Vector3(-0.5f, -0.5f,  0.5f), // 4
                new Vector3( 0.5f, -0.5f,  0.5f), // 5
                new Vector3( 0.5f,  0.5f,  0.5f), // 6
                new Vector3(-0.5f,  0.5f,  0.5f), // 7
            };

            _indices = new short[]
            {
                0,1,2, 0,2,3, // back
                4,6,5, 4,7,6, // front
                0,4,5, 0,5,1, // bottom
                3,2,6, 3,6,7, // top
                1,5,6, 1,6,2, // right
                0,3,7, 0,7,4  // left
            };
        }

        public override int VertexCount => _vertices.Length;
        public override int IndexCount => _indices.Length;

        public override void Fill(VertexPositionColorNormalTexture[] vertices, int vertexOffset, short[] indices, int indexOffset, short baseVertex)
        {
            for (int i = 0; i < _vertices.Length; i++)
            {
                vertices[vertexOffset + i] = new VertexPositionColorNormalTexture(
                    _vertices[i],
                    FillColor,
                    Vector3.Backward,
                    new Vector2(_vertices[i].X + 0.5f, _vertices[i].Y + 0.5f)
                );
            }

            for (int i = 0; i < _indices.Length; i++)
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
