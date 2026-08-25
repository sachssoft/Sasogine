using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools;

public sealed class ToolVectorBSplineSegment : ToolVectorVariableSegment
{
    private Vector2 _startPositionCache;
    private Vector2 _nodePositionCache;
    private float _sampleLengthCache;
    private int _degreeCache;

    private Vector2[]? _controlPositionsCache;
    private Vector2[]? _sampledVerticesCache;

    public ToolVectorBSplineSegment()
    {
    }

    public ToolVectorBSplineSegment(
        Vector2 position,
        IEnumerable<Vector2> controlPoints,
        int degree = 3,
        bool isSelected = false)
        : this()
    {
        if (controlPoints is null)
            throw new ArgumentNullException(nameof(controlPoints));

        if (degree < 1)
            throw new ArgumentOutOfRangeException(nameof(degree));

        Node.Position = position;
        Node.IsSelected = isSelected;
        Degree = degree;

        foreach (Vector2 point in controlPoints)
            ControlNodes.Add(new ToolVectorNode(point));
    }

    /// <summary>
    /// Gets or sets the degree of the B-spline.
    /// </summary>
    public int Degree { get; set; } = 3;

    /// <summary>
    /// Gets the sampled vertices of the B-spline.
    /// </summary>
    /// <param name="startPosition">
    /// The start position of the segment.
    /// </param>
    /// <param name="sampleLength">
    /// The desired distance between sampled vertices.
    /// </param>
    public override Vector2[] GetVertices(
        Vector2 startPosition,
        float sampleLength)
    {
        if (sampleLength <= 0f)
            throw new ArgumentOutOfRangeException(
                nameof(sampleLength));

        if (Degree < 1)
            throw new InvalidOperationException(
                "Degree must be greater than zero.");

        int pointCount =
            ControlNodes.Count + 2;

        if (Degree >= pointCount)
            return Array.Empty<Vector2>();

        bool controlPointsChanged =
            _controlPositionsCache == null ||
            _controlPositionsCache.Length != ControlNodes.Count ||
            _nodePositionCache != Node.Position;

        if (!controlPointsChanged)
        {
            for (int i = 0;
                 i < ControlNodes.Count;
                 i++)
            {
                if (_controlPositionsCache[i] !=
                    ControlNodes[i].Position)
                {
                    controlPointsChanged = true;
                    break;
                }
            }
        }

        if (_sampledVerticesCache == null ||
            _startPositionCache != startPosition ||
            _sampleLengthCache != sampleLength ||
            _degreeCache != Degree ||
            controlPointsChanged)
        {
            _startPositionCache =
                startPosition;

            _nodePositionCache =
                Node.Position;

            _sampleLengthCache =
                sampleLength;

            _degreeCache =
                Degree;

            Vector2[] points =
                new Vector2[pointCount];

            points[0] =
                startPosition;

            for (int i = 0;
                 i < ControlNodes.Count;
                 i++)
            {
                points[i + 1] =
                    ControlNodes[i].Position;
            }

            points[^1] =
                Node.Position;

            int segments =
                CalculateSegments(
                    points,
                    sampleLength);

            _sampledVerticesCache =
                GeometrySampler1.SampleBSpline(
                    points,
                    Degree,
                    segments);

            _controlPositionsCache =
                new Vector2[ControlNodes.Count];

            for (int i = 0;
                 i < ControlNodes.Count;
                 i++)
            {
                _controlPositionsCache[i] =
                    ControlNodes[i].Position;
            }
        }

        return _sampledVerticesCache;
    }

    /// <summary>
    /// Calculates the number of sampling segments
    /// from the approximate control polygon length.
    /// </summary>
    private static int CalculateSegments(
        Vector2[] points,
        float sampleLength)
    {
        float length = 0f;

        for (int i = 1;
             i < points.Length;
             i++)
        {
            length +=
                Vector2.Distance(
                    points[i - 1],
                    points[i]);
        }

        return Math.Max(
            1,
            (int)MathF.Ceiling(
                length / sampleLength));
    }
}