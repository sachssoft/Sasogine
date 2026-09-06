using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Geometry;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a B-spline segment of a vector path defined by a degree
    /// and a sequence of control nodes.
    /// </summary>
    public sealed class VectorBSplineSegment : VectorVariableSegment
    {
        private Point2 _startPositionCache;
        private Point2 _nodePositionCache;

        private float _sampleLengthCache;
        private int _degreeCache;

        private Point2[]? _controlPositionsCache;
        private Point2[]? _sampledVerticesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorBSplineSegment"/> class.
        /// </summary>
        public VectorBSplineSegment()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorBSplineSegment"/> class
        /// using the specified endpoint, control points, degree, and selection state.
        /// </summary>
        /// <param name="position">
        /// The endpoint of the B-spline segment.
        /// </param>
        /// <param name="controlPoints">
        /// The control points that define the shape of the B-spline.
        /// </param>
        /// <param name="degree">
        /// The degree of the B-spline.
        /// </param>
        /// <param name="isSelected">
        /// <see langword="true"/> if the endpoint node should initially be selected;
        /// otherwise, <see langword="false"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="controlPoints"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="degree"/> is less than one.
        /// </exception>
        public VectorBSplineSegment(
            Point2 position,
            IEnumerable<Point2> controlPoints,
            int degree = 3,
            bool isSelected = false)
            : this()
        {
            ArgumentNullException.ThrowIfNull(
                controlPoints);

            if (degree < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(degree));
            }

            Node.Position = position;
            Node.IsSelected = isSelected;
            Degree = degree;

            foreach (var point in controlPoints)
                ControlNodes.Add(new VectorNode(point));
        }

        /// <summary>
        /// Gets or sets the degree of the B-spline.
        /// </summary>
        /// <remarks>
        /// A higher degree produces a smoother curve and requires a sufficient
        /// number of control points.
        /// </remarks>
        public int Degree { get; set; } = 3;

        /// <summary>
        /// Generates a sampled representation of the B-spline between the
        /// specified start position and the segment endpoint.
        /// </summary>
        /// <param name="startPosition">
        /// The start position of the B-spline segment.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// Must be greater than zero.
        /// </param>
        /// <returns>
        /// An array containing the sampled vertices that represent the B-spline.
        /// Returns an empty array when the configured degree is not valid for
        /// the current number of points.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="sampleLength"/> is less than or equal to zero.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <see cref="Degree"/> is less than one.
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

            if (Degree < 1)
            {
                throw new InvalidOperationException(
                    "Degree must be greater than zero.");
            }

            int controlCount =
                ControlNodes.Count;

            int pointCount =
                controlCount + 2;

            if (Degree >= pointCount)
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
                _degreeCache == Degree &&
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

            _degreeCache =
                Degree;

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

            int segments =
                CalculateSegments(
                    points,
                    sampleLength);

            _sampledVerticesCache =
                GeometrySampler.SampleBSpline(
                    points,
                    Degree,
                    segments);

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
        /// Calculates the number of segments used to sample the B-spline
        /// based on the approximate length of its control polygon.
        /// </summary>
        /// <param name="points">
        /// The points defining the B-spline control polygon.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// </param>
        /// <returns>
        /// The number of segments used to sample the B-spline.
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