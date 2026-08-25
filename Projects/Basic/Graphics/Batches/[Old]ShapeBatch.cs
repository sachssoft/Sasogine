//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Sachssoft.Sasogine.Common;
//using Sachssoft.Sasogine.Geometry;
//using Sachssoft.Sasogine.Graphics.Cameras;
//using Sachssoft.Sasogine.Graphics.Rendering;
//using System;
//using System.Collections.Generic;

//namespace Sachssoft.Sasogine.Graphics.Shapes
//{

//    public sealed class ShapeBatch_OLD : IDisposable
//    {
//        private const float Epsilon = 0.000001f;
//        private const float MiterLimit = 4f;

//        private readonly GraphicsDevice _graphicsDevice;

//        private readonly List<VertexPositionTexture> _vertices = new();
//        private readonly List<int> _indices = new();

//        private DynamicVertexBuffer? _vertexBuffer;
//        private IndexBuffer? _indexBuffer;

//        private IShader? _shader;
//        private ICamera? _camera;

//        private bool _begun;
//        private bool _disposed;

//        public ShapeBatch_OLD(
//            GraphicsDevice graphicsDevice)
//        {
//            _graphicsDevice =
//                graphicsDevice ??
//                throw new ArgumentNullException(
//                    nameof(graphicsDevice));
//        }

//        // =========================================================================
//        // Begin / End
//        // =========================================================================

//        public void Begin(
//            IShader shader,
//            ICamera camera)
//        {
//            CheckDisposed();

//            if (_begun)
//            {
//                throw new InvalidOperationException(
//                    "ShapeBatch.Begin already called.");
//            }

//            _shader =
//                shader ??
//                throw new ArgumentNullException(
//                    nameof(shader));

//            _camera =
//                camera ??
//                throw new ArgumentNullException(
//                    nameof(camera));

//            _vertices.Clear();
//            _indices.Clear();

//            _begun = true;
//        }

//        public void End()
//        {
//            CheckDisposed();

//            if (!_begun)
//            {
//                throw new InvalidOperationException(
//                    "ShapeBatch.Begin must be called first.");
//            }

//            try
//            {
//                Flush();
//            }
//            finally
//            {
//                _vertices.Clear();
//                _indices.Clear();

//                _shader = null;
//                _camera = null;

//                _begun = false;
//            }
//        }

//        // =========================================================================
//        // Fill Rectangle
//        // =========================================================================

//        public void AddFillRectangle(
//            Bounds bounds)
//        {
//            AddFillRectangle(
//                bounds,
//                Matrix.Identity);
//        }

//        public void AddFillRectangle(
//            Bounds bounds,
//            Matrix transform)
//        {
//            CheckBegin();

//            Vector2 p0 =
//                Vector2.Transform(
//                    new Vector2(
//                        bounds.Left,
//                        bounds.Top),
//                    transform);

//            Vector2 p1 =
//                Vector2.Transform(
//                    new Vector2(
//                        bounds.Right,
//                        bounds.Top),
//                    transform);

//            Vector2 p2 =
//                Vector2.Transform(
//                    new Vector2(
//                        bounds.Right,
//                        bounds.Bottom),
//                    transform);

//            Vector2 p3 =
//                Vector2.Transform(
//                    new Vector2(
//                        bounds.Left,
//                        bounds.Bottom),
//                    transform);

//            int start =
//                _vertices.Count;

//            AddVertex(
//                p0,
//                new Vector2(0f, 0f));

//            AddVertex(
//                p1,
//                new Vector2(1f, 0f));

//            AddVertex(
//                p2,
//                new Vector2(1f, 1f));

//            AddVertex(
//                p3,
//                new Vector2(0f, 1f));

//            AddQuadIndices(start);
//        }

//        // =========================================================================
//        // Stroke Rectangle
//        // =========================================================================

//        public void AddStrokeRectangle(
//            Bounds bounds,
//            float thickness)
//        {
//            AddStrokeRectangle(
//                bounds,
//                thickness,
//                ShapeLineJoin.Miter,
//                Matrix.Identity);
//        }

//        public void AddStrokeRectangle(
//            Bounds bounds,
//            float thickness,
//            ShapeLineJoin join)
//        {
//            AddStrokeRectangle(
//                bounds,
//                thickness,
//                join,
//                Matrix.Identity);
//        }

//        public void AddStrokeRectangle(
//            Bounds bounds,
//            float thickness,
//            ShapeLineJoin join,
//            Matrix transform)
//        {
//            CheckBegin();

//            if (thickness <= 0f)
//                return;

//            Vector2[] points =
//            {
//            new Vector2(
//                bounds.Left,
//                bounds.Top),

