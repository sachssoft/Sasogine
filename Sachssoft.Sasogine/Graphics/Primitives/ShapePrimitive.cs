using LibTessDotNet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Geometry;
using System;

namespace Sachssoft.Sasogine.Graphics.Primitives
{
    /// <summary>
    /// Abstract base class for a shape or polygon.
    /// Supports a single Path and automatically triangulates it.
    /// </summary>
    public abstract class ShapePrimitive : PrimitiveBase
    {
        private bool _relativeToBounds = true;
        private Tess? _tess;
        private float _depth;

        /// <summary>
        /// Creates a shape primitive with optional depth.
        /// </summary>
        /// <param name="depth">Z-depth for the vertices.</param>
        protected ShapePrimitive(float depth = 0f)
        {
            _depth = depth;
        }

        /// <summary>
        /// When true, the polygon points are normalized to 0..1 based on the bounds.
        /// </summary>
        public bool RelativeToBounds
        {
            get => _relativeToBounds;
            set
            {
                if (_relativeToBounds != value)
                {
                    _relativeToBounds = value;
                    MarkDirty(); // Wichtig: neu triangulieren
                }
            }
        }

        /// <summary>
        /// Must be implemented by subclasses to provide the Path to draw.
        /// </summary>
        protected abstract Path DefinedPath { get; }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
                    count += (pc - 2) * 3; // Triangle fan
                }
                return count;
            }
        }

        /// <summary>
        /// Creates a ShapePrimitive from a given Path.
        /// </summary>
        /// <param name="definedPath">The Path to render.</param>
        /// <returns>A ShapePrimitive instance.</returns>
        public static ShapePrimitive Create(Path definedPath) => new AnonymousShape(definedPath);

        /// <inheritdoc/>
        public override void Fill(VertexPositionColorNormalTexture[] vertices, int vertexOffset, short[] indices, int indexOffset, short baseVertex)
        {
            if (DefinedPath == null || DefinedPath.IsEmpty())
                return;

            _tess = new Tess();

            float minX = DefinedPath.LowerBound.X;
            float minY = DefinedPath.LowerBound.Y;
            float width = MathF.Max(DefinedPath.Width, 1e-5f);
            float height = MathF.Max(DefinedPath.Height, 1e-5f);

            int currentVertex = vertexOffset;
            int currentIndex = indexOffset;

            for (int polyIndex = 0; polyIndex < DefinedPath.GetPolygonCount(); polyIndex++)
            {
                int pc = DefinedPath.GetPointCount(polyIndex);
                if (pc < 3) continue;

                var contour = new ContourVertex[pc];

                for (int j = 0; j < pc; j++)
                {
                    var p = DefinedPath.GetPoint(polyIndex, j);
                    float x = RelativeToBounds ? (p.X - minX) / width : p.X;
                    float y = RelativeToBounds ? (p.Y - minY) / height : p.Y;

                    contour[j].Position = new Vec3(x, y, 0f);
                }

                _tess.AddContour(contour, ContourOrientation.Original);
            }

            _tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

            if (_tess.Vertices == null || _tess.Elements == null)
                return;

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

        /// <inheritdoc/>
        protected override void EffectSetup(IEffectAdapter effect, CameraBase camera, Matrix? transform)
        {
            effect.Color = FillColor;
        }

        /// <summary>
        /// Anonymous implementation for creating a ShapePrimitive from a Path.
        /// </summary>
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
