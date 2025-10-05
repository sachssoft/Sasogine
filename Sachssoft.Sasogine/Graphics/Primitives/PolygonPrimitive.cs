using LibTessDotNet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Geometry;
using System;

namespace Sachssoft.Sasogine.Graphics.Primitives
{
    public class PolygonPrimitive : PrimitiveBase
    {
        private Path? _path;
        private float _depth;
        private Tess? _tess;

        /// <summary>
        /// Wenn true, werden die Polygonkoordinaten relativ zu den Bounds auf 0..1 normalisiert
        /// </summary>
        public bool RelativeToBounds { get; set; } = true;

        public PolygonPrimitive(Path? path = null, float depth = 0f)
        {
            _path = path;
            _depth = depth;
        }

        public Path? Path
        {
            get => _path;
            set
            {
                if (_path != value)
                {
                    _path = value;
                    MarkDirty();
                }
            }
        }

        // VertexCount direkt aus Path berechnen
        public override int VertexCount
        {
            get
            {
                if (_path == null) return 0;
                int count = 0;
                for (int p = 0; p < _path.GetPolygonCount(); p++)
                    count += _path.GetPointCount(p);
                return count;
            }
        }

        // IndexCount direkt aus Path berechnen (Dreiecksfan)
        public override int IndexCount
        {
            get
            {
                if (_path == null) return 0;
                int count = 0;
                for (int p = 0; p < _path.GetPolygonCount(); p++)
                {
                    int pc = _path.GetPointCount(p);
                    if (pc < 3) continue;
                    count += (pc - 2) * 3;
                }
                return count;
            }
        }

        // Fill schreibt direkt in die Arrays, die vom Base bereitgestellt werden
        public override void Fill(VertexPositionColorNormalTexture[] vertices, int vertexOffset, short[] indices, int indexOffset, short baseVertex)
        {
            if (_path == null || _path.IsEmpty())
                return;

            _tess = new Tess();

            float minX = _path.LowerBound.X;
            float minY = _path.LowerBound.Y;
            float width = MathF.Max(_path.Width, 1e-5f);
            float height = MathF.Max(_path.Height, 1e-5f);

            int currentVertex = vertexOffset;
            int currentIndex = indexOffset;

            for (int poly = 0; poly < _path.GetPolygonCount(); poly++)
            {
                int pc = _path.GetPointCount(poly);
                if (pc < 3) continue;

                var contour = new ContourVertex[pc];
                for (int j = 0; j < pc; j++)
                {
                    var p = _path.GetPoint(poly, j);
                    float x = RelativeToBounds ? (p.X - minX) / width : p.X;
                    float y = RelativeToBounds ? (p.Y - minY) / height : p.Y;

                    contour[j].Position = new Vec3(x, y, 0f);
                }

                _tess.AddContour(contour, ContourOrientation.Original);
            }

            _tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

            if (_tess.Vertices == null || _tess.Elements == null)
                return;

            // Vertex direkt in das vom Base bereitgestellte Array schreiben
            for (int i = 0; i < _tess.Vertices.Length; i++)
            {
                var pos = _tess.Vertices[i].Position;
                vertices[currentVertex++] = new VertexPositionColorNormalTexture(
                    new Vector3((float)pos.X, (float)pos.Y, _depth),
                    FillColor,
                    Vector3.Backward,
                    new Vector2((float)pos.X, (float)pos.Y)
                );
            }

            // Index direkt in das vom Base bereitgestellte Array schreiben
            for (int i = 0; i < _tess.Elements.Length; i++)
            {
                indices[currentIndex++] = (short)(_tess.Elements[i] + baseVertex);
            }
        }
    }
}
