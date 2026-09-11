using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Defines a vector segment with a fixed number of control nodes.
    /// </summary>
    public class VectorFixedSegmentDefinition : VectorSegmentDefinition
    {
        /// <summary>
        /// Gets the control node definitions.
        /// </summary>
        public List<VectorNodeDefinition> ControlNodes { get; } = [];
    }
}
