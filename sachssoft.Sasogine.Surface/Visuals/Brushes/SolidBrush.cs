using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using System;
using Sachssoft.Sasogine.Surface.Visuals.Styles;

namespace Sachssoft.Sasogine.Surface.Visuals.Brushes
{
    /// <summary>
    /// Represents a solid-color brush that can fill a rectangular area
    /// using a tintable white texture region (from the active <see cref="Stylesheet"/>).
    /// </summary>
    public class SolidBrush : IBrush
    {
        private Color _color = Color.White;

        /// <summary>
        /// Gets or sets the base color of this brush.
        /// </summary>
        public Color Color
        {
            get => _color;
            set => _color = value;
        }

        /// <summary>
        /// Creates a solid brush with the specified color.
        /// </summary>
        /// <param name="color">The solid color to use for drawing.</param>
        public SolidBrush(Color color)
        {
            _color = color;
        }

        /// <summary>
        /// Creates a solid brush from a named color.
        /// </summary>
        /// <param name="colorName">A color name (e.g. "Red", "Blue", "#FF00FF").</param>
        /// <exception cref="ArgumentException">Thrown if the color name is invalid.</exception>
        public SolidBrush(string colorName)
        {
            var c = ColorStorage.FromName(colorName);

            if (c == null)
                throw new ArgumentException($"Could not recognize color '{colorName}'", nameof(colorName));

            _color = c.Value;
        }

        /// <summary>
        /// Draws a filled rectangle with this brush.
        /// </summary>
        /// <param name="context">The current render context.</param>
        /// <param name="dest">The destination rectangle to fill.</param>
        /// <param name="tint">
        /// An optional tint color (multiplies the base <see cref="Color"/>).
        /// Use <see cref="Color.White"/> for no tint.
        /// </param>
        public void Draw(RenderContext context, Rectangle dest, Color tint)
        {
            // Get the basic white texture region (usually a 1x1 pixel white texture)
            var white = Stylesheet.Current.WhiteRegion;

            // If the caller uses no extra tint → just use the brush color directly
            if (tint == Color.White)
            {
                white.Draw(context, dest, _color);
            }
            else
            {
                // Multiply base color with tint
                var blended = new Color(
                    (int)(_color.R * tint.R / 255f),
                    (int)(_color.G * tint.G / 255f),
                    (int)(_color.B * tint.B / 255f),
                    (int)(_color.A * tint.A / 255f)
                );

                white.Draw(context, dest, blended);
            }
        }

        /// <summary>
        /// Tries to draw the brush safely.
        /// If the <see cref="Stylesheet.Current"/> is missing or <c>WhiteRegion</c> is unavailable, it skips rendering.
        /// </summary>
        /// <param name="context">The render context.</param>
        /// <param name="dest">Destination rectangle.</param>
        /// <param name="tint">Optional tint color.</param>
        /// <returns>True if drawn successfully, false if skipped.</returns>
        public bool TryDraw(RenderContext context, Rectangle dest, Color tint)
        {
            var white = Stylesheet.Current?.WhiteRegion;
            if (white == null)
                return false;

            Draw(context, dest, tint);
            return true;
        }
    }
}
