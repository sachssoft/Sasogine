using Sachssoft.Engine.Common;

namespace Sachssoft.Engine.Graphics.Rendering;

/// <summary>
/// Defines a read-only 2D transform that can be scaled.
/// </summary>
public interface IReadOnlyScalable2 : IReadOnlyTransformScale2
{
    /// <summary>
    /// Gets a value indicating whether scaling is allowed.
    /// </summary>
    bool AllowScale { get; }
}