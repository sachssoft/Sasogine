using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Defines a read-only 2D transform that can be rotated.
/// </summary>
public interface IReadOnlyRotatable2 : IReadOnlyTransformRotation2
{
    /// <summary>
    /// Gets a value indicating whether rotation is allowed.
    /// </summary>
    bool AllowRotation { get; }
}