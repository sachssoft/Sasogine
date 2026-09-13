using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Defines the configurable state of a vector path.
    /// </summary>
    public sealed class VectorPathDefinition : IDefinition
    {
        /// <summary>
        /// Gets or sets the start node definition.
        /// </summary>
        public VectorNodeDefinition Start { get; set; } = new();

        /// <summary>
        /// Gets or sets whether the path is closed.
        /// </summary>
        public bool IsClosed { get; set; }
    }
}
