using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Represents a selection target that can be scaled by the Selection Tool.
    /// </summary>
    public interface ISelectionScalable : ISelectionTarget
    {
        /// <summary>
        /// Gets or sets the scale of the selection target.
        /// </summary>
        Vector2 Scale { get; set; }

        /// <summary>
        /// Gets a value indicating whether scaling is allowed.
        /// </summary>
        bool AllowScale { get; }
    }
}