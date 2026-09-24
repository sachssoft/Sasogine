using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Provides rotation and rotation-pivot interaction behavior for selection tool layers.
/// </summary>
internal sealed class SelectionToolRotationHelper
{
    private readonly SelectionToolNode _topLeftNode;
    private readonly SelectionToolNode _topRightNode;
    private readonly SelectionToolNode _bottomRightNode;
    private readonly SelectionToolNode _bottomLeftNode;
    private readonly SelectionToolNode _pivotNode;
    private readonly SelectionToolNode[] _rotationNodes;

    private SelectionToolNode? _dragNode;
    private Point2 _dragStartPosition;
    private Size2 _dragStartSize;
    private float _dragStartRotation;
    private Point2 _dragStartPivot;
    private float _dragPreviousCursorAngle;
    private float _dragAccumulatedAngle;
    private bool _isDragging;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionToolRotationHelper"/> class.
    /// </summary>
    public SelectionToolRotationHelper()
    {
        _topLeftNode = CreateRotationNode();
        _topRightNode = CreateRotationNode();
        _bottomRightNode = CreateRotationNode();
        _bottomLeftNode = CreateRotationNode();

        _pivotNode = new SelectionToolNode(
            shape: SelectionToolNodeShape.Circle,
            isVisible: true)
        {
            HitPadding = 4f
        };

        _rotationNodes =
        [
            _topLeftNode,
            _topRightNode,
            _bottomRightNode,
            _bottomLeftNode
        ];
    }

    /// <summary>
    /// Gets all rotation and pivot interaction nodes.
    /// </summary>
    public IEnumerable<SelectionToolNode> Nodes
    {
        get
        {
            foreach (var node in _rotationNodes)
                yield return node;

            yield return _pivotNode;
        }
    }

    /// <summary>
    /// Gets the rotation pivot interaction node.
    /// </summary>
    public SelectionToolNode PivotNode => _pivotNode;

    /// <summary>
    /// Updates rotation and pivot nodes for the specified target or definition.
    /// </summary>
    /// <param name="context">The current selection tool layer context.</param>
    /// <param name="target">The runtime selection target, if available.</param>
    /// <param name="definition">The selection target definition, if available.</param>
    public void OnTargetInvalidated(
        SelectionToolLayerContext context,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        Size2 targetSize = definition?.Size ?? target?.Size ?? Size2.Zero;
        Point2 pivot = new Point2(0.5f);

        if (definition is ISelectionRotatable2Definition rotatableDefinition)
            pivot = rotatableDefinition.RotationPivot;
        else if (target is ISelectionRotatable2 rotatable)
            pivot = rotatable.RotationPivot;

        float handleSize = context.HandleSize;
        float halfHandleSize = handleSize / 2f;
        float offset = handleSize * 2f;
        var nodeSize = new Size2(handleSize);

        foreach (var node in _rotationNodes)
            node.Size = nodeSize;

        _pivotNode.Size = nodeSize;

        _topLeftNode.Position = new Point2(-offset - halfHandleSize, -offset - halfHandleSize);
        _topRightNode.Position = new Point2(targetSize.Width + offset - halfHandleSize, -offset - halfHandleSize);
        _bottomRightNode.Position = new Point2(targetSize.Width + offset - halfHandleSize, targetSize.Height + offset - halfHandleSize);
        _bottomLeftNode.Position = new Point2(-offset - halfHandleSize, targetSize.Height + offset - halfHandleSize);
        _pivotNode.Position = new Point2(targetSize.Width * pivot.X - halfHandleSize, targetSize.Height * pivot.Y - halfHandleSize);
    }

    /// <summary>
    /// Determines whether the specified rotation or pivot node can be used.
    /// </summary>
    /// <param name="node">The node to evaluate.</param>
    /// <param name="target">The runtime selection target, if available.</param>
    /// <param name="definition">The selection target definition, if available.</param>
    /// <returns><see langword="true"/> if rotation is supported; otherwise, <see langword="false"/>.</returns>
    public bool AllowHandle(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (!IsRotationNode(node) && !ReferenceEquals(node, _pivotNode))
            return false;

        if (target != null)
            return target is ISelectionRotatable2 rotatable && rotatable.AllowRotate;

        return definition is ISelectionRotatable2Definition;
    }

