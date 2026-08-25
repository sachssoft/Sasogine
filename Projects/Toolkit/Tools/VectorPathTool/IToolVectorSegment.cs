using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools
{
    public interface IToolVectorSegment
    {
        ToolVectorNode Node { get; }

        IReadOnlyList<ToolVectorNode> ControlNodes { get; }

        Vector2[] GetVertices(Vector2 startPosition, float sampleLength);
    }
}
