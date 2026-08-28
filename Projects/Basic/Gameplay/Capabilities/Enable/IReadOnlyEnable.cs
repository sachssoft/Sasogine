namespace Sachssoft.Sasogine.Gameplay.Capabilities
{
    /// <summary>
    /// Provides read-only access to the enabled state of an object.
    /// </summary>
    public interface IReadOnlyEnableable
    {
        /// <summary>
        /// Gets a value indicating whether the object is enabled.
        /// </summary>
        bool IsEnabled { get; }
    }
}
