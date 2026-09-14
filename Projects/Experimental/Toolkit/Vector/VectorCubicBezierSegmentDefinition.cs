using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    public class VectorCubicBezierSegmentDefinition : VectorSegmentDefinition
    {

        public VectorNodeDefinition ControlNode0 { get; } = new();

        public VectorNodeDefinition ControlNode1 { get; } = new();

    }
}
