using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
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

        if (_resize.AllowHandle(
            node,
            target,
            definition))
        {
            _resize.OnNodeInteract(
                context,
                node,
                target,
                definition,
                cursorPosition,
                delta);

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
                delta);
        }
    }

    /// <inheritdoc/>
    protected internal override Vector2 Transform(
        Vector2 point,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        Size2 size = Size2.Zero;
        float rotation = 0f;
        Vector2 pivot = new Vector2(0.5f);

        if (target != null)
        {
            size = target.Size;

            if (target is ISelectionRotatable2 rotatable)
            {
                rotation = rotatable.Rotation;
                pivot = rotatable.RotationPivot;
            }
        }
        else if (definition != null)
        {
            size = definition.Size;

            if (definition is ISelectionRotatable2Definition rotatableDefinition)
            {
                rotation = rotatableDefinition.Rotation;
                pivot = rotatableDefinition.RotationPivot;
            }
        }

        var pivotPosition = new Vector2(
            size.Width * pivot.X,
            size.Height * pivot.Y);

        var relative = point - pivotPosition;

        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);

        return pivotPosition + new Vector2(
            relative.X * cos - relative.Y * sin,
            relative.X * sin + relative.Y * cos);
    }
}