using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using System;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

public sealed class SelectionToolMoveResizeLayer : SelectionToolLayer
{
    private readonly SelectionToolNode _moveNode;
    private readonly SelectionToolNode _topLeftCornerNode;
    private readonly SelectionToolNode _topRightCornerNode;
    private readonly SelectionToolNode _bottomLeftCornerNode;
    private readonly SelectionToolNode _bottomRightCornerNode;
    private readonly SelectionToolNode _topEdgeNode;
    private readonly SelectionToolNode _leftEdgeNode;
    private readonly SelectionToolNode _rightEdgeNode;
    private readonly SelectionToolNode _bottomEdgeNode;

    public SelectionToolMoveResizeLayer()
    {
        _moveNode = new SelectionToolNode(
            SelectionToolNodeShape.Quad,
            Vector2.Zero,
            new Size2(8f));

        _topLeftCornerNode = new SelectionToolNode(
            SelectionToolNodeShape.Quad,
            Vector2.Zero,
            new Size2(8f));

        _topRightCornerNode = new SelectionToolNode(
            SelectionToolNodeShape.Quad,
            Vector2.Zero,
            new Size2(8f));

        _bottomLeftCornerNode = new SelectionToolNode(
            SelectionToolNodeShape.Quad,
            Vector2.Zero,
            new Size2(8f));

        _bottomRightCornerNode = new SelectionToolNode(
            SelectionToolNodeShape.Quad,
            Vector2.Zero,
            new Size2(8f));

        _topEdgeNode = new SelectionToolNode(
            SelectionToolNodeShape.Quad,
            Vector2.Zero,
            new Size2(8f));

        _leftEdgeNode = new SelectionToolNode(
            SelectionToolNodeShape.Quad,
            Vector2.Zero,
            new Size2(8f));

        _rightEdgeNode = new SelectionToolNode(
            SelectionToolNodeShape.Quad,
            Vector2.Zero,
            new Size2(8f));

        _bottomEdgeNode = new SelectionToolNode(
            SelectionToolNodeShape.Quad,
            Vector2.Zero,
            new Size2(8f));

        Nodes.Add(_moveNode);
        Nodes.Add(_topLeftCornerNode);
        Nodes.Add(_topRightCornerNode);
        Nodes.Add(_bottomLeftCornerNode);
        Nodes.Add(_bottomRightCornerNode);
        Nodes.Add(_topEdgeNode);
        Nodes.Add(_leftEdgeNode);
        Nodes.Add(_rightEdgeNode);
        Nodes.Add(_bottomEdgeNode);
    }

    internal protected override void OnNodeInteract(
        SelectionToolNode node)
    {
        if (ReferenceEquals(node, _moveNode))
        {
            // Move
        }
        else if (ReferenceEquals(node, _topLeftCornerNode))
        {
            // Resize top-left
        }
        else if (ReferenceEquals(node, _topRightCornerNode))
        {
            // Resize top-right
        }
        else if (ReferenceEquals(node, _bottomLeftCornerNode))
        {
            // Resize bottom-left
        }
        else if (ReferenceEquals(node, _bottomRightCornerNode))
        {
            // Resize bottom-right
        }
        else if (ReferenceEquals(node, _topEdgeNode))
        {
            // Resize top
        }
        else if (ReferenceEquals(node, _leftEdgeNode))
        {
            // Resize left
        }
        else if (ReferenceEquals(node, _rightEdgeNode))
        {
            // Resize right
        }
        else if (ReferenceEquals(node, _bottomEdgeNode))
        {
            // Resize bottom
        }
    }
}