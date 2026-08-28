using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Defines the selection state of a selection target.
    /// </summary>
    public interface ISelectionTargetDefinition : IDefinition
    {
        /// <summary>
        /// Gets or sets a value indicating whether the selection target is selected.
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the selection target is locked.
        /// </summary>
        bool IsLocked { get; set; }
    }
}