namespace Sachssoft.Sasogine.Graphics
{
    /// <summary>
    /// Simplified camera projection types.
    /// </summary>
    public enum CameraProjection
    {
        /// <summary>
        /// Orthographic projection (including isometric style).
        /// Lines stay parallel, sizes are uniform.
        /// </summary>
        Orthographic,

        /// <summary>
        /// Perspective projection.
        /// Objects appear smaller with distance.
        /// </summary>
        Perspective
    }
}
