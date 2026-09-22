using Microsoft.Xna.Framework;
using Sachssoft.Engine.Common;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Provides resize interaction behavior for selection tool layers.
/// </summary>
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
    private Size2 _appliedSize;
    private Point2 _dragStartCursorPosition;
    private Vector2 _appliedOriginOffset;
    private bool _isDragging;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionToolResizeHelper"/> class.
    /// </summary>
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

    /// <summary>
    /// Gets the resize interaction nodes.
    /// </summary>
    public IEnumerable<SelectionToolNode> Nodes => _nodes;

    /// <summary>
    /// Updates the resize nodes for the specified target or definition.
    /// </summary>
    /// <param name="context">The current selection tool layer context.</param>
    /// <param name="target">The runtime selection target, if available.</param>
    /// <param name="definition">The selection target definition, if available.</param>
    public void OnTargetInvalidated(
        SelectionToolLayerContext context,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        Size2 targetSize;

        if (definition != null)
            targetSize = definition.Size;
        else if (target != null)
            targetSize = target.Size;
        else
            targetSize = Size2.Zero;

        float handleSize = context.HandleSize;
        float halfHandleSize = handleSize / 2f;

        var nodeSize = new Size2(handleSize);

        foreach (var node in _nodes)
            node.Size = nodeSize;

        _topLeftCornerNode.Position = new Point2(
            -halfHandleSize,
            -halfHandleSize);

        _topRightCornerNode.Position = new Point2(
            targetSize.Width - halfHandleSize,
            -halfHandleSize);

        _bottomLeftCornerNode.Position = new Point2(
            -halfHandleSize,
            targetSize.Height - halfHandleSize);

        _bottomRightCornerNode.Position = new Point2(
            targetSize.Width - halfHandleSize,
            targetSize.Height - halfHandleSize);

        _topEdgeNode.Position = new Point2(
            targetSize.Width / 2f - halfHandleSize,
            -halfHandleSize);

        _leftEdgeNode.Position = new Point2(
            -halfHandleSize,
            targetSize.Height / 2f - halfHandleSize);

        _rightEdgeNode.Position = new Point2(
            targetSize.Width - halfHandleSize,
            targetSize.Height / 2f - halfHandleSize);

        _bottomEdgeNode.Position = new Point2(
            targetSize.Width / 2f - halfHandleSize,
            targetSize.Height - halfHandleSize);
    }

    /// <summary>
    /// Determines whether the specified resize node can be used.
    /// </summary>
    /// <param name="node">The node to evaluate.</param>
    /// <param name="target">The runtime selection target, if available.</param>
    /// <param name="definition">The selection target definition, if available.</param>
    /// <returns><see langword="true"/> if resizing is supported; otherwise, <see langword="false"/>.</returns>
    public bool AllowHandle(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (!IsNode(node))
            return false;

        if (target != null)
            return target is ISelectionResizable2 resizable && resizable.AllowResize;

        return definition is ISelectionResizable2Definition;
    }

    /// <summary>
    /// Begins a resize interaction.
    /// </summary>
    /// <param name="node">The resize node being manipulated.</param>
    /// <param name="target">The runtime selection target, if available.</param>
    /// <param name="definition">The selection target definition, if available.</param>
    /// <param name="cursorPosition">The initial cursor position in local target space.</param>
    public void BeginInteraction(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Point2 cursorPosition)
    {
        if (!IsNode(node) || !TryGetSize(target, definition, out var size))
            return;

        _dragNode = node;
        _dragStartSize = size;
        _appliedSize = size;
        _dragStartCursorPosition = cursorPosition;
        _appliedOriginOffset = Vector2.Zero;
        _isDragging = true;
    }

    /// <summary>
    /// Updates the active resize interaction.
    /// </summary>
    /// <param name="context">The current selection tool layer context.</param>
    /// <param name="node">The resize node being manipulated.</param>
    /// <param name="target">The runtime selection target, if available.</param>
    /// <param name="definition">The selection target definition, if available.</param>
    /// <param name="cursorPosition">The current cursor position in local target space.</param>
    /// <param name="originOffset">Receives the incremental local origin offset.</param>
    /// <param name="oldSize">Receives the previously applied size.</param>
    /// <param name="newSize">Receives the newly applied size.</param>
    /// <returns><see langword="true"/> if the resize state changed; otherwise, <see langword="false"/>.</returns>
    public bool OnNodeInteract(
        SelectionToolLayerContext context,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Point2 cursorPosition,
        out Vector2 originOffset,
        out Size2 oldSize,
        out Size2 newSize)
    {
        originOffset = Vector2.Zero;
        oldSize = _appliedSize;
        newSize = _appliedSize;

        if (!_isDragging || !ReferenceEquals(_dragNode, node))
            return false;

        Vector2 dragOffset = cursorPosition - _dragStartCursorPosition;
        float left = 0f;
        float top = 0f;
        float right = _dragStartSize.Width;
        float bottom = _dragStartSize.Height;

        bool resizeLeft = ReferenceEquals(node, _topLeftCornerNode) || ReferenceEquals(node, _leftEdgeNode) || ReferenceEquals(node, _bottomLeftCornerNode);
        bool resizeRight = ReferenceEquals(node, _topRightCornerNode) || ReferenceEquals(node, _rightEdgeNode) || ReferenceEquals(node, _bottomRightCornerNode);
        bool resizeTop = ReferenceEquals(node, _topLeftCornerNode) || ReferenceEquals(node, _topEdgeNode) || ReferenceEquals(node, _topRightCornerNode);
        bool resizeBottom = ReferenceEquals(node, _bottomLeftCornerNode) || ReferenceEquals(node, _bottomEdgeNode) || ReferenceEquals(node, _bottomRightCornerNode);

        if (resizeLeft)
            left += dragOffset.X;
        if (resizeRight)
            right += dragOffset.X;
        if (resizeTop)
            top += dragOffset.Y;
        if (resizeBottom)
            bottom += dragOffset.Y;

        if (context.EnableGridSnap)
        {
            if (context.GridSnapStep.Width > 0f)
            {
                if (resizeLeft)
                    left = MathF.Round(left / context.GridSnapStep.Width, MidpointRounding.AwayFromZero) * context.GridSnapStep.Width;
                if (resizeRight)
                    right = MathF.Round(right / context.GridSnapStep.Width, MidpointRounding.AwayFromZero) * context.GridSnapStep.Width;
            }

            if (context.GridSnapStep.Height > 0f)
            {
                if (resizeTop)
                    top = MathF.Round(top / context.GridSnapStep.Height, MidpointRounding.AwayFromZero) * context.GridSnapStep.Height;
                if (resizeBottom)
                    bottom = MathF.Round(bottom / context.GridSnapStep.Height, MidpointRounding.AwayFromZero) * context.GridSnapStep.Height;
            }
        }

        if (right < left)
            right = left;
        if (bottom < top)
            bottom = top;

        Vector2 totalOriginOffset = new Vector2(left, top);
        newSize = new Size2(right - left, bottom - top);
        originOffset = totalOriginOffset - _appliedOriginOffset;
        oldSize = _appliedSize;

        if (originOffset == Vector2.Zero &&
            newSize.Width == _appliedSize.Width &&
            newSize.Height == _appliedSize.Height)
        {
            return false;
        }

        SetSize(target, definition, newSize);
        _appliedOriginOffset = totalOriginOffset;
        _appliedSize = newSize;
        return true;
    }

    /// <summary>
    /// Ends the current resize interaction.
    /// </summary>
    public void EndInteraction()
    {
        _dragNode = null;
        _isDragging = false;
    }

    private static bool TryGetSize(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        out Size2 size)
    {
        if (definition is ISelectionResizable2Definition resizableDefinition)
        {
            size = resizableDefinition.Size;
            return true;
        }

        if (target is ISelectionResizable2 resizable && resizable.AllowResize)
        {
            size = resizable.Size;
            return true;
        }

        size = Size2.Zero;
        return false;
    }

    private static void SetSize(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Size2 size)
    {
        if (definition is ISelectionResizable2Definition resizableDefinition)
        {
            resizableDefinition.Size = size;
            return;
        }

        if (target is ISelectionResizable2 resizable && resizable.AllowResize)
        {
            if (target.Definition is not ISelectionResizable2Definition targetDefinition)
                throw new InvalidOperationException($"The resizable selection target requires an '{nameof(ISelectionResizable2Definition)}' definition.");

            targetDefinition.Size = size;
        }
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
            isVisible: true)
        {
            HitPadding = 2f
        };
    }
}