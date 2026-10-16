namespace Sachssoft.Engine
{
    /// <summary>
    /// Defines the available operating states of an object.
    /// </summary>
    public enum SwitchState
    {
        /// <summary>
        /// The object is disabled or inactive.
        /// </summary>
        Off = 0,

        /// <summary>
        /// The object is continuously enabled or active.
        /// </summary>
        On = 1,

        /// <summary>
        /// The object operates according to a timed cycle,
        /// including active, inactive, and transition phases.
        /// </summary>
        Cycling = 2
    }
}