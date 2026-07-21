using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Scenes;

namespace Sachssoft.Sasogine.Graphics.Primitives
{
    /// <summary>
    /// Defines a renderable graphics primitive that can be drawn within a scene.
    /// Provides common properties for visibility and local transformation.
    /// </summary>
    public interface IPrimitive
    {
        /// <summary>
        /// Gets or sets a value indicating whether the primitive is rendered.
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the local transformation matrix of the primitive.
        /// Defines the position, rotation, and scale applied during rendering.
        /// </summary>
        Matrix Transform { get; set; }

        /// <summary>
        /// Draws the primitive using the specified scene rendering context.
        /// </summary>
        /// <param name="context">
        /// Provides the rendering information and resources required to draw the primitive.
        /// </param>
        void Draw(SceneDrawContext context);
    }
}