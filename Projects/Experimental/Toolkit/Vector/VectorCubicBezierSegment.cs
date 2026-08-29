using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry;
using System;

namespace Sachssoft.Sasogine.Components.Tools.Vector
{
    /// <summary>Represents a cubic Bézier segment with two control nodes.</summary>
    public sealed class VectorCubicBezierSegment : VectorFixedSegment
    {
        private Vector2 _startPositionCache;
        private float _sampleLengthCache;
        private Vector2 _controlPosition0Cache;
        private Vector2 _controlPosition1Cache;
        private Vector2 _endPositionCache;

        private Vector2[]? _sampledVerticesCache;

        public VectorCubicBezierSegment() : base(2)
        {
        }

        public VectorCubicBezierSegment(
            Vector2 position,
            Vector2 controlPosition0,
            Vector2 controlPosition1)
            : this(
                position,
                controlPosition0,
                controlPosition1,
                false)
        {
        }

        public VectorCubicBezierSegment(
            Vector2 position,
            Vector2 controlPosition0,
            Vector2 controlPosition1,
            bool isSelected)
            : this()
        {
            Node.Position = position;
            ControlNodes[0].Position = controlPosition0;
            ControlNodes[1].Position = controlPosition1;
            Node.IsSelected = isSelected;
        }

        /// <summary>Generates a sampled representation of the cubic Bézier segment between the specified start position and the segment endpoint.</summary>
        /// <param name="startPosition">The start position of the cubic Bézier segment.</param>
        /// <param name="sampleLength">The desired approximate distance between consecutive sampled vertices.</param>
        /// <returns>An array containing the sampled vertices of the cubic Bézier segment.</returns>
        public override Vector2[] GetVertices(
            Vector2 startPosition,
            float sampleLength)
        {
            Vector2 controlPosition0 = ControlNodes[0].Position;
            Vector2 controlPosition1 = ControlNodes[1].Position;
            Vector2 endPosition = Node.Position;

            if (_sampledVerticesCache == null ||
                _startPositionCache != startPosition ||
                _sampleLengthCache != sampleLength ||
                _controlPosition0Cache != controlPosition0 ||
                _controlPosition1Cache != controlPosition1 ||
                _endPositionCache != endPosition)
            {
                _startPositionCache = startPosition;
                _sampleLengthCache = sampleLength;
                _controlPosition0Cache = controlPosition0;
                _controlPosition1Cache = controlPosition1;
                _endPositionCache = endPosition;

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
            }

            return _sampledVerticesCache;
        }

        private static int CalculateCubicBezierSegments(
            Vector2 startPosition,
            Vector2 controlPosition0,
            Vector2 controlPosition1,
            Vector2 endPosition,
            float sampleLength)
        {
            if (sampleLength <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sampleLength));
            }

            float length = Vector2.Distance(startPosition, controlPosition0) +
                Vector2.Distance(controlPosition0, controlPosition1) +
                Vector2.Distance(controlPosition1, endPosition);

            return Math.Max(
                1,
                (int)MathF.Ceiling(
                    length / sampleLength));
        }
    }
}