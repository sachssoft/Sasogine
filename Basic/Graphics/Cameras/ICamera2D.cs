using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics.Camera
{
    /// <summary>
    /// Defines a two-dimensional camera with position and zoom control.
    /// </summary>
    public interface ICamera2D : ICamera
    {
        /// <summary>
        /// Gets or sets the current world position of the camera.
        /// </summary>
        Vector2 Position { get; set; }

        /// <summary>
        /// Gets or sets the minimum allowed camera position.
        /// </summary>
        Vector2 PositionMinimum { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed camera position.
        /// </summary>
        Vector2 PositionMaximum { get; set; }

        /// <summary>
        /// Gets or sets the current camera zoom factor.
        /// </summary>
        float Zoom { get; set; }

        /// <summary>
        /// Gets or sets the base zoom factor used for world scaling.
        /// </summary>
        float BaseZoomFactor { get; set; }

        /// <summary>
        /// Gets or sets the minimum allowed zoom value.
        /// </summary>
        float ZoomMinimum { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed zoom value.
        /// </summary>
        float ZoomMaximum { get; set; }

        /// <summary>
        /// Gets or sets the current camera rotation in radians.
        /// </summary>
        float Rotation { get; set; }

        /// <summary>
        /// Gets or sets the minimum allowed camera rotation in radians.
        /// </summary>
        float RotationMinimum { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowed camera rotation in radians.
        /// </summary>
        float RotationMaximum { get; set; }
    }
}