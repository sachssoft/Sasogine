namespace Sachssoft.Engine.World
{
    /// <summary>
    /// Specifies the current runtime activity state of an entity.
    /// </summary>
    /// <remarks>
    /// The activity state is independent of the entity's integrity and
    /// loading state.
    /// </remarks>
    public enum ActivityState
    {
        /// <summary>
        /// The entity is currently inactive and can be edited.
        /// </summary>
        Idle,

        /// <summary>
        /// The entity is currently active and should not be edited.
        /// </summary>
        Active
    }
}