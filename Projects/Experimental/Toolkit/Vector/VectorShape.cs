using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a vector shape containing a collection of vector paths.
    /// </summary>
    public class VectorShape
    {
        private const float DefaultSampleLength = 4f;

        private readonly ITransform2? _source;
        private readonly Transform2State? _transformState;

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorShape"/> class.
        /// </summary>
        public VectorShape()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorShape"/> class
        /// using the specified transform source.
        /// </summary>
        /// <param name="source">
        /// The transform source used by the shape.
        /// </param>
        public VectorShape(ITransform2? source)
        {
            _source = source;

            if (source != null)
                _transformState = new Transform2State(source);
        }

        /// <summary>
        /// Gets the vector paths that define the shape.
        /// </summary>
        public List<VectorPath> Paths { get; } = [];

        /// <summary>
        /// Gets or sets whether the shape is locked and cannot be modified.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Gets the transform source associated with the shape.
        /// </summary>
        public ITransform2? TransformSource => _source;

        /// <summary>
        /// Gets a value indicating whether the vector geometry has changed.
        /// </summary>
        public bool IsChanged { get; private set; }

        /// <summary>
        /// Occurs when the vector geometry changes.
        /// </summary>
        public event EventHandler? Changed;

        /// <summary>
        /// Gets the sampled point vertices of all vector paths.
        /// </summary>
        /// <param name="valueMode">
        /// Specifies whether the returned vertices use absolute or relative coordinates.
        /// </param>
        /// <param name="trim">
        /// Specifies whether relative vertices are trimmed to the bounds of the shape.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// </param>
        /// <returns>
        /// A collection containing the sampled point vertices of each vector path.
        /// </returns>
        public IReadOnlyList<IReadOnlyList<Point2>> GetPointVertices(
            ValueMode valueMode = ValueMode.Absolute,
            bool trim = true,
            float sampleLength = DefaultSampleLength)
        {
            if (sampleLength <= 0f)
                throw new ArgumentOutOfRangeException(nameof(sampleLength));

            var polygons = new IReadOnlyList<Point2>[Paths.Count];

            for (int i = 0; i < Paths.Count; i++)
                polygons[i] = Paths[i].GetVertices(sampleLength);

            if (valueMode == ValueMode.Absolute)
                return polygons;

            if (!TryGetBounds(out Bounds2 bounds, sampleLength))
                return polygons;

            float width = bounds.Size.Width;
            float height = bounds.Size.Height;

            for (int i = 0; i < polygons.Length; i++)
            {
                IReadOnlyList<Point2> points = polygons[i];
                var relative = new Point2[points.Count];

                for (int j = 0; j < points.Count; j++)
                {
                    Point2 point = points[j];

                    relative[j] = new Point2(
                        width != 0f ? (point.X - (trim ? bounds.X : 0f)) / width : 0f,
                        height != 0f ? (point.Y - (trim ? bounds.Y : 0f)) / height : 0f);
                }

                polygons[i] = relative;
            }

            return polygons;
        }

        /// <summary>
        /// Gets the sampled vector vertices of all vector paths.
        /// </summary>
        /// <param name="valueMode">
        /// Specifies whether the returned vertices use absolute or relative coordinates.
        /// </param>
        /// <param name="trim">
        /// Specifies whether relative vertices are trimmed to the bounds of the shape.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// </param>
        /// <returns>
        /// A collection containing the sampled vector vertices of each vector path.
        /// </returns>
        public IReadOnlyList<IReadOnlyList<Vector2>> GetVectorVertices(
            ValueMode valueMode = ValueMode.Absolute,
            bool trim = true,
            float sampleLength = DefaultSampleLength)
        {
            IReadOnlyList<IReadOnlyList<Point2>> pointPolygons =
                GetPointVertices(valueMode, trim, sampleLength);

            var polygons = new IReadOnlyList<Vector2>[pointPolygons.Count];

            for (int i = 0; i < pointPolygons.Count; i++)
            {
                IReadOnlyList<Point2> points = pointPolygons[i];
                var vectors = new Vector2[points.Count];

                for (int j = 0; j < points.Count; j++)
                    vectors[j] = new Vector2(points[j].X, points[j].Y);

                polygons[i] = vectors;
            }

            return polygons;
        }

        /// <summary>
        /// Attempts to calculate the bounds of all sampled vector paths.
        /// </summary>
        /// <param name="bounds">
        /// Receives the bounds of the complete vector shape when successful.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when the shape contains at least one vertex;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryGetBounds(
            out Bounds2 bounds,
            float sampleLength = DefaultSampleLength)
        {
            if (sampleLength <= 0f)
                throw new ArgumentOutOfRangeException(nameof(sampleLength));

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            bool hasVertex = false;

            for (int i = 0; i < Paths.Count; i++)
            {
                Point2[] vertices = Paths[i].GetVertices(sampleLength);

                for (int j = 0; j < vertices.Length; j++)
                {
                    Point2 vertex = vertices[j];

                    minX = MathF.Min(minX, vertex.X);
                    minY = MathF.Min(minY, vertex.Y);
                    maxX = MathF.Max(maxX, vertex.X);
                    maxY = MathF.Max(maxY, vertex.Y);
                    hasVertex = true;
                }
            }

            if (!hasVertex)
            {
                bounds = default;
                return false;
            }

            bounds = new Bounds2(
                new Point2(minX, minY),
                new Size2(maxX - minX, maxY - minY));

            return true;
        }

        /// <summary>
        /// Applies changes from the transform source to the vector geometry.
        /// </summary>
        public void ApplyTransform()
        {
            if (_source == null ||
                _transformState == null ||
                !_transformState.IsChanged)
            {
                return;
            }

            Matrix oldTransform = CreateTransformMatrix(
                _transformState.Position,
                _transformState.Size,
                _transformState.Scale,
                _transformState.Rotation,
                _transformState.RotationPivot,
                _transformState.Skew);

            Matrix newTransform = CreateTransformMatrix(
                GetPosition(_source),
                GetSize(_source),
                GetScale(_source),
                GetRotation(_source),
                GetRotationPivot(_source),
                GetSkew(_source));

            float determinant = oldTransform.Determinant();

            if (MathF.Abs(determinant) > 1e-6f)
            {
                Matrix deltaTransform =
                    Matrix.Invert(oldTransform) *
                    newTransform;

                ApplyTransform(deltaTransform);
            }

            _transformState.UpdateState();
            NotifyChanged();
        }

        /// <summary>
        /// Resets the changed state of the vector shape.
        /// </summary>
        public void UpdateState()
        {
            IsChanged = false;
        }

        internal void NotifyChanged()
        {
            if (IsChanged)
                return;

            IsChanged = true;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyTransform(Matrix transform)
        {
            var nodes = new HashSet<VectorNode>();

            for (int pathIndex = 0; pathIndex < Paths.Count; pathIndex++)
            {
                VectorPath path = Paths[pathIndex];
                nodes.Add(path.Start);

                for (int segmentIndex = 0;
                     segmentIndex < path.Segments.Count;
                     segmentIndex++)
                {
                    IVectorSegment segment = path.Segments[segmentIndex];
                    nodes.Add(segment.Node);

                    for (int controlIndex = 0;
                         controlIndex < segment.ControlNodes.Count;
                         controlIndex++)
                    {
                        nodes.Add(segment.ControlNodes[controlIndex]);
                    }
                }
            }

            foreach (VectorNode node in nodes)
            {
                Vector2 position = Vector2.Transform(
                    new Vector2(
                        node.Position.X,
                        node.Position.Y),
                    transform);

                node.Position = new Point2(
                    position.X,
                    position.Y);
            }
        }

        private Matrix CreateTransformMatrix(
            Point2 position,
            Size2 size,
            Vector2 scale,
            float rotation,
            Point2 rotationPivot,
            Vector2 skew)
        {
            var matrix = Matrix.Identity;
            var actualSize = Vector2.One;

            if (_source is IReadOnlyTransformSize2)
            {
                matrix *= Matrix.CreateScale(size.Width, size.Height, 1f);
                actualSize = size.ToVector2();
            }

            if (_source is IReadOnlyTransformScale2)
            {
                matrix *= Matrix.CreateScale(scale.X, scale.Y, 1f);
                actualSize *= scale;
            }

            if (_source is IReadOnlyTransformSkew2)
            {
                var skewMatrix = Matrix.Identity;
                skewMatrix.M21 = skew.X;
                skewMatrix.M12 = skew.Y;
                matrix *= skewMatrix;
            }

            if (_source is IReadOnlyTransformRotation2)
            {
                Vector2 pivot = _source is IReadOnlyTransformRotationPivot2
                    ? actualSize * rotationPivot.ToVector2()
                    : Vector2.Zero;

                matrix *= Matrix.CreateTranslation(-pivot.X, -pivot.Y, 0f);
                matrix *= Matrix.CreateRotationZ(rotation);
                matrix *= Matrix.CreateTranslation(pivot.X, pivot.Y, 0f);
            }

            if (_source is IReadOnlyTransformPosition2)
                matrix *= Matrix.CreateTranslation(position.X, position.Y, 0f);

            return matrix;
        }

        private static Point2 GetPosition(ITransform2 source)
        {
            return source is IReadOnlyTransformPosition2 position
                ? position.Position
                : Point2.Zero;
        }

        private static Size2 GetSize(ITransform2 source)
        {
            return source is IReadOnlyTransformSize2 size
                ? size.Size
                : new Size2(1f);
        }

        private static Vector2 GetScale(ITransform2 source)
        {
            return source is IReadOnlyTransformScale2 scale
                ? scale.Scale
                : Vector2.One;
        }

        private static float GetRotation(ITransform2 source)
        {
            return source is IReadOnlyTransformRotation2 rotation
                ? rotation.Rotation
                : 0f;
        }

        private static Point2 GetRotationPivot(ITransform2 source)
        {
            return source is IReadOnlyTransformRotationPivot2 pivot
                ? pivot.RotationPivot
                : Point2.Zero;
        }

        private static Vector2 GetSkew(ITransform2 source)
        {
            return source is IReadOnlyTransformSkew2 skew
                ? skew.Skew
                : Vector2.Zero;
        }
    }
}