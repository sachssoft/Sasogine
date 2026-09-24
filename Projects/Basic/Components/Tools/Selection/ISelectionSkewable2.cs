using Sachssoft.Engine;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Represents a 2D selection target that supports skew transformation.
/// </summary>
public interface ISelectionSkewable2 : ISelectionTarget2, IReadOnlyTransformSkew2
{
    /// <summary>
    /// Gets a value indicating whether the target can be skewed.
    /// </summary>
    bool AllowSkew { get; }
}