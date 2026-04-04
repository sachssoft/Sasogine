using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Presentation.Layouts;
using Sachssoft.Sasogine.Presentation.Styling;

namespace Sachssoft.Sasogine.Presentation.Rendering
{
    /// <summary>
    /// Represents layout settings for rendering text.
    /// Contains alignment, wrap, and flow direction.
    /// Font information is kept separate in the Font class for caching and rendering.
    /// </summary>
    public class TextLayout
    {
        /// <summary>
        /// Horizontal alignment of text within the container (Near / Center / Far).
        /// </summary>
        public Alignment HorizontalAlignment { get; init; } = Alignment.Near;

        /// <summary>
        /// Vertical alignment of text within the container (Near / Center / Far).
        /// </summary>
        public Alignment VerticalAlignment { get; init; } = Alignment.Near;

        /// <summary>
        /// Text alignment within a line (Left, Center, Right).
        /// </summary>
        public TextAlignment TextAlignment { get; init; } = TextAlignment.Left;

        /// <summary>
        /// Text wrapping mode.
        /// </summary>
        public TextWrap Wrap { get; init; } = TextWrap.None;

        /// <summary>
        /// Flow direction (LeftToRight / RightToLeft).
        /// </summary>
        public FlowDirection FlowDirection { get; init; } = FlowDirection.LeftToRight;

        /// <summary>
        /// Indicates whether the text contains multiple lines (for internal rendering logic).
        /// </summary>
        public bool IsMultiline { get; init; } = false;
    }
}