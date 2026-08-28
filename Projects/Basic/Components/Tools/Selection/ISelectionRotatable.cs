using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Represents a selection target that can be rotated by the Selection Tool.
    /// </summary>
    public interface ISelectionRotatable : ISelectionTarget
    {
        /// <summary>
        /// Gets or sets the rotation of the selection target.
        /// </summary>
        float Rotation { get; set; }

        /// <summary>
        /// Gets or sets the pivot point used for rotating the selection target.
        /// </summary>
        Vector2 RotationPivot { get; set; }

        /// <summary>
        /// Gets a value indicating whether rotation is allowed.
        /// </summary>
        bool AllowRotate { get; }
    }
}