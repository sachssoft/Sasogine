using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>Represents a node in a vector path.</summary>
    public sealed class VectorNode
    {
        public VectorNode()
        {
        }

        public VectorNode(Vector2 position)
        {
            Position = position;
        }

        /// <summary>Gets or sets the position of the node.</summary>
        public Vector2 Position { get; set; }

        /// <summary>Gets or sets whether the node is selected.</summary>
        public bool IsSelected { get; set; }
    }
}