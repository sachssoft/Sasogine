namespace Sachssoft.Sasogine.Geometry;

/// <summary>
/// Specifies the winding rule used to determine whether a point
/// lies inside a polygon.
/// </summary>
public enum PolygonWindingRule
{
    /// <summary>
    /// Determines the interior by counting edge crossings and considers
    /// a point inside when the number of crossings is odd.
    /// </summary>
    EvenOdd,

    /// <summary>
    /// Determines the interior using the winding number and considers
    /// a point inside when the winding number is not zero.
    /// </summary>
    NonZero
}