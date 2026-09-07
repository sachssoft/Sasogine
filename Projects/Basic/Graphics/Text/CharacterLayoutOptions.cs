namespace Sachssoft.Sasogine.Graphics.Text
{
    /// <summary>
    /// Defines layout options that control character and line spacing during
    /// text rendering.
    /// </summary>
    public class CharacterLayoutOptions
    {
        /// <summary>
        /// Gets or sets the additional spacing applied between characters.
        /// </summary>
        /// <value>
        /// The additional character spacing.
        /// </value>
        public float CharacterSpacing { get; set; }

        /// <summary>
        /// Gets or sets the additional spacing applied between lines of text.
        /// </summary>
        /// <value>
        /// The additional line spacing.
        /// </value>
        public float LineSpacing { get; set; }
    }
}