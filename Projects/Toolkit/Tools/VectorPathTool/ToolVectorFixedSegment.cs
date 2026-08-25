using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools;

public abstract class ToolVectorFixedSegment : IToolVectorSegment
{
    protected ToolVectorFixedSegment(
        int controlCount)
    {
        Node = new ToolVectorNode();

        ToolVectorNode[] controlNodes = new ToolVectorNode[controlCount];

        for (int i = 0; i < controlNodes.Length; i++)
        {
            controlNodes[i] = new ToolVectorNode();
        }

        ControlNodes = controlNodes;
    }

    public ToolVectorNode Node { get; }

    public IReadOnlyList<ToolVectorNode> ControlNodes { get; }

    public abstract Vector2[] GetVertices(Vector2 startPosition, float sampleLength);
}