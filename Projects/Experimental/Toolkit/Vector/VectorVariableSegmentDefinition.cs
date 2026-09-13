using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Defines a vector segment with a variable number of control nodes.
    /// </summary>
    public class VectorVariableSegmentDefinition : VectorSegmentDefinition
    {
        /// <summary>
        /// Gets the control node definitions.
        /// </summary>
        public ObservableCollection<VectorNodeDefinition> ControlNodes { get; } = [];
    }
}
