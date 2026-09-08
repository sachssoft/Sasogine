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
        /// <summary>
        /// Gets the vector paths that define the shape.
        /// </summary>
        public List<VectorPath> Paths { get; } = [];

        /// <summary>
        /// Gets the sampled vertices of all vector paths that define the shape.
        /// </summary>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// </param>
        /// <returns>
        /// A collection containing the sampled vertices of each vector path.
        /// </returns>
        public IReadOnlyList<IReadOnlyList<Point2>> GetVertices(
            float sampleLength)
        {
            var polygons =
                new IReadOnlyList<Point2>[Paths.Count];

            for (int i = 0; i < Paths.Count; i++)
            {
                polygons[i] =
                    Paths[i].GetVertices(
                        sampleLength);
            }

            return polygons;
        }

        /// <summary>
        /// Attempts to calculate the bounds of all sampled vector paths.
        /// </summary>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// </param>
        /// <param name="bounds">
        /// Receives the bounds of the complete vector shape when successful.
        /// </param>
        /// <returns>
        /// <see langword="true"/> when the shape contains at least one vertex;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool TryGetBounds(
            float sampleLength,
            out Bounds2 bounds)
        {
            if (sampleLength <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sampleLength));
            }

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
    }
}