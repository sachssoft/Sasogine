using Sachssoft.Sasogine.Common;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Defines a vector path segment with an endpoint and optional control nodes.
    /// </summary>
    public interface IVectorSegment
    {
        /// <summary>
        /// Gets the endpoint node of the vector segment.
        /// </summary>
        VectorNode Node { get; }

        /// <summary>
        /// Gets the control nodes used to define the shape of the vector segment.
        /// </summary>
        IReadOnlyList<VectorNode> ControlNodes { get; }

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
        Point2[] GetVertices(
            Point2 startPosition,
            float sampleLength);
    }
}