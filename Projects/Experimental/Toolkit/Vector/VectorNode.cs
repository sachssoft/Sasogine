using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a node in a vector path.
    /// </summary>
    public sealed class VectorNode : EngineObject<VectorNodeDefinition>
    {
        private Point2 _position;
        private bool _isSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorNode"/> class.
        /// </summary>
        public VectorNode()
            : this(new VectorNodeDefinition())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorNode"/> class
        /// at the specified position.
        /// </summary>
        public VectorNode(Point2 position)
            : this(new VectorNodeDefinition { Position = position })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorNode"/> class
        /// using the specified definition.
        /// </summary>
        public VectorNode(VectorNodeDefinition definition)
            : base(definition)
        {
            _position = definition.Position;
            _isSelected = definition.IsSelected;
        }

        /// <summary>
        /// Gets the vector segment that owns this node.
        /// </summary>
        public VectorSegment? Segment { get; internal set; }

        /// <summary>
        /// Gets the current runtime position of the node.
        /// </summary>
        public Point2 Position => _position;

        /// <summary>
        /// Gets whether the node is currently selected.
        /// </summary>
        public bool IsSelected => _isSelected;

        /// <inheritdoc/>
        protected override void ConfigureFromDefinition()
        {
            base.ConfigureFromDefinition();
            _position = Definition.Position;
            _isSelected = Definition.IsSelected;
        }
    }
}
