using Sachssoft.Engine;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Represents a selection target that can be resized by the Selection Tool.
/// </summary>
public interface ISelectionResizable2 : ISelectionTarget2, IReadOnlyTransformSize2
{
    /// <summary>
    /// Gets a value indicating whether resizing is allowed.
    /// </summary>
    bool AllowResize { get; }
}