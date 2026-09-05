using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Tools.Selection;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

/// <summary>
/// Defines a base class for selection tool layers that provide interaction nodes,
/// respond to target changes, handle target-specific interactions, and optionally
/// transform target-local coordinates.
/// </summary>
public abstract class SelectionToolLayer
{
    private readonly List<SelectionToolNode> _nodes = new();

    /// <summary>
    /// Gets the collection of interaction nodes provided by this layer.
    /// </summary>
    protected internal IList<SelectionToolNode> Nodes => _nodes;

    /// <summary>
    /// Notifies the layer that the current selection target or its definition
    /// has been invalidated and may require its interaction nodes to be updated.
    /// </summary>
    /// <param name="context">
    /// Provides the current selection tool settings required by the layer.
    /// </param>
    /// <param name="target">
    /// The runtime selection target, if available.
    /// </param>
    /// <param name="definition">
    /// The selection target definition, if available.
    /// </param>
    protected internal virtual void OnTargetInvalidated(
        SelectionToolLayerContext context,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
    }

    /// <summary>
    /// Determines whether the specified interaction node can be used for the
    /// specified selection target or its definition.
    /// </summary>
    /// <param name="node">
    /// The interaction node to evaluate.
    /// </param>
    /// <param name="target">
    /// The runtime selection target, if available.
    /// </param>
    /// <param name="definition">
    /// The selection target definition, if available.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the node can be used for the specified target
    /// or definition; otherwise, <see langword="false"/>.
    /// </returns>
    protected internal virtual bool AllowHandle(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        return true;
    }

    /// <summary>
    /// Handles interaction with the specified selection tool node.
    /// </summary>
    /// <param name="context">
    /// Provides the current selection tool settings required by the interaction.
    /// </param>
    /// <param name="node">
    /// The interaction node being manipulated.
    /// </param>
    /// <param name="target">
    /// The primary runtime selection target, if available.
    /// </param>
    /// <param name="definition">
    /// The primary selection target definition, if available.
    /// </param>
    /// <param name="otherSelectedTargets">
    /// The other selected runtime targets, if available.
    /// </param>
    /// <param name="otherSelectedTargetDefinitions">
    /// The definitions of the other selected targets, if available.
    /// </param>
    /// <param name="cursorPosition">
    /// The current cursor position in world space.
    /// </param>
    /// <param name="delta">
    /// The cursor movement delta since the previous interaction update.
    /// </param>
    protected internal virtual void OnNodeInteract(
        SelectionToolLayerContext context,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        IEnumerable<ISelectionTarget2>? otherSelectedTargets,
        IEnumerable<ISelectionTarget2Definition>? otherSelectedTargetDefinitions,
        Point2 cursorPosition,
        Point2 delta)
    {
    }

    /// <summary>
    /// Transforms a point from the target's untransformed local coordinate space
    /// into the transformed local coordinate space provided by this layer.
    /// </summary>
    /// <param name="point">
    /// The local point to transform.
    /// </param>
    /// <param name="target">
    /// The runtime selection target, if available.
    /// </param>
    /// <param name="definition">
    /// The selection target definition, if available.
    /// </param>
    /// <returns>
    /// The transformed local point.
    /// </returns>
    protected internal virtual Point2 Transform(
        Point2 point,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        return point;
    }

    /// <summary>
    /// Transforms a point from the transformed local coordinate space back into
    /// the target's untransformed local coordinate space.
    /// </summary>
    /// <param name="point">
    /// The transformed local point to convert.
    /// </param>
    /// <param name="target">
    /// The runtime selection target, if available.
    /// </param>
    /// <param name="definition">
    /// The selection target definition, if available.
    /// </param>
    /// <returns>
    /// The point in the target's untransformed local coordinate space.
    /// </returns>
    protected internal virtual Point2 InverseTransform(
        Point2 point,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        return point;
    }

    /// <summary>
    /// Transforms a resize-origin offset from local target space into the
    /// position offset required to preserve the target's transformed geometry.
    /// </summary>
    /// <param name="offset">
    /// The local resize-origin offset.
    /// </param>
    /// <param name="oldSize">
    /// The target size before the resize operation.
    /// </param>
    /// <param name="newSize">
    /// The target size after the resize operation.
    /// </param>
    /// <param name="target">
    /// The runtime selection target, if available.
    /// </param>
    /// <param name="definition">
    /// The selection target definition, if available.
    /// </param>
    /// <returns>
    /// The position offset to apply to the target or definition.
    /// </returns>
    protected internal virtual Vector2 TransformResizeOffset(
        Vector2 offset,
        Size2 oldSize,
        Size2 newSize,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        return offset;
    }

    /// <summary>
    /// Determines whether the specified position intersects the interaction area
    /// of the specified selection tool node.
    /// </summary>
    /// <param name="position">
    /// The position to test in world space.
    /// </param>
    /// <param name="node">
    /// The interaction node to test.
    /// </param>
    /// <param name="target">
    /// The runtime selection target, if available.
    /// </param>
    /// <param name="definition">
    /// The selection target definition, if available.
    /// </param>
    /// <param name="nodeWorldPosition">
    /// The world-space position of the interaction node.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the position intersects the node;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected internal virtual bool HitTestNode(
        Point2 position,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Point2 nodeWorldPosition)
    {
        return position.X >= nodeWorldPosition.X &&
               position.X <= nodeWorldPosition.X + node.Size.Width &&
               position.Y >= nodeWorldPosition.Y &&
               position.Y <= nodeWorldPosition.Y + node.Size.Height;
    }
}