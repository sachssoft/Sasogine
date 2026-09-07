using Sachssoft.Sasogine.Graphics.Rendering;

namespace Sachssoft.Sasogine.Graphics.Text
{
    /// <summary>
    /// Defines options that control the alignment, wrapping, and flow
    /// of text within a layout area.
    /// </summary>
    public class TextLayoutOptions
    {
        /// <summary>
        /// Gets the horizontal alignment of the text layout within its
        /// available area.
        /// </summary>
        public Alignment HorizontalAlignment { get; init; }

        /// <summary>
        /// Gets the vertical alignment of the text layout within its
        /// available area.
        /// </summary>
        public Alignment VerticalAlignment { get; init; }

        /// <summary>
        /// Gets the horizontal alignment of text within individual lines.
        /// </summary>
        public TextAlignment TextAlignment { get; init; }

        /// <summary>
        /// Gets the text wrapping behavior.
        /// </summary>
        public TextWrap Wrap { get; init; }

        /// <summary>
        /// Gets the direction in which text flows.
        /// </summary>
        public FlowDirection FlowDirection { get; init; }
    }
}