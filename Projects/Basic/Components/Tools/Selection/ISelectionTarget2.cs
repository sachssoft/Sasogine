using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Represents an object that can be selected by a selection tool.
    /// </summary>
    public interface ISelectionTarget2 : ISelectionTarget
    {
        /// <summary>
        /// Gets or sets a value indicating whether the selection target is selected.
        /// </summary>
        Size2 Size { get; set; }
    }
}