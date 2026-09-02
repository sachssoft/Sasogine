namespace Sachssoft.Sasogine.Experimental.Resources.Markup
{
    /// <summary>
    /// Represents a single frame entry loaded from a frame set markup document.
    /// </summary>
    public class FrameSetEntry
    {
        /// <summary>
        /// Gets or sets the name of the frame.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the horizontal position of the frame within the texture.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets the vertical position of the frame within the texture.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Gets or sets the width of the frame.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the frame.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the frame is rotated within the texture.
        /// </summary>
        public bool IsRotated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the frame has been trimmed.
        /// </summary>
        public bool IsTrimmed { get; set; }

        /// <summary>
        /// Gets or sets the horizontal position of the frame within its original source area.
        /// </summary>
        public int SpriteSourceX { get; set; }

        /// <summary>
        /// Gets or sets the vertical position of the frame within its original source area.
        /// </summary>
        public int SpriteSourceY { get; set; }

        /// <summary>
        /// Gets or sets the width of the frame within its original source area.
        /// </summary>
        public int SpriteSourceWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the frame within its original source area.
        /// </summary>
        public int SpriteSourceHeight { get; set; }

        /// <summary>
        /// Gets or sets the original width of the frame before trimming.
        /// </summary>
        public int SourceWidth { get; set; }

        /// <summary>
        /// Gets or sets the original height of the frame before trimming.
        /// </summary>
        public int SourceHeight { get; set; }

        /// <summary>
        /// Gets or sets the horizontal pivot position in normalized coordinates.
        /// </summary>
        public float PivotX { get; set; }

        /// <summary>
        /// Gets or sets the vertical pivot position in normalized coordinates.
        /// </summary>
        public float PivotY { get; set; }
    }
}