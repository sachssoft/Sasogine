using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Common;
using Sachssoft.Sasogine.Components.Models;
using System.ComponentModel;

namespace Sachssoft.Sasogine.Components.Rendering.Cameras
{
    /// <summary>
    /// Defines the default configuration values for a 2D camera.
    /// </summary>
    public class Camera2DDefinition : EngineObjectDefinition
    {
        /// <summary>
        /// Gets or sets the default world position of the camera.
        /// </summary>
        [Category(Categories.Transform)]
        public Vector2 Position { get; set; } = Vector2.Zero;

        /// <summary>
        /// Gets or sets the default zoom factor of the camera.
        /// </summary>
        [Category(Categories.Transform)]
        public float Zoom { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the default rotation of the camera in radians.
        /// </summary>
        [Category(Categories.Transform)]
        public float Rotation { get; set; } = 0.0f;
    }
}