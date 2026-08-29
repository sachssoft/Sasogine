using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Defines the resizing properties of a selection target.
    /// </summary>
    public interface ISelectionResizableDefinition : ISelectionTargetDefinition
    {
        /// <summary>
        /// Gets or sets the size of the selection target.
        /// </summary>
        Size Size { get; set; }
    }
}