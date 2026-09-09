using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Defines a read-only 2D transform that can be moved.
/// </summary>
public interface IReadOnlyMovable2
{
    /// <summary>
    /// Gets the 2D position.
    /// </summary>
    Point2 Position { get; }

    /// <summary>
    /// Gets a value indicating whether moving is allowed.
    /// </summary>
    bool AllowMove { get; }
}