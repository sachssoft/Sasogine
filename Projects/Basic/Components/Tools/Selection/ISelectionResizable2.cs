using Sachssoft.Engine;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Represents a selection target that can be resized by the Selection Tool.
/// </summary>
public interface ISelectionResizable2 : ISelectionTarget2, IReadOnlyTransformSize2
{
    /// <summary>
    /// Gets a value indicating whether resizing through the Selection Tool is allowed.
    /// </summary>
    bool AllowResize { get; }

    /// <summary>
    /// Gets a value indicating whether the aspect ratio is preserved while resizing.
    /// </summary>
    bool PreserveAspectRatio { get; }

    /// <summary>
    /// Coerces the specified size to a valid size for the selection target.
    /// </summary>
    /// <param name="baseValue">The size calculated by the Selection Tool.</param>
    /// <returns>The coerced size.</returns>
    Size2 CoerceSize(Size2 baseValue);
}