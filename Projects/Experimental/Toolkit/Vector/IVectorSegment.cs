using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Defines a vector path segment with an endpoint and optional control nodes.
    /// </summary>
    public interface IVectorSegment
    {
        /// <summary>
        /// Gets the endpoint of the segment.
        /// </summary>
        VectorNode Node { get; }

        /// <summary>
        /// Gets the control nodes used to define the shape of the segment.
        /// </summary>
        IReadOnlyList<VectorNode> ControlNodes { get; }

        /// <summary>
        /// Generates vertices representing the segment.
        /// </summary>
        /// <param name="startPosition">The starting position of the segment.</param>
        /// <param name="sampleLength">The approximate distance between sampled vertices.</param>
        /// <returns>An array of vertices representing the segment.</returns>
        Vector2[] GetVertices(Vector2 startPosition, float sampleLength);
    }
}