//            new Vector2(
//                bounds.Right,
//                bounds.Top),

//            new Vector2(
//                bounds.Right,
//                bounds.Bottom),

//            new Vector2(
//                bounds.Left,
//                bounds.Bottom)
//        };

//            AddClosedStroke(
//                points,
//                thickness,
//                join,
//                transform);
//        }

//        // =========================================================================
//        // Open Line
//        // =========================================================================

//        public void AddLine(
//            Vector2 start,
//            Vector2 end,
//            float thickness)
//        {
//            AddLine(
//                start,
//                end,
//                thickness,
//                ShapeLineCap.Butt,
//                Matrix.Identity);
//        }

//        public void AddLine(
//            Vector2 start,
//            Vector2 end,
//            float thickness,
//            ShapeLineCap cap)
//        {
//            AddLine(
//                start,
//                end,
//                thickness,
//                cap,
//                Matrix.Identity);
//        }

//        public void AddLine(
//            Vector2 start,
//            Vector2 end,
//            float thickness,
//            ShapeLineCap cap,
//            Matrix transform)
//        {
//            CheckBegin();

//            if (thickness <= 0f)
//                return;

//            Vector2[] points =
//            {
//            start,
//            end
//        };

//            AddOpenStroke(
//                points,
//                thickness,
//                ShapeLineJoin.Miter,
//                cap,
//                transform);
//        }

//        // =========================================================================
//        // Open Polyline
//        // =========================================================================

//        public void AddLine(
//            IReadOnlyList<Vector2> points,
//            float thickness)
//        {
//            AddLine(
//                points,
//                thickness,
//                ShapeLineJoin.Miter,
//                ShapeLineCap.Butt,
//                Matrix.Identity);
//        }

//        public void AddLine(
//            IReadOnlyList<Vector2> points,
//            float thickness,
//            ShapeLineJoin join)
//        {
//            AddLine(
//                points,
//                thickness,
//                join,
//                ShapeLineCap.Butt,
//                Matrix.Identity);
//        }

//        public void AddLine(
//            IReadOnlyList<Vector2> points,
//            float thickness,
//            ShapeLineJoin join,
//            ShapeLineCap cap)
//        {
//            AddLine(
//                points,
//                thickness,
//                join,
//                cap,
//                Matrix.Identity);
//        }

//        public void AddLine(
//            IReadOnlyList<Vector2> points,
//            float thickness,
//            ShapeLineJoin join,
//            ShapeLineCap cap,
//            Matrix transform)
//        {
//            CheckBegin();

//            if (points is null)
//            {
//                throw new ArgumentNullException(
//                    nameof(points));
//            }

//            if (points.Count < 2)
//                return;

//            if (thickness <= 0f)
//                return;

//            AddOpenStroke(
//                points,
//                thickness,
//                join,
//                cap,
//                transform);
//        }

//        // =========================================================================
//        // Closed Polygon Stroke
//        // =========================================================================

//        public void AddStrokePolygon(
//            IReadOnlyList<Vector2> points,
//            float thickness)
//        {
//            AddStrokePolygon(
//                points,
//                thickness,
//                ShapeLineJoin.Miter,
//                Matrix.Identity);
//        }

//        public void AddStrokePolygon(
//            IReadOnlyList<Vector2> points,
//            float thickness,
//            ShapeLineJoin join)
//        {
//            AddStrokePolygon(
//                points,
//                thickness,
//                join,
//                Matrix.Identity);
//        }

//        public void AddStrokePolygon(
//            IReadOnlyList<Vector2> points,
//            float thickness,
//            ShapeLineJoin join,
//            Matrix transform)
//        {
//            CheckBegin();

//            if (points is null)
//            {
//                throw new ArgumentNullException(
//                    nameof(points));
//            }

//            if (points.Count < 3)
//                return;

//            if (thickness <= 0f)
//                return;

//            AddClosedStroke(
//                points,
//                thickness,
//                join,
//                transform);
//        }

//        // =========================================================================
//        // Fill Polygon
//        // =========================================================================

//        public void AddFillPolygon(
//            IReadOnlyList<IReadOnlyList<Vector2>> polygon)
//        {
//            AddFillPolygon(
//                polygon,
//                Matrix.Identity);
//        }

//        public void AddFillPolygon(
//            IReadOnlyList<IReadOnlyList<Vector2>> polygon,
//            Matrix transform)
//        {
//            CheckBegin();

//            if (polygon is null)
//            {
//                throw new ArgumentNullException(
//                    nameof(polygon));
//            }

