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
        }
    }
}