namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Defines how a texture or rendered element is repeated along its axes.
    /// </summary>
    public enum RepeatMode
    {
        /// <summary>
        /// No repetition is applied.
        /// </summary>
        None,

        /// <summary>
        /// Repeats along the horizontal axis only.
        /// </summary>
        RepeatX,

        /// <summary>
        /// Repeats along the vertical axis only.
        /// </summary>
        RepeatY,

        /// <summary>
        /// Repeats along both the horizontal and vertical axes.
        /// </summary>
        RepeatXY
    }
}