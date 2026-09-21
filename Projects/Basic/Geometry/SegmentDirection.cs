namespace Sachssoft.Sasogine.Geometry;

/// <summary>
/// Specifies the directions in which geometry is subdivided into segments.
/// </summary>
public enum SegmentDirection
{
    // Einfaches Quad

    /// <summary>
    /// Specifies no subdivision, resulting in a single quad.
    /// </summary>
    None,

    // Unterteilung nur X-Richtung

    /// <summary>
    /// Subdivides the geometry only along the horizontal axis.
    /// </summary>
    Horizontal,

    // Unterteilung nur Y-Richtung

    /// <summary>
    /// Subdivides the geometry only along the vertical axis.
    /// </summary>
    Vertical,

    // Vollständiges Gitter

    /// <summary>
    /// Subdivides the geometry along both the horizontal and vertical axes,
    /// resulting in a complete grid.
    /// </summary>
    Both
}