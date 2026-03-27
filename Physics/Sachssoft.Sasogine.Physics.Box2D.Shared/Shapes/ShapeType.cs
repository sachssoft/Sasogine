namespace Box2D;

/// <summary>
/// Shape type
/// </summary>
public enum ShapeType
{
    /// <summary>
    /// A circle with an offset
    /// </summary>
    Circle,

    /// <summary>
    /// A capsule is an extruded circle
    /// </summary>
    Capsule,

    /// <summary>
    /// A line segment
    /// </summary>
    Segment,

    /// <summary>
    /// A convex polygon
    /// </summary>
    Polygon,

    /// <summary>
    /// A line segment owned by a chain shape
    /// </summary>
    ChainSegment,
}