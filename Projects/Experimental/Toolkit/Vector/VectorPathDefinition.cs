using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Common.Collections;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Defines the configurable state of a vector path.
    /// </summary>
    public sealed class VectorPathDefinition : IDefinition
    {
        /// <summary>
        /// Gets the ordered collection of vector segment definitions
        /// that make up the path.
        /// </summary>
        [Browsable(false)]
        public TrackableCollection<VectorSegmentDefinition> Segments { get; } = [];

        /// <summary>
        /// Gets the definition of the start node of the path.
        /// </summary>
        public VectorNodeDefinition Start { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether the path is closed.
        /// </summary>
        public bool IsClosed { get; set; }
    }
}