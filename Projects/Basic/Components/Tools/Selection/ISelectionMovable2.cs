using Sachssoft.Engine.Common;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Represents a selection target that can be moved by the Selection Tool.
/// </summary>
public interface ISelectionMovable2 : ISelectionTarget2, IReadOnlyTransformPosition2
{
    /// <summary>
    /// Gets a value indicating whether movement is currently allowed.
    /// </summary>
    bool AllowMove { get; }
}