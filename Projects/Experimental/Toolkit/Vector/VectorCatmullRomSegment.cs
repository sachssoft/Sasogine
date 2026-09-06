using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a Catmull-Rom spline segment of a vector path defined by a sequence of control nodes.
    /// </summary>
    public sealed class VectorCatmullRomSegment : VectorVariableSegment
    {
        private Point2 _startPositionCache;
        private Point2 _nodePositionCache;
        private float _sampleLengthCache;
        private bool _closedCache;
        private Point2[]? _controlPositionsCache;
        private Point2[]? _sampledVerticesCache;

        public VectorCatmullRomSegment()
        {
        }

        public VectorCatmullRomSegment(
            Point2 position,
            IEnumerable<Point2> controlPoints,
            bool isSelected)
            : this()
        {
            if (controlPoints is null)
                throw new ArgumentNullException(nameof(controlPoints));

            Node.Position = position;
            Node.IsSelected = isSelected;

            foreach (var point in controlPoints)
                ControlNodes.Add(new VectorNode(point));
        }

        /// <summary>
        /// Gets or sets whether the Catmull-Rom spline is closed.
        /// </summary>
        public bool Closed { get; set; }

        /// <summary>
        /// Generates a sampled representation of the Catmull-Rom spline between the specified start position and the segment endpoint.
        /// </summary>
        /// <param name="startPosition">The start position of the Catmull-Rom spline segment.</param>
        /// <param name="sampleLength">The desired approximate distance between consecutive sampled vertices.</param>
        /// <returns>An array containing the sampled vertices that represent the Catmull-Rom spline.</returns>
        public override Point2[] GetVertices(
            Point2 startPosition,
            float sampleLength)
        {
            if (sampleLength <= 0f)
                throw new ArgumentOutOfRangeException(nameof(sampleLength));

            int pointCount = ControlNodes.Count + 2;

            if (pointCount < 2)
                return Array.Empty<Point2>();

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

                var points = new Point2[pointCount];
                points[0] = startPosition;

                for (int i = 0; i < ControlNodes.Count; i++)
                    points[i + 1] = ControlNodes[i].Position;

                points[^1] = Node.Position;

                int segmentsPerSpan =
                    CalculateSegments(points, sampleLength);

                _sampledVerticesCache =
                    GeometrySampler.SampleCatmullRom(
                        points.Select(x => new Vector2(x.X, x.Y)).ToArray(),
                        segmentsPerSpan,
                        Closed)
                    .Select(x => new Point2(x.X,x.Y))
                    .ToArray();

                _controlPositionsCache =
                    new Point2[ControlNodes.Count];

                for (int i = 0; i < ControlNodes.Count; i++)
                    _controlPositionsCache[i] =
                        ControlNodes[i].Position;
            }

            return _sampledVerticesCache;
        }

        private static int CalculateSegments(
            Point2[] points,
            float sampleLength)
        {
            float length = 0f;

            for (int i = 1; i < points.Length; i++)
                length += Vector2.Distance(
                    points[i - 1].ToVector2(),
                    points[i].ToVector2());

            return Math.Max(
                1,
                (int)MathF.Ceiling(length / sampleLength));
        }
    }
}