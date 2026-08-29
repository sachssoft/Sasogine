using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Tools.Selection
{
    /// <summary>
    /// Defines the scaling properties of a selection target.
    /// </summary>
    public interface ISelectionScalableDefinition : ISelectionTargetDefinition
    {
        /// <summary>
        /// Gets or sets the scale of the selection target.
        /// </summary>
        Vector2 Scale { get; set; }
    }
}