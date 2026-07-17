using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Defines a transformable object with position, rotation, scale,
    /// and origin properties.
    /// </summary>
    public interface ITransformable
    {
        /// <summary>
        /// Gets or sets the translation offset of the object.
        /// </summary>
        Vector2 Translation { get; set; }

        /// <summary>
        /// Gets or sets the rotation angle in radians.
        /// </summary>
        float Rotation { get; set; }

        /// <summary>
        /// Gets or sets the scaling factor.
        /// </summary>
        Vector2 Scale { get; set; }

        /// <summary>
        /// Gets or sets the origin point used for rotation and scaling.
        /// </summary>
        Vector2 Origin { get; set; }
    }
}