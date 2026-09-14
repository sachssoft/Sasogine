using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    public class VectorQuadraticBezierSegmentDefinition : VectorSegmentDefinition
    {

        public VectorNodeDefinition ControlNode { get; } = new();

    }
}
