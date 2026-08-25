using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry;
using System;

namespace Sachssoft.Sasogine.Components.Tools;

public sealed class ToolVectorQuadraticBezierSegment : ToolVectorFixedSegment
{
    private Vector2 _startPositionCache;
    private float _sampleLengthCache;
    private Vector2 _controlPositionCache;
    private Vector2 _endPositionCache;

    private Vector2[]? _sampledVerticesCache;

    public ToolVectorQuadraticBezierSegment() : base(1)
    {
    }

    public ToolVectorQuadraticBezierSegment(
        Vector2 position,
        Vector2 controlPosition)
        : this(position, controlPosition, false)
    {
    }

    public ToolVectorQuadraticBezierSegment(
        Vector2 position,
        Vector2 controlPosition,
        bool isSelected) : this()
    {
        Node.Position = position;
        ControlNodes[0].Position = controlPosition;
        Node.IsSelected = isSelected;
    }

    /// <summary>
    /// Gets the sampled vertices of the quadratic Bézier segment.
    /// </summary>
    /// <param name="startPosition">
    /// The start position (P0) of the segment.
    /// </param>
    /// <param name="sampleLength">
    /// The desired distance between sampled vertices.
    /// </param>
    public override Vector2[] GetVertices(
        Vector2 startPosition,
        float sampleLength)
    {
        Vector2 controlPosition = ControlNodes[0].Position;
        Vector2 endPosition = Node.Position;

        if (_sampledVerticesCache == null ||
            _startPositionCache != startPosition ||
            _sampleLengthCache != sampleLength ||
            _controlPositionCache != controlPosition ||
            _endPositionCache != endPosition)
        {
            _startPositionCache = startPosition;
            _sampleLengthCache = sampleLength;
            _controlPositionCache = controlPosition;
            _endPositionCache = endPosition;

            int segmentCount =
                CalculateQuadraticBezierSegments(
                    startPosition,
                    controlPosition,
                    endPosition,
                    sampleLength);

            _sampledVerticesCache =
                GeometrySampler.SampleQuadraticBezier(
                    startPosition,
                    controlPosition,
                    endPosition,
                    segmentCount);
        }

        return _sampledVerticesCache;
    }

    public static int CalculateQuadraticBezierSegments(
        Vector2 startPosition,
        Vector2 controlPosition,
        Vector2 endPosition,
        float sampleLength)
    {
        if (sampleLength <= 0f)
            throw new ArgumentOutOfRangeException(nameof(sampleLength));

        float length =
            Vector2.Distance(startPosition, controlPosition) +
            Vector2.Distance(controlPosition, endPosition);

        return Math.Max(
            1,
            (int)MathF.Ceiling(length / sampleLength));
    }
}