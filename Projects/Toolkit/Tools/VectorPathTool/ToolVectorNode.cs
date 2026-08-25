using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Tools
{
    public sealed class ToolVectorNode
    {
        public ToolVectorNode()
        {
        }

        public ToolVectorNode(Vector2 position)
        {
            Position = position;
        }

        public Vector2 Position { get; set; }

        public bool IsSelected { get; set; }
    }
}
