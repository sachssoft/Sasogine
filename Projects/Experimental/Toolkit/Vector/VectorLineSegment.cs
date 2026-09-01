using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>Represents a straight line segment between two vector path nodes.</summary>
    public class VectorLineSegment : VectorFixedSegment
    {
        public VectorLineSegment()
            : base(0)
        {
        }

        public VectorLineSegment(
            Vector2 position)
            : this(position, false)
        {
        }

        public VectorLineSegment(
            Vector2 position,
            bool isSelected)
            : this()
        {
            Node.Position = position;
            Node.IsSelected = isSelected;
        }

        /// <summary>Generates the vertices of the line segment between the specified start position and the segment endpoint.</summary>
        /// <param name="startPosition">The start position of the line segment.</param>
        /// <param name="sampleLength">The desired sampling distance. This parameter is not required for a straight line.</param>
        /// <returns>An array containing the start and end positions of the line segment.</returns>
        public override Vector2[] GetVertices(
            Vector2 startPosition,
            float sampleLength)
        {
            return
            [
                startPosition,
                Node.Position
            ];
        }
    }
}