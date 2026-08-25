using Sachssoft.Sasogine.Graphics.Text;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sachssoft.Sasogine.Graphics.Rendering
{
    /// <summary>
    /// Immutable font descriptor for game rendering.
    /// Contains only style information required for rendering and caching.
    /// Layout-related properties such as text, alignment, and wrap are moved to TextLayout.
    /// </summary>
    public class FontOptions
    {
        /// <summary>
        /// The font name
        /// </summary>
        public required string FontName { get; init; }

        /// <summary>
        /// The weight of the font (e.g., Normal, Bold).
        /// </summary>
        // Beeinflusst Rendering und Cache, da verschiedene Gewichte unterschiedliche SpriteFonts erzeugen
        public FontWeight Weight { get; init; } = FontWeight.Normal;

        /// <summary>
        /// The style of the font (Normal or Italic).
        /// </summary>
        // Beeinflusst Rendering und Cache
        public FontStyle Style { get; init; } = FontStyle.Normal;

        /// <summary>
        /// The font size in points. Integer recommended for cache stability.
        /// </summary>
        // Integer Size für Cache-Stabilität:
        // 1. Jede Größe erzeugt einen eindeutigen Cache-Key
        // 2. Vermeidet unnötige Duplikate durch minimale Float-Abweichungen
        // 3. Schneller Vergleich in Dictionaries / Hashes
        public int Size { get; init; } = 16;

        /// <summary>
        /// Parameterless constructor for Font.
        /// Allows initialization with object initializer syntax.
        /// </summary>
        public FontOptions()
        {
        }

        /// <summary>
        /// Full constructor for Font.
        /// </summary>
        /// <param name="family">The font family</param>
        /// <param name="weight">The font weight</param>
        /// <param name="style">The font style</param>
        /// <param name="size">The font size in points (int recommended for cache)</param>
        [SetsRequiredMembers]
        public FontOptions(string fontName, FontWeight weight = FontWeight.Normal,
                    FontStyle style = FontStyle.Normal, int size = 16)
        {
            FontName = fontName ?? throw new ArgumentNullException(nameof(fontName));
            Weight = weight;
            Style = style;
            Size = size;
        }

        /// <summary>
        /// Computes a stable hash code for caching purposes.
        /// </summary>
        public override int GetHashCode()
            => HashCode.Combine(FontName, Weight, Style, Size);

        /// <summary>
        /// Compares this Font with another for equality.
        /// </summary>
        public override bool Equals(object? obj)
            => obj is FontOptions other
               && FontName == other.FontName
               && Weight == other.Weight
               && Style == other.Style
               && Size == other.Size;

        /// <summary>
        /// Returns a readable string representation of the font.
        /// </summary>
        public override string ToString()
            => $"(FontName={FontName}, Weight={Weight}, Style={Style}, Size={Size}pt)";
    }
}