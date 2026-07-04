using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Presentation.Rendering
{
    public static class RenderingExtensions
    {

        public static SolidColorBrush ToSolidColorBrush(this Color color)
        {
            return new SolidColorBrush(color);
        }

        public static SolidColorBrush ToSolidColorBrush(this Color color, float opacity)
        {
            return new SolidColorBrush(color, opacity);
        }

    }
}
