using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Defines the selection state of a selection target.
    /// </summary>
    public interface ISelectionTarget2Definition : ISelectionTargetDefinition
    {
        /// <summary>
        /// Gets or sets a value indicating whether the selection target is selected.
        /// </summary>
        Size2 Size { get; set; }
    }
}