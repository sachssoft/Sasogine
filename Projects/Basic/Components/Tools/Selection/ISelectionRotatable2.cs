using Sachssoft.Engine.Common;

namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Represents a selection target that can be rotated by the Selection Tool.
/// </summary>
public interface ISelectionRotatable2 : ISelectionTarget2, IReadOnlyTransformRotation2, IReadOnlyTransformRotationPivot2
{
    /// <summary>
    /// Gets a value indicating whether rotation is allowed.
    /// </summary>
    bool AllowRotate { get; }
}