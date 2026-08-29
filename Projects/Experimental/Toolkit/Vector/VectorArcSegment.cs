using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Geometry;
using System;

namespace Sachssoft.Sasogine.Components.Tools.Vector
{
    /// <summary>Represents an elliptical arc segment of a vector path.</summary>
    public sealed class VectorArcSegment : VectorFixedSegment
    {
        private Vector2 _startPositionCache;
        private float _sampleLengthCache;

        private Vector2 _endPositionCache;

        private float _radiusXCache;
        private float _radiusYCache;
        private float _rotationCache;

        private bool _largeArcCache;
        private bool _sweepCache;

        private Vector2[]? _sampledVerticesCache;

        public VectorArcSegment() : base(0)
        {
        }

        public VectorArcSegment(
            Vector2 position,
            float radiusX,
            float radiusY,
            float rotation = 0f,
            bool largeArc = false,
            bool sweep = true)
            : this(
                position,
                radiusX,
                radiusY,
                rotation,
                largeArc,
                sweep,
                false)
        {
        }

        public VectorArcSegment(
            Vector2 position,
            float radiusX,
            float radiusY,
            float rotation,
            bool largeArc,
            bool sweep,
            bool isSelected)
            : this()
        {
            Node.Position = position;
            Node.IsSelected = isSelected;

            RadiusX = radiusX;
            RadiusY = radiusY;
            Rotation = rotation;
            LargeArc = largeArc;
            Sweep = sweep;
        }

        /// <summary>Gets or sets the X radius of the elliptical arc.</summary>
        public float RadiusX { get; set; }

        /// <summary>Gets or sets the Y radius of the elliptical arc.</summary>
        public float RadiusY { get; set; }

        /// <summary>Gets or sets the rotation of the ellipse in degrees.</summary>
        public float Rotation { get; set; }

        /// <summary>Gets or sets whether the larger elliptical arc is used instead of the smaller arc between the start and end positions.</summary>
        public bool LargeArc { get; set; }

        /// <summary>Gets or sets the direction in which the arc is swept from the start position to the end position.</summary>
        public bool Sweep { get; set; }

        /// <summary>Generates a sampled representation of the elliptical arc between the specified start position and the segment endpoint.</summary>
        /// <param name="startPosition">The start position of the elliptical arc.</param>
        /// <param name="sampleLength">The desired approximate distance between consecutive sampled vertices.</param>
        /// <returns>An array containing the sampled vertices that represent the elliptical arc.</returns>
        public override Vector2[] GetVertices(
            Vector2 startPosition,
            float sampleLength)
        {
            Vector2 endPosition =
                Node.Position;

            if (sampleLength <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sampleLength));
            }

            if (_sampledVerticesCache == null ||
                _startPositionCache != startPosition ||
                _sampleLengthCache != sampleLength ||
                _endPositionCache != endPosition ||
                _radiusXCache != RadiusX ||
                _radiusYCache != RadiusY ||
                _rotationCache != Rotation ||
                _largeArcCache != LargeArc ||
                _sweepCache != Sweep)
            {
                _startPositionCache = startPosition;
                _sampleLengthCache = sampleLength;
                _endPositionCache = endPosition;
                _radiusXCache = RadiusX;
                _radiusYCache = RadiusY;
                _rotationCache = Rotation;
                _largeArcCache = LargeArc;
                _sweepCache = Sweep;

                int segmentCount =
                    CalculateArcSegments(
                        startPosition,
                        endPosition,
                        RadiusX,
                        RadiusY,
                        Rotation,
                        LargeArc,
                        Sweep,
                        sampleLength);

                _sampledVerticesCache =
                    GeometrySampler.SampleArc(
                        startPosition,
                        endPosition,
                        RadiusX,
                        RadiusY,
                        Rotation,
                        LargeArc,
                        Sweep,
                        segmentCount);
            }

            return _sampledVerticesCache;
        }

        private static int CalculateArcSegments(
            Vector2 startPosition,
            Vector2 endPosition,
            float radiusX,
            float radiusY,
            float rotation,
            bool largeArc,
            bool sweep,
            float sampleLength)
        {
            if (sampleLength <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sampleLength));
            }

            /*
             * Use GeometrySampler itself to obtain an initial
             * approximation of the arc. This keeps all arc
             * geometry in GeometrySampler.
             */
            const int initialSegments = 16;

            Vector2[] initialVertices =
                GeometrySampler.SampleArc(
                    startPosition,
                    endPosition,
                    radiusX,
                    radiusY,
                    rotation,
                    largeArc,
                    sweep,
                    initialSegments);

            float length = 0f;

            for (int i = 1;
                 i < initialVertices.Length;
                 i++)
            {
                length +=
                    Vector2.Distance(
                        initialVertices[i - 1],
                        initialVertices[i]);
            }

            return Math.Max(
                1,
                (int)MathF.Ceiling(
                    length / sampleLength));
        }
    }
}