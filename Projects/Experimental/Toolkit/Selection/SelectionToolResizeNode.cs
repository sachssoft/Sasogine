using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Tools.Selection;

public sealed class SelectionToolResizeNode : SelectionToolNode
{
    public SelectionToolResizeNode(Vector2 position, SelectionToolResizeHandle handle)
        : base(position)
    {
        Handle = handle;
    }

    public SelectionToolResizeHandle Handle { get; }
}