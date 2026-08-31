using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

/// <summary>
/// Provides a selection layer that allows selected targets to be moved,
/// optionally snapping their positions to the configured grid.
/// </summary>
public sealed class SelectionToolMoveLayer : SelectionToolLayer
{
    private readonly SelectionToolMoveHelper _move;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionToolMoveLayer"/> class.
    /// </summary>
    public SelectionToolMoveLayer()
    {
        _move = new SelectionToolMoveHelper();
        Nodes.Add(_move.Node);
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
        _move.OnNodeInteract(
            context,
            node,
            target,
            definition,
            otherSelectedTargets,
            otherSelectedTargetDefinitions,
            cursorPosition,
            delta);
    }
}