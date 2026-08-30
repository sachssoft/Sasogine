using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Defines the movement properties of a selection target.
    /// </summary>
    public interface ISelectionMovable2Definition : ISelectionTarget2Definition
    {
        /// <summary>
        /// Gets or sets the position of the selection target.
        /// </summary>
        Vector2 Position { get; set; }
    }
}