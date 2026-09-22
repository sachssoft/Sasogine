using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Common;

/// <summary>
/// Defines read-write access to a two-dimensional transform skew.
/// </summary>
public interface ITransformSkew2 : IReadOnlyTransformSkew2
{
    /// <summary>
    /// Gets or sets the two-dimensional skew factors.
    /// </summary>
    new Vector2 Skew { get; set; }
}
