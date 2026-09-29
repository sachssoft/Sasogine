namespace Sachssoft.Engine.Geometry;

/// <summary>
/// Defines how traced contours are represented.
/// </summary>
public enum PolygonTraceMode
{
    /// <summary>
    /// Preserves the exact pixel boundary using line segments.
    /// </summary>
    Pixel,

    /// <summary>
    /// Simplifies the pixel boundary and represents it using line segments.
    /// </summary>
    Polygon,

    /// <summary>
    /// Simplifies the boundary and fits cubic Bézier curves while preserving corners.
    /// </summary>
    Spline
}
