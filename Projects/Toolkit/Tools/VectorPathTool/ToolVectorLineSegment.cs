using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Tools
{
    public class ToolVectorLineSegment : ToolVectorFixedSegment
    {
        public ToolVectorLineSegment()
            : base(0)
        {
        }

        public ToolVectorLineSegment(
            Vector2 position)
            : this(position, false)
        {
        }

        public ToolVectorLineSegment(Vector2 position, bool isSelected) : this()
        {
            Node.Position = position;
            Node.IsSelected = isSelected;
        }

        public override Vector2[] GetVertices(Vector2 startPosition, float sampleLength)
        {
            return [];
        }
    }
}