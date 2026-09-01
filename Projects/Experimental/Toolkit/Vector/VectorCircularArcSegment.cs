using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry;
using System;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>Represents a circular arc segment defined by a control point and an endpoint.</summary>
    public sealed class VectorCircularArcSegment : VectorFixedSegment
    {
        private const float Epsilon = 0.000001f;

        private Vector2 _startPositionCache;
        private Vector2 _controlPositionCache;
        private Vector2 _endPositionCache;
        private float _sampleLengthCache;

        private Vector2[]? _sampledVerticesCache;

        public VectorCircularArcSegment() : base(1)
        {
        }

        public VectorCircularArcSegment(
            Vector2 position,
            Vector2 controlPosition)
            : this(
                position,
                controlPosition,
                false)
        {
        }

        public VectorCircularArcSegment(
            Vector2 position,
            Vector2 controlPosition,
            bool isSelected)
            : this()
        {
            Node.Position = position;
            Node.IsSelected = isSelected;

            ControlNodes[0].Position =
                controlPosition;
        }

        /// <summary>Generates a sampled representation of the circular arc defined by the start position, control position, and segment endpoint.</summary>
        /// <param name="startPosition">The start position of the circular arc.</param>
        /// <param name="sampleLength">The desired approximate distance between consecutive sampled vertices.</param>
        /// <returns>An array containing the sampled vertices that represent the circular arc.</returns>
        public override Vector2[] GetVertices(
            Vector2 startPosition,
            float sampleLength)
        {
            if (sampleLength <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sampleLength));
            }

            Vector2 controlPosition =
                ControlNodes[0].Position;

            Vector2 endPosition =
                Node.Position;

            if (_sampledVerticesCache == null ||
                _startPositionCache != startPosition ||
                _controlPositionCache != controlPosition ||
                _endPositionCache != endPosition ||
                _sampleLengthCache != sampleLength)
            {
                _startPositionCache = startPosition;
                _controlPositionCache = controlPosition;
                _endPositionCache = endPosition;
                _sampleLengthCache = sampleLength;

                if (!TryCalculateArc(
                    startPosition,
                    controlPosition,
                    endPosition,
                    out Vector2 center,
                    out float radius,
                    out float startAngle,
                    out float sweepAngle))
                {
                    // Drei kollineare Punkte können keinen
                    // eindeutigen Kreisbogen bilden.
                    _sampledVerticesCache =
                    [
                        startPosition,
                        endPosition
                    ];

                    return _sampledVerticesCache;
                }

                bool sweep = sweepAngle > 0f;
                bool largeArc = MathF.Abs(sweepAngle) > MathF.PI;

                int segmentCount =
                    CalculateArcSegments(
                        center,
                        radius,
                        sweepAngle,
                        sampleLength);

                /*
                 * GeometrySampler.SampleArc erwartet
                 * Start/End + Radius + Rotation.
                 *
                 * Da es sich hier um einen Kreis handelt,
                 * sind RadiusX und RadiusY identisch.
                 */
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
            }

            return _sampledVerticesCache;
        }

        private static bool TryCalculateArc(
            Vector2 start,
            Vector2 control,
            Vector2 end,
            out Vector2 center,
            out float radius,
            out float startAngle,
            out float sweepAngle)
        {
            center = Vector2.Zero;
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
            {
                return false;
            }

            float aSquared = ax * ax + ay * ay;
            float bSquared = bx * bx + by * by;
            float cSquared = cx * cx + cy * cy;

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

            center = new Vector2(centerX, centerY);
            radius = Vector2.Distance(center, start);

            if (radius <= Epsilon)
            {
                return false;
            }

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
                    controlAngle - startAngle);

            float counterClockwiseStartToEnd =
                NormalizePositiveAngle(
                    endAngle - startAngle);

            /*
             * Determine whether the control point lies on
             * the counter-clockwise or clockwise arc.
             */
            if (counterClockwiseStartToControl <=
                counterClockwiseStartToEnd)
            {
                sweepAngle = counterClockwiseStartToEnd;
            }
            else
            {
                sweepAngle =
                    counterClockwiseStartToEnd -
                    MathF.Tau;
            }

            /*
             * Exact 0° / 360° cases are not useful for
             * a three-point arc.
             */
            if (MathF.Abs(sweepAngle) <= Epsilon)
            {
                return false;
            }

            return true;
        }

        private static int CalculateArcSegments(
            Vector2 center,
            float radius,
            float sweepAngle,
            float sampleLength)
        {
            if (sampleLength <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sampleLength));
            }

            /*
             * Arc length:
             *
             *     length = radius * angle
             *
             * The result is rounded upward so that the
             * requested sample length is not exceeded.
             */
            float arcLength =
                MathF.Abs(sweepAngle) *
                radius;

            return Math.Max(
                1,
                (int)MathF.Ceiling(
                    arcLength /
                    sampleLength));
        }

        private static float NormalizePositiveAngle(
            float angle)
        {
            while (angle < 0f)
            {
                angle += MathF.Tau;
            }

            while (angle >= MathF.Tau)
            {
                angle -= MathF.Tau;
            }

            return angle;
        }
    }
}