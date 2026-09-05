namespace Sachssoft.Sasogine.Common.Schedule;

/// <summary>
/// Specifies how a scheduled action is executed by an
/// <see cref="ActionScheduler"/>.
/// </summary>
public enum ActionSchedulerMode
{
    /// <summary>
    /// Executes the action once after the specified delay has elapsed.
    /// </summary>
    Once,

    /// <summary>
    /// Executes the action repeatedly at intervals defined by the specified delay.
    /// </summary>
    Repeat,

    /// <summary>
    /// Executes the action once after the specified delay and then invokes it
    /// during every subsequent update cycle until it is canceled.
    /// </summary>
    OnceThenUpdate
}