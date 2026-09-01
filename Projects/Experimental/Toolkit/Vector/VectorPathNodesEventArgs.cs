using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>Provides data for an event involving multiple nodes of a vector path.</summary>
    public sealed class VectorPathNodesEventArgs : EventArgs
    {
        public VectorPathNodesEventArgs(
            IReadOnlyList<VectorNode> nodes)
        {
            Nodes = nodes;
        }

        /// <summary>Gets the vector nodes associated with the event.</summary>
        public IReadOnlyList<VectorNode> Nodes { get; }
    }
}