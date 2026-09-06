using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Geometry;
using System;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents an elliptical arc segment of a vector path.
    /// </summary>
    public sealed class VectorArcSegment : VectorFixedSegment
    {
        private Point2 _startPositionCache;
        private Point2 _endPositionCache;

        private float _sampleLengthCache;
        private float _radiusXCache;
        private float _radiusYCache;
        private float _rotationCache;

        private bool _largeArcCache;
        private bool _sweepCache;

        private Point2[]? _sampledVerticesCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorArcSegment"/> class.
        /// </summary>
        public VectorArcSegment()
            : base(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorArcSegment"/> class
        /// using the specified endpoint and ellipse parameters.
        /// </summary>
        /// <param name="position">
        /// The endpoint of the elliptical arc segment.
        /// </param>
        /// <param name="radiusX">
        /// The horizontal radius of the ellipse.
        /// </param>
        /// <param name="radiusY">
        /// The vertical radius of the ellipse.
        /// </param>
        /// <param name="rotation">
        /// The rotation of the ellipse in degrees.
        /// </param>
        /// <param name="largeArc">
        /// <see langword="true"/> to use the larger arc between the start and
        /// end positions; otherwise, <see langword="false"/>.
        /// </param>
        /// <param name="sweep">
        /// <see langword="true"/> to sweep the arc in the positive direction;
        /// otherwise, <see langword="false"/>.
        /// </param>
        public VectorArcSegment(
            Point2 position,
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

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorArcSegment"/> class
        /// using the specified endpoint, ellipse parameters, and selection state.
        /// </summary>
        /// <param name="position">
        /// The endpoint of the elliptical arc segment.
        /// </param>
        /// <param name="radiusX">
        /// The horizontal radius of the ellipse.
        /// </param>
        /// <param name="radiusY">
        /// The vertical radius of the ellipse.
        /// </param>
        /// <param name="rotation">
        /// The rotation of the ellipse in degrees.
        /// </param>
        /// <param name="largeArc">
        /// <see langword="true"/> to use the larger arc between the start and
        /// end positions; otherwise, <see langword="false"/>.
        /// </param>
        /// <param name="sweep">
        /// <see langword="true"/> to sweep the arc in the positive direction;
        /// otherwise, <see langword="false"/>.
        /// </param>
        /// <param name="isSelected">
        /// <see langword="true"/> if the endpoint node should initially be selected;
        /// otherwise, <see langword="false"/>.
        /// </param>
        public VectorArcSegment(
            Point2 position,
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

        /// <summary>
        /// Gets or sets the horizontal radius of the elliptical arc.
        /// </summary>
        public float RadiusX { get; set; }

        /// <summary>
        /// Gets or sets the vertical radius of the elliptical arc.
        /// </summary>
        public float RadiusY { get; set; }

        /// <summary>
        /// Gets or sets the rotation of the ellipse in degrees.
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Gets or sets whether the larger elliptical arc is used instead of
        /// the smaller arc between the start and end positions.
        /// </summary>
        public bool LargeArc { get; set; }

        /// <summary>
        /// Gets or sets the direction in which the arc is swept from the
        /// start position to the end position.
        /// </summary>
        public bool Sweep { get; set; }

        /// <summary>
        /// Generates a sampled representation of the elliptical arc between
        /// the specified start position and the segment endpoint.
        /// </summary>
        /// <param name="startPosition">
        /// The start position of the elliptical arc.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// Must be greater than zero.
        /// </param>
        /// <returns>
        /// An array containing the sampled vertices that represent the elliptical arc.
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

            Point2 endPosition = Node.Position;

            if (_sampledVerticesCache != null &&
                _startPositionCache == startPosition &&
                _endPositionCache == endPosition &&
                _sampleLengthCache == sampleLength &&
                _radiusXCache == RadiusX &&
                _radiusYCache == RadiusY &&
                _rotationCache == Rotation &&
                _largeArcCache == LargeArc &&
                _sweepCache == Sweep)
            {
                return _sampledVerticesCache;
            }

            _startPositionCache = startPosition;
            _endPositionCache = endPosition;
            _sampleLengthCache = sampleLength;
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

            return _sampledVerticesCache;
        }

        /// <summary>
        /// Calculates the number of segments required to approximate the
        /// elliptical arc using the specified sampling length.
        /// </summary>
        /// <param name="startPosition">
        /// The start position of the elliptical arc.
        /// </param>
        /// <param name="endPosition">
        /// The end position of the elliptical arc.
        /// </param>
        /// <param name="radiusX">
        /// The horizontal radius of the ellipse.
        /// </param>
        /// <param name="radiusY">
        /// The vertical radius of the ellipse.
        /// </param>
        /// <param name="rotation">
        /// The rotation of the ellipse in degrees.
        /// </param>
        /// <param name="largeArc">
        /// Indicates whether the larger arc is used.
        /// </param>
        /// <param name="sweep">
        /// Indicates the sweep direction of the arc.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive samples.
        /// </param>
        /// <returns>
        /// The number of segments used to sample the arc.
        /// The returned value is always at least one.
        /// </returns>
        private static int CalculateArcSegments(
            Point2 startPosition,
            Point2 endPosition,
            float radiusX,
            float radiusY,
            float rotation,
            bool largeArc,
            bool sweep,
            float sampleLength)
        {
            const int initialSegments = 16;

            Point2[] initialVertices =
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

            for (int i = 1; i < initialVertices.Length; i++)
            {
                length += Point2.Distance(
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