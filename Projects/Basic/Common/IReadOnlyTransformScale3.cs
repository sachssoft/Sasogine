using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-only access to a three-dimensional transform scale.
/// </summary>
public interface IReadOnlyTransformScale3 : ITransform3
{
    /// <summary>
    /// Gets the three-dimensional scale factors.
    /// </summary>
    Vector3 Scale { get; }
}
