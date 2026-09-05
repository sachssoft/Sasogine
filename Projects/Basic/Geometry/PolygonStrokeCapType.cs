namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Defines how the ends of an open polygon stroke are shaped.
    /// </summary>
    public enum PolygonStrokeCapType
    {
        /// <summary>
        /// Ends the stroke directly at the path endpoint.
        /// </summary>
        Butt,

        /// <summary>
        /// Extends the stroke beyond the endpoint using a square cap.
        /// </summary>
        Square,

        /// <summary>
        /// Extends the stroke using a rounded cap.
        /// </summary>
        Round
    }
}