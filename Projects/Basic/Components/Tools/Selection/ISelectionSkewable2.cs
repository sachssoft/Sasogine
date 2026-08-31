using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Represents a 2D selection target that supports skew transformation.
    /// </summary>
    public interface ISelectionSkewable2 : ISelectionTarget2
    {
        /// <summary>
        /// Gets or sets the skew applied to the target.
        /// </summary>
        Vector2 Skew { get; set; }

        /// <summary>
        /// Gets a value indicating whether the target can be skewed.
        /// </summary>
        bool AllowSkew { get; }
    }
}