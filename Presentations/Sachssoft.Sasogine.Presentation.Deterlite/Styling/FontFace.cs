using Sachssoft.Sasogine.Presentation.Deterlite.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Presentation.Deterlite.Styling
{
    /// <summary>
    /// Represents a single font face (file, weight, style).
    /// Compatible with FontStashSharp.
    /// </summary>
    public sealed class FontFace
    {
        /// <summary>
        /// Path to the font file (e.g., TTF)
        /// </summary>
        public required string File { get; init; }

        /// <summary>
        /// Optional resource source wrapper for loading the font
        /// </summary>
        public ResourceFileSource Source { get; init; }

        /// <summary>
        /// Weight (Normal / Bold)
        /// </summary>
        public FontWeight Weight { get; init; } = FontWeight.Normal;

        /// <summary>
        /// Style (Normal / Italic)
        /// </summary>
        public FontStyle Style { get; init; } = FontStyle.Normal;

        /// <summary>
        /// Parameterless constructor für Object Initializer
        /// </summary>
        public FontFace() { }

        /// <summary>
        /// Optional Constructor with all parameters
        /// </summary>
        [SetsRequiredMembers]
        public FontFace(string file, ResourceFileSource source, FontWeight weight = FontWeight.Normal, FontStyle style = FontStyle.Normal)
        {
            File = file;
            Source = source;
            Weight = weight;
            Style = style;
        }

        public override string ToString()
            => $"{System.IO.Path.GetFileName(File)} (Weight={Weight}, Style={Style})";
    }
}