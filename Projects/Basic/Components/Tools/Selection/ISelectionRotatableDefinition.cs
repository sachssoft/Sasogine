using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Defines the rotation properties of a selection target.
    /// </summary>
    public interface ISelectionRotatableDefinition : ISelectionTargetDefinition
    {
        /// <summary>
        /// Gets or sets the rotation of the selection target.
        /// </summary>
        float Rotation { get; set; }

        /// <summary>
        /// Gets or sets the pivot point used for rotating the selection target.
        /// </summary>
        Vector2 RotationPivot { get; set; }
    }
}