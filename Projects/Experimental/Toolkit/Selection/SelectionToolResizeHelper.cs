using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Tools.Selection;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Selection;

internal sealed class SelectionToolResizeHelper
{
    private readonly SelectionToolNode _topLeftCornerNode;
    private readonly SelectionToolNode _topRightCornerNode;
    private readonly SelectionToolNode _bottomLeftCornerNode;
    private readonly SelectionToolNode _bottomRightCornerNode;
    private readonly SelectionToolNode _topEdgeNode;
    private readonly SelectionToolNode _leftEdgeNode;
    private readonly SelectionToolNode _rightEdgeNode;
    private readonly SelectionToolNode _bottomEdgeNode;

    private readonly SelectionToolNode[] _nodes;

    private SelectionToolNode? _dragNode;
    private Size2 _dragStartSize;
    private Vector2 _dragOffset;
    private Vector2 _appliedOriginOffset;

    public SelectionToolResizeHelper()
    {
        _topLeftCornerNode = CreateNode();
        _topRightCornerNode = CreateNode();
        _bottomLeftCornerNode = CreateNode();
        _bottomRightCornerNode = CreateNode();
        _topEdgeNode = CreateNode();
        _leftEdgeNode = CreateNode();
        _rightEdgeNode = CreateNode();
        _bottomEdgeNode = CreateNode();

        _nodes =
        [
            _topLeftCornerNode,
            _topRightCornerNode,
            _bottomLeftCornerNode,
            _bottomRightCornerNode,
            _topEdgeNode,
            _leftEdgeNode,
            _rightEdgeNode,
            _bottomEdgeNode
        ];
    }

    public IEnumerable<SelectionToolNode> Nodes => _nodes;

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

        var nodeSize = new Size2(handleSize);

        foreach (var node in _nodes)
            node.Size = nodeSize;

        _topLeftCornerNode.Position = new Vector2(
            -halfHandleSize,
            -halfHandleSize);

        _topRightCornerNode.Position = new Vector2(
            targetSize.Width - halfHandleSize,
            -halfHandleSize);

        _bottomLeftCornerNode.Position = new Vector2(
            -halfHandleSize,
            targetSize.Height - halfHandleSize);

        _bottomRightCornerNode.Position = new Vector2(
            targetSize.Width - halfHandleSize,
            targetSize.Height - halfHandleSize);

        _topEdgeNode.Position = new Vector2(
            targetSize.Width / 2f - halfHandleSize,
            -halfHandleSize);

        _leftEdgeNode.Position = new Vector2(
            -halfHandleSize,
            targetSize.Height / 2f - halfHandleSize);

        _rightEdgeNode.Position = new Vector2(
            targetSize.Width - halfHandleSize,
            targetSize.Height / 2f - halfHandleSize);

        _bottomEdgeNode.Position = new Vector2(
            targetSize.Width / 2f - halfHandleSize,
            targetSize.Height - halfHandleSize);
    }

    public bool AllowHandle(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (!IsNode(node))
            return false;

        return target is ISelectionResizable2 resizable &&
               resizable.AllowResize ||
               definition is ISelectionResizable2Definition;
    }

    public bool OnNodeInteract(
        SelectionToolLayerContext context,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Vector2 cursorPosition,
        Vector2 delta,
        out Vector2 originOffset,
        out Size2 oldSize,
        out Size2 newSize)
    {
        originOffset = Vector2.Zero;
        oldSize = Size2.Zero;
        newSize = Size2.Zero;

        if (!IsNode(node))
            return false;

        Size2 currentSize;

        if (target is ISelectionResizable2 resizable &&
            resizable.AllowResize)
        {
            currentSize = resizable.Size;
        }
        else if (definition is ISelectionResizable2Definition resizableDefinition)
        {
            currentSize = resizableDefinition.Size;
        }
        else
        {
            return false;
        }

        if (delta == Vector2.Zero ||
            !ReferenceEquals(_dragNode, node))
        {
            _dragNode = node;
            _dragStartSize = currentSize;
            _dragOffset = Vector2.Zero;
            _appliedOriginOffset = Vector2.Zero;
            return false;
        }

        _dragOffset += delta;

        float left = 0f;
        float top = 0f;
        float right = _dragStartSize.Width;
        float bottom = _dragStartSize.Height;

        bool resizeLeft =
            ReferenceEquals(node, _topLeftCornerNode) ||
            ReferenceEquals(node, _leftEdgeNode) ||
            ReferenceEquals(node, _bottomLeftCornerNode);

        bool resizeRight =
            ReferenceEquals(node, _topRightCornerNode) ||
            ReferenceEquals(node, _rightEdgeNode) ||
            ReferenceEquals(node, _bottomRightCornerNode);

        bool resizeTop =
            ReferenceEquals(node, _topLeftCornerNode) ||
            ReferenceEquals(node, _topEdgeNode) ||
            ReferenceEquals(node, _topRightCornerNode);

        bool resizeBottom =
            ReferenceEquals(node, _bottomLeftCornerNode) ||
            ReferenceEquals(node, _bottomEdgeNode) ||
            ReferenceEquals(node, _bottomRightCornerNode);

        if (resizeLeft)
            left += _dragOffset.X;

        if (resizeRight)
            right += _dragOffset.X;

        if (resizeTop)
            top += _dragOffset.Y;

        if (resizeBottom)
            bottom += _dragOffset.Y;

        if (context.EnableGridSnap)
        {
            if (context.GridSnapStep.Width > 0f)
            {
                if (resizeLeft)
                {
                    left = MathF.Round(
                        left / context.GridSnapStep.Width) *
                        context.GridSnapStep.Width;
                }

                if (resizeRight)
                {
                    right = MathF.Round(
                        right / context.GridSnapStep.Width) *
                        context.GridSnapStep.Width;
                }
            }

            if (context.GridSnapStep.Height > 0f)
            {
                if (resizeTop)
                {
                    top = MathF.Round(
                        top / context.GridSnapStep.Height) *
                        context.GridSnapStep.Height;
                }

                if (resizeBottom)
                {
                    bottom = MathF.Round(
                        bottom / context.GridSnapStep.Height) *
                        context.GridSnapStep.Height;
                }
            }
        }

        if (right < left)
            right = left;

        if (bottom < top)
            bottom = top;

        var totalOriginOffset = new Vector2(
            left,
            top);

        originOffset =
            totalOriginOffset -
            _appliedOriginOffset;

        _appliedOriginOffset = totalOriginOffset;

        oldSize = currentSize;

        newSize = new Size2(
            right - left,
            bottom - top);

        if (target is ISelectionResizable2 targetResizable &&
            targetResizable.AllowResize)
        {
            targetResizable.Size = newSize;
        }

        if (definition is ISelectionResizable2Definition definitionResizable)
            definitionResizable.Size = newSize;

        return true;
    }

    private bool IsNode(SelectionToolNode node)
    {
        foreach (var resizeNode in _nodes)
        {
            if (ReferenceEquals(node, resizeNode))
                return true;
        }

        return false;
    }

    private static SelectionToolNode CreateNode()
    {
        return new SelectionToolNode(
            shape: SelectionToolNodeShape.Quad,
            isVisible: true);
    }
}