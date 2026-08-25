namespace Sachssoft.Sasogine.Components.Tools
{
    public sealed class ToolVectorNodeHitTestResult
    {

        public ToolVectorNodeHitTestResult(
            ToolVectorNode? node,
            ToolVectorNode? controlNode,
            IToolVectorSegment? segment
            )
        {
            Node = node;
            ControlNode = controlNode;
            Segment = segment;
        }

        public ToolVectorNode? Node { get; }

        public ToolVectorNode? ControlNode { get; }

        public IToolVectorSegment? Segment { get; }
    }
}
