using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a vector segment with a fixed number of control nodes.
    /// </summary>
    public abstract class VectorFixedSegment : IVectorSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorFixedSegment"/> class
        /// with the specified number of control nodes.
        /// </summary>
        /// <param name="controlCount">
        /// The number of control nodes to create for the segment.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="controlCount"/> is less than zero.
        /// </exception>
        protected VectorFixedSegment(
            int controlCount)
        {
            if (controlCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(controlCount));
            }

            Node = new VectorNode();

            var controlNodes =
                new VectorNode[controlCount];

            for (int i = 0; i < controlCount; i++)
                controlNodes[i] = new VectorNode();

            ControlNodes = controlNodes;
        }

        /// <summary>
        /// Gets the endpoint node of the vector segment.
        /// </summary>
        public VectorNode Node { get; }

        /// <summary>
        /// Gets the fixed collection of control nodes used by the vector segment.
        /// </summary>
        public IReadOnlyList<VectorNode> ControlNodes { get; }

        /// <summary>
        /// Generates a sampled representation of the vector segment.
        /// </summary>
        /// <param name="startPosition">
        /// The start position of the vector segment.
        /// </param>
        /// <param name="sampleLength">
        /// The desired approximate distance between consecutive sampled vertices.
        /// </param>
        /// <returns>
        /// An array containing the sampled vertices of the vector segment.
        /// </returns>
        public abstract Point2[] GetVertices(
            Point2 startPosition,
            float sampleLength);
    }
}