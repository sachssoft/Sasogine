using Microsoft.Xna.Framework;
using System;

namespace Sachssoft.Sasogine.Surface.Visuals.Regions
{
    /// <summary>
    /// Represents a texture region that is drawn repeatedly (tiled) to fill a destination area.
    /// </summary>
    /// <remarks>
    /// A tiled region allows a single texture or sub-region to be repeated in a given pattern.
    /// The tiling can occur horizontally, vertically, or in both directions.
    /// You can also apply an offset to shift the tile pattern for scrolling effects.
    /// </remarks>
    public class TiledRegion : ITextureRegion
    {
        private readonly TextureRegion _region;
        private readonly TileMode _mode;
        private readonly Point _offset;

        /// <summary>
        /// Gets the tile size (single region width/height).
        /// </summary>
        public Point TileSize => new Point(_region.Bounds.Width, _region.Bounds.Height);

        /// <summary>
        /// Gets the tiling mode (X, Y, or XY).
        /// </summary>
        public TileMode Mode => _mode;

        /// <summary>
        /// Gets the offset applied to the tile pattern.
        /// </summary>
        public Point Offset => _offset;

        /// <summary>
        /// Required by <see cref="IImage"/>. Returns the size of a single tile.
        /// </summary>
        public Point Size => TileSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="TiledRegion"/> class.
        /// </summary>
        /// <param name="region">The texture region that will be repeated.</param>
        /// <param name="mode">Specifies whether to tile horizontally, vertically, or both.</param>
        /// <param name="offset">
        /// Optional offset (in pixels) applied to the tiling pattern.
        /// For example, scrolling the pattern by a few pixels.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="region"/> is null.</exception>
        public TiledRegion(TextureRegion region, TileMode mode = TileMode.XY, Point? offset = null)
        {
            _region = region ?? throw new ArgumentNullException(nameof(region));
            _mode = mode;
            _offset = offset ?? Point.Zero;
        }

        /// <summary>
        /// Draws the tiled region inside the specified destination rectangle.
        /// The tile pattern will repeat according to <see cref="Mode"/>.
        /// </summary>
        /// <param name="context">The render context used for drawing.</param>
        /// <param name="dest">The rectangle in which the pattern is drawn.</param>
        /// <param name="color">The color tint applied to the texture.</param>
        public void Draw(RenderContext context, Rectangle dest, Color color)
        {
            var tileWidth = _region.Bounds.Width;
            var tileHeight = _region.Bounds.Height;

            // Apply offset and wrap inside tile size
            int startX = dest.X + (_offset.X % tileWidth);
            int startY = dest.Y + (_offset.Y % tileHeight);

            // Calculate how many tiles we need in each direction
            int tilesX = (_mode == TileMode.Y) ? 1 : (int)Math.Ceiling(dest.Width / (float)tileWidth) + 1;
            int tilesY = (_mode == TileMode.X) ? 1 : (int)Math.Ceiling(dest.Height / (float)tileHeight) + 1;

            for (int y = 0; y < tilesY; y++)
            {
                for (int x = 0; x < tilesX; x++)
                {
                    int drawX = startX + x * tileWidth;
                    int drawY = startY + y * tileHeight;

                    // Optional: skip tiles completely outside dest
                    if (drawX >= dest.Right || drawY >= dest.Bottom)
                        continue;

                    _region.Draw(context, new Rectangle(drawX, drawY, tileWidth, tileHeight), color);
                }
            }
        }
    }
}
