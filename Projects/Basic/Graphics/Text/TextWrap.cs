namespace Sachssoft.Sasogine.Graphics.Text
{
    /// <summary>
    /// Specifies how text is wrapped when it exceeds the available layout width.
    /// </summary>
    public enum TextWrap
    {
        /// <summary>
        /// Does not wrap text.
        /// </summary>
        None,

        /// <summary>
        /// Wraps text at word boundaries.
        /// </summary>
        Word,

        /// <summary>
        /// Wraps text at character boundaries.
        /// </summary>
        Character
    }
}