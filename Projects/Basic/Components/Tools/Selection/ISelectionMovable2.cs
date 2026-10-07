using Sachssoft.Engine;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Represents a selection target that can be moved by the Selection Tool.
/// </summary>
public interface ISelectionMovable2 : ISelectionTarget2, IReadOnlyTransformPosition2
{
    /// <summary>
    /// Gets a value indicating whether movement through the Selection Tool is allowed.
    /// </summary>
    bool AllowMove { get; }

    /// <summary>
    /// Coerces the specified position to a valid position for the selection target.
    /// </summary>
    /// <param name="baseValue">The position calculated by the Selection Tool.</param>
    /// <returns>The coerced position.</returns>
    Point2 CoercePosition(Point2 baseValue);
}