using Microsoft.Xna.Framework;
using Sachssoft.Sasogine.Surface.Visuals.Brushes;

namespace Sachssoft.Sasogine.Surface.Visuals
{
    public static class BrushExtensions
    {
        public static SolidBrush ToBrush(this Color color) => new SolidBrush(color); 
        
        public static void Draw(this IBrush brush, RenderContext context, Rectangle dest)
        {
            brush.Draw(context, dest, Color.White);
        }
    }
}