    /// <summary>
    /// Begins a rotation or pivot interaction.
    /// </summary>
    /// <param name="node">The node being manipulated.</param>
    /// <param name="target">The runtime selection target, if available.</param>
    /// <param name="definition">The selection target definition, if available.</param>
    /// <param name="cursorPosition">The initial cursor position in world space.</param>
    public void BeginInteraction(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Point2 cursorPosition)
    {
        if ((!IsRotationNode(node) && !ReferenceEquals(node, _pivotNode)) ||
            !TryGetTransform(target, definition, out _dragStartPosition, out _dragStartSize, out _dragStartRotation, out _dragStartPivot))
        {
            return;
        }

        _dragNode = node;
        _isDragging = true;

        if (ReferenceEquals(node, _pivotNode))
            return;

        Point2 pivotWorldPosition = GetPivotWorldPosition(_dragStartPosition, _dragStartSize, _dragStartPivot);
        Vector2 pivotToCursor = cursorPosition - pivotWorldPosition;

        if (pivotToCursor.LengthSquared() <= float.Epsilon)
        {
            _isDragging = false;
            _dragNode = null;
            return;
        }

        _dragPreviousCursorAngle = MathF.Atan2(pivotToCursor.Y, pivotToCursor.X);
        _dragAccumulatedAngle = 0f;
    }

    /// <summary>
    /// Updates the active rotation or pivot interaction.
    /// </summary>
    /// <param name="context">The current selection tool layer context.</param>
    /// <param name="node">The node being manipulated.</param>
    /// <param name="target">The runtime selection target, if available.</param>
    /// <param name="definition">The selection target definition, if available.</param>
    /// <param name="cursorPosition">The current cursor position in world space.</param>
    /// <param name="localCursorPosition">The current cursor position in the drag-start local coordinate space.</param>
    public void OnNodeInteract(
        SelectionToolLayerContext context,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Point2 cursorPosition,
        Point2 localCursorPosition)
    {
        if (!_isDragging || !ReferenceEquals(_dragNode, node))
            return;

        if (ReferenceEquals(node, _pivotNode))
        {
            HandlePivot(context, target, definition, localCursorPosition);
            return;
        }

        if (!IsRotationNode(node))
            return;

        Point2 pivotWorldPosition = GetPivotWorldPosition(_dragStartPosition, _dragStartSize, _dragStartPivot);
        Vector2 pivotToCursor = cursorPosition - pivotWorldPosition;

        if (pivotToCursor.LengthSquared() <= float.Epsilon)
            return;

        float cursorAngle = MathF.Atan2(pivotToCursor.Y, pivotToCursor.X);
        _dragAccumulatedAngle += NormalizeAngle(cursorAngle - _dragPreviousCursorAngle);
        _dragPreviousCursorAngle = cursorAngle;

        float rotation = _dragStartRotation + _dragAccumulatedAngle;

        if (context.EnableAngleSnap && context.AngleSnapStep > 0f)
        {
            rotation = MathF.Round(
                rotation / context.AngleSnapStep,
                MidpointRounding.AwayFromZero) * context.AngleSnapStep;
        }

        SetRotation(target, definition, rotation);
    }

    /// <summary>
    /// Ends the current rotation or pivot interaction.
    /// </summary>
    public void EndInteraction()
    {
        _dragNode = null;
        _isDragging = false;
    }

