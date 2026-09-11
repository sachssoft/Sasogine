using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a vector segment with a fixed number of control nodes.
    /// </summary>
    public abstract class VectorFixedSegment : VectorSegment
    {
        private readonly IReadOnlyList<VectorNode> _controlNodes;

        protected VectorFixedSegment(int controlCount)
            : this(CreateDefinition(controlCount))
        {
        }

        protected VectorFixedSegment(VectorFixedSegmentDefinition definition)
            : base(definition)
        {
            ArgumentNullException.ThrowIfNull(definition);

            var controlNodes = new VectorNode[definition.ControlNodes.Count];
            for (int i = 0; i < controlNodes.Length; i++)
                controlNodes[i] = new VectorNode(definition.ControlNodes[i]) { Segment = this };

            _controlNodes = controlNodes;
        }

        public override IReadOnlyList<VectorNode> GetControlNodes() => _controlNodes;

        protected override void ConfigureFromDefinition()
        {
            base.ConfigureFromDefinition();
            for (int i = 0; i < _controlNodes.Count; i++)
                _controlNodes[i].Reload();
        }

        private static VectorFixedSegmentDefinition CreateDefinition(int controlCount)
        {
            if (controlCount < 0)
                throw new ArgumentOutOfRangeException(nameof(controlCount));

            var definition = new VectorFixedSegmentDefinition();
            for (int i = 0; i < controlCount; i++)
                definition.ControlNodes.Add(new VectorNodeDefinition());
            return definition;
        }
    }
}
