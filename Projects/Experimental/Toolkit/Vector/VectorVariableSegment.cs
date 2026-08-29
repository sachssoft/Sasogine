using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Vector
{
    /// <summary>Represents a vector segment with a variable number of control nodes.</summary>
    public abstract class VectorVariableSegment : IVectorSegment
    {
        protected VectorVariableSegment()
        {
            Node = new VectorNode();
            ControlNodes = new List<VectorNode>();
        }

        /// <summary>Gets the endpoint node of the vector segment.</summary>
        public VectorNode Node { get; }

        /// <summary>Gets the control nodes of the vector segment.</summary>
        public List<VectorNode> ControlNodes { get; }

        IReadOnlyList<VectorNode> IVectorSegment.ControlNodes => ControlNodes;

        /// <summary>Generates a sampled representation of the vector segment.</summary>
        /// <param name="startPosition">The start position of the vector segment.</param>
        /// <param name="sampleLength">The desired approximate distance between consecutive sampled vertices.</param>
        /// <returns>An array containing the sampled vertices of the vector segment.</returns>
        public abstract Vector2[] GetVertices(
            Vector2 startPosition,
            float sampleLength);
    }
}