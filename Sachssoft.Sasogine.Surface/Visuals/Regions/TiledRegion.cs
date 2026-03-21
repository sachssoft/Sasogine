using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;
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

        public RegionOptions? Options { get; set; }

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
        public TiledRegion(TextureRegion region, TileMode mode = TileMode.TileXY, Point? offset = null)
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
            // Scissor-Rechteck sichern
            var prevScissor = context.Scissor;

            // Zielbereich als Scissor setzen, damit nichts außerhalb gezeichnet wird
            context.Scissor = new Rectangle(
                (int)context.Transform.Matrix.Translation.X,
                (int)context.Transform.Matrix.Translation.Y,
                dest.Width,
                dest.Height
            );

            var tileWidth = _region.Bounds.Width;
            var tileHeight = _region.Bounds.Height;

            // Offset innerhalb der Tilegröße normalisieren (damit es nicht negativ wird)
            int startX = dest.X + ((_offset.X % tileWidth + tileWidth) % tileWidth) - tileWidth;
            int startY = dest.Y + ((_offset.Y % tileHeight + tileHeight) % tileHeight) - tileHeight;

            // Anzahl benötigter Tiles in X- und Y-Richtung
            int tilesX = (_mode == TileMode.TileY) ? 1 : (int)Math.Ceiling(dest.Width / (float)tileWidth) + 2;
            int tilesY = (_mode == TileMode.TileX) ? 1 : (int)Math.Ceiling(dest.Height / (float)tileHeight) + 2;

            for (int y = 0; y < tilesY; y++)
            {
                for (int x = 0; x < tilesX; x++)
                {
                    int drawX = startX + x * tileWidth;
                    int drawY = startY + y * tileHeight;

                    // Nur zeichnen, wenn Tile innerhalb oder am Rand des Zielrechtecks liegt
                    if (drawX > dest.Right || drawY > dest.Bottom || drawX + tileWidth < dest.Left || drawY + tileHeight < dest.Top)
                        continue;

                    _region.Draw(context, new Rectangle(drawX, drawY, tileWidth, tileHeight), color);
                }
            }

            // Ursprüngliches Scissor-Rechteck wiederherstellen
            context.Scissor = prevScissor;
        }

    }
}
