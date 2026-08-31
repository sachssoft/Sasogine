using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

internal sealed class SelectionToolRotationHelper
{
    private readonly SelectionToolNode _topNode;
    private readonly SelectionToolNode _rightNode;
    private readonly SelectionToolNode _bottomNode;
    private readonly SelectionToolNode _leftNode;
    private readonly SelectionToolNode _pivotNode;

    private readonly SelectionToolNode[] _rotationNodes;

    private SelectionToolNode? _dragNode;
    private Vector2 _dragStartCursorPosition;
    private float _dragStartRotation;

    public SelectionToolRotationHelper()
    {
        _topNode = CreateRotationNode();
        _rightNode = CreateRotationNode();
        _bottomNode = CreateRotationNode();
        _leftNode = CreateRotationNode();

        _pivotNode = new SelectionToolNode(
            shape: SelectionToolNodeShape.Circle,
            isVisible: true);

        _rotationNodes =
        [
            _topNode,
            _rightNode,
            _bottomNode,
            _leftNode
        ];
    }

    public IEnumerable<SelectionToolNode> Nodes
    {
        get
        {
            foreach (var node in _rotationNodes)
                yield return node;

            yield return _pivotNode;
        }
    }

    public SelectionToolNode PivotNode => _pivotNode;

    public void OnTargetInvalidated(
        SelectionToolLayerContext context,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        Size2 targetSize;

        if (target != null)
            targetSize = target.Size;
        else if (definition != null)
            targetSize = definition.Size;
        else
            targetSize = Size2.Zero;

        float handleSize = context.HandleSize;
        float halfHandleSize = handleSize / 2f;
        float offset = handleSize * 2f;

        var nodeSize = new Size2(handleSize);

        foreach (var node in _rotationNodes)
            node.Size = nodeSize;

        _pivotNode.Size = nodeSize;

        _topNode.Position = new Vector2(
            targetSize.Width / 2f - halfHandleSize,
            -offset - halfHandleSize);

        _rightNode.Position = new Vector2(
            targetSize.Width + offset - halfHandleSize,
            targetSize.Height / 2f - halfHandleSize);

        _bottomNode.Position = new Vector2(
            targetSize.Width / 2f - halfHandleSize,
            targetSize.Height + offset - halfHandleSize);

        _leftNode.Position = new Vector2(
            -offset - halfHandleSize,
            targetSize.Height / 2f - halfHandleSize);

        _pivotNode.Position = new Vector2(
            targetSize.Width / 2f - halfHandleSize,
            targetSize.Height / 2f - halfHandleSize);
    }

    public bool AllowHandle(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (!IsRotationNode(node))
            return false;

        return (target is ISelectionRotatable2 rotatable &&
                rotatable.AllowRotate) ||
               definition is ISelectionRotatable2Definition;
    }

    public void OnNodeInteract(
        SelectionToolLayerContext context,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Vector2 cursorPosition,
        Vector2 delta)
    {
        if (!IsRotationNode(node))
            return;

        float currentRotation;

        if (target is ISelectionRotatable2 rotatable &&
            rotatable.AllowRotate)
        {
            currentRotation = rotatable.Rotation;
        }
        else if (definition is ISelectionRotatable2Definition rotatableDefinition)
        {
            currentRotation = rotatableDefinition.Rotation;
        }
        else
        {
            return;
        }

        var pivotPosition = GetPivotWorldPosition(
            target,
            definition);

        if (delta == Vector2.Zero ||
            !ReferenceEquals(_dragNode, node))
        {
            _dragNode = node;
            _dragStartCursorPosition = cursorPosition;
            _dragStartRotation = currentRotation;
            return;
        }

        float startAngle = MathF.Atan2(
            _dragStartCursorPosition.Y - pivotPosition.Y,
            _dragStartCursorPosition.X - pivotPosition.X);

        float currentAngle = MathF.Atan2(
            cursorPosition.Y - pivotPosition.Y,
            cursorPosition.X - pivotPosition.X);

        float rotation =
            _dragStartRotation +
            currentAngle -
            startAngle;

        if (target is ISelectionRotatable2 targetRotatable &&
            targetRotatable.AllowRotate)
        {
            targetRotatable.Rotation = rotation;
        }

        if (definition is ISelectionRotatable2Definition definitionRotatable)
            definitionRotatable.Rotation = rotation;
    }

    private Vector2 GetPivotWorldPosition(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        Vector2 position = Vector2.Zero;
        Size2 size = Size2.Zero;

        if (target != null)
        {
            size = target.Size;

            if (target is ISelectionMovable2 movable)
                position = movable.Position;
        }
        else if (definition != null)
        {
            size = definition.Size;

            if (definition is ISelectionMovable2Definition movableDefinition)
                position = movableDefinition.Position;
        }

        return position + new Vector2(
            size.Width / 2f,
            size.Height / 2f);
    }

    private bool IsRotationNode(SelectionToolNode node)
    {
        foreach (var rotationNode in _rotationNodes)
        {
            if (ReferenceEquals(node, rotationNode))
                return true;
        }

        return false;
    }

    private static SelectionToolNode CreateRotationNode()
    {
        return new SelectionToolNode(
            shape: SelectionToolNodeShape.Circle,
            isVisible: true);
    }
}