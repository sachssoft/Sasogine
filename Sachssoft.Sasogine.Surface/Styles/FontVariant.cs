using FontStashSharp;
using Sachssoft.Sasogine.Surface.Visuals;
using System;

namespace Sachssoft.Sasogine.Surface.Styles
{

    /// <summary>
    /// Represents a font face similar to CSS, including weight and style.
    /// </summary>
    public record FontVariant
    {
        private readonly FontSystem _fontSystem;

        /// <summary>
        /// Initializes a new font face with the specified font system, weight, and style.
        /// </summary>
        /// <param name="fontSystem">The underlying FontSystem instance.</param>
        /// <param name="definedWeight">The font weight (default: Normal).</param>
        /// <param name="definedStyle">The font style (default: Normal).</param>
        public FontVariant(FontSystem fontSystem, FontWeight definedWeight = FontWeight.Normal, FontStyle definedStyle = FontStyle.Normal)
        {
            _fontSystem = fontSystem ?? throw new ArgumentNullException(nameof(fontSystem));
            Weight = definedWeight;
            Style = definedStyle;
        }

        /// <summary>
        /// Gets a dynamic sprite font for the given size.
        /// </summary>
        /// <param name="size">The font size.</param>
        internal DynamicSpriteFont GetFont(float size)
        {
            return _fontSystem.GetFont(size);
        }

        /// <summary>
        /// Font weight (thickness).
        /// </summary>
        public FontWeight Weight { get; init; }

        /// <summary>
        /// Font style (normal, italic).
        /// </summary>
        public FontStyle Style { get; init; }
    }
}