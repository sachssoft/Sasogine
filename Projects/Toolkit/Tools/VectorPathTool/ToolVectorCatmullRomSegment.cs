using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools;

public sealed class ToolVectorCatmullRomSegment : ToolVectorVariableSegment
{
    private Vector2 _startPositionCache;
    private Vector2 _nodePositionCache;
    private float _sampleLengthCache;
    private bool _closedCache;
    private Vector2[]? _controlPositionsCache;
    private Vector2[]? _sampledVerticesCache;

    public ToolVectorCatmullRomSegment()
    {
    }

    public ToolVectorCatmullRomSegment(
        Vector2 position,
        IEnumerable<Vector2> controlPoints,
        bool isSelected)
        : this()
    {
        if (controlPoints is null)
            throw new ArgumentNullException(nameof(controlPoints));

        Node.Position = position;
        Node.IsSelected = isSelected;

        foreach (Vector2 point in controlPoints)
            ControlNodes.Add(new ToolVectorNode(point));
    }

    /// <summary>
    /// Gets or sets whether the Catmull-Rom spline is closed.
    /// </summary>
    public bool Closed { get; set; }

    /// <summary>
    /// Gets the sampled vertices of the Catmull-Rom spline.
    /// </summary>
    public override Vector2[] GetVertices(
        Vector2 startPosition,
        float sampleLength)
    {
        if (sampleLength <= 0f)
            throw new ArgumentOutOfRangeException(nameof(sampleLength));

        int pointCount = ControlNodes.Count + 2;

        if (pointCount < 2)
            return Array.Empty<Vector2>();

        bool controlPointsChanged =
            _controlPositionsCache == null ||
            _controlPositionsCache.Length != ControlNodes.Count ||
            _nodePositionCache != Node.Position;

        if (!controlPointsChanged)
        {
            for (int i = 0; i < ControlNodes.Count; i++)
            {
                if (_controlPositionsCache[i] != ControlNodes[i].Position)
                {
                    controlPointsChanged = true;
                    break;
                }
            }
        }

        if (_sampledVerticesCache == null ||
            _startPositionCache != startPosition ||
            _sampleLengthCache != sampleLength ||
            _closedCache != Closed ||
            controlPointsChanged)
        {
            _startPositionCache = startPosition;
            _nodePositionCache = Node.Position;
            _sampleLengthCache = sampleLength;
            _closedCache = Closed;

            Vector2[] points = new Vector2[pointCount];
            points[0] = startPosition;

            for (int i = 0; i < ControlNodes.Count; i++)
                points[i + 1] = ControlNodes[i].Position;

            points[^1] = Node.Position;

            int segmentsPerSpan =
                CalculateSegments(points, sampleLength);

            _sampledVerticesCache =
                GeometrySampler1.SampleCatmullRom(
                    points,
                    segmentsPerSpan,
                    Closed);

            _controlPositionsCache =
                new Vector2[ControlNodes.Count];

            for (int i = 0; i < ControlNodes.Count; i++)
                _controlPositionsCache[i] =
                    ControlNodes[i].Position;
        }

        return _sampledVerticesCache;
    }

    private static int CalculateSegments(
        Vector2[] points,
        float sampleLength)
    {
        float length = 0f;

        for (int i = 1; i < points.Length; i++)
            length += Vector2.Distance(
                points[i - 1],
                points[i]);

        return Math.Max(
            1,
            (int)MathF.Ceiling(length / sampleLength));
    }
}