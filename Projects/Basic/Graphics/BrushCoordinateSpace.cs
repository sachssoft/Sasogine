
namespace Sachssoft.Engine.Graphics;

/// <summary>
/// Defines the coordinate space used by a <see cref="Brush"/> to evaluate
/// brush-specific vertex data.
/// </summary>
public enum BrushCoordinateSpace
{
    /// <summary>
    /// Uses normalized shape coordinates in the range <c>0</c> through <c>1</c>.
    /// </summary>
    Normalized,

    /// <summary>
    /// Uses the untransformed local coordinates of the shape.
    /// </summary>
    Local,

    /// <summary>
    /// Uses the transformed world coordinates of the generated geometry.
    /// </summary>
    World
}
