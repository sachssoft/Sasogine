using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Provides move interaction behavior for selection tool layers.
/// </summary>
internal sealed class SelectionToolMoveHelper
{
    private Point2 _dragStartPosition;
    private Point2 _dragStartCursorPosition;
    private Point2 _appliedPosition;
    private bool _isDragging;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionToolMoveHelper"/> class.
    /// </summary>
    public SelectionToolMoveHelper()
    {
        Node = new SelectionToolNode(
            shape: SelectionToolNodeShape.Quad,
            isVisible: false,
            opacity: 0.3f);
    }

    /// <summary>
    /// Gets the node used to move a selection target.
    /// </summary>
    public SelectionToolNode Node { get; }

    /// <summary>
    /// Updates the move node for the specified target or definition.
    /// </summary>
    /// <param name="context">The current selection tool layer context.</param>
    /// <param name="target">The runtime selection target, if available.</param>
    /// <param name="definition">The selection target definition, if available.</param>
    public void OnTargetInvalidated(
        SelectionToolLayerContext context,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        Node.Position = Point2.Zero;
        Node.Size = definition?.Size ?? target?.Size ?? Size2.Zero;
    }

    /// <summary>
    /// Determines whether the move node can be used for the specified target.
    /// </summary>
    /// <param name="node">The node to evaluate.</param>
    /// <param name="target">The runtime selection target, if available.</param>
    /// <param name="definition">The selection target definition, if available.</param>
    /// <returns><see langword="true"/> if moving is supported; otherwise, <see langword="false"/>.</returns>
    public bool AllowHandle(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (!ReferenceEquals(node, Node))
            return false;

        if (target != null)
            return target is ISelectionMovable2 movable && movable.AllowMove;

        return definition is ISelectionMovable2Definition;
    }

    /// <summary>
    /// Begins a move interaction.
    /// </summary>
    /// <param name="node">The node being manipulated.</param>
    /// <param name="target">The runtime selection target, if available.</param>
    /// <param name="definition">The selection target definition, if available.</param>
    /// <param name="cursorPosition">The initial cursor position in world space.</param>
    public void BeginInteraction(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Point2 cursorPosition)
    {
        if (!ReferenceEquals(node, Node))
            return;

        if (!TryGetPosition(target, definition, out var position))
            return;

        _dragStartPosition = position;
        _dragStartCursorPosition = cursorPosition;
        _appliedPosition = position;
        _isDragging = true;
    }

    /// <summary>
    /// Updates the active move interaction.
    /// </summary>
    /// <param name="context">The current selection tool layer context.</param>
    /// <param name="node">The node being manipulated.</param>
    /// <param name="target">The runtime selection target, if available.</param>
    /// <param name="definition">The selection target definition, if available.</param>
    /// <param name="otherSelectedTargets">Other selected runtime targets.</param>
    /// <param name="otherSelectedTargetDefinitions">Definitions of other selected targets.</param>
    /// <param name="cursorPosition">The current cursor position in world space.</param>
    public void OnNodeInteract(
        SelectionToolLayerContext context,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        IEnumerable<ISelectionTarget2>? otherSelectedTargets,
        IEnumerable<ISelectionTarget2Definition>? otherSelectedTargetDefinitions,
        Point2 cursorPosition)
    {
        if (!ReferenceEquals(node, Node) || !_isDragging)
            return;

        Point2 newPosition = _dragStartPosition + (cursorPosition - _dragStartCursorPosition);

        if (context.EnableGridSnap)
        {
            if (context.GridSnapStep.Width > 0f)
            {
                newPosition = new Point2(
                    MathF.Round(newPosition.X / context.GridSnapStep.Width, MidpointRounding.AwayFromZero) * context.GridSnapStep.Width,
                    newPosition.Y);
            }

            if (context.GridSnapStep.Height > 0f)
            {
                newPosition = new Point2(
                    newPosition.X,
                    MathF.Round(newPosition.Y / context.GridSnapStep.Height, MidpointRounding.AwayFromZero) * context.GridSnapStep.Height);
            }
        }

        Vector2 movement = newPosition - _appliedPosition;
        if (movement == Vector2.Zero)
            return;

        SetPosition(target, definition, newPosition);
        _appliedPosition = newPosition;

        var updatedDefinitions = new HashSet<ISelectionTarget2Definition>();
        if (definition != null)
            updatedDefinitions.Add(definition);

        if (otherSelectedTargets != null)
        {
            foreach (var otherTarget in otherSelectedTargets)
            {
                if (otherTarget is not ISelectionMovable2 otherMovable || !otherMovable.AllowMove)
                    continue;

                if (otherTarget.Definition is not ISelectionMovable2Definition otherDefinition)
                    throw new InvalidOperationException($"The movable selection target requires an '{nameof(ISelectionMovable2Definition)}' definition.");

                otherDefinition.Position += movement;
                updatedDefinitions.Add(otherDefinition);
            }
        }

        if (otherSelectedTargetDefinitions == null)
            return;

        foreach (var otherDefinition in otherSelectedTargetDefinitions)
        {
            if (!updatedDefinitions.Add(otherDefinition))
                continue;

            if (otherDefinition is ISelectionMovable2Definition otherMovableDefinition)
                otherMovableDefinition.Position += movement;
        }
    }

    /// <summary>
    /// Ends the current move interaction.
    /// </summary>
    public void EndInteraction()
    {
        _isDragging = false;
    }

    private static bool TryGetPosition(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        out Point2 position)
    {
        if (definition is ISelectionMovable2Definition movableDefinition)
        {
            position = movableDefinition.Position;
            return true;
        }

        if (target is ISelectionMovable2 movable && movable.AllowMove)
        {
            position = movable.Position;
            return true;
        }

        position = Point2.Zero;
        return false;
    }

    private static void SetPosition(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Point2 position)
    {
        if (definition is ISelectionMovable2Definition movableDefinition)
        {
            movableDefinition.Position = position;
            return;
        }

        if (target is ISelectionMovable2 movable && movable.AllowMove)
        {
            if (target.Definition is not ISelectionMovable2Definition targetDefinition)
                throw new InvalidOperationException($"The movable selection target requires an '{nameof(ISelectionMovable2Definition)}' definition.");

            targetDefinition.Position = position;
        }
    }
}
