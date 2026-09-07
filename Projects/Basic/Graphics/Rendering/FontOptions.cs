using Sachssoft.Sasogine.Graphics.Text;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Represents an immutable font descriptor used for text rendering and
    /// font caching.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="FontOptions"/> contains only font-specific rendering
    /// information such as family name, weight, style, and size.
    /// </para>
    /// <para>
    /// Layout-related options such as alignment, wrapping, and flow are
    /// configured separately through text layout options.
    /// </para>
    /// </remarks>
    public class FontOptions : IEquatable<FontOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FontOptions"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor supports object initializer syntax. The required
        /// <see cref="FontName"/> property must be assigned during initialization.
        /// </remarks>
        public FontOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FontOptions"/> class
        /// using the specified font properties.
        /// </summary>
        /// <param name="fontName">
        /// The name of the font family.
        /// </param>
        /// <param name="weight">
        /// The font weight.
        /// </param>
        /// <param name="style">
        /// The font style.
        /// </param>
        /// <param name="size">
        /// The font size in points.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="fontName"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="size"/> is less than or equal to zero.
        /// </exception>
        [SetsRequiredMembers]
        public FontOptions(
            string fontName,
            FontWeight weight = FontWeight.Normal,
            FontStyle style = FontStyle.Normal,
            int size = 16)
        {
            ArgumentNullException.ThrowIfNull(fontName);

            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            FontName = fontName;
            Weight = weight;
            Style = style;
            Size = size;
        }

        /// <summary>
        /// Gets the name of the font family.
        /// </summary>
        public required string FontName { get; init; }

        /// <summary>
        /// Gets the font weight.
        /// </summary>
        public FontWeight Weight { get; init; } = FontWeight.Normal;

        /// <summary>
        /// Gets the font style.
        /// </summary>
        public FontStyle Style { get; init; } = FontStyle.Normal;

        /// <summary>
        /// Gets the font size in points.
        /// </summary>
        /// <remarks>
        /// Integer sizes provide stable cache keys and avoid duplicate cached
        /// font instances caused by small floating-point differences.
        /// </remarks>
        public int Size { get; init; } = 16;

        /// <summary>
        /// Determines whether this instance is equal to the specified font
        /// descriptor.
        /// </summary>
        /// <param name="other">
        /// The font descriptor to compare with this instance.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if both instances describe the same font;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(FontOptions? other)
        {
            return other is not null &&
                   FontName == other.FontName &&
                   Weight == other.Weight &&
                   Style == other.Style &&
                   Size == other.Size;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is FontOptions other && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for this font descriptor.
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(FontName, Weight, Style, Size);
        }

        /// <summary>
        /// Returns a readable string representation of the font descriptor.
        /// </summary>
        public override string ToString()
        {
            return $"(FontName={FontName}, Weight={Weight}, Style={Style}, Size={Size}pt)";
        }
    }
}