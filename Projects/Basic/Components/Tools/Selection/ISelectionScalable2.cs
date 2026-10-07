using Microsoft.Xna.Framework;
using Sachssoft.Engine;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Represents a selection target that can be scaled by the Selection Tool.
/// </summary>
public interface ISelectionScalable2 : ISelectionTarget2, IReadOnlyTransformScale2
{
    /// <summary>
    /// Gets a value indicating whether scaling through the Selection Tool is allowed.
    /// </summary>
    bool AllowScale { get; }

    /// <summary>
    /// Coerces the specified scale to a valid scale for the selection target.
    /// </summary>
    /// <param name="baseValue">The scale calculated by the Selection Tool.</param>
    /// <returns>The coerced scale.</returns>
    Vector2 CoerceScale(Vector2 baseValue);

    /// <summary>
    /// Called when the state of a scale operation changes.
    /// </summary>
    /// <param name="state">The current state of the scale operation.</param>
    void OnScale(SelectionTransformState state);
}