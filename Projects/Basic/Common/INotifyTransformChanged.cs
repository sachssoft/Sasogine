using System;

namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Provides event-based notifications for the lifecycle of a transform operation.
/// </summary>
public interface INotifyTransformChanged
{
    /// <summary>
    /// Occurs when a transform operation starts.
    /// </summary>
    event EventHandler? TransformStarted;

    /// <summary>
    /// Occurs while a transform operation changes the target.
    /// </summary>
    event EventHandler? TransformChanged;

    /// <summary>
    /// Occurs when a transform operation completes.
    /// </summary>
    event EventHandler? TransformCompleted;
}