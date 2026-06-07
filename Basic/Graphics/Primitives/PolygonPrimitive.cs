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
        private Tess? _cachedTess;
        private bool _isConvex = true;
        private int _cachedVertexCount = 0;
        private int _cachedIndexCount = 0;

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
                    _cachedTess = null;
                    _cachedVertexCount = 0;
                    _cachedIndexCount = 0;
                    MarkDirty();
                }
            }
        }

        /// <summary>
        /// Prüft, ob ein einfaches Polygon konvex ist
        /// </summary>
        private static bool IsConvex(Path polygon)
        {
            int n = polygon.GetPointCount(0);
            if (n < 3) return false;

            bool? sign = null;

            for (int i = 0; i < n; i++)
            {
                var p0 = polygon.GetPoint(0, i);
                var p1 = polygon.GetPoint(0, (i + 1) % n);
                var p2 = polygon.GetPoint(0, (i + 2) % n);

                float dx1 = p1.X - p0.X;
                float dy1 = p1.Y - p0.Y;
                float dx2 = p2.X - p1.X;
                float dy2 = p2.Y - p1.Y;

                float zCross = dx1 * dy2 - dy1 * dx2;

                if (zCross == 0) continue;
                if (sign == null)
                    sign = zCross > 0;
                else if (sign != (zCross > 0))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Caching und Tessellation aller Polygone
        /// </summary>
        private void TessellateAll()
        {
            if (_path == null || _path.IsEmpty()) return;
            if (_cachedTess != null) return;

            _isConvex = true;
            int totalVertices = 0;
            int totalIndices = 0;

            // Prüfe, ob alle Polygone konvex sind
            for (int poly = 0; poly < _path.GetPolygonCount(); poly++)
            {
                int pc = _path.GetPointCount(poly);
                if (pc < 3) continue;

                if (!IsConvex(_path))
                {
                    _isConvex = false;
                    break;
                }
            }

            if (_isConvex)
            {
                // Bei konvex: einfache Vertex/Indexberechnung
                for (int poly = 0; poly < _path.GetPolygonCount(); poly++)
                {
                    int pc = _path.GetPointCount(poly);
                    if (pc < 3) continue;
                    totalVertices += pc;
                    totalIndices += (pc - 2) * 3;
                }
            }
            else
            {
                // Bei konkav: alles in einen Tessellator packen
                var tess = new Tess();
                for (int poly = 0; poly < _path.GetPolygonCount(); poly++)
                {
                    int pc = _path.GetPointCount(poly);
                    if (pc < 3) continue;

                    var contour = new ContourVertex[pc];

                    float minX = _path.LowerBound.X;
                    float minY = _path.LowerBound.Y;
                    float width = MathF.Max(_path.Width, 1e-5f);
                    float height = MathF.Max(_path.Height, 1e-5f);

                    for (int j = 0; j < pc; j++)
                    {
                        var p = _path.GetPoint(poly, j);
                        float x = RelativeToBounds ? (p.X - minX) / width : p.X;
                        float y = RelativeToBounds ? (p.Y - minY) / height : p.Y;

                        contour[j].Position = new Vec3(x, y, 0f);
                    }

                    tess.AddContour(contour, ContourOrientation.Original);
                }

                tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

                _cachedTess = tess;
                totalVertices = tess.Vertices?.Length ?? 0;
                totalIndices = tess.Elements?.Length ?? 0;
            }

            _cachedVertexCount = totalVertices;
            _cachedIndexCount = totalIndices;
        }

        public override int VertexCount
        {
            get
            {
                TessellateAll();
                return _cachedVertexCount;
            }
        }

        public override int IndexCount
        {
            get
            {
                TessellateAll();
                return _cachedIndexCount;
            }
        }

        public override void Fill(VertexPositionColorNormalTexture[] vertices, int vertexOffset,
                                  short[] indices, int indexOffset, short baseVertex)
        {
            if (_path == null || _path.IsEmpty()) return;

            TessellateAll();

            int currentVertex = vertexOffset;
            int currentIndex = indexOffset;

            if (_isConvex)
            {
                for (int poly = 0; poly < _path.GetPolygonCount(); poly++)
                {
                    int pc = _path.GetPointCount(poly);
                    if (pc < 3) continue;

                    float minX = _path.LowerBound.X;
                    float minY = _path.LowerBound.Y;
                    float width = MathF.Max(_path.Width, 1e-5f);
                    float height = MathF.Max(_path.Height, 1e-5f);

                    // Vertices
                    for (int j = 0; j < pc; j++)
                    {
                        var p = _path.GetPoint(poly, j);
                        float x = RelativeToBounds ? (p.X - minX) / width : p.X;
                        float y = RelativeToBounds ? (p.Y - minY) / height : p.Y;

                        vertices[currentVertex++] = new VertexPositionColorNormalTexture(
                            new Vector3(x, y, _depth),
                            FillColor,
                            Vector3.Backward,
                            new Vector2(x, y)
                        );
                    }

                    // Indices (Dreiecksfan)
                    for (int j = 1; j < pc - 1; j++)
                    {
                        indices[currentIndex++] = (short)(baseVertex + 0);
                        indices[currentIndex++] = (short)(baseVertex + j);
                        indices[currentIndex++] = (short)(baseVertex + j + 1);
                    }

                    baseVertex += (short)pc;
                }
            }
            else
            {
                var tess = _cachedTess!;
                for (int i = 0; i < tess.Vertices.Length; i++)
                {
                    var pos = tess.Vertices[i].Position;
                    vertices[currentVertex++] = new VertexPositionColorNormalTexture(
                        new Vector3((float)pos.X, (float)pos.Y, _depth),
                        FillColor,
                        Vector3.Backward,
                        new Vector2((float)pos.X, (float)pos.Y)
                    );
                }

                for (int i = 0; i < tess.Elements.Length; i++)
                {
                    indices[currentIndex++] = (short)(tess.Elements[i] + baseVertex);
                }
            }
        }
    }
}
