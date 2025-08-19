namespace Sachssoft.Sasogine.Schedule;

/// <summary>
/// Gibt an, wie eine geplante Aktion im <see cref="ActionScheduler"/> ausgeführt wird.
/// </summary>
public enum ActionSchedulerMode
{
    /// <summary>
    /// Die Aktion wird genau einmal nach Ablauf der angegebenen Verzögerung ausgeführt.
    /// </summary>
    Once,

    /// <summary>
    /// Die Aktion wird wiederholt in konstanten Zeitabständen ausgeführt.
    /// Der Abstand zwischen den Ausführungen entspricht der angegebenen Verzögerung.
    /// </summary>
    Repeat,

    /// <summary>
    /// Die Aktion wird einmal nach der angegebenen Verzögerung ausgeführt.
    /// Danach wird sie in jedem Update-Zyklus erneut aufgerufen – ohne weitere Verzögerung –
    /// bis sie explizit durch <see cref="CancelEventArgs.Cancel"/> abgebrochen wird.
    /// </summary>
    OnceThenUpdate
}