using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Defines read-write access to a three-dimensional transform scale.
/// </summary>
public interface ITransformScale3 : IReadOnlyTransformScale3
{
    /// <summary>
    /// Gets or sets the three-dimensional scale factors.
    /// </summary>
    new Vector3 Scale { get; set; }
}
