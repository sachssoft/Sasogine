using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Graphics.Rendering;

/// <summary>
/// Defines a read-only 2D transform that can be skewed.
/// </summary>
public interface IReadOnlySkewable2 : IReadOnlyTransformSkew2
{
    /// <summary>
    /// Gets a value indicating whether skewing is allowed.
    /// </summary>
    bool AllowSkew { get; }
}