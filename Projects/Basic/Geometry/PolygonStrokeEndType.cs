namespace Sachssoft.Sasogine.Geometry
{
    /// <summary>
    /// Defines whether a stroked path is treated as open or closed.
    /// </summary>
    public enum PolygonStrokeEndType
    {
        /// <summary>
        /// Treats the path as open and applies end caps to its endpoints.
        /// </summary>
        Open,

        /// <summary>
        /// Treats the path as closed by connecting its final point
        /// to its first point.
        /// </summary>
        Closed
    }
}