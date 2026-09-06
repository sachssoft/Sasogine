namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents the result of a hit test against a vector path node,
    /// control node, or segment.
    /// </summary>
    public sealed class VectorNodeHitTestResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorNodeHitTestResult"/> class.
        /// </summary>
        /// <param name="node">
        /// The vector node that was hit, if any.
        /// </param>
        /// <param name="controlNode">
        /// The control node that was hit, if any.
        /// </param>
        /// <param name="segment">
        /// The vector segment associated with the hit, if any.
        /// </param>
        public VectorNodeHitTestResult(
            VectorNode? node,
            VectorNode? controlNode,
            IVectorSegment? segment)
        {
            Node = node;
            ControlNode = controlNode;
            Segment = segment;
        }

        /// <summary>
        /// Gets the vector node that was hit, if any.
        /// </summary>
        public VectorNode? Node { get; }

        /// <summary>
        /// Gets the control node that was hit, if any.
        /// </summary>
        public VectorNode? ControlNode { get; }

        /// <summary>
        /// Gets the vector segment associated with the hit, if any.
        /// </summary>
        public IVectorSegment? Segment { get; }
    }
}