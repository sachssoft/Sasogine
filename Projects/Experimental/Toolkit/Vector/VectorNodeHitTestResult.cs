namespace Sachssoft.Sasogine.Components.Tools.Vector
{
    /// <summary>Represents the result of a hit test against a vector path node, control node, or segment.</summary>
    public sealed class VectorNodeHitTestResult
    {
        public VectorNodeHitTestResult(
            VectorNode? node,
            VectorNode? controlNode,
            IVectorSegment? segment)
        {
            Node = node;
            ControlNode = controlNode;
            Segment = segment;
        }

        /// <summary>Gets the vector node that was hit, if any.</summary>
        public VectorNode? Node { get; }

        /// <summary>Gets the control node that was hit, if any.</summary>
        public VectorNode? ControlNode { get; }

        /// <summary>Gets the vector segment associated with the hit, if any.</summary>
        public IVectorSegment? Segment { get; }
    }
}