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
    }
}