using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

public sealed class SelectionToolMoveLayer : SelectionToolLayer
{
    private readonly SelectionToolNode _moveNode;

    public SelectionToolMoveLayer()
    {
        _moveNode = new SelectionToolNode(
            SelectionToolNodeShape.Quad,
            supportsMultipleTargets: true);

        Nodes.Add(_moveNode);
    }

    protected override void OnTargetEnter(
        ISelectionTarget? target)
    {
        UpdateMoveNode(target);
    }

    protected internal override void OnTargetInvalidated(
        ISelectionTarget? target)
    {
        UpdateMoveNode(target);
    }

    protected internal override void OnNodeInteract(
        SelectionToolNode node)
    {
        if (!ReferenceEquals(node, _moveNode))
            return;

        // Move interaction
    }

    private void UpdateMoveNode(
        ISelectionTarget? target)
    {
        if (target is not ISelectionMovable2 movable)
            return;

        _moveNode.Position = movable.Position;
    }
}