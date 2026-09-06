using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a B-spline segment of a vector path defined by a degree and a sequence of control nodes.
    /// </summary>
    public sealed class VectorBSplineSegment : VectorVariableSegment
    {
        private Point2 _startPositionCache;
        private Point2 _nodePositionCache;
        private float _sampleLengthCache;
        private int _degreeCache;

        private Point2[]? _controlPositionsCache;
        private Point2[]? _sampledVerticesCache;

        public VectorBSplineSegment()
        {
        }

        public VectorBSplineSegment(
            Point2 position,
            IEnumerable<Point2> controlPoints,
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

            foreach (var point in controlPoints)
                ControlNodes.Add(new VectorNode(point));
        }

        /// <summary>
        /// Gets or sets the degree of the B-spline. A higher degree produces a smoother curve and requires a sufficient number of control points.
        /// </summary>
        public int Degree { get; set; } = 3;

        /// <summary>
        /// Generates a sampled representation of the B-spline between the specified start position and the segment endpoint.
        /// </summary>
        /// <param name="startPosition">The start position of the B-spline segment.</param>
        /// <param name="sampleLength">The desired approximate distance between consecutive sampled vertices.</param>
        /// <returns>An array containing the sampled vertices that represent the B-spline.</returns>
        public override Point2[] GetVertices(
            Point2 startPosition,
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
                return Array.Empty<Point2>();

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

                var points = new Point2[pointCount];

                points[0] = startPosition;

                for (int i = 0; i < ControlNodes.Count; i++)
                {
                    points[i + 1] = ControlNodes[i].Position;
                }

                points[^1] = Node.Position;

                int segments =
                    CalculateSegments(
                        points,
                        sampleLength);

                _sampledVerticesCache =
                    GeometrySampler.SampleBSpline(
                        points.Select(x => new Vector2(x.X,x.Y)).ToArray(),
                        Degree,
                        segments).Select(x => new Point2(x.X,x.Y)).ToArray();

                _controlPositionsCache =
                    new Point2[ControlNodes.Count];

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

        private static int CalculateSegments(
            Point2[] points,
            float sampleLength)
        {
            float length = 0f;

            for (int i = 1;
                 i < points.Length;
                 i++)
            {
                length += Vector2.Distance(
                        points[i - 1].ToVector2(),
                        points[i].ToVector2());
            }

            return Math.Max(
                1,
                (int)MathF.Ceiling(
                    length / sampleLength));
        }
    }
}