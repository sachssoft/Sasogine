namespace Sachssoft.Sasogine.World
{
    /// <summary>
    /// Specifies the integrity state of an entity.
    /// </summary>
    /// <remarks>
    /// The integrity state represents the persistent health or validity of an
    /// entity independently of its current activity state.
    /// </remarks>
    public enum EntityIntegrity
    {
        /// <summary>
        /// The entity is valid and functioning normally.
        /// </summary>
        Intact,

        /// <summary>
        /// The entity is functional but has one or more warnings.
        /// </summary>
        Warning,

        /// <summary>
        /// The entity is in an invalid or erroneous state.
        /// </summary>
        Error
    }
}