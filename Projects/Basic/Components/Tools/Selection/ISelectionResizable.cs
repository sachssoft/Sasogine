using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Represents a selection target that can be resized by the Selection Tool.
    /// </summary>
    public interface ISelectionResizable : ISelectionTarget
    {
        /// <summary>
        /// Gets or sets the size of the selection target.
        /// </summary>
        Size Size { get; set; }

        /// <summary>
        /// Gets a value indicating whether resizing is allowed.
        /// </summary>
        bool AllowResize { get; }
    }
}