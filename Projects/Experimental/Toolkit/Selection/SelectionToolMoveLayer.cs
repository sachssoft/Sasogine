using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

/// <summary>
/// Provides a selection layer that allows selected targets to be moved.
/// </summary>
public sealed class SelectionToolMoveLayer : SelectionToolLayer
{
    private readonly SelectionToolNode _moveNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionToolMoveLayer"/> class.
    /// </summary>
    public SelectionToolMoveLayer()
    {
        _moveNode = new SelectionToolNode(
            SelectionToolNodeShape.Quad)
        {
            IsVisible = true
        };

        Nodes.Add(_moveNode);
    }

    /// <summary>
    /// Updates the move node to match the specified target or definition.
    /// The node position is always relative to the target and therefore remains at the origin.
    /// </summary>
    /// <param name="target">
    /// The runtime selection target, if available.
    /// </param>
    /// <param name="definition">
    /// The selection target definition, if available.
    /// </param>
    protected internal override void OnTargetInvalidated(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (target != null)
        {
            _moveNode.Position = Vector2.Zero;
            _moveNode.Size = target.Size;
            return;
        }

        if (definition != null)
        {
            _moveNode.Position = Vector2.Zero;
            _moveNode.Size = definition.Size;
            return;
        }

        _moveNode.Position = Vector2.Zero;
        _moveNode.Size = Size2.Zero;
    }

    /// <summary>
    /// Determines whether the move node can be used for the specified target.
    /// </summary>
    /// <param name="node">
    /// The selection tool node.
    /// </param>
    /// <param name="target">
    /// The runtime selection target, if available.
    /// </param>
    /// <param name="definition">
    /// The selection target definition, if available.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the target supports movement; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    protected internal override bool AllowHandle(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        return (target is ISelectionMovable2 movable &&
                movable.AllowMove) ||
               definition is ISelectionMovable2Definition;
    }

    /// <summary>
    /// Moves the target and other selected targets by the specified delta.
    /// </summary>
    /// <param name="node">
    /// The selection tool node being interacted with.
    /// </param>
    /// <param name="target">
    /// The primary runtime selection target, if available.
    /// </param>
    /// <param name="definition">
    /// The primary selection target definition, if available.
    /// </param>
    /// <param name="otherSelectedTargets">
    /// The other selected runtime targets.
    /// </param>
    /// <param name="otherSelectedTargetDefinitions">
    /// The definitions of the other selected targets.
    /// </param>
    /// <param name="cursorPosition">
    /// The current cursor position.
    /// </param>
    /// <param name="delta">
    /// The movement delta since the previous interaction.
    /// </param>
    protected internal override void OnNodeInteract(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        IEnumerable<ISelectionTarget2>? otherSelectedTargets,
        IEnumerable<ISelectionTarget2Definition>? otherSelectedTargetDefinitions,
        Vector2 cursorPosition,
        Vector2 delta)
    {
        if (!ReferenceEquals(node, _moveNode))
            return;

        if (delta == Vector2.Zero)
            return;

        if (target is ISelectionMovable2 movable &&
            movable.AllowMove)
        {
            movable.Position += delta;
        }

        if (definition is ISelectionMovable2Definition movableDefinition)
        {
            movableDefinition.Position += delta;
        }

        if (otherSelectedTargets != null)
        {
            foreach (var otherTarget in otherSelectedTargets)
            {
                if (otherTarget is ISelectionMovable2 otherMovable &&
                    otherMovable.AllowMove)
                {
                    otherMovable.Position += delta;
                }
            }
        }

        if (otherSelectedTargetDefinitions != null)
        {
            foreach (var otherDefinition in otherSelectedTargetDefinitions)
            {
                if (otherDefinition is ISelectionMovable2Definition otherMovableDefinition)
                {
                    otherMovableDefinition.Position += delta;
                }
            }
        }
    }
}