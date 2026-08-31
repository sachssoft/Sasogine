using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Defines a 2D selection target definition that supports skew transformation.
    /// </summary>
    public interface ISelectionSkewable2Definition : ISelectionTarget2Definition
    {
        /// <summary>
        /// Gets or sets the skew applied to the target.
        /// </summary>
        Vector2 Skew { get; set; }
    }
}