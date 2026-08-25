namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Defines the flipping mode applied to a rendered object or texture.
    /// </summary>
    public enum FlipMode
    {
        /// <summary>
        /// No flipping is applied.
        /// </summary>
        None,

        /// <summary>
        /// Flips the object horizontally along the vertical axis.
        /// </summary>
        Horizontal,

        /// <summary>
        /// Flips the object vertically along the horizontal axis.
        /// </summary>
        Vertical,

        /// <summary>
        /// Flips the object both horizontally and vertically.
        /// </summary>
        Both
    }
}