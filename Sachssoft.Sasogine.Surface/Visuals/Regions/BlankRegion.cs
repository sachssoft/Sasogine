using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Graphics;

namespace Sachssoft.Sasogine.Surface.Visuals.Regions
{
    public class BlankRegion : ITextureRegion
    {
        private Texture2D? _blankTexture;
        private Color _color = Color.White;

        public BlankRegion()
        {
        }

        public BlankRegion(Color color)
        {
            Options = new RegionOptions()
            {
                Color = color
            };
        }

        public RegionOptions? Options { get; set; }

        public Point Size => new Point(1, 1);

        public Texture2D Texture => _blankTexture!;

        public void Draw(RenderContext context, Rectangle dest, Color color)
        {
            // Bestimme die Farbe: Options.Color hat Vorrang, sonst übergebene Farbe
            var drawColor = Options?.Color ?? color;

            // Textur nur einmal erzeugen oder wenn sich die Farbe geändert hat
            if (_blankTexture == null || drawColor != _color)
            {
                _blankTexture?.Dispose();
                _color = drawColor;
                _blankTexture = context.GraphicsDevice.CreateEmptyTexture(_color);
            }

            // Textur auf das Zielrechteck zeichnen
            context.Draw(_blankTexture, dest, null, color);
        }
    }
}
