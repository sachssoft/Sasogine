using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Graphics.Camera
{
    /// <summary>
    /// Provides read-only access to camera transformation matrices
    /// used during rendering.
    /// </summary>
    public interface ICameraTransform
    {
        /// <summary>
        /// Gets the projection matrix of the camera.
        /// </summary>
        Matrix Projection { get; }

        /// <summary>
        /// Gets the view matrix of the camera.
        /// </summary>
        Matrix View { get; }

        /// <summary>
        /// Gets the world transformation matrix.
        /// </summary>
        Matrix World { get; }
    }
}