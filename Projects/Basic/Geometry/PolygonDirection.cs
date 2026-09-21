namespace Sachssoft.Sasogine.Geometry;

/// <summary>
/// Specifies the winding direction of a polygon's vertices.
/// </summary>
public enum PolygonDirection
{
    /// <summary>
    /// Indicates that the polygon direction is unknown or cannot be determined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Indicates that the polygon vertices are ordered clockwise.
    /// </summary>
    Clockwise = 1,

    /// <summary>
    /// Indicates that the polygon vertices are ordered anticlockwise.
    /// </summary>
    Anticlockwise = 2
}