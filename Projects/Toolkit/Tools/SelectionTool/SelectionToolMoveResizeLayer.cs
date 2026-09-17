using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System;
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
        _move.OnTargetInvalidated(context, target, definition);
        _resize.OnTargetInvalidated(context, target, definition);
    }

    /// <inheritdoc/>
    protected internal override bool AllowHandle(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        return _move.AllowHandle(node, target, definition) ||
               _resize.AllowHandle(node, target, definition);
    }

    /// <inheritdoc/>
    protected internal override void OnNodeInteract(
        SelectionToolLayerContext context,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        IEnumerable<ISelectionTarget2>? otherSelectedTargets,
        IEnumerable<ISelectionTarget2Definition>? otherSelectedTargetDefinitions,
        Point2 cursorPosition,
        Vector2 delta)
    {
        if (_move.AllowHandle(node, target, definition))
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

        if (!_resize.AllowHandle(node, target, definition))
            return;

        Vector2 localCursorPosition =
            cursorPosition - GetPosition(target, definition);

        if (!_resize.OnNodeInteract(
            context,
            node,
            target,
            definition,
            localCursorPosition,
            delta,
            out var originOffset,
            out _,
            out _))
        {
            return;
        }

        ApplyPositionOffset(originOffset, target, definition);
    }

    /// <summary>
    /// Gets the current position of the specified runtime target or definition.
    /// </summary>
    /// <param name="target">The runtime selection target, if available.</param>
    /// <param name="definition">The selection target definition, if available.</param>
    /// <returns>
    /// The current target position, or <see cref="Point2.Zero"/> if the target
    /// does not provide movable behavior.
    /// </returns>
    private static Point2 GetPosition(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (target is ISelectionMovable2 movable)
            return movable.Position;

        if (definition is ISelectionMovable2Definition movableDefinition)
            return movableDefinition.Position;

        return Point2.Zero;
    }

    private static void ApplyPositionOffset(
       Vector2 offset,
       ISelectionTarget2? target,
       ISelectionTarget2Definition? definition)
    {
        if (target is ISelectionMovable2 movable)
        {
            if (!movable.AllowMove)
                return;

            if (target.Definition is not ISelectionMovable2Definition movableDefinition)
            {
                throw new InvalidOperationException(
                    $"The movable selection target requires an '{nameof(ISelectionMovable2Definition)}' definition.");
            }

            movableDefinition.Position += offset;
            return;
        }

        if (definition is ISelectionMovable2Definition definitionMovable)
            definitionMovable.Position += offset;
    }
}