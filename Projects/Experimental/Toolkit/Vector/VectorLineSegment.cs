using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Experimental.Components.Tools.Vector
{
    /// <summary>
    /// Represents a straight line segment between two vector path nodes.
    /// </summary>
    public class VectorLineSegment : VectorFixedSegment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorLineSegment"/> class.
        /// </summary>
        public VectorLineSegment()
            : base(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorLineSegment"/> class
        /// using the specified endpoint.
        /// </summary>
        /// <param name="position">
        /// The endpoint of the line segment.
        /// </param>
        public VectorLineSegment(
            Point2 position)
            : this(
                position,
                false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VectorLineSegment"/> class
        /// using the specified endpoint and selection state.
        /// </summary>
        /// <param name="position">
        /// The endpoint of the line segment.
        /// </param>
        /// <param name="isSelected">
        /// <see langword="true"/> if the endpoint node should initially be selected;
        /// otherwise, <see langword="false"/>.
        /// </param>
        public VectorLineSegment(
            Point2 position,
            bool isSelected)
            : this()
        {
            Node.Position = position;
            Node.IsSelected = isSelected;
        }

        /// <summary>
        /// Generates the vertices of the straight line segment between the
        /// specified start position and the segment endpoint.
        /// </summary>
        /// <param name="startPosition">
        /// The start position of the line segment.
        /// </param>
        /// <param name="sampleLength">
        /// The desired sampling distance. This value is ignored because a
        /// straight line requires no intermediate samples.
        /// </param>
        /// <returns>
        /// An array containing the start and end positions of the line segment.
        /// </returns>
        public override Point2[] GetVertices(
            Point2 startPosition,
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