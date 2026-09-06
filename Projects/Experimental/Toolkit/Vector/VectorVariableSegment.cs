using Sachssoft.Sasogine.Common;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a vector segment with a variable number of control nodes.
    /// </summary>
    public abstract class VectorVariableSegment : IVectorSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorVariableSegment"/> class.
        /// </summary>
        protected VectorVariableSegment()
        {
            Node = new VectorNode();
            ControlNodes = [];
        }

        /// <summary>
        /// Gets the endpoint node of the vector segment.
        /// </summary>
        public VectorNode Node { get; }

        /// <summary>
        /// Gets the modifiable collection of control nodes used by the vector segment.
        /// </summary>
        public List<VectorNode> ControlNodes { get; }

        /// <inheritdoc/>
        IReadOnlyList<VectorNode> IVectorSegment.ControlNodes =>
            ControlNodes;

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