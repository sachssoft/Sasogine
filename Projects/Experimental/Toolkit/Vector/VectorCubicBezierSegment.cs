using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Geometry;
using System;
using System.Linq;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>Represents a cubic Bézier segment with two control nodes.</summary>
    public sealed class VectorCubicBezierSegment : VectorFixedSegment
    {
        private Point2 _startPositionCache;
        private float _sampleLengthCache;
        private Point2 _controlPosition0Cache;
        private Point2 _controlPosition1Cache;
        private Point2 _endPositionCache;

        private Point2[]? _sampledVerticesCache;

        public VectorCubicBezierSegment() : base(2)
        {
        }

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

        public VectorCubicBezierSegment(
            Point2 position,
            Point2 controlPosition0,
            Point2 controlPosition1,
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
        public override Point2[] GetVertices(
            Point2 startPosition,
            float sampleLength)
        {
            var controlPosition0 = ControlNodes[0].Position;
            var controlPosition1 = ControlNodes[1].Position;
            var endPosition = Node.Position;

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
                        startPosition.ToVector2(),
                        controlPosition0.ToVector2(),
                        controlPosition1.ToVector2(),
                        endPosition.ToVector2(),
                        segmentCount)
                    .Select(x => new Point2(x.X,x.Y))
                    .ToArray();
            }

            return _sampledVerticesCache;
        }

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

            float length = Vector2.Distance(startPosition.ToVector2(), controlPosition0.ToVector2()) +
                Vector2.Distance(controlPosition0.ToVector2(), controlPosition1.ToVector2()) +
                Vector2.Distance(controlPosition1.ToVector2(), endPosition.ToVector2());

            return Math.Max(
                1,
                (int)MathF.Ceiling(
                    length / sampleLength));
        }
    }
}