    private void HandlePivot(
        SelectionToolLayerContext context,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Point2 localCursorPosition)
    {
        if (_dragStartSize.Width == 0f || _dragStartSize.Height == 0f)
            return;

        Point2 newPivot = new Point2(
            localCursorPosition.X / _dragStartSize.Width,
            localCursorPosition.Y / _dragStartSize.Height);

        if (context.EnablePivotSnap)
        {
            if (context.PivotSnapStep.X > 0f)
            {
                newPivot = new Point2(
                    MathF.Round(newPivot.X / context.PivotSnapStep.X, MidpointRounding.AwayFromZero) * context.PivotSnapStep.X,
                    newPivot.Y);
            }

            if (context.PivotSnapStep.Y > 0f)
            {
                newPivot = new Point2(
                    newPivot.X,
                    MathF.Round(newPivot.Y / context.PivotSnapStep.Y, MidpointRounding.AwayFromZero) * context.PivotSnapStep.Y);
            }
        }

        Point2 oldPivotPosition = new Point2(
            _dragStartSize.Width * _dragStartPivot.X,
            _dragStartSize.Height * _dragStartPivot.Y);

        Point2 newPivotPosition = new Point2(
            _dragStartSize.Width * newPivot.X,
            _dragStartSize.Height * newPivot.Y);

        Vector2 pivotDelta = oldPivotPosition - newPivotPosition;
        float cos = MathF.Cos(_dragStartRotation);
        float sin = MathF.Sin(_dragStartRotation);
        Vector2 rotatedPivotDelta = new Vector2(
            pivotDelta.X * cos - pivotDelta.Y * sin,
            pivotDelta.X * sin + pivotDelta.Y * cos);

        Vector2 positionOffset = pivotDelta - rotatedPivotDelta;
        SetPivot(target, definition, newPivot);
        SetPosition(target, definition, _dragStartPosition + positionOffset);
    }

    private static bool TryGetTransform(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        out Point2 position,
        out Size2 size,
        out float rotation,
        out Point2 pivot)
    {
        position = Point2.Zero;
        size = definition?.Size ?? target?.Size ?? Size2.Zero;
        rotation = 0f;
        pivot = new Point2(0.5f);

        if (definition is ISelectionMovable2Definition movableDefinition)
            position = movableDefinition.Position;
        else if (target is ISelectionMovable2 movable)
            position = movable.Position;

        if (definition is ISelectionRotatable2Definition rotatableDefinition)
        {
            rotation = rotatableDefinition.Rotation;
            pivot = rotatableDefinition.RotationPivot;
            return true;
        }

        if (target is ISelectionRotatable2 rotatable && rotatable.AllowRotate)
        {
            rotation = rotatable.Rotation;
            pivot = rotatable.RotationPivot;
            return true;
        }

        return false;
    }

    private static Point2 GetPivotWorldPosition(Point2 position, Size2 size, Point2 pivot)
    {
        return position + new Vector2(size.Width * pivot.X, size.Height * pivot.Y);
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > MathF.PI)
            angle -= MathHelper.TwoPi;
        while (angle < -MathF.PI)
            angle += MathHelper.TwoPi;
        return angle;
    }

    private static void SetRotation(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        float rotation)
    {
        if (definition is ISelectionRotatable2Definition rotatableDefinition)
        {
            rotatableDefinition.Rotation = rotation;
            return;
        }

        if (target is ISelectionRotatable2 rotatable && rotatable.AllowRotate)
        {
            if (target.Definition is not ISelectionRotatable2Definition targetDefinition)
                throw new InvalidOperationException($"The rotatable selection target requires an '{nameof(ISelectionRotatable2Definition)}' definition.");

            targetDefinition.Rotation = rotation;
        }
    }

    private static void SetPivot(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Point2 pivot)
    {
        if (definition is ISelectionRotatable2Definition rotatableDefinition)
        {
            rotatableDefinition.RotationPivot = pivot;
            return;
        }

        if (target is ISelectionRotatable2 rotatable && rotatable.AllowRotate)
        {
            if (target.Definition is not ISelectionRotatable2Definition targetDefinition)
                throw new InvalidOperationException($"The rotatable selection target requires an '{nameof(ISelectionRotatable2Definition)}' definition.");

            targetDefinition.RotationPivot = pivot;
        }
    }

    private static void SetPosition(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Point2 position)
    {
        if (definition is ISelectionMovable2Definition movableDefinition)
        {
            movableDefinition.Position = position;
            return;
        }

        if (target is ISelectionMovable2 movable && movable.AllowMove)
        {
            if (target.Definition is not ISelectionMovable2Definition targetDefinition)
                throw new InvalidOperationException($"The movable selection target requires an '{nameof(ISelectionMovable2Definition)}' definition.");

            targetDefinition.Position = position;
        }
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
            isVisible: true)
        {
            HitPadding = 4f
        };
    }
}
