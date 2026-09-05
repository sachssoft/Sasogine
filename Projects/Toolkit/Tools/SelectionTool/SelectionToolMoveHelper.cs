using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Tools.Selection;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

internal sealed class SelectionToolMoveHelper
{
    private Point2 _dragStartPosition;
    private Point2 _dragStartCursorPosition;
    private bool _isDragging;

    public SelectionToolMoveHelper()
    {
        Node = new SelectionToolNode(
            shape: SelectionToolNodeShape.Quad,
            isVisible: false,
            opacity: 0.3f);
    }

    public SelectionToolNode Node { get; }

    public void OnTargetInvalidated(
        SelectionToolLayerContext context,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        Node.Position = Point2.Zero;

        if (target != null)
        {
            Node.Size = target.Size;
            return;
        }

        if (definition != null)
        {
            Node.Size = definition.Size;
            return;
        }

        Node.Size = Size2.Zero;
    }

    public bool AllowHandle(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (!ReferenceEquals(node, Node))
            return false;

        return (target is ISelectionMovable2 movable &&
                movable.AllowMove) ||
               definition is ISelectionMovable2Definition;
    }

    public void OnNodeInteract(
        SelectionToolLayerContext context,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        IEnumerable<ISelectionTarget2>? otherSelectedTargets,
        IEnumerable<ISelectionTarget2Definition>? otherSelectedTargetDefinitions,
        Point2 cursorPosition,
        Point2 delta)
    {
        if (!ReferenceEquals(node, Node))
            return;

        Point2 currentPosition;

        if (target is ISelectionMovable2 movable && movable.AllowMove)
            currentPosition = movable.Position;
        else if (definition is ISelectionMovable2Definition movableDefinition)
            currentPosition = movableDefinition.Position;
        else
            return;

        if (delta == Point2.Zero)
        {
            _dragStartPosition = currentPosition;
            _dragStartCursorPosition = cursorPosition;
            _isDragging = true;
            return;
        }

        if (!_isDragging)
        {
            _dragStartPosition = currentPosition;
            _dragStartCursorPosition = cursorPosition - delta;
            _isDragging = true;
        }

        Point2 newPosition;

        if (context.EnableGridSnap)
        {
            newPosition = _dragStartPosition +
                cursorPosition -
                _dragStartCursorPosition;

            var gridSize = context.GridSnapStep;

            if (gridSize.Width > 0f)
                newPosition = new Point2(
                    MathF.Round(newPosition.X / gridSize.Width) * gridSize.Width,
                    newPosition.Y
                );

            //newPosition.X = MathF.Round(
            //    newPosition.X / gridSize.Width) * gridSize.Width;

            if (gridSize.Height > 0f)
                newPosition = new Point2(
                    newPosition.X,
                    MathF.Round(
                    newPosition.Y / gridSize.Height) * gridSize.Height
                );
                //newPosition.Y = MathF.Round(
                //    newPosition.Y / gridSize.Height) * gridSize.Height;
        }
        else
        {
            newPosition = currentPosition + delta;
        }

        var movement = newPosition - currentPosition;

        if (movement == Point2.Zero)
            return;

        if (target is ISelectionMovable2 movableTarget &&
            movableTarget.AllowMove)
        {
            movableTarget.Position += movement;
        }

        if (definition is ISelectionMovable2Definition movableDefinitionTarget)
            movableDefinitionTarget.Position += movement;

        if (otherSelectedTargets != null)
        {
            foreach (var otherTarget in otherSelectedTargets)
            {
                if (otherTarget is ISelectionMovable2 otherMovable &&
                    otherMovable.AllowMove)
                {
                    otherMovable.Position += movement;
                }
            }
        }

        if (otherSelectedTargetDefinitions != null)
        {
            foreach (var otherDefinition in otherSelectedTargetDefinitions)
            {
                if (otherDefinition is ISelectionMovable2Definition otherMovableDefinition)
                    otherMovableDefinition.Position += movement;
            }
        }
    }
}