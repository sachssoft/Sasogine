using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Vector
{
    /// <summary>Represents a vector path consisting of a start node and a sequence of vector segments.</summary>
    public class VectorPath
    {
        public VectorPath()
        {
            Start = new VectorNode();
        }

        public VectorPath(
            VectorNode start,
            bool isClosed)
        {
            Start = start;
            IsClosed = isClosed;
        }

        /// <summary>Gets or sets the start node of the vector path.</summary>
        public VectorNode Start { get; set; }

        /// <summary>Gets or sets whether the path is closed by connecting its endpoint to the start node.</summary>
        public bool IsClosed { get; set; }

        /// <summary>Gets or sets whether the path is locked and cannot be modified.</summary>
        public bool IsLocked { get; set; }

        /// <summary>Gets or sets whether the path is selected.</summary>
        public bool IsSelected { get; set; }

        /// <summary>Gets the segments that make up the vector path.</summary>
        public List<IVectorSegment> Segments { get; } = [];

        /// <summary>Generates a sampled representation of the complete vector path.</summary>
        /// <param name="sampleLength">The desired approximate distance between consecutive sampled vertices.</param>
        /// <returns>An array containing the sampled vertices of the vector path.</returns>
        public Vector2[] GetVertices(
            float sampleLength)
        {
            var vertices =
                new List<Vector2>();

            Vector2 currentPosition =
                Start.Position;

            foreach (var segment in Segments)
            {
                var segmentVertices =
                    segment.GetVertices(
                        currentPosition,
                        sampleLength);

                vertices.AddRange(
                    segmentVertices);

                if (segmentVertices.Length > 0)
                {
                    currentPosition =
                        segmentVertices[
                            segmentVertices.Length - 1];
                }
            }

            if (IsClosed)
            {
                vertices.Add(
                    Start.Position);
            }

            return vertices.ToArray();
        }
    }
}