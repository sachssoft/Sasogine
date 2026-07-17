using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Graphics.Camera;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Provides transformation data required by a shader during rendering.
    /// </summary>
    public interface IShaderTransform
    {
        /// <summary>
        /// Gets or sets the camera transformation used for rendering.
        /// </summary>
        ICameraTransform? Camera { get; set; }

        /// <summary>
        /// Gets or sets the local object transformation matrix.
        /// </summary>
        Matrix Transform { get; set; }
    }
}