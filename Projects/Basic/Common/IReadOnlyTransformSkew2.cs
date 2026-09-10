using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-only access to a two-dimensional transform skew.
/// </summary>
public interface IReadOnlyTransformSkew2 : ITransform2
{
    /// <summary>
    /// Gets the two-dimensional skew factors.
    /// </summary>
    Vector2 Skew { get; }
}
