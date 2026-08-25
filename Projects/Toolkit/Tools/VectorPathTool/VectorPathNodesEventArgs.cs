using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools
{
    public sealed class VectorPathNodesEventArgs : EventArgs
    {
        public VectorPathNodesEventArgs(
            IReadOnlyList<ToolVectorNode> nodes)
        {
            Nodes = nodes;
        }

        public IReadOnlyList<ToolVectorNode> Nodes { get; }
    }
}