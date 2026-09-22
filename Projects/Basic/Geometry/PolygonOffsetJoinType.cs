namespace Sachssoft.Engine.Geometry;

/// <summary>
/// Specifies how corners between adjacent polygon edges are joined
/// when generating an offset polygon.
/// </summary>
public enum PolygonOffsetJoinType
{
    /// <summary>
    /// Joins adjacent edges using a square corner.
    /// </summary>
    Square,

    /// <summary>
    /// Joins adjacent edges by cutting off the corner with a straight edge.
    /// </summary>
    Bevel,

    /// <summary>
    /// Joins adjacent edges using a rounded corner.
    /// </summary>
    Round,

    /// <summary>
    /// Joins adjacent edges by extending them until they intersect,
    /// producing a sharp corner.
    /// </summary>
    Miter
}