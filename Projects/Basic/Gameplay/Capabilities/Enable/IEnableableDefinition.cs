using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Gameplay.Capabilities
{
    /// <summary>
    /// Defines the enabled state of an object.
    /// </summary>
    public interface IEnableableDefinition : IDefinition
    {
        /// <summary>
        /// Gets or sets a value indicating whether the object is enabled.
        /// </summary>
        bool IsEnabled { get; set; }
    }
}
