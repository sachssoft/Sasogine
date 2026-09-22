namespace Sachssoft.Engine.Geometry;

/// <summary>
/// Specifies how the ends of paths are handled when generating
/// an offset polygon.
/// </summary>
public enum PolygonOffsetEndType
{
    /// <summary>
    /// Treats the path as a closed polygon.
    /// </summary>
    Polygon,

    /// <summary>
    /// Treats the path as closed by joining its first and last points.
    /// </summary>
    Joined,

    /// <summary>
    /// Ends an open path directly at its endpoints without extending them.
    /// </summary>
    Butt,

    /// <summary>
    /// Ends an open path with square caps that extend beyond its endpoints.
    /// </summary>
    Square,

    /// <summary>
    /// Ends an open path with rounded caps.
    /// </summary>
    Round
}