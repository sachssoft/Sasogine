using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Surface.Visuals.Regions
{
    public interface ITextureRegion
    {
        RegionOptions? Options { get; set; }

        Point Size { get; }

        void Draw(RenderContext context, Rectangle dest, Color color);        

    }
}
