using Microsoft.Xna.Framework;

namespace Sachssoft.Sasogine.Surface.Utility;

internal static class GraphicsExtension
{
    public static Point Size(this Rectangle r)
    {
        return new Point(r.Width, r.Height);
    }

}