using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Tools.Selection;
using System;
using System.Collections.Generic;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

internal sealed class SelectionToolRotationHelper
{
    private readonly SelectionToolNode _topLeftNode;
    private readonly SelectionToolNode _topRightNode;
    private readonly SelectionToolNode _bottomRightNode;
    private readonly SelectionToolNode _bottomLeftNode;
    private readonly SelectionToolNode _pivotNode;

    private readonly SelectionToolNode[] _rotationNodes;

    private SelectionToolNode? _dragNode;
    private Vector2 _dragStartCursorPosition;
    private float _dragStartRotation;

    public SelectionToolRotationHelper()
    {
        _topLeftNode = CreateRotationNode();
        _topRightNode = CreateRotationNode();
        _bottomRightNode = CreateRotationNode();
        _bottomLeftNode = CreateRotationNode();

        _pivotNode = new SelectionToolNode(
            shape: SelectionToolNodeShape.Circle,
            isVisible: true);

        _rotationNodes =
        [
            _topLeftNode,
            _topRightNode,
            _bottomRightNode,
            _bottomLeftNode
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
        var pivot = new Point2(0.5f);

        if (target != null)
        {
            targetSize = target.Size;

            if (target is ISelectionRotatable2 rotatable)
                pivot = new Point2(rotatable.RotationPivot.X, rotatable.RotationPivot.Y);
        }
        else if (definition != null)
        {
            targetSize = definition.Size;

            if (definition is ISelectionRotatable2Definition rotatableDefinition)
                pivot = rotatableDefinition.RotationPivot;
        }
        else
        {
            targetSize = Size2.Zero;
        }

        float handleSize = context.HandleSize;
        float halfHandleSize = handleSize / 2f;
        float offset = handleSize * 2f;

        var nodeSize = new Size2(handleSize);

        foreach (var node in _rotationNodes)
            node.Size = nodeSize;

        _pivotNode.Size = nodeSize;

        _topLeftNode.Position = new Point2(
            -offset - halfHandleSize,
            -offset - halfHandleSize);

        _topRightNode.Position = new Point2(
            targetSize.Width + offset - halfHandleSize,
            -offset - halfHandleSize);

        _bottomRightNode.Position = new Point2(
            targetSize.Width + offset - halfHandleSize,
            targetSize.Height + offset - halfHandleSize);

        _bottomLeftNode.Position = new Point2(
            -offset - halfHandleSize,
            targetSize.Height + offset - halfHandleSize);

        _pivotNode.Position = new Point2(
            targetSize.Width * pivot.X - halfHandleSize,
            targetSize.Height * pivot.Y - halfHandleSize);
    }

    public bool AllowHandle(
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        if (!IsRotationNode(node) &&
            !ReferenceEquals(node, _pivotNode))
        {
            return false;
        }

        return target is ISelectionRotatable2 rotatable &&
               rotatable.AllowRotate ||
               definition is ISelectionRotatable2Definition;
    }

    public void OnNodeInteract(
        SelectionToolLayerContext context,
        SelectionToolNode node,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Vector2 cursorPosition,
        Vector2 localCursorPosition,
        Vector2 delta)
    {
        if (ReferenceEquals(node, _pivotNode))
        {
            HandlePivot(
                context,
                target,
                definition,
                localCursorPosition);

            return;
        }

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

        if (context.EnableAngleSnap &&
            context.AngleSnapStep > 0f)
        {
            rotation = MathF.Round(
                rotation / context.AngleSnapStep) *
                context.AngleSnapStep;
        }

        if (target is ISelectionRotatable2 targetRotatable &&
            targetRotatable.AllowRotate)
        {
            targetRotatable.Rotation = rotation;
        }

        if (definition is ISelectionRotatable2Definition definitionRotatable)
            definitionRotatable.Rotation = rotation;
    }

    private static void HandlePivot(
        SelectionToolLayerContext context,
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition,
        Vector2 localCursorPosition)
    {
        Size2 size;
        float rotation;
        Point2 oldPivot;

        if (target != null)
        {
            size = target.Size;

            if (target is not ISelectionRotatable2 rotatable ||
                !rotatable.AllowRotate)
            {
                return;
            }

            rotation = rotatable.Rotation;
            oldPivot = new Point2(rotatable.RotationPivot.X, rotatable.RotationPivot.Y);
        }
        else if (definition is ISelectionRotatable2Definition rotatableDefinition)
        {
            size = definition.Size;
            rotation = rotatableDefinition.Rotation;
            oldPivot = rotatableDefinition.RotationPivot;
        }
        else
        {
            return;
        }

        if (size.Width == 0f ||
            size.Height == 0f)
        {
            return;
        }

        var newPivot = new Vector2(
            localCursorPosition.X / size.Width,
            localCursorPosition.Y / size.Height);

        if (context.EnablePivotSnap)
        {
            if (context.PivotSnapStep.X > 0f)
            {
                newPivot.X = MathF.Round(
                    newPivot.X / context.PivotSnapStep.X) *
                    context.PivotSnapStep.X;
            }

            if (context.PivotSnapStep.Y > 0f)
            {
                newPivot.Y = MathF.Round(
                    newPivot.Y / context.PivotSnapStep.Y) *
                    context.PivotSnapStep.Y;
            }
        }

        var oldPivotPosition = new Vector2(
            size.Width * oldPivot.X,
            size.Height * oldPivot.Y);

        var newPivotPosition = new Vector2(
            size.Width * newPivot.X,
            size.Height * newPivot.Y);

        var pivotDelta =
            oldPivotPosition -
            newPivotPosition;

        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);

        var rotatedPivotDelta = new Vector2(
            pivotDelta.X * cos - pivotDelta.Y * sin,
            pivotDelta.X * sin + pivotDelta.Y * cos);

        var positionOffset =
            pivotDelta -
            rotatedPivotDelta;

        if (target is ISelectionRotatable2 targetRotatable &&
            targetRotatable.AllowRotate)
        {
            targetRotatable.RotationPivot = new Vector2(newPivot.X, newPivot.Y);

            if (target is ISelectionMovable2 movable &&
                movable.AllowMove)
            {
                movable.Position += positionOffset;
            }
        }

        if (definition is ISelectionRotatable2Definition definitionRotatable)
        {
            definitionRotatable.RotationPivot = new Point2(newPivot.X, newPivot.Y);

            if (definition is ISelectionMovable2Definition movableDefinition)
                movableDefinition.Position += positionOffset;
        }
    }

    private static Vector2 GetPivotWorldPosition(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        Vector2 position = Vector2.Zero;
        Size2 size = Size2.Zero;
        Point2 pivot = new Point2(0.5f);

        if (target != null)
        {
            size = target.Size;

            if (target is ISelectionMovable2 movable)
                position = movable.Position;

            if (target is ISelectionRotatable2 rotatable)
                pivot = new Point2(rotatable.RotationPivot.X, rotatable.RotationPivot.Y);
        }
        else if (definition != null)
        {
            size = definition.Size;

            if (definition is ISelectionMovable2Definition movableDefinition)
                position = movableDefinition.Position;

            if (definition is ISelectionRotatable2Definition rotatableDefinition)
                pivot = new Point2(rotatableDefinition.RotationPivot.X, rotatableDefinition.RotationPivot.Y);
        }

        return position + new Vector2(
            size.Width * pivot.X,
            size.Height * pivot.Y);
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