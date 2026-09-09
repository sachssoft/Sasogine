namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Defines a read-only 2D transform that can be rotated.
/// </summary>
public interface IReadOnlyRotatable2
{
    /// <summary>
    /// Gets the rotation in radians.
    /// </summary>
    float Rotation { get; }

    /// <summary>
    /// Gets a value indicating whether rotation is allowed.
    /// </summary>
    bool AllowRotation { get; }
}