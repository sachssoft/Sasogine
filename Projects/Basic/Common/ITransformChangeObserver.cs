namespace Sachssoft.Sasogine.Common;

/// <summary>
/// Observes the lifecycle of a transform operation.
/// </summary>
public interface ITransformChangeObserver
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