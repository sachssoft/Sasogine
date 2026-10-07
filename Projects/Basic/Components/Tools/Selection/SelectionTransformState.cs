namespace Sachssoft.Engine.Components.Tools;

/// <summary>
/// Specifies the state of a Selection Tool transformation.
/// </summary>
public enum SelectionTransformState
{
    /// <summary>
    /// The transformation has started.
    /// </summary>
    Started,

    /// <summary>
    /// The transformation has changed.
    /// </summary>
    Changed,

    /// <summary>
    /// The transformation has been completed.
    /// </summary>
    Completed,

    /// <summary>
    /// The transformation has been cancelled.
    /// </summary>
    Cancelled
}