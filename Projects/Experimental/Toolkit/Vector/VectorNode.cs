using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>Represents a node in a vector path.</summary>
    public sealed class VectorNode
    {
        public VectorNode()
        {
        }

        public VectorNode(Point2 position)
        {
            Position = position;
        }

        /// <summary>Gets or sets the position of the node.</summary>
        public Point2 Position { get; set; }

        /// <summary>Gets or sets whether the node is selected.</summary>
        public bool IsSelected { get; set; }
    }
}