using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Engine;
using Sachssoft.Engine.Geometry;
using Sachssoft.Engine.Graphics.Brushes;
using Sachssoft.Engine.Graphics.Cameras;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Graphics.Rendering.Batches;

/// <summary>
/// Batches and renders two-dimensional filled and stroked shape geometry.
/// </summary>
/// <remarks>
/// Public geometry APIs use Sasogine point and bounds types, while the
/// internal rendering implementation uses MonoGame vector and GPU types.
/// Coordinates, transforms, and stroke widths must be finite. Complex strokes
/// are unioned and triangulated to avoid internal alpha overdraw; their transformed
/// coordinates and widths must not exceed 1e12 in magnitude at six decimal digits
/// of clipping precision. Ordinary untransformed miter rectangle strokes use a direct ring mesh.
/// </remarks>
public sealed class ShapeBatch : IDisposable
{
    private const float Epsilon = 0.000001f;
    private const int InitialVertexCapacity = 256;
    private const int InitialIndexCapacity = 512;
    private const int RoundSegments = 12;

    /// <summary>Reuses transformed centerline storage between stroke calls.</summary>
    private readonly List<List<Vector2>> _strokeBuffers = new();

    /// <summary>Reuses the contour collection submitted for one unioned stroke mesh.</summary>
    private readonly List<IReadOnlyList<Vector2>> _strokeContours = new();

    private readonly GraphicsDevice _graphicsDevice;

    private readonly List<ShapeVertex> _vertices;
    private readonly List<Brush> _vertexBrushes;
    private readonly List<int> _indices;
    private readonly List<BrushCommand> _brushCommands;
    private readonly Texture2D _whiteTexture;

    private ShapeVertex[] _vertexUpload;
    private int[] _indexUpload;

    private DynamicVertexBuffer? _vertexBuffer;
    private IndexBuffer? _indexBuffer;

    private IShader? _shader;
    private ICamera? _camera;

    private Brush _brush = new SolidColorBrush(Color.White);
    private bool _deferBrushApplication;
    private bool _begun;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShapeBatch"/> class.
    /// </summary>
    /// <param name="graphicsDevice">
    /// The graphics device used to create and render GPU resources.
    /// </param>
    public ShapeBatch(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice =
            graphicsDevice ??
            throw new ArgumentNullException(nameof(graphicsDevice));

        _vertices =
            new List<ShapeVertex>(
                InitialVertexCapacity);

        _vertexBrushes =
            new List<Brush>(
                InitialVertexCapacity);

        _indices =
            new List<int>(
                InitialIndexCapacity);

        _brushCommands =
            new List<BrushCommand>();

        _whiteTexture =
            new Texture2D(
                _graphicsDevice,
                1,
                1);

        _whiteTexture.SetData(
            new[] { Color.White });

        _vertexUpload =
            new ShapeVertex[
                InitialVertexCapacity];

        _indexUpload =
            new int[
                InitialIndexCapacity];
    }

    // =========================================================================
    // Begin / End
    // =========================================================================

    /// <summary>
    /// Begins collecting shapes for rendering.
    /// </summary>
    /// <param name="shader">
    /// The shader used to render the accumulated geometry.
    /// </param>
    /// <param name="camera">
    /// The camera used to render the accumulated geometry.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the batch has already been started.
    /// </exception>
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
        _vertexBrushes.Clear();
        _indices.Clear();
        _brushCommands.Clear();

