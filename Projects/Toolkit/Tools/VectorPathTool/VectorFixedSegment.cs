using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools;

/// <summary>
/// Represents a vector segment with a fixed number of control nodes.
/// </summary>
/// <typeparam name="TDefinition">
/// The type of definition associated with the vector segment.
/// </typeparam>
public abstract class VectorFixedSegment<TDefinition> :
    VectorSegment<TDefinition>
    where TDefinition : VectorSegmentDefinition
{
    private readonly IReadOnlyList<VectorNode> _controlNodes;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="VectorFixedSegment{TDefinition}"/> class.
    /// </summary>
    /// <param name="controlCount">
    /// The number of control nodes in the segment.
    /// </param>
    /// <param name="definition">
    /// The definition associated with the segment.
    /// </param>
    protected VectorFixedSegment(
        int controlCount,
        TDefinition definition)
        : base(definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var controlNodes = new VectorNode[controlCount];

        for (int i = 0; i < controlCount; i++)
            controlNodes[i] = CreateVectorNode(i, definition);

        _controlNodes = controlNodes;
    }

    /// <summary>
    /// Creates the control node at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the control node to create.
    /// </param>
    /// <param name="definition">
    /// The segment definition used to create the control node.
    /// </param>
    /// <returns>
    /// The created control node.
    /// </returns>
    protected abstract VectorNode CreateVectorNode(
        int index,
        TDefinition definition);

    /// <inheritdoc/>
    public override IReadOnlyList<VectorNode> GetControlNodes() =>
        _controlNodes;

    /// <inheritdoc/>
    protected override void ConfigureFromDefinition()
    {
        base.ConfigureFromDefinition();

        for (int i = 0; i < _controlNodes.Count; i++)
            _controlNodes[i].Reload();
    }
}