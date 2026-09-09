using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Defines a read-only 2D transform that can be skewed.
/// </summary>
public interface IReadOnlySkewable2
{
    /// <summary>
    /// Gets the 2D skew.
    /// </summary>
    Vector2 Skew { get; }

    /// <summary>
    /// Gets a value indicating whether skewing is allowed.
    /// </summary>
    bool AllowSkew { get; }
}