using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry;
using System;

namespace Sachssoft.Sasogine.Components.Tools.Vector
{
    /// <summary>Represents a quadratic Bézier segment of a vector path with a single control node.</summary>
    public sealed class VectorQuadraticBezierSegment : VectorFixedSegment
    {
        private Vector2 _startPositionCache;
        private float _sampleLengthCache;
        private Vector2 _controlPositionCache;
        private Vector2 _endPositionCache;

        private Vector2[]? _sampledVerticesCache;

        public VectorQuadraticBezierSegment() : base(1)
        {
        }

        public VectorQuadraticBezierSegment(
            Vector2 position,
            Vector2 controlPosition)
            : this(position, controlPosition, false)
        {
        }

        public VectorQuadraticBezierSegment(
            Vector2 position,
            Vector2 controlPosition,
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
        public override Vector2[] GetVertices(
            Vector2 startPosition,
            float sampleLength)
        {
            Vector2 controlPosition = ControlNodes[0].Position;
            Vector2 endPosition = Node.Position;

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
                        startPosition,
                        controlPosition,
                        endPosition,
                        segmentCount);
            }

            return _sampledVerticesCache;
        }

        private static int CalculateQuadraticBezierSegments(
            Vector2 startPosition,
            Vector2 controlPosition,
            Vector2 endPosition,
            float sampleLength)
        {
            if (sampleLength <= 0f)
                throw new ArgumentOutOfRangeException(nameof(sampleLength));

            float length =
                Vector2.Distance(startPosition, controlPosition) +
                Vector2.Distance(controlPosition, endPosition);

            return Math.Max(
                1,
                (int)MathF.Ceiling(length / sampleLength));
        }
    }
}