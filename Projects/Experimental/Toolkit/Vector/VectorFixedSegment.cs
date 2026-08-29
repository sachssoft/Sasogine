using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Vector
{
    /// <summary>Represents a vector segment with a fixed number of control nodes.</summary>
    public abstract class VectorFixedSegment : IVectorSegment
    {
        protected VectorFixedSegment(
            int controlCount)
        {
            Node = new VectorNode();

            VectorNode[] controlNodes = new VectorNode[controlCount];

            for (int i = 0; i < controlNodes.Length; i++)
            {
                controlNodes[i] = new VectorNode();
            }

            ControlNodes = controlNodes;
        }

        /// <summary>Gets the endpoint node of the vector segment.</summary>
        public VectorNode Node { get; }

        /// <summary>Gets the fixed collection of control nodes used by the vector segment.</summary>
        public IReadOnlyList<VectorNode> ControlNodes { get; }

        /// <summary>Generates a sampled representation of the vector segment.</summary>
        /// <param name="startPosition">The start position of the vector segment.</param>
        /// <param name="sampleLength">The desired approximate distance between consecutive sampled vertices.</param>
        /// <returns>An array containing the sampled vertices of the vector segment.</returns>
        public abstract Vector2[] GetVertices(
            Vector2 startPosition,
            float sampleLength);
    }
}