//            foreach (var points in polygon)
//            {
//                if (points is null)
//                {
//                    continue;
//                }

//                if (points.Count < 3)
//                {
//                    continue;
//                }

//                int offset =
//                    _vertices.Count;

//                foreach (Vector2 point in points)
//                {
//                    AddVertex(
//                        Vector2.Transform(
//                            point,
//                            transform),
//                        Vector2.Zero);
//                }

//                var triangles =
//                    PolygonTriangulator.Triangulate(
//                        points);

//                foreach (int index in triangles)
//                {
//                    _indices.Add(
//                        offset + index);
//                }
//            }
//        }

//        public void AddFillPolygon(
//            Path path)
//        {
//            AddFillPolygon(
//                path,
//                Matrix.Identity);
//        }

//        public void AddFillPolygon(
//            Path path,
//            Matrix transform)
//        {
//            CheckBegin();

//            if (path is null)
//            {
//                throw new ArgumentNullException(
//                    nameof(path));
//            }

//            for (int polygonIndex = 0;
//                 polygonIndex < path.GetPolygonCount();
//                 polygonIndex++)
//            {
//                var points =
//                    path.GetPolygonPoints(
//                        polygonIndex);

//                if (points.Count < 3)
//                    continue;

//                int offset =
//                    _vertices.Count;

//                foreach (Vector2 point in points)
//                {
//                    AddVertex(
//                        Vector2.Transform(
//                            point,
//                            transform),
//                        Vector2.Zero);
//                }

//                var triangles =
//                    PolygonTriangulator.Triangulate(
//                        points);

//                foreach (int index in triangles)
//                {
//                    _indices.Add(
//                        offset + index);
//                }
//            }
//        }

//        // =========================================================================
//        // Stroke Path
//        // =========================================================================

//        public void AddStrokePolygon(
//            Path path,
//            float thickness)
//        {
//            AddStrokePolygon(
//                path,
//                thickness,
//                ShapeLineJoin.Miter,
//                Matrix.Identity);
//        }

//        public void AddStrokePolygon(
//            Path path,
//            float thickness,
//            ShapeLineJoin join)
//        {
//            AddStrokePolygon(
//                path,
//                thickness,
//                join,
//                Matrix.Identity);
//        }

//        public void AddStrokePolygon(
//            Path path,
//            float thickness,
//            ShapeLineJoin join,
//            Matrix transform)
//        {
//            CheckBegin();

//            if (path is null)
//            {
//                throw new ArgumentNullException(
//                    nameof(path));
//            }

//            if (thickness <= 0f)
//                return;

//            for (int polygonIndex = 0;
//                 polygonIndex < path.GetPolygonCount();
//                 polygonIndex++)
//            {
//                var points =
//                    path.GetPolygonPoints(
//                        polygonIndex);

//                if (points.Count < 3)
//                    continue;

//                AddClosedStroke(
//                    points,
//                    thickness,
//                    join,
//                    transform);
//            }
//        }

//        // =========================================================================
//        // Fill Ellipse
//        // =========================================================================

//        public void AddFillEllipse(
//            Bounds bounds,
//            int segments = 32)
//        {
//            AddFillEllipse(
//                bounds,
//                Matrix.Identity,
//                segments);
//        }

//        public void AddFillEllipse(
//            Bounds bounds,
//            Matrix transform,
//            int segments = 32)
//        {
//            CheckBegin();

//            if (segments < 3)
//            {
//                throw new ArgumentOutOfRangeException(
//                    nameof(segments));
//            }

//            Vector2 center =
//                new Vector2(
//                    (bounds.Left + bounds.Right) * 0.5f,
//                    (bounds.Top + bounds.Bottom) * 0.5f);

//            Vector2 radius =
//                new Vector2(
//                    (bounds.Right - bounds.Left) * 0.5f,
//                    (bounds.Bottom - bounds.Top) * 0.5f);

//            AddFillEllipse(
//                center,
//                radius,
//                transform,
//                segments);
//        }

//        public void AddFillEllipse(
//            Vector2 center,
//            Vector2 radius,
//            int segments = 32)
//        {
//            AddFillEllipse(
//                center,
//                radius,
//                Matrix.Identity,
//                segments);
//        }

//        public void AddFillEllipse(
//            Vector2 center,
//            Vector2 radius,
//            Matrix transform,
//            int segments = 32)
//        {
//            CheckBegin();

//            if (segments < 3)
//            {
//                throw new ArgumentOutOfRangeException(
//                    nameof(segments));
//            }

//            int centerIndex =
//                _vertices.Count;

//            AddVertex(
//                Vector2.Transform(
//                    center,
//                    transform),
//                new Vector2(
//                    0.5f,
//                    0.5f));

