using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Defines a read-only 2D transform that can be scaled.
/// </summary>
public interface IReadOnlyScalable2
{
    /// <summary>
    /// Gets the 2D scale.
    /// </summary>
    Vector2 Scale { get; }

    /// <summary>
    /// Gets a value indicating whether scaling is allowed.
    /// </summary>
    bool AllowScale { get; }
}