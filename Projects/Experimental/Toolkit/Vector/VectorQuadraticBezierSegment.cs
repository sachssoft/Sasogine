using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Geometry;
using System;
using System.Linq;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>Represents a quadratic Bézier segment of a vector path with a single control node.</summary>
    public sealed class VectorQuadraticBezierSegment : VectorFixedSegment
    {
        private Point2 _startPositionCache;
        private float _sampleLengthCache;
        private Point2 _controlPositionCache;
        private Point2 _endPositionCache;

        private Point2[]? _sampledVerticesCache;

        public VectorQuadraticBezierSegment() : base(1)
        {
        }

        public VectorQuadraticBezierSegment(
            Point2 position,
            Point2 controlPosition)
            : this(position, controlPosition, false)
        {
        }

        public VectorQuadraticBezierSegment(
            Point2 position,
            Point2 controlPosition,
            bool isSelected) : this()
        {
            Node.Position = position;
            ControlNodes[0].Position = controlPosition;
            Node.IsSelected = isSelected;
        }

        /// <summary>Generates a sampled representation of the quadratic Bézier segment between the specified start position and the segment endpoint.</summary>
        /// <param name="startPosition">The start position (P0) of the quadratic Bézier segment.</param>
        /// <param name="sampleLength">The desired approximate distance between consecutive sampled vertices.</param>
        /// <returns>An array containing the sampled vertices that represent the quadratic Bézier segment.</returns>
        public override Point2[] GetVertices(
            Point2 startPosition,
            float sampleLength)
        {
            var controlPosition = ControlNodes[0].Position;
            var endPosition = Node.Position;

            if (_sampledVerticesCache == null ||
                _startPositionCache != startPosition ||
                _sampleLengthCache != sampleLength ||
                _controlPositionCache != controlPosition ||
                _endPositionCache != endPosition)
            {
                _startPositionCache = startPosition;
                _sampleLengthCache = sampleLength;
                _controlPositionCache = controlPosition;
                _endPositionCache = endPosition;

                int segmentCount =
                    CalculateQuadraticBezierSegments(
                        startPosition,
                        controlPosition,
                        endPosition,
                        sampleLength);

                _sampledVerticesCache =
                    GeometrySampler.SampleQuadraticBezier(
                        startPosition.ToVector2(),
                        controlPosition.ToVector2(),
                        endPosition.ToVector2(),
                        segmentCount)
                    .Select(x => new Point2(x.X,x.Y))
                    .ToArray();
            }

            return _sampledVerticesCache;
        }

        private static int CalculateQuadraticBezierSegments(
            Point2 startPosition,
            Point2 controlPosition,
            Point2 endPosition,
            float sampleLength)
        {
            if (sampleLength <= 0f)
                throw new ArgumentOutOfRangeException(nameof(sampleLength));

            float length =
                Vector2.Distance(startPosition.ToVector2(), controlPosition.ToVector2()) +
                Vector2.Distance(controlPosition.ToVector2(), endPosition.ToVector2());

            return Math.Max(
                1,
                (int)MathF.Ceiling(length / sampleLength));
        }
    }
}