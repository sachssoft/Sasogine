using System.Collections.Generic;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Represents a vector segment with a variable number of control nodes.
/// </summary>
/// <typeparam name="TDefinition">
/// The type of definition that configures the variable vector segment.
/// </typeparam>
/// <remarks>
/// The control nodes are created from the control node definitions contained
/// in <typeparamref name="TDefinition"/> and are owned by this segment.
/// </remarks>
public abstract class VectorVariableSegment<TDefinition> : VectorSegment<TDefinition>, IVectorVariableSegment
    where TDefinition : VectorVariableSegmentDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VectorVariableSegment{TDefinition}"/> class
    /// from the specified definition.
    /// </summary>
    /// <param name="definition">
    /// The definition containing the configurable state and control node definitions
    /// of the segment.
    /// </param>
    protected VectorVariableSegment(TDefinition definition)
        : base(definition)
    {
        ControlNodes = new VectorNodeCollection(this);
        for (int i = 0; i < definition.ControlNodes.Count; i++)
            ControlNodes.Add(new VectorNode(definition.ControlNodes[i]));
    }

    /// <summary>
    /// Gets the collection of control nodes that define or influence the geometry
    /// of the vector segment.
    /// </summary>
    /// <value>
    /// The <see cref="VectorNodeCollection"/> containing the control nodes owned
    /// by this segment.
    /// </value>
    public VectorNodeCollection ControlNodes { get; }

    /// <summary>
    /// Gets the control nodes used by the vector segment.
    /// </summary>
    /// <returns>
    /// A read-only view of the control nodes used to define or influence
    /// the geometry of the segment.
    /// </returns>
    public override IReadOnlyList<VectorNode> GetControlNodes() => ControlNodes;

    /// <summary>
    /// Applies the current definition state to the vector segment.
    /// </summary>
    /// <remarks>
    /// In addition to the configuration performed by the base implementation,
    /// each existing control node is reloaded from its associated definition.
    /// </remarks>
    protected override void ConfigureFromDefinition()
    {
        base.ConfigureFromDefinition();
        foreach (var node in ControlNodes)
            node.Reload();
    }
}