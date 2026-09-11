using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Geometry;
using System;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a quadratic Bézier segment of a vector path
    /// with a single control node.
    /// </summary>
    public sealed class VectorQuadraticBezierSegment : VectorFixedSegment
    {
        private Point2 _startPositionCache;
        private Point2 _controlPositionCache;
        private Point2 _endPositionCache;
        private float _sampleLengthCache;

        private Point2[]? _sampledVerticesCache;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="VectorQuadraticBezierSegment"/> class.
        /// </summary>
        public VectorQuadraticBezierSegment()
            : base(1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="VectorQuadraticBezierSegment"/> class
        /// using the specified endpoint and control point.
        /// </summary>
        /// <param name="position">
        /// The endpoint of the quadratic Bézier segment.
        /// </param>
        /// <param name="controlPosition">
        /// The control point of the quadratic Bézier segment.
        /// </param>
        public VectorQuadraticBezierSegment(
            Point2 position,
            Point2 controlPosition)
            : this(
                position,
                controlPosition,
                false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="VectorQuadraticBezierSegment"/> class
        /// using the specified endpoint, control point, and selection state.
        /// </summary>
        /// <param name="position">
        /// The endpoint of the quadratic Bézier segment.
        /// </param>
        /// <param name="controlPosition">
        /// The control point of the quadratic Bézier segment.
        /// </param>
        /// <param name="isSelected">
        /// <see langword="true"/> if the endpoint node should initially be selected;
        /// otherwise, <see langword="false"/>.
        /// </param>
        public VectorQuadraticBezierSegment(
            Point2 position,
            Point2 controlPosition,
            bool isSelected)
            : this()
        {
            Node.Position = position;
            Node.IsSelected = isSelected;

            GetControlNodes()[0].Position =
                controlPosition;
        }

        /// <summary>
        /// Generates a sampled representation of the quadratic Bézier segment
        /// between the specified start position and the segment endpoint.
        /// </summary>
        /// <param name="startPosition">
        /// The start position of the quadratic Bézier segment.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// Must be greater than zero.
        /// </param>
        /// <returns>
        /// An array containing the sampled vertices that represent
        /// the quadratic Bézier segment.
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

            Point2 controlPosition =
                GetControlNodes()[0].Position;

            Point2 endPosition =
                Node.Position;

            if (_sampledVerticesCache != null &&
                _startPositionCache == startPosition &&
                _sampleLengthCache == sampleLength &&
                _controlPositionCache == controlPosition &&
                _endPositionCache == endPosition)
            {
                return _sampledVerticesCache;
            }

            _startPositionCache =
                startPosition;

            _sampleLengthCache =
                sampleLength;

            _controlPositionCache =
                controlPosition;

            _endPositionCache =
                endPosition;

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

            return _sampledVerticesCache;
        }

        /// <summary>
        /// Calculates the number of segments required to approximate the
        /// quadratic Bézier curve using the specified sampling length.
        /// </summary>
        /// <param name="startPosition">
        /// The start point of the quadratic Bézier curve.
        /// </param>
        /// <param name="controlPosition">
        /// The control point of the quadratic Bézier curve.
        /// </param>
        /// <param name="endPosition">
        /// The endpoint of the quadratic Bézier curve.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate maximum distance between consecutive samples.
        /// </param>
        /// <returns>
        /// The number of segments used to sample the curve.
        /// The returned value is always at least one.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="sampleLength"/> is less than or equal to zero.
        /// </exception>
        private static int CalculateQuadraticBezierSegments(
            Point2 startPosition,
            Point2 controlPosition,
            Point2 endPosition,
            float sampleLength)
        {
            if (sampleLength <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sampleLength));
            }

            float length =
                Point2.Distance(
                    startPosition,
                    controlPosition) +
                Point2.Distance(
                    controlPosition,
                    endPosition);

            return Math.Max(
                1,
                (int)MathF.Ceiling(
                    length /
                    sampleLength));
        }
    }
}