//            int first =
//                _vertices.Count;

//            for (int i = 0;
//                 i < segments;
//                 i++)
//            {
//                float angle =
//                    MathF.Tau * i / segments;

//                float cos =
//                    MathF.Cos(angle);

//                float sin =
//                    MathF.Sin(angle);

//                Vector2 point =
//                    center +
//                    new Vector2(
//                        cos * radius.X,
//                        sin * radius.Y);

//                AddVertex(
//                    Vector2.Transform(
//                        point,
//                        transform),
//                    new Vector2(
//                        0.5f + cos * 0.5f,
//                        0.5f + sin * 0.5f));
//            }

//            for (int i = 0;
//                 i < segments;
//                 i++)
//            {
//                int current =
//                    first + i;

//                int next =
//                    first +
//                    ((i + 1) % segments);

//                _indices.Add(centerIndex);
//                _indices.Add(current);
//                _indices.Add(next);
//            }
//        }

//        // =========================================================================
//        // Stroke Ellipse
//        // =========================================================================

//        public void AddStrokeEllipse(
//            Bounds bounds,
//            float thickness,
//            int segments = 32)
//        {
//            AddStrokeEllipse(
//                bounds,
//                thickness,
//                ShapeLineJoin.Round,
//                Matrix.Identity,
//                segments);
//        }

//        public void AddStrokeEllipse(
//            Bounds bounds,
//            float thickness,
//            ShapeLineJoin join,
//            int segments = 32)
//        {
//            AddStrokeEllipse(
//                bounds,
//                thickness,
//                join,
//                Matrix.Identity,
//                segments);
//        }

//        public void AddStrokeEllipse(
//            Bounds bounds,
//            float thickness,
//            ShapeLineJoin join,
//            Matrix transform,
//            int segments = 32)
//        {
//            CheckBegin();

//            if (thickness <= 0f)
//                return;

//            if (segments < 3)
//            {
//                throw new ArgumentOutOfRangeException(
//                    nameof(segments));
//            }

//            Vector2 center =
//                new Vector2(
//                    (bounds.Left + bounds.Right) * 0.5f,
//                    (bounds.Top + bounds.Bottom) * 0.5f);

//            Vector2 radius =
//                new Vector2(
//                    (bounds.Right - bounds.Left) * 0.5f,
//                    (bounds.Bottom - bounds.Top) * 0.5f);

//            AddStrokeEllipse(
//                center,
//                radius,
//                thickness,
//                join,
//                transform,
//                segments);
//        }

//        public void AddStrokeEllipse(
//            Vector2 center,
//            Vector2 radius,
//            float thickness,
//            int segments = 32)
//        {
//            AddStrokeEllipse(
//                center,
//                radius,
//                thickness,
//                ShapeLineJoin.Round,
//                Matrix.Identity,
//                segments);
//        }

//        public void AddStrokeEllipse(
//            Vector2 center,
//            Vector2 radius,
//            float thickness,
//            ShapeLineJoin join,
//            int segments = 32)
//        {
//            AddStrokeEllipse(
//                center,
//                radius,
//                thickness,
//                join,
//                Matrix.Identity,
//                segments);
//        }

//        public void AddStrokeEllipse(
//            Vector2 center,
//            Vector2 radius,
//            float thickness,
//            ShapeLineJoin join,
//            Matrix transform,
//            int segments = 32)
//        {
//            CheckBegin();

//            if (thickness <= 0f)
//                return;

//            if (segments < 3)
//            {
//                throw new ArgumentOutOfRangeException(
//                    nameof(segments));
//            }

//            var points =
//                new Vector2[segments];

//            for (int i = 0;
//                 i < segments;
//                 i++)
//            {
//                float angle =
//                    MathF.Tau * i / segments;

//                points[i] =
//                    center +
//                    new Vector2(
//                        MathF.Cos(angle) * radius.X,
//                        MathF.Sin(angle) * radius.Y);
//            }

//            AddClosedStroke(
//                points,
//                thickness,
//                join,
//                transform);
//        }

//        // =========================================================================
//        // Open Stroke
//        // =========================================================================

//        private void AddOpenStroke(
//            IReadOnlyList<Vector2> points,
//            float thickness,
//            ShapeLineJoin join,
//            ShapeLineCap cap,
//            Matrix transform)
//        {
//            int count =
//                points.Count;

//            var transformed =
//                new Vector2[count];

//            for (int i = 0;
//                 i < count;
//                 i++)
//            {
//                transformed[i] =
//                    Vector2.Transform(
//                        points[i],
//                        transform);
//            }

