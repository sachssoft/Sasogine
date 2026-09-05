namespace Sachssoft.Sasogine.Input
{
    /// <summary>
    /// Specifies the state of a touch interaction.
    /// </summary>
    public enum TouchButton
    {
        /// <summary>
        /// The touch contact was pressed.
        /// </summary>
        Pressed = 0,

        /// <summary>
        /// The touch contact moved.
        /// </summary>
        Moved = 1,

        /// <summary>
        /// The touch contact was released.
        /// </summary>
        Released = 2
    }
}