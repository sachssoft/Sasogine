using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Geometry;
using System;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a cubic Bézier segment with two control nodes.
    /// </summary>
    public sealed class VectorCubicBezierSegment : VectorFixedSegment
    {
        private Point2 _startPositionCache;
        private Point2 _controlPosition0Cache;
        private Point2 _controlPosition1Cache;
        private Point2 _endPositionCache;
        private float _sampleLengthCache;

        private Point2[]? _sampledVerticesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorCubicBezierSegment"/> class.
        /// </summary>
        public VectorCubicBezierSegment()
            : base(2)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorCubicBezierSegment"/> class.
        /// </summary>
        /// <param name="position">
        /// The endpoint of the cubic Bézier segment.
        /// </param>
        /// <param name="controlPosition0">
        /// The first control point of the cubic Bézier segment.
        /// </param>
        /// <param name="controlPosition1">
        /// The second control point of the cubic Bézier segment.
        /// </param>
        public VectorCubicBezierSegment(
            Point2 position,
            Point2 controlPosition0,
            Point2 controlPosition1)
            : this(
                position,
                controlPosition0,
                controlPosition1,
                false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorCubicBezierSegment"/> class.
        /// </summary>
        /// <param name="position">
        /// The endpoint of the cubic Bézier segment.
        /// </param>
        /// <param name="controlPosition0">
        /// The first control point of the cubic Bézier segment.
        /// </param>
        /// <param name="controlPosition1">
        /// The second control point of the cubic Bézier segment.
        /// </param>
        /// <param name="isSelected">
        /// <see langword="true"/> if the endpoint node should initially be selected;
        /// otherwise, <see langword="false"/>.
        /// </param>
        public VectorCubicBezierSegment(
            Point2 position,
            Point2 controlPosition0,
            Point2 controlPosition1,
            bool isSelected)
            : this()
        {
            Node.Position = position;
            Node.IsSelected = isSelected;

            ControlNodes[0].Position =
                controlPosition0;

            ControlNodes[1].Position =
                controlPosition1;
        }

        /// <summary>
        /// Generates a sampled representation of the cubic Bézier segment between
        /// the specified start position and the segment endpoint.
        /// </summary>
        /// <param name="startPosition">
        /// The start position of the cubic Bézier segment.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// Must be greater than zero.
        /// </param>
        /// <returns>
        /// An array containing the sampled vertices of the cubic Bézier segment.
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

            Point2 controlPosition0 =
                ControlNodes[0].Position;

            Point2 controlPosition1 =
                ControlNodes[1].Position;

            Point2 endPosition =
                Node.Position;

            if (_sampledVerticesCache != null &&
                _startPositionCache == startPosition &&
                _sampleLengthCache == sampleLength &&
                _controlPosition0Cache == controlPosition0 &&
                _controlPosition1Cache == controlPosition1 &&
                _endPositionCache == endPosition)
            {
                return _sampledVerticesCache;
            }

            _startPositionCache =
                startPosition;

            _sampleLengthCache =
                sampleLength;

            _controlPosition0Cache =
                controlPosition0;

            _controlPosition1Cache =
                controlPosition1;

            _endPositionCache =
                endPosition;

            int segmentCount =
                CalculateCubicBezierSegments(
                    startPosition,
                    controlPosition0,
                    controlPosition1,
                    endPosition,
                    sampleLength);

            _sampledVerticesCache =
                GeometrySampler.SampleCubicBezier(
                    startPosition,
                    controlPosition0,
                    controlPosition1,
                    endPosition,
                    segmentCount);

            return _sampledVerticesCache;
        }

        /// <summary>
        /// Calculates the number of segments required to approximate the cubic
        /// Bézier curve using the specified sampling length.
        /// </summary>
        /// <param name="startPosition">
        /// The start point of the cubic Bézier curve.
        /// </param>
        /// <param name="controlPosition0">
        /// The first control point of the curve.
        /// </param>
        /// <param name="controlPosition1">
        /// The second control point of the curve.
        /// </param>
        /// <param name="endPosition">
        /// The endpoint of the curve.
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
        private static int CalculateCubicBezierSegments(
            Point2 startPosition,
            Point2 controlPosition0,
            Point2 controlPosition1,
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
                    controlPosition0) +
                Point2.Distance(
                    controlPosition0,
                    controlPosition1) +
                Point2.Distance(
                    controlPosition1,
                    endPosition);

            return Math.Max(
                1,
                (int)MathF.Ceiling(
                    length /
                    sampleLength));
        }
    }
}