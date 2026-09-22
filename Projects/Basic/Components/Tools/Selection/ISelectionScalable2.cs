using Sachssoft.Engine.Common;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Represents a selection target that can be scaled by the Selection Tool.
/// </summary>
public interface ISelectionScalable2 : ISelectionTarget2, IReadOnlyTransformScale2
{
    /// <summary>
    /// Gets a value indicating whether scaling is allowed.
    /// </summary>
    bool AllowScale { get; }
}