using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Common;

/// <summary>
/// Defines read-write access to a two-dimensional transform scale.
/// </summary>
public interface ITransformScale2 : IReadOnlyTransformScale2
{
    /// <summary>
    /// Gets or sets the two-dimensional scale factors.
    /// </summary>
    new Vector2 Scale { get; set; }
}
