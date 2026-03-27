using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Components.Rendering.Camera
{
    /// <summary>
    /// Provides access to camera transformation matrices (Projection, View, World)
    /// and allows copying these matrices between camera instances.
    /// </summary>
    public interface ICameraTransform
    {
        /// <summary>
        /// Gets or sets the projection matrix of the camera.
        /// </summary>
        Matrix Projection { get; set; }

        /// <summary>
        /// Gets or sets the view matrix of the camera.
        /// </summary>
        Matrix View { get; set; }

        /// <summary>
        /// Gets or sets the world matrix of the camera.
        /// </summary>
        Matrix World { get; set; }

        /// <summary>
        /// Copies the camera matrices (Projection, View, World) from another camera instance
        /// to this camera.
        /// </summary>
        /// <param name="source">The source camera providing the matrices.</param>
        void ApplyFrom(ICameraTransform source);

        /// <summary>
        /// Copies the camera matrices (Projection, View, World) from this camera
        /// to another camera instance.
        /// </summary>
        /// <param name="target">The target camera to receive the matrices.</param>
        void CopyTo(ICameraTransform target);
    }
}
