using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System;
using System.Collections.Generic;
using Sachssoft.Sasogine.Components.Tools.Selection;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Selection;

internal sealed class SelectionToolRotationHelper
{
    private readonly SelectionToolNode _topLeftNode;
    private readonly SelectionToolNode _topRightNode;
    private readonly SelectionToolNode _bottomRightNode;
    private readonly SelectionToolNode _bottomLeftNode;
    private readonly SelectionToolNode _pivotNode;

    private readonly SelectionToolNode[] _rotationNodes;

    private SelectionToolNode? _dragNode;
    private float _dragRotationOffset;

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
        Point2 pivot = new Point2(0.5f);

        if (target != null)
        {
            targetSize = target.Size;

            if (target is ISelectionRotatable2 rotatable)
                pivot = rotatable.RotationPivot;
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
        Point2 cursorPosition,
        Point2 localCursorPosition,
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

        Point2 pivotPosition = GetPivotWorldPosition(
            target,
            definition);

        Vector2 pivotToCursor =
            cursorPosition.ToVector2() -
            pivotPosition.ToVector2();

        if (pivotToCursor.LengthSquared() <= float.Epsilon)
            return;

        float cursorAngle = MathF.Atan2(
            pivotToCursor.Y,
            pivotToCursor.X);

        if (delta == Vector2.Zero ||
            !ReferenceEquals(_dragNode, node))
        {
            _dragNode = node;
            _dragRotationOffset =
                currentRotation - cursorAngle;
        }

        // The cursor always remains unsnapped. The desired rotation is
        // calculated directly from the line from the pivot to the cursor.
        float rotation =
            cursorAngle + _dragRotationOffset;

        if (context.EnableAngleSnap &&
            context.AngleSnapStep > 0f)
        {
            rotation = MathF.Round(
                rotation / context.AngleSnapStep,
                MidpointRounding.AwayFromZero) *
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
        Point2 localCursorPosition)
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
            oldPivot = rotatable.RotationPivot;
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

        Point2 newPivot = new Point2(
            localCursorPosition.X / size.Width,
            localCursorPosition.Y / size.Height);

        if (context.EnablePivotSnap)
        {
            if (context.PivotSnapStep.X > 0f)
            {
                newPivot = new Point2(
                    MathF.Round(
                        newPivot.X /
                        context.PivotSnapStep.X,
                        MidpointRounding.AwayFromZero) *
                        context.PivotSnapStep.X,
                    newPivot.Y);
            }

            if (context.PivotSnapStep.Y > 0f)
            {
                newPivot = new Point2(
                    newPivot.X,
                    MathF.Round(
                        newPivot.Y /
                        context.PivotSnapStep.Y,
                        MidpointRounding.AwayFromZero) *
                        context.PivotSnapStep.Y);
            }
        }

        Point2 oldPivotPosition = new Point2(
            size.Width * oldPivot.X,
            size.Height * oldPivot.Y);

        Point2 newPivotPosition = new Point2(
            size.Width * newPivot.X,
            size.Height * newPivot.Y);

        Vector2 pivotDelta =
            oldPivotPosition -
            newPivotPosition;

        float cos = MathF.Cos(rotation);
        float sin = MathF.Sin(rotation);

        Vector2 rotatedPivotDelta = new Vector2(
            pivotDelta.X * cos - pivotDelta.Y * sin,
            pivotDelta.X * sin + pivotDelta.Y * cos);

        Vector2 positionOffset =
            pivotDelta -
            rotatedPivotDelta;

        if (target is ISelectionRotatable2 targetRotatable &&
            targetRotatable.AllowRotate)
        {
            targetRotatable.RotationPivot = newPivot;

            if (target is ISelectionMovable2 movable &&
                movable.AllowMove)
            {
                movable.Position += positionOffset;
            }
        }

        if (definition is ISelectionRotatable2Definition definitionRotatable)
        {
            definitionRotatable.RotationPivot = newPivot;

            if (definition is ISelectionMovable2Definition movableDefinition)
                movableDefinition.Position += positionOffset;
        }
    }

    private static Point2 GetPivotWorldPosition(
        ISelectionTarget2? target,
        ISelectionTarget2Definition? definition)
    {
        Point2 position = Point2.Zero;
        Size2 size = Size2.Zero;
        Point2 pivot = new Point2(0.5f);

        if (target != null)
        {
            size = target.Size;

            if (target is ISelectionMovable2 movable)
                position = movable.Position;

            if (target is ISelectionRotatable2 rotatable)
                pivot = rotatable.RotationPivot;
        }
        else if (definition != null)
        {
            size = definition.Size;

            if (definition is ISelectionMovable2Definition movableDefinition)
                position = movableDefinition.Position;

            if (definition is ISelectionRotatable2Definition rotatableDefinition)
                pivot = rotatableDefinition.RotationPivot;
        }

        Vector2 pivotOffset = new Vector2(
            size.Width * pivot.X,
            size.Height * pivot.Y);

        return position + pivotOffset;
    }

    private bool IsRotationNode(
        SelectionToolNode node)
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
