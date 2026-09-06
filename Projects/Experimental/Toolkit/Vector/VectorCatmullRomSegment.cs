using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Geometry;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a Catmull-Rom spline segment of a vector path defined by
    /// a sequence of control nodes.
    /// </summary>
    public sealed class VectorCatmullRomSegment : VectorVariableSegment
    {
        private Point2 _startPositionCache;
        private Point2 _nodePositionCache;

        private float _sampleLengthCache;
        private bool _closedCache;

        private Point2[]? _controlPositionsCache;
        private Point2[]? _sampledVerticesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorCatmullRomSegment"/> class.
        /// </summary>
        public VectorCatmullRomSegment()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorCatmullRomSegment"/> class
        /// using the specified endpoint, control points, and selection state.
        /// </summary>
        /// <param name="position">
        /// The endpoint of the Catmull-Rom spline segment.
        /// </param>
        /// <param name="controlPoints">
        /// The control points that define the shape of the Catmull-Rom spline.
        /// </param>
        /// <param name="isSelected">
        /// <see langword="true"/> if the endpoint node should initially be selected;
        /// otherwise, <see langword="false"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="controlPoints"/> is <see langword="null"/>.
        /// </exception>
        public VectorCatmullRomSegment(
            Point2 position,
            IEnumerable<Point2> controlPoints,
            bool isSelected)
            : this()
        {
            ArgumentNullException.ThrowIfNull(
                controlPoints);

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
        /// Generates a sampled representation of the Catmull-Rom spline
        /// between the specified start position and the segment endpoint.
        /// </summary>
        /// <param name="startPosition">
        /// The start position of the Catmull-Rom spline segment.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// Must be greater than zero.
        /// </param>
        /// <returns>
        /// An array containing the sampled vertices that represent
        /// the Catmull-Rom spline.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="sampleLength"/> is less than or equal to zero.
        /// </exception>
        public override Point2[] GetVertices(
            Point2 startPosition,
            float sampleLength)
        {
            if (sampleLength <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sampleLength));
            }

            int controlCount =
                ControlNodes.Count;

            int pointCount =
                controlCount + 2;

            if (pointCount < 2)
                return Array.Empty<Point2>();

            bool controlPointsChanged =
                _controlPositionsCache == null ||
                _controlPositionsCache.Length != controlCount ||
                _nodePositionCache != Node.Position;

            if (!controlPointsChanged)
            {
                for (int i = 0; i < controlCount; i++)
                {
                    if (_controlPositionsCache![i] !=
                        ControlNodes[i].Position)
                    {
                        controlPointsChanged = true;
                        break;
                    }
                }
            }

            if (_sampledVerticesCache != null &&
                _startPositionCache == startPosition &&
                _sampleLengthCache == sampleLength &&
                _closedCache == Closed &&
                !controlPointsChanged)
            {
                return _sampledVerticesCache;
            }

            _startPositionCache =
                startPosition;

            _nodePositionCache =
                Node.Position;

            _sampleLengthCache =
                sampleLength;

            _closedCache =
                Closed;

            var points =
                new Point2[pointCount];

            points[0] =
                startPosition;

            for (int i = 0; i < controlCount; i++)
            {
                points[i + 1] =
                    ControlNodes[i].Position;
            }

            points[^1] =
                Node.Position;

            int segmentsPerSpan =
                CalculateSegments(
                    points,
                    sampleLength);

            _sampledVerticesCache =
                GeometrySampler.SampleCatmullRom(
                    points,
                    segmentsPerSpan,
                    Closed);

            if (_controlPositionsCache == null ||
                _controlPositionsCache.Length != controlCount)
            {
                _controlPositionsCache =
                    new Point2[controlCount];
            }

            for (int i = 0; i < controlCount; i++)
            {
                _controlPositionsCache[i] =
                    ControlNodes[i].Position;
            }

            return _sampledVerticesCache;
        }

        /// <summary>
        /// Calculates the number of segments used to sample each spline span
        /// based on the approximate length of the control polygon.
        /// </summary>
        /// <param name="points">
        /// The points defining the Catmull-Rom spline.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// </param>
        /// <returns>
        /// The number of segments used to sample each spline span.
        /// The returned value is always at least one.
        /// </returns>
        private static int CalculateSegments(
            Point2[] points,
            float sampleLength)
        {
            float length = 0f;

            for (int i = 1; i < points.Length; i++)
            {
                length += Point2.Distance(
                    points[i - 1],
                    points[i]);
            }

            return Math.Max(
                1,
                (int)MathF.Ceiling(
                    length /
                    sampleLength));
        }
    }
}