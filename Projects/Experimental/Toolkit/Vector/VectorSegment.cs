using Sachssoft.Sasogine.Common;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents the base class for a vector path segment.
    /// </summary>
    public abstract class VectorSegment : EngineObject<VectorSegmentDefinition>, IVectorSegment, IVectorSegmentInternal
    {
        private VectorPath? _path;
        private VectorNode _node;

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorSegment"/> class.
        /// </summary>
        protected VectorSegment(VectorSegmentDefinition definition)
            : base(definition)
        {
            _node = new VectorNode(definition.Node) { Segment = this };
        }

        /// <summary>
        /// Gets the vector path that owns this segment.
        /// </summary>
        public VectorPath? Path => _path;

        VectorPath? IVectorSegmentInternal.Path
        {
            get => _path;
            set => _path = value;
        }

        /// <summary>
        /// Gets the endpoint node of the vector segment.
        /// </summary>
        public VectorNode Node => _node;

        /// <inheritdoc/>
        protected override void ConfigureFromDefinition()
        {
            base.ConfigureFromDefinition();

            if (!ReferenceEquals(_node.Definition, Definition.Node))
            {
                _node.Segment = null;
                _node = new VectorNode(Definition.Node) { Segment = this };
            }
            else
            {
                _node.Reload();
            }
        }

        public abstract IReadOnlyList<VectorNode> GetControlNodes();
        public abstract Point2[] GetVertices(Point2 startPosition, float sampleLength);
    }
}
