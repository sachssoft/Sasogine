using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Geometry;
using Sachssoft.Sasogine.Graphics.Cameras;
using Sachssoft.Sasogine.Graphics.Rendering;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Graphics.Rendering.Batches
{
    public sealed class ShapeBatch : IDisposable
    {
        private const float Epsilon = 0.000001f;
        private const float MiterLimit = 4f;
        private const int InitialVertexCapacity = 256;
        private const int InitialIndexCapacity = 512;
        private const int RoundSegments = 12;

        private readonly GraphicsDevice _graphicsDevice;

        private readonly List<VertexPositionTexture> _vertices;
        private readonly List<int> _indices;

        private VertexPositionTexture[] _vertexUpload;
        private int[] _indexUpload;

        private DynamicVertexBuffer? _vertexBuffer;
        private IndexBuffer? _indexBuffer;

        private IShader? _shader;
        private ICamera? _camera;

        private bool _begun;
        private bool _disposed;

        public ShapeBatch(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice =
                graphicsDevice ??
                throw new ArgumentNullException(nameof(graphicsDevice));

            _vertices =
                new List<VertexPositionTexture>(
                    InitialVertexCapacity);

            _indices =
                new List<int>(
                    InitialIndexCapacity);

            _vertexUpload =
                new VertexPositionTexture[
                    InitialVertexCapacity];

            _indexUpload =
                new int[
                    InitialIndexCapacity];
        }

        // =========================================================================
        // Begin / End
        // =========================================================================

        public void Begin(
            IShader shader,
            ICamera camera)
        {
            CheckDisposed();

            if (_begun)
                throw new InvalidOperationException(
                    "ShapeBatch.Begin already called.");

            _shader =
                shader ??
                throw new ArgumentNullException(nameof(shader));

            _camera =
                camera ??
                throw new ArgumentNullException(nameof(camera));

            _vertices.Clear();
            _indices.Clear();

            _begun = true;
        }

        public void End()
        {
            CheckDisposed();

            if (!_begun)
                throw new InvalidOperationException(
                    "ShapeBatch.Begin must be called first.");

            try
            {
                Flush();
            }
            finally
            {
                _vertices.Clear();
                _indices.Clear();

                _shader = null;
                _camera = null;

                _begun = false;
            }
        }

        // =========================================================================
        // Fill Rectangle
        // =========================================================================

        public void AddFillRectangle(
            Bounds2 bounds)
        {
            AddFillRectangle(
                bounds,
                Matrix.Identity);
        }

        public void AddFillRectangle(
            Bounds2 bounds,
            Matrix transform)
        {
            CheckBegin();

            int start =
                _vertices.Count;

            AddVertex(
                Vector2.Transform(
                    new Vector2(
                        bounds.Left,
                        bounds.Top),
                    transform),
                new Vector2(0f, 0f));

            AddVertex(
                Vector2.Transform(
                    new Vector2(
                        bounds.Right,
                        bounds.Top),
                    transform),
                new Vector2(1f, 0f));

            AddVertex(
                Vector2.Transform(
                    new Vector2(
                        bounds.Right,
                        bounds.Bottom),
                    transform),
                new Vector2(1f, 1f));

            AddVertex(
                Vector2.Transform(
                    new Vector2(
                        bounds.Left,
                        bounds.Bottom),
                    transform),
                new Vector2(0f, 1f));

            AddQuadIndices(start);
        }

        // =========================================================================
        // Stroke Rectangle
        // =========================================================================

        public void AddStrokeRectangle(
            Bounds2 bounds,
            float thickness)
        {
            AddStrokeRectangle(
                bounds,
                thickness,
                LineJoin.Miter,
                Matrix.Identity);
        }

        public void AddStrokeRectangle(
            Bounds2 bounds,
            float thickness,
            LineJoin join)
        {
            AddStrokeRectangle(
                bounds,
                thickness,
                join,
                Matrix.Identity);
        }

        public void AddStrokeRectangle(
            Bounds2 bounds,
            float thickness,
            LineJoin join,
            Matrix transform)
        {
            CheckBegin();

            if (thickness <= 0f)
                return;

            Vector2[] points =
            {
                new Vector2(
                    bounds.Left,
                    bounds.Top),

                new Vector2(
                    bounds.Right,
                    bounds.Top),

                new Vector2(
                    bounds.Right,
                    bounds.Bottom),

                new Vector2(
                    bounds.Left,
                    bounds.Bottom)
            };

            AddClosedStroke(
                points,
                thickness,
                join,
                transform);
        }

        // =========================================================================
        // Open Line
        // =========================================================================

        public void AddLine(
            Vector2 start,
            Vector2 end,
            float thickness)
        {
            AddLine(
                start,
                end,
                thickness,
                LineCap.Butt,
                Matrix.Identity);
        }

        public void AddLine(
            Vector2 start,
            Vector2 end,
            float thickness,
            LineCap cap)
        {
            AddLine(
                start,
                end,
                thickness,
                cap,
                Matrix.Identity);
        }

        public void AddLine(
            Vector2 start,
            Vector2 end,
            float thickness,
            LineCap cap,
            Matrix transform)
        {
            CheckBegin();

            if (thickness <= 0f)
                return;

            Vector2 a =
                Vector2.Transform(
                    start,
                    transform);

            Vector2 b =
                Vector2.Transform(
                    end,
                    transform);

            AddLineSegment(
                a,
                b,
                thickness,
                cap);
        }

        // =========================================================================
        // Open Polyline
        // =========================================================================

        public void AddLine(
            IReadOnlyList<Vector2> points,
            float thickness)
        {
            AddLine(
                points,
                thickness,
                LineJoin.Miter,
                LineCap.Butt,
                Matrix.Identity);
        }

        public void AddLine(
            IReadOnlyList<Vector2> points,
            float thickness,
            LineJoin join)
        {
            AddLine(
                points,
                thickness,
                join,
                LineCap.Butt,
                Matrix.Identity);
        }

        public void AddLine(
            IReadOnlyList<Vector2> points,
            float thickness,
            LineJoin join,
            LineCap cap)
        {
            AddLine(
                points,
                thickness,
                join,
                cap,
                Matrix.Identity);
        }

        public void AddLine(
            IReadOnlyList<Vector2> points,
            float thickness,
            LineJoin join,
            LineCap cap,
            Matrix transform)
        {
            CheckBegin();

            if (points is null)
                throw new ArgumentNullException(nameof(points));

            if (points.Count < 2 || thickness <= 0f)
                return;

            if (points.Count == 2)
            {
                AddLine(
                    points[0],
                    points[1],
                    thickness,
                    cap,
                    transform);

                return;
            }

            AddOpenStroke(
                points,
                thickness,
                join,
                cap,
                transform);
        }

        // =========================================================================
        // Closed Polygon Stroke
        // =========================================================================

        public void AddStrokePolygon(
            IReadOnlyList<Vector2> points,
            float thickness)
        {
            AddStrokePolygon(
                points,
                thickness,
                LineJoin.Miter,
                Matrix.Identity);
        }

        public void AddStrokePolygon(
            IReadOnlyList<Vector2> points,
            float thickness,
            LineJoin join)
        {
            AddStrokePolygon(
                points,
                thickness,
                join,
                Matrix.Identity);
        }

        public void AddStrokePolygon(
            IReadOnlyList<Vector2> points,
            float thickness,
            LineJoin join,
            Matrix transform)
        {
            CheckBegin();

            if (points is null)
                throw new ArgumentNullException(nameof(points));

            if (points.Count < 3 || thickness <= 0f)
                return;

            AddClosedStroke(
                points,
                thickness,
                join,
                transform);
        }

        // =========================================================================
        // Fill Polygon
        // =========================================================================

        public void AddFillPolygon(
            IReadOnlyList<IReadOnlyList<Vector2>> polygon)
        {
            AddFillPolygon(
                polygon,
                Matrix.Identity);
        }

        public void AddFillPolygon(
            IReadOnlyList<IReadOnlyList<Vector2>> polygon,
            Matrix transform)
        {
            CheckBegin();

            if (polygon is null)
                throw new ArgumentNullException(nameof(polygon));

            if (polygon.Count == 0)
                return;

            var transformed =
                new List<IReadOnlyList<Vector2>>(
                    polygon.Count);

            for (int i = 0;
                 i < polygon.Count;
                 i++)
            {
                IReadOnlyList<Vector2>? contour =
                    polygon[i];

                if (contour is null ||
                    contour.Count < 3)
                {
                    continue;
                }

                transformed.Add(
                    TransformPoints(
                        contour,
                        transform));
            }

            if (transformed.Count == 0)
                return;

            var result =
                PolygonOperations.Triangulate(
                    transformed,
                    new PolygonTriangulationOptions());

            if (result.Vertices.Count == 0 ||
                result.Indices.Count == 0)
            {
                return;
            }

            int offset =
                _vertices.Count;

            for (int i = 0;
                 i < result.Vertices.Count;
                 i++)
            {
                AddVertex(
                    result.Vertices[i],
                    Vector2.Zero);
            }

            for (int i = 0;
                 i < result.Indices.Count;
                 i++)
            {
                _indices.Add(
                    offset +
                    result.Indices[i]);
            }
        }

        public void AddFillPolygon(
            Path path)
        {
            AddFillPolygon(
                path,
                Matrix.Identity);
        }

        public void AddFillPolygon(
            Path path,
            Matrix transform)
        {
            CheckBegin();

            if (path is null)
                throw new ArgumentNullException(nameof(path));

            int polygonCount =
                path.GetPolygonCount();

            if (polygonCount == 0)
                return;

            var contours =
                new List<IReadOnlyList<Vector2>>(
                    polygonCount);

            for (int i = 0;
                 i < polygonCount;
                 i++)
            {
                IReadOnlyList<Vector2> points =
                    path.GetPolygonPoints(i);

                if (points.Count >= 3)
                    contours.Add(points);
            }

            if (contours.Count == 0)
                return;

            AddFillPolygon(
                contours,
                transform);
        }

        // =========================================================================
        // Stroke Path
        // =========================================================================

        public void AddStrokePolygon(
            Path path,
            float thickness)
        {
            AddStrokePolygon(
                path,
                thickness,
                LineJoin.Miter,
                Matrix.Identity);
        }

        public void AddStrokePolygon(
            Path path,
            float thickness,
            LineJoin join)
        {
            AddStrokePolygon(
                path,
                thickness,
                join,
                Matrix.Identity);
        }

        public void AddStrokePolygon(
            Path path,
            float thickness,
            LineJoin join,
            Matrix transform)
        {
            CheckBegin();

            if (path is null)
                throw new ArgumentNullException(nameof(path));

            if (thickness <= 0f)
                return;

            int polygonCount =
                path.GetPolygonCount();

            for (int i = 0;
                 i < polygonCount;
                 i++)
            {
                IReadOnlyList<Vector2> points =
                    path.GetPolygonPoints(i);

                if (points.Count < 3)
                    continue;

                AddClosedStroke(
                    points,
                    thickness,
                    join,
                    transform);
            }
        }

        // =========================================================================
        // Fill Ellipse
        // =========================================================================

        public void AddFillEllipse(
            Bounds2 bounds,
            int segments = 32)
        {
            AddFillEllipse(
                bounds,
                Matrix.Identity,
                segments);
        }

        public void AddFillEllipse(
            Bounds2 bounds,
            Matrix transform,
            int segments = 32)
        {
            CheckBegin();

            ValidateSegments(segments);

            Vector2 center =
                new Vector2(
                    (bounds.Left + bounds.Right) * 0.5f,
                    (bounds.Top + bounds.Bottom) * 0.5f);

            Vector2 radius =
                new Vector2(
                    (bounds.Right - bounds.Left) * 0.5f,
                    (bounds.Bottom - bounds.Top) * 0.5f);

            AddFillEllipse(
                center,
                radius,
                transform,
                segments);
        }

        public void AddFillEllipse(
            Vector2 center,
            Vector2 radius,
            int segments = 32)
        {
            AddFillEllipse(
                center,
                radius,
                Matrix.Identity,
                segments);
        }

        public void AddFillEllipse(
            Vector2 center,
            Vector2 radius,
            Matrix transform,
            int segments = 32)
        {
            CheckBegin();

            ValidateSegments(segments);

            Vector2 transformedCenter =
                Vector2.Transform(
                    center,
                    transform);

            int centerIndex =
                _vertices.Count;

            AddVertex(
                transformedCenter,
                new Vector2(
                    0.5f,
                    0.5f));

            int first =
                _vertices.Count;

            float step =
                MathF.Tau / segments;

            for (int i = 0;
                 i < segments;
                 i++)
            {
                float angle =
                    step * i;

                float cos =
                    MathF.Cos(angle);

                float sin =
                    MathF.Sin(angle);

                Vector2 point =
                    center +
                    new Vector2(
                        cos * radius.X,
                        sin * radius.Y);

                AddVertex(
                    Vector2.Transform(
                        point,
                        transform),
                    new Vector2(
                        0.5f + cos * 0.5f,
                        0.5f + sin * 0.5f));
            }

            for (int i = 0;
                 i < segments;
                 i++)
            {
                int current =
                    first + i;

                int next =
                    first +
                    ((i + 1) % segments);

                _indices.Add(centerIndex);
                _indices.Add(current);
                _indices.Add(next);
            }
        }

        // =========================================================================
        // Stroke Ellipse
        // =========================================================================

        public void AddStrokeEllipse(
            Bounds2 bounds,
            float thickness,
            int segments = 32)
        {
            AddStrokeEllipse(
                bounds,
                thickness,
                LineJoin.Round,
                Matrix.Identity,
                segments);
        }

        public void AddStrokeEllipse(
            Bounds2 bounds,
            float thickness,
            LineJoin join,
            int segments = 32)
        {
            AddStrokeEllipse(
                bounds,
                thickness,
                join,
                Matrix.Identity,
                segments);
        }

        public void AddStrokeEllipse(
            Bounds2 bounds,
            float thickness,
            LineJoin join,
            Matrix transform,
            int segments = 32)
        {
            CheckBegin();

            if (thickness <= 0f)
                return;

            ValidateSegments(segments);

            Vector2 center =
                new Vector2(
                    (bounds.Left + bounds.Right) * 0.5f,
                    (bounds.Top + bounds.Bottom) * 0.5f);

            Vector2 radius =
                new Vector2(
                    (bounds.Right - bounds.Left) * 0.5f,
                    (bounds.Bottom - bounds.Top) * 0.5f);

            AddStrokeEllipse(
                center,
                radius,
                thickness,
                join,
                transform,
                segments);
        }

        public void AddStrokeEllipse(
            Vector2 center,
            Vector2 radius,
            float thickness,
            int segments = 32)
        {
            AddStrokeEllipse(
                center,
                radius,
                thickness,
                LineJoin.Round,
                Matrix.Identity,
                segments);
        }

        public void AddStrokeEllipse(
            Vector2 center,
            Vector2 radius,
            float thickness,
            LineJoin join,
            int segments = 32)
        {
            AddStrokeEllipse(
                center,
                radius,
                thickness,
                join,
                Matrix.Identity,
                segments);
        }

        public void AddStrokeEllipse(
            Vector2 center,
            Vector2 radius,
            float thickness,
            LineJoin join,
            Matrix transform,
            int segments = 32)
        {
            CheckBegin();

            if (thickness <= 0f)
                return;

            ValidateSegments(segments);

            AddEllipseStroke(
                center,
                radius,
                thickness,
                transform,
                segments);
        }

        // =========================================================================
        // Fast Line Segment
        // =========================================================================

        private void AddLineSegment(
            Vector2 start,
            Vector2 end,
            float thickness,
            LineCap cap)
        {
            Vector2 direction =
                end - start;

            float lengthSquared =
                direction.LengthSquared();

            if (lengthSquared <= Epsilon)
                return;

            direction /=
                MathF.Sqrt(lengthSquared);

            Vector2 normal =
                new Vector2(
                    -direction.Y,
                    direction.X);

            float halfThickness =
                thickness * 0.5f;

            int index =
                _vertices.Count;

            AddVertex(
                start +
                normal * halfThickness,
                Vector2.Zero);

            AddVertex(
                end +
                normal * halfThickness,
                Vector2.Zero);

            AddVertex(
                end -
                normal * halfThickness,
                Vector2.Zero);

            AddVertex(
                start -
                normal * halfThickness,
                Vector2.Zero);

            AddQuadIndices(index);

            if (cap == LineCap.Square)
            {
                AddSquareStartCap(
                    start,
                    direction,
                    normal,
                    halfThickness);

                AddSquareEndCap(
                    end,
                    direction,
                    normal,
                    halfThickness);
            }
            else if (cap == LineCap.Round)
            {
                AddRoundCap(
                    start,
                    -direction,
                    halfThickness);

                AddRoundCap(
                    end,
                    direction,
                    halfThickness);
            }
        }

        // =========================================================================
        // Open Stroke
        // =========================================================================

        private void AddOpenStroke(
            IReadOnlyList<Vector2> points,
            float thickness,
            LineJoin join,
            LineCap cap,
            Matrix transform)
        {
            int count =
                points.Count;

            Vector2[] transformed =
                new Vector2[count];

            for (int i = 0;
                 i < count;
                 i++)
            {
                transformed[i] =
                    Vector2.Transform(
                        points[i],
                        transform);
            }

            float halfThickness =
                thickness * 0.5f;

            // Segment bodies.
            for (int i = 0;
                 i < count - 1;
                 i++)
            {
                Vector2 start =
                    transformed[i];

                Vector2 end =
                    transformed[i + 1];

                AddStrokeSegment(
                    start,
                    end,
                    halfThickness);
            }

            // Joins.
            for (int i = 1;
                 i < count - 1;
                 i++)
            {
                AddJoin(
                    transformed[i - 1],
                    transformed[i],
                    transformed[i + 1],
                    halfThickness,
                    join);
            }

            int firstSegment =
                FindFirstValidSegment(
                    transformed);

            int lastSegment =
                FindLastValidSegment(
                    transformed);

            if (firstSegment < 0 ||
                lastSegment < 0)
            {
                return;
            }

            Vector2 firstDirection =
                NormalizeSafe(
                    transformed[firstSegment + 1] -
                    transformed[firstSegment]);

            Vector2 lastDirection =
                NormalizeSafe(
                    transformed[lastSegment + 1] -
                    transformed[lastSegment]);

            AddStartCap(
                transformed[firstSegment],
                firstDirection,
                halfThickness,
                cap);

            AddEndCap(
                transformed[lastSegment + 1],
                lastDirection,
                halfThickness,
                cap);
        }

        private void AddStrokeSegment(
            Vector2 start,
            Vector2 end,
            float halfThickness)
        {
            Vector2 direction =
                end - start;

            float lengthSquared =
                direction.LengthSquared();

            if (lengthSquared <= Epsilon)
                return;

            direction /=
                MathF.Sqrt(lengthSquared);

            Vector2 normal =
                new Vector2(
                    -direction.Y,
                    direction.X);

            int index =
                _vertices.Count;

            AddVertex(
                start +
                normal * halfThickness,
                Vector2.Zero);

            AddVertex(
                end +
                normal * halfThickness,
                Vector2.Zero);

            AddVertex(
                end -
                normal * halfThickness,
                Vector2.Zero);

            AddVertex(
                start -
                normal * halfThickness,
                Vector2.Zero);

            AddQuadIndices(index);
        }

        // =========================================================================
        // Closed Stroke
        // =========================================================================

        private void AddClosedStroke(
            IReadOnlyList<Vector2> points,
            float thickness,
            LineJoin join,
            Matrix transform)
        {
            int count =
                points.Count;

            Vector2[] transformed =
                new Vector2[count];

            for (int i = 0;
                 i < count;
                 i++)
            {
                transformed[i] =
                    Vector2.Transform(
                        points[i],
                        transform);
            }

            float halfThickness =
                thickness * 0.5f;

            // Segment bodies.
            for (int i = 0;
                 i < count;
                 i++)
            {
                Vector2 start =
                    transformed[i];

                Vector2 end =
                    transformed[
                        (i + 1) % count];

                AddStrokeSegment(
                    start,
                    end,
                    halfThickness);
            }

            // Joins.
            for (int i = 0;
                 i < count;
                 i++)
            {
                Vector2 previous =
                    transformed[
                        (i - 1 + count) % count];

                Vector2 current =
                    transformed[i];

                Vector2 next =
                    transformed[
                        (i + 1) % count];

                AddJoin(
                    previous,
                    current,
                    next,
                    halfThickness,
                    join);
            }
        }

        // =========================================================================
        // Join
        // =========================================================================

        private void AddJoin(
            Vector2 previous,
            Vector2 current,
            Vector2 next,
            float halfThickness,
            LineJoin join)
        {
            Vector2 directionA =
                NormalizeSafe(
                    current - previous);

            Vector2 directionB =
                NormalizeSafe(
                    next - current);

            if (directionA.LengthSquared() <= Epsilon ||
                directionB.LengthSquared() <= Epsilon)
            {
                return;
            }

            float cross =
                directionA.X * directionB.Y -
                directionA.Y * directionB.X;

            if (MathF.Abs(cross) <= Epsilon)
                return;

            Vector2 normalA =
                new Vector2(
                    -directionA.Y,
                    directionA.X);

            Vector2 normalB =
                new Vector2(
                    -directionB.Y,
                    directionB.X);

            Vector2 outerNormalA =
                cross > 0f
                    ? -normalA
                    : normalA;

            Vector2 outerNormalB =
                cross > 0f
                    ? -normalB
                    : normalB;

            switch (join)
            {
                case LineJoin.Miter:
                    AddMiterJoin(
                        current,
                        outerNormalA,
                        outerNormalB,
                        halfThickness);
                    break;

                case LineJoin.Bevel:
                    AddBevelJoin(
                        current,
                        outerNormalA,
                        outerNormalB,
                        halfThickness);
                    break;

                case LineJoin.Round:
                    AddRoundJoin(
                        current,
                        outerNormalA,
                        outerNormalB,
                        halfThickness);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(join));
            }
        }

        // =========================================================================
        // Miter Join
        // =========================================================================

        private void AddMiterJoin(
            Vector2 position,
            Vector2 normalA,
            Vector2 normalB,
            float halfThickness)
        {
            Vector2 miter =
                normalA + normalB;

            float lengthSquared =
                miter.LengthSquared();

            if (lengthSquared <= Epsilon)
                return;

            miter /=
                MathF.Sqrt(lengthSquared);

            float denominator =
                Vector2.Dot(
                    miter,
                    normalB);

            if (MathF.Abs(denominator) <= Epsilon)
            {
                AddBevelJoin(
                    position,
                    normalA,
                    normalB,
                    halfThickness);

                return;
            }

            float miterLength =
                halfThickness /
                denominator;

            if (MathF.Abs(miterLength) >
                halfThickness * MiterLimit)
            {
                AddBevelJoin(
                    position,
                    normalA,
                    normalB,
                    halfThickness);

                return;
            }

            Vector2 a =
                position +
                normalA * halfThickness;

            Vector2 b =
                position +
                normalB * halfThickness;

            Vector2 miterPoint =
                position +
                miter * miterLength;

            int ia =
                AddStrokeVertex(a);

            int im =
                AddStrokeVertex(miterPoint);

            int ib =
                AddStrokeVertex(b);

            _indices.Add(ia);
            _indices.Add(im);
            _indices.Add(ib);
        }

        // =========================================================================
        // Bevel Join
        // =========================================================================

        private void AddBevelJoin(
            Vector2 position,
            Vector2 normalA,
            Vector2 normalB,
            float halfThickness)
        {
            Vector2 a =
                position +
                normalA * halfThickness;

            Vector2 b =
                position +
                normalB * halfThickness;

            int center =
                AddStrokeVertex(position);

            int ia =
                AddStrokeVertex(a);

            int ib =
                AddStrokeVertex(b);

            _indices.Add(center);
            _indices.Add(ia);
            _indices.Add(ib);
        }

        // =========================================================================
        // Round Join
        // =========================================================================

        private void AddRoundJoin(
            Vector2 position,
            Vector2 normalA,
            Vector2 normalB,
            float halfThickness)
        {
            float startAngle =
                MathF.Atan2(
                    normalA.Y,
                    normalA.X);

            float endAngle =
                MathF.Atan2(
                    normalB.Y,
                    normalB.X);

            float delta =
                endAngle - startAngle;

            while (delta <= -MathF.PI)
                delta += MathF.Tau;

            while (delta > MathF.PI)
                delta -= MathF.Tau;

            int segments =
                Math.Max(
                    1,
                    (int)(
                        MathF.Abs(delta) /
                        (MathF.PI / 8f)));

            int center =
                AddStrokeVertex(position);

            int previous =
                AddStrokeVertex(
                    position +
                    new Vector2(
                        MathF.Cos(startAngle),
                        MathF.Sin(startAngle)) *
                    halfThickness);

            for (int i = 1;
                 i <= segments;
                 i++)
            {
                float t =
                    i / (float)segments;

                float angle =
                    startAngle +
                    delta * t;

                Vector2 point =
                    position +
                    new Vector2(
                        MathF.Cos(angle),
                        MathF.Sin(angle)) *
                    halfThickness;

                int current =
                    AddStrokeVertex(point);

                _indices.Add(center);
                _indices.Add(previous);
                _indices.Add(current);

                previous = current;
            }
        }

        // =========================================================================
        // Caps
        // =========================================================================

        private void AddStartCap(
            Vector2 position,
            Vector2 direction,
            float halfThickness,
            LineCap cap)
        {
            if (direction.LengthSquared() <= Epsilon)
                return;

            Vector2 normal =
                new Vector2(
                    -direction.Y,
                    direction.X);

            switch (cap)
            {
                case LineCap.Butt:
                    return;

                case LineCap.Square:
                    AddSquareStartCap(
                        position,
                        direction,
                        normal,
                        halfThickness);
                    break;

                case LineCap.Round:
                    AddRoundCap(
                        position,
                        -direction,
                        halfThickness);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(cap));
            }
        }

        private void AddEndCap(
            Vector2 position,
            Vector2 direction,
            float halfThickness,
            LineCap cap)
        {
            if (direction.LengthSquared() <= Epsilon)
                return;

            Vector2 normal =
                new Vector2(
                    -direction.Y,
                    direction.X);

            switch (cap)
            {
                case LineCap.Butt:
                    return;

                case LineCap.Square:
                    AddSquareEndCap(
                        position,
                        direction,
                        normal,
                        halfThickness);
                    break;

                case LineCap.Round:
                    AddRoundCap(
                        position,
                        direction,
                        halfThickness);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(cap));
            }
        }

        private void AddSquareStartCap(
            Vector2 position,
            Vector2 direction,
            Vector2 normal,
            float halfThickness)
        {
            Vector2 extension =
                -direction * halfThickness;

            int index =
                _vertices.Count;

            AddVertex(
                position +
                normal * halfThickness +
                extension,
                Vector2.Zero);

            AddVertex(
                position +
                normal * halfThickness,
                Vector2.Zero);

            AddVertex(
                position -
                normal * halfThickness,
                Vector2.Zero);

            AddVertex(
                position -
                normal * halfThickness +
                extension,
                Vector2.Zero);

            AddQuadIndices(index);
        }

        private void AddSquareEndCap(
            Vector2 position,
            Vector2 direction,
            Vector2 normal,
            float halfThickness)
        {
            Vector2 extension =
                direction * halfThickness;

            int index =
                _vertices.Count;

            AddVertex(
                position +
                normal * halfThickness,
                Vector2.Zero);

            AddVertex(
                position +
                normal * halfThickness +
                extension,
                Vector2.Zero);

            AddVertex(
                position -
                normal * halfThickness +
                extension,
                Vector2.Zero);

            AddVertex(
                position -
                normal * halfThickness,
                Vector2.Zero);

            AddQuadIndices(index);
        }

        private void AddRoundCap(
            Vector2 position,
            Vector2 direction,
            float radius)
        {
            float startAngle =
                MathF.Atan2(
                    direction.Y,
                    direction.X) -
                MathF.PI * 0.5f;

            int center =
                AddStrokeVertex(position);

            int previous =
                AddStrokeVertex(
                    position +
                    new Vector2(
                        MathF.Cos(startAngle),
                        MathF.Sin(startAngle)) *
                    radius);

            for (int i = 1;
                 i <= RoundSegments;
                 i++)
            {
                float angle =
                    startAngle +
                    MathF.PI *
                    i / RoundSegments;

                Vector2 point =
                    position +
                    new Vector2(
                        MathF.Cos(angle),
                        MathF.Sin(angle)) *
                    radius;

                int current =
                    AddStrokeVertex(point);

                _indices.Add(center);
                _indices.Add(previous);
                _indices.Add(current);

                previous = current;
            }
        }

        // =========================================================================
        // Fast Ellipse Stroke
        // =========================================================================

        private void AddEllipseStroke(
            Vector2 center,
            Vector2 radius,
            float thickness,
            Matrix transform,
            int segments)
        {
            float halfThickness =
                thickness * 0.5f;

            Vector2 innerRadius =
                new Vector2(
                    MathF.Max(
                        0f,
                        MathF.Abs(radius.X) -
                        halfThickness),

                    MathF.Max(
                        0f,
                        MathF.Abs(radius.Y) -
                        halfThickness));

            Vector2 outerRadius =
                new Vector2(
                    MathF.Abs(radius.X) +
                    halfThickness,

                    MathF.Abs(radius.Y) +
                    halfThickness);

            int start =
                _vertices.Count;

            float step =
                MathF.Tau / segments;

            for (int i = 0;
                 i < segments;
                 i++)
            {
                float angle =
                    step * i;

                float cos =
                    MathF.Cos(angle);

                float sin =
                    MathF.Sin(angle);

                Vector2 outer =
                    center +
                    new Vector2(
                        cos * outerRadius.X,
                        sin * outerRadius.Y);

                Vector2 inner =
                    center +
                    new Vector2(
                        cos * innerRadius.X,
                        sin * innerRadius.Y);

                AddVertex(
                    Vector2.Transform(
                        outer,
                        transform),
                    Vector2.Zero);

                AddVertex(
                    Vector2.Transform(
                        inner,
                        transform),
                    Vector2.Zero);
            }

            for (int i = 0;
                 i < segments;
                 i++)
            {
                int current =
                    start + i * 2;

                int next =
                    start +
                    ((i + 1) % segments) * 2;

                _indices.Add(current);
                _indices.Add(next);
                _indices.Add(current + 1);

                _indices.Add(next);
                _indices.Add(next + 1);
                _indices.Add(current + 1);
            }
        }

        // =========================================================================
        // Helpers
        // =========================================================================

        private static int FindFirstValidSegment(
            IReadOnlyList<Vector2> points)
        {
            for (int i = 0;
                 i < points.Count - 1;
                 i++)
            {
                if ((points[i + 1] - points[i])
                    .LengthSquared() > Epsilon)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindLastValidSegment(
            IReadOnlyList<Vector2> points)
        {
            for (int i = points.Count - 2;
                 i >= 0;
                 i--)
            {
                if ((points[i + 1] - points[i])
                    .LengthSquared() > Epsilon)
                {
                    return i;
                }
            }

            return -1;
        }

        private static IReadOnlyList<Vector2> TransformPoints(
            IReadOnlyList<Vector2> points,
            Matrix transform)
        {
            var result =
                new Vector2[points.Count];

            for (int i = 0;
                 i < points.Count;
                 i++)
            {
                result[i] =
                    Vector2.Transform(
                        points[i],
                        transform);
            }

            return result;
        }

        private static Vector2 NormalizeSafe(
            Vector2 value)
        {
            float lengthSquared =
                value.LengthSquared();

            if (lengthSquared <= Epsilon)
                return Vector2.Zero;

            return value /
                   MathF.Sqrt(lengthSquared);
        }

        private static void ValidateSegments(
            int segments)
        {
            if (segments < 3)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(segments),
                    "Ellipse must contain at least 3 segments.");
            }
        }

        // =========================================================================
        // Vertex / Index
        // =========================================================================

        private int AddStrokeVertex(
            Vector2 position)
        {
            int index =
                _vertices.Count;

            AddVertex(
                position,
                Vector2.Zero);

            return index;
        }

        private void AddVertex(
            Vector2 position,
            Vector2 textureCoordinate)
        {
            _vertices.Add(
                new VertexPositionTexture(
                    new Vector3(
                        position,
                        0f),
                    textureCoordinate));
        }

        private void AddQuadIndices(
            int start)
        {
            _indices.Add(start);
            _indices.Add(start + 1);
            _indices.Add(start + 3);

            _indices.Add(start + 1);
            _indices.Add(start + 2);
            _indices.Add(start + 3);
        }

        // =========================================================================
        // Flush
        // =========================================================================

        private void Flush()
        {
            int vertexCount =
                _vertices.Count;

            int indexCount =
                _indices.Count;

            if (vertexCount == 0 ||
                indexCount == 0)
            {
                return;
            }

            if (_shader is null ||
                _camera is null)
            {
                throw new InvalidOperationException(
                    "ShapeBatch is not initialized.");
            }

            EnsureUploadArrays(
                vertexCount,
                indexCount);

            _vertices.CopyTo(
                0,
                _vertexUpload,
                0,
                vertexCount);

            _indices.CopyTo(
                0,
                _indexUpload,
                0,
                indexCount);

            EnsureBuffers(
                vertexCount,
                indexCount);

            _vertexBuffer!.SetData(
                _vertexUpload,
                0,
                vertexCount);

            _indexBuffer!.SetData(
                _indexUpload,
                0,
                indexCount);

            _graphicsDevice.SetVertexBuffer(
                _vertexBuffer);

            _graphicsDevice.Indices =
                _indexBuffer;

            if (_shader is IShaderTransform transform)
            {
                transform.Camera =
                    _camera;

                transform.Transform =
                    Matrix.Identity;
            }

            _shader.Apply();

            foreach (EffectPass pass in
                     _shader.Effect
                         .Techniques[0]
                         .Passes)
            {
                pass.Apply();

                _graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    0,
                    indexCount / 3);
            }
        }

        // =========================================================================
        // Upload Arrays
        // =========================================================================

        private void EnsureUploadArrays(
            int vertexCount,
            int indexCount)
        {
            if (_vertexUpload.Length < vertexCount)
            {
                _vertexUpload =
                    new VertexPositionTexture[
                        GrowCapacity(
                            _vertexUpload.Length,
                            vertexCount)];
            }

            if (_indexUpload.Length < indexCount)
            {
                _indexUpload =
                    new int[
                        GrowCapacity(
                            _indexUpload.Length,
                            indexCount)];
            }
        }

        private static int GrowCapacity(
            int current,
            int required)
        {
            int capacity =
                current <= 0
                    ? 256
                    : current;

            while (capacity < required)
            {
                int next =
                    capacity * 2;

                if (next <= capacity)
                    return required;

                capacity = next;
            }

            return capacity;
        }

        // =========================================================================
        // Buffers
        // =========================================================================

        private void EnsureBuffers(
            int vertexCount,
            int indexCount)
        {
            if (_vertexBuffer is null ||
                _vertexBuffer.VertexCount < vertexCount)
            {
                _vertexBuffer?.Dispose();

                _vertexBuffer =
                    new DynamicVertexBuffer(
                        _graphicsDevice,
                        VertexPositionTexture.VertexDeclaration,
                        GrowCapacity(
                            _vertexBuffer?.VertexCount ??
                            InitialVertexCapacity,
                            vertexCount),
                        BufferUsage.WriteOnly);
            }

            if (_indexBuffer is null ||
                _indexBuffer.IndexCount < indexCount)
            {
                _indexBuffer?.Dispose();

                _indexBuffer =
                    new IndexBuffer(
                        _graphicsDevice,
                        IndexElementSize.ThirtyTwoBits,
                        GrowCapacity(
                            _indexBuffer?.IndexCount ??
                            InitialIndexCapacity,
                            indexCount),
                        BufferUsage.WriteOnly);
            }
        }

        // =========================================================================
        // State
        // =========================================================================

        private void CheckBegin()
        {
            CheckDisposed();

            if (!_begun)
            {
                throw new InvalidOperationException(
                    "ShapeBatch.Begin must be called first.");
            }
        }

        private void CheckDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    nameof(ShapeBatch));
            }
        }

        // =========================================================================
        // Dispose
        // =========================================================================

        public void Dispose()
        {
            if (_disposed)
                return;

            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();

            _vertexBuffer = null;
            _indexBuffer = null;

            _vertices.Clear();
            _indices.Clear();

            _shader = null;
            _camera = null;

            _disposed = true;
        }
    }
}