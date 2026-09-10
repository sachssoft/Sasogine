using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Defines a read-only 2D transform that can be resized.
/// </summary>
public interface IReadOnlyResizable2 : IReadOnlyTransformSize2
{
    /// <summary>
    /// Gets a value indicating whether resizing is allowed.
    /// </summary>
    bool AllowResize { get; }
}