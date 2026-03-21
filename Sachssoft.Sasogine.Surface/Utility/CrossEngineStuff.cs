using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sachssoft.Sasogine.Surface.Basic;

namespace Sachssoft.Sasogine.Surface.Utility;

internal static class CrossEngineStuff
{
    public static Point ViewSize
    {
        get
        {
            var device = UIEnvironment.GraphicsDevice;
            return new Point(device.Viewport.Width, device.Viewport.Height);
        }
    }

    public static Color MultiplyColor(Color color, float value)
    {
        return color * value;
    }

    public static Texture2D CreateTexture(GraphicsDevice device, int width, int height)
    {
        var texture2d = new Texture2D(device, width, height);
        return texture2d;
    }

    public static void SetTextureData(Texture2D texture, Rectangle bounds, byte[] data)
    {
        texture.SetData(0, bounds, data, 0, bounds.Width * bounds.Height * 4);
    }
}
