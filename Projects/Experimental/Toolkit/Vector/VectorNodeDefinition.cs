using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Defines the configurable state of a vector node.
    /// </summary>
    public sealed class VectorNodeDefinition : IDefinition
    {
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