//            float halfThickness =
//                thickness * 0.5f;

//            /*
//             * Segment geometry.
//             */
//            for (int i = 0;
//                 i < count - 1;
//                 i++)
//            {
//                Vector2 start =
//                    transformed[i];

//                Vector2 end =
//                    transformed[i + 1];

//                Vector2 direction =
//                    NormalizeSafe(
//                        end - start);

//                if (direction.LengthSquared() <= Epsilon)
//                    continue;

//                Vector2 normal =
//                    new Vector2(
//                        -direction.Y,
//                        direction.X);

//                int index =
//                    _vertices.Count;

//                AddVertex(
//                    start +
//                    normal * halfThickness,
//                    Vector2.Zero);

//                AddVertex(
//                    end +
//                    normal * halfThickness,
//                    Vector2.Zero);

//                AddVertex(
//                    end -
//                    normal * halfThickness,
//                    Vector2.Zero);

//                AddVertex(
//                    start -
//                    normal * halfThickness,
//                    Vector2.Zero);

//                AddQuadIndices(index);
//            }

//            /*
//             * Joins.
//             */
//            for (int i = 1;
//                 i < count - 1;
//                 i++)
//            {
//                AddJoin(
//                    transformed[i - 1],
//                    transformed[i],
//                    transformed[i + 1],
//                    halfThickness,
//                    join);
//            }

//            /*
//             * Start cap.
//             */
//            Vector2 firstDirection =
//                NormalizeSafe(
//                    transformed[1] -
//                    transformed[0]);

//            AddStartCap(
//                transformed[0],
//                firstDirection,
//                halfThickness,
//                cap);

//            /*
//             * End cap.
//             */
//            Vector2 lastDirection =
//                NormalizeSafe(
//                    transformed[count - 1] -
//                    transformed[count - 2]);

//            AddEndCap(
//                transformed[count - 1],
//                lastDirection,
//                halfThickness,
//                cap);
//        }

//        // =========================================================================
//        // Closed Stroke
//        // =========================================================================

//        private void AddClosedStroke(
//            IReadOnlyList<Vector2> points,
//            float thickness,
//            ShapeLineJoin join,
//            Matrix transform)
//        {
//            int count =
//                points.Count;

//            var transformed =
//                new Vector2[count];

//            for (int i = 0;
//                 i < count;
//                 i++)
//            {
//                transformed[i] =
//                    Vector2.Transform(
//                        points[i],
//                        transform);
//            }

//            float halfThickness =
//                thickness * 0.5f;

//            /*
//             * Segment geometry.
//             */
//            for (int i = 0;
//                 i < count;
//                 i++)
//            {
//                Vector2 start =
//                    transformed[i];

//                Vector2 end =
//                    transformed[
//                        (i + 1) % count];

//                Vector2 direction =
//                    NormalizeSafe(
//                        end - start);

//                if (direction.LengthSquared() <= Epsilon)
//                    continue;

//                Vector2 normal =
//                    new Vector2(
//                        -direction.Y,
//                        direction.X);

//                int index =
//                    _vertices.Count;

//                AddVertex(
//                    start +
//                    normal * halfThickness,
//                    Vector2.Zero);

//                AddVertex(
//                    end +
//                    normal * halfThickness,
//                    Vector2.Zero);

//                AddVertex(
//                    end -
//                    normal * halfThickness,
//                    Vector2.Zero);

//                AddVertex(
//                    start -
//                    normal * halfThickness,
//                    Vector2.Zero);

//                AddQuadIndices(index);
//            }

//            /*
//             * Joins at every corner.
//             */
//            for (int i = 0;
//                 i < count;
//                 i++)
//            {
//                Vector2 previous =
//                    transformed[
//                        (i - 1 + count) % count];

//                Vector2 current =
//                    transformed[i];

//                Vector2 next =
//                    transformed[
//                        (i + 1) % count];

//                AddJoin(
//                    previous,
//                    current,
//                    next,
//                    halfThickness,
//                    join);
//            }
//        }

//        // =========================================================================
//        // Join
//        // =========================================================================

//        private void AddJoin(
//            Vector2 previous,
//            Vector2 current,
//            Vector2 next,
//            float halfThickness,
//            ShapeLineJoin join)
//        {
//            Vector2 directionA =
//                NormalizeSafe(
//                    current - previous);

//            Vector2 directionB =
//                NormalizeSafe(
//                    next - current);

//            if (directionA.LengthSquared() <= Epsilon ||
//                directionB.LengthSquared() <= Epsilon)
//            {
//                return;
//            }

