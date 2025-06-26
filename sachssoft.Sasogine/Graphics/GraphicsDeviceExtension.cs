using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace sachssoft.Sasogine.Graphics;

public static class GraphicsDeviceExtension
{
    public static Texture2D CreateEmptyTexture(this GraphicsDevice graphics_device, Color color)
    {
        var tex = new Texture2D(graphics_device, 1, 1);
        tex.SetData<Color>([color]);
        return tex;
    }

    public static Texture2D CreateScreenhot(this GraphicsDevice graphics_device)
    {
        var width = graphics_device.PresentationParameters.BackBufferWidth;
        var height = graphics_device.PresentationParameters.BackBufferHeight;

        var back_buffer = new int[width * height];
        graphics_device.GetBackBufferData(back_buffer);

        var texture = new Texture2D(graphics_device, width, height, false, graphics_device.PresentationParameters.BackBufferFormat);
        texture.SetData(back_buffer);

        return texture;
    }
}
