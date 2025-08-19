using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Surface.Visuals.Regions
{
    /// <summary>
    /// Represents a texture region that can be drawn with a base tint color.
    /// </summary>
    /// <remarks>
    /// A <see cref="ColoredRegion"/> is a simple wrapper around a <see cref="TextureRegion"/> 
    /// that allows applying a base color tint. When drawing, an additional tint can be supplied, 
    /// which will be multiplied with the base color.
    /// </remarks>
    public class ColoredRegion : ITextureRegion
    {
        private Color _baseColor;

        /// <summary>
        /// Gets the wrapped <see cref="TextureRegion"/>.
        /// </summary>
        public TextureRegion TextureRegion { get; }

        /// <summary>
        /// Gets the original size of the region in pixels.
        /// </summary>
        public Point Size => new Point(TextureRegion.Bounds.Width, TextureRegion.Bounds.Height);

        /// <summary>
        /// Gets or sets the base tint color of this region.
        /// </summary>
        public Color Color
        {
            get => _baseColor;
            set => _baseColor = value;
        }

        /// <summary>
        /// Creates a new <see cref="ColoredRegion"/> instance.
        /// </summary>
        /// <param name="textureRegion">The underlying texture region to be drawn.</param>
        /// <param name="color">The base color tint to apply when drawing.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="textureRegion"/> is null.</exception>
        public ColoredRegion(TextureRegion textureRegion, Color color)
        {
            TextureRegion = textureRegion ?? throw new ArgumentNullException(nameof(textureRegion));
            _baseColor = color;
        }

        /// <summary>
        /// Draws the region inside the specified destination rectangle with a given additional tint.
        /// </summary>
        /// <param name="context">The current render context.</param>
        /// <param name="dest">The destination rectangle where the region should be drawn.</param>
        /// <param name="tint">An additional tint color to multiply with the base <see cref="Color"/>.</param>
        public void Draw(RenderContext context, Rectangle dest, Color tint)
        {
            // If no additional tint is given, use the base color directly
            if (tint == Color.White)
            {
                TextureRegion.Draw(context, dest, _baseColor);
                return;
            }

            // Multiply base color with the given tint
            var finalColor = MultiplyColors(_baseColor, tint);
            TextureRegion.Draw(context, dest, finalColor);
        }

        /// <summary>
        /// Multiplies two colors by their normalized RGBA values.
        /// </summary>
        private static Color MultiplyColors(in Color a, in Color b)
        {
            return new Color(
                (byte)((a.R * b.R) / 255),
                (byte)((a.G * b.G) / 255),
                (byte)((a.B * b.B) / 255),
                (byte)((a.A * b.A) / 255)
            );
        }
    }
}
