using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Visuals.Regions;

namespace Sachssoft.Sasogine.Surface.Visuals.Brushes
{
    public class RegionBrush : IBrush
    {
        private readonly ITextureRegion? _region;

        public RegionBrush(ITextureRegion? region)
        {
            _region = region;
        }

        public ITextureRegion? Region => _region;

        public void Draw(RenderContext context, Rectangle dest, Color color)
        {
            _region?.Draw(context, dest, color);
        }
    }
}
