namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Defines the shape used to render the ends of a line.
    /// </summary>
    public enum LineCap
    {
        /// <summary>
        /// Ends the line exactly at its endpoints.
        /// </summary>
        Butt,

        /// <summary>
        /// Extends the line beyond its endpoints by half of its thickness.
        /// </summary>
        Square,

        /// <summary>
        /// Renders the line ends as semicircles.
        /// </summary>
        Round
    }
}