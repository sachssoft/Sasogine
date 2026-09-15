using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Common.Collections;
using Sachssoft.Sasogine.Components.Models;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Defines the configurable state of a vector path.
    /// </summary>
    public sealed class VectorPathDefinition : IDefinition
    {
        /// <summary>
        /// Gets the ordered collection of vector segment definitions that make up the path.
        /// </summary>
        /// <remarks>
        /// The segments are managed through specialized vector editing tools rather
        /// than directly through the property inspector.
        /// </remarks>
        [Browsable(false)]
        public TrackableCollection<VectorSegmentDefinition> Segments { get; } = [];

        /// <summary>
        /// Gets the definition of the start node of the vector path.
        /// </summary>
        /// <value>
        /// The <see cref="VectorNodeDefinition"/> that defines the starting position
        /// and state of the path.
        /// </value>
        [Category(Categories.Design)]
        [DisplayName("Start")]
        public VectorNodeDefinition Start { get; } = new();

        /// <summary>
        /// Gets or sets whether the vector path forms a closed shape.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the end of the path is connected back to its
        /// start; otherwise, <see langword="false"/>.
        /// The default value is <see langword="false"/>.
        /// </value>
        [Category(Categories.Design)]
        [DisplayName("Closed")]
        public bool IsClosed { get; set; }
    }
}