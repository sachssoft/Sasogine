namespace Sachssoft.Sasogine.Surface.Controls
{
    /// <summary>
    /// Bestimmt, wie Text in einer TextBox reagiert, wenn er länger als die verfügbare Breite ist.
    /// </summary>
    public enum TextBoxOverflowMode
    {
        /// <summary>
        /// Die TextBox passt ihre Breite automatisch an den Text an, wenn Width, MinWidth und MaxWidth null sind.
        /// </summary>
        AutoWidth,

        /// <summary>
        /// Der Text wird in der TextBox abgeschnitten, wenn er überläuft.
        /// </summary>
        Clip
    }
}
