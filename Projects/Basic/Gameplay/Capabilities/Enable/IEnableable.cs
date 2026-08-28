namespace Sachssoft.Sasogine.Gameplay.Capabilities
{
    /// <summary>
    /// Represents an object whose enabled state can be changed.
    /// </summary>
    public interface IEnableable : IReadOnlyEnableable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the object is enabled.
        /// </summary>
        new bool IsEnabled { get; set; }
    }
}
