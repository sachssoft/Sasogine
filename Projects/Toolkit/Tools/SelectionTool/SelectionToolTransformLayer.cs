using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Tools.Selection;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

/// <summary>
/// Provides a selection layer that allows selected targets to be moved,
/// resized, and rotated.
/// </summary>
public sealed class SelectionToolTransformLayer : SelectionToolLayer
{
    private readonly SelectionToolMoveHelper _move;
    private readonly SelectionToolResizeHelper _resize;
    private readonly SelectionToolRotationHelper _rotation;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionToolTransformLayer"/> class.
    /// </summary>
    public SelectionToolTransformLayer()
    {
        _move = new SelectionToolMoveHelper();
        _resize = new SelectionToolResizeHelper();
        _rotation = new SelectionToolRotationHelper();

        Nodes.Add(_move.Node);

        foreach (var node in _resize.Nodes)
            Nodes.Add(node);

        foreach (var node in _rotation.Nodes)
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

        _rotation.OnTargetInvalidated(
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
                   definition) ||
               _rotation.AllowHandle(
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
        Point2 cursorPosition,
        Point2 delta)
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

        var position = GetPosition(
            target,
            definition);

        var localCursorPosition = InverseTransform(
            cursorPosition - position,
            target,
            definition);

        var previousLocalCursorPosition = InverseTransform(
            cursorPosition - delta - position,
            target,
            definition);

        var localDelta =
            localCursorPosition -
            previousLocalCursorPosition;

        if (_resize.AllowHandle(
            node,
            target,
            definition))
        {
            if (!_resize.OnNodeInteract(
                context,
                node,
                target,
                definition,
                localCursorPosition,
                localDelta,
                out var originOffset,
                out var oldSize,
                out var newSize))
            {
                return;
            }

            var positionOffset = TransformResizeOffset(
                originOffset,
                oldSize,
                newSize,
                target,
                definition);

            ApplyPositionOffset(
                positionOffset,
                target,
                definition);

            return;
        }

        if (_rotation.AllowHandle(
            node,
            target,
            definition))
        {
            _rotation.OnNodeInteract(
                context,
                node,
                target,
                definition,
                cursorPosition,
                localCursorPosition,
                delta);
        }
    }

    /// <inheritdoc/>
    protected internal override Point2 Transform(
        Point2 point,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        GetRotation(
            target,
            definition,
            out var size,
            out var rotation,
            out var pivot);

        var pivotPosition = new Point2(
            size.Width * pivot.X,
            size.Height * pivot.Y);

        var relative =
            point -
            pivotPosition;

        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);

        return pivotPosition + new Point2(
            relative.X * cos - relative.Y * sin,
            relative.X * sin + relative.Y * cos);
    }

    /// <inheritdoc/>
    protected internal override Point2 InverseTransform(
        Point2 point,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        GetRotation(
            target,
            definition,
            out var size,
            out var rotation,
            out var pivot);

        var pivotPosition = new Point2(
            size.Width * pivot.X,
            size.Height * pivot.Y);

        var relative = point - pivotPosition;

        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);

        return pivotPosition + new Point2(
            relative.X * cos + relative.Y * sin,
            -relative.X * sin + relative.Y * cos);
    }

    /// <inheritdoc/>
    protected internal override Vector2 TransformResizeOffset(
        Vector2 offset,
        Size2 oldSize,
        Size2 newSize,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        GetRotation(
            target,
            definition,
            out _,
            out var rotation,
            out var pivot);

        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);

        Vector2 Rotate(Vector2 value)
        {
            return new Vector2(
                value.X * cos - value.Y * sin,
                value.X * sin + value.Y * cos);
        }

        var oldPivot = new Vector2(
            oldSize.Width * pivot.X,
            oldSize.Height * pivot.Y);

        var newPivot = new Vector2(
            newSize.Width * pivot.X,
            newSize.Height * pivot.Y);

        var pivotDelta =
            oldPivot -
            newPivot;

        return Rotate(offset) +
            pivotDelta -
            Rotate(pivotDelta);
    }

    /// <inheritdoc/>
    protected internal override bool HitTestNode(
        Point2 position,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Point2 nodeWorldPosition)
    {
        if (!ReferenceEquals(node, _move.Node))
        {
            return base.HitTestNode(
                position,
                node,
                target,
                definition,
                nodeWorldPosition);
        }

        var targetPosition = GetPosition(
            target,
            definition);

        var localPosition = InverseTransform(
            position - targetPosition,
            target,
            definition);

        return localPosition.X >= node.Position.X &&
               localPosition.X <= node.Position.X + node.Size.Width &&
               localPosition.Y >= node.Position.Y &&
               localPosition.Y <= node.Position.Y + node.Size.Height;
    }

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

    private static void GetRotation(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        out Size2 size,
        out float rotation,
        out Point2 pivot)
    {
        if (target != null)
            size = target.Size;
        else if (definition != null)
            size = definition.Size;
        else
            size = Size2.Zero;

        rotation = 0f;
        pivot = new Point2(0.5f);

        if (target is ISelectionRotatable2 rotatable)
        {
            rotation = rotatable.Rotation;
            pivot = new Point2(rotatable.RotationPivot.X, rotatable.RotationPivot.Y);
        }
        else if (definition is ISelectionRotatable2Definition rotatableDefinition)
        {
            rotation = rotatableDefinition.Rotation;
            pivot = rotatableDefinition.RotationPivot;
        }
    }

    private static void ApplyPositionOffset(
        Vector2 offset,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (target is ISelectionMovable2 movable &&
            movable.AllowMove)
        {
            movable.Position += new Point2(offset.X, offset.Y);
        }

        if (definition is ISelectionMovable2Definition movableDefinition)
            movableDefinition.Position += new Point2(offset.X, offset.Y);
    }
}