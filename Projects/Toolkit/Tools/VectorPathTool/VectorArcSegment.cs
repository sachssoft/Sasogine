using Sachssoft.Engine.Geometry;
using System;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Represents an elliptical arc segment of a vector path.
/// </summary>
/// <remarks>
/// The start point, first control point, and endpoint define the base circular arc.
/// The second control point controls the secondary radius of the resulting ellipse.
/// </remarks>
public sealed class VectorArcSegment : VectorFixedSegment<VectorArcSegmentDefinition>
{
    private const float Epsilon = 0.000001f;

    private Point2 _startPositionCache;
    private Point2 _controlPosition0Cache;
    private Point2 _controlPosition1Cache;
    private Point2 _endPositionCache;
    private float _sampleLengthCache;
    private Point2[]? _sampledVerticesCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorArcSegment"/> class.
    /// </summary>
    public VectorArcSegment() : this(CreateDefinition())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorArcSegment"/> class
    /// using the specified endpoint and control points.
    /// </summary>
    /// <param name="position">The endpoint of the arc segment.</param>
    /// <param name="controlPosition0">
    /// The point through which the base circular arc passes.
    /// </param>
    /// <param name="controlPosition1">
    /// The control point defining the secondary ellipse radius.
    /// </param>
    public VectorArcSegment(Point2 position, Point2 controlPosition0, Point2 controlPosition1)
        : this(position, controlPosition0, controlPosition1, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorArcSegment"/> class
    /// using the specified endpoint, control points, and selection state.
    /// </summary>
    /// <param name="position">The endpoint of the arc segment.</param>
    /// <param name="controlPosition0">
    /// The point through which the base circular arc passes.
    /// </param>
    /// <param name="controlPosition1">
    /// The control point defining the secondary ellipse radius.
    /// </param>
    /// <param name="isSelected">
    /// <see langword="true"/> if the endpoint node should initially be selected;
    /// otherwise, <see langword="false"/>.
    /// </param>
    public VectorArcSegment(
        Point2 position,
        Point2 controlPosition0,
        Point2 controlPosition1,
        bool isSelected)
        : this(CreateDefinition(position, controlPosition0, controlPosition1, isSelected))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorArcSegment"/> class
    /// using the specified definition.
    /// </summary>
    /// <param name="definition">The definition used to initialize the arc segment.</param>
    public VectorArcSegment(VectorArcSegmentDefinition definition)
        : base(controlCount: 2, definition)
    {
    }

    /// <inheritdoc/>
    protected override VectorNode CreateVectorNode(int index, VectorArcSegmentDefinition definition)
    {
        return index switch
        {
            0 => new VectorNode(definition.ControlNode0),
            1 => new VectorNode(definition.ControlNode1),
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }

    /// <inheritdoc/>
    public override Point2[] GetVertices(Point2 startPosition, float sampleLength)
    {
        if (sampleLength <= 0f)
            throw new ArgumentOutOfRangeException(nameof(sampleLength));

        Point2 controlPosition0 = GetControlNodes()[0].Position;
        Point2 controlPosition1 = GetControlNodes()[1].Position;
        Point2 endPosition = Node.Position;

        if (_sampledVerticesCache != null &&
            _startPositionCache == startPosition &&
            _controlPosition0Cache == controlPosition0 &&
            _controlPosition1Cache == controlPosition1 &&
            _endPositionCache == endPosition &&
            _sampleLengthCache == sampleLength)
        {
            return _sampledVerticesCache;
        }

        _startPositionCache = startPosition;
        _controlPosition0Cache = controlPosition0;
        _controlPosition1Cache = controlPosition1;
        _endPositionCache = endPosition;
        _sampleLengthCache = sampleLength;

        if (!TryCalculateCircularArc(
            startPosition,
            controlPosition0,
            endPosition,
            out Point2 center,
            out float radius,
            out float startAngle,
            out float sweepAngle))
        {
            _sampledVerticesCache = [startPosition, endPosition];
            return _sampledVerticesCache;
        }

        // ControlNode1 only controls the stretch of the original circular arc.
        float stretchDistance = Point2.Distance(center, controlPosition1);
        float stretch = stretchDistance / radius;

        if (!float.IsFinite(stretch) || stretch <= Epsilon)
            stretch = Epsilon;

        int segmentCount = CalculateArcSegments(
            radius,
            radius * stretch,
            sweepAngle,
            sampleLength);

        _sampledVerticesCache = SampleStretchedArc(
            center,
            radius,
            startAngle,
            sweepAngle,
            stretch,
            segmentCount);

        return _sampledVerticesCache;
    }

    private static VectorArcSegmentDefinition CreateDefinition()
    {
        return new VectorArcSegmentDefinition();
    }

    private static VectorArcSegmentDefinition CreateDefinition(
        Point2 position,
        Point2 controlPosition0,
        Point2 controlPosition1,
        bool isSelected)
    {
        var definition = new VectorArcSegmentDefinition();

        definition.Node.Position = position;
        definition.Node.IsSelected = isSelected;
        definition.ControlNode0.Position = controlPosition0;
        definition.ControlNode1.Position = controlPosition1;

        return definition;
    }

    private static bool TryCalculateCircularArc(
        Point2 start,
        Point2 control,
        Point2 end,
        out Point2 center,
        out float radius,
        out float startAngle,
        out float sweepAngle)
    {
        center = Point2.Zero;
        radius = 0f;
        startAngle = 0f;
        sweepAngle = 0f;

        float ax = start.X;
        float ay = start.Y;

        float bx = control.X;
        float by = control.Y;

        float cx = end.X;
        float cy = end.Y;

        float denominator = 2f * (
            ax * (by - cy) +
            bx * (cy - ay) +
            cx * (ay - by));

        if (MathF.Abs(denominator) <= Epsilon)
            return false;

        float aSquared = ax * ax + ay * ay;
        float bSquared = bx * bx + by * by;
        float cSquared = cx * cx + cy * cy;

        float centerX = (
            aSquared * (by - cy) +
            bSquared * (cy - ay) +
            cSquared * (ay - by)) / denominator;

        float centerY = (
            aSquared * (cx - bx) +
            bSquared * (ax - cx) +
            cSquared * (bx - ax)) / denominator;

        center = new Point2(centerX, centerY);

        radius = Point2.Distance(center, start);

        if (radius <= Epsilon)
            return false;

        startAngle = MathF.Atan2(
            start.Y - center.Y,
            start.X - center.X);

        float controlAngle = MathF.Atan2(
            control.Y - center.Y,
            control.X - center.X);

        float endAngle = MathF.Atan2(
            end.Y - center.Y,
            end.X - center.X);

        float counterClockwiseStartToControl =
            NormalizePositiveAngle(controlAngle - startAngle);

        float counterClockwiseStartToEnd =
            NormalizePositiveAngle(endAngle - startAngle);

        if (counterClockwiseStartToControl <= counterClockwiseStartToEnd)
            sweepAngle = counterClockwiseStartToEnd;
        else
            sweepAngle = counterClockwiseStartToEnd - MathF.Tau;

        return MathF.Abs(sweepAngle) > Epsilon;
    }

    private static Point2[] SampleEllipse(
        Point2 center,
        float radius,
        float secondaryRadius,
        float startAngle,
        float sweepAngle,
        int segmentCount)
    {
        var vertices = new Point2[segmentCount + 1];

        /*
         * The base circle is scaled along its local secondary axis.
         *
         * The primary axis is aligned with the start radius. This guarantees
         * that the generated ellipse still begins exactly at startPosition.
         */
        float axisX = MathF.Cos(startAngle);
        float axisY = MathF.Sin(startAngle);

        float perpendicularX = -axisY;
        float perpendicularY = axisX;

        for (int i = 0; i <= segmentCount; i++)
        {
            float amount = (float)i / segmentCount;
            float angle = sweepAngle * amount;

            float localX = MathF.Cos(angle) * radius;
            float localY = MathF.Sin(angle) * secondaryRadius;

            float x =
                center.X +
                axisX * localX +
                perpendicularX * localY;

            float y =
                center.Y +
                axisY * localX +
                perpendicularY * localY;

            vertices[i] = new Point2(x, y);
        }

        return vertices;
    }

    private static int CalculateArcSegments(
        float radius,
        float secondaryRadius,
        float sweepAngle,
        float sampleLength)
    {
        float a = MathF.Max(radius, secondaryRadius);
        float b = MathF.Min(radius, secondaryRadius);

        float sum = a + b;

        if (sum <= Epsilon)
            return 1;

        float difference = a - b;
        float h = difference * difference / (sum * sum);

        // Ramanujan approximation of the ellipse circumference.
        float circumference =
            MathF.PI *
            sum *
            (1f + 3f * h /
                (10f + MathF.Sqrt(4f - 3f * h)));

        float arcLength =
            circumference *
            MathF.Abs(sweepAngle) /
            MathF.Tau;

        return Math.Max(
            2,
            (int)MathF.Ceiling(arcLength / sampleLength));
    }

    private static float NormalizePositiveAngle(float angle)
    {
        angle %= MathF.Tau;

        if (angle < 0f)
            angle += MathF.Tau;

        return angle;
    }

    private static Point2[] SampleStretchedArc(
    Point2 center,
    float radius,
    float startAngle,
    float sweepAngle,
    float stretch,
    int segmentCount)
    {
        var vertices = new Point2[segmentCount + 1];

        for (int i = 0; i <= segmentCount; i++)
        {
            float amount = (float)i / segmentCount;
            float angle = startAngle + sweepAngle * amount;

            float x = MathF.Cos(angle) * radius;
            float y = MathF.Sin(angle) * radius;

            // ControlNode1 only stretches the circular arc.
            y *= stretch;

            vertices[i] = new Point2(
                center.X + x,
                center.Y + y);
        }

        return vertices;
    }
}