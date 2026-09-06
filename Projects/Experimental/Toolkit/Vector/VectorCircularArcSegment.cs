using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Geometry;
using System;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a circular arc segment of a vector path defined by a control point
    /// and an endpoint.
    /// </summary>
    public sealed class VectorCircularArcSegment : VectorFixedSegment
    {
        private const float Epsilon = 0.000001f;

        private Point2 _startPositionCache;
        private Point2 _controlPositionCache;
        private Point2 _endPositionCache;
        private float _sampleLengthCache;

        private Point2[]? _sampledVerticesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorCircularArcSegment"/> class.
        /// </summary>
        public VectorCircularArcSegment()
            : base(1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorCircularArcSegment"/> class
        /// using the specified endpoint and control point.
        /// </summary>
        /// <param name="position">
        /// The endpoint of the circular arc segment.
        /// </param>
        /// <param name="controlPosition">
        /// The control point through which the circular arc passes.
        /// </param>
        public VectorCircularArcSegment(
            Point2 position,
            Point2 controlPosition)
            : this(
                position,
                controlPosition,
                false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorCircularArcSegment"/> class
        /// using the specified endpoint, control point, and selection state.
        /// </summary>
        /// <param name="position">
        /// The endpoint of the circular arc segment.
        /// </param>
        /// <param name="controlPosition">
        /// The control point through which the circular arc passes.
        /// </param>
        /// <param name="isSelected">
        /// <see langword="true"/> if the endpoint node should initially be selected;
        /// otherwise, <see langword="false"/>.
        /// </param>
        public VectorCircularArcSegment(
            Point2 position,
            Point2 controlPosition,
            bool isSelected)
            : this()
        {
            Node.Position = position;
            Node.IsSelected = isSelected;

            ControlNodes[0].Position =
                controlPosition;
        }

        /// <summary>
        /// Generates a sampled representation of the circular arc defined by the
        /// specified start position, the control point, and the segment endpoint.
        /// </summary>
        /// <param name="startPosition">
        /// The start position of the circular arc.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// Must be greater than zero.
        /// </param>
        /// <returns>
        /// An array containing the sampled vertices that represent the circular arc.
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
                ControlNodes[0].Position;

            Point2 endPosition =
                Node.Position;

            if (_sampledVerticesCache != null &&
                _startPositionCache == startPosition &&
                _controlPositionCache == controlPosition &&
                _endPositionCache == endPosition &&
                _sampleLengthCache == sampleLength)
            {
                return _sampledVerticesCache;
            }

            _startPositionCache =
                startPosition;

            _controlPositionCache =
                controlPosition;

            _endPositionCache =
                endPosition;

            _sampleLengthCache =
                sampleLength;

            if (!TryCalculateArc(
                startPosition,
                controlPosition,
                endPosition,
                out _,
                out float radius,
                out _,
                out float sweepAngle))
            {
                _sampledVerticesCache =
                [
                    startPosition,
                    endPosition
                ];

                return _sampledVerticesCache;
            }

            bool sweep =
                sweepAngle > 0f;

            bool largeArc =
                MathF.Abs(sweepAngle) >
                MathF.PI;

            int segmentCount =
                CalculateArcSegments(
                    radius,
                    sweepAngle,
                    sampleLength);

            _sampledVerticesCache =
                GeometrySampler.SampleArc(
                    startPosition,
                    endPosition,
                    radius,
                    radius,
                    0f,
                    largeArc,
                    sweep,
                    segmentCount);

            return _sampledVerticesCache;
        }

        /// <summary>
        /// Calculates the circle passing through the specified start, control,
        /// and end points and determines the corresponding arc parameters.
        /// </summary>
        /// <param name="start">
        /// The start point of the arc.
        /// </param>
        /// <param name="control">
        /// A point through which the arc must pass.
        /// </param>
        /// <param name="end">
        /// The endpoint of the arc.
        /// </param>
        /// <param name="center">
        /// Receives the center point of the calculated circle.
        /// </param>
        /// <param name="radius">
        /// Receives the radius of the calculated circle.
        /// </param>
        /// <param name="startAngle">
        /// Receives the angle of the start point relative to the circle center,
        /// in radians.
        /// </param>
        /// <param name="sweepAngle">
        /// Receives the signed sweep angle from the start point to the endpoint,
        /// in radians. A positive value represents a counter-clockwise sweep and
        /// a negative value represents a clockwise sweep.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if a valid circular arc could be calculated;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        private static bool TryCalculateArc(
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

            float denominator =
                2f *
                (
                    ax * (by - cy) +
                    bx * (cy - ay) +
                    cx * (ay - by)
                );

            if (MathF.Abs(denominator) <= Epsilon)
                return false;

            float aSquared =
                ax * ax +
                ay * ay;

            float bSquared =
                bx * bx +
                by * by;

            float cSquared =
                cx * cx +
                cy * cy;

            float centerX =
                (
                    aSquared * (by - cy) +
                    bSquared * (cy - ay) +
                    cSquared * (ay - by)
                ) /
                denominator;

            float centerY =
                (
                    aSquared * (cx - bx) +
                    bSquared * (ax - cx) +
                    cSquared * (bx - ax)
                ) /
                denominator;

            center =
                new Point2(
                    centerX,
                    centerY);

            radius =
                Point2.Distance(
                    center,
                    start);

            if (radius <= Epsilon)
                return false;

            startAngle =
                MathF.Atan2(
                    start.Y - center.Y,
                    start.X - center.X);

            float controlAngle =
                MathF.Atan2(
                    control.Y - center.Y,
                    control.X - center.X);

            float endAngle =
                MathF.Atan2(
                    end.Y - center.Y,
                    end.X - center.X);

            float counterClockwiseStartToControl =
                NormalizePositiveAngle(
                    controlAngle -
                    startAngle);

            float counterClockwiseStartToEnd =
                NormalizePositiveAngle(
                    endAngle -
                    startAngle);

            if (counterClockwiseStartToControl <=
                counterClockwiseStartToEnd)
            {
                sweepAngle =
                    counterClockwiseStartToEnd;
            }
            else
            {
                sweepAngle =
                    counterClockwiseStartToEnd -
                    MathF.Tau;
            }

            if (MathF.Abs(sweepAngle) <= Epsilon)
                return false;

            return true;
        }

        /// <summary>
        /// Calculates the number of line segments required to approximate an arc
        /// using the specified sampling length.
        /// </summary>
        /// <param name="radius">
        /// The radius of the circular arc.
        /// </param>
        /// <param name="sweepAngle">
        /// The signed sweep angle of the arc in radians.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate maximum distance between consecutive samples.
        /// </param>
        /// <returns>
        /// The number of segments required to approximate the arc.
        /// The returned value is always at least one.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="sampleLength"/> is less than or equal to zero.
        /// </exception>
        private static int CalculateArcSegments(
            float radius,
            float sweepAngle,
            float sampleLength)
        {
            if (sampleLength <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sampleLength));
            }

            float arcLength =
                MathF.Abs(sweepAngle) *
                radius;

            return Math.Max(
                1,
                (int)MathF.Ceiling(
                    arcLength /
                    sampleLength));
        }

        /// <summary>
        /// Normalizes the specified angle to the range from zero inclusive
        /// to <see cref="MathF.Tau"/> exclusive.
        /// </summary>
        /// <param name="angle">
        /// The angle to normalize, in radians.
        /// </param>
        /// <returns>
        /// The normalized angle in the range
        /// <c>[0, 2π)</c>.
        /// </returns>
        private static float NormalizePositiveAngle(
            float angle)
        {
            angle %= MathF.Tau;

            if (angle < 0f)
                angle += MathF.Tau;

            return angle;
        }
    }
}