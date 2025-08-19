using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Visuals.Regions;
using System;

namespace Sachssoft.Sasogine.Surface.Visuals.Brushes
{
    public class TextureBrush : IBrush
    {
        public TextureRegion TextureRegion { get; }

        public StretchMode Stretch { get; set; } = StretchMode.Fill;
        public TileMode Tile { get; set; } = TileMode.None;

        /// <summary>
        /// Offset for tiling or scrolling, in pixels.
        /// </summary>
        public Vector2 Offset { get; set; } = Vector2.Zero;

        /// <summary>
        /// Rotation in radians around the center of the drawn region.
        /// </summary>
        public float Rotation { get; set; } = 0f;

        public TextureBrush(TextureRegion textureRegion)
        {
            TextureRegion = textureRegion ?? throw new ArgumentNullException(nameof(textureRegion));
        }

        public void Draw(RenderContext context, Rectangle dest, Color color)
        {
            if (TextureRegion?.Texture == null)
                return;

            var src = TextureRegion.Bounds;

            // Berechne das Zielrechteck basierend auf StretchMode
            Rectangle drawRect = CalculateDrawRectangle(dest, src);

            // Ursprung für Rotation (Mitte des drawRect)
            Vector2 origin = new Vector2(drawRect.Width / 2f, drawRect.Height / 2f);

            if (Tile != TileMode.None)
            {
                // Tilegröße nach StretchMode anpassen
                int tileWidth = drawRect.Width;
                int tileHeight = drawRect.Height;

                if (tileWidth == 0 || tileHeight == 0)
                    return;

                // Anzahl der Kacheln in X und Y
                int countX = Tile.HasFlag(TileMode.TileX) ? (int)Math.Ceiling(dest.Width / (float)tileWidth) + 1 : 1;
                int countY = Tile.HasFlag(TileMode.TileY) ? (int)Math.Ceiling(dest.Height / (float)tileHeight) + 1 : 1;

                // Offset innerhalb einer Kachel (scrolling)
                float offsetX = Offset.X % tileWidth;
                float offsetY = Offset.Y % tileHeight;

                for (int y = 0; y < countY; y++)
                {
                    for (int x = 0; x < countX; x++)
                    {
                        var tilePosX = dest.X + x * tileWidth - offsetX;
                        var tilePosY = dest.Y + y * tileHeight - offsetY;

                        var tileDest = new Rectangle(
                            (int)tilePosX,
                            (int)tilePosY,
                            tileWidth,
                            tileHeight);

                        // Zeichne jede Kachel mit Rotation (um Mittelpunkt)
                        context.Draw(
                            TextureRegion.Texture,
                            tileDest,
                            src,
                            color,
                            Rotation,
                            origin);
                    }
                }
            }
            else
            {
                // Kein Kacheln: Einfach einmal mit Rotation zeichnen
                context.Draw(
                    TextureRegion.Texture,
                    drawRect,
                    src,
                    color,
                    Rotation,
                    origin);
            }
        }

        private Rectangle CalculateDrawRectangle(Rectangle dest, Rectangle src)
        {
            switch (Stretch)
            {
                case StretchMode.None:
                    return new Rectangle(dest.X, dest.Y, src.Width, src.Height);

                case StretchMode.Fill:
                    return dest;

                case StretchMode.Uniform:
                    {
                        float scale = Math.Min(dest.Width / (float)src.Width, dest.Height / (float)src.Height);
                        int width = (int)(src.Width * scale);
                        int height = (int)(src.Height * scale);
                        int x = dest.X + (dest.Width - width) / 2;
                        int y = dest.Y + (dest.Height - height) / 2;
                        return new Rectangle(x, y, width, height);
                    }

                case StretchMode.UniformToFill:
                    {
                        float scale = Math.Max(dest.Width / (float)src.Width, dest.Height / (float)src.Height);
                        int width = (int)(src.Width * scale);
                        int height = (int)(src.Height * scale);
                        int x = dest.X + (dest.Width - width) / 2;
                        int y = dest.Y + (dest.Height - height) / 2;
                        return new Rectangle(x, y, width, height);
                    }

                default:
                    return dest;
            }
        }
    }
}
