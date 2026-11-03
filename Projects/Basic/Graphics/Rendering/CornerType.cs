namespace Sachssoft.Engine.Graphics.Rendering;

/// <summary>
/// Defines the geometric treatment of a shape's corners.
/// </summary>
/// <remarks>
/// ShapeBatch rectangle overloads use a local corner size for the radius or diagonal cut.
/// Stroke joins are configured independently through LineJoin.
/// </remarks>
public enum CornerType
{
    /// <summary>
    /// Preserves the original sharp corner without rounding or beveling.
    /// </summary>
    None = 0,

    /// <summary>
    /// Replaces the corner with a straight diagonal cut.
    /// </summary>
    Bevel = 1,

    /// <summary>
    /// Replaces the corner with a rounded arc.
    /// </summary>
    Rounded = 2
}
