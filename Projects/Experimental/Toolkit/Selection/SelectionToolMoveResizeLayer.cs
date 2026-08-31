using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

/// <summary>
/// Provides a selection layer that allows selected targets to be moved
/// and resized using the current selection tool settings.
/// </summary>
public sealed class SelectionToolMoveResizeLayer : SelectionToolLayer
{
    private readonly SelectionToolMoveHelper _move;
    private readonly SelectionToolResizeHelper _resize;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionToolMoveResizeLayer"/> class.
    /// </summary>
    public SelectionToolMoveResizeLayer()
    {
        _move = new SelectionToolMoveHelper();
        _resize = new SelectionToolResizeHelper();

        Nodes.Add(_move.Node);

        foreach (var node in _resize.Nodes)
            Nodes.Add(node);
    }

    /// <inheritdoc/>
    protected internal override void OnTargetInvalidated(
        SelectionToolLayerContext context,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        _move.OnTargetInvalidated(
            context,
            target,
            definition);

        _resize.OnTargetInvalidated(
            context,
            target,
            definition);
    }

    /// <inheritdoc/>
    protected internal override bool AllowHandle(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        return _move.AllowHandle(
                   node,
                   target,
                   definition) ||
               _resize.AllowHandle(
                   node,
                   target,
                   definition);
    }

    /// <inheritdoc/>
    protected internal override void OnNodeInteract(
        SelectionToolLayerContext context,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        IEnumerable<ISelectionTarget2>? otherSelectedTargets,
        IEnumerable<ISelectionTarget2Definition>? otherSelectedTargetDefinitions,
        Vector2 cursorPosition,
        Vector2 delta)
    {
        if (_move.AllowHandle(
            node,
            target,
            definition))
        {
            _move.OnNodeInteract(
                context,
                node,
                target,
                definition,
                otherSelectedTargets,
                otherSelectedTargetDefinitions,
                cursorPosition,
                delta);

            return;
        }

        if (!_resize.AllowHandle(
            node,
            target,
            definition))
        {
            return;
        }

        if (!_resize.OnNodeInteract(
            context,
            node,
            target,
            definition,
            cursorPosition - GetPosition(target, definition),
            delta,
            out var originOffset,
            out _,
            out _))
        {
            return;
        }

        ApplyPositionOffset(
            originOffset,
            target,
            definition);
    }

    /// <summary>
    /// Gets the current position of the specified runtime target or definition.
    /// </summary>
    /// <param name="target">
    /// The runtime selection target, if available.
    /// </param>
    /// <param name="definition">
    /// The selection target definition, if available.
    /// </param>
    /// <returns>
    /// The current target position, or <see cref="Vector2.Zero"/> if the target
    /// does not provide movable behavior.
    /// </returns>
    private static Vector2 GetPosition(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (target is ISelectionMovable2 movable)
            return movable.Position;

        if (definition is ISelectionMovable2Definition movableDefinition)
            return movableDefinition.Position;

        return Vector2.Zero;
    }

    /// <summary>
    /// Applies the specified position offset to the movable runtime target
    /// or its definition.
    /// </summary>
    /// <param name="offset">
    /// The position offset to apply.
    /// </param>
    /// <param name="target">
    /// The runtime selection target, if available.
    /// </param>
    /// <param name="definition">
    /// The selection target definition, if available.
    /// </param>
    private static void ApplyPositionOffset(
        Vector2 offset,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (target is ISelectionMovable2 movable &&
            movable.AllowMove)
        {
            movable.Position += offset;
        }

        if (definition is ISelectionMovable2Definition movableDefinition)
            movableDefinition.Position += offset;
    }
}