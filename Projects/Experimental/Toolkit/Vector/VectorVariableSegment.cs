using System;
using System.Collections.Generic;
using System.Linq;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a vector segment with a variable number of control nodes.
    /// </summary>
    public abstract class VectorVariableSegment<TDefinition> : VectorSegment<TDefinition>, IVectorVariableSegment
        where TDefinition : VectorVariableSegmentDefinition
    {
        protected VectorVariableSegment()
            : this(new VectorVariableSegmentDefinition())
        {
        }

        protected VectorVariableSegment(VectorVariableSegmentDefinition definition)
            : base(definition)
        {
            ControlNodes = new VectorNodeCollection(this);

            if (definition.ControlNodes != null)
                ControlNodes.AddRange(
                    definition.ControlNodes.Select(x => new VectorNode(x)));
            //for (int i = 0; i < definition.ControlNodes.Count; i++)
            //    ControlNodes.Add(new VectorNode(definition.ControlNodes[i]));
        }

        public VectorNodeCollection ControlNodes { get; }

        public override IReadOnlyList<VectorNode> GetControlNodes() => ControlNodes;

        protected override void ConfigureFromDefinition()
        {
            base.ConfigureFromDefinition();
            foreach (var node in ControlNodes)
                node.Reload();
        }
    }
}
