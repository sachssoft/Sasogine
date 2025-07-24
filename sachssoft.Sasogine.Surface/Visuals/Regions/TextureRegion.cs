using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace sachssoft.Sasogine.Surface.Visuals.Regions
{
    /// <summary>
    /// Represents a rectangular sub-region of a <see cref="Texture2D"/>.
    /// Used for rendering parts of a larger texture (e.g., from an atlas).
    /// </summary>
    public class TextureRegion : IImage
    {
        private readonly Texture2D _texture;
        private readonly Rectangle _bounds;

        /// <summary>
        /// Gets the underlying texture.
        /// </summary>
        public Texture2D Texture => _texture;

        /// <summary>
        /// Gets the bounds of this region within the <see cref="Texture"/>.
        /// </summary>
        public Rectangle Bounds => _bounds;

        /// <summary>
        /// Gets the width and height of this region.
        /// </summary>
        public Point Size => new(Bounds.Width, Bounds.Height);

#if MONOGAME || FNA || STRIDE
        /// <summary>
        /// Creates a region covering the entire <paramref name="texture"/>.
        /// </summary>
        /// <param name="texture">The full texture.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="texture"/> is null.</exception>
        public TextureRegion(Texture2D texture)
            : this(texture, new Rectangle(0, 0, texture?.Width ?? 0, texture?.Height ?? 0))
        {
        }
#endif

        /// <summary>
        /// Creates a region from a given texture and bounds.
        /// </summary>
        /// <param name="texture">The texture to reference.</param>
        /// <param name="bounds">The sub-rectangle of the texture.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="texture"/> is null.</exception>
        public TextureRegion(Texture2D texture, Rectangle bounds)
        {
            _texture = texture ?? throw new ArgumentNullException(nameof(texture));
            _bounds = bounds;
        }

        /// <summary>
        /// Creates a region relative to another <see cref="TextureRegion"/>.
        /// </summary>
        /// <param name="region">The base region to offset from.</param>
        /// <param name="bounds">The sub-rectangle relative to the base region.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="region"/> is null.</exception>
        public TextureRegion(TextureRegion region, Rectangle bounds)
        {
            if (region == null)
                throw new ArgumentNullException(nameof(region));

            _texture = region.Texture;

            // Offset the sub-bounds relative to the parent region
            bounds.Offset(region.Bounds.Location);
            _bounds = bounds;
        }

        /// <summary>
        /// Draws this texture region into the specified destination rectangle.
        /// </summary>
        /// <param name="context">The current <see cref="RenderContext"/>.</param>
        /// <param name="destination">The destination rectangle on screen.</param>
        /// <param name="color">
        /// A tint color to apply. Use <see cref="Color.White"/> for no tint.
        /// </param>
        public virtual void Draw(RenderContext context, Rectangle destination, Color color)
        {
            context.Draw(Texture, destination, Bounds, color);
        }

        /// <summary>
        /// Tries to draw this region safely. If the texture is null or already disposed,
        /// it will skip rendering instead of throwing an exception.
        /// </summary>
        /// <param name="context">The render context.</param>
        /// <param name="destination">Destination rectangle on screen.</param>
        /// <param name="color">Tint color.</param>
        /// <returns>True if drawn successfully, false if skipped.</returns>
        public bool TryDraw(RenderContext context, Rectangle destination, Color color)
        {
            // Prevent crash if texture is missing or disposed
            if (_texture == null || _texture.IsDisposed)
                return false;

            context.Draw(_texture, destination, _bounds, color);
            return true;
        }
    }
}
