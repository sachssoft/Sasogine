namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Provides optional notifications for the lifecycle of a transform operation.
/// </summary>
public interface INotifyTransformChanged
{
    /// <summary>
    /// Called when a transform operation starts.
    /// </summary>
    void OnTransformStarted();

    /// <summary>
    /// Called while a transform operation changes the target.
    /// </summary>
    void OnTransformChanged();

    /// <summary>
    /// Called when a transform operation completes.
    /// </summary>
    void OnTransformCompleted();
}
