namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Defines the shape used to render the corners between connected line segments.
    /// </summary>
    public enum LineJoin
    {
        /// <summary>
        /// Extends the outer edges of the connected line segments until they intersect.
        /// </summary>
        Miter,

        /// <summary>
        /// Connects the outer edges of the line segments with a straight edge.
        /// </summary>
        Bevel,

        /// <summary>
        /// Connects the outer edges of the line segments with a rounded corner.
        /// </summary>
        Round
    }
}