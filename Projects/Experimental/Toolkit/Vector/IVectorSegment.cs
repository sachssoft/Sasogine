using Sachssoft.Sasogine.Common;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    public interface IVectorSegment
    {

        VectorPath? Path { get; }

        VectorNode Node { get; }

        IReadOnlyList<VectorNode> GetControlNodes();

        Point2[] GetVertices(Point2 startPosition, float sampleLength);

    }
}