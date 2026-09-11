using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Defines the configurable state shared by vector segments.
    /// </summary>
    public class VectorSegmentDefinition : IDefinition
    {
        /// <summary>
        /// Gets or sets the endpoint node definition.
        /// </summary>
        public VectorNodeDefinition Node { get; set; } = new();
    }
}
