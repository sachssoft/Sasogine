using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Represents the base class for a vector path segment.
/// </summary>
public abstract class VectorSegment<TDefinition> :
    EngineObject<TDefinition>,
    IVectorSegment,
    IVectorSegmentInternal
    where TDefinition : VectorSegmentDefinition
{
    private VectorPath? _path;
    private VectorNode _node;

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorSegment{TDefinition}"/> class.
    /// </summary>
    /// <param name="definition">
    /// The definition describing the vector segment.
    /// </param>
    protected VectorSegment(TDefinition definition)
        : base(definition)
    {
        _node = new VectorNode(definition.Node) { Segment = this };
    }

    /// <summary>
    /// Gets the vector path that owns this segment.
    /// </summary>
    public VectorPath? Path => _path;

    VectorPath? IVectorSegmentInternal.Owner
    {
        get => _path;
        set
        {
            if (ReferenceEquals(_path, value))
                return;

            if (_path is not null && value is not null)
            {
                throw new InvalidOperationException(
                    "The vector segment already belongs to another vector path.");
            }

            VectorPath? oldOwner = _path;

            _path = value;

            ((IVectorSegmentInternal)this).OnOwnerChanged(
                oldOwner,
                value);
        }
    }

    /// <summary>
    /// Gets the endpoint node of the vector segment.
    /// </summary>
    public VectorNode Node => _node;

    VectorSegmentDefinition IVectorSegment.Definition => Definition;

    void IVectorSegmentInternal.OnOwnerChanged(
        VectorPath? oldOwner,
        VectorPath? newOwner)
    {
        OnOwnerChanged(oldOwner, newOwner);
    }

    /// <summary>
    /// Called when the vector path that owns this segment changes.
    /// </summary>
    /// <param name="oldOwner">
    /// The previous owning vector path, or <see langword="null"/> if
    /// the segment was not previously owned.
    /// </param>
    /// <param name="newOwner">
    /// The new owning vector path, or <see langword="null"/> if
    /// the segment has been detached.
    /// </param>
    protected virtual void OnOwnerChanged(
        VectorPath? oldOwner,
        VectorPath? newOwner)
    {
    }

    /// <inheritdoc/>
    protected override void ConfigureFromDefinition()
    {
        base.ConfigureFromDefinition();

        if (!ReferenceEquals(_node.Definition, Definition.Node))
        {
            _node.Segment = null;
            _node = new VectorNode(Definition.Node) { Segment = this };
        }
        else
        {
            _node.Reload();
        }
    }

    /// <summary>
    /// Gets the control nodes used by the vector segment.
    /// </summary>
    /// <returns>
    /// The control nodes used by the segment.
    /// </returns>
    public abstract IReadOnlyList<VectorNode> GetControlNodes();

    /// <summary>
    /// Generates sampled vertices for the vector segment.
    /// </summary>
    /// <param name="startPosition">
    /// The position at which the segment starts.
    /// </param>
    /// <param name="sampleLength">
    /// The desired approximate distance between consecutive sampled vertices.
    /// </param>
    /// <returns>
    /// The sampled vertices of the segment.
    /// </returns>
    public abstract Point2[] GetVertices(
        Point2 startPosition,
        float sampleLength);
}