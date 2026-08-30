using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

public sealed class SelectionToolNode
{
    public SelectionToolNode(
        SelectionToolNodeShape shape)
    {
        Shape = shape;
    }

    public SelectionToolNode(
        SelectionToolNodeShape shape,
        Vector2 position,
        Size2 size)
    {
        Shape = shape;
        Position = position;
        Size = size;
    }

    public SelectionToolNodeShape Shape { get; }

    public Vector2 Position { get; set; }

    public Size2 Size { get; set; }

    public bool IsVisible { get; set; } = true;
}