using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Provides data for an event involving multiple nodes of a vector path.
    /// </summary>
    public sealed class VectorPathNodesEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorPathNodesEventArgs"/> class
        /// using the specified vector nodes.
        /// </summary>
        /// <param name="nodes">
        /// The vector nodes associated with the event.
        /// </param>
        public VectorPathNodesEventArgs(
            IReadOnlyList<VectorNode> nodes)
        {
            Nodes = nodes;
        }

        /// <summary>
        /// Gets the vector nodes associated with the event.
        /// </summary>
        public IReadOnlyList<VectorNode> Nodes { get; }
    }
}