namespace Sachssoft.Engine.Common
{
    /// <summary>
    /// Represents an object that records changes which can be consumed by polling code.
    /// </summary>
    /// <typeparam name="TChanges">The type returned when pending changes are consumed.</typeparam>
    public interface ITrackable<out TChanges>
    {
        /// <summary>
        /// Gets whether unconsumed changes are available.
        /// </summary>
        bool HasChanges { get; }

        /// <summary>
        /// Gets the current change version.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Returns the changes recorded since the previous consume operation
        /// and marks those tracked changes as consumed.
        /// </summary>
        TChanges ConsumeChanges();
    }
}
