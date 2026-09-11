using Sachssoft.Sasogine.Common;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents the base class for a vector path segment.
    /// </summary>
    public abstract class VectorSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorSegment"/> class.
        /// </summary>
        protected VectorSegment()
        {
            Node = new VectorNode
            {
                Segment = this
            };
        }

        /// <summary>
        /// Gets the vector path that owns this segment.
        /// </summary>
        public VectorPath? Path { get; internal set; }

        /// <summary>
        /// Gets the endpoint node of the vector segment.
        /// </summary>
        public VectorNode Node { get; }

        /// <summary>
        /// Gets the control nodes used to define the shape of the vector segment.
        /// </summary>
        /// <returns>
        /// A read-only list containing the control nodes of the vector segment.
        /// </returns>
        public abstract IReadOnlyList<VectorNode> GetControlNodes();

        /// <summary>
        /// Generates a sampled representation of the vector segment.
        /// </summary>
        /// <param name="startPosition">The start position of the vector segment.</param>
        /// <param name="sampleLength">The desired approximate distance between consecutive sampled vertices.</param>
        /// <returns>An array containing the sampled vertices of the vector segment.</returns>
        public abstract Point2[] GetVertices(Point2 startPosition, float sampleLength);
    }
}
