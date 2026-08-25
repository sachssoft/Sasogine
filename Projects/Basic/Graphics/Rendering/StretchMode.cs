namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Defines how content is scaled to fit within the available area.
    /// </summary>
    public enum StretchMode
    {
        /// <summary>
        /// No scaling is applied. The original size is preserved.
        /// </summary>
        None,

        /// <summary>
        /// Stretches the content to completely fill the available area.
        /// The aspect ratio may be distorted.
        /// </summary>
        Fill,

        /// <summary>
        /// Scales the content uniformly while preserving its aspect ratio.
        /// The entire content remains visible.
        /// </summary>
        Uniform,

        /// <summary>
        /// Scales the content uniformly while preserving its aspect ratio
        /// until the available area is completely filled. Parts of the
        /// content may be clipped.
        /// </summary>
        UniformToFill
    }
}