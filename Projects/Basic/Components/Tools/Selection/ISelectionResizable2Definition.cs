using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Defines the resizing properties of a selection target.
    /// </summary>
    public interface ISelectionResizable2Definition : ISelectionTarget2Definition
    {
        /// <summary>
        /// Gets or sets the size of the selection target.
        /// </summary>
        Size2 Size { get; set; }
    }
}