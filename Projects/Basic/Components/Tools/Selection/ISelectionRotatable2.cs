using Sachssoft.Engine;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Represents a selection target that can be rotated by the Selection Tool.
/// </summary>
public interface ISelectionRotatable2 :
    ISelectionTarget2,
    IReadOnlyTransformRotation2,
    IReadOnlyTransformRotationPivot2
{
    /// <summary>
    /// Gets a value indicating whether rotation through the Selection Tool is allowed.
    /// </summary>
    bool AllowRotate { get; }

    /// <summary>
    /// Coerces the specified rotation to a valid rotation for the selection target.
    /// </summary>
    /// <param name="baseValue">The rotation calculated by the Selection Tool.</param>
    /// <returns>The coerced rotation.</returns>
    float CoerceRotation(float baseValue);

    /// <summary>
    /// Coerces the specified rotation pivot to a valid pivot for the selection target.
    /// </summary>
    /// <param name="baseValue">The rotation pivot calculated by the Selection Tool.</param>
    /// <returns>The coerced rotation pivot.</returns>
    Point2 CoerceRotationPivot(Point2 baseValue);

    /// <summary>
    /// Called when the state of a rotation operation changes.
    /// </summary>
    /// <param name="state">The current state of the rotation operation.</param>
    void OnRotation(SelectionTransformState state);

    /// <summary>
    /// Called when the state of a rotation pivot move operation changes.
    /// </summary>
    /// <param name="state">The current state of the rotation pivot operation.</param>
    void OnRotationPivot(SelectionTransformState state);
}