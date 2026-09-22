using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Common;

/// <summary>
/// Defines read-only access to a two-dimensional transform scale.
/// </summary>
public interface IReadOnlyTransformScale2 : ITransform2
{
    /// <summary>
    /// Gets the two-dimensional scale factors.
    /// </summary>
    Vector2 Scale { get; }
}