        _begun = true;
    }

    /// <summary>
    /// Ends the batch and renders all accumulated geometry.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the batch has not been started.
    /// </exception>
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
            _vertexBrushes.Clear();
            _indices.Clear();
            _brushCommands.Clear();

            _shader = null;
            _camera = null;

            _begun = false;
        }
    }

    // =========================================================================
    // Fill Rectangle
    // =========================================================================

    /// <summary>
    /// Adds a filled rectangle to the batch.
    /// </summary>
    /// <param name="bounds">
    /// The bounds of the shape.
    /// </param>
    public void AddFillRectangle(
        Bounds2 bounds)
    {
        AddFillRectangle(
            bounds,
            Matrix.Identity);
    }

    /// <summary>
    /// Adds a filled rectangle to the batch.
    /// </summary>
    /// <param name="bounds">
    /// The bounds of the shape.
    /// </param>
    /// <param name="transform">
    /// The transformation applied to the geometry.
    /// </param>
    public void AddFillRectangle(
        Bounds2 bounds,
        Matrix transform)
    {
        CheckBegin();
        ValidateShapeTransform(transform);
        ShapeStrokeGeometry.ValidateBounds(bounds);

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

    /// <summary>
    /// Adds the outline of a rectangle to the batch.
    /// </summary>
    /// <param name="bounds">
    /// The bounds of the shape.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
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

    /// <summary>
    /// Adds the outline of a rectangle to the batch.
    /// </summary>
    /// <param name="bounds">
    /// The bounds of the shape.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="join">
    /// The line join style used between connected segments.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
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

    /// <summary>
    /// Adds the outline of a rectangle to the batch.
    /// </summary>
    /// <param name="bounds">
    /// The bounds of the shape.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="join">
    /// The line join style used between connected segments.
    /// </param>
    /// <param name="transform">
    /// The transformation applied to the geometry.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddStrokeRectangle(
        Bounds2 bounds,
        float thickness,
        LineJoin join,
        Matrix transform)
    {
        CheckBegin();
        ValidateStrokeSettings(thickness, join, LineCap.Butt, transform);
        ShapeStrokeGeometry.ValidateBounds(bounds);
        if (thickness <= 0 || bounds.Width == 0 || bounds.Height == 0) return;
        if (join == LineJoin.Miter && transform == Matrix.Identity)
        {
            Span<Vector2> vertices = stackalloc Vector2[8];
            Span<int> indices = stackalloc int[24];
            int count = ShapeStrokeGeometry.RectangleRing(bounds, thickness, vertices, indices, out int vertexCount);
            AppendMesh(vertices[..vertexCount], indices[..count]);
            return;
        }
        AddStrokeRectangle(bounds, thickness, CornerType.None, 0, join, transform);
    }

    // =========================================================================
    // Open Line
    // =========================================================================

    /// <summary>
    /// Adds a line or polyline to the batch.
    /// </summary>
    /// <param name="start">
    /// The starting point of the line.
    /// </param>
    /// <param name="end">
    /// The ending point of the line.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddLine(
        Point2 start,
        Point2 end,
        float thickness)
    {
        AddLine(
            start,
            end,
            thickness,
            LineCap.Butt,
            Matrix.Identity);
    }

    /// <summary>
    /// Adds a line or polyline to the batch.
    /// </summary>
    /// <param name="start">
    /// The starting point of the line.
    /// </param>
    /// <param name="end">
    /// The ending point of the line.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="cap">
    /// The line cap style used at open line endpoints.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddLine(
        Point2 start,
        Point2 end,
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

    /// <summary>
    /// Adds a line or polyline to the batch.
    /// </summary>
    /// <param name="start">
    /// The starting point of the line.
    /// </param>
    /// <param name="end">
    /// The ending point of the line.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="cap">
    /// The line cap style used at open line endpoints.
    /// </param>
    /// <param name="transform">
    /// The transformation applied to the geometry.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddLine(
        Point2 start,
        Point2 end,
        float thickness,
        LineCap cap,
        Matrix transform)
    {
        CheckBegin();
        ValidateStrokeSettings(thickness, LineJoin.Miter, cap, transform);
        ShapeStrokeGeometry.ValidatePoint(new Vector2(start.X, start.Y));
        ShapeStrokeGeometry.ValidatePoint(new Vector2(end.X, end.Y));


        if (thickness <= 0f)
            return;

        Vector2 a =
            Vector2.Transform(
                new Vector2(start.X, start.Y),
                transform);

        Vector2 b =
            Vector2.Transform(
                new Vector2(end.X, end.Y),
                transform);

        ShapeStrokeGeometry.ValidatePoint(a);
        ShapeStrokeGeometry.ValidatePoint(b);
        AddLineSegment(
            a,
            b,
            thickness,
            cap);
    }

    // =========================================================================
    // Open Polyline
    // =========================================================================

    /// <summary>
    /// Adds a line or polyline to the batch.
    /// </summary>
    /// <param name="points">
    /// The points that define the line or polygon.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddLine(
        IReadOnlyList<Point2> points,
        float thickness)
    {
        AddLine(
            points,
            thickness,
            LineJoin.Miter,
            LineCap.Butt,
            Matrix.Identity);
    }

    /// <summary>
    /// Adds a line or polyline to the batch.
    /// </summary>
    /// <param name="points">
    /// The points that define the line or polygon.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="join">
    /// The line join style used between connected segments.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddLine(
        IReadOnlyList<Point2> points,
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

    /// <summary>
    /// Adds a line or polyline to the batch.
    /// </summary>
    /// <param name="points">
    /// The points that define the line or polygon.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="join">
    /// The line join style used between connected segments.
    /// </param>
    /// <param name="cap">
    /// The line cap style used at open line endpoints.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddLine(
        IReadOnlyList<Point2> points,
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

    /// <summary>
    /// Adds a line or polyline to the batch.
    /// </summary>
    /// <param name="points">
    /// The points that define the line or polygon.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="join">
    /// The line join style used between connected segments.
    /// </param>
    /// <param name="cap">
    /// The line cap style used at open line endpoints.
    /// </param>
    /// <param name="transform">
    /// The transformation applied to the geometry.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddLine(
        IReadOnlyList<Point2> points,
        float thickness,
        LineJoin join,
        LineCap cap,
        Matrix transform)
    {
        CheckBegin();
        ValidateShapeTransform(transform);
        ShapeStrokeGeometry.ValidateThickness(thickness);
        if (join is not (LineJoin.Miter or LineJoin.Bevel or LineJoin.Round)) throw new ArgumentOutOfRangeException(nameof(join));
        if (cap is not (LineCap.Butt or LineCap.Square or LineCap.Round)) throw new ArgumentOutOfRangeException(nameof(cap));

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

    /// <summary>
    /// Adds the outline of a polygon to the batch.
    /// </summary>
    /// <param name="points">
    /// The points that define the line or polygon.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddStrokePolygon(
        IReadOnlyList<Point2> points,
        float thickness)
    {
        AddStrokePolygon(
            points,
            thickness,
            LineJoin.Miter,
            Matrix.Identity);
    }

    /// <summary>
    /// Adds the outline of a polygon to the batch.
    /// </summary>
    /// <param name="points">
    /// The points that define the line or polygon.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="join">
    /// The line join style used between connected segments.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddStrokePolygon(
        IReadOnlyList<Point2> points,
        float thickness,
        LineJoin join)
    {
        AddStrokePolygon(
            points,
            thickness,
            join,
            Matrix.Identity);
    }

    /// <summary>
    /// Adds the outline of a polygon to the batch.
    /// </summary>
    /// <param name="points">
    /// The points that define the line or polygon.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="join">
    /// The line join style used between connected segments.
    /// </param>
    /// <param name="transform">
    /// The transformation applied to the geometry.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddStrokePolygon(
        IReadOnlyList<Point2> points,
        float thickness,
        LineJoin join,
        Matrix transform)
    {
        CheckBegin();
        ValidateShapeTransform(transform);
        ShapeStrokeGeometry.ValidateThickness(thickness);
        if (join is not (LineJoin.Miter or LineJoin.Bevel or LineJoin.Round)) throw new ArgumentOutOfRangeException(nameof(join));

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

    /// <summary>
    /// Adds a filled polygon to the batch.
    /// </summary>
    /// <param name="polygon">
    /// The polygon contours to render.
    /// </param>
    public void AddFillPolygon(
        IReadOnlyList<IReadOnlyList<Point2>> polygon)
    {
        AddFillPolygon(
            polygon,
            Matrix.Identity);
    }

    /// <summary>
    /// Adds a filled polygon to the batch.
    /// </summary>
    /// <param name="polygon">
    /// The polygon contours to render.
    /// </param>
    /// <param name="transform">
    /// The transformation applied to the geometry.
    /// </param>
    public void AddFillPolygon(
        IReadOnlyList<IReadOnlyList<Point2>> polygon,
        Matrix transform)
    {
        CheckBegin();
        ValidateShapeTransform(transform);

        ArgumentNullException.ThrowIfNull(polygon);

        if (polygon.Count == 0)
            return;

        var transformed = new List<IReadOnlyList<Vector2>>(polygon.Count);

        for (int i = 0; i < polygon.Count; i++)
        {
            IReadOnlyList<Point2>? contour = polygon[i];

            if (contour is null || contour.Count < 3)
                continue;

            var points = new Vector2[contour.Count];

            for (int j = 0; j < contour.Count; j++)
            {
                points[j] = Vector2.Transform(
                    contour[j],
                    transform);
                ShapeStrokeGeometry.ValidatePoint(points[j]);
            }

            transformed.Add(points);
        }

        if (transformed.Count == 0)
            return;

        var result = PolygonOperations.Triangulate(
            transformed,
            new PolygonTriangulationOptions());

        if (result.Vertices.Count == 0 ||
            result.Indices.Count == 0)
        {
            return;
        }

        int offset = _vertices.Count;

        for (int i = 0; i < result.Vertices.Count; i++)
        {
            AddVertex(
                result.Vertices[i],
                Vector2.Zero);
        }

        for (int i = 0; i < result.Indices.Count; i++)
        {
            _indices.Add(
                offset + result.Indices[i]);
        }
    }

    /// <summary>
    /// Adds a filled polygon to the batch.
    /// </summary>
    /// <param name="path">
    /// The path containing polygon geometry to render.
    /// </param>
    public void AddFillPolygon(
        Path path)
    {
        AddFillPolygon(
            path,
            Matrix.Identity);
    }

    /// <summary>
    /// Adds a filled polygon to the batch.
    /// </summary>
    /// <param name="path">
    /// The path containing polygon geometry to render.
    /// </param>
    /// <param name="transform">
    /// The transformation applied to the geometry.
    /// </param>
    public void AddFillPolygon(
        Path path,
        Matrix transform)
    {
        CheckBegin();
        ValidateShapeTransform(transform);

        if (path is null)
            throw new ArgumentNullException(nameof(path));

        int polygonCount =
            path.GetPolygonCount();

        if (polygonCount == 0)
            return;

        var contours = new List<IReadOnlyList<Point2>>(polygonCount);

        for (int i = 0;
             i < polygonCount;
             i++)
        {
            var points = path.GetPolygonPoints(i);

            if (points.Length >= 3)
                contours.Add(points);
        }

        if (contours.Count == 0)
            return;

        AddFillPolygonVectors(
            contours,
            transform);
    }

    private void AddFillPolygonVectors(
        IReadOnlyList<IReadOnlyList<Point2>> polygon,
        Matrix transform)
    {
        if (polygon.Count == 0)
            return;

        var transformed = new List<IReadOnlyList<Vector2>>(polygon.Count);

        for (int i = 0; i < polygon.Count; i++)
        {
            IReadOnlyList<Point2>? contour = polygon[i];

            if (contour is null || contour.Count < 3)
                continue;

            var points = new Vector2[contour.Count];

            for (int j = 0; j < contour.Count; j++)
            {
                points[j] = Vector2.Transform(
                    contour[j],
                    transform);
                ShapeStrokeGeometry.ValidatePoint(points[j]);
            }

            transformed.Add(points);
        }

        if (transformed.Count == 0)
            return;

        var result = PolygonOperations.Triangulate(
            transformed,
            new PolygonTriangulationOptions());

        if (result.Vertices.Count == 0 ||
            result.Indices.Count == 0)
        {
            return;
        }

        int offset = _vertices.Count;

        for (int i = 0; i < result.Vertices.Count; i++)
        {
            AddVertex(
                result.Vertices[i],
                Vector2.Zero);
        }

        for (int i = 0; i < result.Indices.Count; i++)
        {
            _indices.Add(
                offset + result.Indices[i]);
        }
    }

    // =========================================================================
    // Stroke Path
    // =========================================================================

    /// <summary>
    /// Adds the outline of a polygon to the batch.
    /// </summary>
    /// <param name="path">
    /// The path containing polygon geometry to render.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
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

    /// <summary>
    /// Adds the outline of a polygon to the batch.
    /// </summary>
    /// <param name="path">
    /// The path containing polygon geometry to render.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="join">
    /// The line join style used between connected segments.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
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

    /// <summary>
    /// Adds the outline of a polygon to the batch.
    /// </summary>
    /// <param name="path">
    /// The path containing polygon geometry to render.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="join">
    /// The line join style used between connected segments.
    /// </param>
    /// <param name="transform">
    /// The transformation applied to the geometry.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddStrokePolygon(
        Path path,
        float thickness,
        LineJoin join,
        Matrix transform)
    {
        CheckBegin();
        ArgumentNullException.ThrowIfNull(path);
        ValidateStrokeSettings(thickness, join, LineCap.Butt, transform);
        if (thickness <= 0) return;
        _strokeContours.Clear();
        for (int i = 0; i < path.GetPolygonCount(); i++)
        {
            var points = path.GetPolygonPoints(i);
            if (points.Length >= 3) _strokeContours.Add(PrepareStrokeContour(points, transform, i));
        }
        AppendStrokeMesh(ShapeStrokeGeometry.Stroke(_strokeContours, thickness, join, LineCap.Butt, true));
    }

    // =========================================================================
    // Fill Ellipse
    // =========================================================================

    /// <summary>
    /// Adds a filled ellipse to the batch.
    /// </summary>
    /// <param name="bounds">
    /// The bounds of the shape.
    /// </param>
    /// <param name="segments">
    /// The number of segments used to approximate the ellipse.
    /// </param>
    public void AddFillEllipse(
        Bounds2 bounds,
        int segments = 32)
    {
        AddFillEllipse(
            bounds,
            Matrix.Identity,
            segments);
    }

    /// <summary>
    /// Adds a filled ellipse to the batch.
    /// </summary>
    /// <param name="bounds">
    /// The bounds of the shape.
    /// </param>
    /// <param name="transform">
    /// The transformation applied to the geometry.
    /// </param>
    /// <param name="segments">
    /// The number of segments used to approximate the ellipse.
    /// </param>
    public void AddFillEllipse(
        Bounds2 bounds,
        Matrix transform,
        int segments = 32)
    {
        CheckBegin();
        ValidateShapeTransform(transform);
        ShapeStrokeGeometry.ValidateBounds(bounds);

        ValidateSegments(segments);

        Point2 center =
            new Point2(
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

    /// <summary>
    /// Adds a filled ellipse to the batch.
    /// </summary>
    /// <param name="center">
    /// The center point of the ellipse.
    /// </param>
    /// <param name="radius">
    /// The horizontal and vertical radii of the ellipse.
    /// </param>
    /// <param name="segments">
    /// The number of segments used to approximate the ellipse.
    /// </param>
    public void AddFillEllipse(
        Point2 center,
        Vector2 radius,
        int segments = 32)
    {
        AddFillEllipse(
            center,
            radius,
            Matrix.Identity,
            segments);
    }

    /// <summary>
    /// Adds a filled ellipse to the batch.
    /// </summary>
    /// <param name="center">
    /// The center point of the ellipse.
    /// </param>
    /// <param name="radius">
    /// The horizontal and vertical radii of the ellipse.
    /// </param>
    /// <param name="transform">
    /// The transformation applied to the geometry.
    /// </param>
    /// <param name="segments">
    /// The number of segments used to approximate the ellipse.
    /// </param>
    public void AddFillEllipse(
        Point2 center,
        Vector2 radius,
        Matrix transform,
        int segments = 32)
    {
        CheckBegin();
        ValidateShapeTransform(transform);
        ShapeStrokeGeometry.ValidatePoint(new Vector2(center.X, center.Y));
        ShapeStrokeGeometry.ValidatePoint(radius);

        ValidateSegments(segments);

        Vector2 centerVector =
            new Vector2(
                center.X,
                center.Y);

        Vector2 transformedCenter =
            Vector2.Transform(
                centerVector,
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
                centerVector +
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

    /// <summary>
    /// Adds the outline of an ellipse to the batch.
    /// </summary>
    /// <param name="bounds">
    /// The bounds of the shape.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="segments">
    /// The number of segments used to approximate the ellipse.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
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

    /// <summary>
    /// Adds the outline of an ellipse to the batch.
    /// </summary>
    /// <param name="bounds">
    /// The bounds of the shape.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="join">
    /// The line join style used between connected segments.
    /// </param>
    /// <param name="segments">
    /// The number of segments used to approximate the ellipse.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
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

    /// <summary>
    /// Adds the outline of an ellipse to the batch.
    /// </summary>
    /// <param name="bounds">
    /// The bounds of the shape.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="join">
    /// The line join style used between connected segments.
    /// </param>
    /// <param name="transform">
    /// The transformation applied to the geometry.
    /// </param>
    /// <param name="segments">
    /// The number of segments used to approximate the ellipse.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddStrokeEllipse(
        Bounds2 bounds,
        float thickness,
        LineJoin join,
        Matrix transform,
        int segments = 32)
    {
        CheckBegin();
        ValidateShapeTransform(transform);
        ShapeStrokeGeometry.ValidateBounds(bounds);
        ShapeStrokeGeometry.ValidateThickness(thickness);
        if (join is not (LineJoin.Miter or LineJoin.Bevel or LineJoin.Round)) throw new ArgumentOutOfRangeException(nameof(join));

        if (thickness <= 0f)
            return;

        ValidateSegments(segments);

        Point2 center =
            new Point2(
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

    /// <summary>
    /// Adds the outline of an ellipse to the batch.
    /// </summary>
    /// <param name="center">
    /// The center point of the ellipse.
    /// </param>
    /// <param name="radius">
    /// The horizontal and vertical radii of the ellipse.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="segments">
    /// The number of segments used to approximate the ellipse.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddStrokeEllipse(
        Point2 center,
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

    /// <summary>
    /// Adds the outline of an ellipse to the batch.
    /// </summary>
    /// <param name="center">
    /// The center point of the ellipse.
    /// </param>
    /// <param name="radius">
    /// The horizontal and vertical radii of the ellipse.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="join">
    /// The line join style used between connected segments.
    /// </param>
    /// <param name="segments">
    /// The number of segments used to approximate the ellipse.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddStrokeEllipse(
        Point2 center,
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

    /// <summary>
    /// Adds the outline of an ellipse to the batch.
    /// </summary>
    /// <param name="center">
    /// The center point of the ellipse.
    /// </param>
    /// <param name="radius">
    /// The horizontal and vertical radii of the ellipse.
    /// </param>
    /// <param name="thickness">
    /// The stroke thickness.
    /// </param>
    /// <param name="join">
    /// The line join style used between connected segments.
    /// </param>
    /// <param name="transform">
    /// The transformation applied to the geometry.
    /// </param>
    /// <param name="segments">
    /// The number of segments used to approximate the ellipse.
    /// </param>
    /// <remarks>
    /// Non-positive thickness values do not add geometry.
    /// </remarks>
    public void AddStrokeEllipse(
        Point2 center,
        Vector2 radius,
        float thickness,
        LineJoin join,
        Matrix transform,
        int segments = 32)
    {
        CheckBegin();
        ValidateShapeTransform(transform);
        ShapeStrokeGeometry.ValidateThickness(thickness);
        ShapeStrokeGeometry.ValidatePoint(new Vector2(center.X, center.Y));
        ShapeStrokeGeometry.ValidatePoint(radius);
        if (join is not (LineJoin.Miter or LineJoin.Bevel or LineJoin.Round)) throw new ArgumentOutOfRangeException(nameof(join));

        if (thickness <= 0f)
            return;

        ValidateSegments(segments);

        AddEllipseStroke(
            new Vector2(center.X, center.Y),
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
        IReadOnlyList<Point2> points,
        float thickness,
        LineJoin join,
        LineCap cap,
        Matrix transform)
    {
        ValidateStrokeSettings(thickness, join, cap, transform);
        _strokeContours.Clear();
        _strokeContours.Add(PrepareStrokeContour(points, transform, 0));
        AppendStrokeMesh(ShapeStrokeGeometry.Stroke(_strokeContours, thickness, join, cap, false));
    }

    // =========================================================================
    // Closed Stroke
    // =========================================================================

    private void AddClosedStroke(
        IReadOnlyList<Point2> points,
        float thickness,
        LineJoin join,
        Matrix transform)
    {
        ValidateStrokeSettings(thickness, join, LineCap.Butt, transform);
        _strokeContours.Clear();
        _strokeContours.Add(PrepareStrokeContour(points, transform, 0));
        AppendStrokeMesh(ShapeStrokeGeometry.Stroke(_strokeContours, thickness, join, LineCap.Butt, true));
    }

    // =========================================================================
    // Caps
    // =========================================================================

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

    private static IReadOnlyList<Point2> TransformPoints(
        IReadOnlyList<Point2> points,
        Matrix transform)
    {
        var result = new Point2[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            result[i] =
                Vector2.Transform(
                    new Vector2(
                        points[i].X,
                        points[i].Y),
                    transform).ToPoint2();
        }

        return result;
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
    // Brush / Pen overloads
    // =========================================================================

    /// <summary>
    /// Adds a filled rectangle using the specified brush.
    /// </summary>
    /// <param name="bounds">The rectangle bounds.</param>
    /// <param name="brush">The brush used to fill the rectangle.</param>
    /// <param name="transform">The transformation applied to the geometry.</param>
    public void AddFillRectangle(
        Bounds2 bounds,
        Brush brush,
        Matrix transform) =>
        WithBrush(
            brush,
            transform,
            () => AddFillRectangle(bounds, transform));

    /// <summary>
    /// Adds a filled rectangle using the specified brush.
    /// </summary>
    /// <param name="bounds">The rectangle bounds.</param>
    /// <param name="brush">The brush used to fill the rectangle.</param>
    public void AddFillRectangle(
        Bounds2 bounds,
        Brush brush) =>
        AddFillRectangle(
            bounds,
            brush,
            Matrix.Identity);

    /// <summary>
    /// Adds a filled polygon using the specified brush.
    /// </summary>
    /// <param name="polygon">The polygon contours to render.</param>
    /// <param name="brush">The brush used to fill the polygon.</param>
    /// <param name="transform">The transformation applied to the geometry.</param>
    public void AddFillPolygon(
        IReadOnlyList<IReadOnlyList<Point2>> polygon,
        Brush brush,
        Matrix transform) =>
        WithBrush(
            brush,
            transform,
            () => AddFillPolygon(polygon, transform));

    /// <summary>
    /// Adds a filled polygon using the specified brush.
    /// </summary>
    /// <param name="polygon">The polygon contours to render.</param>
    /// <param name="brush">The brush used to fill the polygon.</param>
    public void AddFillPolygon(
        IReadOnlyList<IReadOnlyList<Point2>> polygon,
        Brush brush) =>
        AddFillPolygon(
            polygon,
            brush,
            Matrix.Identity);

    /// <summary>
    /// Adds filled path geometry using the specified brush.
    /// </summary>
    /// <param name="path">The path containing polygon geometry to render.</param>
    /// <param name="brush">The brush used to fill the path.</param>
    /// <param name="transform">The transformation applied to the geometry.</param>
    public void AddFillPolygon(
        Path path,
        Brush brush,
        Matrix transform) =>
        WithBrush(
            brush,
            transform,
            () => AddFillPolygon(path, transform));

    /// <summary>
    /// Adds filled path geometry using the specified brush.
    /// </summary>
    /// <param name="path">The path containing polygon geometry to render.</param>
    /// <param name="brush">The brush used to fill the path.</param>
    public void AddFillPolygon(
        Path path,
        Brush brush) =>
        AddFillPolygon(
            path,
            brush,
            Matrix.Identity);

    /// <summary>
    /// Adds a filled ellipse using the specified brush.
    /// </summary>
    /// <param name="bounds">The ellipse bounds.</param>
    /// <param name="brush">The brush used to fill the ellipse.</param>
    /// <param name="transform">The transformation applied to the geometry.</param>
    /// <param name="segments">The number of segments used to approximate the ellipse.</param>
    public void AddFillEllipse(
        Bounds2 bounds,
        Brush brush,
        Matrix transform,
        int segments = 32) =>
        WithBrush(
            brush,
            transform,
            () => AddFillEllipse(
                bounds,
                transform,
                segments));

    /// <summary>
    /// Adds a filled ellipse using the specified brush.
    /// </summary>
    /// <param name="bounds">The ellipse bounds.</param>
    /// <param name="brush">The brush used to fill the ellipse.</param>
    /// <param name="segments">The number of segments used to approximate the ellipse.</param>
    public void AddFillEllipse(
        Bounds2 bounds,
        Brush brush,
        int segments = 32) =>
        AddFillEllipse(
            bounds,
            brush,
            Matrix.Identity,
            segments);

    /// <summary>
    /// Adds a filled ellipse using the specified brush.
    /// </summary>
    /// <param name="center">The ellipse center.</param>
    /// <param name="radius">The horizontal and vertical radii.</param>
    /// <param name="brush">The brush used to fill the ellipse.</param>
    /// <param name="transform">The transformation applied to the geometry.</param>
    /// <param name="segments">The number of segments used to approximate the ellipse.</param>
    public void AddFillEllipse(
        Point2 center,
        Vector2 radius,
        Brush brush,
        Matrix transform,
        int segments = 32) =>
        WithBrush(
            brush,
            transform,
            () => AddFillEllipse(
                center,
                radius,
                transform,
                segments));

    /// <summary>
    /// Adds a filled ellipse using the specified brush.
    /// </summary>
    /// <param name="center">The ellipse center.</param>
    /// <param name="radius">The horizontal and vertical radii.</param>
    /// <param name="brush">The brush used to fill the ellipse.</param>
    /// <param name="segments">The number of segments used to approximate the ellipse.</param>
    public void AddFillEllipse(
        Point2 center,
        Vector2 radius,
        Brush brush,
        int segments = 32) =>
        AddFillEllipse(
            center,
            radius,
            brush,
            Matrix.Identity,
            segments);

    /// <summary>
    /// Adds a stroked rectangle using the specified pen.
    /// </summary>
    /// <param name="bounds">The rectangle bounds.</param>
    /// <param name="pen">The pen used to render the stroke.</param>
    /// <param name="transform">The transformation applied to the geometry.</param>
    public void AddStrokeRectangle(
        Bounds2 bounds,
        Pen pen,
        Matrix transform)
    {
        ArgumentNullException.ThrowIfNull(pen);

        WithBrush(
            pen.Brush,
            transform,
            () => AddStrokeRectangle(
                bounds,
                pen.Thickness,
                pen.Join,
                transform));
    }

    /// <summary>
    /// Adds a stroked rectangle using the specified pen.
    /// </summary>
    /// <param name="bounds">The rectangle bounds.</param>
    /// <param name="pen">The pen used to render the stroke.</param>
    public void AddStrokeRectangle(
        Bounds2 bounds,
        Pen pen) =>
        AddStrokeRectangle(
            bounds,
            pen,
            Matrix.Identity);

    /// <summary>
    /// Adds a line segment using the specified pen.
    /// </summary>
    /// <param name="start">The line start point.</param>
    /// <param name="end">The line end point.</param>
    /// <param name="pen">The pen used to render the line.</param>
    /// <param name="transform">The transformation applied to the geometry.</param>
    public void AddLine(
        Point2 start,
        Point2 end,
        Pen pen,
        Matrix transform)
    {
        ArgumentNullException.ThrowIfNull(pen);

        WithBrush(
            pen.Brush,
            transform,
            () => AddLine(
                start,
                end,
                pen.Thickness,
                pen.Cap,
                transform));
    }

    /// <summary>
    /// Adds a line segment using the specified pen.
    /// </summary>
    /// <param name="start">The line start point.</param>
    /// <param name="end">The line end point.</param>
    /// <param name="pen">The pen used to render the line.</param>
    public void AddLine(
        Point2 start,
        Point2 end,
        Pen pen) =>
        AddLine(
            start,
            end,
            pen,
            Matrix.Identity);

    /// <summary>
    /// Adds an open polyline using the specified pen.
    /// </summary>
    /// <param name="points">The points that define the line.</param>
    /// <param name="pen">The pen used to render the line.</param>
    /// <param name="transform">The transformation applied to the geometry.</param>
    public void AddLine(
        IReadOnlyList<Point2> points,
        Pen pen,
        Matrix transform)
    {
        ArgumentNullException.ThrowIfNull(pen);

        WithBrush(
            pen.Brush,
            transform,
            () => AddLine(
                points,
                pen.Thickness,
                pen.Join,
                pen.Cap,
                transform));
    }

    /// <summary>
    /// Adds an open polyline using the specified pen.
    /// </summary>
    /// <param name="points">The points that define the line.</param>
    /// <param name="pen">The pen used to render the line.</param>
    public void AddLine(
        IReadOnlyList<Point2> points,
        Pen pen) =>
        AddLine(
            points,
            pen,
            Matrix.Identity);

    /// <summary>
    /// Adds a closed polygon stroke using the specified pen.
    /// </summary>
    /// <param name="points">The polygon points.</param>
    /// <param name="pen">The pen used to render the stroke.</param>
    /// <param name="transform">The transformation applied to the geometry.</param>
    public void AddStrokePolygon(
        IReadOnlyList<Point2> points,
        Pen pen,
        Matrix transform)
    {
        ArgumentNullException.ThrowIfNull(pen);

        WithBrush(
            pen.Brush,
            transform,
            () => AddStrokePolygon(
                points,
                pen.Thickness,
                pen.Join,
                transform));
    }

    /// <summary>
    /// Adds a closed polygon stroke using the specified pen.
    /// </summary>
    /// <param name="points">The polygon points.</param>
    /// <param name="pen">The pen used to render the stroke.</param>
    public void AddStrokePolygon(
        IReadOnlyList<Point2> points,
        Pen pen) =>
        AddStrokePolygon(
            points,
            pen,
            Matrix.Identity);

    /// <summary>
    /// Adds a path stroke using the specified pen.
    /// </summary>
    /// <param name="path">The path containing polygon geometry to stroke.</param>
    /// <param name="pen">The pen used to render the stroke.</param>
    /// <param name="transform">The transformation applied to the geometry.</param>
    public void AddStrokePolygon(
        Path path,
        Pen pen,
        Matrix transform)
    {
        ArgumentNullException.ThrowIfNull(pen);

        WithBrush(
            pen.Brush,
            transform,
            () => AddStrokePolygon(
                path,
                pen.Thickness,
                pen.Join,
                transform));
    }

    /// <summary>
    /// Adds a path stroke using the specified pen.
    /// </summary>
    /// <param name="path">The path containing polygon geometry to stroke.</param>
    /// <param name="pen">The pen used to render the stroke.</param>
    public void AddStrokePolygon(
        Path path,
        Pen pen) =>
        AddStrokePolygon(
            path,
            pen,
            Matrix.Identity);

    /// <summary>
    /// Adds an ellipse stroke using the specified pen.
    /// </summary>
    /// <param name="bounds">The ellipse bounds.</param>
    /// <param name="pen">The pen used to render the stroke.</param>
    /// <param name="transform">The transformation applied to the geometry.</param>
    /// <param name="segments">The number of segments used to approximate the ellipse.</param>
    public void AddStrokeEllipse(
        Bounds2 bounds,
        Pen pen,
        Matrix transform,
        int segments = 32)
    {
        ArgumentNullException.ThrowIfNull(pen);

        WithBrush(
            pen.Brush,
            transform,
            () => AddStrokeEllipse(
                bounds,
                pen.Thickness,
                pen.Join,
                transform,
                segments));
    }

    /// <summary>
    /// Adds an ellipse stroke using the specified pen.
    /// </summary>
    /// <param name="bounds">The ellipse bounds.</param>
    /// <param name="pen">The pen used to render the stroke.</param>
    /// <param name="segments">The number of segments used to approximate the ellipse.</param>
    public void AddStrokeEllipse(
        Bounds2 bounds,
        Pen pen,
        int segments = 32) =>
        AddStrokeEllipse(
            bounds,
            pen,
            Matrix.Identity,
            segments);

    /// <summary>
    /// Adds an ellipse stroke using the specified pen.
    /// </summary>
    /// <param name="center">The ellipse center.</param>
    /// <param name="radius">The horizontal and vertical radii.</param>
    /// <param name="pen">The pen used to render the stroke.</param>
    /// <param name="transform">The transformation applied to the geometry.</param>
    /// <param name="segments">The number of segments used to approximate the ellipse.</param>
    public void AddStrokeEllipse(
        Point2 center,
        Vector2 radius,
        Pen pen,
        Matrix transform,
        int segments = 32)
    {
        ArgumentNullException.ThrowIfNull(pen);

        WithBrush(
            pen.Brush,
            transform,
            () => AddStrokeEllipse(
                center,
                radius,
                pen.Thickness,
                pen.Join,
                transform,
                segments));
    }

    /// <summary>
    /// Adds an ellipse stroke using the specified pen.
    /// </summary>
    /// <param name="center">The ellipse center.</param>
    /// <param name="radius">The horizontal and vertical radii.</param>
    /// <param name="pen">The pen used to render the stroke.</param>
    /// <param name="segments">The number of segments used to approximate the ellipse.</param>
    public void AddStrokeEllipse(
        Point2 center,
        Vector2 radius,
        Pen pen,
        int segments = 32) =>
        AddStrokeEllipse(
            center,
            radius,
            pen,
            Matrix.Identity,
            segments);

    /// <summary>Adds a filled rectangle with explicit geometric corners.</summary>
    /// <param name="bounds">The local rectangle bounds.</param>
    /// <param name="cornerType">The geometric corner treatment, independent of the stroke join.</param>
    /// <param name="cornerSize">The local radius or diagonal-cut extent; clamped to half the smaller dimension.</param>
    public void AddFillRectangle(Bounds2 bounds, CornerType cornerType, float cornerSize) =>
        AddFillRectangle(bounds, cornerType, cornerSize, Matrix.Identity);

    /// <summary>Adds a transformed filled rectangle with explicit geometric corners.</summary>
    /// <param name="bounds">The local rectangle bounds.</param>
    /// <param name="cornerType">The geometric corner treatment, independent of the stroke join.</param>
    /// <param name="cornerSize">The local radius or diagonal-cut extent; clamped to half the smaller dimension.</param>
    /// <param name="transform">The finite transformation applied to the rectangle contour.</param>
    public void AddFillRectangle(Bounds2 bounds, CornerType cornerType, float cornerSize, Matrix transform)
    {
        CheckBegin();
        ShapeStrokeGeometry.ValidateBounds(bounds);
        ValidateShapeTransform(transform);
        Span<Vector2> points = stackalloc Vector2[ShapeStrokeGeometry.RectangleCapacity];
        int count = ShapeStrokeGeometry.RectangleContour(bounds, cornerType, cornerSize, points);
        if (count < 3) return;
        for (int i = 0; i < count; i++) ShapeStrokeGeometry.ValidatePoint(Vector2.Transform(points[i], transform));
        int start = _vertices.Count;
        float left = MathF.Min(bounds.Left, bounds.Right), top = MathF.Min(bounds.Top, bounds.Bottom);
        float width = MathF.Abs(bounds.Width), height = MathF.Abs(bounds.Height);
        for (int i = 0; i < count; i++)
            AddVertex(Vector2.Transform(points[i], transform), new Vector2((points[i].X - left) / width, (points[i].Y - top) / height));
        for (int i = 1; i < count - 1; i++)
        {
            _indices.Add(start); _indices.Add(start + i); _indices.Add(start + i + 1);
        }
    }

    /// <summary>Adds a brush-filled rectangle with explicit geometric corners.</summary>
    /// <param name="bounds">The local rectangle bounds.</param>
    /// <param name="brush">The fill brush.</param>
    /// <param name="cornerType">The geometric corner treatment, independent of the stroke join.</param>
    /// <param name="cornerSize">The local radius or diagonal-cut extent; clamped to half the smaller dimension.</param>
    public void AddFillRectangle(Bounds2 bounds, Brush brush, CornerType cornerType, float cornerSize) =>
        AddFillRectangle(bounds, brush, cornerType, cornerSize, Matrix.Identity);

    /// <summary>Adds a transformed brush-filled rectangle with explicit geometric corners.</summary>
    /// <param name="bounds">The local rectangle bounds.</param>
    /// <param name="brush">The fill brush.</param>
    /// <param name="cornerType">The geometric corner treatment, independent of the stroke join.</param>
    /// <param name="cornerSize">The local radius or diagonal-cut extent; clamped to half the smaller dimension.</param>
    /// <param name="transform">The finite transformation applied to the rectangle contour.</param>
    public void AddFillRectangle(Bounds2 bounds, Brush brush, CornerType cornerType, float cornerSize, Matrix transform) =>
        WithBrush(brush, transform, () => AddFillRectangle(bounds, cornerType, cornerSize, transform));

    /// <summary>Adds a centered rectangle outline with explicit geometric corners.</summary>
    /// <param name="bounds">The local rectangle bounds.</param>
    /// <param name="thickness">The finite centered stroke width in transformed coordinates; non-positive values add no geometry.</param>
    /// <param name="cornerType">The geometric corner treatment, independent of the stroke join.</param>
    /// <param name="cornerSize">The local radius or diagonal-cut extent; clamped to half the smaller dimension.</param>
    public void AddStrokeRectangle(Bounds2 bounds, float thickness, CornerType cornerType, float cornerSize) =>
        AddStrokeRectangle(bounds, thickness, cornerType, cornerSize, LineJoin.Miter, Matrix.Identity);

    /// <summary>Adds a rectangle outline with independent geometric corners and stroke joins.</summary>
    /// <param name="bounds">The local rectangle bounds.</param>
    /// <param name="thickness">The finite centered stroke width; non-positive values add no geometry.</param>
    /// <param name="cornerType">The geometric corner treatment.</param>
    /// <param name="cornerSize">The local radius or diagonal-cut extent, clamped to half the smaller dimension.</param>
    /// <param name="join">The join used on the generated corner contour.</param>
    public void AddStrokeRectangle(Bounds2 bounds, float thickness, CornerType cornerType, float cornerSize, LineJoin join) =>
        AddStrokeRectangle(bounds, thickness, cornerType, cornerSize, join, Matrix.Identity);

    /// <summary>Adds a transformed rectangle outline with explicit geometric corners.</summary>
    /// <param name="bounds">The local rectangle bounds.</param>
    /// <param name="thickness">The finite centered stroke width in transformed coordinates; non-positive values add no geometry.</param>
    /// <param name="cornerType">The geometric corner treatment, independent of the stroke join.</param>
    /// <param name="cornerSize">The local radius or diagonal-cut extent; clamped to half the smaller dimension.</param>
    /// <param name="transform">The finite transformation applied to the rectangle contour.</param>
    public void AddStrokeRectangle(Bounds2 bounds, float thickness, CornerType cornerType, float cornerSize, Matrix transform) =>
        AddStrokeRectangle(bounds, thickness, cornerType, cornerSize, LineJoin.Miter, transform);

    /// <summary>Adds a transformed rectangle outline with independent corner and join settings.</summary>
    /// <param name="bounds">The local rectangle bounds.</param>
    /// <param name="thickness">The finite centered stroke width in transformed coordinates; non-positive values add no geometry.</param>
    /// <param name="cornerType">The geometric corner treatment, independent of the stroke join.</param>
    /// <param name="cornerSize">The local radius or diagonal-cut extent; clamped to half the smaller dimension.</param>
    /// <param name="join">The join used on the generated corner contour.</param>
    /// <param name="transform">The finite transformation applied to the rectangle contour.</param>
    /// <remarks>Corner size is measured before transformation; thickness is measured after transformation, matching existing rectangle strokes.</remarks>
    public void AddStrokeRectangle(Bounds2 bounds, float thickness, CornerType cornerType, float cornerSize, LineJoin join, Matrix transform)
    {
        CheckBegin();
        ShapeStrokeGeometry.ValidateBounds(bounds);
        ValidateStrokeSettings(thickness, join, LineCap.Butt, transform);
        Span<Vector2> points = stackalloc Vector2[ShapeStrokeGeometry.RectangleCapacity];
        int count = ShapeStrokeGeometry.RectangleContour(bounds, cornerType, cornerSize, points);
        if (count < 3 || thickness <= 0) return;
        if ((cornerType == CornerType.None || cornerSize == 0) && join == LineJoin.Miter && transform == Matrix.Identity)
        {
            Span<Vector2> vertices = stackalloc Vector2[8];
            Span<int> indices = stackalloc int[24];
            int indexCount = ShapeStrokeGeometry.RectangleRing(bounds, thickness, vertices, indices, out int vertexCount);
            AppendMesh(vertices[..vertexCount], indices[..indexCount]);
            return;
        }
        var contour = GetStrokeBuffer(0);
        for (int i = 0; i < count; i++)
        {
            Vector2 point = Vector2.Transform(points[i], transform);
            ShapeStrokeGeometry.ValidatePoint(point);
            contour.Add(point);
        }
        _strokeContours.Clear();
        _strokeContours.Add(contour);
        AppendStrokeMesh(ShapeStrokeGeometry.Stroke(_strokeContours, thickness, join, LineCap.Butt, true));
    }

    /// <summary>Adds a pen-stroked rectangle with explicit geometric corners.</summary>
    /// <param name="bounds">The local rectangle bounds.</param>
    /// <param name="pen">The stroke brush, thickness, and join settings.</param>
    /// <param name="cornerType">The geometric corner treatment, independent of the stroke join.</param>
    /// <param name="cornerSize">The local radius or diagonal-cut extent; clamped to half the smaller dimension.</param>
    public void AddStrokeRectangle(Bounds2 bounds, Pen pen, CornerType cornerType, float cornerSize) =>
        AddStrokeRectangle(bounds, pen, cornerType, cornerSize, Matrix.Identity);

    /// <summary>Adds a transformed pen-stroked rectangle with explicit geometric corners.</summary>
    /// <param name="bounds">The local rectangle bounds.</param>
    /// <param name="pen">The stroke brush, thickness, and join settings.</param>
    /// <param name="cornerType">The geometric corner treatment, independent of the stroke join.</param>
    /// <param name="cornerSize">The local radius or diagonal-cut extent; clamped to half the smaller dimension.</param>
    /// <param name="transform">The finite transformation applied to the rectangle contour.</param>
    public void AddStrokeRectangle(Bounds2 bounds, Pen pen, CornerType cornerType, float cornerSize, Matrix transform)
    {
        ArgumentNullException.ThrowIfNull(pen);
        WithBrush(pen.Brush, transform, () => AddStrokeRectangle(bounds, pen.Thickness, cornerType, cornerSize, pen.Join, transform));
    }

    /// <summary>Validates finite transforms before geometry is appended.</summary>
    /// <param name="transform">The shape transformation.</param>
    private static void ValidateShapeTransform(Matrix transform)
    {
        if (!IsFinite(transform)) throw new ArgumentOutOfRangeException(nameof(transform));
    }

    /// <summary>Validates stroke options independently of the number of usable segments.</summary>
    /// <param name="thickness">The stroke width.</param>
    /// <param name="join">The segment join.</param>
    /// <param name="cap">The open endpoint shape.</param>
    /// <param name="transform">The shape transformation.</param>
    private static void ValidateStrokeSettings(float thickness, LineJoin join, LineCap cap, Matrix transform)
    {
        ShapeStrokeGeometry.ValidateThickness(thickness);
        ValidateShapeTransform(transform);
        if (join is not (LineJoin.Miter or LineJoin.Bevel or LineJoin.Round)) throw new ArgumentOutOfRangeException(nameof(join));
        if (cap is not (LineCap.Butt or LineCap.Square or LineCap.Round)) throw new ArgumentOutOfRangeException(nameof(cap));
    }

    /// <summary>Clears and reuses one transformed contour buffer.</summary>
    /// <param name="index">The contour slot to use.</param>
    /// <returns>The reusable empty contour.</returns>
    private List<Vector2> GetStrokeBuffer(int index)
    {
        while (_strokeBuffers.Count <= index) _strokeBuffers.Add(new List<Vector2>());
        var result = _strokeBuffers[index];
        result.Clear();
        return result;
    }

    /// <summary>Copies a transformed centerline without allocating a new array for each draw.</summary>
    /// <param name="points">The source points.</param>
    /// <param name="transform">The source transformation.</param>
    /// <param name="index">The reusable contour slot.</param>
    /// <returns>The finite transformed centerline.</returns>
    private List<Vector2> PrepareStrokeContour(IReadOnlyList<Point2> points, Matrix transform, int index)
    {
        var result = GetStrokeBuffer(index);
        for (int i = 0; i < points.Count; i++)
        {
            ShapeStrokeGeometry.ValidatePoint(new Vector2(points[i].X, points[i].Y));
            Vector2 point = Vector2.Transform(points[i], transform);
            ShapeStrokeGeometry.ValidatePoint(point);
            result.Add(point);
        }
        return result;
    }

    /// <summary>Appends a fully computed, non-overlapping stroke mesh.</summary>
    /// <param name="mesh">The triangulated stroke region.</param>
    private void AppendStrokeMesh(PolygonTriangulationResult mesh)
    {
        int start = _vertices.Count;
        for (int i = 0; i < mesh.Vertices.Count; i++) AddVertex(mesh.Vertices[i], Vector2.Zero);
        for (int i = 0; i < mesh.Indices.Count; i++) _indices.Add(start + mesh.Indices[i]);
    }

    /// <summary>Appends a small stack-allocated mesh after all points have been validated.</summary>
    /// <param name="vertices">The mesh vertices.</param>
    /// <param name="indices">Triangle indices relative to these vertices.</param>
    private void AppendMesh(ReadOnlySpan<Vector2> vertices, ReadOnlySpan<int> indices)
    {
        int start = _vertices.Count;
        foreach (Vector2 point in vertices) AddVertex(point, Vector2.Zero);
        foreach (int index in indices) _indices.Add(start + index);
    }

    private void WithBrush(
        Brush brush,
        Matrix transform,
        Action action)
    {
        ArgumentNullException.ThrowIfNull(brush);
        ArgumentNullException.ThrowIfNull(action);

        Brush previousBrush = _brush;
        bool previousDeferBrushApplication =
            _deferBrushApplication;

        int vertexStart =
            _vertices.Count;

        int indexStart = _indices.Count;
        _brush = brush;
        _deferBrushApplication = true;

        try
        {
            action();

            ApplyBrush(
                brush,
                transform,
                vertexStart,
                _vertices.Count - vertexStart);
        }
        catch
        {
            _vertices.RemoveRange(vertexStart, _vertices.Count - vertexStart);
            _vertexBrushes.RemoveRange(vertexStart, _vertexBrushes.Count - vertexStart);
            _indices.RemoveRange(indexStart, _indices.Count - indexStart);
            throw;
        }
        finally
        {
            _brush = previousBrush;
            _deferBrushApplication =
                previousDeferBrushApplication;
        }
    }

    private void ApplyBrush(
        Brush brush,
        Matrix transform,
        int vertexStart,
        int vertexCount)
    {
        if (vertexCount <= 0)
            return;

        Matrix inverseTransform =
            Matrix.Invert(transform);

        bool useInverseTransform =
            IsFinite(inverseTransform);

        Vector2 boundsMin = new(
            float.MaxValue,
            float.MaxValue);

        Vector2 boundsMax = new(
            float.MinValue,
            float.MinValue);

        for (int i = vertexStart;
             i < vertexStart + vertexCount;
             i++)
        {
            Vector2 position = new(
                _vertices[i].Position.X,
                _vertices[i].Position.Y);

            Vector2 localPosition =
                useInverseTransform
                    ? Vector2.Transform(
                        position,
                        inverseTransform)
                    : position;

            boundsMin =
                Vector2.Min(boundsMin, localPosition);

            boundsMax =
                Vector2.Max(boundsMax, localPosition);
        }

        float width =
            boundsMax.X - boundsMin.X;

        float height =
            boundsMax.Y - boundsMin.Y;

        for (int i = vertexStart;
             i < vertexStart + vertexCount;
             i++)
        {
            ShapeVertex vertex =
                _vertices[i];

            Vector2 position = new(
                vertex.Position.X,
                vertex.Position.Y);

            Vector2 localPosition =
                useInverseTransform
                    ? Vector2.Transform(
                        position,
                        inverseTransform)
                    : position;

            Vector2 textureCoordinate = new(
                MathF.Abs(width) <= Epsilon
                    ? 0.5f
                    : (localPosition.X - boundsMin.X) / width,
                MathF.Abs(height) <= Epsilon
                    ? 0.5f
                    : (localPosition.Y - boundsMin.Y) / height);

            var context = new BrushVertexContext(
                position,
                localPosition,
                textureCoordinate,
                boundsMin,
                boundsMax);

            brush.Apply(
                ref vertex,
                in context);

            _vertices[i] = vertex;
        }
    }

    private static bool IsFinite(Matrix matrix) =>
        float.IsFinite(matrix.M11) &&
        float.IsFinite(matrix.M12) &&
        float.IsFinite(matrix.M13) &&
        float.IsFinite(matrix.M14) &&
        float.IsFinite(matrix.M21) &&
        float.IsFinite(matrix.M22) &&
        float.IsFinite(matrix.M23) &&
        float.IsFinite(matrix.M24) &&
        float.IsFinite(matrix.M31) &&
        float.IsFinite(matrix.M32) &&
        float.IsFinite(matrix.M33) &&
        float.IsFinite(matrix.M34) &&
        float.IsFinite(matrix.M41) &&
        float.IsFinite(matrix.M42) &&
        float.IsFinite(matrix.M43) &&
        float.IsFinite(matrix.M44);

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
        ShapeStrokeGeometry.ValidatePoint(position);
        ShapeStrokeGeometry.ValidatePoint(textureCoordinate);
        var vertex = new ShapeVertex(
            new Vector3(position, 0f),
            Color.White,
            textureCoordinate);

        if (!_deferBrushApplication)
        {
            var context = new BrushVertexContext(
                position,
                position,
                textureCoordinate,
                Vector2.Zero,
                Vector2.One);

            _brush.Apply(
                ref vertex,
                in context);
        }

        _vertices.Add(vertex);
        _vertexBrushes.Add(_brush);
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

        BuildBrushCommands();

        for (int i = 0; i < _brushCommands.Count; i++)
        {
            BrushCommand command =
                _brushCommands[i];

            command.Brush.ConfigureShader(_shader);
            _shader.Apply();

            foreach (EffectPass pass in
                     _shader.CurrentTechnique.Passes)
            {
                pass.Apply();

                _graphicsDevice.Textures[0] =
                    _whiteTexture;

                _graphicsDevice.SamplerStates[0] =
                    SamplerState.LinearClamp;

                command.Brush.ApplyDeviceState(
                    _graphicsDevice);

                _graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    0,
                    command.StartIndex,
                    command.IndexCount / 3);
            }
        }
    }

    private void BuildBrushCommands()
    {
        _brushCommands.Clear();

        for (int startIndex = 0;
             startIndex < _indices.Count;
             startIndex += 3)
        {
            Brush brush =
                _vertexBrushes[_indices[startIndex]];

            int commandIndex =
                _brushCommands.Count - 1;

            if (commandIndex >= 0 &&
                _brushCommands[commandIndex].Brush
                    .CanBatchWith(brush))
            {
                BrushCommand command =
                    _brushCommands[commandIndex];

                _brushCommands[commandIndex] =
                    new BrushCommand(
                        command.Brush,
                        command.StartIndex,
                        command.IndexCount + 3);

                continue;
            }

            _brushCommands.Add(
                new BrushCommand(
                    brush,
                    startIndex,
                    3));
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
                new ShapeVertex[
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
                    ShapeVertex.VertexDeclaration,
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

    /// <summary>
    /// Releases graphics resources used by this batch.
    /// </summary>
    /// <remarks>
    /// Calling this method more than once is safe.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
            return;

        _vertexBuffer?.Dispose();
        _indexBuffer?.Dispose();
        _whiteTexture.Dispose();

        _vertexBuffer = null;
        _indexBuffer = null;

        _vertices.Clear();
        _vertexBrushes.Clear();
        _indices.Clear();
        _brushCommands.Clear();

        _shader = null;
        _camera = null;

        _disposed = true;
    }
}
