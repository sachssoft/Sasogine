using LibTessDotNet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Geometry;
using System;

namespace Sachssoft.Sasogine.Graphics.Primitives
{
    public abstract class ShapePrimitive : PrimitiveBase
    {
        private bool _relativeToBounds = true;
        private Tess? _tess;
        private float _depth;

        protected ShapePrimitive(float depth = 0f)
        {
            _depth = depth;
        }

        public bool RelativeToBounds
        {
            get => _relativeToBounds;
            set
            {
                if (_relativeToBounds != value)
                {
                    _relativeToBounds = value;
                    MarkDirty();
                }
            }
        }

        protected abstract Path DefinedPath { get; }

        public override int VertexCount
        {
            get
            {
                if (DefinedPath == null) return 0;
                int count = 0;
                for (int poly = 0; poly < DefinedPath.GetPolygonCount(); poly++)
                    count += DefinedPath.GetPointCount(poly);
                return count;
            }
        }

        public override int IndexCount
        {
            get
            {
                if (DefinedPath == null) return 0;
                int count = 0;
                for (int poly = 0; poly < DefinedPath.GetPolygonCount(); poly++)
                {
                    int pc = DefinedPath.GetPointCount(poly);
                    if (pc < 3) continue;
                    count += (pc - 2) * 3;
                }
                return count;
            }
        }

        public static ShapePrimitive Create(Path definedPath) => new AnonymousShape(definedPath);

        public override void Fill(
            VertexPositionColorNormalTexture[] vertices,
            int vertexOffset,
            short[] indices,
            int indexOffset,
            short baseVertex)
        {
            var path = DefinedPath;
            if (path == null || path.IsEmpty())
                return;

            // Prüfen auf ungültige Punkte
            for (int poly = 0; poly < path.GetPolygonCount(); poly++)
            {
                for (int i = 0; i < path.GetPointCount(poly); i++)
                {
                    var p = path.GetPoint(poly, i);
                    if (float.IsNaN(p.X) || float.IsNaN(p.Y) || float.IsInfinity(p.X) || float.IsInfinity(p.Y))
                        return; // Ungültiger Path → unsichtbar
                }
            }

            _tess = new Tess();

            float minX = path.LowerBound.X;
            float minY = path.LowerBound.Y;
            float width = MathF.Max(path.Width, 1e-5f);
            float height = MathF.Max(path.Height, 1e-5f);

            int currentVertex = vertexOffset;
            int currentIndex = indexOffset;

            bool anyContour = false;

            // Konturen hinzufügen
            for (int poly = 0; poly < path.GetPolygonCount(); poly++)
            {
                int pc = path.GetPointCount(poly);
                if (pc < 3) continue;

                var contour = new ContourVertex[pc];
                bool skip = false;

                for (int i = 0; i < pc; i++)
                {
                    var p = path.GetPoint(poly, i);
                    float x = RelativeToBounds ? (p.X - minX) / width : p.X;
                    float y = RelativeToBounds ? (p.Y - minY) / height : p.Y;

                    if (float.IsNaN(x) || float.IsNaN(y) || float.IsInfinity(x) || float.IsInfinity(y))
                    {
                        skip = true;
                        break;
                    }

                    contour[i].Position = new Vec3(x, y, 0f);
                }

                if (skip) continue;

                _tess.AddContour(contour, ContourOrientation.Original);
                anyContour = true;
            }

            if (!anyContour) return;

            try
            {
                _tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);
            }
            catch
            {
                return;
            }

            if (_tess.Vertices == null || _tess.Elements == null)
                return;

            // Vertex-Array überprüfen
            if (vertices.Length < currentVertex + _tess.Vertices.Length ||
                indices.Length < currentIndex + _tess.Elements.Length)
                return; // Arrays zu klein → nicht rendern

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

            for (int i = 0; i < _tess.Elements.Length; i++)
            {
                indices[currentIndex++] = (short)(_tess.Elements[i] + baseVertex);
            }
        }

        protected override void EffectSetup(IEffectAdapter effect, CameraBase camera, Matrix? transform)
        {
            effect.Color = FillColor;
        }

        private class AnonymousShape : ShapePrimitive
        {
            private readonly Path _definedPath;

            public AnonymousShape(Path definedPath)
            {
                _definedPath = definedPath ?? throw new ArgumentNullException(nameof(definedPath));
            }

            protected override Path DefinedPath => _definedPath;
        }
    }
}