//            float cross =
//                directionA.X * directionB.Y -
//                directionA.Y * directionB.X;

//            /*
//             * Gerade Linie.
//             */
//            if (MathF.Abs(cross) <= Epsilon)
//                return;

//            Vector2 normalA =
//                new Vector2(
//                    -directionA.Y,
//                    directionA.X);

//            Vector2 normalB =
//                new Vector2(
//                    -directionB.Y,
//                    directionB.X);

//            /*
//             * Nur die Außenseite der Kurve braucht
//             * zusätzliche Join-Geometrie.
//             *
//             * Linksabbiegung:
//             * Außenseite = rechts.
//             *
//             * Rechtsabbiegung:
//             * Außenseite = links.
//             */
//            Vector2 outerNormalA =
//                cross > 0f
//                    ? -normalA
//                    : normalA;

//            Vector2 outerNormalB =
//                cross > 0f
//                    ? -normalB
//                    : normalB;

//            switch (join)
//            {
//                case ShapeLineJoin.Miter:

//                    AddMiterJoin(
//                        current,
//                        outerNormalA,
//                        outerNormalB,
//                        halfThickness);

//                    break;

//                case ShapeLineJoin.Bevel:

//                    AddBevelJoin(
//                        current,
//                        outerNormalA,
//                        outerNormalB,
//                        halfThickness);

//                    break;

//                case ShapeLineJoin.Round:

//                    AddRoundJoin(
//                        current,
//                        outerNormalA,
//                        outerNormalB,
//                        halfThickness);

//                    break;

//                default:

//                    throw new ArgumentOutOfRangeException(
//                        nameof(join));
//            }
//        }

//        // =========================================================================
//        // Miter Join
//        // =========================================================================

//        private void AddMiterJoin(
//            Vector2 position,
//            Vector2 normalA,
//            Vector2 normalB,
//            float halfThickness)
//        {
//            Vector2 miter =
//                normalA + normalB;

//            if (miter.LengthSquared() <= Epsilon)
//                return;

//            miter =
//                Vector2.Normalize(
//                    miter);

//            float denominator =
//                Vector2.Dot(
//                    miter,
//                    normalB);

//            if (MathF.Abs(denominator) <= Epsilon)
//            {
//                AddBevelJoin(
//                    position,
//                    normalA,
//                    normalB,
//                    halfThickness);

//                return;
//            }

//            float miterLength =
//                halfThickness /
//                denominator;

//            /*
//             * Miter limit.
//             */
//            if (MathF.Abs(miterLength) >
//                halfThickness * MiterLimit)
//            {
//                AddBevelJoin(
//                    position,
//                    normalA,
//                    normalB,
//                    halfThickness);

//                return;
//            }

//            Vector2 a =
//                position +
//                normalA * halfThickness;

//            Vector2 b =
//                position +
//                normalB * halfThickness;

//            Vector2 miterPoint =
//                position +
//                miter * miterLength;

//            int ia =
//                AddStrokeVertex(a);

//            int im =
//                AddStrokeVertex(miterPoint);

//            int ib =
//                AddStrokeVertex(b);

//            _indices.Add(ia);
//            _indices.Add(im);
//            _indices.Add(ib);
//        }

//        // =========================================================================
//        // Bevel Join
//        // =========================================================================

//        private void AddBevelJoin(
//            Vector2 position,
//            Vector2 normalA,
//            Vector2 normalB,
//            float halfThickness)
//        {
//            Vector2 a =
//                position +
//                normalA * halfThickness;

//            Vector2 b =
//                position +
//                normalB * halfThickness;

//            int center =
//                AddStrokeVertex(
//                    position);

//            int ia =
//                AddStrokeVertex(
//                    a);

//            int ib =
//                AddStrokeVertex(
//                    b);

//            _indices.Add(center);
//            _indices.Add(ia);
//            _indices.Add(ib);
//        }

//        // =========================================================================
//        // Round Join
//        // =========================================================================

//        private void AddRoundJoin(
//            Vector2 position,
//            Vector2 normalA,
//            Vector2 normalB,
//            float halfThickness)
//        {
//            float startAngle =
//                MathF.Atan2(
//                    normalA.Y,
//                    normalA.X);

//            float endAngle =
//                MathF.Atan2(
//                    normalB.Y,
//                    normalB.X);

//            float delta =
//                endAngle - startAngle;

//            while (delta <= -MathF.PI)
//                delta += MathF.Tau;

//            while (delta > MathF.PI)
//                delta -= MathF.Tau;

//            int segments =
//                Math.Max(
//                    1,
//                    (int)(
//                        MathF.Abs(delta) /
//                        (MathF.PI / 8f)));

