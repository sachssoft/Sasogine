using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools;

public abstract class ToolVectorDynamicSegment : IToolVectorSegment
{
    protected ToolVectorDynamicSegment()
    {
        Node = new ToolVectorNode();
        ControlNodes = new List<ToolVectorNode>();
    }

    public ToolVectorNode Node { get; }

    public List<ToolVectorNode> ControlNodes { get; }

    IReadOnlyList<ToolVectorNode> IToolVectorSegment.ControlNodes => ControlNodes;

    public abstract Vector2[] GetVertices(
        Vector2 startPosition,
        float sampleLength);
}