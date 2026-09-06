using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a node in a vector path.
    /// </summary>
    public sealed class VectorNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorNode"/> class.
        /// </summary>
        public VectorNode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorNode"/> class
        /// at the specified position.
        /// </summary>
        /// <param name="position">
        /// The initial position of the node.
        /// </param>
        public VectorNode(
            Point2 position)
        {
            Position = position;
        }

        /// <summary>
        /// Gets or sets the position of the node.
        /// </summary>
        public Point2 Position { get; set; }

        /// <summary>
        /// Gets or sets whether the node is selected.
        /// </summary>
        public bool IsSelected { get; set; }
    }
}