namespace Sachssoft.Sasogine.Geometry;

/// <summary>
/// Specifies the boolean operation used to combine or clip polygons.
/// </summary>
public enum PolygonClipOperation
{
    /// <summary>
    /// Combines the areas of all polygons into a single resulting area.
    /// </summary>
    Union,

    /// <summary>
    /// Retains only the area shared by the polygons.
    /// </summary>
    Intersection,

    /// <summary>
    /// Removes the overlapping area of the clipping polygon
    /// from the subject polygon.
    /// </summary>
    Difference,

    /// <summary>
    /// Retains areas belonging to either polygon, but excludes
    /// areas shared by both polygons.
    /// </summary>
    Xor
}