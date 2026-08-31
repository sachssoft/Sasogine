using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

/// <summary>
/// Defines a base class for selection tool layers that provide interaction nodes,
/// respond to target changes, and handle target-specific interactions.
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
    /// has been invalidated and may require its nodes to be updated.
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
    /// Determines whether the specified node can be used to interact with
    /// the specified selection target or its definition.
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
    /// <see langword="true"/> if the node can be used for the target or definition;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected internal virtual bool AllowHandle(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        return true;
    }

    /// <summary>
    /// Handles interaction with a selection tool node using the current
    /// selection tool settings.
    /// </summary>
    /// <param name="context">
    /// Provides the current selection tool settings required by the interaction.
    /// </param>
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
    protected internal virtual void OnNodeInteract(
        SelectionToolLayerContext context,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        IEnumerable<ISelectionTarget2>? otherSelectedTargets,
        IEnumerable<ISelectionTarget2Definition>? otherSelectedTargetDefinitions,
        Vector2 cursorPosition,
        Vector2 delta)
    {
    }


    /// <summary>
    /// Transforms a local point according to the transformation behavior
    /// provided by this layer.
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
    protected internal virtual Vector2 Transform(
        Vector2 point,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        return point;
    }
}