//            int center =
//                AddStrokeVertex(
//                    position);

//            int previous =
//                AddStrokeVertex(
//                    position +
//                    new Vector2(
//                        MathF.Cos(startAngle),
//                        MathF.Sin(startAngle)) *
//                    halfThickness);

//            for (int i = 1;
//                 i <= segments;
//                 i++)
//            {
//                float t =
//                    i / (float)segments;

//                float angle =
//                    startAngle +
//                    delta * t;

//                Vector2 point =
//                    position +
//                    new Vector2(
//                        MathF.Cos(angle),
//                        MathF.Sin(angle)) *
//                    halfThickness;

//                int current =
//                    AddStrokeVertex(
//                        point);

//                _indices.Add(center);
//                _indices.Add(previous);
//                _indices.Add(current);

//                previous =
//                    current;
//            }
//        }

//        // =========================================================================
//        // Start Cap
//        // =========================================================================

//        private void AddStartCap(
//            Vector2 position,
//            Vector2 direction,
//            float halfThickness,
//            ShapeLineCap cap)
//        {
//            Vector2 normal =
//                new Vector2(
//                    -direction.Y,
//                    direction.X);

//            switch (cap)
//            {
//                case ShapeLineCap.Butt:
//                    return;

//                case ShapeLineCap.Square:
//                    {
//                        Vector2 extension =
//                            -direction *
//                            halfThickness;

//                        int index =
//                            _vertices.Count;

//                        AddVertex(
//                            position +
//                            normal * halfThickness +
//                            extension,
//                            Vector2.Zero);

//                        AddVertex(
//                            position +
//                            normal * halfThickness,
//                            Vector2.Zero);

//                        AddVertex(
//                            position -
//                            normal * halfThickness,
//                            Vector2.Zero);

//                        AddVertex(
//                            position -
//                            normal * halfThickness +
//                            extension,
//                            Vector2.Zero);

//                        AddQuadIndices(index);

//                        break;
//                    }

//                case ShapeLineCap.Round:

//                    AddRoundCap(
//                        position,
//                        -direction,
//                        halfThickness);

//                    break;

//                default:

//                    throw new ArgumentOutOfRangeException(
//                        nameof(cap));
//            }
//        }

//        // =========================================================================
//        // End Cap
//        // =========================================================================

//        private void AddEndCap(
//            Vector2 position,
//            Vector2 direction,
//            float halfThickness,
//            ShapeLineCap cap)
//        {
//            Vector2 normal =
//                new Vector2(
//                    -direction.Y,
//                    direction.X);

//            switch (cap)
//            {
//                case ShapeLineCap.Butt:
//                    return;

//                case ShapeLineCap.Square:
//                    {
//                        Vector2 extension =
//                            direction *
//                            halfThickness;

//                        int index =
//                            _vertices.Count;

//                        AddVertex(
//                            position +
//                            normal * halfThickness,
//                            Vector2.Zero);

//                        AddVertex(
//                            position +
//                            normal * halfThickness +
//                            extension,
//                            Vector2.Zero);

//                        AddVertex(
//                            position -
//                            normal * halfThickness +
//                            extension,
//                            Vector2.Zero);

//                        AddVertex(
//                            position -
//                            normal * halfThickness,
//                            Vector2.Zero);

//                        AddQuadIndices(index);

//                        break;
//                    }

//                case ShapeLineCap.Round:

//                    AddRoundCap(
//                        position,
//                        direction,
//                        halfThickness);

//                    break;

//                default:

//                    throw new ArgumentOutOfRangeException(
//                        nameof(cap));
//            }
//        }

//        // =========================================================================
//        // Round Cap
//        // =========================================================================

//        private void AddRoundCap(
//            Vector2 position,
//            Vector2 direction,
//            float radius)
//        {
//            float startAngle =
//                MathF.Atan2(
//                    direction.Y,
//                    direction.X) -
//                MathF.PI * 0.5f;

//            float endAngle =
//                startAngle +
//                MathF.PI;

//            const int segments = 12;

//            int center =
//                AddStrokeVertex(
//                    position);

//            int previous =
//                AddStrokeVertex(
//                    position +
//                    new Vector2(
//                        MathF.Cos(startAngle),
//                        MathF.Sin(startAngle)) *
//                    radius);

//            for (int i = 1;
//                 i <= segments;
//                 i++)
//            {
//                float t =
//                    i / (float)segments;

//                float angle =
//                    startAngle +
//                    MathF.PI * t;

//                Vector2 point =
//                    position +
//                    new Vector2(
//                        MathF.Cos(angle),
//                        MathF.Sin(angle)) *
//                    radius;

