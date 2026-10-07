using Microsoft.Xna.Framework;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Represents a 2D selection target that supports skew transformation.
/// </summary>
public interface ISelectionSkewable2 : ISelectionTarget2, IReadOnlyTransformSkew2
{
    /// <summary>
    /// Gets a value indicating whether skewing through the Selection Tool is allowed.
    /// </summary>
    bool AllowSkew { get; }

    /// <summary>
    /// Coerces the specified skew to a valid skew for the selection target.
    /// </summary>
    /// <param name="baseValue">The skew calculated by the Selection Tool.</param>
    /// <returns>The coerced skew.</returns>
    Vector2 CoerceSkew(Vector2 baseValue);
}