//                int current =
//                    AddStrokeVertex(
//                        point);

//                _indices.Add(center);
//                _indices.Add(previous);
//                _indices.Add(current);

//                previous =
//                    current;
//            }
//        }

//        // =========================================================================
//        // Vertex
//        // =========================================================================

//        private int AddStrokeVertex(
//            Vector2 position)
//        {
//            int index =
//                _vertices.Count;

//            AddVertex(
//                position,
//                Vector2.Zero);

//            return index;
//        }

//        private void AddVertex(
//            Vector2 position,
//            Vector2 textureCoordinate)
//        {
//            _vertices.Add(
//                new VertexPositionTexture(
//                    new Vector3(
//                        position,
//                        0f),
//                    textureCoordinate));
//        }

//        // =========================================================================
//        // Indices
//        // =========================================================================

//        private void AddQuadIndices(
//            int start)
//        {
//            _indices.Add(start + 0);
//            _indices.Add(start + 1);
//            _indices.Add(start + 3);

//            _indices.Add(start + 1);
//            _indices.Add(start + 2);
//            _indices.Add(start + 3);
//        }

//        // =========================================================================
//        // Flush
//        // =========================================================================

//        private void Flush()
//        {
//            if (_vertices.Count == 0 ||
//                _indices.Count == 0)
//            {
//                return;
//            }

//            if (_shader is null ||
//                _camera is null)
//            {
//                throw new InvalidOperationException(
//                    "ShapeBatch is not initialized.");
//            }

//            EnsureBuffers();

//            _vertexBuffer!.SetData(
//                _vertices.ToArray(),
//                0,
//                _vertices.Count);

//            _indexBuffer!.SetData(
//                _indices.ToArray(),
//                0,
//                _indices.Count);

//            _graphicsDevice.SetVertexBuffer(
//                _vertexBuffer);

//            _graphicsDevice.Indices =
//                _indexBuffer;

//            if (_shader is IShaderTransform transform)
//            {
//                transform.Camera =
//                    _camera;

//                transform.Transform =
//                    Matrix.Identity;
//            }

//            _shader.Apply();

//            foreach (EffectPass pass
//                     in _shader.Effect
//                         .Techniques[0]
//                         .Passes)
//            {
//                pass.Apply();

//                _graphicsDevice.DrawIndexedPrimitives(
//                    PrimitiveType.TriangleList,
//                    0,
//                    0,
//                    _indices.Count / 3);
//            }
//        }

//        // =========================================================================
//        // Buffers
//        // =========================================================================

//        private void EnsureBuffers()
//        {
//            if (_vertexBuffer is null ||
//                _vertexBuffer.VertexCount < _vertices.Count)
//            {
//                _vertexBuffer?.Dispose();

//                _vertexBuffer =
//                    new DynamicVertexBuffer(
//                        _graphicsDevice,
//                        VertexPositionTexture.VertexDeclaration,
//                        Math.Max(
//                            _vertices.Count,
//                            256),
//                        BufferUsage.WriteOnly);
//            }

//            if (_indexBuffer is null ||
//                _indexBuffer.IndexCount < _indices.Count)
//            {
//                _indexBuffer?.Dispose();

//                _indexBuffer =
//                    new IndexBuffer(
//                        _graphicsDevice,
//                        IndexElementSize.ThirtyTwoBits,
//                        Math.Max(
//                            _indices.Count,
//                            256),
//                        BufferUsage.WriteOnly);
//            }
//        }

//        // =========================================================================
//        // Helpers
//        // =========================================================================

//        private static Vector2 NormalizeSafe(
//            Vector2 value)
//        {
//            float lengthSquared =
//                value.LengthSquared();

//            if (lengthSquared <= Epsilon)
//                return Vector2.Zero;

//            return value /
//                MathF.Sqrt(lengthSquared);
//        }

//        private void CheckBegin()
//        {
//            CheckDisposed();

//            if (!_begun)
//            {
//                throw new InvalidOperationException(
//                    "ShapeBatch.Begin must be called first.");
//            }
//        }

//        private void CheckDisposed()
//        {
//            if (_disposed)
//            {
//                throw new ObjectDisposedException(
//                    nameof(ShapeBatch));
//            }
//        }

//        // =========================================================================
//        // Dispose
//        // =========================================================================

//        public void Dispose()
//        {
//            if (_disposed)
//                return;

//            _vertexBuffer?.Dispose();
//            _indexBuffer?.Dispose();

//            _vertexBuffer = null;
//            _indexBuffer = null;

//            _disposed = true;
//        }
//    }
//}